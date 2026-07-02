using System.IO;
using StartingWeapons.Core.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace StartingWeapons.Content.Items;

public class WandOfPlantingProjectile : ModProjectile
{
    /// <summary>
    ///     The maximum number of projectiles that can be stuck to an <see cref="NPC" /> instance at once.
    /// </summary>
    public const int MAX_STICK_COUNT = 6;

    /// <summary>
    ///     The state of the projectile when it is stuck to an <see cref="NPC" /> instance.
    /// </summary>
    public const float STATE_STICK = 1f;

    /// <summary>
    ///     The state of the projectile when it is being channeled by the <see cref="Player" /> that owns
    ///     it.
    /// </summary>
    public const float STATE_CHANNEL = 2f;

    /// <summary>
    ///     The state of the projectile when it is exploding.
    /// </summary>
    public const float STATE_EXPLOSION = 3f;

    private static readonly VertexStrip Strip = new();

    /// <summary>
    ///     The buffer that stores the positions of the projectiles that are stuck to an <see cref="NPC" />
    ///     instance.
    /// </summary>
    public readonly Point[] Buffer = new Point[MAX_STICK_COUNT];

    public bool WasRightClickPressed { get; private set; }

    public Vector2 Offset { get; private set; }

    /// <summary>
    ///     Gets or sets the state of the projectile. Shorthand for <c>Projectile.ai[0]</c>.
    /// </summary>
    public ref float State => ref Projectile.ai[0];
    
    /// <summary>
    ///     Gets or sets the index of the <see cref="NPC" /> instance that this projectile is currently stuck to.
    /// </summary>
    public ref float Index => ref Projectile.ai[1];

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        ProjectileID.Sets.TrailingMode[Type] = 3;
        ProjectileID.Sets.TrailCacheLength[Type] = 30;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 60;

        Projectile.friendly = true;

        Projectile.width = 16;
        Projectile.height = 16;

        Projectile.timeLeft = 1200;

        Projectile.penetrate = -1;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

        SoundEngine.PlaySound(in SoundID.Grass, Projectile.Center);

        return base.OnTileCollide(oldVelocity);
    }

    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);

        SpawnHitEffects(Projectile);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        Projectile.KillOldestJavelin(Projectile.whoAmI, Type, target.whoAmI, Buffer);

        SpawnHitEffects(target);

        var sticking = State == STATE_STICK;
        var exploding = State == STATE_EXPLOSION;

        if (sticking || exploding)
        {
            return;
        }

        State = STATE_STICK;

        Index = target.whoAmI;

        Offset = (target.Center - Projectile.Center) * 0.75f;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        base.OnHitPlayer(target, info);

        SpawnHitEffects(target);
    }

    private static void SpawnHitEffects(Entity entity)
    {
        for (var i = 0; i < 5; i++)
        {
            var dust = Dust.NewDustDirect(entity.position, entity.width, entity.height, DustID.Grass);

            dust.scale /= 2f;

            dust.noGravity = true;
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);

        writer.Write(WasRightClickPressed);

        writer.WriteVector2(Offset);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);

        WasRightClickPressed = reader.ReadBoolean();

        Offset = reader.ReadVector2();
    }

    public override void AI()
    {
        base.AI();

        switch (State)
        {
            case STATE_STICK:
                UpdateStick();
                break;
            case STATE_CHANNEL:
                UpdateChannel();
                break;
            case STATE_EXPLOSION:
                UpdateExplosion();
                break;
            default:
                UpdateHoming();
                break;
        }

        UpdateVisuals();

        var channeling = State == STATE_CHANNEL;

        Projectile.tileCollide = !channeling;
    }

    private void UpdateHoming()
    {
        const float range = 16f * 16f;
        
        var index = Projectile.FindTargetWithLineOfSight(range);

        if (index == -1)
        {
            return;
        }

        var target = Main.npc[index];

        if (!target.CanBeChasedBy())
        {
            return;
        }

        var direction = Projectile.DirectionTo(target.Center);
        var velocity = direction * 8f;

        Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, velocity, 0.1f);
    }

    private void UpdateStick()
    {
        var npc = Main.npc[(int)Index];

        if (!npc.active || npc.dontTakeDamage)
        {
            Projectile.Kill();
        }
        else
        {
            Projectile.Center = npc.Center - Offset * 2f;
            Projectile.gfxOffY = npc.gfxOffY;
        }

        if (!Main.mouseRight)
        {
            return;
        }

        Projectile.NewProjectile
        (
            Projectile.GetSource_FromAI("Explosion"),
            Projectile.Center,
            Vector2.Zero,
            ModContent.ProjectileType<WandOfPlantingExplosionProjectile>(),
            Math.Max(Projectile.damage / 6, 5),
            4f,
            Projectile.owner
        );
        
        State = STATE_CHANNEL;

        Projectile.netUpdate = true;
    }

    private void UpdateChannel()
    {
        if (Main.mouseRight)
        {
            var player = Main.player[Projectile.owner];

            player.itemTime = 2;
            player.itemAnimation = 2;

            var offset = new Vector2(16f, 0f).RotatedBy(Main.GameUpdateCount * 0.01f + Projectile.whoAmI);
            var position = player.Center + offset + player.DirectionTo(Main.MouseWorld) * 32f;

            var direction = Projectile.DirectionTo(position);
            var distance = Projectile.Distance(position);

            var speed = MathHelper.Clamp(distance * distance / (64f * 64f), 0.1f, 1f);
            var velocity = direction * (8f * speed);

            var factor = MathHelper.Clamp(distance / 32f, 0f, 1f);

            Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, velocity, 0.25f * factor);
        }
        else if (WasRightClickPressed)
        {
            Projectile.velocity = new Vector2(Main.rand.NextFloatDirection() * 4f, -Main.rand.NextFloat(4f, 6f));

            State = STATE_EXPLOSION;

            Projectile.netUpdate = true;
        }

        WasRightClickPressed = Main.mouseRight;
    }

    private void UpdateExplosion()
    {
        Projectile.velocity.X *= 1.01f;
        Projectile.velocity.Y += 0.35f;
    }

    private void UpdateVisuals()
    {
        if (Projectile.timeLeft < 255 / 15)
        {
            Projectile.alpha += 15;
        }
        
        var sticking = State == STATE_STICK;

        if (!sticking)
        {
            Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
        }

        Projectile.spriteDirection = Projectile.direction;

        var exploding = State == STATE_EXPLOSION;

        if (!exploding)
        {
            return;
        }

        const int rate = 10;

        if (!Main.rand.NextBool(rate))
        {
            return;
        }

        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass, Projectile.velocity.X / 2f, Projectile.velocity.Y / 2f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        PixellationSystem.Queue(DrawTrail);

        DrawProjectile(in lightColor);

        return false;
    }

    private void DrawTrail()
    {
        var data = GameShaders.Misc["MagicMissile"];

        data.UseSaturation(-2.8f);
        data.UseOpacity(2f);

        data.Apply();

        var offset = -Main.screenPosition + Projectile.Size / 2f;

        Strip.PrepareStripWithProceduralPadding
        (
            Projectile.oldPos,
            Projectile.oldRot,
            StripColor,
            StripWidth,
            offset
        );

        Strip.DrawTrail();

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
    }

    private Color StripColor(float progress)
    {
        var opacity = 1f - progress;

        var result = Projectile.GetAlpha(new Color(31, 127, 38, 150) * opacity);

        return result;
    }

    private float StripWidth(float progress)
    {
        var value = Utils.GetLerpValue(0f, 0.2f, progress, true);
        var multiplier = 1f - (1f - value) * (1f - value);

        var result = MathHelper.Lerp(0f, 32f, multiplier);
        
        return State == STATE_STICK ? 0f : result;
    }

    private void DrawProjectile(in Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var color = Projectile.GetAlpha(lightColor);
        var origin = texture.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);

        var position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);

        Main.EntitySpriteDraw(texture, position, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
    }
}
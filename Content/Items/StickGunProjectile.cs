using System.Collections.Generic;
using ReLogic.Content;
using StartingWeapons.Core.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace StartingWeapons.Content.Items;

public class StickGunProjectile : ModProjectile
{
    /// <summary>
    ///     The number of extra updates the projectile receives, per tick.
    /// </summary>
    public const int UPDATES = 8;

    /// <summary>
    ///     The lifespan of the projectile, in ticks.
    /// </summary>
    public const int LIFESPAN = 120 * UPDATES;

    public const float RICOCHET_RANGE = 32f * 16f;

    /// <summary>
    ///     Gets the projectile type of <see cref="StickGunRicochetProjectile" />. This value is set in the
    ///     <see cref="SetStaticDefaults" /> method.
    /// </summary>
    public static int RicochetType { get; private set; }

    private static readonly VertexStrip Strip = new();
    
    public static readonly SoundStyle RicochetHitSound = new($"{nameof(StartingWeapons)}/Assets/Sounds/StickGunRicochetHit");

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        RicochetType = ModContent.ProjectileType<StickGunRicochetProjectile>();

        ProjectileID.Sets.TrailingMode[Type] = 3;
        ProjectileID.Sets.TrailCacheLength[Type] = 150;

        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.friendly = true;
        Projectile.hide = true;

        Projectile.width = 16;
        Projectile.height = 16;

        Projectile.timeLeft = LIFESPAN;
        Projectile.MaxUpdates = UPDATES;
    }

    public override bool? CanDamage()
    {
        return Projectile.numHits == 0 ? null : false;
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

        SoundEngine.PlaySound(in RicochetHitSound, Projectile.Center);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        SpawnHitEffects(target);
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

    public override void AI()
    {
        base.AI();

        UpdateRicochet();

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
    }

    private void UpdateRicochet()
    {
        foreach (var projectile in Main.ActiveProjectiles)
        {
            var isRicochet = projectile.type == RicochetType && Projectile.whoAmI != projectile.whoAmI;
            var isColliding = projectile.Hitbox.Intersects(Projectile.Hitbox);

            if (!isRicochet || !isColliding)
            {
                continue;
            }

            projectile.Kill();

            var hasRicochetTarget = TryRicochetProjectile();

            if (hasRicochetTarget)
            {
                continue;
            }

            var hasNPCTarget = TryRicochetNPC();

            if (hasNPCTarget)
            {
                continue;
            }

            var direction = Vector2.Normalize(Projectile.Center - projectile.Center);
            var velocity = Vector2.Reflect(Projectile.velocity, direction);

            Projectile.velocity = velocity;
        }
    }

    private bool TryRicochetProjectile()
    {
        foreach (var target in Main.ActiveProjectiles)
        {
            if (target.type != RicochetType)
            {
                continue;
            }

            var withinRange = target.DistanceSQ(Projectile.Center) < RICOCHET_RANGE * RICOCHET_RANGE;

            if (!withinRange)
            {
                continue;
            }

            Projectile.damage *= 2;

            var direction = Projectile.DirectionTo(target.Center);
            var velocity = direction * Projectile.velocity.Length();

            Projectile.velocity = velocity;

            Projectile.netUpdate = true;

            return true;
        }

        return false;
    }
    
    private bool TryRicochetNPC()
    {
        foreach (var target in Main.ActiveNPCs)
        {
            if (!target.CanBeChasedBy())
            {
                continue;
            }

            var withinRange = target.DistanceSQ(Projectile.Center) < RICOCHET_RANGE * RICOCHET_RANGE;

            if (!withinRange)
            {
                continue;
            }

            var direction = Projectile.DirectionTo(target.Center);
            var velocity = direction * Projectile.velocity.Length();

            Projectile.velocity = velocity;

            Projectile.netUpdate = true;

            return true;
        }

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);

        behindProjectiles.Add(index);
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
        
        return Projectile.GetAlpha(new Color(31, 127, 38, 150) * opacity);
    }

    private float StripWidth(float progress) 
    {
        var value = Utils.GetLerpValue(0f, 0.2f, progress, clamped: true);
        var multiplier = 1f - (1f - value) * (1f - value);
        
        return MathHelper.Lerp(0f, 32f, multiplier);
    }

    private void DrawProjectile(in Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var color = Projectile.GetAlpha(lightColor);
        var origin = texture.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);

        var position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);

        Main.EntitySpriteDraw(texture, position, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.FlipVertically);
    }
}
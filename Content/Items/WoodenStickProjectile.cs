using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;

namespace StartingWeapons.Content.Items;

public class WoodenStickProjectile : ModProjectile
{
    /// <summary>
    ///     The lifespan of the projectile, in ticks.
    /// </summary>
    public const int LIFESPAN = 45;

    public const int DIRECTION_UPWARDS = 0;
    
    public const int DIRECTION_DOWNWARDS = 1;
    
    /// <summary>
    ///     Gets or sets the direction of the swing of the projectile.
    /// </summary>
    public ref float Direction => ref Projectile.ai[1];
    
    /// <summary>
    ///     Gets the <see cref="Player" /> instance that owns the projectile. Shorthand for
    ///     <c>Main.player[Projectile.owner]</c>.
    /// </summary>
    private Player Owner => Main.player[Projectile.owner];

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.ownerHitCheck = true;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        
        Projectile.width = 64;
        Projectile.height = 64;

        Projectile.penetrate = -1;
        
        Projectile.timeLeft = LIFESPAN;

        Projectile.localNPCHitCooldown = -1;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);

        var direction = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;

        Projectile.direction = direction;
        Projectile.spriteDirection = direction;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);
        
        if (target.lifeMax > 250 || target.boss || !target.CanBeChasedBy() || !target.TryGetGlobalNPC(out WoodenStickGlobalNPC global))
        {
            return;
        }

        global.Enabled = true;

        target.velocity = new Vector2(4f * Owner.direction, -4f);
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);
        
        writer.Write((sbyte)Projectile.spriteDirection);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);

        Projectile.spriteDirection = reader.ReadSByte();
    }

    public override void AI()
    {
        base.AI();
        
        if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
        {
            Projectile.Kill();
            return;
        }
        
        UpdateSwing();
        UpdateOwner();

        Projectile.velocity = Vector2.Zero;
    }

    private void UpdateSwing()
    {
        var offset = Projectile.rotation.ToRotationVector2() * 32f * Projectile.spriteDirection;
        var position = Owner.MountedCenter + offset;

        var multiplier = 1f - Projectile.timeLeft / (float)LIFESPAN;
        var progress = multiplier * multiplier * multiplier;
        
        var start = MathHelper.ToRadians(-110f);
        var end = MathHelper.ToRadians(135f);
        var rotation = MathHelper.SmoothStep(start, end, progress);
        
        if (Projectile.spriteDirection == -1)
        {
            rotation = MathHelper.Pi - rotation;
        }

        Projectile.Center = position;
        Projectile.rotation = Projectile.rotation.AngleLerp(rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f), 0.2f);

        Projectile.scale = MathHelper.SmoothStep(1.2f, 0f, progress * progress * progress);
    }
    
    private void UpdateOwner()
    {
        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(135f) * Projectile.spriteDirection);
        Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.ToRadians(135f) * Projectile.spriteDirection);
        
        Owner.heldProj = Projectile.whoAmI;
        
        Owner.itemTime = 2;
        Owner.itemAnimation = 2;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawProjectile(in lightColor);

        return false;
    }
    
    private void DrawProjectile(in Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var color = Projectile.GetAlpha(lightColor);
        var origin = (Projectile.spriteDirection == -1 ? texture.Size() : new Vector2(0f, texture.Height)) + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);;

        var offset = Projectile.rotation.ToRotationVector2() * 32f * Projectile.spriteDirection;
        var position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY) - offset;

        var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Main.EntitySpriteDraw(texture, position, null, color, Projectile.rotation, origin, Projectile.scale, effects);
    }
}
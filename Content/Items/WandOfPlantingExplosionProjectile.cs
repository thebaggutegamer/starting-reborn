using StartingWeapons.Core.Graphics;
using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class WandOfPlantingExplosionProjectile : ModProjectile
{
    /// <summary>
    ///     The lifespan of the projectile, in frames.
    /// </summary>
    public const int LIFESPAN = 30;

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.tileCollide = false;
        Projectile.friendly = true;

        Projectile.width = 64;
        Projectile.height = 64;

        Projectile.penetrate = -1;
        Projectile.timeLeft = LIFESPAN;

        Projectile.localNPCHitCooldown = -1;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override void AI()
    {
        base.AI();

        UpdateScale();
        UpdateOpacity();
    }

    private void UpdateScale()
    {
        var progress = 1f - Projectile.timeLeft / (float)LIFESPAN;

        Projectile.scale = MathHelper.SmoothStep(0f, 0.5f, progress);
    }

    private void UpdateOpacity()
    {
        Projectile.alpha += 255 / LIFESPAN;

        if (Projectile.alpha < 255)
        {
            return;
        }

        Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        PixellationSystem.Queue(DrawProjectile);

        return false;
    }

    private void DrawProjectile()
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var color = Projectile.GetAlpha(new Color(31, 127, 38, 0));
        var origin = texture.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);

        var position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);

        Main.EntitySpriteDraw(texture, position, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
    }
}
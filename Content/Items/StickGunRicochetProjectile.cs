using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class StickGunRicochetProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.friendly = true;

        Projectile.width = 16;
        Projectile.height = 16;

        Projectile.penetrate = -1;
    }

    public override bool? CanDamage()
    {
        return false;
    }

    public override bool? CanCutTiles()
    {
        return false;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

        return base.OnTileCollide(oldVelocity);
    }

    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);

        for (var i = 0; i < 3; i++)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass, Projectile.velocity.X, Projectile.velocity.Y);

            var type = Mod.Find<ModGore>($"StickGunRicochet{i}").Type;
            var gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, type);

            gore.timeLeft = 1;
        }
    }

    public override void AI()
    {
        base.AI();

        var gravity = Player.defaultGravity / Projectile.MaxUpdates;

        Projectile.velocity.Y += gravity;

        Projectile.rotation = Projectile.velocity.X * 0.5f;
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
        var origin = texture.Size() / 2f + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);

        var position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);

        Main.EntitySpriteDraw(texture, position, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.FlipVertically);
    }
}
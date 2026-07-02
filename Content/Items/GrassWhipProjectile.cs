using System.Collections.Generic;
using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class GrassWhipProjectile : ModProjectile
{
    /// <summary>
    ///     Gets or sets the timer of the projectile, in ticks.
    /// </summary>
    private ref float Timer => ref Projectile.ai[0];

    /// <summary>
    ///     Gets the <see cref="Player" /> instance that owns the projectile. Shorthand for
    ///     <c>Main.player[Projectile.owner]</c>.
    /// </summary>
    private Player Owner => Main.player[Projectile.owner];

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.DefaultToWhip();
    }

    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);

        SpawnHitEffects(Projectile);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        SpawnHitEffects(target);

        Owner.MinionAttackTargetNPC = target.whoAmI;
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

    public override bool PreDraw(ref Color lightColor)
    {
        DrawProjectile();

        return false;
    }

    private void DrawProjectile()
    {
        var points = new List<Vector2>();

        Projectile.FillWhipControlPoints(Projectile, points);

        var effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        Main.instance.LoadProjectile(Type);

        var texture = TextureAssets.Projectile[Type].Value;

        var position = points[0];

        for (var i = 0; i < points.Count - 1; i++)
        {
            var frame = new Rectangle(0, 0, 16, 14);
            var origin = new Vector2(5, 8);

            var scale = 1f;

            switch (i)
            {
                case var _ when i == points.Count - 2:
                    frame.Y = 70;
                    frame.Height = 22;

                    Projectile.GetWhipSettings(Projectile, out var timeToFlyOut, out _, out _);

                    var modifier = Timer / timeToFlyOut;

                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, modifier, true) * Utils.GetLerpValue(0.9f, 0.7f, modifier, true));
                    break;
                case > 10:
                    frame.Y = 58;
                    frame.Height = 12;
                    break;
                case > 5:
                    frame.Y = 42;
                    frame.Height = 16;
                    break;
                case > 0:
                    frame.Y = 26;
                    frame.Height = 22;
                    break;
            }

            var element = points[i];
            var difference = points[i + 1] - element;

            var rotation = difference.ToRotation() - MathHelper.PiOver2;
            var color = Lighting.GetColor(element.ToTileCoordinates());

            Main.EntitySpriteDraw(texture, position - Main.screenPosition, frame, color, rotation, origin, scale, effects);

            position += difference;
        }
    }
}
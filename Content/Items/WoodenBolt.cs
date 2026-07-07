using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class WoodenBolt : ModProjectile
{
    private Player _owner => Main.player[Projectile.owner];
    public override void SetDefaults()
    {
        Projectile.Size = new(8, 8);
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.friendly = true;
    }
    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
    }
    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var modPlayer = _owner.GetModPlayer<WoodenCrossbowPlayer>();
        modPlayer.ShotsHit++;
        if (modPlayer.TaggedTargetIds.Count > 5)
            modPlayer.TaggedTargetIds.Dequeue();
       
       if (!modPlayer.TaggedTargetIds.Contains(target.whoAmI)) 
        modPlayer.TaggedTargetIds.Enqueue(target.whoAmI);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D sprite = TextureAssets.Projectile[Type].Value;
        Main.spriteBatch.Draw(sprite, Projectile.Center-Main.screenPosition, null, lightColor, Projectile.rotation, sprite.Size()/2, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}

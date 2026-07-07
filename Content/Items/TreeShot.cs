using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class TreeShot : ModProjectile
{
    private NPC _target => Main.npc[(int)Projectile.ai[1]];
    private int _targetTimer { get { return (int)Projectile.ai[2]; } set { Projectile.ai[2] = value; } }
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.Size = new(8, 8);
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.penetrate = 1;
    }
    public override void AI()
    {
        const int AIM_DELAY = 20;
        _targetTimer++;
        if (_target.active && _targetTimer > AIM_DELAY)
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(_target.Center)*12, 0.03f);
        }
        Projectile.rotation = Projectile.velocity.ToRotation();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D sprite = TextureAssets.Projectile[Type].Value;
        Vector2 offset = sprite.Size() / 2;
        for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
        {
            Main.spriteBatch.Draw(sprite, Projectile.oldPos[i]-Main.screenPosition+Projectile.getRect().Size()/2, null, lightColor * ((5-(i+1))/5f), Projectile.oldRot[i], offset, Projectile.scale, SpriteEffects.None, 0);
        }
        Main.spriteBatch.Draw(sprite, Projectile.Center-Main.screenPosition, null, lightColor, Projectile.rotation, offset, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = Projectile.ai[0];
        Projectile.knockBack = 0.05f;
    }
}

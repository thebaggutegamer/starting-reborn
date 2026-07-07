using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class MagicLeaf : ModProjectile
{
    public override string Texture => "StartingWeapons/Content/Items/TreeShot";
    private Player _owner => Main.player[Projectile.owner];
    private NPC _target;
    private int _attackTimer { get { return (int)Projectile.ai[0]; } set { Projectile.ai[0] = value; } }
    private int _targetOverride { get { return (int)Projectile.ai[1]; } set { Projectile.ai[1] = value; } }
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.Size = new(8, 8);
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.friendly = true;
    }
    public override void AI()
    {
        _attackTimer++;
        if (_attackTimer > 20)
        {
            const float MAX_AIM_DISTANCE = 250 * 16 * 250 * 16;
            if (((_target is null || !_target.active) && _targetOverride == -1) || (_target is not null && _target.DistanceSQ(Projectile.Center) > MAX_AIM_DISTANCE))
            {
                (float distance, int index) targetDistancePair = (float.MaxValue, -1);
                var queue = _owner.GetModPlayer<WoodenCrossbowPlayer>().TaggedTargetIds;
                for (int i = 0; i < queue.Count; i++)
                {
                    NPC potentialTarget = Main.npc[queue.ElementAt(i)];
                    if (potentialTarget is not null && potentialTarget.active)
                    {
                        float distance = potentialTarget.DistanceSQ(Projectile.Center);
                        if (targetDistancePair.distance > distance)
                        {
                            targetDistancePair.distance = distance;
                            targetDistancePair.index = queue.ElementAt(i);
                        }
                    }
                }
                if (Main.npc.IndexInRange(targetDistancePair.index))
                    _target = Main.npc[targetDistancePair.index];
            }
            if (_target is not null && _target.active)
            {
                float speed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(_target.Center) * 20, 0.1f);
            }
        }
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, Color.Green.ToVector3());
    }
    public override void OnSpawn(IEntitySource source)
    {
        if (_targetOverride != -1)
        {
            _target = Main.npc[_targetOverride];
            if (!_target.active)
                _targetOverride = -1;
        }
    }
    public override bool? CanHitNPC(NPC target)
    {
        if (_targetOverride != -1)
            return target.whoAmI == _targetOverride;
        else return true;
    }
    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D sprite = TextureAssets.Projectile[Type].Value;
        Vector2 offset = sprite.Size() / 2;
        for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
        {
            Main.spriteBatch.Draw(sprite, Projectile.oldPos[i] - Main.screenPosition + Projectile.getRect().Size() / 2, null, lightColor * ((5 - (i + 1)) / 5f), Projectile.oldRot[i], offset, Projectile.scale, SpriteEffects.None, 0);
        }
        Main.spriteBatch.Draw(sprite, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, offset, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}

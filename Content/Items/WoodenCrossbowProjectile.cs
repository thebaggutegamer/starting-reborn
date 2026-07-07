using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class WoodenCrossbowProjectile : ModProjectile
{
    public override string Texture => "StartingWeapons/Content/Items/WoodenCrossbow";
    private Player _owner => Main.player[Projectile.owner];
    private int _attackTimer { get { return (int)Projectile.ai[0]; } set { Projectile.ai[0] = value; } }
    private float _recoil { get { return Projectile.ai[1]; } set { Projectile.ai[1] = value; } }
    
    private float _barrageRotationOffset { get { return Projectile.ai[2]; } set { Projectile.ai[2] = value; } }
    private bool _barrage;
    // we need to keep an isolated field for this so the crossbow aims where the owning player is actually aiming, rather than at the current client's cursor.
    private Vector2 _cursorPosition;

    public override void SetDefaults()
    {
        Projectile.Size = new(16, 16);
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ownerHitCheck = true;
    }
    public override void AI()
    {
        if (_owner.channel || _barrage)
        {
            if (Main.myPlayer == Projectile.owner)
                _cursorPosition = Main.MouseWorld;
            Projectile.timeLeft++;
            var playerData = _owner.GetModPlayer<WoodenCrossbowPlayer>();
            _owner.ChangeDir((_cursorPosition.X > _owner.Center.X).ToDirectionInt());
            _owner.itemTime = 2;
            if (_owner.Center.DistanceSQ(_cursorPosition) < 640)
            {
                _cursorPosition += _owner.Center.DirectionTo(_cursorPosition) * (640 - _cursorPosition.DistanceSQ(_owner.Center));
            }
            Vector2 directionToCursor = Projectile.DirectionTo(_cursorPosition).SafeNormalize(Vector2.Zero);
            if (_barrage)
            {
                directionToCursor = -Vector2.UnitY.RotatedBy(_barrageRotationOffset);
            }
            Projectile.Center = _owner.Center + directionToCursor * 10;
            Projectile.Center += directionToCursor * -8 * _recoil;
            Projectile.rotation = directionToCursor.ToRotation();

            _owner.heldProj = Projectile.whoAmI;
            _attackTimer++;
            if (playerData.ShotsHit >= 3 && _owner.altFunctionUse == 2)
            {
                _barrage = true;
            }
            Vector2 projectileSpawnPosition = Projectile.Center + directionToCursor * 20;
            if (!_barrage && _attackTimer % 60 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                const float BOLT_SHOT_SPEED = 16;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), projectileSpawnPosition, directionToCursor * BOLT_SHOT_SPEED, ModContent.ProjectileType<WoodenBolt>(), Projectile.damage, 0.2f, Owner: _owner.whoAmI);
                SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot with { Pitch = 0.2f }, Projectile.Center);
                _recoil = 1f;
            }
            else if (_barrage && _attackTimer % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient && playerData.ShotsHit >= 3)
            {
                const float LEAF_SHOT_SPEED = 24;
                int targetOverride = playerData.TaggedTargetIds.ElementAt(Main.rand.Next(0, playerData.TaggedTargetIds.Count));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), projectileSpawnPosition, directionToCursor * LEAF_SHOT_SPEED, ModContent.ProjectileType<MagicLeaf>(), Projectile.damage, 0, Owner: _owner.whoAmI, ai1:targetOverride);
                playerData.ShotsHit -= 3;
                SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot with { Pitch = 0.2f }, Projectile.Center);
                _barrageRotationOffset = Main.rand.NextFloat(-MathHelper.PiOver4 / 2, MathHelper.PiOver4 / 2);
                _recoil = 1;
                if (playerData.ShotsHit < 3)
                {
                    Projectile.timeLeft += 20;
                    _barrage = false;
                }
            }
            _recoil = MathHelper.Lerp(_recoil, 0, _barrage ? 0.3f : 0.02f);
            _owner.SetCompositeArmFront(true, _recoil > 0.5f ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
        }
        else
            Projectile.Kill();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D sprite = TextureAssets.Projectile[Type].Value;
        SpriteEffects drawEffects = _owner.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None; 
        Main.spriteBatch.Draw(sprite, Projectile.Center-Main.screenPosition, null, lightColor, Projectile.rotation, sprite.Size() / 2, Projectile.scale, drawEffects, 0);
        return false;
    }
    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_ItemUse  itemSource)
        {
            _barrage = itemSource.Player.altFunctionUse == 2;
        }
    }
    public override bool? CanDamage()
    {
        return false;
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _barrage = reader.ReadBoolean();
        _cursorPosition = reader.ReadVector2();
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_barrage);
        writer.WriteVector2(_cursorPosition);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;

namespace StartingWeapons.Content.Items;

public class WoodenClubProjectile : ModProjectile
{
    // start tilted in front, charge back, swing, if 'tip' is colliding, do whatever
    // if overcharged, launch the player forward
    enum SwingState
    {
        Charging = 0,
        Swing = 1,
        OverchargedSwing = 2
    }

    private Player _owner => Main.player[Projectile.owner];

    private float _startingAngle { get { return Projectile.ai[0]; } set { Projectile.ai[0] = value; } }

    private SwingState _clubState { get { return (SwingState)Projectile.ai[1]; } set { Projectile.ai[1] = (float)value; } }

    private int _direction { get { return (int)Projectile.ai[2]; } set { Projectile.ai[2] = value; } }

    private int _chargeTimer;

    private float _currentAngle;

    private float _previousAngle;

    private float _collisionAngle;

    private int _killTimer;


    public override string Texture => "StartingWeapons/Content/Items/WoodenClub";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        // size doesn't matter here, we use a custom collision method anyway
        Projectile.Size = new(40, 40);
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 60;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.ownerHitCheck = true;
        Projectile.timeLeft = 40;
    }
    public override void AI()
    {
        if (_owner is not null)
        {
            Projectile.timeLeft++;
            if (_killTimer == 0 || _clubState != SwingState.OverchargedSwing || !Collision.SolidCollision(GetTipPosition(_currentAngle), 14, 14))
            {
                Projectile.Center = _owner.Center;
                _owner.ChangeDir(_direction);
                _owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _currentAngle);
                _owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, _currentAngle - MathHelper.PiOver4 * _direction * -1);
            }
            switch (_clubState)
            {
                case SwingState.Charging:
                    Charge();
                    break;
                case SwingState.Swing:
                case SwingState.OverchargedSwing:
                    Swing();
                    break;
            }
            _previousAngle = _currentAngle;
            Projectile.oldRot[0] = _currentAngle;
            _owner.itemTime = 2;
        }
    }
    private void Charge()
    {
        const int TIME_TO_OVERCHARGE = 100;
        if (!_owner.channel)
        {
            _clubState = _chargeTimer > TIME_TO_OVERCHARGE ? SwingState.OverchargedSwing : SwingState.Swing;
            Projectile.damage = (int)(Projectile.damage * (MathHelper.Clamp(_chargeTimer, 40, 130) / 60f));
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.2f, Volume = 1.5f }, Projectile.Center);
            return;
        }
        _chargeTimer++;
        if (_chargeTimer == TIME_TO_OVERCHARGE)
        {
            SoundEngine.PlaySound(SoundID.MaxMana, GetTipPosition(_currentAngle));
        }
        _currentAngle = MathHelper.Lerp(_currentAngle, _startingAngle + MathHelper.PiOver2 * _direction * -1, 0.045f);
    }
    private void Swing()
    {
        bool collisionWithTile = !Collision.CanHitLine(_owner.Center, 8, 8, GetTipPosition(_currentAngle), 8, 8);
        // cant collide if the club is "behind" the player
        bool canCollide = Math.Sign((GetTipPosition(_currentAngle).X - _owner.Center.X) * _direction) == 1;

        float old = _currentAngle;

        if ((!collisionWithTile && _collisionAngle == 0) || !canCollide)
            _currentAngle = MathHelper.Lerp(_currentAngle, _startingAngle + MathHelper.Pi * _direction, _clubState == SwingState.OverchargedSwing ? 0.15f : 0.1f);
        else if (collisionWithTile && canCollide)
            _collisionAngle = old;


        Vector2 tipPos = GetTipPosition(_currentAngle);

        if (collisionWithTile && _killTimer == 0 && canCollide)
        {
            var tilePos = tipPos.ToTileCoordinates();
            Vector2 delta = GetTipPosition(_previousAngle) - tipPos;
            float effectStrength = _clubState == SwingState.OverchargedSwing ? 1.5f : 1f;
            SoundEngine.PlaySound(SoundID.DeerclopsRubbleAttack with { Pitch = 0.5f * effectStrength, Volume = 0.6f }, tipPos);
            for (int x = -1; x <= 1; x++)
            {
                for (int i = 0; i < 3 * (int)_clubState; i++)
                {
                    int dust = WorldGen.KillTile_MakeTileDust(tilePos.X + x, tilePos.Y, Main.tile[tilePos.X + x, tilePos.Y]);
                    Main.dust[dust].velocity = (MathF.Atan2(delta.Y, delta.X) + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2() * Main.rand.NextFloat(2, 3.5f) * effectStrength;
                    Main.dust[dust].velocity.Y -= 4f;
                    Main.dust[dust].scale *= 1.3f;
                }
            }
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(tipPos, Vector2.One * effectStrength * 8, 0.2f * effectStrength, 5, 20));
            if (_clubState == SwingState.OverchargedSwing)
            {
                Vector2 lungeDirection = _owner.DirectionTo(tipPos);
                lungeDirection.Y = -8;
                if (_owner.velocity.Y < 0)
                    lungeDirection.Y = -5;
                lungeDirection.X *= 12;
                _owner.velocity += lungeDirection;
            }
            _killTimer++;
        }

        if (Math.Round(old, 3) == Math.Round(_currentAngle, 3))
            _killTimer++;

        if (_killTimer > 5)
            Projectile.Kill();

    }
    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;
        var drawColor = lightColor;
        SpriteEffects effects = SpriteEffects.FlipVertically;
        Vector2 origin = Vector2.Zero;

        if (_clubState != SwingState.Charging && _killTimer == 0)
        {
            for (int i = 1; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor * (0.8f * ((5 - i) * 0.25f)), Projectile.oldRot[i] + MathHelper.PiOver4, origin, 1f, effects, 0);
            }
        }
        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor, _currentAngle+MathHelper.PiOver4, origin, 1f, effects, 0);
        if (_chargeTimer > 120 && _clubState == SwingState.Charging)
        {
            for (int i = 0; i < 2; i++)
            {
                float rotation = Main.GlobalTimeWrappedHourly + i * MathHelper.PiOver2;
                texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
                float scale = MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.Pi * 2) * 0.15f + 0.15f;
                Main.spriteBatch.Draw(texture, (GetTipPosition(_currentAngle)-Main.screenPosition), null, Color.White, rotation, texture.Size()/2, scale, SpriteEffects.None, 0);
            }
        }
        return false;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_chargeTimer);
        writer.Write(_currentAngle);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _chargeTimer = reader.ReadInt32();
        _currentAngle = reader.ReadSingle();
    }
    public override void OnSpawn(IEntitySource source)
    {
        _direction = Math.Sign(_owner.DirectionTo(Main.MouseWorld).X);
        _startingAngle = MathHelper.Pi + _direction * MathHelper.PiOver4;
        _currentAngle = _startingAngle;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (target.lifeMax > 250 || target.boss || !target.CanBeChasedBy() || !target.TryGetGlobalNPC(out WoodenClubGlobalNPC global))
        {
            return;
        }

        global.Enabled = true;

        target.velocity = new Vector2(4f * _owner.direction, -4f);
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float zero = 0;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, GetTipPosition(_currentAngle, 120), 30, ref zero);
    }

    public override bool? CanDamage()
    {
        return !Collision.SolidCollision(GetTipPosition(_currentAngle), 10, 10) && _clubState != SwingState.Charging;
    }

    private Vector2 GetTipPosition(float angle, float length = 100)
    {
        return Vector2.UnitY.RotatedBy(angle) * length + Projectile.Center;
    }
}

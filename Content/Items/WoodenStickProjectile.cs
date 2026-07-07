using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;

namespace StartingWeapons.Content.Items;

public class WoodenStickProjectile : ModProjectile
{
    private enum StickBehavior
    {
        Charging,
        Throwing,
        Thrown,
        Returning
    }
    private int _chargeTimer { get { return (int)Projectile.ai[0]; } set { Projectile.ai[0] = value; } }
    private float _startingAngle { get { return Projectile.ai[1]; } set { Projectile.ai[1] = value; } }
    private float _currentAngle { get { return Projectile.ai[2]; } set { Projectile.ai[2] = value; } }
    private StickBehavior _stickState;
    private int _returnTimer;
    private int _direction;
    private bool _canTargetEnemies;
    private bool _ownerDashedAtStick;
    private Player _owner => Main.player[Projectile.owner];
    private NPC[] _targets = new NPC[3];
    private int _targetsHit;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 5;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.tileCollide = true;
        Projectile.Size = new(20, 20);
        Projectile.friendly = true;
        Projectile.penetrate = -1;
    }
    public override void AI()
    {
        Projectile.timeLeft++;
        if (_stickState == StickBehavior.Charging)
        {
            if (_owner.channel)
            {
                _currentAngle = MathHelper.Lerp(_currentAngle, _startingAngle - MathHelper.PiOver2 * _direction, 0.1f);
                _chargeTimer++;
            }
            else
            {
                _stickState = StickBehavior.Throwing;
            }
            Projectile.Center = _owner.Center;
            _owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _currentAngle - MathHelper.PiOver2);
        }
        if (_stickState == StickBehavior.Throwing)
        {
            float destinationAngle = _startingAngle + MathHelper.PiOver4 * _direction;
            _currentAngle = MathHelper.Lerp(_currentAngle, destinationAngle, 0.33f);
            if (Math.Abs(_currentAngle - destinationAngle) < 0.1f)
            {
                _stickState = StickBehavior.Thrown;
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                Projectile.velocity = (_currentAngle).ToRotationVector2() * (16 * MathHelper.Clamp(_chargeTimer / 25f, 0.8f, 1.2f));
            }
            Projectile.Center = _owner.Center;
            _owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _currentAngle - MathHelper.PiOver2);
        }
        if (_stickState == StickBehavior.Thrown)
        {
            _returnTimer++;
            if (_returnTimer > 20)
                _stickState = StickBehavior.Returning;
            _currentAngle += 0.3f;
        }
        if (_stickState == StickBehavior.Returning)
        {
            Projectile.tileCollide = false;
            if (!_canTargetEnemies)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(_owner.Center) * (16 * MathHelper.Clamp(_chargeTimer / 25f, 0.8f, 1.2f)), 0.05f);
            else if (_targetsHit < 3)
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(_targets[_targetsHit].Center) * 16, 0.03f);
            }
            _currentAngle += 0.3f;
        }
        float distance = _owner.Center.DistanceSQ(Projectile.Center);
        const float MAX_DISTANCE_TO_GRAB_STICK_FROM = 16 * 16 * 16 * 16;
        if ((_stickState == StickBehavior.Returning || _stickState == StickBehavior.Thrown))
        {
            if (Projectile.soundDelay == 0)
            {
                SoundEngine.PlaySound(SoundID.Item7 with { Volume = 5f }, Projectile.Center);
                Projectile.soundDelay = 8;
            }
            if (_owner.altFunctionUse == 2 && !_ownerDashedAtStick)
            {
                _owner.velocity += _owner.DirectionTo(Projectile.Center) * 24;
                _ownerDashedAtStick = true;
            }
        }
        if ((_stickState == StickBehavior.Returning && distance < 1280) || (_ownerDashedAtStick && distance < 1600))
        {
            Projectile.Kill();
            SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
            if (_ownerDashedAtStick)
                _owner.velocity *= 0.2f;
        }
        Projectile.rotation = _currentAngle;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        _targetsHit++;
        if (_targetsHit >= 3)
            _canTargetEnemies = false;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (_stickState == StickBehavior.Thrown)
        {
            _stickState = StickBehavior.Returning;
            _canTargetEnemies = true;
            Projectile.tileCollide = false;
            for (int i = 0; i < 3; i++)
            {
                if (TryFindClosestTarget(Projectile.Center, out int target, _targets))
                {
                    _targets[i] = Main.npc[target];
                }
            }
        }
        return false;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D sprite = TextureAssets.Projectile[Type].Value;
        SpriteEffects effects = SpriteEffects.None;
        float rotation = _currentAngle + MathHelper.PiOver4;
        Vector2 origin = sprite.Size()/2;
        Vector2 position = Projectile.Center;
        if (_stickState is StickBehavior.Charging or StickBehavior.Throwing)
        {
            position += rotation.ToRotationVector2() * -5;
            origin = new Vector2(0, sprite.Height);
        }
        if (_stickState != StickBehavior.Charging)
        {
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
            {
                Main.spriteBatch.Draw(sprite, Projectile.oldPos[i] - Main.screenPosition + Projectile.getRect().Size() / 2, null, lightColor * ((5 - (i + 1)) / 5f), Projectile.oldRot[i] + MathHelper.PiOver4, origin, Projectile.scale, SpriteEffects.None, 0);
            }
        }
        Main.spriteBatch.Draw(sprite, position - Main.screenPosition, null, lightColor, rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
    public override bool? CanDamage()
    {
        return _stickState == StickBehavior.Thrown || _stickState == StickBehavior.Returning;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float zero = 0f;
        Vector2 axis = (_currentAngle + MathHelper.PiOver4).ToRotationVector2();
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center-axis*20, Projectile.Center+axis*20, 30, ref zero);
    }
    public override void OnSpawn(IEntitySource source)
    {
        _direction = Math.Sign(_owner.DirectionTo(Main.MouseWorld).X);
        _startingAngle = _owner.AngleTo(Main.MouseWorld) + _direction * MathHelper.PiOver4*-1;
        _currentAngle = _startingAngle;
    }
    private bool TryFindClosestTarget(Vector2 center, out int targetIndex, NPC[] npcsToIgnore)
    {
        targetIndex = -1;
        (float distance, int index) targetDistancePair = (float.MaxValue, -1);
        foreach(var npc in Main.ActiveNPCs)
        {
            float distance = npc.DistanceSQ(center);
            if (targetDistancePair.distance > distance && !npcsToIgnore.Any(n => (n is not null && n.whoAmI == npc.whoAmI)))
            {
                targetDistancePair.distance = distance;
                targetDistancePair.index = npc.whoAmI;
            }
        }
        if (Main.npc.IndexInRange(targetDistancePair.index))
        {
            targetIndex = targetDistancePair.index;
            return true;
        }
        else return false;
    }
}   
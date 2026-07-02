using ReLogic.Content;
using StartingWeapons.Common.Physics;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class GrassHookProjectile : ModProjectile
{
    public const float CHAIN_SEGMENTS_LENGTH = 6f;

    public const int CHAIN_SEGMENTS = 12;

    public const float GRAPPLE_RANGE = 20f * 16f;

    public const int GRAPPLE_AMOUNT = 1;

    public const float GRAPPLE_SPEED = 10f;

    public VerletChain PhysicsChain { get; private set; }

    public VerletChain VisualsChain { get; private set; }

    public static Asset<Texture2D> StartTexture { get; private set; }

    public static Asset<Texture2D> EndTexture { get; private set; }

    public ref float Movement => ref Projectile.ai[1];

    /// <summary>
    ///     Gets the <see cref="Player" /> instance that owns the projectile. Shorthand for
    ///     <c>Main.player[Projectile.owner]</c>.
    /// </summary>
    private Player Owner => Main.player[Projectile.owner];

    public override void Load()
    {
        base.Load();

        if (Main.dedServ)
        {
            return;
        }

        StartTexture = ModContent.Request<Texture2D>(Texture + "_Start");
        EndTexture = ModContent.Request<Texture2D>(Texture + "_End");
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        ProjectileID.Sets.SingleGrappleHook[Type] = true;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);

        Projectile.friendly = false;
        Projectile.hostile = false;

        Projectile.penetrate = -1;
    }

    public override bool? CanUseGrapple(Player player)
    {
        var count = 0;

        foreach (var projectile in Main.ActiveProjectiles)
        {
            if (projectile.owner == Main.myPlayer && projectile.type == Projectile.type)
            {
                count++;
            }
        }

        return count == 0;
    }

    public override float GrappleRange()
    {
        return GRAPPLE_RANGE;
    }

    public override void NumGrappleHooks(Player player, ref int numHooks)
    {
        base.NumGrappleHooks(player, ref numHooks);

        numHooks = GRAPPLE_AMOUNT;
    }

    public override void GrappleRetreatSpeed(Player player, ref float speed)
    {
        base.GrappleRetreatSpeed(player, ref speed);

        speed = GRAPPLE_SPEED;
    }

    public override void GrapplePullSpeed(Player player, ref float speed)
    {
        base.GrapplePullSpeed(player, ref speed);

        speed = GRAPPLE_SPEED;
    }

    public override void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY)
    {
        base.GrappleTargetPoint(player, ref grappleX, ref grappleY);

        var last = PhysicsChain.Points[^1];

        var target = last.Position;

        grappleX = target.X;
        grappleY = target.Y;
    }

    public override bool? GrappleCanLatchOnTo(Player player, int x, int y)
    {
        var tile = Framing.GetTileSafely(x, y);

        return TileID.Sets.IsATreeTrunk[tile.TileType] || tile.TileType == TileID.Platforms;
    }

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);

        PhysicsChain = new VerletChain();

        InitializeChain(PhysicsChain);

        VisualsChain = new VerletChain();

        InitializeChain(VisualsChain);
    }

    private void InitializeChain(VerletChain chain)
    {
        var origin = Projectile.Center;

        for (var i = 0; i < CHAIN_SEGMENTS; i++)
        {
            var pinned = i == 0;
            var point = new VerletPoint(origin, pinned);

            chain.AddPoint(in point);
        }

        for (var i = 0; i < CHAIN_SEGMENTS - 1; i++)
        {
            var stick = new VerletStick(chain.Points[i], chain.Points[i + 1], CHAIN_SEGMENTS_LENGTH);

            chain.AddStick(in stick);
        }
    }

    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);

        SpawnDeathEffects();
    }

    private void SpawnDeathEffects()
    {
        for (var i = 0; i < VisualsChain.Points.Count; i++)
        {
            var point = VisualsChain.Points[i];

            Dust.NewDust(point.Position, 0, 0, DustID.Grass, point.Velocity.X / 2f, point.Velocity.Y / 2f);

            var type = i switch
            {
                0 => Mod.Find<ModGore>("GrassHook1").Type,
                CHAIN_SEGMENTS - 1 => Mod.Find<ModGore>("GrassHook2").Type,
                _ => Mod.Find<ModGore>("GrassHook0").Type
            };

            var gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), point.Position, point.Velocity, type);

            gore.timeLeft = 1;
        }
    }

    public override void AI()
    {
        base.AI();

        UpdatePhysics();
        UpdateVisuals();
    }

    private void UpdatePhysics()
    {
        PhysicsChain.Update();

        var first = PhysicsChain.Points[0];
        var last = PhysicsChain.Points[^1];

        first.Position = Projectile.Center;

        var direction = Owner.controlRight.ToInt() + -Owner.controlLeft.ToInt();

        const float multiplier = 0.1f;
        const float maxVelocity = 4f;

        Movement += direction * multiplier;
        Movement = MathHelper.Clamp(Movement, -maxVelocity, maxVelocity);
        Movement *= 0.9f;

        last.Position.X += Movement;
    }

    private void UpdateVisuals()
    {
        VisualsChain.Update();

        var first = VisualsChain.Points[0];
        var last = VisualsChain.Points[^1];

        first.Position = Projectile.Center;
        last.Position = Owner.Center;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }

    public override bool PreDrawExtras()
    {
        VisualsChain.Render
        (
            new TextureVerletRenderer
            (
                (i, _) => i switch
                {
                    0 => StartTexture,
                    CHAIN_SEGMENTS - 1 => EndTexture,
                    _ => TextureAssets.Projectile[Type]
                }
            )
        );

        return false;
    }
}
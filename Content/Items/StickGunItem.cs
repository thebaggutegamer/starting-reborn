using StartingWeapons.Utilities;
using System.Collections.Generic;
using Terraria.Audio;

namespace StartingWeapons.Content.Items;

public class StickGunItem : ModItem
{
    public static readonly SoundStyle DefaultUseSound = new($"{nameof(StartingWeapons)}/Assets/Sounds/StickGunShot");

    public static readonly SoundStyle AlternativeUseSound = new($"{nameof(StartingWeapons)}/Assets/Sounds/StickGunRicochet");

    /// <summary>
    ///     Gets or sets the bullet projectile type.
    /// </summary>
    public static int BulletType { get; private set; }

    /// <summary>
    ///     Gets or sets the ricochet projectile type.
    /// </summary>
    public static int RicochetType { get; private set; }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        BulletType = ModContent.ProjectileType<StickGunProjectile>();
        RicochetType = ModContent.ProjectileType<StickGunRicochetProjectile>();
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DamageType = DamageClass.Ranged;

        Item.autoReuse = true;
        Item.noMelee = true;

        Item.damage = 5;

        Item.width = 50;
        Item.height = 26;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = DefaultUseSound;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.shootSpeed = 6f;
        Item.shoot = ModContent.ProjectileType<StickGunProjectile>();
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        var speed = player.HasAltFunctionUse() ? 12f : 6f;

        Item.shootSpeed = speed;

        var type = player.HasAltFunctionUse() ? RicochetType : BulletType;

        Item.shoot = type;

        var sound = player.HasAltFunctionUse() ? AlternativeUseSound : DefaultUseSound;

        Item.UseSound = sound;

        return base.CanUseItem(player);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);

        position.Y -= 10f;
    }

    public override float UseSpeedMultiplier(Player player)
    {
        return player.HasAltFunctionUse() ? 3f : 1f;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        base.UseStyle(player, heldItemFrame);

        var direction = Math.Sign(Main.MouseWorld.X - player.Center.X);

        player.ChangeDir(direction);

        var rotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

        var position = player.MountedCenter + rotation.ToRotationVector2() * 7f;
        var size = new Vector2(49f, 27f);
        var origin = new Vector2(-15f, 10f);

        origin.X *= player.direction;
        origin.Y *= player.gravDir;

        player.itemRotation = rotation;

        if (player.direction < 0)
        {
            player.itemRotation += MathHelper.Pi;
        }

        var consistentCenterAnchor = player.itemRotation.ToRotationVector2() * (size.X / -2f - 10f) * player.direction;
        var consistentAnchor = consistentCenterAnchor - origin.RotatedBy(player.itemRotation);

        var offset = size * -0.5f;

        var location = position + offset + consistentAnchor;

        var frame = player.bodyFrame.Y / player.bodyFrame.Height;

        if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
        {
            location -= Vector2.UnitY * 2f;
        }

        player.itemLocation = location + new Vector2(size.X * 0.5f, 0);
    }

    public override void UseItemFrame(Player player)
    {
        base.UseItemFrame(player);

        var direction = Math.Sign(Main.MouseWorld.X - player.Center.X);

        player.ChangeDir(direction);

        var progress = 1 - player.itemTime / (float)player.itemTimeMax;
        var rotation = (player.Center - Main.MouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;

        if (progress < 0.4f)
        {
            rotation += -0.45f * MathF.Pow((0.4f - progress) / 0.4f, 2f) * player.direction;
        }

        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();

        CreateRecipe()
            .AddIngredient(ItemID.Wood, 10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
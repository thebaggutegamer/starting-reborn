using StartingWeapons.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace StartingWeapons.Content.Items;

public class WandOfPlantingItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DamageType = DamageClass.Magic;
        Item.mana = 3;
        Item.damage = 6;
        Item.knockBack = 2f;

        Item.autoReuse = true;
        Item.noMelee = true;

        Item.width = 30;
        Item.height = 32;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Grass;
        Item.useTime = 24;
        Item.useAnimation = 24;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.value = 300;
        Item.shootSpeed = 10f;
        Item.shoot = ModContent.ProjectileType<WandOfPlantingProjectile>();
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        Item.UseSound = player.HasAltFunctionUse() ? null : SoundID.Grass;

        return base.CanUseItem(player);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return !player.HasAltFunctionUse();
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
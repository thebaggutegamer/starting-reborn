using StartingWeapons.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace StartingWeapons.Content.Items;

public class WoodenStickItem : ModItem
{
    public int Direction { get; set; } = WoodenStickProjectile.DIRECTION_DOWNWARDS;
    
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Melee;
        Item.damage = 10;
        Item.knockBack = 3f;

        Item.noUseGraphic = true;
        Item.autoReuse = true;
        Item.noMelee = true;

        Item.width = 46;
        Item.height = 54;

        Item.UseSound = SoundID.Item19;
        Item.useTime = 40;
        Item.useAnimation = 40;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<WoodenStickProjectile>();

        Item.value = 300;

        Item.rare = ItemRarityID.Green;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);

        projectile.ai[1] = Direction;
        
        Direction = Direction == WoodenStickProjectile.DIRECTION_UPWARDS ? WoodenStickProjectile.DIRECTION_DOWNWARDS : WoodenStickProjectile.DIRECTION_UPWARDS;
        
        return false;
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
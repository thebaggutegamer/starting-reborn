using System.Collections.Generic;

namespace StartingWeapons.Content.Items;

public class WoodenDaggerItem : ModItem
{
    public override void SetDefaults()
    {
        // Modders can use Item.DefaultToRangedWeapon to quickly set many common properties, such as: useTime, useAnimation, useStyle, autoReuse, DamageType, shoot, shootSpeed, useAmmo, and noMelee. These are all shown individually here for teaching purposes.
        if (ModContent.TryFind("CalamityMod", "GildedDagger", out ModItem GildedDagger))
        {
            Item.CloneDefaults(GildedDagger.Type);
            Item.shoot = ModContent.ProjectileType<WoodenDaggerProjectile>();
            Item.damage = 8;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = 1;
            Item.shootSpeed = 15f;
            Item.value = 300;
            Item.rare = ItemRarityID.Green;



        }
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
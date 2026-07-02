using StartingWeapons.Utilities;
using System.Collections.Generic;

namespace StartingWeapons.Content.Items;

public class GrassWhipItem : ModItem
{
    /// <summary>
    ///     Gets or sets the whip projectile type.
    /// </summary>
    public static int WhipType { get; private set; }

    /// <summary>
    ///     Gets or sets the hook projectile type.
    /// </summary>
    public static int HookType { get; private set; }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        WhipType = ModContent.ProjectileType<GrassWhipProjectile>();
        HookType = ModContent.ProjectileType<GrassHookProjectile>();
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DefaultToWhip(ModContent.ProjectileType<GrassWhipProjectile>(), 7, 2, 4f);

        Item.autoReuse = true;

        Item.UseSound = SoundID.Grass;

        Item.value = 300;
        Item.rare = ItemRarityID.Green;
    }

    public override bool MeleePrefix()
    {
        return true;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        var speed = player.HasAltFunctionUse() ? 8f : 4f;

        Item.shootSpeed = speed;

        var type = player.HasAltFunctionUse() ? HookType : WhipType;

        Item.shoot = type;

        return player.ownedProjectileCounts[Item.shoot] == 0;
    }

    
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
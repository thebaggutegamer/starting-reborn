namespace StartingWeapons.Common.Recipes;

public sealed class FinchStaffRecipe : ModSystem
{
    public override void AddRecipes()
    {
        base.AddRecipes();
        
        Recipe.Create(ItemID.BabyBirdStaff)
            .AddIngredient(ItemID.Wood, 10)
            .AddIngredient(ItemID.Bird)
            .AddTile(TileID.LivingLoom)
            .Register();
    }
}
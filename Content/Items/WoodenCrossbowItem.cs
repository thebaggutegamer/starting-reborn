using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StartingWeapons.Content.Items;

public class WoodenCrossbowItem : ModItem
{
    public override string Texture => "StartingWeapons/Content/Items/WoodenCrossbow";
    public override void SetDefaults()
    {
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.channel = true;
        Item.damage = 7;
        Item.DamageType = DamageClass.Ranged;
        Item.Size = new(64, 26);
        Item.shoot = ModContent.ProjectileType<WoodenCrossbowProjectile>();
    }
    public override bool CanShoot(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }
    public override bool AltFunctionUse(Player player)
    {
        return player.GetModPlayer<WoodenCrossbowPlayer>().ShotsHit >= 3 && player.ownedProjectileCounts[Item.shoot] == 0;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse == 2 && player.GetModPlayer<WoodenCrossbowPlayer>().ShotsHit < 3)
            return false;
        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StartingWeapons.Content.Items;

public class WoodenClub : ModItem
{
    public override string Texture => "StartingWeapons/Content/Items/WoodenClub";
    public override void SetDefaults()
    {
        Item.channel = true;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = 2;
        Item.useAnimation = 2;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.autoReuse = false;
        Item.damage = 20;
        Item.Size = new(82, 100);
        Item.shoot = ModContent.ProjectileType<WoodenClubProjectile>();
    }
    public override bool CanShoot(Player player)
    {
        return player.ownedProjectileCounts[ModContent.ProjectileType<WoodenClubProjectile>()] == 0;
    }
}

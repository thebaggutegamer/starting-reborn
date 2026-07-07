using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StartingWeapons.Content.Items;

public class TreeStaff : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.sentry = true;
        Item.Size = new(40, 40);
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.mana = 8;
        Item.DamageType = DamageClass.Summon;
        Item.useTime = Item.useAnimation = 20;
        Item.damage = 10;
        Item.autoReuse = true;
        Item.UseSound = SoundID.Item78;
        Item.shoot = ModContent.ProjectileType<TreeSentry>();
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        position = Main.MouseWorld;
        if (Collision.SolidCollision(position, 56, 10))
            return false;
        player.FindSentryRestingSpot(type, out int xPos, out int yPos, out _);
        position = new Vector2(xPos, yPos - ContentSamples.ProjectilesByType[type].height/2);
        int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        if (index != 1000)
        {
            Main.projectile[index].originalDamage = Item.damage;
        }
        player.UpdateMaxTurrets();
        return false;
    }
}

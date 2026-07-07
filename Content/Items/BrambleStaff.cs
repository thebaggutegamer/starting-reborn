using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StartingWeapons.Content.Items;

public class BrambleStaff : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.mana = 5;
        Item.damage = 12;
        Item.Size = new(48, 50);
        Item.shootSpeed = 12;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useAnimation = 90;
        Item.useTime = 30;
        Item.DamageType = DamageClass.Magic;
        Item.shoot = ModContent.ProjectileType<BrambleProjectile>();
        Item.noMelee = true;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        position = Main.MouseWorld;
        player.LimitPointToPlayerReachableArea(ref position);
        position += Main.rand.NextVector2Circular(8, 8);
        Vector2 pos = GetValidBramblePosition(player, position).ToWorldCoordinates(Main.rand.Next(17), Main.rand.Next(17));
        velocity = (position - pos).SafeNormalize(-Vector2.UnitY) * 16f;
        int brambleLength = Main.rand.Next(3, 5);
        float scaleOffset = Main.rand.NextFloat() * 0.25f + 0.75f;
        Projectile.NewProjectile(source, pos, velocity, type, damage, knockback, player.whoAmI, brambleLength, scaleOffset);
        return false;
    }

    private static Point GetValidBramblePosition(Player player, Vector2 position)
    {
        return __CallFindSharpTearsSpot(player, position);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "FindSharpTearsSpot")]
        extern static Point __CallFindSharpTearsSpot(Player player, Vector2 targetSpot);

    }
}

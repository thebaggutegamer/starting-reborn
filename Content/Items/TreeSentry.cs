using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StartingWeapons.Content.Items;

public class TreeSentry : ModProjectile
{
    private struct LeafData
    {
        public Vector2 Position;
        public float Rotation;
        public float Scale;
        public float DestinationScale;
        public int Id;
        public bool Alive;
    }
    private LeafData[] _leaves = new LeafData[20];

    private int _leafSpawnTimer { get { return (int)Projectile.ai[0]; } set { Projectile.ai[0] = value; } }

    private int _activeLeaves { get { return (int)Projectile.ai[2]; } set { Projectile.ai[2] = value; } }


    private const int LEAF_SPAWN_INTERVAL = 90;

    private const float TARGET_RANGE = 16 * 16 * 16 * 16; // 16 tile radius

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.Size = new(56, 84);
        Projectile.sentry = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.tileCollide = true;
        Projectile.friendly = true;
        for (int i = 0; i < _leaves.Length; i++)
        {
            _leaves[i].Id = i;
        }
    }
    public override void AI()
    {
        _leafSpawnTimer++;
        Projectile.timeLeft = 2;
        Projectile.velocity.Y += 0.2f;
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            if (_leafSpawnTimer >= LEAF_SPAWN_INTERVAL)
            {
                int freeIndex = Array.FindIndex(_leaves, (leaf) => !leaf.Alive);
                if (freeIndex != -1)
                {
                    ref var leaf = ref _leaves[freeIndex];
                    leaf.Position = new Vector2(Projectile.Center.X + Main.rand.Next(-15, 15), Projectile.Center.Y + Main.rand.Next(-20, 10));
                    leaf.DestinationScale = Main.rand.NextFloat(0.8f, 1f);
                    leaf.Scale = leaf.DestinationScale * 0.5f;
                    leaf.Rotation = Main.rand.NextFloat(-MathHelper.PiOver4/2, MathHelper.PiOver4/2) - MathHelper.PiOver2;
                    leaf.Alive = true;
                    _activeLeaves++;
                    _leafSpawnTimer = 0;
                }
            }
            if (_activeLeaves > 0  && CanShoot(out int target) && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int freeIndex = Array.FindIndex(_leaves, (leaf) => leaf.Alive);
                if (freeIndex != -1 && (_leaves[freeIndex].DestinationScale - _leaves[freeIndex].Scale) < 0.01f)
                {
                    _leaves[freeIndex].Alive = false;
                    _activeLeaves--;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                        _leaves[freeIndex].Position, _leaves[freeIndex].Rotation.ToRotationVector2() * 12,
                        ModContent.ProjectileType<TreeShot>(), Projectile.damage, 0, Projectile.owner, _leaves[freeIndex].Scale, target);
                }
            }
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D treeSprite = TextureAssets.Projectile[Type].Value;
        Texture2D leafSprite = TextureAssets.Projectile[ModContent.ProjectileType<TreeShot>()].Value;
        Main.spriteBatch.Draw(treeSprite, Projectile.Center - Main.screenPosition, null, lightColor, 0, treeSprite.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        for (int i = 0; i < _leaves.Length; i++)
        {
            if (_leaves[i].Alive)
            {
                _leaves[i].Scale = MathHelper.Lerp(_leaves[i].Scale, _leaves[i].DestinationScale, 0.05f);
                Main.spriteBatch.Draw(leafSprite, _leaves[i].Position - Main.screenPosition, null, lightColor, _leaves[i].Rotation, leafSprite.Size()/2, _leaves[i].Scale, SpriteEffects.None, 0);
            }
        }
        return false;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_activeLeaves);
        for(int i = 0; i < _leaves.Length; i++)
        {
            writer.WriteVector2(_leaves[i].Position);
            writer.Write(_leaves[i].Rotation);
            writer.Write(_leaves[i].Scale);
            writer.Write(_leaves[i].DestinationScale);
            writer.Write(_leaves[i].Alive);
        }
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _activeLeaves = reader.ReadInt32();
        for (int i = 0; i < _leaves.Length; i++)
        {
            _leaves[i].Position = reader.ReadVector2();
            _leaves[i].Rotation = reader.ReadSingle();
            _leaves[i].Scale = reader.ReadSingle();
            _leaves[i].DestinationScale = reader.ReadSingle();
            _leaves[i].Alive = reader.ReadBoolean();
        }
    }
    public override bool? CanDamage()
    {
        return false;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = false;
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
    private bool CanShoot(out int targetIndex)
    {
        targetIndex = -1;
        foreach(var npc in Main.ActiveNPCs)
        {
            if (npc.CanBeChasedBy() && !npc.friendly && npc.DistanceSQ(Projectile.Center) < TARGET_RANGE)
            {
                targetIndex = npc.whoAmI;
                return true;
            }
        }
        return false;
    }
}

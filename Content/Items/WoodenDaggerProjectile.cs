using System.Collections.Generic;
using Terraria.Audio;

namespace StartingWeapons.Content.Items;

public class WoodenDaggerProjectile : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    private bool hasHitEnemy = false;
    private int targetNPC = -1;
    private List<int> previousNPCs = new List<int>() { -1 };
    public int GravityDelayTimer
    {
        get => (int)Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }
    public override void SetDefaults()
    {
        // Modders can use Item.DefaultToRangedWeapon to quickly set many common properties, such as: useTime, useAnimation, useStyle, autoReuse, DamageType, shoot, shootSpeed, useAmmo, and noMelee. These are all shown individually here for teaching purposes.
        if (ModContent.TryFind("CalamityMod", "GildedDaggerProj", out ModProjectile GildedDaggerProj))
        {
            Projectile.CloneDefaults(GildedDaggerProj.Type);
            Projectile.timeLeft = 600;
            Projectile.penetrate = 5;
        }
    }
    private const int GravityDelay = 45;

    private void NormalAI()
    {
        GravityDelayTimer++; // doesn't make sense.

        // For a little while, the javelin will travel with the same speed, but after this, the javelin drops velocity very quickly.
        if (GravityDelayTimer >= GravityDelay)
        {
            GravityDelayTimer = GravityDelay;

            // wind resistance
            Projectile.velocity.X *= 0.98f;
            // gravity
            Projectile.velocity.Y += 0.35f;
        }
    }
    public override void AI()
    {
        Projectile.ai[0] += 1f; // Use a timer to wait 15 ticks before applying gravity.
        if (Projectile.ai[0] >= 15f)
        {
            Projectile.ai[0] = 15f;
            Projectile.velocity.Y = Projectile.velocity.Y + 0.6f;
        }
        if (Projectile.velocity.Y > 16f)
        {
            Projectile.velocity.Y = 16f;
        }
        if (Main.rand.NextBool(7))
        {
            Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Grass, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
        }
        if (hasHitEnemy)
        {
            Projectile.rotation += Projectile.direction * 0.4f;
        }
        else
        {
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.PiOver2);
            //Rotating 90 degrees if shooting right
            if (Projectile.spriteDirection == 1)
            {
                Projectile.rotation += MathHelper.ToRadians(90f);
            }
        }

    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        Projectile.Kill();
        return false;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        float minDist = 999f;
        int index = 0;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            bool hasHitNPC = false;
            for (int j = 0; j < previousNPCs.Count; j++)
            {
                if (previousNPCs[j] == npc.whoAmI)
                {
                    hasHitNPC = true;
                }
            }

            if (npc == target)
            {
                previousNPCs.Add(npc.whoAmI);
            }
            if (npc.CanBeChasedBy(Projectile, false) && npc != target && !hasHitNPC)
            {
                float dist = (Projectile.Center - npc.Center).Length();
                if (dist < minDist)
                {
                    minDist = dist;
                    index = npc.whoAmI;
                }
            }
        }
        Vector2 velocityNew;
        if (minDist < 999f)
        {

            hasHitEnemy = true;
            targetNPC = index;
            velocityNew = Main.npc[index].Center - Projectile.Center;
            velocityNew.Normalize();
            velocityNew *= 10f;
            Projectile.velocity = velocityNew;
        }
    }

}

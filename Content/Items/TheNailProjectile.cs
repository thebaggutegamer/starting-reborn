using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace StartingWeapons.Content.Items
{ 
	public class TheNailProjectile : ModProjectile
	{
        int timePerFrame = 5;
        int maxTimeLeft = 0;
        int timeLeft = 10;
        Vector2 direction = Vector2.One;
        private bool hasHit;
        private bool init;
        private bool disableCustomItemAnim;
        Vector2 offsetPos;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            
        }

        override public void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            timePerFrame = Projectile.timeLeft / Main.projFrames[Projectile.type];
            maxTimeLeft = timeLeft;

        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!init)
            {
                init = true;
                direction = Vector2.Normalize(new Vector2((float)Projectile.ai[0], (float)Projectile.ai[1]));
                Projectile.rotation = direction.ToRotation() - MathHelper.ToRadians(90f);
                float degRot = MathHelper.ToDegrees(Projectile.rotation);
                Projectile.spriteDirection = degRot < -180f || degRot >= 0 ? -1 : 1;
                offsetPos = Projectile.position - owner.position;
            }

            // --- POSING THE ARM ---
            owner.ChangeDir(Projectile.spriteDirection);

            // Calculate the current swing angle (same math used in PostDraw)
            float progress = (float)(maxTimeLeft - timeLeft) / (float)maxTimeLeft;
            float arcRange = 1.2f;
            float swingOffset = MathHelper.Lerp(-arcRange, arcRange, progress);
            float baseAngle = new Vector2(Projectile.ai[0], Projectile.ai[1]).ToRotation();

            // The arm should point toward the handle/rotation of the swing
            float armAngle = baseAngle + (swingOffset * Projectile.spriteDirection);

            // SetCompositeArmFront(enabled, stretchAmount, rotation)
            // We use PiOver2 offset for the rotation because arm rotation is relative to the body
            if(!disableCustomItemAnim)
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armAngle - MathHelper.PiOver2);
            // -----------------------

            Projectile.position = owner.position + offsetPos;
            Anim();

            if (timeLeft > 0)
                timeLeft--;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            float degRot = MathHelper.ToDegrees(Projectile.rotation);
            if (!hasHit && degRot < 15f && degRot > -15f)
            {
                hasHit = true;
                Player player = Main.player[Projectile.owner];
                player.velocity = -direction * 10f;
            }


        }


        private void Anim()
        {
            if (Projectile.frame > Main.projFrames[Projectile.type])
                return;

            Projectile.frameCounter++;

            if (Projectile.frameCounter < 5)
                return;

            Projectile.frame++;
            Projectile.frameCounter = 0;
        }


        public override void PostDraw(Color lightColor)
        {
            if(!disableCustomItemAnim)
            {
                Texture2D itemTexture = TextureAssets.Item[ModContent.ItemType<Items.TheNail>()].Value;
                Player owner = Main.player[Projectile.owner];

                // 1. Calculate Swing Progress (0.0 to 1.0)
                float progress = (float)(maxTimeLeft - timeLeft) / (float)maxTimeLeft;

                // 2. The Swing Arc (How wide the swipe is)
                float arcRange = 1.2f;
                float swingOffset = MathHelper.Lerp(-arcRange, arcRange, progress);

                // 3. Base direction from the shoot velocity
                float baseAngle = new Vector2(Projectile.ai[0], Projectile.ai[1]).ToRotation();

                // 4. Final Rotation Logic
                // We use PiOver4 (45 degrees) as the tilt.
                // We multiply swingOffset by spriteDirection so it swipes the correct way.
                float finalRotation = baseAngle + (swingOffset * Projectile.spriteDirection) + MathHelper.PiOver4;

                // If facing left, we need to flip the rotation logic slightly to maintain the 45-degree angle
                if (Projectile.spriteDirection == -1)
                {
                    finalRotation += MathHelper.PiOver2; // Adjust for the horizontal flip
                }

                // 5. Origin Point (The Handle)
                // Bottom-left for right-facing, Bottom-right for left-facing
                Vector2 origin = new Vector2(0, itemTexture.Height);
                if (Projectile.spriteDirection == -1)
                {
                    origin = new Vector2(itemTexture.Width, itemTexture.Height);
                }

                // 6. Draw centered on the Player's hand
                Vector2 drawPos = owner.MountedCenter - Main.screenPosition;

                Main.EntitySpriteDraw(
                    itemTexture,
                    drawPos,
                    null,
                    lightColor,
                    finalRotation,
                    origin,
                    Projectile.scale,
                    Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0
                );
            }
                
        }
    }
}

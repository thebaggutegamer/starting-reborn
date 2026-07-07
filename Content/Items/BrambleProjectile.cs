using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace StartingWeapons.Content.Items;
public class BrambleProjectile : ModProjectile
{
    // ai[0]
    private int _length;
    // ai[1]
    private float _projectileScaleOffset;
    // ai[2]
    private int _aiTimer { get { return (int)Projectile.ai[2]; } set { Projectile.ai[2] = value; } }
    public override void SetDefaults()
    {
        Projectile.penetrate = -1;
        Projectile.Size = new(32, 32);
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }
    public override void AI()
    {
        // adapted from vanilla, maybe mess with the numbers later
        const int TIME_TO_GROW = 15;
        const int TIME_TO_SHRINK = 20;
        const int TIME_TO_DIE = 80;
        _aiTimer++;
        if (_aiTimer < TIME_TO_GROW) {
            Projectile.localAI[0] += 1/15f;
            Projectile.scale = Projectile.localAI[0] * _projectileScaleOffset;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.JungleGrass, Projectile.velocity * MathHelper.Lerp(0.2f, 0.5f, Main.rand.NextFloat()));
            dust.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
            dust.velocity *= 0.5f;
            dust.scale = 0.8f + Main.rand.NextFloat() * 0.5f * 0.2f;
        }
        if (_aiTimer >= TIME_TO_SHRINK)
        {
            Projectile.localAI[0] -= 0.2f;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.JungleGrass, Projectile.velocity * MathHelper.Lerp(0.2f, 0.5f, Main.rand.NextFloat()));
            dust.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
            dust.velocity *= 0.5f;
            dust.scale = 0.8f + Main.rand.NextFloat() * 0.5f * 0.2f;
        }
        if (_aiTimer >= TIME_TO_DIE)
            Projectile.Kill();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D brambleSprite = TextureAssets.Projectile[Type].Value;
        Rectangle tipFrame = new Rectangle(0, 0, 38, 24);
        Rectangle loopFrame = new Rectangle(0, 26, 38, 44);
        Vector2 loopOrigin = new Vector2(19, loopFrame.Bottom);
        Vector2 tipOrigin = new Vector2(19, tipFrame.Bottom);
        Vector2 scale = new Vector2(Projectile.scale);
        scale.Y *= _aiTimer < 20 ? (float)_aiTimer / 20f : 1;
        scale.X *= _aiTimer > 70 ? (80 - _aiTimer) / 10f : 1;
        float x = scale.X;
        for (int i = 0; i < _length; i++)
        {
            Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * (44 * scale.Y * i);
            Main.spriteBatch.Draw(brambleSprite, Projectile.Center - Main.screenPosition + offset, loopFrame, lightColor*1.5f, Projectile.rotation + MathHelper.PiOver2, loopOrigin, scale, SpriteEffects.None, 0);
        }
        Vector2 tipOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * (44 * scale.Y * (_length) + (24*scale.Y));
        Vector2 bottomOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * (26*scale.Y);
        Main.spriteBatch.Draw(brambleSprite, Projectile.Center - Main.screenPosition + tipOffset, tipFrame, lightColor*1.5f, Projectile.rotation + MathHelper.PiOver2, tipOrigin, scale, SpriteEffects.None, 0);
        Main.spriteBatch.Draw(brambleSprite, Projectile.Center - Main.screenPosition + bottomOffset, tipFrame, lightColor*1.5f, Projectile.rotation - MathHelper.PiOver2, tipOrigin, scale, SpriteEffects.None, 0);
        return false;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float zero = 0;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 200 * Projectile.scale, 22f * Projectile.scale, ref zero);
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.Lerp(lightColor, Color.Black, 0.25f);
    }
    public override void OnSpawn(IEntitySource source)
    {
        _length = (int)Projectile.ai[0];
        _projectileScaleOffset = Projectile.ai[1];
        Projectile.rotation = Projectile.velocity.ToRotation();
        SoundEngine.PlaySound(in SoundID.Item60, Projectile.Center);
        for (int i = 0; i < 5; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), DustID.JungleGrass, Projectile.velocity * MathHelper.Lerp(0.2f, 0.7f, Main.rand.NextFloat()), Scale: Main.rand.NextFloat(0.5f, 0.8f));
            dust.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
            dust.scale = 0.8f + Main.rand.NextFloat() * 0.5f * 0.2f;
        }
        for (int j = 0; j < 5; j++)
        {
            Dust dust2 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), DustID.JungleGrass, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity * MathHelper.Lerp(0.2f, 0.5f, Main.rand.NextFloat()));
            dust2.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
            dust2.scale = 0.8f + Main.rand.NextFloat() * 0.5f * 0.2f;
            dust2.fadeIn = 1f;
        }
    }
    public override bool ShouldUpdatePosition()
    {
        return false;
    }
}

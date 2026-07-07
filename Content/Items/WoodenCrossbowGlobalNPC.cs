using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartingWeapons.Content.Items;
public class WoodenCrossbowGlobalNPC : GlobalNPC
{
    private static Asset<Texture2D> _crosshairTexture;
    public override void Load()
    {
        _crosshairTexture = ModContent.Request<Texture2D>("StartingWeapons/Assets/Images/CrossbowCrosshair");
    }
    public override bool InstancePerEntity => true;
    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.LocalPlayer.GetModPlayer<WoodenCrossbowPlayer>().TaggedTargetIds.Contains(npc.whoAmI))
        {
            spriteBatch.Draw(_crosshairTexture.Value, npc.Center - screenPos,  null, Color.White with { A = 127}, (float)Main.timeForVisualEffects*0.02f, _crosshairTexture.Size() / 2, 1f, SpriteEffects.None, 0);
        }
    }
}

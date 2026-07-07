using System.IO;
using Terraria.ModLoader.IO;

namespace StartingWeapons.Content.Items;

public sealed class WoodenClubGlobalNPC : GlobalNPC
{
    /// <summary>
    ///     The duration of the stun effect applied by the <see cref="WoodenStickProjectile" />, in ticks.
    /// </summary>
    public const int DURATION = 120;
    
    /// <summary>
    ///     Gets or sets whether the NPC attached to this global is affected by the stun effect applied by
    ///     the <see cref="WoodenStickProjectile" /> projectile.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets the timer of the NPC attached to this global for the stun effect applied by the
    ///     <see cref="WoodenStickProjectile" /> projectile.
    /// </summary>
    public int Timer { get; set; }

    /// <summary>
    ///     Gets or sets the original rotation of the NPC attached to this global for the stun effect
    ///     applied by the <see cref="WoodenStickProjectile" /> projectile.
    /// </summary>
    public float Rotation { get; set; }

    public override bool InstancePerEntity { get; } = true;

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        base.SendExtraAI(npc, bitWriter, binaryWriter);

        binaryWriter.Write(Enabled);
        
        binaryWriter.Write(Timer);
        binaryWriter.Write(Rotation);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        base.ReceiveExtraAI(npc, bitReader, binaryReader);

        Enabled = binaryReader.ReadBoolean();

        Timer = binaryReader.ReadInt32();
        Rotation = binaryReader.ReadSingle();
    }

    public override bool PreAI(NPC npc)
    {
        return !Enabled;
    }

    public override void PostAI(NPC npc)
    {
        base.PostAI(npc);

        UpdateNPCStun(npc);
    }

    private void UpdateNPCStun(NPC npc)
    {
        if (!Enabled)
        {
            return;
        }

        Timer++;

        npc.velocity *= 0.98f;

        npc.rotation += npc.velocity.X * 0.05f;

        if (Timer < DURATION)
        {
            return;
        }

        npc.rotation = Rotation;

        npc.velocity.Y = -4f;

        Timer = 0;

        Enabled = false;

        npc.netUpdate = true;
    }
}
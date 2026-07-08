using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameInput;

namespace StartingWeapons.Content.Items;

public class WoodenCrossbowPlayer : ModPlayer
{
    public int ShotsHit { get { return _shotsHit; }  set { _shotsHit = (int)MathHelper.Clamp(value, 0, 21); } }
    private int _shotsHit;
    public Queue<int> TaggedTargetIds = new Queue<int>(5);

    public override void ResetEffects()
    {
        // really bad???
        List<int> activeTargets = [];
        for (int i = 0; i < TaggedTargetIds.Count; i++)
        {
            int npcIndex = TaggedTargetIds.ElementAt(i);
            if (Main.npc[npcIndex].active)
                activeTargets.Add(npcIndex);
        }
        TaggedTargetIds = new Queue<int>(activeTargets);
    }

    public bool TryFindClosestTarget(Vector2 center, out int targetIndex)
    {
        targetIndex = -1;
        (float distance, int index) targetDistancePair = (float.MaxValue, -1);
        for (int i = 0; i < TaggedTargetIds.Count; i++)
        {
            NPC potentialTarget = Main.npc[TaggedTargetIds.ElementAt(i)];
            if (potentialTarget is not null && potentialTarget.active)
            {
                float distance = potentialTarget.DistanceSQ(center);
                if (targetDistancePair.distance > distance)
                {
                    targetDistancePair.distance = distance;
                    targetDistancePair.index = TaggedTargetIds.ElementAt(i);
                }
            }
        }
        if (Main.npc.IndexInRange(targetDistancePair.index))
        {
            targetIndex = targetDistancePair.index;
            return true;
        }
        else return false;
    }
}

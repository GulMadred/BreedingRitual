using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.GetPosture))]
    public static class Patch_PawnUtility_GetPosture
    {
        public static void Postfix(ref PawnPosture __result, ref Pawn p)
        {
            if (p.jobs != null && p.jobs.curDriver != null && p.jobs.curDriver as JobDriver_Carried != null)
            {
                // Pawn is being carried. But is it the correct pawn?
                if (LordJob_BreedingRitual.RitualParticipant(p.thingIDNumber))
                {
                    // It's one of the participants in the breeding ritual AND they're currently being carried.
                    //
                    // We can't assume that this is a bridal carry, though.
                    // Laying in bed has a similar signature. An additional check is needed.
                    if (__result != PawnPosture.LayingInBed)
                    {
                        // The pawn is NOT in bed. We can perform our graphical override.
                        __result = PawnPosture.LayingOnGroundFaceUp;

                        // TODO: LayingOnGroundNormal looks better in most cases (pawns are face-to-face)
                        // but sometimes it gets flipped - leaving the carried pawn staring at the ground.
                        // If we can figure out the flipping logic then we should alter this code.
                    }
                }
            }
        }
    }
}

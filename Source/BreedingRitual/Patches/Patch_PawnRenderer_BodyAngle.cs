using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BodyAngle))]
    public static class Patch_PawnRenderer_BodyAngle
    {
        public static void Postfix(ref float __result, ref Pawn ___pawn)
        {
            if (___pawn.jobs != null && ___pawn.jobs.curDriver != null && ___pawn.jobs.curDriver as JobDriver_Carried != null)
            {
                // Pawn is being carried. But is it the correct pawn?
                if (LordJob_BreedingRitual.RitualParticipant(___pawn.thingIDNumber))
                {
                    // It's one of the participants in the breeding ritual AND they're currently being carried.
                    //
                    // It's safe to assume that it's a Bridal Carry
                    // Note: the pawn MIGHT be in bed, but our numerical override is harmless - bed pawns ignore BodyAngle
                    __result = 300f;

                    // 300 degree rotation looks good for West and North orientation.
                    // We'd like a 60 degree rotation for East or South.
                    Pawn_CarryTracker carrier = (___pawn.ParentHolder) as Pawn_CarryTracker;
                    if ((carrier != null) && (carrier.pawn != null))
                    {
                        Rot4 rotation = carrier.pawn.Rotation;
                        if (rotation == Rot4.East || rotation == Rot4.South)
                        {
                            __result = 60f;
                        }
                    }
                }
            }
        }
    }
}

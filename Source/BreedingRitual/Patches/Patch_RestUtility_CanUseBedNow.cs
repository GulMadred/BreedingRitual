using HarmonyLib;
using RimWorld;
using Verse;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CanUseBedNow))]
    public static class Patch_RestUtility_CanUseBedNow
    {
        // This postfix method is intended to patch an unwanted if-check 
        // in the CanUseBedNow method. Transpiling would be more efficient,
        // but would also have broader/unwanted consequences.
        //
        // Although we INTEND to patch only the one if-check, it's simpler to
        // just bypass the entire thing. The ritual context means that the pawn
        // in question is currently trying to use their own ASSIGNED bed.
        // 
        // RimWorld is a bit too fussy about the rules, though. If the bed
        // is a prison bed, then RimWorld won't let a colonist sleep in it.
        // Even if the player put significant effort into getting the colonist
        // assigned to that bed. It won't unassign him - it just blocks him
        // from lying there (and hence he can't perform Lovin').
        public static void Postfix(ref bool __result, ref Thing bedThing, ref Pawn sleeper)
        {
            Building_Bed bed = bedThing as Building_Bed;
            if (BreedingRitual.BreedingRitualSettings.bedUsageTolerance &&
                LordJob_BreedingRitual.RitualParticipant(sleeper.thingIDNumber) &&
                (bed != null) && bed.IsOwner(sleeper))
            {
                // The mod option is active, THIS is a ritual participant, and THIS
                // bed is assigned to him. Override any previous logic. Let him use it.
                __result = true;
            }
        }
    }
}

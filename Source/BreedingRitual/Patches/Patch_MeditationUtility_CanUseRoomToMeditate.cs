using HarmonyLib;
using RimWorld;
using Verse;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(MeditationUtility), nameof(MeditationUtility.CanUseRoomToMeditate))]
    public static class Patch_MeditationUtility_CanUseRoomToMeditate
    {
        // While a ritual is ongoing, people are allowed to meditate in the ritual
        // bedroom. Otherwise the psybreeding ritual wouldn't work properly
        // because all of the spectators would be too "polite" to meditate
        // in the couple's bedroom. Instead they'd just revert to dancing.
        //
        // Note that this also works for barracks with multiple beds.
        public static void Postfix(ref bool __result, ref Room r, ref Pawn p)
        {
            if (LordJob_BreedingRitual.roomID == -1)
            {
                // The ritual is occurring outdoors. No intervention needed.
                return;
            }
            else if (LordJob_BreedingRitual.roomID != r.ID)
            {
                // This isn't the ritual room. Do not allow meditation here.
                return;
            }
            else
            {
                // This is the ritual room. Allow meditation.
                __result = true;
            }
        }
    }
}

using HarmonyLib;
using RimWorld;
using Verse;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), nameof(CompAssignableToPawn_Bed.IdeoligionForbids))]
    public static class Patch_CompAssignableToPawn_Bed_IdeoligionForbids
    {
        public static void Postfix(ref bool __result, ref Pawn pawn)
        {
            // Does the player want us to suppress ideoligion error messages?
            if (BreedingRitual.BreedingRitualSettings.suppressIdeoErrors)
            {
                // Yes. The game has already decided whether Ideoligion rules
                // would forbid this action. Simply overwrite its answer.
                __result = false;
                return;
            }
            else
            {
                // No. Let the original result stand.
            }
        }
    }
}
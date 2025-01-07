using HarmonyLib;
using RimWorld;
using Verse;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.GetPregnancyApproachForPartner))]
    public static class Patch_PawnRelationsTracker_GetPregnancyApproachForPartner
    {
        public static void Postfix(ref PregnancyApproach __result, ref Pawn ___pawn, Pawn partner)
        {
            // Check whether any action is needed
            if (!BreedingRitual.BreedingRitualSettings.overridePregnancyApproach)
            {
                // The pregnancy-approach-override option is OFF
                // No need to intervene. Return the original dict-lookup as usual.
                return;
            }

            // Okay, the pregnancy-approach-override option is active. We're supposed to intervene.
            // But this is a postfix method, so it will trigger on EVERY dictionary lookup.
            // Remember that we don't WANT to adjust most of those. Even during a breeding ritual,
            // there could be other pawns independently Lovin' elsewhere on the map. We must invoke 
            // the postfix logic ONLY for the pawns who are currently participating in the ritual.

            // Attempt to match our instance+param pawns to the IDs of the ritual participants
            if (LordJob_BreedingRitual.RitualParticipant(___pawn.thingIDNumber) &&
                LordJob_BreedingRitual.RitualParticipant(partner.thingIDNumber))
            {
                // We've found a match. The Player want THIS couple to Try for Baby.

                // We do NOT delve into the actual PawnRelations dictionary. It would be very inappropriate
                // to make any permanent changes to RimWorld data structures. Instead we just replace
                // the RESULT of the lookup. From the player's perspective, the couple's approach will
                // appear to change (to Try for Baby) and then this change will auto-revert when the ritual ends.
                __result = PregnancyApproach.TryForBaby;
                return;
            }
            else
            {
                // We've intervened in a pregnancy calculation which exists OUTSIDE our ritual.
                // The Player wants the ritual couple to Try for Baby ... but THIS isn't the ritual couple.

                // Therefore we mustn't alter the result. Return the original dict-lookup as usual.
                return;
            }
        }
    }
}

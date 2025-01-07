using HarmonyLib;
using RimWorld;
using Verse;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetLovinMtbHours))]
    public static class Patch_LovePartnerRelationUtility_GetLovinMtbHours
    {
        public static void Postfix(ref float __result, ref Pawn pawn, ref Pawn partner)
        {
            // Attempt to match the param pawns to the IDs of the ritual participants
            if (LordJob_BreedingRitual.RitualParticipant(pawn.thingIDNumber) ||
                LordJob_BreedingRitual.RitualParticipant(partner.thingIDNumber))
            {
                // We've found a match. One or more of these pawns are involved in a ritual.
                //
                // While it's very sweet that they're thinking of doing Lovin' on their own 
                // initiative, we don't actually *want* that to happen. The mod code expects
                // to start EVERY Lovin' action that occurs during the ritual. If the couple
                // starts one ... then we won't be able to count it or track it properly.
                //
                // Such an occurrence doesn't fundamentally mess up the ritual. The couple will
                // still perform as expected and pregnancy will (potentially) occur. But the
                // reported statistics will be wrong (which may confuse players) and the duration
                // will deviate from expectations. That's no-big-deal for a Breeding ritual -
                // it might take 2000 ticks where the player expected 1600. But it screws up
                // the timing for Psybreeding, which is unacceptable.
                //
                // Note: you might ask "how can this actually happen?" The mod intervenes on the
                // first possible Tick - as soon as the man lays down, we order him to do Lovin'.
                // But the woman might have arrived in bed first (if she's been Bridal Carry-ed,
                // then she'll ALWAYS arrive first). In the ~30 ticks it takes the man to walk
                // around the bed and settle in, she might spontaneously decide to start some
                // Lovin'. This ought to be rare, but a very high libido can make it an almost-
                // guaranteed occurrence.

                __result = -1f;
                return;
            }
            else
            {
                // We've intervened in the Lovin' affairs of someone unrelated to the ritual.
                // We mustn't make any changes. Let the original result stand.
                return;
            }
        }
    }
}

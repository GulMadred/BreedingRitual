using HarmonyLib;
using Verse;
using RimWorld;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.PregnancyChanceForPartners))]
    public static class Patch_PregnancyUtility_PregnancyChanceForPartners
    {
        public static void Postfix(ref float __result, Pawn woman, Pawn man)
        {
            // This is a postfix method, so it will potentially intervene in EVERY pregnancy calculation.
            // Remember that we don't WANT to adjust most of those. Even during a breeding ritual,
            // there could be other pawns independently Lovin' elsewhere on the map. We must invoke 
            // the postfix logic ONLY for the pawns who are currently participating in the ritual.
            if (!LordJob_BreedingRitual.RitualParticipant(man.thingIDNumber) ||
                !LordJob_BreedingRitual.RitualParticipant(woman.thingIDNumber))
            {
                // We've intervened in a pregnancy calculation which exists OUTSIDE our ritual.
                // Perhaps there's no ritual happening right now. Perhaps the couple we're
                // evaulating is NOT the couple engaged in the ritual.

                // Regardless, we mustn't interfere. Let the original result stand.
                return;
            }

            // At this point, we know that we're intervening in a pregnancy calculation which pertains
            // to our breeding ritual. We must check the Options to decide how to adjust things.

            // When a couple does ritual Lovin', their jobs will complete on the same Tick.
            // They'll each make an independent check for pregnancy. That's weird logic,
            // and it skews the math. The BreedingRitual mod has an option to disable it,
            // so that a breeding ritual will generate only ONE pregnancy check per tick.
            if (!RitualBehaviorWorker_Breeding.pregnancyAllowedThisTick)
            {
                // A pregnancy check has already been made on this Tick. We can't make another
                // because that would skew the overall fertility math. Auto-fail the preg check.
                __result = 0f;
                return;
            } else if (BreedingRitual.BreedingRitualSettings.singlePregnancyCheck)
            {
                // We're making a pregnancy check for the ritual couple. Setup a flag so that 
                // a SECOND check occurring within the same tick will automatically fail.
                RitualBehaviorWorker_Breeding.pregnancyAllowedThisTick = false;
            }

            if (BreedingRitual.BreedingRitualSettings.overridePregnancyApproachCheat || 
                ((LordJob_AnimabreedingRitual.animaTree != null) && 
                (BreedingRitual.BreedingRitualSettings.animaFertilityBoost == BreedingRitual.BreedingRitualSettings.AnimaFertilityBoostMax)) )
            {
                // The player has invoked the Cheat option. Our goal is to ensure that conception happens.

                // Rimworld code will internally multiply our returned value by 0.05f when doing conception RNG.
                // Therefore we ought to return 20f, which would provide a 100% chance of success.
                // ... but instead we'll return 200f just to be thorough.
                __result = 200f;
                return;
            }

            if (!BreedingRitual.BreedingRitualSettings.psyboostFertility)
            {
                // The Player has opted to disable the fertility psy-boost.
                // Do nothing.
            }
            else if (LordJob_PsybreedingRitual.psySensitivityTotal < 0f)
            {
                // There isn't a psybreeding ritual taking place right now.
                // Therefore we mustn't apply the psy-boost to fertility.
            }
            else
            {
                // Increase the fertility value by applying the psy-sensitivity boost
                __result *= LordJob_PsybreedingRitual.psySensitivityTotal;
            }

            if (LordJob_AnimabreedingRitual.animaTree == null)
            {
                // There isn't an animabreeding ritual in-progress right now.
            }
            else if (BreedingRitual.BreedingRitualSettings.animaFertilityBoost <= 1f)
            {
                // There's an animabreeding ritual in-progress, but the player has disabled the boost
            }
            else
            {
                // Increase the fertility value by applying the anima boost
                __result *= BreedingRitual.BreedingRitualSettings.animaFertilityBoost;
            }
            return;
        }
    }
}

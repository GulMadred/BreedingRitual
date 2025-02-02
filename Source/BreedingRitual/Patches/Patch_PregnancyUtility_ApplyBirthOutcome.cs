using HarmonyLib;
using Verse;
using RimWorld;
using static RimWorld.PsychicRitualRoleDef;

namespace BreedingRitual.Patches
{
    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ApplyBirthOutcome))]
    public static class Patch_PregnancyUtility_ApplyBirthOutcome
    {
        public static void Postfix(ref Thing __result, Pawn geneticMother, Pawn father, Thing birtherThing)
        {
            // This is a postfix method. Unlike most of our patches, this one is universal.
            // We don't attempt to apply it exclusively onto Ritual participants, because childbirth
            // occurs long after the ritual is complete.

            // The purpose of this patch is to react immediately after childbirth. The newborn
            // will have been assigned a sex at random. The result may be implausible.
            // If we find a nonsense result, simply overwrite the newborn's sex.

            if (!ModsConfig.RoyaltyActive)
            {
                // This patch is relevant only for Animabreeding (which requires Royalty DLC).
                return;
            }
            else if (!BreedingRitual.BreedingRitualSettings.sanitizeNewbornGender)
            {
                // The intervention option is disabled. Do nothing.
                return;
            }
            
            Pawn newborn = (Pawn)__result;

            if (geneticMother != null && geneticMother.gender == Gender.Female &&
                father != null && father.gender == Gender.Female)
            {
                // This is a lesbian couple. There's no Y chromosome available.
                newborn.gender = Gender.Female;
            }
            else if (geneticMother != null && father != null && father.thingIDNumber == geneticMother.thingIDNumber)
            {
                // This is a clone.
                //
                // Recombination of sperm could yield XX, but players may find it confusing.
                // To keep things simple, we'll just apply the parent's sex onto the newborn.
                newborn.gender = geneticMother.gender;
            }

        }
    }
}
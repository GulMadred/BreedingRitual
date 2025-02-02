using System.Linq;
using Verse;

namespace RimWorld
{
    public class RitualRole_Childbearer : RitualRole_BreedingCandidate
    {
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason))
            {
                return false;
            }

            // Note: it may seem duplicative to perform the following checks, because these eligibility rules 
            // *are* enforced elsewhere. The reason is that THESE checks provide immediate feedback to the player.
            // The other checks are (potentially) more confusing because they'll cause a ritual to be cancelled
            // immediately after it begins. These checks can guide the player during the Ritual Planning stage.

            if (!BreedingRitual.BreedingRitualSettings.blockSterileAnimabreeding)
            {
                // If the player is willing to allow hopeless anima-breeding rituals ...
                // then we don't need to perform any checks. Just let it proceed and see what happens.
                return true;
            }

            if (!BreedingRitual.BreedingRitualSettings.allowMalePregnancy && (p.gender == Gender.Male))
            {
                reason = "MessageAnimabreedingMPregForbidden".Translate(p.Named("PAWN")).CapitalizeFirst();
                return false;
            }

            if (PregnancyUtility.GetPregnancyHediff(p) != null)
            {
                reason = "MessageAnimabreedingPregnantForbidden".Translate(p.Named("PAWN")).CapitalizeFirst();
                return false;
            }

            if (!BreedingRitual.BreedingRitualSettings.allowHomosexualBreeding && assignments != null && 
                assignments.Participants != null && assignments.Participants.Any(pawn=>(pawn.gender == p.gender) && (pawn.thingIDNumber != p.thingIDNumber) && (!assignments.PawnSpectating(pawn))))
            {
                reason = "MessageAnimabreedingHomoForbidden".Translate().CapitalizeFirst();
                return false;
            }

            if (BreedingRitual.BreedingRitualSettings.overridePregnancyApproachCheat ||
                BreedingRitual.BreedingRitualSettings.animaFertilityBoost == BreedingRitual.BreedingRitualSettings.AnimaFertilityBoostMax)
            {
                // We don't need to check whether this pawn is sterile, because a fertility-bypass cheat is active
            }
            else if (PregnancyUtility.PregnancyChanceForPawn(p) == 0f)
            {
                reason = "MessageAnimabreedingSterileForbidden".Translate(p.Named("PAWN")).CapitalizeFirst();
                return false;
            }

            return true;
        }
    }
}
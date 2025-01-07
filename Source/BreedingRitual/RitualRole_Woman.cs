using System;
using System.Linq;
using Verse;

namespace RimWorld
{
    public class RitualRole_Woman : RitualRole_BreedingCandidate
    {
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason)) {
                return false;
            }

            if (p.gender != Gender.Female)
            {
                reason = "A woman is needed to fulfill this role.";
                return false;
            }

            // Note that we do NOT check for fertility. Sterile pawns are eligible to participate.
            // The mod will announce the odds of success; if players want to waste time with hopeless breeding attempts then they're free to do so.

            // We do, however, check for pregnancy (if the Player wants us to do so)
            if (!BreedingRitual.BreedingRitualSettings.allowPregnantWomen && PregnancyUtility.GetPregnancyHediff(p) != null)
            {
                reason = "Pregnant women aren't eligible for breeding (check mod Options).";
                return false;
            }

            return true;
        }
    }
}

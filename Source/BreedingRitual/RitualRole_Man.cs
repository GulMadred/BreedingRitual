using Verse;

namespace RimWorld
{
    public class RitualRole_Man : RitualRole_BreedingCandidate
    {
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason))
            {
                return false;
            }

            if (p.gender != Gender.Male)
            {
                reason = "A man is needed to fulfill this role.";
                return false;
            }

            // Note that we do NOT check for fertility. Sterile pawns are eligible to participate.
            // The mod will announce the odds of success; if players want to waste time with hopeless breeding attempts then they're free to do so.

            return true;
        }
    }
}

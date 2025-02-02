using System;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
    public class RitualRole_BreedingCandidate : RitualRole
    {
        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            // Child-exclusion is NOT handled by the mod code; it's part of the XML definition of the ritual.
            // We could belt-and-suspenders it with an additional juvenile check, but I'm worried about accidentally
            // creating conflicts with some of the big overhaul mods (e.g. Humanoid Alien Races).
            if (!base.AppliesIfChild(p, out reason, skipReason))
            {
                reason = "This role requires an adult.";
                return false;
            }

            if (p.GetLord() != null)
            {
                if (!p.GetLord().LordJob.ToString().ToLower().Contains("hospitality"))
                {
                    // This person is not a guest. I don't know what they are exactly.
                    // Therefore we do nothing. Let the process proceed (or fail) naturally.
                }
                else if (!BreedingRitual.BreedingRitualSettings.seduceGuests)
                {
                    reason = "Seduction of guests is not allowed (check mod options).";
                    return false;
                }
                else
                {
                    // This person is a guest ... but we're allowed to seduce them.
                    //
                    // In order for the ritual to proceed, we'll need to Release
                    // the pawn from their current Lord. We do NOT do that here.
                    // This is just the planning context. The player might decide
                    // NOT to begin the ritual, so any action in this context
                    // would be premature. Check the method:
                    //   RitualBehaviorWorker_Breeding.TryExecuteOn
                    // to see the actual Lord-release logic.
                }
            }

            // The ritual targets a BED rather than a pawn. Players are theoretically free to swap to different pawns,
            // but we don't actually let them do so (because reassigning beds mid-ritual would be painful and prone to errors).
            if (selectedTarget != null)
            {
                Building_Bed building_Bed = (Building_Bed)selectedTarget.Thing;
                if (building_Bed == null || building_Bed.GetAssignedPawns() == null || !building_Bed.GetAssignedPawns().Contains(p))
                {
                    reason = "The people assigned to this bed must participate in the ritual.";
                    return false;
                }
            }
            return true;
        }

        // Unused by this mod. Included because the compiler gets annoyed if we don't override this function.
        public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn p = null, bool skipReason = false)
        {
            reason = null;
            return false;
        }
    }
}

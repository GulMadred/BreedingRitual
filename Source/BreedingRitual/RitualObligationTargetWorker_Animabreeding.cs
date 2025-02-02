using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
    // The TARGET for the Breeding ritual is a bed. Therefore whenever a bed is selected, the game must decide
    // whether to display the "begin animabreeding ritual" button. This is analogous to the "Gather for childbirth" button.
    // Obviously, we want to show the button only under appropriate circumstances (when it's currently clickable
    // or ALMOST clickable). Showing it constantly would annoy players and waste GUI space.
    public class RitualObligationTargetWorker_Animabreeding : RitualObligationTargetWorker_Breeding
    {
        public RitualObligationTargetWorker_Animabreeding()
        {
        }

        public RitualObligationTargetWorker_Animabreeding(RitualObligationTargetFilterDef def)
            : base(def)
        {
        }

        public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
        {
            return new List<TargetInfo>();
        }

        // This is the payload. It's a boolean function which determines whether to show the Start Ritual gizmo (aka button)
        protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
        {
            // The target is not a THING. It's probably a pawn.
            if (!target.HasThing)
            {
                return false;
            }

            // The target is a THING. Assume that it's a bed.
            Building_Bed building_Bed = target.Thing as Building_Bed;
            if (building_Bed == null)
            {
                // Nope. Wasn't a bed.
                return false;
            }
            if (!building_Bed.def.building.bed_humanlike)
            {
                // It's a bed, but it's the wrong kind (e.g. animal crate)
                return false;
            }
            if (building_Bed.Faction == null || !building_Bed.Faction.IsPlayer)
            {
                // It's a bed, but it's an unclaimed ruin. Not suitable.
                return false;
            }

            // At this point we've got a usable bed owned by the player faction. We don't bother to check whether
            // it's a Double Bed. Doing so would be a mistake, because it might accidentally exclude mod-based
            // equivalents (various royal beds, ergonomic beds, etc).
            // Instead we just count the number of pawns assigned to sleep here.
            if (building_Bed.GetAssignedPawns() == null || building_Bed.GetAssignedPawns().Count<Pawn>() < 2)
            {
                // If the number is less than 2 then we don't care WHY. It might be a crib, it might be a Double Bed
                // which belongs to a bachelor. In any case, breeding can't happen here.
                return false;

                // Note: we implicitly allow beds which contain 3+ people. It's up to players to test such
                // scenarios; I don't know whether they'll be officially supported. The breeding ritual itself
                // will always have two main participants. Extra people won't be able to join them in the giant
                // polyamory bed, but they'll be able to spectate.
            }

            // Is this bed nearby to an Anima tree?
            Plant animaTree = FindAnimaTree(building_Bed.Position, building_Bed.Map);
            if (animaTree == null)
            {
                // No Anima tree found
                return false;
            }

            // This is a suitable location for an anima-breeding ritual to occur. Show the button.
            return true;

            // Note: the button may not actually be clickable; it's susceptible to various additional factors (such as
            // Leader cooldowns, identical ritual already in-progress, lady already pregnant, etc). But showing it is
            // okay because it automatically includes a tooltop ("this button is disabled for the next 2 hours because...").
        }

        public static Plant FindAnimaTree(IntVec3 position, Map map, float searchRadius = 6f)
        {
            ThingDef animaTreeDef = DefDatabase<ThingDef>.GetNamed("Plant_TreeAnima");
            return (Plant)GenClosest.ClosestThingReachable(position, map, ThingRequest.ForDef(animaTreeDef), PathEndMode.Touch, TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater, Danger.None), searchRadius);
        }
    }
}

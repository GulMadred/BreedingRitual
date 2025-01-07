using System.Linq;
using Verse.AI;
using Verse;

namespace RimWorld
{
    internal class LordToil_SpectateMeditate : LordToil_SpectateDanceMusic
    {
        public LordToil_SpectateMeditate(IntVec3 spot, LordJob_Ritual ritual, RitualStage stage, Pawn organizer)
            : base(spot, ritual, stage, organizer) { }

        // We change the base-class behavior in two ways:
        // >spectators seek MeditationSpots instead of instruments
        // >all spectators try to use buildings (instead of only half)
        protected override PawnDuty DutyForPawn(Pawn pawn, int i)
        {
            // Try to find a meditation spot
            // Unlike scheduled meditation, we don't accept want pawns to just sit down at random locations
            // If they can't find a designated Meditation Spot in the ritual zone, they'll fallback to
            // other spectator activities (dance/drum).
            
            // First - if this pawn has already reserved a nearby Meditation Spot then continue to use that one
            Building meditationSpot = (from m in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MeditationSpot)
                                       where GatheringsUtility.InGatheringArea(m.InteractionCell, this.spot, pawn.Map) && pawn.Map.reservationManager.ReservedBy(m, pawn)
                                       select m).RandomElementWithFallback(null);

            // Otherwise - attempt to reserve any free spot
            meditationSpot = meditationSpot ?? (from m in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MeditationSpot)
                                                                        where GatheringsUtility.InGatheringArea(m.InteractionCell, this.spot, pawn.Map) && MeditationUtility.IsValidMeditationBuildingForPawn(m, pawn)
                                                                        && !this.reservedThings.Contains(m)
                                                                        select m).RandomElementWithFallback(null);
            if (meditationSpot != null)
            {
                DutyDef dutyDef = DefDatabase<DutyDef>.GetNamed("MeditateAtTarget");
                this.ritual.usedThings.Add(meditationSpot);
                this.reservedThings.Add(meditationSpot);
                LocalTargetInfo spot = new LocalTargetInfo(meditationSpot);
                LocalTargetInfo focus = new LocalTargetInfo(ritual.selectedTarget.Thing);
                LocalTargetInfo inv = LocalTargetInfo.Invalid;
                return new PawnDuty(dutyDef, spot, inv, focus, 1f);
            }
            else
            {
                // No reachable meditation spot found/available
                ((LordJob_PsybreedingRitual)this.ritual).WarnInsufficientMeditationSpots();

                // Fallback to the default task (presumably dancing)
            }
            return base.DutyForPawn(pawn, i);
        }
    }
}

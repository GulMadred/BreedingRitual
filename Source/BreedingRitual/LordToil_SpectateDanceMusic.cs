using System.Linq;
using Verse.AI;
using Verse;

namespace RimWorld
{
    internal class LordToil_SpectateDanceMusic : RimWorld.LordToil_Ritual
    {
        public LordToil_SpectateDanceMusic(IntVec3 spot, LordJob_Ritual ritual, RitualStage stage, Pawn organizer)
            : base(spot, ritual, stage, organizer)
        {
            this.data = new LordToilData_PartyDanceDrums();
        }

        // This is our payload. We're going to slightly change the PartyDanceDrums
        // logic so that it skips the main participants in the ritual
        // (they're supposed to keep Lovin' while everyone ELSE dances).
        // We also want even-numbered spectators instead of odd-numbered
        // spectators to grab the instruments (so that if there's only ONE spectator
        // then they'll play music instead of dancing around alone like a jackass).
        public override void UpdateAllDuties()
        {
            // RimWorld has a nasty tendency to perform unwanted updates mid-ritual.
            // This usually happens during Stage transitions. Even if the duties
            // before-and-after are identical, RimWorld will randomly reassign everyone.
            // This looks bad - people rush to new positions, swap musical instruments, etc.
            // TODO: playerForced didn't resolve the problem. Find another approach!

            // Assign normal duties to all participants (based on RitualDef)
            base.UpdateAllDuties();

            if (!BreedingRitual.BreedingRitualSettings.playInstruments)
            {
                // The Player doesn't want anyone to perform music. Abort immediately.
                return;
            }

            // Reset the list of reserved instruments
            this.reservedThings.Clear();

            // Iterate through the list of participants
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                Pawn p = this.lord.ownedPawns[i];
                if (!this.ritual.assignments.PawnSpectating(p))
                {
                    // This isn't a spectator. Leave them alone.
                }
                else if ((p.mindState.duty != null) && (p.mindState.duty.def.defName != "PartyDance"))
                {
                    // This is a spectator, but they're performing a special duty.
                    // Leave them alone.
                }
                else
                {
                    // Try to assign a special task (such as drumming)
                    // If none is provided, stick with the default task
                    p.mindState.duty = DutyForPawn(p, i) ?? p.mindState.duty;
                }
            }
        }

        // Copy/pasted from LordJob_PartyDanceDrums. Slightly edited for better control flow.
        // (the original implementation used an inefficient nested loop)
        protected virtual PawnDuty DutyForPawn(Pawn pawn, int i)
        {
            if ((i % 2) == 1)
            {
                // This is a spectator ... but they're odd-numbered
                // We only want to tinker with even-numbered spectators (because we're
                // seeking a 50/50 mix of musicians and dancers).

                // Therefore we leave this pawn alone
                return null;
            }
            else
            {
                // This pawn should be reassigned to music-playing duty
                // Try to find an instrument for them

                // Although this class mentions the word "drum", we don't actually discriminate.
                // Any <Building_MusicalIntrument> will be employed if it's accessible and nearby
                // to the ritual site. Pianos, harpsichords, and harps will all get played just like drums.

                LocalTargetInfo localTargetInfo = LocalTargetInfo.Invalid;
                Building_MusicalInstrument building_MusicalInstrument = (from m in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_MusicalInstrument>()
                                                                         where GatheringsUtility.InGatheringArea(m.InteractionCell, this.spot, pawn.Map) && GatheringWorker_Concert.InstrumentAccessible(m, pawn)
                                                                         select m).RandomElementWithFallback(null);
                if (building_MusicalInstrument != null && building_MusicalInstrument.Spawned)
                {
                    DutyDef dutyDef = DutyDefOf.PlayTargetInstrument;
                    localTargetInfo = building_MusicalInstrument;
                    this.ritual.usedThings.Add(building_MusicalInstrument);
                    this.reservedThings.Add(building_MusicalInstrument);
                    (this.Data as LordToilData_PartyDanceDrums).playedInstruments.SetOrAdd(pawn, building_MusicalInstrument);
                    return new PawnDuty(dutyDef, spot, localTargetInfo, (LocalTargetInfo)this.ritual.selectedTarget, -1f);
                }
                else
                {
                    // No reachable instruments found/available
                    // Fallback to the default task (presumably dancing)
                    return null;
                }
            }
        }
    }
}

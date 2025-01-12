using System.Collections.Generic;
using System.Linq;
using Verse.AI.Group;
using Verse;
using Verse.Sound;

namespace RimWorld
{
    public class RitualBehaviorWorker_PsyBreeding : RitualBehaviorWorker_Breeding
    {
        public RitualBehaviorWorker_PsyBreeding()
        {
        }

        public RitualBehaviorWorker_PsyBreeding(RitualBehaviorDef def)
            : base(def) { }

        // The tick method allows us to tinker with a ritual in-progress and manipulate pawn behavior
        public override void Tick(LordJob_Ritual ritual)
        {
            // The base-class Tick behavior handles conventional requirements
            // (such forcing pawns to do Lovin' actions)
            base.Tick(ritual);

            // If the ritual's scheduled duration is over (i.e. couple is napping, everyone else has left)
            // then we don't do any further psy stuff.
            if (ritual.TicksLeft < 0) { return; }

            // When progress hits 100%, attempt the awakening.
            if (ritual.TicksLeft == 0) { ((LordJob_PsybreedingRitual) ritual).AttemptPsyAwakening(); }

            // Okay, we're in the middle of the psybreeding ritual. The couple is probably Lovin'.
            // We need to do some psy stuff: reduce Psyfocus, add Neural Heat.
            // First, we must identify the psycaster(s)
            List<Pawn> psycaster = new List<Pawn>();
            Pawn man = ritual.PawnWithRole("man");
            Pawn woman = ritual.PawnWithRole("woman");
            if (man != null && man.HasPsylink) { psycaster.Add(man); }
            if (woman != null && woman.HasPsylink) { psycaster.Add(woman); }
            if (psycaster.Empty())
            {
                // This should be impossible. Presumably someone is messing around by *removing* psy
                // powers mid-ritual (e.g. for testing purposes). Perhaps one of the pawns has died.
                // There's nothing useful that we can do at this point. Abort.
                CancelBreedingRitual((Building_Bed)ritual.selectedTarget, "MessagePsybreedingLostPsycaster".Translate().CapitalizeFirst(), ritual);
            }

            if (ritual.CurrentStage.GetDuty(man, null, ritual).defName == "KeepLyingDown")
            {
                // We're in the appropriate ritual stage. Pawns should be Lovin' right now.
                foreach (Pawn p in psycaster)
                {
                    // Apply the per-tick Psyfocus and Psyheat changes
                    p.psychicEntropy.OffsetPsyfocusDirectly(LordJob_PsybreedingRitual.perTickPsyfocus);
                    if (BreedingRitual.BreedingRitualSettings.neuralHeatPsybreeding > 0f)
                    {
                        if (p.psychicEntropy.TryAddEntropy(LordJob_PsybreedingRitual.perTickPsyheat, (Building_Bed)ritual.selectedTarget, true, false))
                        {
                            // Added psyheat successfully
                        }
                        else
                        {
                            // Failed to add psyheat
                            CancelBreedingRitual_PsychicHeat(p, ritual);
                            return;
                        }
                    }
                }
            }


            // The remainder is just special effects

            // Create the VFX (ripple) and SFX (psychic "warmup" noise) if needed
            Building_Bed bed = ((Building_Bed)ritual.selectedTarget.Thing);
            if (BreedingRitual.BreedingRitualSettings.psybreedingRipple && (this.mote == null || this.mote.Destroyed))
            {
                // The mote appears between the couple
                this.mote = MoteMaker.MakeStaticMote((man.TrueCenter() + woman.TrueCenter()) / 2f, bed.Map, ThingDefOf.Mote_Bestow, 1f, false, 0f);
                //mote.Scale = 0.5f;
            }
            if ((this.sound == null || this.sound.Ended) && ritual.TicksLeft <= JobDriver_BestowingCeremony.PlayWarmupSoundAfterTicks)
            {
                this.sound = SoundDefOf.Bestowing_Warmup.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(bed.Position, bed.Map, false), MaintenanceType.PerTick));
            }
            // If the VFX and SFX already exist, just sustain them
            if (this.mote != null)
            {
                // The psychic mote gradually expands as the ritual progresses
                // The final size is smaller than usual; we don't want it to expand much beyond the edge of the bed
                mote.Scale = (ritual.DurationTicks - ritual.TicksLeft) / (2f * ritual.DurationTicks);
                mote.Maintain();
            }
            if (this.sound != null)
            {
                this.sound.Maintain();
            }
        }

        private Mote mote;
        private Sustainer sound;
        //public const int PlayWarmupSoundAfterTicks = 307;
        //private const float WarmupSoundLength = 5.125f;

        protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            return new LordJob_PsybreedingRitual(target, ritual, obligation, this.def.stages, assignments, organizer);
        }

        // This method is invoked when the player begins a ritual. We use it to perform some crucial setup, logging, reporting, etc.
        public override void TryExecuteOn(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments, bool playerForced = false)
        {
            // Normally we'd call the base class method first. Unfortunately it will perform a Fertility calculation
            // and it would potentially report an incorrect value. Therefore we must populate some information first.
            List<Pawn> psycaster = new List<Pawn>();
            Pawn man = assignments.FirstAssignedPawn("man");
            Pawn woman = assignments.FirstAssignedPawn("woman");
            if (man != null && man.HasPsylink) { psycaster.Add(man); }
            if (woman != null && woman.HasPsylink) { psycaster.Add(man); }
            // Sum the pair's psychic sensitivity
            LordJob_PsybreedingRitual.psySensitivityTotal = man.GetStatValue(StatDefOf.PsychicSensitivity) + woman.GetStatValue(StatDefOf.PsychicSensitivity);

            // At this point we could safely call the base-class method.
            // But instead let's finish the remaining psy-setup first.

            // We need to setup the psy-specific stuff. Begin by resetting the variables.
            LordJob_PsybreedingRitual.perTickPsyheat = -1f;
            LordJob_PsybreedingRitual.perTickPsyfocus = 0f;

            // Assign appropriate values based on Options values (e.g. length of the ritual)
            // TODO: try to be more precise about this (set a pawn-specific focus-depletion rate so that they both reach 0 at the culmination of the ritual)
            LordJob_PsybreedingRitual.perTickPsyfocus = -1f / BreedingRitual.BreedingRitualSettings.psybreedingDurationTicks;
            if (BreedingRitual.BreedingRitualSettings.neuralHeatPsybreeding > 0f)
            {
                // Basic logic: take the total amount of heat (for the full ritual) and dole it out evenly on every tick
                LordJob_PsybreedingRitual.perTickPsyheat = BreedingRitual.BreedingRitualSettings.neuralHeatPsybreeding / BreedingRitual.BreedingRitualSettings.psybreedingDurationTicks;

                // Special logic: If BOTH partners are psy-talented, then they can share the burden equally
                if (man.HasPsylink && woman.HasPsylink)
                {
                    LordJob_PsybreedingRitual.perTickPsyheat /= 2f;
                }
            }

            // Invoke the base class method to handle the usual stuff (cooldowns, reservations, duty assignments, etc)
            base.TryExecuteOn(target, organizer, ritual, obligation, assignments, playerForced);
        }

        // Check whether the ritual's preconditions are satisfied
        public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
        {
            string failureReason = base.CanStartRitualNow(target, ritual, selectedPawn, forcedForRole);
            if (failureReason != null) { return failureReason; }

            Building_Bed bed = (Building_Bed)target.Thing;
            if (bed.GetAssignedPawns() == null || bed.GetAssignedPawns().Count() < 2)
            {
                // This should never happen... but we'll provide a failure explanation just in case.
                return "MessageBreedingNeedsCouple".Translate().CapitalizeFirst();
            }
            Pawn p1 = bed.GetAssignedPawns().First();
            Pawn p2 = bed.GetAssignedPawns().Last();

            // Ensure that the couple is psybonded
            // In order to accommodate 'harem' scenarios, we'll allow unidirectional psybonds
            Hediff_PsychicBond psybond1 = (Hediff_PsychicBond) p1.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond, false);
            Hediff_PsychicBond psybond2 = (Hediff_PsychicBond) p2.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond, false);
            if ((psybond1 != null) && (Pawn)psybond1.target == p2)
            {
                // Psybond found!
            }
            else if ((psybond2 != null) && (Pawn)psybond2.target == p1)
            {
                // Psybond found!
            }
            else if (BreedingRitual.BreedingRitualSettings.psybondRequired)
            {
                return "MessagePsybreedingNeedsPsybond".Translate().CapitalizeFirst();
            }

            // Attempt to find a psycaster
            if (p1.GetPsylinkLevel() + p2.GetPsylinkLevel() == 0)
            {
                return "MessagePsybreedingNeedsPsycaster".Translate().CapitalizeFirst();
            }

            // Check whether the psycaster has gathered sufficient psyfocus
            if ((p1.GetPsylinkLevel() > 0) && p1.psychicEntropy.CurrentPsyfocus >= BreedingRitual.BreedingRitualSettings.psyfocusMinimum)
            {
                // Pawn 1 meets the requirement
            }
            else if ((p2.GetPsylinkLevel() > 0) && p2.psychicEntropy.CurrentPsyfocus >= BreedingRitual.BreedingRitualSettings.psyfocusMinimum)
            {
                // Pawn 2 meets the requirement
            }
            else
            { 
                return "MessagePsybreedingNeedsPsyfocus".Translate(BreedingRitual.BreedingRitualSettings.psyfocusMinimum.ToStringPercent("0.0").Named("PSYFOCUS")).Resolve().CapitalizeFirst();
            }

            // All checks passed. No errors found.
            return null;    // We're supposed to return a string containing the failure reason. Null means OK.
        }

        protected void CancelBreedingRitual_PsychicHeat(Pawn psycaster, LordJob_Ritual ritual)
        {
            base.CancelBreedingRitual((Building_Bed) ritual.selectedTarget.Thing, "MessagePsybreedingCancelledNeuralHeat".Translate(psycaster.Named("PSYCASTER")).Resolve().CapitalizeFirst(), ritual);
        }

        protected override int? CustomLovinDuration(Pawn man, Pawn woman)
        {
            // This is a psybreeding.
            // Psybreeding consists of ONE extremely long Lovin' action. Therefore we simply overwrite
            // the Lovin' duration so that it matches the length of the ritual (as set in Options)
            return (int)BreedingRitual.BreedingRitualSettings.psybreedingDurationTicks;
        }
    }
}

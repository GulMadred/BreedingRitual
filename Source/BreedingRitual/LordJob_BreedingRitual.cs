using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
    // This class "oversees" a breeding ritual. The tick-by-tick updates
    // are handled by the RitualBehaviorWorker_Breeding class.
    public class LordJob_BreedingRitual : LordJob_Ritual
    {
        // This is a pseudo-constructor. It shouldn't be used by mod code (because variables
        // won't initialize properly). It's invoked automatically by underlying RimWorld code
        // upon loading a savegame. In that case, the game will invoke blank constructors and
        // then rebuild internal state information via the ExposeData method.
        public LordJob_BreedingRitual()
            : base() { }

        // This is the actual constructor, invoked by mod code during normal gameplay
        public LordJob_BreedingRitual(TargetInfo selectedTarget, Precept_Ritual ritual, RitualObligation obligation, List<RitualStage> allStages, RitualRoleAssignments assignments, Pawn organizer = null)
            : base(selectedTarget, ritual, obligation, allStages, assignments, organizer)
        {
            Pawn man = assignments.FirstAssignedPawn("man");
            Pawn woman = assignments.FirstAssignedPawn("woman");

            // It's necessary to record the couple's ID values so that we know whose math to tinker with
            // Other couples might do Lovin' while this ritual is active; the mod must leave THEIR math unaltered
            LordJob_BreedingRitual.manID = man.thingIDNumber;
            LordJob_BreedingRitual.womanID = woman.thingIDNumber;
            // Bed and Room IDs are recorded for similar reasons
            LordJob_BreedingRitual.bedID = (selectedTarget.Thing).thingIDNumber;
            LordJob_BreedingRitual.roomID = (selectedTarget.Thing.GetRoom() == null) ? -1 : selectedTarget.Thing.GetRoom().ID;
            // Reset the Lovin' counter
            LordJob_BreedingRitual.lovinActions = 0;
            // Reset the performance tracker
            LordJob_BreedingRitual.totalFitnessScore = 0f;

            // If the player wants to know the odds of successful conception, then we must calculate and announce them.
            // To be clear: we'll ALWAYS calculate the odds (as a precautionary measure, and to assist debugging) but we'll
            // keep quiet about it ... unless the Player has specifically asked to be informed.
            //
            // Technical note: the calculation involves JobDriver_Lovin.PregnancyChance. That's a private field,
            // whose value is hardcoded at 0.05f. It's possible for other mods to adjust that value, but we can't
            // know about it from this context. Therefore we'll just assume that the value is still 0.05f
            // Therefore the true chance of conception is PregnancyUtility.PregnancyChanceForPartners(...), multiplied by 0.05.
            //
            // TODO: add some Reflection; find the true value at runtime. Grab it once per play-session and then just cache it.
            //
            LordJob_BreedingRitual.cachedFertilityScore = PregnancyUtility.PregnancyChanceForPartners(woman, man) * 0.05f;
            if (BreedingRitual.BreedingRitualSettings.announcePregnancyChance)
            {
                // TODO: we should probably cap this value within 0%...100% range before displaying it.

                Messages.Message( (BreedingRitual.BreedingRitualSettings.overridePregnancyApproachCheat ? "MessageFertilityCHEAT" : 
                    "MessageFertility").Translate(LordJob_BreedingRitual.cachedFertilityScore.ToStringPercent("0.00").Named("FERTILITY")).CapitalizeFirst()
                    , selectedTarget, MessageTypeDefOf.NeutralEvent, true);

                // Provide a supplemental message to explain a 0% result
                if (PregnancyUtility.GetPregnancyHediff(woman) != null)
                {
                    Messages.Message("MessageFertilityPregnancy".Translate(woman.Named("PAWN")).CapitalizeFirst(), woman, MessageTypeDefOf.NeutralEvent, true);
                }
            }

            // Note that we could calculate the Pawn stat factors, but we DO NOT. This is intentional.
            //
            // First: the re-calculation occurs rarely (at the start of a new Lovin' action) so the 
            // waste is negligible. It's not a per-tick recalculation.
            //
            // Second: Pawn stats may degrade over the duration of the ritual (pawns become tired,
            // performance-enhancing drugs wear off, etc). Stat decay SHOULD result in worsening
            // efficiency w/r/t breeding activities. Caching would preserve an inaccurate snapshot
            // of a fresh-and-well-rested pawn. We WANT exhaustion to degrade Lovin' performance.
        }
        
        // This is the work that we'll assign to participants. It's only relevant for spectators
        // (unless someone has modified/extended the RitualDef XML).
        // In the "vanilla mod" version, the man and woman have special work to perform and so 
        // they'll never be given default/spectator responsibilities (e.g. dancing, music).
        protected override LordToil_Ritual MakeToil(RitualStage stage)
        {
            return new LordToil_SpectateDanceMusic(this.spot, this, stage, this.organizer);
        }

        // The RitualDef XML allows us to specify jobReportOverride details whenever a pawn
        // performs a specific role (such as anima linking or gladiatorial combat).
        // This is very helpful in "narrating" a scene for the benefit of a confused player.
        // Unfortunately, the XML will NOT accept a jobReportOverride for spectators.
        // So we'll insert some special code to do it here instead.
        public override string GetJobReport(Pawn pawn)
        {
            if (!this.assignments.SpectatorsForReading.Contains(pawn))
            {
                // This pawn isn't a spectator. No intervention needed.
                return base.GetJobReport(pawn);
            }
            else if (pawn.mindState.duty.def.defName != "SpectateCircle")
            {
                // We're not yet in the final stage. No intervention needed.
                return base.GetJobReport(pawn);
            }
            else
            {
                // This pawn is a spectator in the final stage of the ritual.

                // This stage involves spectators gathering around the tired couple
                // to offer well-wishes, support, constructive criticism, etc.

                // But pawns don't emote and their animation repertoire is limited.
                // When a dozen people suddenly surround the bed ... it looks creepy.

                // Therefore we'll override the job report in order to "explain" the action
                return ((pawn.thingIDNumber % 2) == 0) ? "BreedingConclusion1".Translate().CapitalizeFirst()
                    : "BreedingConclusion2".Translate().CapitalizeFirst();
            }
        }

        // If the user saves the game (and reloads) while a ritual is in-progress, then
        // we'd risk losing some crucial status data. Write it to the savegame XML for safety.
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref LordJob_BreedingRitual.manID, "manID", -1, false);
            Scribe_Values.Look<int>(ref LordJob_BreedingRitual.womanID, "womanID", -1, false);
            Scribe_Values.Look<int>(ref LordJob_BreedingRitual.bedID, "bedID", -1, false);
            Scribe_Values.Look<int>(ref LordJob_BreedingRitual.roomID, "roomID", -1, false);
            Scribe_Values.Look<int>(ref LordJob_BreedingRitual.lovinActions, "lovinActions", -1, false);
            Scribe_Values.Look<float>(ref LordJob_BreedingRitual.cachedFertilityScore, "cachedFertilityScore", -1f, false);
            Scribe_Values.Look<float>(ref LordJob_BreedingRitual.totalFitnessScore, "totalFitnessScore", -1f, false);
            Scribe_References.Look<Lord>(ref LordJob_BreedingRitual.manFormerLord, "manFormerLord", false);
            Scribe_References.Look<Lord>(ref LordJob_BreedingRitual.womanFormerLord, "womanFormerLord", false);
        }

        public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
        {
            base.Notify_PawnLost(p, condition);

            // Was it a spectator or a participant?
            if (this.RoleFor(p) == null)
            {
                // Spectator. We don't care. The ritual will proceed without them.
            }
            else
            {
                // Participant lost. The ritual will be auto-cancelled.

                // Ensure that the couple immediately stops any ongoing Lovin' actions
                RitualBehaviorWorker_Breeding.StopLovin(this);
                // Reset the tracking information
                ResetTrackingVariables();
            }
        }

        public override void Notify_InMentalState(Pawn pawn, MentalStateDef stateDef)
        {
            base.Notify_InMentalState(pawn, stateDef);

            // Was it a spectator or a participant?
            if (this.RoleFor(pawn) == null)
            {
                // Spectator. We don't care. The ritual will proceed without them.
            }
            else
            {
                // Participant lost. The ritual will be auto-cancelled.

                // Ensure that the couple immediately stops any ongoing Lovin' actions
                RitualBehaviorWorker_Breeding.StopLovin(this);
                // Reset the tracking information
                ResetTrackingVariables();
            }
        }

        public override void Notify_BuildingLost(Building b)
        {
            base.Notify_BuildingLost(b);

            // Was it a musical instrument or a bed?
            if (((Building_Bed) b) == null)
            {
                // Instrument. We don't care. The ritual will proceed without it.
                // Note: this also applies to Meditation Spots. We don't care about them.
            }
            else
            {
                // Bed lost. The ritual will be auto-cancelled.

                // Ensure that the couple immediately stops any ongoing Lovin' actions
                RitualBehaviorWorker_Breeding.StopLovin(this);
                // Reset the tracking information (otherwise we might have a dangling mutex).
                ResetTrackingVariables();
            }
        }

        // These ought to be instance variables. I've made them static for now because it's the simplest
        // way to handle save/load operations alongside Postfix manipulation. I'll probably revisit with
        // a cleaner solution in the future.

        // We record the ID values of the participants for the sake of static Postfix methods.
        // These methods will be invoked during the ritual, but they may be invoked for pawns
        // other than our actual participants. Having the ID values means that we can do some
        // quick filtering - thus ensuring that the mod doesn't inflict unexpected side-effects.
        public static int manID;
        public static int womanID;
        // Similarly, some Postfix methods need to discriminate based on the specific BED
        // (which might or might not be involved in the ritual). So we store its ID.
        public static int bedID;
        public static int roomID;
        // We track the number of Lovin' actions for the sake of providing a detailed
        // after-action report at the end of the ritual.
        public static int lovinActions;
        // We record the original value because fertility can CHANGE during the ritual.
        // Most obviously: if the woman becomes pregnant then her fertility falls to 0.
        public static float cachedFertilityScore;
        // We also record any speed/fitness calculation values, adding them together
        // into one big sum. Fitness can change during the ritual (as people become
        // tired) but it's still a useful report of overall performance.
        public static float totalFitnessScore;
        // TODO: Precept_Ritual.tmpActiveRituals may be the key. It's complicated by the potential presence of multiple Ideos, though.
        public static Lord manFormerLord;
        public static Lord womanFormerLord;

        protected override bool RitualFinished(float progress, bool cancelled)
        {
            if (cancelled)
            {
                // Ensure that the couple immediately stops any ongoing Lovin' actions
                RitualBehaviorWorker_Breeding.StopLovin(this);
                // Clear out any status-tracking information (it's obsolete/misleading now)
                ResetTrackingVariables(true);
            }
            return base.RitualFinished(progress, cancelled);
        }

        // Call this method whenever a ritual is completed, cancelled, aborted, failed, etc
        public static void ResetTrackingVariables(bool thorough = true)
        {
            if (thorough)
            {
                LordJob_PsybreedingRitual.ResetTrackingVars(false);
                LordJob_AnimabreedingRitual.ResetTrackingVars(false);
            }
            manID = -1;
            womanID = -1;
            bedID = -1;
            roomID = -1;
            lovinActions = -1;
            cachedFertilityScore = -1f;
            totalFitnessScore = -1f;

            // Note: we don't reset the two formerLord variables because they're unusual.
            // They actually get set BEFORE this object is constructed, and they need
            // to persist after this object is discarded.
        }

        public static bool RitualParticipant(int pawnID)
        {
            return (pawnID == manID) || (pawnID == womanID);
        }
    }
}

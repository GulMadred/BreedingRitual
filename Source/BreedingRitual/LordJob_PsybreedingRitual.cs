using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimWorld
{
    public class LordJob_PsybreedingRitual : LordJob_BreedingRitual
    {
        public LordJob_PsybreedingRitual(TargetInfo selectedTarget, Precept_Ritual ritual, RitualObligation obligation, List<RitualStage> allStages, RitualRoleAssignments assignments, Pawn organizer = null)
            : base(selectedTarget, ritual, obligation, allStages, assignments, organizer)  { }

        public LordJob_PsybreedingRitual()
            : base() { }

        // If the user saves the game (and reloads) while a ritual is in-progress, then
        // we'd risk losing some crucial status data. Write it to the savegame XML for safety.
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref LordJob_PsybreedingRitual.psySensitivityTotal, "psySensitivityTotal", -1f, false);
            Scribe_Values.Look<float>(ref LordJob_PsybreedingRitual.perTickPsyheat, "perTickPsyheat", -1f, false);
            Scribe_Values.Look<float>(ref LordJob_PsybreedingRitual.perTickPsyfocus, "perTickPsyfocus", -1f, false);
        }

        // These ought to be instance variables. I've made them static for now because it's the simplest
        // way to handle save/load operations alongside Postfix manipulation. I'll probably revisit with
        // a cleaner solution in the future.

        // We record the sum of the psy-sensitivity for the breeding pair for efficiency
        // purposes (to avoid repeated lookups on every Tick).
        public static float psySensitivityTotal = -1f;
        // As above. We could re-calculate these values on every Tick but it would be wasteful.
        public static float perTickPsyheat = -1f;
        public static float perTickPsyfocus = -1f;

        // This is the work that we'll assign to participants. It's only relevant for spectators
        // (unless someone has modified/extended the RitualDef XML).
        // In the "vanilla mod" version, the man and woman have special work to perform and so 
        // they'll never be given default/spectator responsibilities (e.g. dancing, music).
        protected override LordToil_Ritual MakeToil(RitualStage stage)
        {
            if (BreedingRitual.BreedingRitualSettings.psybreedingMeditation)
            {
                // The Player wants spectators to meditate
                return new LordToil_SpectateMeditate(this.spot, this, stage, this.organizer);
            }
            // The Player wants spectators to do default stuff (dance/drum/etc)
            return base.MakeToil(stage);
        }

        public static void ResetTrackingVars(bool thorough = true)
        {
            if (thorough)
            {
                LordJob_BreedingRitual.ResetTrackingVariables(false);
            }
            psySensitivityTotal = -1f;
            perTickPsyheat = -1f;
            perTickPsyfocus = -1f;
        }

        public void WarnInsufficientMeditationSpots()
        {
            if (!warningGivenNeedSpots)
            {
                // Warn the player so that they can designate meditation spots.
                Messages.Message("MessagePsybreedingNeedsSpots".Translate().CapitalizeFirst(), MessageTypeDefOf.NeutralEvent, true);
                warningGivenNeedSpots = true;
            }
            // Don't repeat the warning if the problem recurs. Once per ritual is enough.
            warningGivenNeedSpots = true;
        }
        private bool warningGivenNeedSpots = false;

        public void AttemptPsyAwakening()
        {
            Pawn man = assignments.FirstAssignedPawn("man");
            Pawn woman = assignments.FirstAssignedPawn("woman");

            if (PregnancyUtility.GetPregnancyHediff(woman) == null)
            {
                // The psybreeding was unsuccessful. No further action is needed.
                return;
            }

            // Which of the pawns needs to have a psychic awakening?
            Pawn psycaster;
            Pawn awakened;
            if (man.HasPsylink && woman.HasPsylink)
            {
                // Both participants are already psycasters. Psy-awakening is not needed.
                return;
            }
            else if (man.HasPsylink)
            {
                psycaster = man;
                awakened = woman;
            }
            else
            {
                psycaster = woman;
                awakened = man;
            }

            // Do the psy-awakening; award the psylink
            awakened.ChangePsylinkLevel(1, false);      // we omit the standard notification letter because we're going to send a custom one
            awakened.psychicEntropy.OffsetPsyfocusDirectly(-1f); // the ritual is very strenuous, so the new psycaster begins with 0 psyfocus

            // Now we'll assign a special "gratitude" memory which the awakened pawn thinks about the psycaster
            ThoughtDef psyAwakening = DefDatabase<ThoughtDef>.GetNamedSilentFail("PsyAwakening");
            if (psyAwakening != null)
            {
                Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(psyAwakening, this.ritual);
                memory.otherPawn = psycaster;
                awakened.needs.mood.thoughts.memories.TryGainMemory(memory);
            }

            // Send a letter to inform the player about the newly-awakened psycaster
            TaggedString taggedString = "LetterPsybreedingAwakening".Translate(awakened.Named("AWAKENED"), Faction.OfPlayer.def.pawnsPlural.Named("PAWNSPLURAL"));
            Find.LetterStack.ReceiveLetter("LetterPsybreedingAwakeningTitle".Translate(), taggedString, RimWorld.LetterDefOf.RitualOutcomePositive, awakened, null, null, null, null, 0, true);
        }
    }
}

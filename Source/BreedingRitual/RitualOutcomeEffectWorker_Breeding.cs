﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
    public class RitualOutcomeEffectWorker_Breeding : RitualOutcomeEffectWorker_FromQuality
    {
        // The game normally shows a mouseover tooltip for the "Start Ritual" button.
        // This tooltip says that the ritual will provide a mood boost of -5 to +8.
        // That's not true. We could provide an accurate prediction, but I don't actually
        // want that text to be included (it wastes GUI space). Just omit it entirely.
        public override string Description
        {
            get { return null; }
        }

        public override bool SupportsAttachableOutcomeEffect
        {
            get
            {
                return BreedingRitual.BreedingRitualSettings.attachableOutcomes;
            }
        }
        
        public RitualOutcomeEffectWorker_Breeding()
        {
        }

        public RitualOutcomeEffectWorker_Breeding(RitualOutcomeEffectDef def)
            : base(def)
        {
        }

        public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            Pawn man = jobRitual.PawnWithRole("man");
            Pawn woman = jobRitual.PawnWithRole("woman");
            float quality = base.GetQuality(jobRitual, progress);
            RitualOutcomePossibility outcome = this.GetOutcome(quality, jobRitual);

            // We must intervene at this point. There are two sets of Thoughts pertaining to outcomes,
            // depending on whether conception occurs. A Boring ritual is more tolerable _if_ it creates a pregnancy.
            // This can't be handled by the RimWorld defs and code. It will correctly assign Boring, Encouraging, etc.
            // We must "upgrade" the thought to reflect a successful breeding (if such an event has actually occured).
            ThoughtDef thought = outcome.memory;

            if (PregnancyUtility.GetPregnancyHediff(woman) != null)
            {
                // Great! The lady has become pregnant (or the Player cheated by bringing an already-pregnant lady)
                // Let's upgrade our Thought into the improved version
                try
                {
                    // The substitution is simple; just append "Success" to the defName
                    thought = DefDatabase<ThoughtDef>.GetNamed(thought.defName + "Success");
                } 
                catch { }
            }

            // Now we must assign our Thought to the participants (spectators mostly)
            // Ideally, each spectator would gain a single Thought which points towards BOTH the man and woman
            // RimWorld doesn't allow us to do that. Each Thought can have only a single target.
            // We could assign two Thoughts, but that might confuse players ("why does Urist remember this event twice?")
            // Instead we'll just simplify things via sexism. If the ceremony fails, blame the guy. If it succeeds, credit the lady.
            // Of course, the man and woman will each gain a Thought (positive or negative) about each other.
            Pawn thoughtTarget;
            Pawn otherTarget;
            if (outcome.Positive) { 
                thoughtTarget = woman;
                otherTarget = man;
            }
            else 
            { 
                thoughtTarget = man;
                otherTarget = woman;
            }

            // Attach the outcome to the ritual
            string attachableOutcomeReport = string.Empty;
            LookTargets lookTarget = thoughtTarget;
            if (jobRitual.Ritual != null)
            {
                this.ApplyAttachableOutcome(totalPresence, jobRitual, outcome, out attachableOutcomeReport, ref lookTarget);
            }

            // Special intervention: if the ritual had an "organizer" (Leader or Moralizer) who spent their ability
            // cooldown in order for the ritual to begin, then we can treat them as an "honorary" spectator
            // (even if they didn't bother to attend).
            if (BreedingRitual.BreedingRitualSettings.organizerRemembersRitual && jobRitual.Organizer != null && !totalPresence.ContainsKey(jobRitual.Organizer))
            {
                try
                {
                    // Pretend that the organizer "attended" for zero minutes
                    totalPresence[jobRitual.Organizer] = 0;
                }
                catch { }
            }

            // Iterate through the list of spectators, adding the new Thought to each of them
            foreach (Pawn p in totalPresence.Keys)
            {
                if (p.ideo != null && p.ideo.Ideo != jobRitual.Ritual.ideo)
                {
                    // This person was present for the ritual but doesn't believe in the ideoligion.
                    // If they participated then they've already received thoughts for Lovin'.
                    // If they spectated ... then they've mostly just wasted their time.
                    // Since they don't believe in breeding, we mustn't make them feel good/bad
                    // about a good/bad breeding ritual.
                    continue;
                }

                // This person believes in the ideoligion. They'll receive a memory based on ritual quality.
                Thought_Memory memory = base.MakeMemory(p, jobRitual, thought);
                memory.otherPawn = thoughtTarget;
                if (p == thoughtTarget) {
                    // We've reached the Pawn who is the praise/blame target for everyone else
                    // Obviously they won't share this opinion; they'll praise/blame their *partner* instead
                    memory.otherPawn = otherTarget; 
                }
                p.needs.mood.thoughts.memories.TryGainMemory(memory);
            }

            // If the participants are engaged, then the completion of the breeding ritual
            // can be treated as an unofficial marriage ceremony.
            string marriageReport = String.Empty;
            if (BreedingRitual.BreedingRitualSettings.breedingIsMarriage)
            {
                if (man.relations.DirectRelationExists(PawnRelationDefOf.Fiance, woman))
                {
                    man.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, woman);
                    man.relations.AddDirectRelation(PawnRelationDefOf.Spouse, woman);

                    marriageReport = "LetterBreedingMarriage".Translate(man.Named("MAN"), woman.Named("WOMAN")).CapitalizeFirst();
                    // Note: we just mark them as married. They do NOT gain the positive
                    // memories which would occur during the wedding ceremony.
                }
            }


            // Attempt to calculate total conception probability.
            string detailedFertilityReport = String.Empty;
            if (BreedingRitual.BreedingRitualSettings.fertilityReportLetter && LordJob_BreedingRitual.lovinActions > 0 && LordJob_BreedingRitual.cachedFertilityScore >= 0f)
            {
                float failedConceptionChancePerRound = 1f - Mathf.Clamp01(LordJob_BreedingRitual.cachedFertilityScore);
                float failedConceptionChanceTotal = Mathf.Pow(failedConceptionChancePerRound, LordJob_BreedingRitual.lovinActions);
                detailedFertilityReport = "MessageFertilityReport".Translate((1f - failedConceptionChanceTotal).ToStringPercent("0.00").Named("CONCEPTIONTOTAL"),
                    LordJob_BreedingRitual.lovinActions.ToString().Named("LOVINACTIONS"),
                    LordJob_BreedingRitual.cachedFertilityScore.ToStringPercent("0.00").Named("CONCEPTION")
                    ).CapitalizeFirst();
                if (ReportFitness() && LordJob_BreedingRitual.totalFitnessScore > 0f)
                {
                    detailedFertilityReport += "\n\n" +
                        "MessageFitnessReport".Translate((LordJob_BreedingRitual.totalFitnessScore / LordJob_BreedingRitual.lovinActions).ToStringPercent("0").Named("SCORE")).CapitalizeFirst();
                }
            }

            TaggedString taggedString = "LetterFinishedBreeding".Translate(man.Named("MAN"), woman.Named("WOMAN"), jobRitual.Ritual.def.defName.Named("RITUALNAME")).CapitalizeFirst() + " " + ("Letter" + outcome.memory.defName).Translate();
            taggedString += " " + ((PregnancyUtility.GetPregnancyHediff(woman) == null) ? "LetterBreedingFailure" : "LetterBreedingSuccess").Translate(woman.Named("WOMAN"));
            taggedString += "\n\n" + this.OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
            taggedString += "\n\n" + detailedFertilityReport;
            if (SupplementalReport() != null && SupplementalReport().Length > 0)
            {
                taggedString += "\n\n" + SupplementalReport();
            }
            if (marriageReport != null && marriageReport.Length > 0)
            {
                taggedString += "\n\n" + marriageReport;
            }
            if (attachableOutcomeReport != null && attachableOutcomeReport.Length > 0)
            {
                taggedString += "\n\n" + attachableOutcomeReport;
            }
            if (progress < 1f)
            {
                taggedString += "\n\n" + "LetterBreedingInterrupted".Translate();
            }
            Find.LetterStack.ReceiveLetter("OutcomeLetterLabel".Translate(outcome.label.Named("OUTCOMELABEL"), jobRitual.Ritual.Label.Named("RITUALLABEL")), taggedString, outcome.Positive ? LetterDefOf.RitualOutcomePositive : LetterDefOf.RitualOutcomeNegative, lookTarget, null, null, null, null, 0, true);

            // The ritual is complete. Clear out the status-tracking variables
            LordJob_BreedingRitual.ResetTrackingVariables();
        }

        protected virtual string SupplementalReport()
        {
            return null;
        }

        // Psybreeding doesn't use the fitness parameter, so we need a way to disable it in the subclass
        protected virtual bool ReportFitness()
        {
            return true;
        }
    }
}

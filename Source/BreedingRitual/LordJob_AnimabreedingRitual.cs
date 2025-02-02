using BreedingRitual;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
    public class LordJob_AnimabreedingRitual : LordJob_BreedingRitual
    {
        public LordJob_AnimabreedingRitual(TargetInfo selectedTarget, Precept_Ritual ritual, RitualObligation obligation, List<RitualStage> allStages, RitualRoleAssignments assignments, Pawn organizer = null)
            : base(selectedTarget, ritual, obligation, allStages, assignments, organizer)  { }

        public LordJob_AnimabreedingRitual()
            : base() { }

        // If the user saves the game (and reloads) while a ritual is in-progress, then
        // we'd risk losing some crucial status data. Write it to the savegame XML for safety.
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Plant>(ref LordJob_AnimabreedingRitual.animaTree, "animaTree", false);
            Scribe_Values.Look<int>(ref LordJob_AnimabreedingRitual.animaGrassConsumed, "animaGrassConsumed", -1, false);
            Scribe_Values.Look<int>(ref LordJob_AnimabreedingRitual.animaGrassConserved, "animaGrassConserved", -1, false);
        }

        // These ought to be instance variables. I've made them static for now because it's the simplest
        // way to handle save/load operations alongside Postfix manipulation. I'll probably revisit with
        // a cleaner solution in the future.

        // We store a reference to the Anima tree because we'll need to invoke some of its features.
        public static Plant animaTree;
        // When we destroy anima grass, we need to "remember" some details (so that we can report them later)
        public static int animaGrassConsumed;
        public static int animaGrassConserved;

        // This is the work that we'll assign to participants. It's only relevant for spectators
        // (unless someone has modified/extended the RitualDef XML).
        // In the "vanilla mod" version, the man and woman have special work to perform and so 
        // they'll never be given default/spectator responsibilities (e.g. dancing, music).
        protected override LordToil_Ritual MakeToil(RitualStage stage)
        {
            // This ritual is based on Anima-Linking. Spectators should always meditate. Dancing is forbidden.
            return new LordToil_SpectateMeditate(this.spot, this, stage, this.organizer);
        }

        public static void ResetTrackingVars(bool thorough = true)
        {
            if (thorough)
            {
                LordJob_BreedingRitual.ResetTrackingVariables(false);
            }
            animaTree = null;
            animaGrassConsumed = -1;
            animaGrassConserved = -1;
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

        /// <summary>
        /// Helper method to check whether a couple is eligible for anima-breeding
        /// </summary>
        /// <returns>Null if compatible. Otherwise returns a string explaining the problem.</returns>
        public static string CheckCompatibility(Pawn p1, Pawn p2)
        {
            if (p1.gender != p2.gender)
            {
                if (p1.gender == Gender.Male && p2.gender == Gender.Female)
                {
                    // This is a vanilla heterosexual pairing. Those are allowed by default.
                    return null;
                }
                else if (BreedingRitual.BreedingRitualSettings.allowMutualBreeding)
                {
                    // This is a reversed heterosexual pairing, but it's still possible for the woman to conceive.
                    return null;
                }
                else if (BreedingRitual.BreedingRitualSettings.allowMalePregnancy)
                {
                    // This is a reversed heterosexual pairing, but it's possible for the man to conceive.
                    return null;
                }
                return "MessageAnimabreedingMPregForbidden".Translate().Resolve().CapitalizeFirst();
            }
            else if (BreedingRitual.BreedingRitualSettings.childbearerGenesOnly)
            {
                if (p2.gender == Gender.Female)
                {
                    // We're trying to clone a woman. That's allowed by default.
                    return null;
                }
                else if (BreedingRitual.BreedingRitualSettings.allowMalePregnancy)
                {
                    // We're trying to clone a man, and the player has enabled mpreg.
                    return null;
                }
                else if (BreedingRitual.BreedingRitualSettings.allowMutualBreeding && p1.gender == Gender.Female)
                {
                    // We can't clone p2, but we CAN clone p1.
                    return null;
                }
                return "MessageAnimabreedingMPregForbidden".Translate().Resolve().CapitalizeFirst();
            }
            else if (!BreedingRitualSettings.allowHomosexualBreeding)
            {
                return "MessageAnimabreedingHomoForbidden".Translate().Resolve().CapitalizeFirst();
            }
            else if (BreedingRitualSettings.allowHomosexualBreeding)
            {
                if (p2.gender == Gender.Female)
                {
                    // We're trying to breed a woman. That's allowed by default.
                    return null;
                }
                else if (BreedingRitual.BreedingRitualSettings.allowMalePregnancy)
                {
                    // We're trying to breed a man, and the player has enabled it.
                    return null;
                }
                return "MessageAnimabreedingMPregForbidden".Translate().Resolve().CapitalizeFirst();
            }
            return "MessageAnimabreedingCoupleIneligible".Translate().Resolve().CapitalizeFirst();
        }

        public void AttemptBreeding(bool reverseRoles = false)
        {
            // For clarity, we drop the man/woman terminology. Focus instead on childbearing.
            Pawn childbearer = assignments.FirstAssignedPawn("woman");
            Pawn lover = assignments.FirstAssignedPawn("man");

            // Recursive step (to handle bidirectional breeding)
            if (reverseRoles)
            {
                // Swap the roles and proceed through the method body
                childbearer = lover;
                lover = assignments.FirstAssignedPawn("woman");
            }
            else if (BreedingRitual.BreedingRitualSettings.allowMutualBreeding)
            {
                // Recursively invoke this method
                AttemptBreeding(true);

                // Note: after doing the reverse-role method invocation, 
                // we proceed through the method body (to perform a regular breeding)
            }

            if (BreedingRitual.BreedingRitualSettings.childbearerGenesOnly)
            {
                // This is a "cloning" scenario. Treat the childbearer as both mother and father.
                lover = childbearer;
            }

            if (!RitualBehaviorWorker_Breeding.pregnancyAllowedThisTick)
            {
                // The single-preg-check rule prevents us from attempting two impregnations
                // on the same tick. That's troublesome, because the MutualBreeding scenario
                // requires us to perform two impregnations near-simultaneously.
                //
                // We'll apply a crude workaround by resetting the flag.
                RitualBehaviorWorker_Breeding.pregnancyAllowedThisTick = true;
            }

            // Because we're tinkering significatly with the logic, we'll just copy-paste the code and edit it by hand.
            // Transpiling might be feasible, but we need to leave the base RimWorld code intact.
            // The special behavior is supposed to occur ONLY during Anima breeding.

            if (PregnancyUtility.GetPregnancyHediff(childbearer) != null)
            {
                // The intended childbearer is ALREADY pregnant.
                //
                // Creating a second pregnancy isn't really ~~harmful~~ (it gets merged into 
                // the first pregnancy Hediff, increasing the severity by 0.001).
                // But for the sake of simplicity we'll skip doing any redundant work.
            }
            else if (!BreedingRitual.BreedingRitualSettings.allowMalePregnancy && childbearer.gender == Gender.Male)
            {
                // We're not allowed to impregnate men. Skip.
            }
            else if (!BreedingRitual.BreedingRitualSettings.allowHomosexualBreeding && (childbearer.gender == lover.gender) && (childbearer.thingIDNumber != lover.thingIDNumber))
            {
                // We're not allowed to do homosexual breeding. Skip.
            }
            else if (Rand.Chance(0.05f * PregnancyUtility.PregnancyChanceForPartners(childbearer, lover)))
            {
                // Pregnancy RNG roll succeeded.
                bool genesCreated;
                GeneSet inheritedGeneSet = PregnancyUtility.GetInheritedGeneSet(childbearer, lover, out genesCreated);
                if (genesCreated)
                {
                    // The geneset is viable. Implant the pregnancy.
                    Hediff_Pregnant pregnancy = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, childbearer, null);
                    pregnancy.SetParents(null, lover, inheritedGeneSet);
                    // TODO: do we need special handling for cloning? father==mother scenarios?
                    childbearer.health.AddHediff(pregnancy, null, null, null);
                }
                else if (PawnUtility.ShouldSendNotificationAbout(childbearer) || PawnUtility.ShouldSendNotificationAbout(lover))
                {
                    // The geneset is inviable (presumably it's too complex)
                    Messages.Message("MessagePregnancyFailed".Translate(lover.Named("FATHER"), childbearer.Named("MOTHER")) + ": " + "CombinedGenesExceedMetabolismLimits".Translate(), new LookTargets(new TargetInfo[] { lover, childbearer }), MessageTypeDefOf.NegativeEvent, true);
                }
            }
            else
            {
                // Pregnancy RNG roll failed. Do nothing.
            }
        }
    }
}

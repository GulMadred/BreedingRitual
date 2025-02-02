using System.Collections.Generic;
using System.Linq;
using Verse.AI.Group;
using Verse;
using Verse.Sound;
using System;

namespace RimWorld
{
    public class RitualBehaviorWorker_Animabreeding : RitualBehaviorWorker_Breeding
    {
        public RitualBehaviorWorker_Animabreeding()
        {
        }

        public RitualBehaviorWorker_Animabreeding(RitualBehaviorDef def)
            : base(def) { }

        // The tick method allows us to tinker with a ritual in-progress and manipulate pawn behavior
        public override void Tick(LordJob_Ritual ritual)
        {
            // The base-class Tick behavior handles conventional requirements
            // (such as forcing pawns to do Lovin' actions)
            base.Tick(ritual);

            // During animabreeding, we often want to achieve unusual forms of procreation
            // (such as cloning). These things are normally impossible; they won't occur by
            // accident. So we just force them to happen when we want them to happen.
            //
            // But there's a catch. If the participants are CAPABLE of breeding normally
            // (i.e. it's a heterosexual couple) then they might achieve normal reproduction
            // "by accident" while we're trying to do something else.
            //
            // In order to prevent any accidents, we reset the pregnancy-allowed flag.
            // This means that "natural" impregnation becomes impossible for the ritual
            // participants (for the duration of animabreeding).
            //
            // Mod code will manually clear this flag when appropriateB.
            RitualBehaviorWorker_Breeding.pregnancyAllowedThisTick = false;

            // If the ritual's scheduled duration is over (i.e. couple is napping, everyone else has left)
            // then we don't do anything further.
            if (ritual.TicksLeft < 0) { return; }

            Plant animaTree = LordJob_AnimabreedingRitual.animaTree;
            if (animaTree == null || animaTree.Destroyed || !animaTree.Spawned)
            {
                // Tree lost. Abort the ritual.
                CancelBreedingRitual((Building_Bed)ritual.selectedTarget, "MessageAnimabreedingCancelledTreeLost".Translate(), ritual);
                return;
            }
        
            // Progress has reached 100%. Consume anima grass and attempt the breeding.
            if (ritual.TicksLeft == 0)
            {
                // Calculate how much anima grass to spare from destruction ("how much of the cost should we refund?")
                int grassRefund = Math.Min(UnityEngine.Mathf.FloorToInt(BreedingRitual.BreedingRitualSettings.animaGrassRefundMax), ritual.assignments.SpectatorsForReading.Count());

                // Delete the appropriate amount of grass.
                CompSpawnSubplant animaGrass = animaTree.TryGetComp<CompSpawnSubplant>();
                int grassCost = Math.Max(0, UnityEngine.Mathf.FloorToInt(BreedingRitual.BreedingRitualSettings.animaGrassCost) - grassRefund);
                List<Thing> grass = animaGrass.SubplantsForReading.OrderByDescending((Thing p) => p.Position.DistanceTo(animaTree.Position)).ToList<Thing>();
                LordJob_AnimabreedingRitual.animaGrassConsumed = Math.Min(grassCost, grass.Count);
                LordJob_AnimabreedingRitual.animaGrassConserved = Math.Max(0, UnityEngine.Mathf.FloorToInt(BreedingRitual.BreedingRitualSettings.animaGrassCost) - LordJob_AnimabreedingRitual.animaGrassConsumed);
                for (int i = 0; i < LordJob_AnimabreedingRitual.animaGrassConsumed; i++)
                {
                    grass[i].Destroy(DestroyMode.Vanish);
                }
                // We've finished deleting grass. Tell the tree to update itself.
                animaGrass.Cleanup();

                // Attempt to impregnate the pawn(s)
                ((LordJob_AnimabreedingRitual)ritual).AttemptBreeding();

                // Generate a visual effect (to make the sudden grass-disappearance less noticeable)
                if (BreedingRitual.BreedingRitualSettings.animabreedingRipple)
                {
                    Pawn man = ritual.PawnWithRole("man");
                    Pawn woman = ritual.PawnWithRole("woman");
                    FleckMaker.Static(woman.TrueCenter(), woman.Map, FleckDefOf.PsycastAreaEffect, 10f);
                    SoundDefOf.PsycastPsychicPulse.PlayOneShot(woman);
                }
            }
        }

        protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            return new LordJob_AnimabreedingRitual(target, ritual, obligation, this.def.stages, assignments, organizer);
        }

        // This method is invoked when the player begins a ritual. We use it to perform some crucial setup, logging, reporting, etc.
        public override void TryExecuteOn(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments, bool playerForced = false)
        {
            // Normally we'd call the base class method first. Unfortunately it will perform a Fertility calculation
            // and it would potentially report an incorrect value. Therefore we must populate some information first.

            // Store the Anima tree in a static variable so that we can find it easily (avoid redundant+expensive lookups)
            Plant animaTree = RitualObligationTargetWorker_Animabreeding.FindAnimaTree(target.Cell, target.Map);
            if (animaTree == null)
            {
                // Tree missing. This should be impossible, but we'll provide an explanation just-in-case
                AbortAnimabreedingRitual("MessageAnimabreedingCancelledTreeMissing", target);
                return;
            }
            LordJob_AnimabreedingRitual.animaTree = animaTree;

            Pawn p1 = assignments.FirstAssignedPawn("man");
            Pawn p2 = assignments.FirstAssignedPawn("woman");

            // Does the player want to allow hopeless/wasteful anima breeding rituals?
            if (BreedingRitual.BreedingRitualSettings.blockSterileAnimabreeding)
            {
                // Check whether the pawns are sexually compatible (according to player-specified options)
                string sexualIncompatibility = LordJob_AnimabreedingRitual.CheckCompatibility(p1, p2);
                if (sexualIncompatibility != null)
                {
                    AbortAnimabreedingRitual(sexualIncompatibility, target);
                    return;
                }

                // Check fertility
                if ((PregnancyUtility.GetPregnancyHediff(p2) != null && !BreedingRitual.BreedingRitualSettings.allowMutualBreeding) ||
                    (PregnancyUtility.GetPregnancyHediff(p1) != null && PregnancyUtility.GetPregnancyHediff(p2) != null))
                {
                    // Anyone who would be eligible for breeding is ALREADY pregnant. Do not allow the ritual to start.
                    AbortAnimabreedingRitual("MessageAnimabreedingCouplePregnant", target);
                    return;
                }
                else if (BreedingRitual.BreedingRitualSettings.overridePregnancyApproachCheat)
                {
                    // The guaranteed-conception cheat is active. Sterility doesn't matter. Proceed with the ritual.
                }
                else if (BreedingRitual.BreedingRitualSettings.animaFertilityBoost == BreedingRitual.BreedingRitualSettings.AnimaFertilityBoostMax)
                {
                    // The anima-fertility boost is set to max (guaranteed conception). Sterility doesn't matter. Proceed with the ritual.
                }
                else if (PregnancyUtility.PregnancyChanceForPartners(p1, p2) > 0f)
                {
                    // Sterility is important ... but THIS couple is fertile. Proceed with the ritual.
                }
                else if (BreedingRitual.BreedingRitualSettings.childbearerGenesOnly && PregnancyUtility.PregnancyChanceForPartners(p2, p2) > 0f)
                {
                    // p1's fertility is 0, but we don't care. p2 is fertile and we intend to clone them. Proceed with the ritual.
                }
                else if (BreedingRitual.BreedingRitualSettings.childbearerGenesOnly && BreedingRitual.BreedingRitualSettings.allowMutualBreeding && PregnancyUtility.PregnancyChanceForPartners(p1, p1) > 0f)
                {
                    // p2's fertility is 0, but we don't care. p1 is fertile and we intend to clone them. Proceed with the ritual.
                }
                else
                {
                    // The couple is sterile. Do not allow the ritual to start.
                    AbortAnimabreedingRitual("MessageAnimabreedingCoupleInfertile", target);
                    return;
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

            if (!ModsConfig.RoyaltyActive)
            {
                return "MessageRoyaltyDLCRequired".Translate().CapitalizeFirst();
            }

            Building_Bed bed = (Building_Bed)target.Thing;
            if (bed.GetAssignedPawns() == null || bed.GetAssignedPawns().Count() < 2)
            {
                // This should never happen... but we'll provide a failure explanation just in case.
                return "MessageBreedingNeedsCouple".Translate().CapitalizeFirst();
            }
            Pawn p1 = bed.GetAssignedPawns().First();
            Pawn p2 = bed.GetAssignedPawns().Last();

            // Find the nearby Anima tree
            Plant animaTree = RitualObligationTargetWorker_Animabreeding.FindAnimaTree(target.Cell, target.Map);
            if (animaTree == null)
            {
                // Tree missing. This should be impossible, but we'll provide an explanation just-in-case
                return "MessageAnimabreedingCancelledTreeMissing".Translate().Resolve().CapitalizeFirst();
            }
            
            // Check for anima grass
            CompPsylinkable linkableTree = animaTree.TryGetComp<CompPsylinkable>();
            if (linkableTree.CompSubplant.SubplantsForReading.Count < UnityEngine.Mathf.FloorToInt(BreedingRitual.BreedingRitualSettings.animaGrassCost))
            {
                // Insufficient grass
                return "MessageAnimabreedingNeedsGrass".Translate(UnityEngine.Mathf.FloorToInt(BreedingRitual.BreedingRitualSettings.animaGrassCost).Named("QUANTITY")).Resolve().CapitalizeFirst();
            }

            // All checks passed. No errors found.
            return null;    // We're supposed to return a string containing the failure reason. Null means OK.
        }

        // Helper method to reduce copy/paste
        protected void AbortAnimabreedingRitual(string reason, LookTargets blame)
        {
            // Explain the cancellation
            Messages.Message(reason.Translate().CapitalizeFirst(), blame, MessageTypeDefOf.NegativeEvent, true);

            // Update status-tracking variables
            LordJob_AnimabreedingRitual.ResetTrackingVars();
        }

        protected override int? CustomLovinDuration(Pawn man, Pawn woman)
        {
            // Anima breeding consists of ONE extremely long Lovin' action. Therefore we simply overwrite
            // the Lovin' duration so that it matches the length of the ritual (as set in Options)
            return (int)BreedingRitual.BreedingRitualSettings.psybreedingDurationTicks;
        }
    }
}

using BreedingRitual;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using static RimWorld.PsychicRitualRoleDef;

namespace RimWorld
{
    public class RitualBehaviorWorker_Breeding : RitualBehaviorWorker
    {
        // This variable is used for delayed-action purposes. We set it in the 
        // "constructor" if there's an action which needs to be performed later.
        // This action is completed on the first Tick of the ritual.
        private bool abilityCooldownInvoked = false;

        // This variable is used to circumvent an anonymous method (which Harmony
        // can't easily patch). Since we can't block the anon method, we just
        // allow it to run, wait a few ticks, and then partially undo its effects.
        private int waitingTicks;

        // c# doesn't require empty default constructors, but Mono gets annoyed
        // if they aren't included.
        public RitualBehaviorWorker_Breeding()
        {
        }

        public RitualBehaviorWorker_Breeding(RitualBehaviorDef def)
            : base(def) { }

        public override void PostCleanup(LordJob_Ritual ritual)
        {
            Pawn man = ritual.PawnWithRole("man");
            Pawn woman = ritual.PawnWithRole("woman");

            // Finish the Cuddling job (technically "LayDown") so that the couple can resume normal activities
            if (man.jobs.curJob != null && man.jobs.curJob.def.defName == "LayDown")
            {
                man.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
            if (woman.jobs.curJob != null && woman.jobs.curJob.def.defName == "LayDown")
            {
                woman.jobs.EndCurrentJob(JobCondition.Succeeded);
            }

            // If the participants were Guests, try to return them to their visiting parties
            try { ReturnGuest(man, LordJob_BreedingRitual.manFormerLord); } catch { }
            try { ReturnGuest(woman, LordJob_BreedingRitual.womanFormerLord); } catch { }
            // We no longer need to remember Lordship details; clear out the variables
            LordJob_BreedingRitual.manFormerLord = null;
            LordJob_BreedingRitual.womanFormerLord = null;
        }

        // The tick method allows us to tinker with a ritual in-progress and manipulate pawn behavior
        public override void Tick(LordJob_Ritual ritual)
        {
            // Do basic ritual stuff
            base.Tick(ritual);

            // Find the key participants in the breeding ritual. We'll need them for all of the subsequent logic.
            Pawn man = ritual.PawnWithRole("man");
            Pawn woman = ritual.PawnWithRole("woman");

            // Due to a quirk in RimWorld code, the ability cooldowns don't get invoked properly upon ritual start
            // That's actually a somewhat-good thing, because it means that we can perform an early cancellation
            // (e.g. due to a couple being sexually incompatible) without "wasting" an ability charge.
            //
            // But now that we've reached THIS section of code, the ritual is underway. We're at Tick 1 (or later).
            // It's time to "spend" that charge.
            if (this.abilityCooldownInvoked)
            {
                // We've already spent it (ie. this is NOT the first tick). Don't need to do anything.
            }
            else if (ritual.Organizer == null)
            {
                // We don't know WHOSE charge we ought to spend. Can't do anything.
            }
            else if (BreedingRitualSettings.useLeaderCooldowns || BreedingRitualSettings.useMoralistCooldowns)
            {
                // Select the appropriate pawn (leader or moralist)
                AbilityGroupDef abilityGroup;
                Pawn pawn = ritual.Organizer;
                if (BreedingRitualSettings.useLeaderCooldowns)
                {
                    abilityGroup = DefDatabase<AbilityGroupDef>.GetNamed("Leader");
                }
                else
                {
                    abilityGroup = DefDatabase<AbilityGroupDef>.GetNamed("Moralist");
                }

                // Iterate through the pawn's abilities, putting each one on cooldown
                foreach (Ability ability in pawn.abilities.AllAbilitiesForReading)
                {
                    ability.Notify_GroupStartedCooldown(abilityGroup, abilityGroup.cooldownTicks);
                }

                // TODO: create an actual "Bless Ritual" duty and force the pawn to perform it
                // TODO: the ability-usage animation (brandish weapon) ought to suffice

                // Update the status variable to indicate that we've spent the charge
                this.abilityCooldownInvoked = true;
            }
            // Note: the reason WHY ability charges don't get spent automatically is because RimWorld assumes
            // that specialists will be assigned to a matching role name. If the RitualDef says
            // <useCooldownFromAbilityGroupDef>Moralist</useCooldownFromAbilityGroupDef>
            // then RimWorld will search for a pawn in the "Moralist" role. If the moralist is present at the ritual
            // in a different named role (such as "woman") or as a spectator, then their cooldown will *NOT* get invoked.


            // Okay, initialization is complete. From this point on, we're going to assume that we're past the first Tick
            // of the ritual. We might still be in the early stages though.
            // If we've reached the middle stage of the ritual, then we need to force Pawns to start Lovin'


            // The key problem is that Lovin' happens during sleep ... but a pawn participating in a Ritual isn't allowed to sleep idly.
            // We can FORCE them to sleep, but that's a priority action which doesn't allow random/spontaneous Lovin'.
            // Therefore we include a proxy activity in the RitualDef. When our mod code detects that activity, we force the pawn
            // to immediately start Lovin' instead (by injecting a Lovin' job).

            // Fortunately, Lovin' is treated as a reciprocal activity within Rimworld code.
            // If we tell a pawn to do Lovin' then their bed-partner will automatically begin Lovin' as well.

            // Of course, the pawns will eventually finish Lovin' (before the ritual is complete). They will revert to the scheduled
            // duty for this stage of the ritual (KeepLyingDown). We immediately detect this change ... and tell the man to start Lovin' again.

            // Hence, the pawns will stay in bed for the full duration, Lovin' without interruption. This will generate several
            // opportunities for conception to occur. We could potentially end the ritual early if conception succeeds, but that wouldn't
            // fit the tone/intent of the mod. Instead we just make them keep Lovin' for the entire hour.

            if (ritual.CurrentStage.GetDuty(man, null, ritual).defName != "KeepLyingDown")
            {
                // We're in the wrong ritual stage. We should leave the man alone so that he can do his scheduled
                // activities (wandering, dancing, walking to bed, etc).
            }
            else if (man.jobs.curJob != null && man.jobs.curJob.def.defName == "Lovin")
            {
                // The man is already performing the Lovin' action. We don't need to do anything.
            }
            else
            {
                // We're at the critical moment of the ritual. We must replace "KeepLyingDown" with "Lovin'".
                // All other ritual activities (walking, dancing, getting into bed, etc) are ignored; they'll play out unchanged.

                // Note that we ONLY assign a job to the man. RimWorld's reciprocal logic will handle the woman's behavior;
                // she'll automatically begin Lovin' when she finds herself in bed with someone who is currently Lovin' her.

                // However, there's a special catch. Our code is slightly TOO efficient. We keep the man Lovin' continuously.
                // Normally, the woman has a long-duration Lovin action (it lasts 999999 ticks). It never finishes naturally,
                // but it has a conditional check: it will instantly complete when her partner stops Lovin.
                //
                // If this mod is in charge, her partner will NEVER stop Lovin. We force him to become a Lovin' MACHINE.
                //
                // Hence, the man will (correctly) cycle through several Lovin' jobs at appropriate intervals (gaining
                // happy memories and rolling pregnancy RNG chances) throughout the ritual. But the woman will only get
                // to mark ONE Lovin' action as complete - when the ritual finally ends and the couple takes a nap.
                //
                // To avoid this silly outcome, we simply mark the woman's Lovin' job as complete whenever we start a new
                // Lovin' job for the man.
                woman.jobs.EndCurrentJob(JobCondition.Succeeded);

                // We'll now start a Lovin job for the man.
                Building_Bed building_Bed = (Building_Bed)ritual.selectedTarget.Thing;
                Job job = JobMaker.MakeJob(JobDefOf.Lovin, woman, building_Bed);
                man.jobs.StartJob(job, JobCondition.InterruptForced, null, false, true);

                // Update the status-tracking variable. We need to remember how many Lovin' actions were performed
                LordJob_BreedingRitual.lovinActions++;

                // We would also like to adjust the DURATION of the Lovin action. Unfortunately it's too soon to do so.
                // If we made a change now, then it would be overwritten (in 1-2 ticks) by an anonymous method within JDL.
                // Instead we "remind" ourselves to intervene after a few ticks have elapsed.
                ResetWaitingPeriod();
            }

            // If a Lovin action is in-progress, decrement the counter.
            // If that waiting interval is complete, try to set a custom Lovin' duration.
            JobDriver_Lovin jdl = man.jobs.curDriver as JobDriver_Lovin;
            if ((jdl != null) && (waitingTicks-- == 0)) {
                // The time is now ripe. Perform the adjustment.
                int? customLovinTicks = CustomLovinDuration(man, woman);
                if (customLovinTicks != null) 
                {
                    // We have a custom duration. Store it for after-action reporting purposes.
                    LordJob_BreedingRitual.totalFitnessScore += (lovinTicksBaseline / customLovinTicks.Value);
                    // Note: we just keep a running tally of the snapshot values. 104% + 62% + 86% etc. Later, we'll 
                    // divide by LordJob_BreedingRitual.lovinActions in order to find the average fitness.

                    // Write our calculated value into the man's JobDriver_Lovin instance
                    // Because it's a private field, we must use reflection (via Harmony)
                    HarmonyLib.Traverse.Create(jdl).Field("ticksLeft").SetValue((int)customLovinTicks);

                    // Note: we don't need to adjust ticksLeft for the woman's JDL. Her ticksLeft will be ~999999
                    // Her job will never tick down naturally; it will auto-complete when the man finishes Lovin'
                    // So as long as we set HIS ticksLeft value appropriately, the couple will behave correctly.
                    //
                    // There are some weird edge cases (e.g. if man dies then woman will successfully "finish" Lovin)
                    // but they're rare enough that we don't really care.
                }
            }

            // In the final stage of the ritual, there's an option for spectators to huddle close around the bed
            // and to gossip cheerfully. Ritual duties actually BLOCK them from random socialization.
            // We could mod in a bunch of custom JobGiver, JobDriver, Job, and Duty subclasses...
            // but it's MUCH easier to just manually intervene, forcing the pawns to chat with each other
            IEnumerable<Pawn> potentialSpeakers = ritual.assignments.SpectatorsForReading.Where(p => !p.interactions.InteractedTooRecentlyToInteract());
            if (ritual.CurrentStage.defaultDuty.defName == "SpectateCircle" && potentialSpeakers.Count() > 0 && Rand.Chance(0.02f))
            {
                // Choose a spectator who is eligible to speak
                Pawn speaker = potentialSpeakers.RandomElement();

                // TODO: should we speak to the *couple* at this point? RitualDef is written to preclude any social interaction for the couple
                // it would make sense for spectators to offer praise / reassurance, but it makes less sense for the (exhausted) couple to REPLY

                // Choose a random spectator to chat with
                Pawn target = ritual.assignments.SpectatorsForReading.Where(p => p != speaker).FirstOrDefault();

                if ((target != null) && speaker.interactions.CanInteractNowWith(target) && 
                    speaker.interactions.TryInteractWith(target, InteractionDefOf.Chitchat))
                {
                    // We managed to speak to someone. Perform an excited "hop" for emphasis.
                    JitterHandler jitter = HarmonyLib.Traverse.Create(speaker.Drawer).Field("jitterer").GetValue<JitterHandler>();
                    jitter.AddOffset(0.1f, 0f);
                    // Note: we don't need to reset/undo this visual offset. It will decay automatically.
                }
            }

            // For flavor reasons, the ritual ends with the participants taking an extended "nap"
            // They'll be sprawled out in bed, with the spectators approaching to offer congratulations
            // This behavior extends beyond the "scheduled" end of the ritual; everyone is forced to hang around for ~20s extra
            // That's not ideal; it looks silly for all spectators to "break apart" suddenly.
            // Instead, we want them to gradually disperse, sneaking away so that they leave the couple resting peacefully.
            //
            // On each late-ritual Tick, we roll the dice. There's a small chance that we randomly release a Pawn
            // We should see one departure per ~10min of game time (a few seconds of player time)
            if (BreedingRitualSettings.gradualDispersal && ritual.TicksLeft < (ritual.DurationTicks * 0.08f) && Rand.Chance(0.008f))
            {
                // TODO make the RNG details configurable

                Pawn departureCandidate = ritual.assignments.SpectatorsForReading.FirstOrDefault();
                if (departureCandidate != null)
                {
                    try
                    {
                        // Remove this pawn "voluntarily" from the ritual. They'll wander away to resume normal duties

                        // Note: the pawn may have ALREADY been removed by the player (via the Leave Ritual button)
                        // If so, the Notify_PawnLost method call will generate an error. Try to prevent that
                        // by checking the Lord's own attendance list.
                        if (ritual.lord.ownedPawns.Contains(departureCandidate))
                        {
                            ritual.lord.Notify_PawnLost(departureCandidate, PawnLostCondition.LeftVoluntarily, null);
                        }

                        Pawn_JobTracker jobs = departureCandidate.jobs;
                        if (jobs != null)
                        {
                            jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                        }
                        // The spectators list does NOT update automatically. We must manually cull it.
                        ritual.assignments.SpectatorsForReading.Remove(departureCandidate);
                    }
                    catch { }
                    // Note that dismissing spectators does not impose any game-mechanics penalties. Quality evaluation
                    // for the ritual will still report that it reached 100% progress.
                }
            }

            // Reset the per-tick pregnancy opportunity flag
            pregnancyAllowedThisTick = true;
        }

        // Helper method to reduce copy/paste
        protected void CancelBreedingRitual(Building_Bed ritualBed, string cancelReason, LordJob_Ritual ritual)
        {
            // Explain the cancellation
            Messages.Message(cancelReason, ritualBed, MessageTypeDefOf.NegativeEvent, true);

            // Stop any ongoing Lovin' actions (both because we don't want them to linger ...
            // and because we don't want the couple to get "credit" for completing them).
            StopLovin(ritual);

            // Perform the cancellation
            ritual.Cancel();

            // TODO: send the letter?

            // Update status-tracking variables
            LordJob_BreedingRitual.ResetTrackingVariables();
        }

        // Helper method to reduce copy/paste
        protected void AbortBreedingRitual_Sexuality(Pawn refuser, string incompatibleSexualTrait)
        {
            // Explain the cancellation
            Messages.Message("MessageBreedingCancelledIncompatibleSexuality".Translate(refuser.Named("PAWN"), incompatibleSexualTrait.Named("SEXUALTRAIT")).Resolve().CapitalizeFirst(), refuser, MessageTypeDefOf.NegativeEvent, true);

            // Update status-tracking variables
            LordJob_BreedingRitual.ResetTrackingVariables();
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        // This method is invoked when the player begins a ritual. We use it to perform some crucial setup, logging, reporting, etc.
        public override void TryExecuteOn(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments, bool playerForced = false)
        {
            Pawn man = assignments.FirstAssignedPawn("man");
            Pawn woman = assignments.FirstAssignedPawn("woman");
            Building_Bed bed = target.Thing as Building_Bed;

            // It should be impossible to invoke this ritual without a target or participants.
            // If it happens, all that we can do is fail silently - any attempt to proceed would throw exceptions.
            if (target == null || man == null || woman == null) { return; }

            // Does the player care about pawn sexuality?
            if (!BreedingRitual.BreedingRitualSettings.respectPawnSexuality)
            {
                // The player wants us to ignore pawn sexuality. Don't bother checking. Let the ritual proceed.
            }
            else
            {
                // Look for incompatible traits. If we find one, we must abort the ritual preparation.
                TraitDef ace = TraitDef.Named("Asexual");
                TraitDef gay = TraitDef.Named("Gay");
                if (man.story.traits.HasTrait(ace))
                {
                    AbortBreedingRitual_Sexuality(man, ace.defName);
                    return;
                }
                else if (man.story.traits.HasTrait(gay))
                {
                    AbortBreedingRitual_Sexuality(man, gay.defName);
                    return;
                }
                else if (woman.story.traits.HasTrait(ace))
                {
                    AbortBreedingRitual_Sexuality(woman, ace.defName);
                    return;
                }
                else if (woman.story.traits.HasTrait(gay))
                {
                    AbortBreedingRitual_Sexuality(woman, gay.defName);
                    return;
                }

                // No conflicts found. We can proceed with the ritual.
            }

            // If the player has chosen a Guest to participate, apply the "seduction" logic
            // Note: we store this guest's allegiance. We'll try to return them when breeding is complete.
            LordJob_BreedingRitual.manFormerLord = LiberateGuest(man);
            if (LordJob_BreedingRitual.manFormerLord != null)
            {
                // Releasing the guest from their Lord will automatically unclaim their current Bed
                // (this is supposed to release Guest beds, but it will occur even if we've modded
                // things up - the guest WILL lose access to the ritual bed, which blocks the ritual
                // from proceeding).
                //
                // Fortunately, there's an easy fix. Re-claim the ritual bed for this Guest.
                man.ownership.ClaimBedIfNonMedical(bed);
            }
            // Repeat the previous logic, but apply it to the woman
            LordJob_BreedingRitual.womanFormerLord = LiberateGuest(woman);
            if (LordJob_BreedingRitual.womanFormerLord != null)
            {
                woman.ownership.ClaimBedIfNonMedical(bed);
            }
            
            // Should the activation of this ritual "spend" an ability charge?
            if (BreedingRitual.BreedingRitualSettings.useLeaderCooldowns || BreedingRitual.BreedingRitualSettings.useMoralistCooldowns)
            {
                // Yes, it should. Set the status-tracking variable to False.
                abilityCooldownInvoked = false;

                // Note: we COULD spend the cooldown from this context. We don't do so, because we're about to
                // invoke base-class behavior which launches the Ritual properly (creates a LordJob, etc).
                // It's possible for that code to fail unexpectedly in a way that we haven't checked for
                // (e.g. a pawn might be in an unreachable location). Therefore we'll postpone the cooldown-spend
                // operation until the Ritual is actually in-progress. We spend the cooldown on the first actual Tick.
            }
            else
            {
                // No, it doesn't need to. Set the status-tracking variable to True
                // (to indicate that no further action is needed re: cooldowns)
                abilityCooldownInvoked = true;
            }

            // To simplify future lookups, we'll record the Pawn in charge (Leader or Moralist) as the "Organizer" of this ritual
            // If neither is responsible, then we'll leave it NULL
            if (BreedingRitualSettings.useLeaderCooldowns)
            {
                organizer = BreedingRitualMod.FindSpecialist(target.Map, (Precept_RoleSingle)ritual.ideo.RolesListForReading.First(r => r.def.defName == "IdeoRole_Moralist"));
            }
            else if (BreedingRitualSettings.useMoralistCooldowns)
            {
                organizer = BreedingRitualMod.FindSpecialist(target.Map, (Precept_RoleSingle)ritual.ideo.RolesListForReading.First(r => r.def.defName == "IdeoRole_Leader"));
            }

            // We've performed the necessary checks and preparations. Attempt to start the ritual.
            base.TryExecuteOn(target, organizer, ritual, obligation, assignments, playerForced);
            // Note: this is a VOID call. We don't know whether it succeeds or fails.
            // Assuming that the ritual actually begins, the next step (for mod code) is
            // the LordJob_BreedingRitual constructor.
        }

        // We don't need to do anything here. We're overriding it mainly because the game's implementation foolishly DISCARDS one of the input parameters.
        protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        {
            // The base class inexplicably omits the Organizer param. We're passing it through to the constructor call.
            return new LordJob_BreedingRitual(target, ritual, obligation, this.def.stages, assignments, organizer);
        }

        // Check whether the ritual's preconditions are satisfied. Many of these checks are already 
        // performed elsewhere by other chunks of code, but we'll be slightly repetitive for safety reasons.
        // If this method provides a reason to refuse the ritual, then the "Start Ritual" button in
        // the RimWorld GUI will become disabled. A mouseover tooltip will display the reason for refusal.
        public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
        {
            // Mutex. We combine breeding and psybreeding into a common category; concurrent instances are not permitted.
            int numBreedingRituals = Find.IdeoManager.GetActiveRituals(target.Map).Where(r => (r.Ritual.def.defName == "Breeding" || r.Ritual.def.defName == "Psybreeding")).Count();
            if (numBreedingRituals > 0)
            {
                return "CantStartRitualAlreadyInProgress".Translate(ritual.Label).CapitalizeFirst();
            }

            if (target == null)
            {
                // This should never happen... but we'll provide a failure explanation just in case.
                return "MessageBreedingNeedsBed".Translate().CapitalizeFirst();
            }

            Building_Bed bed = (Building_Bed)target.Thing;
            if (bed == null)
            {
                // This should never happen... but we'll provide a failure explanation just in case.
                return "MessageBreedingNeedsBed".Translate().CapitalizeFirst();
            }

            // For the sake of Player convenience, we'll be more-thorough-than-necessary at this point
            // It's possible that the bed may be assigned to pawns of different Ideoligions. Instead of just
            // grabbing the FIRST pawn, we'll preferentially grab a colonist (rather than a guest or prisoner).
            // If the Player has assigned two colonists of different ideoligions to share a bed then 
            // we can't help them; it's basically a coinflip.
            Pawn owner;
            if (bed.GetAssignedPawns() == null || bed.GetAssignedPawns().Count() < 2)
            {
                // This should never happen... but we'll provide a failure explanation just in case.
                return "MessageBreedingNeedsCouple".Translate().CapitalizeFirst();
            }
            owner = bed.GetAssignedPawns().FirstOrDefault(p => p.IsColonist);
            if (owner == null) {
                // Fallback option. Couldn't find a colonist; just designate somebody as owner so we can proceed.
                owner = bed.GetAssignedPawn();
            }
            if (owner.ideo == null)
            {
                return "MessageBreedingNeedsIdeo".Translate().CapitalizeFirst();
                // TODO: maybe provide an option here. If participants lack Ideo, then we could fallback to the local dominant Ideo instead of refusing
            }

            // Lookup the owner's ideoligion - and the specialist roles which serve that ideoligion
            Ideo ownerIdeo = owner.ideo.Ideo;
            Precept_RoleSingle moralist = (Precept_RoleSingle) ownerIdeo.RolesListForReading.First(r => r.def.defName == "IdeoRole_Moralist");
            Precept_RoleSingle leader = (Precept_RoleSingle) ownerIdeo.RolesListForReading.First(r => r.def.defName == "IdeoRole_Leader");
            if (BreedingRitualSettings.useMoralistCooldowns && moralist == null)
            {
                return "MessageBreedingNeedsMoralist".Translate(ownerIdeo.name.Named("IDEO")).Resolve().CapitalizeFirst();
            }
            else if (BreedingRitualSettings.useMoralistCooldowns && BreedingRitualMod.FindSpecialist(target.Map, moralist) == null)
            {
                return "MessageBreedingNeedsMoralistMssing".Translate(moralist.Label.Named("MORALISTTITLE")).Resolve().CapitalizeFirst();
            }
            if (BreedingRitualSettings.useLeaderCooldowns && leader == null)
            {
                return "MessageBreedingNeedsLeader".Translate(ownerIdeo.name.Named("IDEO")).Resolve().CapitalizeFirst();
            }
            else if (BreedingRitualSettings.useLeaderCooldowns && BreedingRitualMod.FindSpecialist(target.Map, leader) == null)
            {
                string leaderTitle = ownerIdeo.leaderTitleMale;
                if (!ownerIdeo.leaderTitleFemale.Equals(leaderTitle))
                {
                    leaderTitle += " or " + ownerIdeo.leaderTitleFemale;
                }
                return "MessageBreedingNeedsLeaderMissing".Translate(leaderTitle.Named("LEADERTITLE")).Resolve().CapitalizeFirst();
            }

            // All checks passed. No errors found.
            return null;    // We're supposed to return a string containing the failure reason. Null means OK.
        }

        // This method is invoked by the Postfix in JobDriver_Lovin. It informs our Tick method that there's
        // work which must be done. It also "reminds" our Tick method to wait 3 ticks before reacting.
        private const int waitingPeriod = 3;
        private void ResetWaitingPeriod()
        {
            waitingTicks = waitingPeriod;
        }

        // This is the "work which must be done" mention above. We can calculate a custom duration for
        // Lovin' actions, bypassing the (absurdly random) behavior in the vanilla game.
        // Note: the Psybreeding subclass overrides this method with a (much simpler) custom duration.
        protected virtual int? CustomLovinDuration(Pawn man, Pawn woman)
        {
            if (!BreedingRitual.BreedingRitualSettings.recalculateLovinDuration)
            {
                // The duration-override option is OFF. No need to intervene.
                return null;
            }

            // We need to adjust the Lovin' duration based on the stats of our ritual pawns.

            // We'll need to discard the original result, because it's VERY random (250 ... 2750 ticks).
            // If we multiply that result by a carefully-calculated statistical value then
            // our output would still be ... very random. We could do some clever normalization
            // math - but it's much easier to just generate a new random number. We use the 
            // same EV, so the fundamental balance/difficulty ought to be unchanged.

            // The formula for duration is:
            // RandomFactor * (1500 ticks) / ((Consciousness + Moving + Manipulation + Pain) / 6)
            // The four stat factors are summed across both pawns. Normal pawns have 100% across the board
            // except for Pain, which should be zero.
            // Thus, it becomes:             ((1.0 + 1.0 + 1.0 + 0.0) * 2 pawns) / 6)
            //                               (6.0 / 6)
            //                               1.0
            // For normal pawns it cancels out. But if pawns are injured/infirm then they'll
            // need MORE ticks to complete Lovin'. Augmented/drugged pawns can complete Lovin'
            // actions in FEWER ticks, and thus they get more RNG rolls for pregnancy over the 
            // course of a breeding ritual.

            float randFactor = Rand.Range(0.75f, 1.25f);
            float statFactorSum = man.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) +
                man.health.capacities.GetLevel(PawnCapacityDefOf.Moving) +
                man.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation) +
                man.health.hediffSet.PainTotal +
                woman.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) +
                woman.health.capacities.GetLevel(PawnCapacityDefOf.Moving) +
                woman.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation) +
                woman.health.hediffSet.PainTotal;
            int duration = (int)(lovinTicksBaseline * randFactor / statFactorSum * 6f);
            return duration;
        }

        public static void StopLovin(LordJob_Ritual ritual)
        {
            // If the ritual ends prematurely (due to cancellation by the player, mental break, etc)
            // the we need to cleanup the ongoing tasks. Most of them will end automatically, but Lovin'
            // is a special case. Lovin' jobs can have a very long duration (Psybreeding can last hours)
            // so it's prudent to forcibly end any Lovin' jobs and tidy up the JobDriver.
            //
            // Doing so also ensures that participants don't get "credit" (in the form of a mood boost,
            // social boost, pregnancy RNG roll, etc) for the incomplete job.
            Pawn man = ritual.assignments.FirstAssignedPawn("man");
            if (man != null && man.jobs.curJob.def.defName == "Lovin")
            {
                man.jobs.ClearDriver();
                man.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
            Pawn woman = ritual.assignments.FirstAssignedPawn("woman");
            if (woman != null && woman.jobs.curJob.def.defName == "Lovin")
            {
                woman.jobs.ClearDriver();
                woman.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }

            // Note that there are a few circumstances in which this intervention fails.
            // A pawn dying mid-ritual (or suffering mental break) usually allows their
            // partner to get credit for completing the job. This is very rare so it's
            // not a priority to fix it.
        }

        private Lord LiberateGuest(Pawn p)
        {
            // TODO: integrate this more thoroughly with Hospitality. Maybe incur a relations penalty for seducing their citizen
            //  (especially if the guest's society follows strict precepts re: sexuality)

            Lord formerLord = p.GetLord();
            if (formerLord != null && formerLord.LordJob.ToString().ToLower().Contains("hospitality"))
            {
                formerLord.Notify_PawnLost(p, PawnLostCondition.LeftVoluntarily, null);
                return formerLord;
            }
            return null;
        }

        public bool ReturnGuest(Pawn p, Lord formerLord = null)
        {
            if (formerLord == null)
            {
                // We don't know who this pawn should belong to
                // (or maybe they never belonged to anyone in the first place)
                // 
                // Do nothing
                return false;
            }
            else
            {
                formerLord.AddPawn(p);
            }
            return false;
        }


        // Standard duration of a Lovin' action. Derived from the vanilla RimWorld
        // code (range 250...2750; expected value = 1500).
        private const float lovinTicksBaseline = 1500f;

        // This is a simple on/off flag intended to be used by postfix patches
        // The value will be set to FALSE whenever a pregnancy check is made
        // The value will be set to TRUE by this class' Tick method
        // If multiple checks are made on the same tick, the latter ones will fail
        public static bool pregnancyAllowedThisTick = true;
    }
}

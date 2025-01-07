using KTrie;
using System;
using Unity.Jobs;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
    public class JobGiver_MeditateAtTarget : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            // Lookup the assigned duty (inside the pawn's brain)
            PawnDuty duty = pawn.mindState.duty;
            try
            {
                // Resolve the components/details of the duty
                MeditationSpotAndFocus meditationSpot;
                meditationSpot.spot = duty.focus;
                meditationSpot.focus = duty.focusThird;
                if (!MeditationUtility.IsValidMeditationBuildingForPawn((Building) meditationSpot.spot, pawn))
                {
                    return null;
                }

                // Set the basic job parameters
                JobDef jobDef = JobDefOf.Meditate;
                Job job = JobMaker.MakeJob(jobDef, meditationSpot.spot, null, meditationSpot.focus);
                job.ignoreJoyTimeAssignment = true;

                // Set time parameters
                LordJob_Ritual lordJob_Ritual = pawn.GetLord().LordJob as LordJob_Ritual;
                job.doUntilGatheringEnded = true;
                if (lordJob_Ritual != null)
                {
                    job.expiryInterval = lordJob_Ritual.DurationTicks;
                }
                else
                {
                    job.expiryInterval = 2000;
                }

                return job;
            }
            catch
            {
                // Something went wrong; unable to generate a job assignment
                return null;
            }
        }
    }
}

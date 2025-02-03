using System.Collections.Generic;
using Verse;

namespace RimWorld
{
    public class RitualOutcomeEffectWorker_Psybreeding : RitualOutcomeEffectWorker_Breeding
    {
        public RitualOutcomeEffectWorker_Psybreeding()
        {
        }

        public RitualOutcomeEffectWorker_Psybreeding(RitualOutcomeEffectDef def)
            : base(def)
        {
        }

        public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            // The ritual is complete. Awaken psy powers (if appropriate).
            ((LordJob_PsybreedingRitual)jobRitual).AttemptPsyAwakening();

            // Do the normal post-breeding stuff (cleanup, send a letter, etc)
            base.Apply(progress, totalPresence, jobRitual);
        }

        // Psybreeding doesn't rely on fitness; couples will always do exactly ONE round of Lovin'.
        // Hence, it would be confusing/misleading to tell the player about their "poor" performance
        protected override bool ReportFitness()
        {
            return false;
        }
    }
}

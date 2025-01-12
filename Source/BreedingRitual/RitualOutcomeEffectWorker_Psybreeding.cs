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

        // Psybreeding doesn't rely on fitness; couples will always do exactly ONE round of Lovin'.
        // Hence, it would be confusing/misleading to tell the player about their "poor" performance
        protected override bool ReportFitness()
        {
            return false;
        }
    }
}

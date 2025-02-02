using Verse;

namespace RimWorld
{
    public class RitualOutcomeEffectWorker_Animabreeding : RitualOutcomeEffectWorker_Breeding
    {
        public RitualOutcomeEffectWorker_Animabreeding()
        {
        }

        public RitualOutcomeEffectWorker_Animabreeding(RitualOutcomeEffectDef def)
            : base(def)
        {
        }

        // Animabreeding doesn't rely on fitness; couples will always do exactly ONE round of Lovin'.
        // Hence, it would be confusing/misleading to tell the player about their "poor" performance
        protected override bool ReportFitness()
        {
            return false;
        }

        protected override string SupplementalReport()
        {
            return "MessageAnimabreedingReport".Translate(LordJob_AnimabreedingRitual.animaGrassConsumed.Named("COST"), LordJob_AnimabreedingRitual.animaGrassConserved.Named("REFUND")).CapitalizeFirst();
        }
    }
}

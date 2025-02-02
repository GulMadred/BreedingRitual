using System;
using Verse;

namespace RimWorld
{
    /// <summary>
    /// This class defines a measure of athletic fitness, for use during Lovin' actions.
    /// It is NOT a measure of REPRODUCTIVE fitness; that's a different concept entirely.
    /// We're trying to estimate how effectively the pawn could perform bedroom activities:
    /// would they need more or less time than average? Within a fixed timeframe, could they
    /// complete more or fewer rounds of Lovin' than the average pawn?
    /// 
    /// Even a completely sterile pawn can have a high fitness value.
    /// </summary>
    public class RitualOutcomeComp_Fitness : RitualOutcomeComp_QualitySingleOffset
    {
        public override float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            Pawn pawn = ritual.PawnWithRole(this.roleId);
            return this.curve.Evaluate(FitnessValue(pawn));
        }

        // The four stat factors are summed to calculate an overall fitness value.
        // Pain is expected to be zero, so a normal pawn would have:
        //      (1.0 + 1.0 + 1.0 - 0.0) / 3.0
        //      (3.0 / 3.0)
        //      1.0
        // For normal pawns it yields 100%. If pawns are injured/infirm then they'll
        // see reduced fitness. Augmented/drugged pawns will get a bonus to fitness.
        // Under standard RimWorld rules, pain directly degrades both Moving and Manipulation.
        // Therefore even a moderate amount of pain can significantly reduce fitness.
        // The expectation is that the player will wait for pawns to heal before asking
        // them to perform any breeding ceremonies.
        public static float FitnessValue(Pawn pawn)
        {
            return (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) +
                pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) +
                pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation) -
                pawn.health.hediffSet.PainTotal) / 3f;
        }

        public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
        {
            // This method gets invoked by the Ritual Planning window
            // In that context we don't yet HAVE a ritual, so we must handle null
            if (ritual == null)
            {
                return this.labelAbstract;
            }

            // Lookup the assigned pawn
            Pawn pawn = ritual.PawnWithRole(this.roleId);
            if (pawn == null)
            {
                // Not found. If this is in the planning context, the role may be blank.
                // If it's in a ritual context ... maybe the pawn has become lost/dead?
                return null;
            }

            float fitnessBonus = 0f;
            if (this.curve == null)
            {
                // Unable to evaluate. Retain the 0% default value (neither bonus nor malus).
            }
            else
            {
                // Apply the curve
                fitnessBonus = this.curve.Evaluate(FitnessValue(pawn));
            }
            return this.LabelForDesc.Formatted(pawn.Named("PAWN")) + ": " + 
                "OutcomeBonusDesc_QualitySingleOffset".Translate(((fitnessBonus < 0f) ? "" : "+") + fitnessBonus.ToStringPercent()) + ".";
        }

        public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
        {
            float fitnessBonus = 0f;

            // Lookup the pawn
            Pawn pawn = assignments.FirstAssignedPawn(this.roleId);
            if (pawn == null)
            {
                // Not found. Perhaps he's lost/dead?
                return null;
            }

            // We must calculate the ritual quality bonus (such as 3%)
            // from the raw fitness value (such as 105%)

            if (this.curve == null)
            {
                // Unable to evaluate. Retain the 0% default value (neither bonus nor malus).
            }
            else
            {
                // Apply the curve
                fitnessBonus = this.curve.Evaluate(FitnessValue(pawn));
            }

            return new QualityFactor
            {
                label = this.label.Formatted(pawn.Named("PAWN")),
                count = FitnessValue(pawn).ToStringPercent(),
                qualityChange = ((Math.Abs(fitnessBonus) > float.Epsilon) ? "OutcomeBonusDesc_QualitySingleOffset".Translate(fitnessBonus.ToStringWithSign("0.#%")).Resolve() : " - "),
                positive = (fitnessBonus >= 0f),
                quality = fitnessBonus,
                priority = 0f
                // TODO: we could provide a tooltip here as well
            };
        }

        public override bool Applies(LordJob_Ritual ritual)
        {
            return true;
        }

        [NoTranslate]
        public string roleId;
    }
}

using Verse;

namespace RimWorld
{
    // This is a new ritual quality factor, akin to "Room Impressiveness" or "Participant Count"
    // It simply evaluates whether the woman who participated in the ritual is now pregnant
    // Obviously this could be cheesed if players choose an already-pregnant woman,
    // but we assume that code elsewhere in the mod will guard against such misbehavior.
    public class RitualOutcomeComp_Pregnancy : RitualOutcomeComp_Quality
    {
        public RitualOutcomeComp_Pregnancy() : base() { }

        // Unused by this mod. Included because the compiler gets annoyed if we don't override this function.
        protected bool Counts(RitualRoleAssignments assignments, Pawn p)
        {
            // Just in case it actually gets invoked somewhere, let's define a proper inclusion criterion.
            return (p == assignments.FirstAssignedPawn("woman"));
        }

        // Verbose description (for letters mostly)
        public override string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
        {
            Pawn woman = ritual.PawnWithRole("woman");
            if (woman != null && PregnancyUtility.GetPregnancyHediff(woman) != null) {
                return "MessageBreedingSuccess".Translate(this.curve.Evaluate(1f).ToStringPercent().Named("QUALITY")).Resolve();
            }
            return "MessageBreedingFailure".Translate(this.curve.Evaluate(0f).ToStringPercent().Named("QUALITY")).Resolve();
        }

        public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
        {
            // The outcome is boolean (pregnant VS not pregnant) but we need to express it numerically
            // (for scoreboard reporting purposes) and then map it into a float (using the curve function
            // included in the RitualDef).
            Pawn woman = assignments.FirstAssignedPawn("woman");
            int pregnancies = 0;
            if (woman != null && PregnancyUtility.GetPregnancyHediff(woman) != null)
            {
                pregnancies = 1;
            }
            // Rimworld's Biotech DLC doesn't actually support multi-conception... but mods might.
            // The integer approach is a bit of future-proofing in case we eventually support them.
            // Ideally, we would provide a higher Quality score for the Ritual if it yielded twins.

            // Curve is defined in the RitualDef; modders and players may tinker with it.
            // We just need to invoke it.
            float qualityScore = this.curve.Evaluate((float)pregnancies);

            // This is the "scoreboard" output. IIRC it's used to fill the Ritual Planning popup window
            return new QualityFactor
            {
                label = "Successful breeding: ",
                present = (pregnancies > 0),
                count = pregnancies.ToString() + " / 1",
                qualityChange = this.ExpectedOffsetDesc(true, qualityScore),
                quality = qualityScore,
                positive = true,
                priority = 5f,
                toolTip = qualityScore.ToStringPercent() + " / " + this.curve.Evaluate(1f).ToStringPercent()
            };
        }

        // This is an internally-invoked method. The call hierarchy is very messy.
        // We just need to count the number of successes (ie. the number of pregnancies).
        public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            Pawn woman = ritual.PawnWithRole("woman");
            if (woman != null && PregnancyUtility.GetPregnancyHediff(woman) != null)
            {
                // The woman is pregnant.
                return 1f;
            }
            // The woman is missing or she's not pregnant.
            return 0f;
        }
    }
}

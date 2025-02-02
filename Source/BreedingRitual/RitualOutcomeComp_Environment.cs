using System;
using Verse;
using Verse.Noise;

namespace RimWorld
{
    public class RitualOutcomeComp_Environment : RitualOutcomeComp_Quality
    {
        public override bool DataRequired
        {
            get
            {
                return false;
            }
        }

        private float GetBeautyOrImpressiveness(IntVec3 position, Map map)
        {
            // Assume that we're assessing Impressiveness
            this.label = "BreedingRitual.RoomImpressiveness".Translate();

            // Check whether this is an indoor or outdoor spot
            Room room = position.GetRoom(map);
            if ((room == null) || room.PsychologicallyOutdoors)
            {
                // Outdoor spot
                if (BreedingRitual.BreedingRitualSettings.evaluateBeautyOutdoors)
                {
                    // Show that we're assessing Beauty instead of Impressiveness
                    this.label = "BreedingRitual.SurroundingBeauty".Translate();

                    // Note: we apply a 10x multiplier on Beauty to make it roughly comparable with Impressiveness
                    return 10f * BeautyUtility.AverageBeautyPerceptible(position, map);
                }
                // Fallback on the base-class behavior; evaluate Room Impressiveness
                return room.GetStat(RoomStatDefOf.Impressiveness);
            }
            else
            {
                // Indoor spot
                if (BreedingRitual.BreedingRitualSettings.evaluateBeautyIndoors)
                {
                    // Show that we're assessing Beauty instead of Impressiveness
                    this.label = "BreedingRitual.SurroundingBeauty".Translate();

                    // Note: we apply a 10x multiplier on Beauty to make it roughly comparable with Impressiveness
                    return 10f * BeautyUtility.AverageBeautyPerceptible(position, map);
                }
                // Fallback on the base-class behavior; evaluate Room Impressiveness
                return room.GetStat(RoomStatDefOf.Impressiveness);

                //return this.curve.Evaluate(room.GetStat(RoomStatDefOf.Impressiveness));
            }
        }

        public override float Count(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
        {
            return GenMath.RoundTo(GetBeautyOrImpressiveness(ritual.Spot, ritual.Map), 0.1f);
        }

        public override QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
        {
            // Calculate the score and its impact on ritual quality
            float score = GetBeautyOrImpressiveness(ritualTarget.Cell, ritualTarget.Map);
            float qualityOffset = 0f;
            if (this.curve != null)
            {
                qualityOffset = this.curve.Evaluate(score);
            }

            // Create the report
            return new QualityFactor
            {
                label = this.label.CapitalizeFirst(),
                count = score.ToString("0.0") + " / " + base.MaxValue,
                qualityChange = this.ExpectedOffsetDesc(true, qualityOffset),
                quality = qualityOffset,
                positive = (qualityOffset > 0f),
                priority = 0f
            };
        }
    }
}


using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace BreedingRitual
{
    public class BreedingRitualSettings : ModSettings
    {
        public static float durationTicks;
        public static bool singlePregnancyCheck;
        public static bool useLeaderCooldowns;
        public static bool useMoralistCooldowns;
        public static bool repetitionPenalty;
        public static bool respectPawnSexuality;
        public static bool overridePregnancyApproach;
        public static bool overridePregnancyApproachCheat;
        public static bool recalculateLovinDuration;
        public static bool manCarriesWoman;
        public static bool womanCarriesMan;
        public static bool announcePregnancyChance;
        public static bool fertilityReportLetter;
        public static bool allowPregnantWomen;
        public static bool organizerRemembersRitual;
        public static bool gradualDispersal;
        public static bool playInstruments;
        public static bool playIdeoMusic;
        public static bool showIdeoVFX;
        public static bool finalGathering;
        public static float psyfocusMinimum;
        public static bool psybondRequired;
        public static bool psyboostFertility;
        public static bool psybreedingMeditation;
        public static bool psybreedingRipple;
        public static float psybreedingDurationTicks;
        public static float neuralHeatPsybreeding;
        public static bool bedUsageTolerance;
        public static bool seduceGuests;
        public static bool autoInviteSpectators;
        public static bool neverAllowSpectators;
        public static bool attachableOutcomes;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref durationTicks, "durationTicks", 2500f);
            Scribe_Values.Look(ref singlePregnancyCheck, "singlePregnancyCheck", true);
            Scribe_Values.Look(ref useLeaderCooldowns, "useLeaderCooldowns", false);
            Scribe_Values.Look(ref useMoralistCooldowns, "useMoralistCooldowns", false);
            Scribe_Values.Look(ref repetitionPenalty, "repetitionPenalty", false);
            Scribe_Values.Look(ref respectPawnSexuality, "respectPawnSexuality", false);
            Scribe_Values.Look(ref overridePregnancyApproach, "overridePregnancyApproach", true);
            Scribe_Values.Look(ref overridePregnancyApproachCheat, "overridePregnancyApproachCheat", false);
            Scribe_Values.Look(ref recalculateLovinDuration, "recalculateLovinDuration", false);
            Scribe_Values.Look(ref manCarriesWoman, "manCarriesWoman", true);
            Scribe_Values.Look(ref womanCarriesMan, "womanCarriesMan", false);
            Scribe_Values.Look(ref announcePregnancyChance, "announcePregnancyChance", false);
            Scribe_Values.Look(ref fertilityReportLetter, "fertilityReportLetter", false);
            Scribe_Values.Look(ref allowPregnantWomen, "allowPregnantWomen", false);
            Scribe_Values.Look(ref organizerRemembersRitual, "organizerRemembersRitual", false);
            Scribe_Values.Look(ref gradualDispersal, "gradualDispersal", true);
            Scribe_Values.Look(ref playInstruments, "playInstruments", true);
            Scribe_Values.Look(ref playIdeoMusic, "playIdeoMusic", false);
            Scribe_Values.Look(ref showIdeoVFX, "showIdeoVFX", true);
            Scribe_Values.Look(ref finalGathering, "finalGathering", true);
            Scribe_Values.Look(ref psyfocusMinimum, "psyfocusMinimum", 0.95f);
            Scribe_Values.Look(ref psybreedingDurationTicks, "psybreedingDurationTicks", 10000f);
            Scribe_Values.Look(ref psybondRequired, "psybondRequired", true);
            Scribe_Values.Look(ref psyboostFertility, "psyboostFertility", true);
            Scribe_Values.Look(ref psybreedingMeditation, "psybreedingMeditation", false);
            Scribe_Values.Look(ref psybreedingRipple, "psybreedingRipple", true);
            Scribe_Values.Look(ref neuralHeatPsybreeding, "neuralHeatPsybreeding", 75f);
            Scribe_Values.Look(ref bedUsageTolerance, "bedUsageTolerance", true);
            Scribe_Values.Look(ref seduceGuests, "seduceGuests", false);
            Scribe_Values.Look(ref autoInviteSpectators, "autoInviteSpectators", true);
            Scribe_Values.Look(ref neverAllowSpectators, "neverAllowSpectators", false);
            Scribe_Values.Look(ref attachableOutcomes, "attachableOutcomes", false);
        }
    }

    public class BreedingRitualMod : Mod
    {
        public BreedingRitualSettings settings;
        public BreedingRitualMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<BreedingRitualSettings>();
        }

        private static Vector2 scrollPosition = new Vector2(0f, 0f);
        private static float totalContentHeight = 3225f;
        private const float ScrollBarWidthMargin = 18f;

        public override void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            Rect outerRect = rect.ContractedBy(5f);
            bool scrollBarVisible = true;
            var scrollViewTotal = new Rect(0f, 0f, outerRect.width - (scrollBarVisible ? ScrollBarWidthMargin : 0), totalContentHeight);
            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollViewTotal);
            listingStandard.Begin(new Rect(0f, 0f, scrollViewTotal.width, 9999f));

            listingStandard.GapLine();
            listingStandard.SubLabel("BreedingRitual.OverviewHeader".Translate(), 1f);
            listingStandard.Label("BreedingRitual.OverviewPara1".Translate());
            listingStandard.Label("BreedingRitual.OverviewPara2".Translate());
            listingStandard.Label("BreedingRitual.OverviewPara3".Translate());
            listingStandard.GapLine();

            listingStandard.SubLabel("BreedingRitual.LengthHeader".Translate(), 1f);
            listingStandard.Label("BreedingRitual.LengthPara1".Translate());
            listingStandard.Label("BreedingRitual.LengthPara2".Translate());
            BreedingRitualSettings.durationTicks = listingStandard.SliderLabeled("BreedingRitual.LengthSlider".Translate((BreedingRitualSettings.durationTicks / 2500f).ToString("0.0").Named("DURATION")), BreedingRitualSettings.durationTicks, 1250f, 12500f);
            listingStandard.GapLine();

            listingStandard.SubLabel("BreedingRitual.ArrivalHeader".Translate(), 1f);
            listingStandard.Label("BreedingRitual.ArrivalPara1".Translate());
            listingStandard.Label("BreedingRitual.ArrivalPara2".Translate());
            listingStandard.Label("BreedingRitual.ArrivalPara3".Translate());
            if (listingStandard.RadioButton("BreedingRitual.ArrivalOption1".Translate(), !BreedingRitualSettings.manCarriesWoman && !BreedingRitualSettings.womanCarriesMan, 0f, "BreedingRitual.ArrivalOption1Tooltip".Translate()))
            { BreedingRitualSettings.manCarriesWoman = false; BreedingRitualSettings.womanCarriesMan = false; }
            if (listingStandard.RadioButton("BreedingRitual.ArrivalOption2".Translate(), BreedingRitualSettings.manCarriesWoman, 0f, "BreedingRitual.ArrivalOption2Tooltip".Translate()))
            { BreedingRitualSettings.manCarriesWoman = true; BreedingRitualSettings.womanCarriesMan = false; }
            if (listingStandard.RadioButton("BreedingRitual.ArrivalOption3".Translate(), !BreedingRitualSettings.manCarriesWoman && BreedingRitualSettings.womanCarriesMan, 0f, "BreedingRitual.ArrivalOption3Tooltip".Translate()))
            { BreedingRitualSettings.manCarriesWoman = false; BreedingRitualSettings.womanCarriesMan = true; }
            listingStandard.Label("BreedingRitual.ArrivalPostscript".Translate());
            listingStandard.GapLine();

            listingStandard.SubLabel("BreedingRitual.SpectatorsHeader".Translate(), 1f);
            listingStandard.Label("BreedingRitual.SpectatorsPara1".Translate());
            listingStandard.Label("BreedingRitual.SpectatorsPara2".Translate());
            if (listingStandard.RadioButton("BreedingRitual.SpectatorsOption1".Translate(), BreedingRitualSettings.autoInviteSpectators && !BreedingRitualSettings.neverAllowSpectators, 0f, "BreedingRitual.SpectatorsOption1Tooltip".Translate()))
            { BreedingRitualSettings.neverAllowSpectators = false; BreedingRitualSettings.autoInviteSpectators = true; }
            if (listingStandard.RadioButton("BreedingRitual.SpectatorsOption2".Translate(), !BreedingRitualSettings.autoInviteSpectators && !BreedingRitualSettings.neverAllowSpectators, 0f, "BreedingRitual.SpectatorsOption2Tooltip".Translate()))
            { BreedingRitualSettings.neverAllowSpectators = false; BreedingRitualSettings.autoInviteSpectators = false; }
            if (listingStandard.RadioButton("BreedingRitual.SpectatorsOption3".Translate(), BreedingRitualSettings.neverAllowSpectators, 0f, "BreedingRitual.SpectatorsOption3Tooltip".Translate()))
            { BreedingRitualSettings.neverAllowSpectators = true; BreedingRitualSettings.autoInviteSpectators = false; }
            listingStandard.GapLine();

            listingStandard.SubLabel("BreedingRitual.FertilityHeader".Translate(), 1f);
            listingStandard.Label("BreedingRitual.FertilityPara1".Translate());
            listingStandard.Label("BreedingRitual.FertilityPara2".Translate());
            listingStandard.Label("BreedingRitual.FertilityPara3".Translate());
            listingStandard.Label("BreedingRitual.FertilityPara4".Translate());
            if (listingStandard.RadioButton("BreedingRitual.FertilityOption1".Translate(), !BreedingRitualSettings.overridePregnancyApproach && !BreedingRitualSettings.overridePregnancyApproachCheat, 0f, "BreedingRitual.FertilityOption1Tooltip".Translate()))
            { BreedingRitualSettings.overridePregnancyApproach = false; BreedingRitualSettings.overridePregnancyApproachCheat = false; }
            if (listingStandard.RadioButton("BreedingRitual.FertilityOption2".Translate(), BreedingRitualSettings.overridePregnancyApproach, 0f, "BreedingRitual.FertilityOption2Tooltip".Translate()))
            { BreedingRitualSettings.overridePregnancyApproach = true; BreedingRitualSettings.overridePregnancyApproachCheat = false; }
            if (listingStandard.RadioButton("BreedingRitual.FertilityOption3".Translate(), !BreedingRitualSettings.overridePregnancyApproach && BreedingRitualSettings.overridePregnancyApproachCheat, 0f, "BreedingRitual.FertilityOption3Tooltip".Translate()))
            { BreedingRitualSettings.overridePregnancyApproach = false; BreedingRitualSettings.overridePregnancyApproachCheat = true; }
            listingStandard.CheckboxLabeled("BreedingRitual.FertilitySinglePregCheck".Translate(), ref BreedingRitualSettings.singlePregnancyCheck, "BreedingRitual.FertilitySinglePregCheckTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.FertilityStatBasedLovin".Translate(), ref BreedingRitualSettings.recalculateLovinDuration, "BreedingRitual.FertilityStatBasedLovinTooltip".Translate());
            listingStandard.Label("BreedingRitual.FertilityPostscript".Translate());
            listingStandard.GapLine();

            listingStandard.SubLabel("Cooldown policy".Translate(), 1f);
            listingStandard.Label("BreedingRitual.CooldownPara1".Translate());
            listingStandard.Label("BreedingRitual.CooldownPara2".Translate());
            listingStandard.Label("BreedingRitual.CooldownPara3".Translate());
            listingStandard.Label("BreedingRitual.CooldownPara4".Translate());
            listingStandard.Label("BreedingRitual.CooldownPara5".Translate());
            if (listingStandard.RadioButton("BreedingRitual.CooldownOption1".Translate(), !BreedingRitualSettings.useLeaderCooldowns && !BreedingRitualSettings.useMoralistCooldowns, 0f, "BreedingRitual.CooldownOption1Tooltip".Translate()))
            { BreedingRitualSettings.useLeaderCooldowns = false; BreedingRitualSettings.useMoralistCooldowns = false; }
            if (listingStandard.RadioButton("BreedingRitual.CooldownOption2".Translate(), BreedingRitualSettings.useLeaderCooldowns, 0f, "BreedingRitual.CooldownOption2Tooltip".Translate()))
            { BreedingRitualSettings.useLeaderCooldowns = true; BreedingRitualSettings.useMoralistCooldowns = false; }
            if (listingStandard.RadioButton("BreedingRitual.CooldownOption3".Translate(), !BreedingRitualSettings.useLeaderCooldowns && BreedingRitualSettings.useMoralistCooldowns, 0f, "BreedingRitual.CooldownOption3Tooltip".Translate()))
            { BreedingRitualSettings.useLeaderCooldowns = false; BreedingRitualSettings.useMoralistCooldowns = true; }
            listingStandard.Label("BreedingRitual.CooldownPostscript".Translate());
            listingStandard.GapLine();

            listingStandard.SubLabel("BreedingRitual.PsybreedingHeader".Translate(), 1f);
            listingStandard.Label("BreedingRitual.PsybreedingPara1".Translate());
            listingStandard.Label("BreedingRitual.PsybreedingPara2".Translate());
            listingStandard.Label("BreedingRitual.PsybreedingPara3".Translate());
            listingStandard.Label("BreedingRitual.PsybreedingPara4".Translate());
            listingStandard.Label("BreedingRitual.PsybreedingPara5".Translate());
            listingStandard.Label("BreedingRitual.PsybreedingPara6".Translate());
            listingStandard.Label("BreedingRitual.PsybreedingPara7".Translate());
            listingStandard.Label("BreedingRitual.PsybreedingPara8".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.PsybreedingRequirePsybond".Translate(), ref BreedingRitualSettings.psybondRequired, "BreedingRitual.PsybreedingRequirePsybondTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.PsybreedingBoostFertility".Translate(), ref BreedingRitualSettings.psyboostFertility, "BreedingRitual.PsybreedingBoostFertilityTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.PsybreedingSpectatorMeditation".Translate(), ref BreedingRitualSettings.psybreedingMeditation, "BreedingRitual.PsybreedingSpectatorMeditationTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.PsybreedingRippleVFX".Translate(), ref BreedingRitualSettings.psybreedingRipple, "BreedingRitual.PsybreedingRippleVFXTooltip".Translate());
            BreedingRitualSettings.psybreedingDurationTicks = listingStandard.SliderLabeled("BreedingRitual.LengthSlider".Translate((BreedingRitualSettings.psybreedingDurationTicks / 2500f).ToString("0.0").Named("DURATION")), BreedingRitualSettings.psybreedingDurationTicks, 2500f, 25000f);
            BreedingRitualSettings.psyfocusMinimum = listingStandard.SliderLabeled("BreedingRitual.PsybreedingPsyfocusCost".Translate((BreedingRitualSettings.psyfocusMinimum).ToStringPercent("0.0").Named("QUANTITY")), BreedingRitualSettings.psyfocusMinimum, 0f, 0.99f);
            BreedingRitualSettings.neuralHeatPsybreeding = listingStandard.SliderLabeled("BreedingRitual.PsybreedingNeuralHeat".Translate((BreedingRitualSettings.neuralHeatPsybreeding).ToString("0").Named("QUANTITY")), BreedingRitualSettings.neuralHeatPsybreeding, 0f, 1000f);
            listingStandard.GapLine();

            listingStandard.SubLabel("BreedingRitual.MiscellaneousHeader".Translate(), 1f);
            listingStandard.CheckboxLabeled("BreedingRitual.RepetitionPenalty".Translate(), ref BreedingRitualSettings.repetitionPenalty, "BreedingRitual.RepetitionPenaltyTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.RespectSexuality".Translate(), ref BreedingRitualSettings.respectPawnSexuality, "BreedingRitual.RespectSexualityTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.AnnouncePregChance".Translate(), ref BreedingRitualSettings.announcePregnancyChance, "BreedingRitual.AnnouncePregChanceTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.FertilityReport".Translate(), ref BreedingRitualSettings.fertilityReportLetter, "BreedingRitual.FertilityReportTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.AllowPregnantWoman".Translate(), ref BreedingRitualSettings.allowPregnantWomen, "BreedingRitual.AllowPregnantWomanTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.OrganizerGetsMemory".Translate(), ref BreedingRitualSettings.organizerRemembersRitual, "BreedingRitual.OrganizerGetsMemoryTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.PlayInstruments".Translate(), ref BreedingRitualSettings.playInstruments, "BreedingRitual.PlayInstrumentsTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.IdeoSFX".Translate(), ref BreedingRitualSettings.playIdeoMusic, "BreedingRitual.IdeoSFXTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.IdeoVFX".Translate(), ref BreedingRitualSettings.showIdeoVFX, "BreedingRitual.IdeoVFXTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.FarewellHuddle".Translate(), ref BreedingRitualSettings.finalGathering, "BreedingRitual.FarewellHuddleTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.GradualDispersal".Translate(), ref BreedingRitualSettings.gradualDispersal, "BreedingRitual.GradualDispersalTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.BedUsageOverride".Translate(), ref BreedingRitualSettings.bedUsageTolerance, "BreedingRitual.BedUsageOverrideTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.AllowGuestBreeding".Translate(), ref BreedingRitualSettings.seduceGuests, "BreedingRitual.AllowGuestBreedingTooltip".Translate());
            listingStandard.CheckboxLabeled("BreedingRitual.AttachableOutcomes".Translate(), ref BreedingRitualSettings.attachableOutcomes, "BreedingRitual.AttachableOutcomesTooltip".Translate());
            listingStandard.GapLine();

            listingStandard.SubLabel("BreedingRitual.DebugHeader".Translate(), 1f);
            listingStandard.Label("BreedingRitual.DebugPara1".Translate());
            listingStandard.Label("BreedingRitual.DebugPara2".Translate());
            listingStandard.Label("BreedingRitual.DebugPara3".Translate());
            listingStandard.Label("BreedingRitual.DebugPara4".Translate());
            if (listingStandard.ButtonText("BreedingRitual.DebugButton1".Translate(), null, 0.25f)) { AddRemoveRitual("Breeding"); }
            if (listingStandard.ButtonText("BreedingRitual.DebugButton2".Translate(), null, 0.25f)) { AddRemoveRitual("Psybreeding"); }
            if (listingStandard.ButtonText("BreedingRitual.DebugButton3".Translate(), null, 0.25f)) { AddRemoveRitual("Breeding", false); }
            if (listingStandard.ButtonText("BreedingRitual.DebugButton4".Translate(), null, 0.25f)) { AddRemoveRitual("Psybreeding", false); }
            if (listingStandard.ButtonText("BreedingRitual.DebugButton5".Translate(), null, 0.25f)) { BondPawns(); }
            listingStandard.End();
            Widgets.EndScrollView();

            base.DoSettingsWindowContents(rect);
        }

        // Debug tool to assist with tests of psybreeding
        protected void BondPawns()
        {
            if (Verse.Current.Game == null)
            {
                Messages.Message("MessageGameNotRunning".Translate(), MessageTypeDefOf.NegativeEvent, true);
            }
            Building_Bed bed = (Building_Bed)((UIRoot_Play)Verse.Current.Root_Play.uiRoot).mapUI.selector.SingleSelectedThing;
            if (bed == null)
            {
                Messages.Message("MessageBedNotSelected".Translate(), MessageTypeDefOf.NegativeEvent, true);
                return;
            }
            if (bed.GetAssignedPawns() == null || bed.GetAssignedPawns().Count() < 2)
            {
                Messages.Message("MessageBedNotAssigned".Translate(), MessageTypeDefOf.NegativeEvent, true);
                return;
            }
            Pawn p1 = bed.GetAssignedPawns().First();
            Pawn p2 = bed.GetAssignedPawns().Last();
            Gene_PsychicBonding g1 = new Gene_PsychicBonding();
            g1.pawn = p1;
            Gene_PsychicBonding g2 = new Gene_PsychicBonding();
            g2.pawn = p2;
            g1.BondTo(p2);
            g2.BondTo(p1);
            Messages.Message("MessagePsybondCreated".Translate(), MessageTypeDefOf.PositiveEvent, true);
        }

        // Splice breeding rituals into an existing game - or remove them (to clean up before uninstalling the mod)
        protected void AddRemoveRitual(string defName, bool add = true)
        {
            PreceptDef preceptDef = DefDatabase<PreceptDef>.GetNamedSilentFail(defName);
            RitualPatternDef ritualPattern = DefDatabase<RitualPatternDef>.GetNamedSilentFail(defName);
            if (Verse.Current.Game == null)
            {
                Messages.Message("MessageGameNotRunning".Translate(), MessageTypeDefOf.NegativeEvent, true);
            }
            else if (preceptDef == null)
            {
                Messages.Message("MessageDefinitionMissing".Translate("Precept".Named("CATEGORY"), defName.Named("DEFNAME")), MessageTypeDefOf.NegativeEvent, true);
            }
            else if (ritualPattern == null)
            {
                Messages.Message("MessageDefinitionMissing".Translate("Ritual Pattern".Named("CATEGORY"), defName.Named("DEFNAME")), MessageTypeDefOf.NegativeEvent, true);
            }
            else if (Find.IdeoManager.classicMode)
            {
                Messages.Message("MessageIdeologyInactive".Translate(), MessageTypeDefOf.NegativeEvent, true);
            }
            else
            {
                // Survey the colonists
                Dictionary<Ideo, int> survey = new Dictionary<Ideo, int>();
                foreach (Pawn p in Verse.Current.Game.CurrentMap.mapPawns.FreeColonists)
                {
                    if ((p.ideo != null) && (p.ideo.Ideo != null))
                    {
                        if (survey.ContainsKey(p.ideo.Ideo)) {
                            survey[p.ideo.Ideo]++;
                        }
                        else
                        {
                            survey[p.ideo.Ideo] = 1;
                        }
                    }
                }

                if (survey.Count() == 0)
                {
                    Messages.Message("MessageNoIdeoligion".Translate(), MessageTypeDefOf.NegativeEvent, true);
                    return;
                }

                Ideo dominantIdeo = survey.Keys.OrderByDescending(k => survey[k]).First();
                Messages.Message("MessageDominantIdeoligion".Translate(dominantIdeo.Named("IDEO"), survey[dominantIdeo].Named("COUNT")), MessageTypeDefOf.NeutralEvent, true);

                // Splice the ritual into the dominant ideoligion
                Precept_Ritual localRitual = dominantIdeo.GetAllPreceptsOfType<Precept_Ritual>().FirstOrDefault(p => p.def.defName == defName);
                if (add)
                {
                    if (localRitual != null)
                    {
                        Messages.Message("MessageDuplicatePrecept".Translate(dominantIdeo.Named("IDEO"), defName.Named("DEFNAME")), MessageTypeDefOf.NegativeEvent, true);
                        return;
                    }
                    Precept preceptToAdd = PreceptMaker.MakePrecept(preceptDef);
                    preceptToAdd.SetName(defName);            // just in case RimWorld auto-generates something absurd like "Fiesta of Concupiscence"
                    dominantIdeo.AddPrecept(preceptToAdd, true, null, ritualPattern);
                    Messages.Message("MessagePreceptAdded".Translate(dominantIdeo.Named("IDEO"), defName.Named("DEFNAME")), MessageTypeDefOf.PositiveEvent, true);
                }
                else
                {
                    if (localRitual == null)
                    {
                        Messages.Message("MessagePreceptNotFound".Translate(dominantIdeo.Named("IDEO"), defName.Named("DEFNAME")), MessageTypeDefOf.NegativeEvent, true);
                        return;
                    }
                    dominantIdeo.RemovePrecept(localRitual);
                    Messages.Message("MessagePreceptRemoved".Translate(dominantIdeo.Named("IDEO"), defName.Named("DEFNAME")), MessageTypeDefOf.PositiveEvent, true);
                }
            }
        }

        public override string SettingsCategory()
        {
            return "Breeding Ritual";
        }

        public override void WriteSettings()
        {
            // Most of the Player's changes will flip the value of a static bool variable
            // Hence, those changes will take effect as soon as the relevant code is invoked (e.g. next Tick)

            // However, a few Options changes can only be realized by altering the actual Defs
            // The correct/efficient way to do this would be for the Player to manually edit the mod's XML files
            // But that's tedious (and error-prone) so we'll edit the in-memory version of those Defs instead
            UpdateRitualDefs();
            
            // At this point, the Defs have been updated to reflect the Player's choices. Now we must store those
            // values to disk so that they'll be preserved (and automatically re-applied during the next game session).

            // Write the altered config values to disk
            base.WriteSettings();
        }

        // This method updates Ritual definitions (in memory, not XML) to reflect the various Options values.
        // This will typically be called once during startup (to apply Options values previously set by Player)
        // but it must be invoked again whenever changes are made to Options.
        public static void UpdateRitualDefs()
        {
            // We don't actually know what's *changed*, so this is going to be inefficient
            // Fortunately, this code is called VERY infrequently (e.g. upon closing of the ModSettings window)
            // therefore a bit of inefficiency won't damage game performance

            // Lookup the ritual defs
            PreceptDef breedingPrecept = DefDatabase<PreceptDef>.GetNamed("Breeding");
            PreceptDef psybreedingPrecept = DefDatabase<PreceptDef>.GetNamed("Psybreeding");
            RitualBehaviorDef breedingBehavior = DefDatabase<RitualBehaviorDef>.GetNamed("Breeding");
            RitualBehaviorDef psybreedingBehavior = DefDatabase<RitualBehaviorDef>.GetNamed("Psybreeding");
            RitualPatternDef breedingPattern = DefDatabase<RitualPatternDef>.GetNamed("Breeding");
            RitualPatternDef psybreedingPattern = DefDatabase<RitualPatternDef>.GetNamed("Psybreeding");
            AbilityGroupDef leaderGroup = DefDatabase<AbilityGroupDef>.GetNamed("Leader");
            AbilityGroupDef moralistGroup = DefDatabase<AbilityGroupDef>.GetNamed("Moralist");

            // Step 1
            // Which cooldowns (if any) will we invoke when starting the ritual?
            if (BreedingRitualSettings.useLeaderCooldowns)
            {
                psybreedingPrecept.useCooldownFromAbilityGroupDef = breedingPrecept.useCooldownFromAbilityGroupDef = leaderGroup;
            }
            else if (BreedingRitualSettings.useMoralistCooldowns)
            {
                psybreedingPrecept.useCooldownFromAbilityGroupDef = breedingPrecept.useCooldownFromAbilityGroupDef = moralistGroup;
            }
            else
            {
                psybreedingPrecept.useCooldownFromAbilityGroupDef = breedingPrecept.useCooldownFromAbilityGroupDef = null;
                // We've cleared the cooldowns specification; future invocations of the ritual will be cooldown-free
                // But... the Player might still be annoyed because they need to wait 3-10 days for the EXISTING
                // cooldown to expire.

                // As a convenience feature, we'll attempt to clear the cooldowns on this ritual
                // Obviously, this only works if there's a game *currently* running
                if (Verse.Current.Game != null)
                {
                    // Remember that there could be multiple Breeding Rituals (belonging to different societies and ideoligions)
                    // This is analogous to how each ideoligion has its own Trial, its own Funeral, etc.
                    // We need to iterate through all of them
                    foreach (Ideo ideo in Verse.Find.IdeoManager.IdeosInViewOrder)
                    {
                        // Try to find this ideoligion's breeding ritual
                        Precept_Ritual localBreedingRitual = ideo.GetAllPreceptsOfType<Precept_Ritual>().FirstOrDefault(p => p.def.defName == "Breeding");
                        if (localBreedingRitual != null)
                        {
                            // Reset the cooldown
                            localBreedingRitual.abilityOnCooldownUntilTick = -1;

                            // Sidebar: since we're already here, make bonus adjustments
                            localBreedingRitual.playsIdeoMusic = BreedingRitualSettings.playIdeoMusic;
                        }
                        // Try to find this ideoligion's psybreeding ritual
                        Precept_Ritual localPsybreedingRitual = ideo.GetAllPreceptsOfType<Precept_Ritual>().FirstOrDefault(p => p.def.defName == "Psybreeding");
                        if (localPsybreedingRitual != null)
                        {
                            // Reset the cooldown
                            localPsybreedingRitual.abilityOnCooldownUntilTick = -1;

                            // Sidebar: since we're already here, make bonus adjustments
                            localPsybreedingRitual.playsIdeoMusic = BreedingRitualSettings.playIdeoMusic;
                        }
                    }
                }
            }

            // Step 2
            // Should the ritual be impacted by the usual 20-day spam penalty?
            psybreedingPrecept.useRepeatPenalty = breedingPrecept.useRepeatPenalty = BreedingRitualSettings.repetitionPenalty;

            // Step 3
            // Should the woman walk to bed or be carried?
            // Try to lookup the relevant stage. This will fail if someone has edited the XML file. In that case, abandon the effort.
            RitualStage approach = breedingBehavior.stages.FirstOrDefault(s => (s.highlightRolePawns != null) && s.highlightRolePawns.Count > 0);
            RitualStage approachPsy = psybreedingBehavior.stages.FirstOrDefault(s => (s.highlightRolePawns != null) && s.highlightRolePawns.Count > 0);
            if (approach != null && approachPsy != null)
            {
                // Try to lookup the behaviors for each pawn during this stage
                RitualRoleBehavior manApproach = approach.roleBehaviors.FirstOrDefault(b => b.roleId == "man");
                RitualRoleBehavior manApproachPsy = approachPsy.roleBehaviors.FirstOrDefault(b => b.roleId == "man");
                RitualRoleBehavior womanApproach = approach.roleBehaviors.FirstOrDefault(b => b.roleId == "woman");
                RitualRoleBehavior womanApproachPsy = approachPsy.roleBehaviors.FirstOrDefault(b => b.roleId == "woman");
                // Try to lookup the duties
                DutyDef goToBed = DefDatabase<DutyDef>.GetNamed("GoToBed");
                DutyDef deliverPawnToBed = DefDatabase<DutyDef>.GetNamed("DeliverPawnToBedForBirth");
                DutyDef wanderClose = DutyDefOf.WanderClose; // DefDatabase<DutyDef>.GetNamed("WanderClose");

                // TODO: add an auto-skip condition for early stage if we're not doing Bridal Carry!

                if (manApproach == null || womanApproach == null || manApproachPsy == null || womanApproachPsy == null)
                {
                    // Something has gone wrong. Presumably the XML files have been mangled. Abandon the effort.
                }
                else if (BreedingRitualSettings.manCarriesWoman)
                {
                    // Man carries woman to bed
                    manApproachPsy.dutyDef = manApproach.dutyDef = deliverPawnToBed;
                    manApproachPsy.jobReportOverride = manApproach.jobReportOverride = "carrying woman to bed";
                    ((RitualStage_InteractWithRole)approachPsy).targetId = ((RitualStage_InteractWithRole)approach).targetId = "woman";
                    womanApproachPsy.dutyDef = womanApproach.dutyDef = wanderClose;
                    womanApproachPsy.jobReportOverride = womanApproach.jobReportOverride = "preparing";

                    // It's impossible for BOTH partners to carry each other
                    // If the config file has both options set to true, then fix it
                    BreedingRitualSettings.womanCarriesMan = false;
                }
                else if (BreedingRitualSettings.womanCarriesMan)
                {
                    // Woman carries woman to bed
                    manApproachPsy.dutyDef = manApproach.dutyDef = wanderClose;
                    manApproachPsy.jobReportOverride = manApproach.jobReportOverride = "preparing for snu-snu";
                    ((RitualStage_InteractWithRole)approachPsy).targetId = ((RitualStage_InteractWithRole)approach).targetId = "man";
                    womanApproachPsy.dutyDef = womanApproach.dutyDef = deliverPawnToBed;
                    womanApproachPsy.jobReportOverride = womanApproach.jobReportOverride = "carrying man to bed";
                }
                else
                {
                    // Both pawns walk to bed
                    manApproachPsy.dutyDef = manApproach.dutyDef = goToBed;
                    manApproachPsy.jobReportOverride = manApproach.jobReportOverride = "going to bed";
                    womanApproachPsy.dutyDef = womanApproach.dutyDef = goToBed;
                    womanApproachPsy.jobReportOverride = womanApproach.jobReportOverride = "going to bed";
                }
            }

            // Step 4: Tweak the ritual def slightly to explain the exclusion of pregnant women
            RitualRole womanRole = breedingBehavior.roles.First(r => r.id.ToLower() == "woman");
            RitualRole womanRolePsy = psybreedingBehavior.roles.First(r => r.id.ToLower() == "woman");
            womanRolePsy.missingDesc = womanRole.missingDesc = BreedingRitualSettings.allowPregnantWomen ? "woman" : "non-pregnant woman";

            // Step 5: Adjust ritual duration
            breedingBehavior.durationTicks = new IntRange((int)BreedingRitualSettings.durationTicks, (int)BreedingRitualSettings.durationTicks);
            psybreedingBehavior.durationTicks = new IntRange((int)BreedingRitualSettings.psybreedingDurationTicks, (int)BreedingRitualSettings.psybreedingDurationTicks);

            // Step 6: Tinker with final stage
            DutyDef partyDance = DefDatabase<DutyDef>.GetNamed("PartyDance");
            DutyDef spectateCircle = DefDatabase<DutyDef>.GetNamed("SpectateCircle");
            RitualStage finalStage = breedingBehavior.stages[breedingBehavior.stages.Count - 1];
            RitualStage finalStagePsy = psybreedingBehavior.stages[breedingBehavior.stages.Count - 1];
            // Should the spectators gather close around the couple? Or should they fill the room with dancing and music?
            finalStagePsy.defaultDuty = finalStage.defaultDuty = BreedingRitualSettings.finalGathering ? spectateCircle : partyDance;

            // Step 7: Enable/disable ideo effects
            psybreedingPattern.playsIdeoMusic = breedingPattern.playsIdeoMusic = BreedingRitualSettings.playIdeoMusic;
            psybreedingPrecept.usesIdeoVisualEffects = breedingPrecept.usesIdeoVisualEffects = BreedingRitualSettings.showIdeoVFX;

            psybreedingBehavior.spectatorFilter.description = breedingBehavior.spectatorFilter.description =
                (BreedingRitualSettings.neverAllowSpectators) ? "MessageSpectatorsNotAllowed".Translate() :
                "MessageSpectatorsNotAllowedChild".Translate();


            // Step 8: Include/exclude ritual quality factors
            // TODO: this is a giant pain-in-the-ass. Probably remove it from the mod instead of implementing it.
        }

        // Helper method to find the Leader or Moralist (needed in a few weird contexts)
        // Remember that this method CAN return null (i.e. Leader is missing/dead or not appointed yet)
        public static Pawn FindSpecialist(Map map, Precept_RoleSingle role)
        {
            if (map == null || role == null) { return null; }
            return map.mapPawns.FreeColonists.FirstOrDefault(p => p.ideo.Ideo.GetRole(p) == role);
        }
    }

    [StaticConstructorOnStartup]
    static class ModInitializer
    {
        // This method will be automatically invoked LATE in game startup - after mods have initialized and Defs are loaded
        static ModInitializer()
        {
            // At this point the mod's Options are loaded, and the game's Defs are loaded.
            // However, the Defs reflect the bare/default contents of the mod's XML files.
            // That's not correct. We want Defs which reflect XML _*and*_ player-chosen Options.

            // Therefore we invoke the UpdateRitualDefs method. Effectively, we're simulating an action
            // in which the Player opened the Options menu and re-applied all of their previous choices.
            try
            {
                // This is a slightly risky action (the update method can fail if integrity of XML files has been compromised)
                // Therefore we wrap the whole thing in try-catch and send out a red error message if it fails
                BreedingRitualMod.UpdateRitualDefs();
            }
            catch
            {
                if ("BreedingRitualMod_ErrorDuringLoading".CanTranslate())
                {
                    Log.Error("ErrorDuringLoading".Translate());
                } else
                {
                    Log.Error("An error occurred while loading the Breeding Ritual mod. This is usually caused by missing or damaged XML files. If you have recently edited those files then please attempt to revert your changes. Alternatively, you can try to download a fresh copy of the mod.");
                }
                
            }

            // Apply the Harmony postfix patches
            Harmony harmony = new Harmony("GulMadred.BreedingRitual.1");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

//TODO: fail triggers
// bed dead
// man dead
// woman dead

// TODO: test with bed getting minified instead of destroyed

// TODO: test with colonist breeding atheist visitor
// TODO: test with 2 ideo-guests
// todo: test with slave+slave
// todo: test with 2 atheist

// TODO: keyed strings for auto-generation of ritual names

// TODO: replace Preview.png file

// TODO: SleepForever duty seems to cause "Slept on Ground" thoughts. Investigate.

// TODO: retest with malnutrition! Pawn kept failing out of meditation jobs!

// TODO: reduce frequency of UpdateAllDuties. Try to preserve before-and-after RitualStage transitions to minimize
    // silly scramble activity in which pawns shuffle around for no reason.

// TODO: pawn incapped by neural heat gets 1 climax. Probably leave as Known Issue. Very low priority.
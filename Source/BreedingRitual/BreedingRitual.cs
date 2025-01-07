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

        public override void ExposeData()
        {
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

            base.ExposeData();
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
        private static float totalContentHeight = 3000f;
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
            listingStandard.SubLabel("Overview", 1f);
            listingStandard.Label("Any changes made to these Options will take effect immediately. It is NOT necessary to restart the game.");
            listingStandard.Label("Changes can be made at the main menu or during normal gameplay. It's slightly risky to make changes while a breeding ritual is in-progress, but you're welcome to try. If the ritual seems to get stuck, just Cancel it and try again.");
            listingStandard.Label("To use this mod:\n0. The new ritual must be part of your society's ideoligion. Either start a new game, or scroll to bottom of this page for assistance\n1. Setup a Double Bed (or any equivalent, such as Royal Bed or Double Bedroll)\n2. Assign a male and female pawn to the bed (they don't need to be in a relationship)\n3. Select the bed. You'll notice a new button: \"Gather for breeding\"\n4. Click this button to open the Ritual planning window.\n5. Click \"Begin\"");
            listingStandard.GapLine();

            listingStandard.SubLabel("Length of the breeding ritual", 1f);
            listingStandard.Label("The default length is 1 in-game hour. The participants will perform Lovin' repeatedly until the allotted time runs out, and then they'll take a nap to recover. Very brief rituals may include only a single round of Lovin', whereas a long ritual provides multiple opportunities for conception.");
            listingStandard.Label("If you're powergaming then you should probably choose longer rituals. But if you care about pawns' quality-of-life (and/or realistic storytelling) then you should remember that a long ritual would be very strenous. If you want a compromise approach then you could choose a long duration BUT also ensure that participants consume performance-enhancing drugs before the ritual begins.");
            //listingStandard.Label(BreedingRitualSettings.durationTicks.ToString("0"));
            BreedingRitualSettings.durationTicks = listingStandard.SliderLabeled("Duration: " + (BreedingRitualSettings.durationTicks / 2500f).ToString("0.0") + " hours", BreedingRitualSettings.durationTicks, 1250f, 12500f);
            listingStandard.GapLine();

            listingStandard.SubLabel("Arrival method", 1f);
            listingStandard.Label("The ritual can't begin until both participants are in bed. The simplest option is for both of them to walk there. Alternatively, the man can bring the woman in a 'bridal carry'.");
            listingStandard.Label("Please note that both people must be CAPABLE of walking. A person is not eligible to participate if they're comatose, paralyzed, downed, etc. Bridal carry doesn't alter the rules; it's just a less-efficient form of locomotion which is included for flavor/roleplay reasons.");
            listingStandard.Label("How should the chosen couple go to bed?");
            if (listingStandard.RadioButton("Both pawns walk", !BreedingRitualSettings.manCarriesWoman && !BreedingRitualSettings.womanCarriesMan, 0f, "Both participants walk to bed."))
            { BreedingRitualSettings.manCarriesWoman = false; BreedingRitualSettings.womanCarriesMan = false; }
            if (listingStandard.RadioButton("Man carries woman", BreedingRitualSettings.manCarriesWoman, 0f, "The man will retrieve the woman and carry her to bed."))
            { BreedingRitualSettings.manCarriesWoman = true; BreedingRitualSettings.womanCarriesMan = false; }
            if (listingStandard.RadioButton("Woman carries man", !BreedingRitualSettings.manCarriesWoman && BreedingRitualSettings.womanCarriesMan, 0f, "According to ancient cosmotexts, men can be used for snu-snu."))
            { BreedingRitualSettings.manCarriesWoman = false; BreedingRitualSettings.womanCarriesMan = true; }
            listingStandard.Label("Prisoners are not officially supported. If you want to experiment then please choose an arrival method which makes sense. A prisoner is able to do Lovin' (if he's assigned to a Double Bed), but he's forbidden from leaving his cell - so he can't Bridal Carry someone unless they're standing nearby.");
            listingStandard.GapLine();

            listingStandard.SubLabel("Fertility adjustment", 1f);
            listingStandard.Label("During the breeding ritual, the couple will abide by their usual Pregnancy Approach (such as 'Avoid Pregnancy' or 'Try for Baby'). The player can use this ritual to bring together strangers, rivals, or enemies - the mod will accept any adult pawns assigned to the same bed. If the selected pawns aren't actually a couple, then their Pregnancy Approach will default to 'Normal'.");
            listingStandard.Label("I expect that players will usually select an actual couple to participate in the ritual. But the existing couple's approach might have been set to 'Avoid Pregnancy' at some point in the past.");
            listingStandard.Label("The theme of this mod is the enthusiastic celebration of babymaking. It doesn't really fit the tone if participants are hesitant about the possibility of having a child. Therefore we can temporarily force the couple to 'Try for Baby' until the ritual is complete. Alternatively, we can apply a cheat which bypasses fertility calculations and ensures conception (even for sterile pawns).");
            listingStandard.Label("Should we influence the chance of conception during a ritual?");
            if (listingStandard.RadioButton("Normal", !BreedingRitualSettings.overridePregnancyApproach && !BreedingRitualSettings.overridePregnancyApproachCheat, 0f, "The couple will use their usual approach."))
            { BreedingRitualSettings.overridePregnancyApproach = false; BreedingRitualSettings.overridePregnancyApproachCheat = false; }
            if (listingStandard.RadioButton("Try for Baby", BreedingRitualSettings.overridePregnancyApproach, 0f, "A couple will always 'Try for Baby' during a breeding ritual (400% boost) but will revert to their normal approach afterwards."))
            { BreedingRitualSettings.overridePregnancyApproach = true; BreedingRitualSettings.overridePregnancyApproachCheat = false; }
            if (listingStandard.RadioButton("Conception guaranteed (CHEAT)", !BreedingRitualSettings.overridePregnancyApproach && BreedingRitualSettings.overridePregnancyApproachCheat, 0f, "A couple will always conceive during a breeding ritual, even if both of them are completely sterile. THIS IS A CHEAT!"))
            { BreedingRitualSettings.overridePregnancyApproach = false; BreedingRitualSettings.overridePregnancyApproachCheat = true; }
            listingStandard.CheckboxLabeled("Single pregnancy check?", ref BreedingRitualSettings.singlePregnancyCheck, "In RimWorld, each pawn performs an independent Lovin' job. This job is usually (but not always) synchronized with their partner's job. When the job finishes, the pawn gains a happy memory and performs an RNG check for pregnancy. Hence, each round of Lovin' generates TWO simultaneous opportunities for conception. That seems like a mistake. The mod can correct it, ensuring that ritual Lovin' yields one RNG roll instead of two (the number of happy memories is unchanged). Please keep this option in mind when viewing reports. If the mod reports that a couple performed FOUR rounds of Lovin' during a breeding ceremony, and this option is switched off, then the game probably made EIGHT conception checks.");
            listingStandard.CheckboxLabeled("Stats influence Lovin?", ref BreedingRitualSettings.recalculateLovinDuration, "The duration of a Lovin' action is random (anywhere from 250 ticks to 2750). This mod can reduce the randomness, making it much more stats-based. ↑Moving↑ ↑Manipulation↑ ↑Consciousness↑ and ↓Pain↓ are considered, so the player can obtain better outcomes via drugs (or bionics, magic, etc). The revised calculation doesn't change fertility, but it allows healthier pawns to complete the task more quickly (alternatively: you might imagine that they're RECOVERING more quickly). Thus a healthy pawn will have more opportunities for conception within the same ritual timeframe. This policy is applied ONLY during the breeding ritual. Normal Lovin' is assumed to be casual and spontaneous rather than results-focused. Uncheck this box to use the vanilla (i.e. fully random) duration for Lovin'.");
            listingStandard.Label("Please note that only the *participants* in the ritual are impacted. Other pawns can perform Lovin' while the ritual is ongoing, but they won't be influenced by the policy adjustment.");
            listingStandard.GapLine();

            listingStandard.SubLabel("Cooldown policy", 1f);
            listingStandard.Label("For balance reasons, I recommend placing a usage restriction on this ritual. It's possible for the ritual to invoke the abilities of your ideoligion Leader, putting them on cooldown. Until the cooldown is completed, you'll be unable to perform the ritual again. You won't be able to perform the ritual AT ALL if your society doesn't yet have a Leader. Effectively, you'll need to 'spend' one usage of [Combat Command] or [Work Drive] in order to start this ritual.");
            listingStandard.Label("You can optionally switch this duty so that it impacts the Moral Guide instead. Thus, you'll be sacrificing a [Preach Health] or [Conversion] for each breeding ritual. Neither the Leader nor the Moral Guide needs to actually ATTEND the ritual; they just take a moment to give it their blessing.");
            listingStandard.Label("Why? Well, the ritual provides a significant on-demand boost to the Mood of the selected pawns. If it's readily available then you might be tempted to employ it whenever your colonists get upset (instead of properly addressing their needs). You could also imagine that the Moral Guide must CONVINCE the chosen couple to participate \"for the good of the tribe\". Thus, the ritual is similar to using the [Reassure] ability - and it ought to be subject to the same cooldown system.");
            listingStandard.Label("If this sounds like too much hassle then you're free to ignore it. By default, the ritual can be started at any time - and it can be repeated as often as you wish.");
            listingStandard.Label("Whose special abilities should be invoked by the breeding ritual?");
            if (listingStandard.RadioButton("Nobody (unlimited usage)", !BreedingRitualSettings.useLeaderCooldowns && !BreedingRitualSettings.useMoralistCooldowns, 0f, "The ritual can be performed whenever the player decides to do so."))
            { BreedingRitualSettings.useLeaderCooldowns = false; BreedingRitualSettings.useMoralistCooldowns = false; }
            if (listingStandard.RadioButton("Leader", BreedingRitualSettings.useLeaderCooldowns, 0f, "The ritual depends on the Leader's ability cooldown."))
            { BreedingRitualSettings.useLeaderCooldowns = true; BreedingRitualSettings.useMoralistCooldowns = false; }
            if (listingStandard.RadioButton("Moral Guide", !BreedingRitualSettings.useLeaderCooldowns && BreedingRitualSettings.useMoralistCooldowns, 0f, "The ritual depends on the Moral Guide's ability cooldown."))
            { BreedingRitualSettings.useLeaderCooldowns = false; BreedingRitualSettings.useMoralistCooldowns = true; }
            listingStandard.Label("Unfortunately, if you saved the game while the ritual was currently on cooldown ... then that cooldown will need to run out naturally. All *subsequent* uses of the ritual will follow your newly-selected policy.");
            listingStandard.GapLine();

            listingStandard.SubLabel("Psybreeding", 1f);
            listingStandard.Label("Psybreeding is a variation on the normal breeding ritual. The couple must already be psychically bonded to each other, and one of them must be a psycaster. The ritual will expend all of their psyfocus as the couple attempts to achieve a perfect union of body and mind. If they succeed (i.e. they conceive a child) then the partner's latent psy abilities will be awakened.");
            listingStandard.Label("This is meant to be a major ordeal. It will require a lot of time for preparation (i.e. meditating to gather psyfocus), execution (i.e. bedroom activities), and recovery. Unlike a standard breeding (which includes many rounds of Lovin'), the couple will perform only one hours-long Lovin' action. Players should take care that no interruptions occur, lest the entire ritual be wasted.");
            listingStandard.Label("If you use the stats-based Lovin' (scroll down to find it) then young and healthy pawns will be more productive breeders - because they can complete several rounds of Lovin' per hour. This advantage doesn't apply during Psybreeding; it's inherently *slow* and it relies on a skilled psycaster (who is presumably older). You can decide which of these rituals fits your playstyle - or include both and choose whichever one best fits your couple.");
            listingStandard.Label("Spectators are allowed to attend a Psybreeding ritual, but they don't really fit the theme. If the normal ritual is a raucous communal celebration of life and hope, then this one is a private affirmation of a couple's connection to each other and the sharing of a gift. If you wish to include spectators while preserving the 'Psy' theme, there's an option for spectators to meditate instead of dancing.");
            listingStandard.Label("You can reduce the psyfocus threshold to 0 if you want, but remember that a psycaster is still required. Any psycaster who participates in this ritual will gradually expend ALL of their accumulated psyfocus - the idea is that the couple pours everything into the strengthening of their bond and the spark of a new life.");
            listingStandard.Label("Remember that accumulated psyfocus decays over time. If you set the threshold VERY high then your psycaster could fall below the limit when he stops for a snack enroute to the ritual site. It's okay to set a high threshold for thematic reasons but then reduce it slightly for gameplay reasons.");
            listingStandard.Label("If you feel that this feature is lore-unfriendly and/or overpowered, then you can simply leave it out of your ideoligion. Like the breeding ritual, psybreeding will NEVER be randomly included during Ideoligion generation. If you want to use it but feel it's slightly overpowered, then you can make it riskier by adding Neural Heat. The slider below specifies the TOTAL amount accrued over the ritual - therefore a long ritual will be safer because it allows more time for Heat to dissipate.");
            listingStandard.Label("When using neural heat, don't forget the limiter switch! The ritual will automatically be cancelled if a participant reaches their limit with their safety switch ON. Flicking the switch OFF may allow the ritual to finish - but your psycaster could suffer injuries as a result. Any gear, genes, or effects which reduce incoming Neural Heat (such as eltex robes) will make the ritual more manageable.");
            listingStandard.CheckboxLabeled("Require psybond?", ref BreedingRitualSettings.psybondRequired, "This ritual is based on the concept of psychic bonding, but that Gene is quite rare in the vanilla game. If you're running mods then it may be more accessible. There's also a debug option (scroll to bottom) which will psybond a couple for testing purposes. Uncheck this box if you want the ritual to be available to any couple regardless of genes or bonds.");
            listingStandard.CheckboxLabeled("Psyboosted fertility?", ref BreedingRitualSettings.psyboostFertility, "This ritual calls on the couple to unite their minds, focusing intensively on the goal of conception. To reflect this, the mod applies a fertility boost equal to the sum of their Psychic Sensitivity stats. A couple with normal sensitivity would receive a 200% boost. Note that the Breeding Ritual policy (such as mandatory 'Try for Baby') applies to this ritual as well. Uncheck this box to omit the special psy-boost.");
            listingStandard.CheckboxLabeled("Spectators meditate?", ref BreedingRitualSettings.psybreedingMeditation, "Spectators can be made to meditate instead of dancing. Please remember to create a few Meditation Spots near the ritual bed (the spots don't need to be assigned; spectators will grab them opportunistically). Spectator meditation does not influence ritual quality; it's intended to be used for aesthetic or thematic purposes.");
            listingStandard.CheckboxLabeled("Psychic ripple?", ref BreedingRitualSettings.psybreedingRipple, "The psybreeding ritual can display a visual \"Ripple\" effect, similar to the Bestowing ceremony. This is purely cosmetic; it has no impact on the participants or spectators. Uncheck this box to omit it.");
            BreedingRitualSettings.psybreedingDurationTicks = listingStandard.SliderLabeled("Duration: " + (BreedingRitualSettings.psybreedingDurationTicks / 2500f).ToString("0.0") + " hours", BreedingRitualSettings.psybreedingDurationTicks, 2500f, 25000f);
            BreedingRitualSettings.psyfocusMinimum = listingStandard.SliderLabeled("Psyfocus needed to begin: " + (BreedingRitualSettings.psyfocusMinimum).ToStringPercent("0.0"), BreedingRitualSettings.psyfocusMinimum, 0f, 0.99f);
            BreedingRitualSettings.neuralHeatPsybreeding = listingStandard.SliderLabeled("Neural heat: " + (BreedingRitualSettings.neuralHeatPsybreeding).ToString("0"), BreedingRitualSettings.neuralHeatPsybreeding, 0f, 1000f);
            listingStandard.GapLine();

            listingStandard.SubLabel("Miscellaneous options", 1f);
            listingStandard.CheckboxLabeled("Penalize repetition?", ref BreedingRitualSettings.repetitionPenalty, "Many rituals in Rimworld impose a severe penalty to quality if they're repeated within 20 days of the previous occurrence. By default the Breeding ritual is immune to this effect, because you're expected to 'pay' with Leader ability charges. Check this box if you want to be penalized for ritual spam.");
            listingStandard.CheckboxLabeled("Respect pawn sexuality?", ref BreedingRitualSettings.respectPawnSexuality, "Pawns are forced to do Lovin' during a breeding ritual, regardless of their preferences. Checking this box will add a pre-check which cancels the ritual if it would contravene a pawn's sexuality. This means that Gay and Asexual pawns will be excluded from participation (although they're welcome to spectate).");
            listingStandard.CheckboxLabeled("Announce pregnancy chance?", ref BreedingRitualSettings.announcePregnancyChance, "At the start of a ceremony, the mod will send a message to announce the percentage likelihood of conception (per round of Lovin'). Uncheck this box to silence the mod.");
            listingStandard.CheckboxLabeled("Detailed fertility report?", ref BreedingRitualSettings.fertilityReportLetter, "As with most rituals in RimWorld, you'll receive a Letter at the end of the ritual describing the various factors which influenced its quality. The mod can extend this letter, providing numerical details about fertility and performance. These numbers might harm your sense of immersion; feel free to uncheck this box if you'd rather preserve a bit of mystery.");
            listingStandard.CheckboxLabeled("Allow pregnant women?", ref BreedingRitualSettings.allowPregnantWomen, "By default, a pregnant woman is ineligible to participate (though she's welcome to spectate). This is intended to protect the player from accidentally wasting resources on a redundant ceremony. However, some mods might allows for 'super pregnancy' - or players might want to do the ritual for storytelling reasons. Check this box if you think a pregnant woman should be treated as a normal breeding candidate.");
            listingStandard.CheckboxLabeled("Boss gets memory?", ref BreedingRitualSettings.organizerRemembersRitual, "Participants and spectators will gain a memory about the ritual, providing a small boost or penalty to Mood. If an Ideoligion specialist (Leader or Moral Guide) 'spent' their special abilities in order to start the ritual, then we can grant them this memory as well. Obviously this is redundant if they participate or spectate; the checkbox option will grant them a memory even if they DO NOT attend the ritual. Uncheck this box if memories should be given ONLY to attendees.");
            listingStandard.CheckboxLabeled("Play instruments?", ref BreedingRitualSettings.playInstruments, "By default, half of the spectators will attempt to play nearby instruments while the other half will dance. This has no impact on ritual quality; it's just done for thematic reasons. Uncheck this box if you want ALL of the spectators to dance.");
            listingStandard.CheckboxLabeled("Play ideoligion music?", ref BreedingRitualSettings.playIdeoMusic, "Rituals can generate background music which matches your Ideoligion. This is disabled by default for the breeding ritual, because it's assumed that your pawns will play instruments. Check this box to activate the standard music.");
            listingStandard.CheckboxLabeled("Show ideoligion VFX?", ref BreedingRitualSettings.showIdeoVFX, "Rituals usually generate a visual overlay at the ritual site, and a few props are drawn nearby which represent the theme of your ideoligion (such as totem poles). These tend to work well for a grandiose ceremony, but they may appear silly if you're organizing a breeding ritual within a small bedroom. Uncheck this box to disable the visual effects.");
            listingStandard.CheckboxLabeled("Farewell huddle?", ref BreedingRitualSettings.finalGathering, "When the participants have finished Lovin', spectators will approach to offer congratulations and prayers. This has no effect on ritual quality; it's just done for thematic reasons. Uncheck this box if you'd prefer to see them to dance and play music until the end of the ritual.");
            listingStandard.CheckboxLabeled("Gradual dispersal?", ref BreedingRitualSettings.gradualDispersal, "The ritual concludes with spectators approaching to offer their blessings, while the couple cuddles together. This is supposed to be a peaceful conclusion, so it looks bad if everyone sprints away simultaneously. By default, spectators will sneak away individually as the ceremony winds down - leaving the couple alone to nap. Uncheck this box if you want to maintain a strict attendance policy.");
            listingStandard.CheckboxLabeled("Bed usage tolerance?", ref BreedingRitualSettings.bedUsageTolerance, "RimWorld enforces many rules regarding propriety of bed usage. These rules tend to block mingling across social lines (colonists - slaves - guests - prisoners). The mod can override all of these, ensuring that a pawn can always USE the bed to which he's been ASSIGNED. You may need to use tricks and mods to assign one of your people to a prisoner's bed, but with this option you'll be able to make the assigned couple perform a Breeding Ritual. Uncheck if you'd rather use standard RimWorld rules. If you uncheck this option then trying to force Lovin' across social categories will probably generate errors.");
            listingStandard.CheckboxLabeled("Allow guest breeding?", ref BreedingRitualSettings.seduceGuests, "When a group of Hospitality guests visits your colony, they're treated as 'belonging' to a Lord (an entity which directs the visit). In order for a Guest to participate in Breeding we must emancipate the Guest from their Lord; the mod does this automatically. We return them to their Lord when the breeding ritual is over, but the process isn't perfect. The Guest will wander around idly for a while before resuming normal Guest behavior. If their group departed during the breeding, the Guest will leave independently (similar to an injured guest who needed to convalesce in your hospital). If you try to breed the ENTIRE visiting party then things might get weird; I would advise against doing that. Uncheck this box to make Guests ineligible for breeding.");
            listingStandard.GapLine();

            listingStandard.SubLabel("Debug tools", 1f);
            listingStandard.Label("The recommended usage of this mod is:\n1.Start a new game\n2.Create a new ideoligion\n3.Add the breeding ritual to your ideoligion\n4.Playtest for a while (you can try the ritual immediately - just setup a Double Sleeping Spot on the grass)\n5.Decide whether you want to keep the mod or remove it");
            listingStandard.Label("If you want to continue using the mod, but you don't want to start over and lose progress, try the button below. It will attempt to identify your colony's dominant ideoligion, and then inject a breeding ritual into that ideoligion's precepts. Obviously you must load a savegame before doing this. I encourage you to make a backup of your savegame file before clicking the button, just in case something goes wrong.");
            listingStandard.Label("Note that your ability to USE this ritual depends on the rules of your ideoligion. Total prohibition of physical love means pawns can't share beds, so you'll never be able to start the ritual. In that case, you'd need to generate a new colony.");
            listingStandard.Label("There's another button below to remove the ritual. This could be used temporarily for GUI tidiness (\"I don't want to see that Breeding gizmo appear whenever I click a bed! I'll re-add the ritual when I'm ready to perform a ceremony!\"). But the expected usage is to PERMANENTLY remove the ritual from a savegame prior to uninstalling the mod. Doing so should ensure that you don't receive any red error messages re: missing RitualDefs.");
            if (listingStandard.ButtonText("Add Breeding Ritual", null, 0.25f)) { AddRemoveRitual("Breeding"); }
            if (listingStandard.ButtonText("Add Psybreeding Ritual", null, 0.25f)) { AddRemoveRitual("Psybreeding"); }
            if (listingStandard.ButtonText("Remove Breeding Ritual", null, 0.25f)) { AddRemoveRitual("Breeding", false); }
            if (listingStandard.ButtonText("Remove Psybreeding Ritual", null, 0.25f)) { AddRemoveRitual("Psybreeding", false); }
            if (listingStandard.ButtonText("Bond this couple", null, 0.25f)) { BondPawns(); }
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
            try
            {
                UpdateRitualDefs();
            }
            catch { }
            
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
                        if (localBreedingRitual != null)
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
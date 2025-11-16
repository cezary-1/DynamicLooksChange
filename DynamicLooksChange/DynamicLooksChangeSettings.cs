using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace DynamicLooksChange
{

    public sealed class DynamicLooksChangeSettings : AttributeGlobalSettings<DynamicLooksChangeSettings>
    {
        public override string Id => "DynamicLooksChange";
        public override string DisplayName => new TextObject("{=DLC_TITLE}Dynamic Looks & Change").ToString();
        public override string FolderName => "DynamicLooksChange";
        public override string FormatType => "json";

        //── General ────────────────────────────────────────────────────────

        [SettingPropertyInteger(
            "{=DLC_ChangeInterval}Days Between Checks",
            1, 81, Order = 0, RequireRestart = false,
            HintText = "{=DLC_ChangeInterval_H}How many days elapse before we re‑run hair/body/tattoo changes.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General", GroupOrder = 0)]
        public int ChangeIntervalDays { get; set; } = 1;

        [SettingPropertyInteger(
            "{=DLC_MaxHeroes}Max Heroes Per Check",
            1, 10000, Order = 1, RequireRestart = false,
            HintText = "{=DLC_MaxHeroes_H}How many (random) heroes to process each interval.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General")]
        public int ChangeHeroMax { get; set; } = 50;

        [SettingPropertyBool(
            "{=DLC_SecondGrow}Use Second Grow Mode",
            Order = 13, RequireRestart = false,
            HintText = "{=DLC_SecondGrow_H}When growing, allow skipping intermediate buckets.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_HAIR}Hair")]
        public bool SecondGrowMode { get; set; } = false;

        [SettingPropertyBool(
            "{=DLC_SecondShave}Use Second Shave Mode",
            Order = 14, RequireRestart = false,
            HintText = "{=DLC_SecondShave_H}When shaving, allow skipping intermediate buckets.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_HAIR}Hair")]
        public bool SecondShaveMode { get; set; } = false;

        [SettingPropertyFloatingInteger(
            "{=DLC_BuildMinChange}Min Build Change",
            0f, 1f, Order = 2, RequireRestart = false,
            HintText = "{=DLC_BuildMinChange_H}Minimum fraction to change build.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_BODY}Body")]
        public float BuildMin { get; set; } = 0.10f;

        [SettingPropertyFloatingInteger(
            "{=DLC_BuildMaxChange}Max Build Change",
            0f, 1f, Order = 3, RequireRestart = false,
            HintText = "{=DLC_BuildMaxChange_H}Maximum fraction to change build.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_BODY}Body")]
        public float BuildMax { get; set; } = 1.00f;

        [SettingPropertyFloatingInteger(
            "{=DLC_WeightMinChange}Min Weight Change",
            0f, 1f, Order = 6, RequireRestart = false,
            HintText = "{=DLC_WeightMinChange_H}Minimum fraction to change weight.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_BODY}Body")]
        public float WeightMin { get; set; } = 0.10f;

        [SettingPropertyFloatingInteger(
            "{=DLC_WeightMaxChange}Max Weight Change",
            0f, 1f, Order = 7, RequireRestart = false,
            HintText = "{=DLC_WeightMaxChange_H}Maximum fraction to change weight.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_BODY}Body")]
        public float WeightMax { get; set; } = 1.00f;

        //── Scars ────────────────────────────────────────────────────────

        [SettingPropertyBool(
            "{=DLC_ScarPermanent}Scar Permanent",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_ScarPermanent_H}If true, scars never get overwritten.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_TATTOO}Tattoo/{=MCM_SCAR}Scar")]
        public bool ScarPermament { get; set; } = true;

        [SettingPropertyFloatingInteger(
            "{=DLC_ScarProb}Scar Chance",
            0f, 1f, "#0%",
            Order = 1, RequireRestart = false,
            HintText = "{=DLC_ScarProb_H}Chance wounded hero gains a scar.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_TATTOO}Tattoo/{=MCM_SCAR}Scar")]
        public float ScarProbability { get; set; } = 0.2f;

        [SettingPropertyText(
            "{=DLC_ScarsMale}Male Scar Indexes",
            Order = 2, RequireRestart = false,
            HintText = "{=DLC_ScarsMale_H}Comma‑separate allowed scar IDs for men.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_TATTOO}Tattoo/{=MCM_SCAR}Scar")]
        public string ScarsMaleCsv { get; set; } = "14,15,16,17,18,19,20,21,22,23,24,25,26,29,30";

        [SettingPropertyText(
            "{=DLC_ScarsFemale}Female Scar Indexes",
            Order = 3, RequireRestart = false,
            HintText = "{=DLC_ScarsFemale_H}Comma‑separate allowed scar IDs for women.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_TATTOO}Tattoo/{=MCM_SCAR}Scar")]
        public string ScarsFemaleCsv { get; set; } = "24,25,26,27,28,29,30,31,32";

        //── Filters ────────────────────────────────────────────────────────

        [SettingPropertyBool(
            "{=DLC_AllowNotable}Allow Notable NPCs",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_AllowNotable_H}If false, named NPCs are skipped.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_FILTERS}Filters")]
        public bool AllowNotable { get; set; } = false;

        [SettingPropertyBool(
            "{=DLC_AllowPlayer}Allow Player",
            Order = 1, RequireRestart = false,
            HintText = "{=DLC_AllowPlayer_H}If false, the player’s own character is never changed.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_FILTERS}Filters")]
        public bool AllowPlayer { get; set; } = true;

        [SettingPropertyBool(
            "{=DLC_AllowOthers}Allow Others",
            Order = 2, RequireRestart = false,
            HintText = "{=DLC_AllowOthers_H}If false, only player & player‑clan members are affected.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_FILTERS}Filters")]
        public bool AllowOthers { get; set; } = true;

        [SettingPropertyBool(
            "{=DLC_AllowPlayerClan}Allow Player‑Clan",
            Order = 3, RequireRestart = false,
            HintText = "{=DLC_AllowPlayerClan_H}If false, members of your clan are skipped.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_FILTERS}Filters")]
        public bool AllowPlayerClan { get; set; } = true;

        [SettingPropertyBool(
            "{=DLC_AllowWanderer}Allow Wanderers",
            Order = 4, RequireRestart = false,
            HintText = "{=DLC_AllowWanderer_H}If false, stray wanderers are skipped.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_FILTERS}Filters")]
        public bool AllowWanderer { get; set; } = true;

        // ──────────── Age Modifiers ────────────

        [SettingPropertyFloatingInteger(
            "{=DLC_AgeGrow}Age Grow Modifier",
            0f, 1f, Order = 20, RequireRestart = false,
            HintText = "{=DLC_AgeGrow_H}How strongly age reduces hair‑grow chance.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_AGE_MODIFIERS}Age Modifiers")]
        public float AgeGrow { get; set; } = 0.01f;

        [SettingPropertyFloatingInteger(
            "{=DLC_AgeShave}Age Shave Modifier",
            0f, 1f, Order = 21, RequireRestart = false,
            HintText = "{=DLC_AgeShave_H}How strongly age reduces hair‑shave chance.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_AGE_MODIFIERS}Age Modifiers")]
        public float AgeShave { get; set; } = 0.01f;

        [SettingPropertyFloatingInteger(
            "{=DLC_AgeGain}Age Build/Weight Gain Modifier",
            0f, 1f, Order = 22, RequireRestart = false,
            HintText = "{=DLC_AgeGain_H}How strongly age reduces build/weight‑gain.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_AGE_MODIFIERS}Age Modifiers")]
        public float AgeGain { get; set; } = 0.01f;

        [SettingPropertyFloatingInteger(
            "{=DLC_AgeLoss}Age Build/Weight Loss Modifier",
            0f, 1f, Order = 23, RequireRestart = false,
            HintText = "{=DLC_AgeLoss_H}How strongly age reduces build/weight‑loss.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_AGE_MODIFIERS}Age Modifiers")]
        public float AgeLoss { get; set; } = 0.01f;

        // ──────────── Call Barber ────────────

        [SettingPropertyBool(
            "{=DLC_CallBarber}Enable 'Call Barber' Option",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_CallBarber_H}If true, in village/castle/town you will be able to call for barber.")]
        [SettingPropertyGroup("{=MCM_GENERAL}General/{=MCM_BARBER}Barber")]
        public bool EnableBarber { get; set; } = true;

        // ──────────── Diagnostics & Notifications ────────────
        [SettingPropertyBool(
            "{=DLC_Debug}Enable Debug Messages",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_Debug_H}Toggle verbose debug.")]
        [SettingPropertyGroup("{=MCM_NOTIFICATIONS}Notifications")]
        public bool Debug { get; set; } = false;

        [SettingPropertyBool(
            "{=DLC_InformClan}Inform Player Clan Changes",
            Order = 1, RequireRestart = false,
            HintText = "{=DLC_InformClan_H}Notify when your clan NPCs update.")]
        [SettingPropertyGroup("{=MCM_NOTIFICATIONS}Notifications")]
        public bool InformPlayerClan { get; set; } = true;

        [SettingPropertyBool(
            "{=DLC_InformPlayer}Inform You",
            Order = 2, RequireRestart = false,
            HintText = "{=DLC_InformPlayer_H}Notify when *you* change.")]
        [SettingPropertyGroup("{=MCM_NOTIFICATIONS}Notifications")]
        public bool InformPlayer { get; set; } = true;

        [SettingPropertyBool(
            "{=DLC_ShowSettingsOnLoad}Show Settings on Load",
            Order = 3, RequireRestart = false,
            HintText = "{=DLC_ShowSettingsOnLoad_H}Print config values at game start.")]
        [SettingPropertyGroup("{=MCM_NOTIFICATIONS}Notifications")]
        public bool StatInfo { get; set; } = true;

        // Value is displayed as a percentage
        [SettingPropertyButton("{=DLC_CheckHairs}Check Heroes", Content = "{=DLC_CheckHairs}Check", Order = 2, RequireRestart = false, HintText = "{=DLC_CheckHairs_H}Force change heroes hair if the ones they have are in their exclude lists")]
        [SettingPropertyGroup("{=MCM_CHECK_HEROES_HAIR}Check Heroes Hair", GroupOrder = 10)]
        public Action ChangeHairCulture
        {
            get;
            set;
        } = () =>
        {
            var b = DynamicLooksChangeBehavior.Instance;
            if (b == null) return;
            var s = Instance;
            if (s == null) return;
            var perClan = DynamicLooksChangeSettingsPerCulture.Instance;

            int changed = 0;

            var heroes = Hero.AllAliveHeroes
                .Where(c => !c.IsChild)
                .Where(c => s.AllowOthers || c == Hero.MainHero || c.Clan == Clan.PlayerClan)
                .Where(c => s.AllowNotable || !c.IsNotable)
                .Where(c => s.AllowWanderer || !c.IsWanderer)
                .Where(c => s.AllowPlayer || c != Hero.MainHero)
                .Where(c => s.AllowPlayerClan || c.Clan != Clan.PlayerClan)
                .ToList();

            foreach (var h in heroes)
            {
                var cultureId = h.Culture.StringId; // maps use case-insensitive comparer
                var config = perClan.LooksList.FirstOrDefault(d => d.Culture == cultureId) ?? perClan.LooksList.FirstOrDefault(d => d.Culture == "Default") ?? new LooksConfig();

                var faceParams = default(FaceGenerationParams);
                MBBodyProperties.GetParamsFromKey(
                    ref faceParams,
                    h.BodyProperties,
                    earsAreHidden: false,
                    mouthHidden: false
                );

                int hairIndex = faceParams.CurrentHair;
                int beardIndex = faceParams.CurrentBeard;
                int tattooIndex = faceParams.CurrentFaceTattoo;

                if (h.IsFemale)
                {
                    var femHair = DynamicLooksChangeBehavior.GetHairForGender(h.IsFemale, config, false);

                    // Was current hair removed by exclusions? (i.e. not present in any allowed bucket)
                    var stillAllowedContainsCurrent = femHair.Values.Any(list => list.Contains(hairIndex));
                    if (!stillAllowedContainsCurrent)
                    {
                        // flattened candidates (excluding current index)
                        var candidates = femHair.SelectMany(kv => kv.Value).Where(i => i != hairIndex).Distinct().ToList();
                        if (candidates.Count > 0)
                        {
                            var newHair = candidates.GetRandomElement();
                            h.ModifyHair(newHair, beardIndex, tattooIndex);
                            changed++;
                        }
                        else
                        {
                            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"[DLC] No replacement candidates after exclusion for {h.Name} (female, culture {cultureId})", Colors.Magenta));
                        }
                    }
                }
                else
                {
                    var menHair = DynamicLooksChangeBehavior.GetHairForGender(h.IsFemale, config, false);

                    var headStillAllowed = menHair.Values.Any(list => list.Contains(hairIndex));
                    if (!headStillAllowed)
                    {
                        var headCandidates = menHair.SelectMany(kv => kv.Value).Where(i => i != hairIndex).Distinct().ToList();
                        if (headCandidates.Count > 0)
                        {
                            var newHair = headCandidates.GetRandomElement();
                            h.ModifyHair(newHair, beardIndex, tattooIndex);
                            changed++;
                        }
                        else
                        {
                            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"[DLC] No head-hair replacement after exclusion for {h.Name} (culture {cultureId})", Colors.Magenta));
                        }
                    }

                    var faceHair = DynamicLooksChangeBehavior.GetHairForGender(h.IsFemale, config, true);

                    var facialStillAllowed = faceHair.Values.Any(list => list.Contains(beardIndex));
                    if (!facialStillAllowed)
                    {
                        var facialCandidates = faceHair.SelectMany(kv => kv.Value).Where(i => i != beardIndex).Distinct().ToList();
                        if (facialCandidates.Count > 0)
                        {
                            var newBeard = facialCandidates.GetRandomElement();
                            h.ModifyHair(hairIndex, newBeard, tattooIndex);
                            changed++;
                        }
                        else
                        {
                            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"[DLC] No facial replacement after exclusion for {h.Name} (culture {cultureId})", Colors.Magenta));
                        }
                    }
                }
            }

            InformationManager.DisplayMessage(
                new InformationMessage($"Changed {changed} heroes hairs", Colors.Green)
            );
        };

    }
}

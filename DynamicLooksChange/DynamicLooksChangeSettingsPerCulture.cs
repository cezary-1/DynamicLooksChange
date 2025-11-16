using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DynamicLooksChange
{
    //Config
    public class LooksConfig
    {
        public string Culture { get; set; } = "Default";
        public float HairGrowProbability { get; set; } = 0.30f;
        public float HairShaveProbability { get; set; } = 0.15f;
        public float FacialHairGrowProbability { get; set; } = 0.25f;

        // ──────────── Facial Hair Buckets (Male) ────────────
        public string ManFacial_None_Csv { get; set; } = "0";
        public string ManFacial_VeryShort_Csv { get; set; } = "1,11,23,25,26,27";
        public string ManFacial_Short_Csv { get; set; } = "2,3,4,5,6,7,10,12,24,31,32,33";
        public string ManFacial_Medium_Csv { get; set; } = "8,9,13,20,21,22,28,29,35,36";
        public string ManFacial_Long_Csv { get; set; } = "14,15,30,34,37,38";
        public string ManFacial_VeryLong_Csv { get; set; } = "16,17,18,19,39,40,41";

        // ──────────── Hair Buckets (Male) ────────────
        public string ManHair_None_Csv { get; set; } = "0";
        public string ManHair_VeryShort_Csv { get; set; } = "6,8,10,12,13,14,15,17,18,26,27,28";
        public string ManHair_Short_Csv { get; set; } = "4,5,9";
        public string ManHair_Medium_Csv { get; set; } = "1,3,11,19,25";
        public string ManHair_Long_Csv { get; set; } = "2,7,16,20,24";
        public string ManHair_VeryLong_Csv { get; set; } = "21,22,23";

        // ──────────── Hair Buckets (Female) ────────────
        public string FemHair_None_Csv { get; set; } = "0";
        public string FemHair_VeryShort_Csv { get; set; } = "9,20";
        public string FemHair_Short_Csv { get; set; } = "7,12,21";
        public string FemHair_Medium_Csv { get; set; } = "5,6,11,14,16,19";
        public string FemHair_Long_Csv { get; set; } = "3,8,15,18";
        public string FemHair_VeryLong_Csv { get; set; } = "1,2,4,10,13,17";


        //── Body ───────────────────────────────────────────────────────────
        public float BuildGainProbability { get; set; } = 0.20f;
        public float BuildLossProbability { get; set; } = 0.20f;
        public float WeightGainProbability { get; set; } = 0.20f;
        public float WeightLossProbability { get; set; } = 0.20f;

        //── Tattoo & Scar ──────────────────────────────────────────────────
        public float TattooProbability { get; set; } = 0.20f;
        public string TattooMaleCsv { get; set; }
            = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,27,28";
        public string TattooFemaleCsv { get; set; }
            = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23";


        // ──────────── Color‑Chances ────────────
        public float HairColorProbability { get; set; } = 0f;
        public int HairColorFilter { get; set; } = 0;
        public float TattooColorProbability { get; set; } = 0.10f;
        public int TattooColorFilter { get; set; } = 0;

        public override string ToString()
            => new TextObject("{=DLC_" + Culture + "}" + Culture)
            .ToString();
    }

    public class ConfigData
    {
        public List<LooksConfig> Looks { get; set; } = new List<LooksConfig>();
    }


    public sealed class DynamicLooksChangeSettingsPerCulture : AttributeGlobalSettings<DynamicLooksChangeSettingsPerCulture>
    {
        public override string Id => "DynamicLooksChangePerCulture";
        public override string DisplayName => new TextObject("{=DLC_PERCULTURE_TITLE}Dynamic Looks & Change Per Culture").ToString();
        public override string FolderName => "DynamicLooksChangePerCulture";
        public override string FormatType => "json";

        //config
        private const string FILE_NAME = "editor_config.json";

        private readonly string _editorFolder;
        private readonly string _filePath;

        // In‑memory lists of configs
        public List<LooksConfig> LooksList { get; private set; }

        public DynamicLooksChangeSettingsPerCulture()
        {
            var asm = DynamicLooksChangeModule._module;
            _editorFolder = Path.Combine(Path.GetDirectoryName(asm), "editor");
            Directory.CreateDirectory(_editorFolder);
            _filePath = Path.Combine(_editorFolder, FILE_NAME);

            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonConvert.DeserializeObject<ConfigData>(json)
                        ?? new ConfigData();
                LooksList = data.Looks;
            }
            else
            {
                LooksList = new List<LooksConfig>
                {
                    new LooksConfig
                    {
                        Culture = "Default",
                        HairGrowProbability = 0.3f,
                        HairShaveProbability = 0.15f,
                        FacialHairGrowProbability = 0.25f,
                        ManFacial_None_Csv = "0",
                        ManFacial_VeryShort_Csv = "1,11,23,25,26,27",
                        ManFacial_Short_Csv = "2,3,4,5,6,7,10,12,24,31,32,33",
                        ManFacial_Medium_Csv = "8,9,13,20,21,22,28,29,35,36",
                        ManFacial_Long_Csv = "14,15,30,34,37,38",
                        ManFacial_VeryLong_Csv = "16,17,18,19,39,40,41",
                        ManHair_None_Csv = "0",
                        ManHair_VeryShort_Csv = "6,8,10,12,13,14,15,17,18,26,27,28",
                        ManHair_Short_Csv = "4,5,9",
                        ManHair_Medium_Csv = "1,3,11,19,25",
                        ManHair_Long_Csv = "2,7,16,20,24",
                        ManHair_VeryLong_Csv = "21,22,23",
                        FemHair_None_Csv = "0",
                        FemHair_VeryShort_Csv = "9,20",
                        FemHair_Short_Csv = "7,12,21",
                        FemHair_Medium_Csv = "5,6,11,14,16,19",
                        FemHair_Long_Csv = "3,8,15,18",
                        FemHair_VeryLong_Csv = "1,2,4,10,13,17",
                        BuildGainProbability = 0.2f,
                        BuildLossProbability = 0.2f,
                        WeightGainProbability = 0.2f,
                        WeightLossProbability = 0.2f,
                        TattooProbability = 0.2f,
                        TattooMaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,27,28",
                        TattooFemaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23",
                        HairColorProbability = 0f,
                        HairColorFilter = 0,
                        TattooColorProbability = 0.1f,
                        TattooColorFilter = 0

                    },
                    new LooksConfig
                    {
                        Culture = "aserai",
                        HairGrowProbability = 0.4f,
                        HairShaveProbability = 0.05f,
                        FacialHairGrowProbability = 0.35f,
                        ManFacial_None_Csv = "0",
                        ManFacial_VeryShort_Csv = "11,23,25,26,27",
                        ManFacial_Short_Csv = "2,3,4,5,6,7,10,12,24,31,32,33",
                        ManFacial_Medium_Csv = "8,9,13,28,29,35,36",
                        ManFacial_Long_Csv = "14,15,30,34,37,38",
                        ManFacial_VeryLong_Csv = "16,17,18,39,40,41",
                        ManHair_None_Csv = "0",
                        ManHair_VeryShort_Csv = "6,8,13,14,15,18,26",
                        ManHair_Short_Csv = "4,5,9",
                        ManHair_Medium_Csv = "1,3,11,19,25",
                        ManHair_Long_Csv = "2,20,24",
                        ManHair_VeryLong_Csv = "21,22,23",
                        FemHair_None_Csv = "0",
                        FemHair_VeryShort_Csv = "20",
                        FemHair_Short_Csv = "7,12",
                        FemHair_Medium_Csv = "5,6,11,14,16,19",
                        FemHair_Long_Csv = "3,8,18",
                        FemHair_VeryLong_Csv = "1,2,4,10,13,17",
                        BuildGainProbability = 0.2f,
                        BuildLossProbability = 0.2f,
                        WeightGainProbability = 0.3f,
                        WeightLossProbability = 0.1f,
                        TattooProbability = 0.05f,
                        TattooMaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,27,28",
                        TattooFemaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23",
                        HairColorProbability = 0f,
                        HairColorFilter = 0,
                        TattooColorProbability = 0.1f,
                        TattooColorFilter = 0

                    },
                    new LooksConfig
                    {
                        Culture = "battania",
                        HairGrowProbability = 0.4f,
                        HairShaveProbability = 0.05f,
                        FacialHairGrowProbability = 0.35f,
                        ManFacial_None_Csv = "0",
                        ManFacial_VeryShort_Csv = "11,23,25,26,27",
                        ManFacial_Short_Csv = "2,3,4,5,6,7,10,12,24,31,32,33",
                        ManFacial_Medium_Csv = "8,9,13,28,29,35,36",
                        ManFacial_Long_Csv = "14,15,30,34,37,38",
                        ManFacial_VeryLong_Csv = "16,17,18,39,40,41",
                        ManHair_None_Csv = "0",
                        ManHair_VeryShort_Csv = "6,8,10,13,14,15,18,26",
                        ManHair_Short_Csv = "4,5,9",
                        ManHair_Medium_Csv = "1,3,11,19,25",
                        ManHair_Long_Csv = "2,7,16,20",
                        ManHair_VeryLong_Csv = "",
                        FemHair_None_Csv = "0",
                        FemHair_VeryShort_Csv = "9,20",
                        FemHair_Short_Csv = "7,12",
                        FemHair_Medium_Csv = "5,6,11,14,16",
                        FemHair_Long_Csv = "3,8,15",
                        FemHair_VeryLong_Csv = "1,2,4,10,13",
                        BuildGainProbability = 0.3f,
                        BuildLossProbability = 0.1f,
                        WeightGainProbability = 0.1f,
                        WeightLossProbability = 0.3f,
                        TattooProbability = 0.3f,
                        TattooMaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,27,28",
                        TattooFemaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23",
                        HairColorProbability = 0f,
                        HairColorFilter = 0,
                        TattooColorProbability = 0.1f,
                        TattooColorFilter = 0
                    },
                    new LooksConfig
                    {
                        Culture = "empire",
                        HairGrowProbability = 0.3f,
                        HairShaveProbability = 0.25f,
                        FacialHairGrowProbability = 0.25f,
                        ManFacial_None_Csv = "0",
                        ManFacial_VeryShort_Csv = "11,23,25,26,27",
                        ManFacial_Short_Csv = "2,3,4,5,6,7,10,12,24,31,32,33",
                        ManFacial_Medium_Csv = "8,9,13,28,29,35,36",
                        ManFacial_Long_Csv = "14,15,30,34,37,38",
                        ManFacial_VeryLong_Csv = "16,17,18,39,40,41",
                        ManHair_None_Csv = "0",
                        ManHair_VeryShort_Csv = "6,8,13,14,15,26",
                        ManHair_Short_Csv = "4,5,9",
                        ManHair_Medium_Csv = "1,3,11,25",
                        ManHair_Long_Csv = "",
                        ManHair_VeryLong_Csv = "",
                        FemHair_None_Csv = "0",
                        FemHair_VeryShort_Csv = "20",
                        FemHair_Short_Csv = "7,12",
                        FemHair_Medium_Csv = "5,6,11,14,16",
                        FemHair_Long_Csv = "3",
                        FemHair_VeryLong_Csv = "1,2,4,10,13",
                        BuildGainProbability = 0.2f,
                        BuildLossProbability = 0.2f,
                        WeightGainProbability = 0.2f,
                        WeightLossProbability = 0.2f,
                        TattooProbability = 0f,
                        TattooMaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,27,28",
                        TattooFemaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23",
                        HairColorProbability = 0.0f,
                        HairColorFilter = 0,
                        TattooColorProbability = 0.1f,
                        TattooColorFilter = 0

                    },
                  new LooksConfig
                    {
                        Culture = "khuzait",
                        HairGrowProbability = 0.3f,
                        HairShaveProbability = 0.15f,
                        FacialHairGrowProbability = 0.25f,
                        ManFacial_None_Csv = "0",
                        ManFacial_VeryShort_Csv = "1,11,23,25,26,27",
                        ManFacial_Short_Csv = "2,3,4,5,6,7,10,12,24,31,32,33",
                        ManFacial_Medium_Csv = "8,9,13,20,28,29,35,36",
                        ManFacial_Long_Csv = "14,15,30,34,37,38",
                        ManFacial_VeryLong_Csv = "16,17,18,19,39,40,41",
                        ManHair_None_Csv = "0",
                        ManHair_VeryShort_Csv = "6,8,10,13,14,15,17,18,26,27,28",
                        ManHair_Short_Csv = "4,5,9",
                        ManHair_Medium_Csv = "1,3,11,19,25",
                        ManHair_Long_Csv = "7,20,24",
                        ManHair_VeryLong_Csv = "21,22,23",
                        FemHair_None_Csv = "0",
                        FemHair_VeryShort_Csv = "9,20",
                        FemHair_Short_Csv = "7,12",
                        FemHair_Medium_Csv = "5,6,11,14,16",
                        FemHair_Long_Csv = "3",
                        FemHair_VeryLong_Csv = "1,2,4,10,13",
                        BuildGainProbability = 0.1f,
                        BuildLossProbability = 0.3f,
                        WeightGainProbability = 0.1f,
                        WeightLossProbability = 0.3f,
                        TattooProbability = 0.2f,
                        TattooMaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,27,28",
                        TattooFemaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23",
                        HairColorProbability = 0.0f,
                        HairColorFilter = 0,
                        TattooColorProbability = 0.1f,
                        TattooColorFilter = 0

                    },
                    /* sturgia (based on Default + modifiers + excludes) */
                    new LooksConfig
                    {
                        Culture = "sturgia",
                        HairGrowProbability = 0.4f,
                        HairShaveProbability = 0.05f,
                        FacialHairGrowProbability = 0.35f,
                        ManFacial_None_Csv = "0",
                        ManFacial_VeryShort_Csv = "11,23,25,26,27",
                        ManFacial_Short_Csv = "2,3,4,5,6,7,10,12,24,31,32,33",
                        ManFacial_Medium_Csv = "8,9,13,20,21,22,28,29,35,36",
                        ManFacial_Long_Csv = "14,15,30,34,37,38",
                        ManFacial_VeryLong_Csv = "16,17,18,19,39,40,41",
                        ManHair_None_Csv = "0",
                        ManHair_VeryShort_Csv = "6,8,10,13,14,15,18,26",
                        ManHair_Short_Csv = "4,5,9",
                        ManHair_Medium_Csv = "1,3,11,19,25",
                        ManHair_Long_Csv = "2,7,20",
                        ManHair_VeryLong_Csv = "",
                        FemHair_None_Csv = "0",
                        FemHair_VeryShort_Csv = "9,20",
                        FemHair_Short_Csv = "7,12",
                        FemHair_Medium_Csv = "5,6,11,14,16",
                        FemHair_Long_Csv = "3,8,15",
                        FemHair_VeryLong_Csv = "1,2,4,10,13",
                        BuildGainProbability = 0.3f,
                        BuildLossProbability = 0.1f,
                        WeightGainProbability = 0.3f,
                        WeightLossProbability = 0.1f,
                        TattooProbability = 0.3f,
                        TattooMaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,27,28",
                        TattooFemaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23",
                        HairColorProbability = 0.0f,
                        HairColorFilter = 0,
                        TattooColorProbability = 0.1f,
                        TattooColorFilter = 0

                    },
                    /* vlandia (based on Default + modifiers + excludes) */
                    new LooksConfig
                    {
                        Culture = "vlandia",
                        HairGrowProbability = 0.3f,
                        HairShaveProbability = 0.15f,
                        FacialHairGrowProbability = 0.25f,
                        ManFacial_None_Csv = "0",
                        ManFacial_VeryShort_Csv = "11,23,25,26,27",
                        ManFacial_Short_Csv = "2,3,4,5,6,7,10,12,24,31,32,33",
                        ManFacial_Medium_Csv = "8,9,13,28,29,35,36",
                        ManFacial_Long_Csv = "14,15,30,34,37,38",
                        ManFacial_VeryLong_Csv = "16,17,18,39,40,41",
                        ManHair_None_Csv = "0",
                        ManHair_VeryShort_Csv = "6,8,12,13,14,15,18,26,27,28",
                        ManHair_Short_Csv = "4,5,9",
                        ManHair_Medium_Csv = "1,3,11,19,25",
                        ManHair_Long_Csv = "2,20",
                        ManHair_VeryLong_Csv = "",
                        FemHair_None_Csv = "0",
                        FemHair_VeryShort_Csv = "20",
                        FemHair_Short_Csv = "7,12,21",
                        FemHair_Medium_Csv = "5,6,11,14,16",
                        FemHair_Long_Csv = "3",
                        FemHair_VeryLong_Csv = "1,2,4,10,13",
                        BuildGainProbability = 0.2f,
                        BuildLossProbability = 0.2f,
                        WeightGainProbability = 0.2f,
                        WeightLossProbability = 0.2f,
                        TattooProbability = 0f,
                        TattooMaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,27,28",
                        TattooFemaleCsv = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23",
                        HairColorProbability = 0.0f,
                        HairColorFilter = 0,
                        TattooColorProbability = 0.1f,
                        TattooColorFilter = 0

                    },
                };
                SaveToFile();
            }
            RefreshLooksDropdown();
        }

        private void SaveToFile()
        {
            var data = new ConfigData
            {
                Looks = LooksList
            };
            File.WriteAllText(_filePath,
                JsonConvert.SerializeObject(data, Formatting.Indented)
            );
        }

        private void RefreshLooksDropdown()
        {
            var list = LooksList.ToList();
            // find the old selection index (or 0 if new)
            var old = _looksSelector?.SelectedValue;
            var newIndex = list.IndexOf(old ?? list.FirstOrDefault());

            if (newIndex < 0 && old != null)
            {
                newIndex = list.FindIndex(x => string.Equals(x.Culture, old.Culture, StringComparison.OrdinalIgnoreCase));
            }

            if (newIndex < 0) newIndex = 0;

            _looksSelector = new Dropdown<LooksConfig>(list, newIndex);

            // update last selected so the getter's change-detection doesn't fire spuriously
            _lastSelectedLooks = _looksSelector.SelectedValue;

            // notify MCM/UI that your public selector has changed
            OnPropertyChanged(nameof(LooksSelector));
        }

        // ── backing fields ────────────────────────────────────────────────
        private Dropdown<LooksConfig> _looksSelector;
        private LooksConfig _lastSelectedLooks;

        // convenience:
        private LooksConfig CurrentLooks =>
            LooksList.ElementAtOrDefault(_looksSelector?.SelectedIndex ?? 0)
            ?? new LooksConfig();


        //--------Per Culture ---------

        [SettingPropertyDropdown(
            "{=DLC_SelectLooks}Select Looks Config",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_SelectLooks_H}Pick which looks config to edit")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture")]
        public Dropdown<LooksConfig> LooksSelector
        {
            get
            {
                // MCM has updated SelectedValue/Index under the hood?
                var current = _looksSelector?.SelectedValue;
                if (current != _lastSelectedLooks)
                {
                    _lastSelectedLooks = current;
                    OnPropertyChanged(nameof(CurrentLooks));
                }
                return _looksSelector;
            }
        }

        [SettingPropertyButton(
            "{=DLC_AddLooks}Add New Looks Config",
            Content = "{=DLC_Add}Add",
            Order = 1, RequireRestart = false,
            HintText = "{=DLC_AddDisease_H}Append a new looks config with default values")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture")]
        public Action AddLooksButton
        {
            get;
            set;
        } = () =>
        {
            Instance.LooksList.Add(new LooksConfig { Culture = "New Looks Config" });
            Instance.RefreshLooksDropdown();
            Instance.SaveToFile();
        };

        [SettingPropertyButton(
            "{=DLC_DeleteLooks}Delete Looks Config",
            Content = "{=DLC_Delete}Delete",
            Order = 2, RequireRestart = false,
            HintText = "{=DLC_DeleteLooks_H}Remove the selected looks config")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture")]
        public Action DeleteLooksButton
        {
            get;
            set;
        } = () =>
        {
            if (Instance.LooksSelector.SelectedIndex.InRange(0, Instance.LooksList.Count - 1))
            {
                Instance.LooksList.RemoveAt(Instance.LooksSelector.SelectedIndex);
                Instance.RefreshLooksDropdown();
                Instance.SaveToFile();
            }
        };

        [SettingPropertyButton(
            "{=DLC_ClearLooks}Clear All Looks Config",
            Content = "{=DLC_Clear}Clear",
            Order = 3, RequireRestart = false,
            HintText = "{=DLC_ClearLooks_H}Remove all looks configs except the first one.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture")]
        public Action ClearLooksButton
        {
            get;
            set;
        } = () =>
        {
            // keep index 0, remove everything else
            if (Instance.LooksList.Count > 1)
            {
                Instance.LooksList.RemoveRange(1, Instance.LooksList.Count - 1);
                Instance.RefreshLooksDropdown();
                Instance.SaveToFile();
                InformationManager.DisplayMessage(
                    new InformationMessage("[DLC] Cleared all looks (except first)", Colors.Green)
                );
            }
        };

        [SettingPropertyText(
            "{=DLC_Culture}Culture ID",
            Order = 4, RequireRestart = false,
            HintText = "{=DLC_Culture_H}Culture ID that this config will use."
        )]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture")]
        public string Culture
        {
            get => CurrentLooks.Culture;
            set { CurrentLooks.Culture = value; SaveToFile(); }
        }

        [SettingPropertyFloatingInteger(
            "{=DLC_HairGrowProb}Hair Grow Probability",
            0f, 1f, "#0%",
            Order = 1, RequireRestart = false,
            HintText = "{=DLC_HairGrowProb_H}Base chance hair will grow.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_CHANCES}Chances")]
        public float HairGrowProbability
        {
            get => CurrentLooks.HairGrowProbability;
            set { CurrentLooks.HairGrowProbability = value; SaveToFile(); }
        }

        [SettingPropertyFloatingInteger(
            "{=DLC_HairShaveProb}Hair Shave Probability",
            0f, 1f, "#0%",
            Order = 1, RequireRestart = false,
            HintText = "{=DLC_HairShaveProb_H}Base chance hair will be shaved.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_CHANCES}Chances")]
        public float HairShaveProbability
        {
            get => CurrentLooks.HairShaveProbability;
            set { CurrentLooks.HairShaveProbability = value; SaveToFile(); }
        }

        [SettingPropertyFloatingInteger(
            "{=DLC_FacialGrowProb}Facial Hair Grow Probability",
            0f, 1f, "#0%",
            Order = 2, RequireRestart = false,
            HintText = "{=DLC_FacialGrowProb_H}Base chance beard grows.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_CHANCES}Chances")]
        public float FacialHairGrowProbability
        {
            get => CurrentLooks.FacialHairGrowProbability;
            set { CurrentLooks.FacialHairGrowProbability = value; SaveToFile(); }
        }

        // ──────────── Facial Hair Buckets (Male) ────────────
        [SettingPropertyText(
            "{=DLC_ManFacial_None}Male Facial 'None'",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_ManFacial_None_H}Comma‑separate style IDs.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_FACIAL}Facial/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManFacial_None_Csv
        {
            get => CurrentLooks.ManFacial_None_Csv;
            set { CurrentLooks.ManFacial_None_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManFacial_VShort}Male Facial 'VeryShort'",
            Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_FACIAL}Facial/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManFacial_VeryShort_Csv
        {
            get => CurrentLooks.ManFacial_VeryShort_Csv;
            set { CurrentLooks.ManFacial_VeryShort_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManFacial_Short}Male Facial 'Short' ",
            Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_FACIAL}Facial/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManFacial_Short_Csv
        {
            get => CurrentLooks.ManFacial_Short_Csv;
            set { CurrentLooks.ManFacial_Short_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManFacial_Medium}Male Facial 'Medium' ",
            Order = 3, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_FACIAL}Facial/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManFacial_Medium_Csv
        {
            get => CurrentLooks.ManFacial_Medium_Csv;
            set { CurrentLooks.ManFacial_Medium_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManFacial_Long}Male Facial 'Long' ",
            Order = 4, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_FACIAL}Facial/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManFacial_Long_Csv
        {
            get => CurrentLooks.ManFacial_Long_Csv;
            set { CurrentLooks.ManFacial_Long_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManFacial_VLong}Male Facial 'VeryLong' ",
            Order = 5, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_FACIAL}Facial/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManFacial_VeryLong_Csv
        {
            get => CurrentLooks.ManFacial_VeryLong_Csv;
            set { CurrentLooks.ManFacial_VeryLong_Csv = value; SaveToFile(); }
        }

        // ──────────── Hair Buckets (Male) ────────────
        [SettingPropertyText(
            "{=DLC_ManHair_None}Male Hair 'None'",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_ManHair_None_H}Comma‑separate style IDs.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManHair_None_Csv
        {
            get => CurrentLooks.ManHair_None_Csv;
            set { CurrentLooks.ManHair_None_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManHair_VShort}Male Hair 'VeryShort'",
            Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManHair_VeryShort_Csv
        {
            get => CurrentLooks.ManHair_VeryShort_Csv;
            set { CurrentLooks.ManHair_VeryShort_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManHair_Short}Male Hair 'Short'",
            Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManHair_Short_Csv
        {
            get => CurrentLooks.ManHair_Short_Csv;
            set { CurrentLooks.ManHair_Short_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManHair_Medium}Male Hair 'Medium' ",
            Order = 3, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManHair_Medium_Csv
        {
            get => CurrentLooks.ManHair_Medium_Csv;
            set { CurrentLooks.ManHair_Medium_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManHair_Long}Male Hair 'Long' ",
            Order = 4, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManHair_Long_Csv
        {
            get => CurrentLooks.ManHair_Long_Csv;
            set { CurrentLooks.ManHair_Long_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_ManHair_VLong}Male Hair 'VeryLong'",
            Order = 5, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_MALE}Male")]
        public string ManHair_VeryLong_Csv
        {
            get => CurrentLooks.ManHair_VeryLong_Csv;
            set { CurrentLooks.ManHair_VeryLong_Csv = value; SaveToFile(); }
        }

        // ──────────── Hair Buckets (Female) ────────────
        [SettingPropertyText(
            "{=DLC_FemHair_None}Female Hair 'None' ",
            Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_FEMALE}Female")]
        public string FemHair_None_Csv
        {
            get => CurrentLooks.FemHair_None_Csv;
            set { CurrentLooks.FemHair_None_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_FemHair_VShort}Female Hair 'VeryShort' ",
            Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_FEMALE}Female")]
        public string FemHair_VeryShort_Csv
        {
            get => CurrentLooks.FemHair_VeryShort_Csv;
            set { CurrentLooks.FemHair_VeryShort_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_FemHair_Short}Female Hair 'Short' ",
            Order = 2, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_FEMALE}Female")]
        public string FemHair_Short_Csv
        {
            get => CurrentLooks.FemHair_Short_Csv;
            set { CurrentLooks.FemHair_Short_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_FemHair_Medium}Female Hair 'Medium' ",
            Order = 3, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_FEMALE}Female")]
        public string FemHair_Medium_Csv
        {
            get => CurrentLooks.FemHair_Medium_Csv;
            set { CurrentLooks.FemHair_Medium_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_FemHair_Long}Female Hair 'Long' ",
            Order = 4, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_FEMALE}Female")]
        public string FemHair_Long_Csv
        {
            get => CurrentLooks.FemHair_Long_Csv;
            set { CurrentLooks.FemHair_Long_Csv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_FemHair_VLong}Female Hair 'VeryLong' ",
            Order = 5, RequireRestart = false)]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_HAIR}Hair/{=MCM_BUCKETS}Buckets/{=MCM_FEMALE}Female")]
        public string FemHair_VeryLong_Csv
        {
            get => CurrentLooks.FemHair_VeryLong_Csv;
            set { CurrentLooks.FemHair_VeryLong_Csv = value; SaveToFile(); }
        }


        //── Body ───────────────────────────────────────────────────────────

        [SettingPropertyFloatingInteger(
            "{=DLC_BuildGainProb}Build Gain Probability",
            0f, 1f, "#0%",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_BuildGainProb_H}Base chance to gain build.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_BODY}Body/{=MCM_CHANCES}Chances")]
        public float BuildGainProbability
        {
            get => CurrentLooks.BuildGainProbability;
            set { CurrentLooks.BuildGainProbability = value; SaveToFile(); }
        }

        [SettingPropertyFloatingInteger(
            "{=DLC_BuildLossProb}Build Loss Probability",
            0f, 1f, "#0%",
            Order = 1, RequireRestart = false,
            HintText = "{=DLC_BuildLossProb_H}Base chance to lose build.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_BODY}Body/{=MCM_CHANCES}Chances")]
        public float BuildLossProbability
        {
            get => CurrentLooks.BuildLossProbability;
            set { CurrentLooks.BuildLossProbability = value; SaveToFile(); }
        }

        [SettingPropertyFloatingInteger(
            "{=DLC_WeightGainProb}Weight Gain Probability",
            0f, 1f, "#0%",
            Order = 4, RequireRestart = false,
            HintText = "{=DLC_WeightGainProb_H}Base chance to gain weight.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_BODY}Body/{=MCM_CHANCES}Chances")]
        public float WeightGainProbability
        {
            get => CurrentLooks.WeightGainProbability;
            set { CurrentLooks.WeightGainProbability = value; SaveToFile(); }
        }

        [SettingPropertyFloatingInteger(
            "{=DLC_WeightLossProb}Weight Loss Probability",
            0f, 1f, "#0%",
            Order = 5, RequireRestart = false,
            HintText = "{=DLC_WeightLossProb_H}Base chance to lose weight.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_BODY}Body/{=MCM_CHANCES}Chances")]
        public float WeightLossProbability
        {
            get => CurrentLooks.WeightLossProbability;
            set { CurrentLooks.WeightLossProbability = value; SaveToFile(); }
        }

        //── Tattoo & Scar ──────────────────────────────────────────────────

        [SettingPropertyFloatingInteger(
            "{=DLC_TattooProb}Tattoo Chance",
            0f, 1f, "#0%",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_TattooProb_H}Base chance NPC gets a new tattoo.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_TATTOO}Tattoo/{=MCM_SCAR}Scar")]
        public float TattooProbability
        {
            get => CurrentLooks.TattooProbability;
            set { CurrentLooks.TattooProbability = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_TattooMale}Male Tattoo Indexes",
            Order = 2, RequireRestart = false,
            HintText = "{=DLC_TattooMale_H}Comma‑separate allowed tattoo IDs for men.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_TATTOO}Tattoo/{=MCM_SCAR}Scar")]
        public string TattooMaleCsv
        {
            get => CurrentLooks.TattooMaleCsv;
            set { CurrentLooks.TattooMaleCsv = value; SaveToFile(); }
        }

        [SettingPropertyText(
            "{=DLC_TattooFemale}Female Tattoo Indexes",
            Order = 3, RequireRestart = false,
            HintText = "{=DLC_TattooFemale_H}Comma‑separate allowed tattoo IDs for women.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_TATTOO}Tattoo/{=MCM_SCAR}Scar")]
        public string TattooFemaleCsv
        {
            get => CurrentLooks.TattooFemaleCsv;
            set { CurrentLooks.TattooFemaleCsv = value; SaveToFile(); }
        }


        // ──────────── Color‑Chances ────────────
        [SettingPropertyFloatingInteger(
            "{=DLC_HairColorProb}Hair Color Change Prob.",
            0f, 1f, "#0%",
            Order = 0, RequireRestart = false,
            HintText = "{=DLC_HairColorProb_H}Chance NPC’s hair color shifts.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_COLOR}Color/{=MCM_CHANCES}Chances")]
        public float HairColorProbability
        {
            get => CurrentLooks.HairColorProbability;
            set { CurrentLooks.HairColorProbability = value; SaveToFile(); }
        }

        [SettingPropertyInteger(
            "{=DLC_HairColorFilter}Hair Color Change Filter.",
            0, 2,
            Order = 1, RequireRestart = false,
            HintText = "{=DLC_HairColorFilter_H}0 = all, 1 = female only, 2 = male only")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_COLOR}Color/{=MCM_CHANCES}Chances")]
        public int HairColorFilter
        {
            get => CurrentLooks.HairColorFilter;
            set { CurrentLooks.HairColorFilter = value; SaveToFile(); }
        }

        [SettingPropertyFloatingInteger(
            "{=DLC_TattooColorProb}Tattoo Color Change Prob.",
            0f, 1f, "#0%",
            Order = 2, RequireRestart = false,
            HintText = "{=DLC_TattooColorProb_H}Chance NPC’s tattoo color shifts.")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_COLOR}Color/{=MCM_CHANCES}Chances")]
        public float TattooColorProbability
        {
            get => CurrentLooks.TattooColorProbability;
            set { CurrentLooks.TattooColorProbability = value; SaveToFile(); }
        }

        [SettingPropertyInteger(
            "{=DLC_TattooColorFilter}Tattoo Color Change Filter.",
            0, 2,
            Order = 3, RequireRestart = false,
            HintText = "{=DLC_TattooColorFilter_H}0 = all, 1 = female only, 2 = male only")]
        [SettingPropertyGroup("{=MCM_PER_CULTURE}Per Culture/{=MCM_COLOR}Color/{=MCM_CHANCES}Chances")]
        public int TattooColorFilter
        {
            get => CurrentLooks.TattooColorFilter;
            set { CurrentLooks.TattooColorFilter = value; SaveToFile(); }
        }

    }

    // ── Helpers ───────────────────────────────────────────────────
    public static class IntExtensions
    {
        public static bool InRange(this int i, int min, int max)
            => i >= min && i <= max;
    }
}

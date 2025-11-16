using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace DynamicLooksChange
{
    public class DynamicLooksChangeCommands
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("debug_force_change_hero", "dynamiclookschange")]
        private static string DebugForceChange(List<string> args)
        {


            if (args.Count <= 0) return "Hero not specified. Use dynamiclookschange.debug_force_change_hero HeroNameHere";
            var heroName = string.Join(" ", args);
            var hero = Hero.AllAliveHeroes
                .FirstOrDefault(h => h.Name.ToString().Equals(heroName, System.StringComparison.OrdinalIgnoreCase));

            if (hero == null) return "Hero not found (Or dead)";
            if (hero.IsChild) return "Hero is child";

            var behavior = Campaign.Current.GetCampaignBehavior<DynamicLooksChangeBehavior>();
            if (behavior == null) return "Behavior not loaded";
            var s = DynamicLooksChangeSettings.Instance;
            var perClan = DynamicLooksChangeSettingsPerCulture.Instance;
            if (s == null || perClan == null) return "Settings not loaded";

            var femScars = DynamicLooksChangeBehavior.ParseCsv(s.ScarsFemaleCsv);
            var manScars = DynamicLooksChangeBehavior.ParseCsv(s.ScarsMaleCsv);

            var cultureId = hero.Culture.StringId.ToLowerInvariant();
            var config = perClan.LooksList.FirstOrDefault(d => d.Culture == cultureId) ?? perClan.LooksList.FirstOrDefault(d => d.Culture == "Default") ?? new LooksConfig();
            if (hero.IsFemale)
            {
                behavior.CalculateProbaility(hero, cultureId, config, femScars);
            }
            else
            {
                behavior.CalculateProbaility(hero, cultureId, config, manScars);
            }
            return $"Starting hair change process for {hero.Name} from {hero.Clan.Name}";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("debug_check_index_hero", "dynamiclookschange")]
        private static string CheckIndex(List<string> args)
        {


            if (args.Count <= 0) return "Hero not specified. Use dynamiclookschange.debug_check_index_hero HeroNameHere";
            var heroName = string.Join(" ", args);
            var hero = Hero.AllAliveHeroes
                .FirstOrDefault(h => h.Name.ToString().Equals(heroName, System.StringComparison.OrdinalIgnoreCase));

            if (hero == null) return "Hero not found (Or dead)";

            var faceParams = default(FaceGenerationParams);
            MBBodyProperties.GetParamsFromKey(
                ref faceParams,
                hero.BodyProperties,
                earsAreHidden: false,   // usually `false` when reading NPCs
                mouthHidden: false // we just want to read, not regenerate
            );

            int hairIndex = faceParams.CurrentHair;
            int beardIndex = faceParams.CurrentBeard;
            int tattooIndex = faceParams.CurrentFaceTattoo; // <— this is the real tattoo ID
            var haircolorOffset = faceParams.CurrentHairColorOffset;
            var tattoocolorOffset = faceParams.CurrentFaceTattooColorOffset1;
            var skincolorOffset = faceParams.CurrentSkinColorOffset;
            var eyecolorOffset = faceParams.CurrentEyeColorOffset;

            // 2) Compute integer indexes for each color channel
            //    using the same logic the CharacterEditor UI uses internally:
            int genderInt = hero.IsFemale ? 1 : 0; // map your gender to int
            int age = (int)hero.Age;

            // 1) fetch the available skin/hair‐color palettes
            var skinColors = MBBodyProperties.GetSkinColorCount(
                hero.CharacterObject.Race, genderInt, age
            );
            var hairColors = MBBodyProperties.GetHairColorCount(
                hero.CharacterObject.Race, genderInt, age
            );
            var tattooColors = MBBodyProperties.GetTatooColorCount(hero.CharacterObject.Race, genderInt, age);

            // 2) convert the normalized offsets (0–1 floats) back into integer indexes
            int skincolorIdx = (int)Math.Round(faceParams.CurrentSkinColorOffset * (skinColors - 1));
            int haircolorIdx = (int)Math.Round(faceParams.CurrentHairColorOffset * (hairColors - 1));
            int tattoocolorIdx = (int)Math.Round(faceParams.CurrentFaceTattooColorOffset1 * (tattooColors - 1));
            int eyecolorIdx = (int)Math.Round(faceParams.CurrentEyeColorOffset * 100);

            // 3) clamp them so we never go out of bounds
            skincolorIdx = Math.Max(0, Math.Min(skincolorIdx, skinColors - 1));
            haircolorIdx = Math.Max(0, Math.Min(haircolorIdx, hairColors - 1));
            tattoocolorIdx = Math.Max(0, Math.Min(tattoocolorIdx, tattooColors - 1));

            return $"Hair Index: {hairIndex}, Hair Color Index: {haircolorIdx}, Hair Color Offset: {haircolorOffset}, Beard Index: {beardIndex}, Tattoo Index: {tattooIndex}, Tattoo Color Index: {tattoocolorIdx}, Tattoo Color Offset: {tattoocolorOffset}, Skin Color Index: {skincolorIdx}, Skin Color Offset: {skincolorOffset}, Eye Color Offset: {eyecolorOffset}  for {hero.Name}";
        }

    }
}

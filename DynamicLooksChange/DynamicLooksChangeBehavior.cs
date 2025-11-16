using Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace DynamicLooksChange
{
    public class DynamicLooksChangeBehavior : CampaignBehaviorBase
    {
        private bool _fatalErrorLogged;
        private int day;
        public static DynamicLooksChangeBehavior Instance { get; private set; }
        private static readonly string[] HairBuckets = { "VeryLong", "Long", "Medium", "Short", "VeryShort", "None" };
        private static readonly Dictionary<string, string[]> DowngradeMap = new Dictionary<string, string[]>()
        {
            // From VeryLong: anything except VeryLong
            ["VeryLong"] = HairBuckets.Except(new[] { "VeryLong" }).ToArray(),
            // From Long: Medium, Short, VeryShort, None
            ["Long"] = new[] { "Medium", "Short", "VeryShort", "None" },
            // From Medium: Short, VeryShort, None
            ["Medium"] = new[] { "Short", "VeryShort", "None" },
            // From Short: VeryShort, None
            ["Short"] = new[] { "VeryShort", "None" },
            // From VeryShort: None
            ["VeryShort"] = new[] { "None" },
            // From None: stay None (or you could decide to “grow” instead)
            ["None"] = new[] { "None" }
        };
        private static readonly Dictionary<string, string[]> UpgradeMap = new Dictionary<string, string[]>()
        {
            // From None: anything except None (i.e. all longer)
            ["None"] = HairBuckets.Except(new[] { "None" }).ToArray(),
            // From VeryShort: Short, Medium, Long, VeryLong
            ["VeryShort"] = new[] { "Short", "Medium", "Long", "VeryLong" },
            // From Short: Medium, Long, VeryLong
            ["Short"] = new[] { "Medium", "Long", "VeryLong" },
            // From Medium: Long, VeryLong
            ["Medium"] = new[] { "Long", "VeryLong" },
            // From Long: VeryLong
            ["Long"] = new[] { "VeryLong" },
            // From VeryLong: stay VeryLong
            ["VeryLong"] = new[] { "VeryLong" }
        };

        public override void SyncData(IDataStore dataStore) { }


        public override void RegisterEvents()
        {

            Instance = this;
            // Show config on load
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);

            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);

            CampaignEvents.HeroWounded.AddNonSerializedListener(this, OnHeroWounded);

            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);

        }

        // Helpers for lists
        public static List<int> ParseCsv(string raw)
        {
            var outList = new List<int>();
            if (string.IsNullOrWhiteSpace(raw)) return outList;

            foreach (var piece in raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = piece.Trim();
                if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    outList.Add(v);
            }
            return outList;
        }

        public void ShuffleList<T>(IList<T> source, int howManyToShuffle)
        {
            if (source == null) return;

            var count = source.Count;
            if (count <= 1) return;

            int k = Math.Min(howManyToShuffle, count);

            for (int i = 0; i < k; i++)
            {
                int j = MBRandom.RandomInt(i, count);
                (source[i], source[j]) = (source[j], source[i]);
            }
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {

            day = (int)Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow;
            var s = DynamicLooksChangeSettings.Instance;
            if(s == null) return;
            if (s.StatInfo)
            {

                InformationManager.DisplayMessage(new InformationMessage("DynamicLooksChange settings:", Colors.Cyan));
                InformationManager.DisplayMessage(new InformationMessage($"ChangeIntervalDays: {s.ChangeIntervalDays}"));
                InformationManager.DisplayMessage(new InformationMessage($"ChangeHeroMax: {s.ChangeHeroMax}"));
                InformationManager.DisplayMessage(new InformationMessage($"AgeGrow: {s.AgeGrow}"));
                InformationManager.DisplayMessage(new InformationMessage($"AgeShave: {s.AgeShave}"));
                InformationManager.DisplayMessage(new InformationMessage($"SecondGrowMode: {s.SecondGrowMode}"));
                InformationManager.DisplayMessage(new InformationMessage($"SecondShaveMode: {s.SecondShaveMode}"));
                
                InformationManager.DisplayMessage(new InformationMessage($"AgeGain: {s.AgeGain}"));
                InformationManager.DisplayMessage(new InformationMessage($"AgeLoss: {s.AgeLoss}"));
                InformationManager.DisplayMessage(new InformationMessage($"Min Build: {s.BuildMin}"));
                InformationManager.DisplayMessage(new InformationMessage($"Max Build: {s.BuildMax}"));
                InformationManager.DisplayMessage(new InformationMessage($"Min Weight: {s.WeightMin}"));
                InformationManager.DisplayMessage(new InformationMessage($"Max Weight: {s.WeightMax}"));

                InformationManager.DisplayMessage(new InformationMessage($"AllowOthers: {s.AllowOthers}"));
                InformationManager.DisplayMessage(new InformationMessage($"AllowNotable: {s.AllowNotable}"));
                InformationManager.DisplayMessage(new InformationMessage($"AllowWanderer: {s.AllowWanderer}"));
                InformationManager.DisplayMessage(new InformationMessage($"AllowPlayer: {s.AllowPlayer}"));
                InformationManager.DisplayMessage(new InformationMessage($"AllowPlayerClan: {s.AllowPlayerClan}"));
                InformationManager.DisplayMessage(new InformationMessage($"InformPlayerClan: {s.InformPlayerClan}"));
                InformationManager.DisplayMessage(new InformationMessage($"InformPlayer: {s.InformPlayer}"));
                InformationManager.DisplayMessage(new InformationMessage($"Debug: {s.Debug}"));
            }

        }

        private void OnDailyTick()
        {
            var s = DynamicLooksChangeSettings.Instance;
            var perClan = DynamicLooksChangeSettingsPerCulture.Instance;
            day++;
            if (s.ChangeIntervalDays <= 0) return;
            if (day % s.ChangeIntervalDays != 0)
                return;  // skip until the next interval
            try
            {

                if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"[DynamicLooksChange] onDailyTick works"));
                // Snapshot copy
                var candidates = Hero.AllAliveHeroes
                    .Where(c => EligibleHeroes(c))
                    .OrderBy(c => MBRandom.RandomFloat)
                    .ToList();

                if (candidates.Count > s.ChangeHeroMax)
                {
                    ShuffleList(candidates, candidates.Count);

                    candidates = candidates.Take(s.ChangeHeroMax).ToList();
                }

                var femScars = ParseCsv(s.ScarsFemaleCsv);
                var maleScars = ParseCsv(s.ScarsMaleCsv);
                foreach (var hero in candidates)
                {
                    var cultureId = hero.Culture.StringId.ToLowerInvariant();

                    var config = perClan.LooksList.FirstOrDefault(d => d.Culture == cultureId) ?? perClan.LooksList.FirstOrDefault(d => d.Culture == "Default") ?? new LooksConfig();

                    if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Goes to process for {hero.Name}"));
                    if (hero.IsFemale)
                    {
                        CalculateProbaility(hero, cultureId, config, femScars);
                    }
                    else
                    {
                        CalculateProbaility(hero, cultureId, config, maleScars);
                    }
                        

                    if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"End of process"));
                }

            }
            catch (Exception ex)
            {
                if (!_fatalErrorLogged)
                {
                    _fatalErrorLogged = true;
                    InformationManager.DisplayMessage(new InformationMessage($"[DynamicLooksChange] DailyTick error: {ex.Message}"));
                }
            }
        }

        public void CalculateProbaility(Hero hero, string cultureId, LooksConfig config, List<int> scars)
        {
            var s = DynamicLooksChangeSettings.Instance;
            if (s == null) return;
            if (hero == null || config == null) return;
            var faceParams = default(FaceGenerationParams);
            MBBodyProperties.GetParamsFromKey(
                ref faceParams,
                hero.BodyProperties,
                earsAreHidden: false,   // usually `false` when reading NPCs
                mouthHidden: false // we just want to read, not regenerate
            );

            var hairDict = GetHairForGender(hero.IsFemale, config, false);

            //hair
            int hairIndex = faceParams.CurrentHair;
            int beardIndex = faceParams.CurrentBeard;
            int tattooIndex = faceParams.CurrentFaceTattoo; // <— this is the real tattoo ID

            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Hair Index: {hairIndex}"));
            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Beard Index: {beardIndex}"));
            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Tattoo Index: {tattooIndex}"));

            float hairGrow = (float)(config.HairGrowProbability - (hero.Age * s.AgeGrow * config.HairGrowProbability));
            float hairShave = (float)(config.HairShaveProbability - (hero.Age * s.AgeShave * config.HairShaveProbability));
            float fhairGrow = (float)(config.FacialHairGrowProbability - (hero.Age * s.AgeGrow * config.FacialHairGrowProbability));

            if (hairGrow < 0) hairGrow = 0;
            if (hairShave < 0) hairShave = 0;
            if (fhairGrow < 0) fhairGrow = 0;

            var roll = MBRandom.RandomFloat * (float)(hairGrow + hairShave);
            //grow hair
            if (roll < hairGrow &&
                hairDict.TryGetValue("VeryLong", out var VeryLongList1)
                        && !VeryLongList1.Contains(hairIndex))
            {

                HairChangeGrow(hero, hairDict, hairIndex, false, hairIndex, beardIndex, tattooIndex);
            }
            //shave hair
            else if (roll < hairGrow + hairShave && hero != Hero.MainHero &&
                hairDict.TryGetValue("None", out var noneList1)
                        && !noneList1.Contains(hairIndex))
            {

                HairChangeShave(hero, hairDict, hairIndex, false, hairIndex, beardIndex, tattooIndex);
            }

            var facialDict = GetHairForGender(hero.IsFemale, config, true);

            //grow beard
            roll = MBRandom.RandomFloat * (float)(fhairGrow + hairShave);
            if (roll < fhairGrow && !hero.IsFemale &&
                facialDict.TryGetValue("VeryLong", out var VeryLongList2)
                        && !VeryLongList2.Contains(beardIndex))
            {
                HairChangeGrow(hero, facialDict, beardIndex, true, hairIndex, beardIndex, tattooIndex);
            }
            //shave beard
            else if (roll < fhairGrow + hairShave && !hero.IsFemale && hero != Hero.MainHero &&
                facialDict.TryGetValue("None", out var noneList2)
                        && !noneList2.Contains(beardIndex))
            {

                HairChangeShave(hero, facialDict, beardIndex, true, hairIndex, beardIndex, tattooIndex);
            }

            //build/weight

            // base values from config
            float gainBuild = (float)config.BuildGainProbability;
            float lossBuild = (float)config.BuildLossProbability;
            float gainWeight = (float)config.WeightGainProbability;
            float lossWeight = (float)config.WeightLossProbability;

            // --- AGE EFFECT (normalized between adultAge..maxAge) ---
            // Tune these to your game's lifespan. Example: adulthood 16, 'old' 60
            float adultAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
            float oldAge = Campaign.Current.Models.AgeModel.BecomeOldAge;
            float rawAge = hero.Age;
            float ageNorm = 0f;
            if (rawAge <= adultAge) ageNorm = 0f;
            else if (rawAge >= oldAge) ageNorm = 1f;
            else ageNorm = (rawAge - adultAge) / (oldAge - adultAge); // 0..1

            // s.AgeGain / s.AgeLoss are expected small (tweakable)
            // Older heroes gain muscle less and lose muscle more; similar for weight.
            // We use multipliers instead of subtracting absolute probabilities.
            gainBuild *= 1f - (ageNorm * (float)s.AgeGain);      // older -> less ability to gain build
            lossBuild *= 1f + (ageNorm * (float)s.AgeLoss);      // older -> more loss
            gainWeight *= 1f - (ageNorm * (float)s.AgeGain);     // older -> less likely to put on new weight
            lossWeight *= 1f + (ageNorm * (float)s.AgeLoss);     // older -> more likely to lose weight

            // --- PARTY / FOOD EFFECT (only for main hero's party as in original) ---
            var party = hero.PartyBelongedTo;
            if (party != null && party == Hero.MainHero.PartyBelongedTo)
            {
                int daysFood = party.GetNumDaysForFoodToLast();

                if (daysFood <= 0)
                {
                    // starved: no gains, increased losses
                    gainWeight = 0f;
                    gainBuild = 0f;

                    // increase losses strongly under starvation
                    lossWeight *= 2f;
                    lossBuild *= 1.5f;
                }
                else
                {
                    // fractional food supply relative to a year (0..inf)
                    float foodFrac = daysFood / (float)CampaignTime.DaysInYear;

                    // scarcity multiplier: when foodFrac<1, increase losses progressively (max +50% at 0)
                    float scarcityMultiplier = 1f + Math.Max(0f, 1f - Math.Min(foodFrac, 1f)) * 0.5f;
                    lossWeight *= scarcityMultiplier;
                    lossBuild *= scarcityMultiplier * 0.8f;

                    // abundance effect: if they have > half-year food, allow greater gains
                    if (foodFrac > 0.5f)
                    {
                        // map (0.5..1.0+) -> (0..1) abundanceFactor (caps smoothly)
                        float abundanceFactor = Math.Min((foodFrac - 0.5f) / 0.5f, 1f);
                        gainWeight *= 1f + abundanceFactor * 0.5f;   // up to +50% gain chance
                        gainBuild *= 1f + abundanceFactor * 0.25f;   // smaller effect on build
                    }
                }
            }

            // --- ATHLETICS EFFECT (0..1 normalized) ---
            float athletics = hero.GetSkillValue(DefaultSkills.Athletics) / 300f;
            if (athletics < 0f) athletics = 0f;
            if (athletics > 1f) athletics = 1f;

            // High athletics: easier to gain build, harder to gain weight and easier to maintain weight
            if (athletics >= 0.7f)
            {
                float highFactor = (athletics - 0.7f) / 0.3f; // 0..1 across [0.7..1]
                gainBuild *= 1f + highFactor * 0.6f;          // up to +60%
                lossWeight *= 1f - highFactor * 0.5f;         // up to -50% less weight loss (more maintenance)
                gainWeight *= 1f - highFactor * 0.4f;         // up to -40% less weight gain
            }
            // Low athletics: easier to gain weight, harder to gain/maintain build
            else if (athletics <= 0.3f)
            {
                float lowFactor = (0.3f - athletics) / 0.3f;  // 0..1 across [0.3..0]
                gainBuild *= 1f - lowFactor * 0.5f;           // up to -50% gain build
                lossBuild *= 1f + lowFactor * 0.4f;           // up to +40% build loss
                gainWeight *= 1f + lowFactor * 0.6f;          // up to +60% weight gain
                lossWeight *= 1f + lowFactor * 0.2f;          // slight increase in weight loss volatility
            }

            // --- FINAL CLAMP ---
            // Ensure none of them go negative or exceed 1.0
            gainBuild = Math.Max(0f, Math.Min(1f, gainBuild));
            lossBuild = Math.Max(0f, Math.Min(1f, lossBuild));
            gainWeight = Math.Max(0f, Math.Min(1f, gainWeight));
            lossWeight = Math.Max(0f, Math.Min(1f, lossWeight));

            roll = MBRandom.RandomFloat * (gainBuild + lossBuild);

            if (roll < gainBuild)
            {
                BodyChangeGain(hero, true);
            }
            else if (roll < gainBuild + lossBuild)
            {
                BodyChangeLoss(hero, true);
            }

            roll = MBRandom.RandomFloat * (gainWeight + lossWeight);

            if (roll < gainWeight)
            {
                BodyChangeGain(hero, false);
            }
            else if (roll < gainWeight + lossWeight)
            {
                BodyChangeLoss(hero, false);
            }


            //Tattoo

            var chanceTattoo = config.TattooProbability;
            if (MBRandom.RandomFloat < chanceTattoo && !hero.IsHumanPlayerCharacter)
            {
                var tattoos = hero.IsFemale ? ParseCsv(config.TattooFemaleCsv) : ParseCsv(config.TattooMaleCsv);
                TattooChange(hero, tattoos, tattooIndex, hairIndex, beardIndex, scars);
            }

            //Colors
            if (MBRandom.RandomFloat < config.HairColorProbability && !hero.IsHumanPlayerCharacter)
            {
                HairTattooColorChange(hero, faceParams, true, config);
            }
            if (MBRandom.RandomFloat < config.TattooColorProbability && !hero.IsHumanPlayerCharacter)
            {
                HairTattooColorChange(hero, faceParams, false, config);
            }



            }

        public void HairChangeGrow(Hero hero, Dictionary<string, List<int>> hairDict, int hairIndex, bool beard, int oldHair, int oldBeard, int oldTattoo)
        {
            var s = DynamicLooksChangeSettings.Instance;
            // grow…
            if (s.InformPlayer && hero == Hero.MainHero)
            {
                var text = new TextObject("{=LookChanged_GrowHair_PLAYER}Your hair grew.");
                StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                InformationManager.DisplayMessage(new InformationMessage(text
                .ToString(), Colors.Green));
            }
            else if (s.InformPlayerClan && hero.Clan == Clan.PlayerClan && hero != Hero.MainHero)
            {
                var text = new TextObject("{=LookChanged_GrowHair_PLAYERCLAN}{HERO}'s hair grew.")
                .SetTextVariable("HERO", hero.Name);
                StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                InformationManager.DisplayMessage(new InformationMessage(text
                .ToString(), Colors.Yellow));
            }
            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Grow Hair for {hero.Name}"));

            int newHairIndex = hairIndex;

            if (s.SecondGrowMode == true)
            {
                // 1) Define your buckets in order (longest ➔ shortest)
                string[] hairBuckets = HairBuckets;

                // 2) Build an *upgrade* map: from each bucket, list all strictly *longer* buckets
                var upgradeMap = UpgradeMap;

                // Find current bucket (fallback to "None")
                string currentBucket = hairBuckets
                  .FirstOrDefault(b => hairDict.TryGetValue(b, out var list) && list.Contains(hairIndex))
                  ?? "None";

                // Look up which buckets we can upgrade into
                var allowedBuckets = upgradeMap[currentBucket];


                // Flatten those buckets into a single list of style‐indices,
                // exclude the current style, and fall back to itself if nothing else
                var candidateStyles = allowedBuckets
                    .SelectMany(b => hairDict[b])
                    .Except(new[] { hairIndex })
                    .DefaultIfEmpty(hairIndex)
                    .ToList();

                // Pick one at random
                newHairIndex = candidateStyles.GetRandomElement();

            }
            else
            {

                if (hairDict.TryGetValue("None", out var noneList)
                    && noneList.Contains(hairIndex)
                    && hairDict.TryGetValue("VeryShort", out var vsList)
                    && vsList != null && vsList.Count > 0)
                {
                    newHairIndex = vsList.GetRandomElement();
                }
                else if (hairDict.TryGetValue("VeryShort", out var veryShortList)
                    && veryShortList.Contains(hairIndex)
                    && hairDict.TryGetValue("Short", out var shortList)
                    && shortList != null && shortList.Count > 0)
                {
                    newHairIndex = shortList.GetRandomElement();
                }
                else if (hairDict.TryGetValue("Short", out var sList)
                    && sList.Contains(hairIndex)
                    && hairDict.TryGetValue("Medium", out var medList)
                    && medList != null && medList.Count > 0)
                {
                    newHairIndex = medList.GetRandomElement();
                }
                else if (hairDict.TryGetValue("Medium", out var mList)
                    && mList.Contains(hairIndex)
                    && hairDict.TryGetValue("Long", out var longList)
                    && longList != null && longList.Count > 0)
                {
                    newHairIndex = longList.GetRandomElement();
                }
                else if (hairDict.TryGetValue("Long", out var lList)
                    && lList.Contains(hairIndex)
                    && hairDict.TryGetValue("VeryLong", out var vlList)
                    && vlList != null && vlList.Count > 0)
                {
                    newHairIndex = vlList.GetRandomElement();
                }

            }

            if(newHairIndex != hairIndex)
            {
                if (beard)
                    ApplyFaceChanges(hero, oldHair, newHairIndex, oldTattoo);
                else
                    ApplyFaceChanges(hero, newHairIndex, oldBeard, oldTattoo);
            }

        }

        public void HairChangeShave(Hero hero, Dictionary<string, List<int>> hairDict, int hairIndex, bool beard, int oldHair, int oldBeard, int oldTattoo)
        {
            var s = DynamicLooksChangeSettings.Instance;
            // shave…
            if (s.Debug) InformationManager.DisplayMessage(new InformationMessage($"Shave Hair for {hero.Name}"));

            if (s.InformPlayerClan && hero.Clan == Clan.PlayerClan && hero != Hero.MainHero)
            {
                var text = new TextObject("{=LookChanged_ShaveHair_PLAYERCLAN}{HERO} shaved his head.")
                .SetTextVariable("HERO", hero.Name);
                StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                InformationManager.DisplayMessage(new InformationMessage(text
                .ToString(), Colors.Yellow));
            }

            int newHairIndex = hairIndex;
            if (s.SecondShaveMode == true)
            {
                // 1) Define your buckets in order (longest ➔ shortest)
                string[] hairBuckets = HairBuckets;

                // 2) Precompute for each bucket which *other* buckets you’re allowed to pick from
                var downgradeMap = DowngradeMap;

                // Find current bucket name
                string currentBucket = hairBuckets
                    .FirstOrDefault(name =>
                        hairDict.TryGetValue(name, out var list) && list.Contains(hairIndex))
                    ?? "None";  // fallback if FirstOrDefault returns null :contentReference[oaicite:0]{index=0}


                // Lookup allowed next buckets
                var allowedBuckets = downgradeMap[currentBucket];

                // Flatten to a list of style-indices
                var candidateStyles = allowedBuckets
                    .SelectMany(bucket => hairDict[bucket])
                    .Except(new[] { hairIndex })            // just in case “None” only has itself
                    .DefaultIfEmpty(hairIndex)             // fallback to same style
                    .ToList();

                // Pick your new style (one call, guaranteed valid)
                newHairIndex = candidateStyles.GetRandomElement();
            }
            else
            {
                // assume var hairDict = hero.IsFemale ? s.FemHair : s.ManHair;
                if (hairDict.TryGetValue("VeryLong", out var veryLongList)
                    && veryLongList.Contains(hairIndex)
                    && hairDict.TryGetValue("Long", out var longList)
                    && longList != null && longList.Count > 0)
                {
                    newHairIndex = longList.GetRandomElement();
                }
                else if (hairDict.TryGetValue("Long", out longList)
                    && longList.Contains(hairIndex)
                    && hairDict.TryGetValue("Medium", out var medList)
                    && medList != null && medList.Count > 0)
                {
                    newHairIndex = medList.GetRandomElement();
                }
                else if (hairDict.TryGetValue("Medium", out medList)
                    && medList.Contains(hairIndex)
                    && hairDict.TryGetValue("Short", out var shortList)
                    && shortList != null && shortList.Count > 0)
                {
                    newHairIndex = shortList.GetRandomElement();
                }
                else if (hairDict.TryGetValue("Short", out shortList)
                    && shortList.Contains(hairIndex)
                    && hairDict.TryGetValue("VeryShort", out var vsList)
                    && vsList != null && vsList.Count > 0)
                {
                    newHairIndex = vsList.GetRandomElement();
                }
                else if (hairDict.TryGetValue("VeryShort", out vsList)
                    && vsList.Contains(hairIndex)
                    && hairDict.TryGetValue("None", out var noneList)
                    && noneList != null && noneList.Count > 0)
                {
                    newHairIndex = noneList.GetRandomElement();
                }

            }

            if (newHairIndex != hairIndex)
            {
                if (beard)
                    ApplyFaceChanges(hero, oldHair, newHairIndex, oldTattoo);
                else
                    ApplyFaceChanges(hero, newHairIndex, oldBeard, oldTattoo);
            }

        }

        private void ApplyFaceChanges(Hero hero, int newHairIndex, int newBeardIndex, int newTattooIndex)
        {
            var s = DynamicLooksChangeSettings.Instance;

                hero.ModifyHair(newHairIndex, newBeardIndex, newTattooIndex);

                if (s.Debug) InformationManager.DisplayMessage(
      new InformationMessage(
        $"NewHair: {newHairIndex:F2}, NewBeard: {newBeardIndex:F2}, Tattoo: {newTattooIndex:F2}",
        Colors.Magenta
      ));
            
        }

        public void BodyChangeGain(Hero hero, bool build)
        {
            var s = DynamicLooksChangeSettings.Instance;
            var oldBuild = hero.Build;
            var oldWeight = hero.Weight;

            if (build)
            {
                if (s.InformPlayer && hero == Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_GainBuild_PLAYER}You got more bulky.");
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Green));
                }
                else if (s.InformPlayerClan && hero.Clan == Clan.PlayerClan && hero != Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_GainBuild_PLAYERCLAN}{HERO} got more bulky.")
                    .SetTextVariable("HERO", hero.Name);
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Yellow));
                }
                hero.Build += MBRandom.RandomFloatRanged((float)s.BuildMin, (float)s.BuildMax);
            }
            else
            {
                if (s.InformPlayer && hero == Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_GainWeight_PLAYER}You gained weight.");
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Green));
                }
                if (s.InformPlayerClan && hero.Clan == Clan.PlayerClan && hero != Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_GainWeight_PLAYERCLAN}{HERO} gain weight.")
                    .SetTextVariable("HERO", hero.Name);
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Yellow));
                }
                hero.Weight += MBRandom.RandomFloatRanged((float)s.WeightMin, (float)s.WeightMax);
            }

            if (hero.Build != oldBuild || hero.Weight != oldWeight)
            {
                hero.Build = Math.Min(Math.Max(hero.Build, 0.0f), 1.0f);         // Math.Min/Max overload for floats :contentReference[oaicite:2]{index=2}
                hero.Weight = Math.Min(Math.Max(hero.Weight, 0.0f), 1.0f);

                int buildInt = (int)(hero.Build * 100f);
                int weightInt = (int)(hero.Weight * 100f);
                ApplyBodyChanges(hero, buildInt, weightInt);
            }

        }

        public void BodyChangeLoss(Hero hero, bool build)
        {
            var s = DynamicLooksChangeSettings.Instance;
            var oldBuild = hero.Build;
            var oldWeight = hero.Weight;

            if (build)
            {
                if (s.InformPlayer && hero == Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_LossBuild_PLAYER}You lost some muscles.");
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Green));
                }
                else if (s.InformPlayerClan && hero.Clan == Clan.PlayerClan && hero != Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_LossBuild_PLAYERCLAN}{HERO} lost some muscles.")
                    .SetTextVariable("HERO", hero.Name);
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Yellow));
                }
                hero.Build -= MBRandom.RandomFloatRanged((float)s.BuildMin, (float)s.BuildMax);
            }
            else
            {
                if (s.InformPlayer && hero == Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_LossWeight_PLAYER}You lost weight.");
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Green));
                }
                if (s.InformPlayerClan && hero.Clan == Clan.PlayerClan && hero != Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_LossWeight_PLAYERCLAN}{HERO} lost weight.")
                    .SetTextVariable("HERO", hero.Name);
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Yellow));
                }
                hero.Weight -= MBRandom.RandomFloatRanged((float)s.WeightMin, (float)s.WeightMax);
            }

            if (hero.Build != oldBuild || hero.Weight != oldWeight)
            {
                hero.Build = Math.Min(Math.Max(hero.Build, 0.0f), 1.0f);         // Math.Min/Max overload for floats :contentReference[oaicite:2]{index=2}
                hero.Weight = Math.Min(Math.Max(hero.Weight, 0.0f), 1.0f);

                int buildInt = (int)(hero.Build * 100f);
                int weightInt = (int)(hero.Weight * 100f);
                ApplyBodyChanges(hero, buildInt, weightInt);
            }

        }

        private void ApplyBodyChanges(Hero hero, int newBuild, int newWeight)
        {
            var s = DynamicLooksChangeSettings.Instance;

            // 1) Grab the full BodyProperties struct
            var bp = hero.BodyProperties;
            
            // 2) Call FaceGen.SetBody to mutate the dynamicProperties inside it
            TaleWorlds.Core.FaceGen.SetBody(ref bp, newBuild, newWeight);

            // 3) Write back only the StaticBodyProperties half:
            hero.ModifyPlayersFamilyAppearance(bp.StaticProperties);

        }

        public static Dictionary<string, List<int>> GetHairForGender(bool isFemale, LooksConfig config, bool isBeard)
        {
            if (isBeard)
            {
                return new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["None"] = ParseCsv(config.ManFacial_None_Csv),
                    ["VeryShort"] = ParseCsv(config.ManFacial_VeryShort_Csv),
                    ["Short"] = ParseCsv(config.ManFacial_Short_Csv),
                    ["Medium"] = ParseCsv(config.ManFacial_Medium_Csv),
                    ["Long"] = ParseCsv(config.ManFacial_Long_Csv),
                    ["VeryLong"] = ParseCsv(config.ManFacial_VeryLong_Csv),
                };
            }
            else if (isFemale)
            {
                return new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["None"] = ParseCsv(config.FemHair_None_Csv),
                    ["VeryShort"] = ParseCsv(config.FemHair_VeryShort_Csv),
                    ["Short"] = ParseCsv(config.FemHair_Short_Csv),
                    ["Medium"] = ParseCsv(config.FemHair_Medium_Csv),
                    ["Long"] = ParseCsv(config.FemHair_Long_Csv),
                    ["VeryLong"] = ParseCsv(config.FemHair_VeryLong_Csv),
                };
            }
            else
            {
                return new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["None"] = ParseCsv(config.ManHair_None_Csv),
                    ["VeryShort"] = ParseCsv(config.ManHair_VeryShort_Csv),
                    ["Short"] = ParseCsv(config.ManHair_Short_Csv),
                    ["Medium"] = ParseCsv(config.ManHair_Medium_Csv),
                    ["Long"] = ParseCsv(config.ManHair_Long_Csv),
                    ["VeryLong"] = ParseCsv(config.ManHair_VeryLong_Csv),
                };
            }
        }

        public void TattooChange(Hero hero, List<int> tattoos, int oldTattoo, int oldHair, int oldBeard, List<int> scars)
        {
            var s = DynamicLooksChangeSettings.Instance;

            if (!tattoos.Any()) return;

            if (s.ScarPermament == true && scars.Contains(oldTattoo)) return;

            var candidateTattoos = tattoos.ToList();
            if (candidateTattoos.Contains(oldTattoo)) candidateTattoos.Remove(oldTattoo);
            if (!candidateTattoos.Any()) return;
            // Pick a random scar index from your configured pool
            int newTattooIndex = candidateTattoos.GetRandomElement();

            // Apply via ModifyHair (third parameter is the tattoo index)
            hero.ModifyHair(oldHair, oldBeard, newTattooIndex);

            if (s.InformPlayerClan && hero.Clan == Clan.PlayerClan && hero != Hero.MainHero)
            {
                var text = new TextObject("{=LookChanged_Tattoo_PLAYERCLAN}{HERO} got tattoo.")
                .SetTextVariable("HERO", hero.Name);
                StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                InformationManager.DisplayMessage(new InformationMessage(text
                .ToString(), Colors.Yellow));
            }
        }

        //colors
        public void HairTattooColorChange(Hero hero, FaceGenerationParams faceParams, bool hair, LooksConfig config)
        {
            var s = DynamicLooksChangeSettings.Instance;
            if (hero.IsHumanPlayerCharacter) return;

                int genderInt = hero.IsFemale ? 1 : 0;

            bool changed = false;

            if (hair)
            {
                if (hero.IsFemale && config.HairColorFilter == 2) return;
                else if (!hero.IsFemale && config.HairColorFilter == 1) return;
                // pull out the full list so we know how many slots there are
                var hairColors = MBBodyProperties.GetHairColorGradientPoints(
                    hero.CharacterObject.Race, genderInt, (int)hero.Age
                );
                if (hairColors.Count > 1)
                {
                    // pick one slot at random
                    int newHairSlot = MBRandom.RandomInt(0, hairColors.Count - 1);

                    // write *only* the hair offset
                    faceParams.CurrentHairColorOffset =
                        (float)newHairSlot / (hairColors.Count - 1);
                    changed = true;
                }
            }
            else if (!hair)
            {
                if (hero.IsFemale && config.TattooColorFilter == 2) return;
                else if (!hero.IsFemale && config.TattooColorFilter == 1) return;
                // pull out the full list so we know how many slots there are
                var tattooColors = MBBodyProperties.GetTatooColorCount(
                    hero.CharacterObject.Race, genderInt, (int)hero.Age
                );
                if (tattooColors > 1)
                {
                    // pick one slot at random
                    int newHTattooSlot = MBRandom.RandomInt(0, tattooColors - 1);

                    // write *only* the hair offset
                    faceParams.CurrentFaceTattooColorOffset1 =
                        (float)newHTattooSlot / (tattooColors - 1);
                    changed = true;
                }

            }

            if (changed)
            {
                // re‑encode all three offsets at once
                var bp = hero.BodyProperties;
                MBBodyProperties.ProduceNumericKeyWithParams(
                    faceParams,
                    false,   // don't regenerate meshes here
                    false,
                    ref bp
                );
                hero.ModifyPlayersFamilyAppearance(bp.StaticProperties);

                if (s.InformPlayerClan && hero.Clan == Clan.PlayerClan && hero != Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_HairTattooColor_PLAYERCLAN}{HERO} changed colors.")
                                                    .SetTextVariable("HERO", hero.Name);
                    StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(
                                            new InformationMessage(text
                                                    .ToString(),
                                                Colors.Yellow
                                            )
                                        );
                }

            }

        }

        //Scar
        private void OnHeroWounded(Hero woundedHero)
        {
            try
            {
                var s = DynamicLooksChangeSettings.Instance;
                var menScars = ParseCsv(s.ScarsMaleCsv);
                var femScars = ParseCsv(s.ScarsFemaleCsv);
                if (s.Debug)
                    InformationManager.DisplayMessage(
                      new InformationMessage("[DLC] OnHeroWounded fired!", Colors.Magenta)
                    );

                // only adults, only allowed heroes
                EligibleHeroes(woundedHero);

                // pick the right pool
                var scars = woundedHero.IsFemale
                    ? femScars
                    : menScars;
                if (scars == null || scars.Count == 0) return;

                // read current
                var faceParams = default(FaceGenerationParams);
                MBBodyProperties.GetParamsFromKey(
                    ref faceParams,
                    woundedHero.BodyProperties,
                    earsAreHidden: false,
                    mouthHidden: false
                );
                int currentScar = faceParams.CurrentFaceTattoo;

                // if they already have one of your scars, skip entirely
                if (scars.Contains(currentScar)) return;

                // now roll
                if (MBRandom.RandomFloat >= s.ScarProbability) return;

                // build list excluding what they already have
                var candidates = scars.Where(idx => idx != currentScar).ToList();
                if (candidates.Count == 0) return;

                int newScar = candidates[MBRandom.RandomInt(candidates.Count)];

                // apply
                woundedHero.ModifyHair(
                    faceParams.CurrentHair,
                    faceParams.CurrentBeard,
                    newScar
                );

                if (s.InformPlayer && woundedHero == Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_Scar_PLAYER}You got scar.");
                    StringHelpers.SetCharacterProperties("CHARACTER", woundedHero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Green));
                }
                if (s.InformPlayerClan && woundedHero.Clan == Clan.PlayerClan && woundedHero != Hero.MainHero)
                {
                    var text = new TextObject("{=LookChanged_Scar_PLAYERCLAN}{HERO} got scar.")
                    .SetTextVariable("HERO", woundedHero.Name);
                    StringHelpers.SetCharacterProperties("CHARACTER", woundedHero.CharacterObject, text, true);
                    InformationManager.DisplayMessage(new InformationMessage(text
                    .ToString(), Colors.Yellow));
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"[DLC] OnHeroWounded error: {ex.Message}")
                );
            }
        }

        private bool EligibleHeroes(Hero hero)
        {
            var s = DynamicLooksChangeSettings.Instance;
            if (hero == null || hero.IsChild) return false;
            if (!s.AllowOthers && !hero.IsHumanPlayerCharacter && hero.Clan != Clan.PlayerClan) return false;
            if (!s.AllowNotable && hero.IsNotable) return false;
            if (!s.AllowPlayer && hero.IsHumanPlayerCharacter) return false;
            if (!s.AllowPlayerClan && hero.Clan == Hero.MainHero.Clan) return false;
            if (!s.AllowWanderer && hero.IsWanderer) return false;

            return true;
        }


        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            var s = DynamicLooksChangeSettings.Instance;
            if (s == null || !s.EnableBarber) return;
            // For each world‑map menu we want to inject into:
            foreach (var parent in new[] { "town", "village", "castle" })
            {
                var rootMenuId = $"dlc_barber_root_{parent}_menu";
                starter.AddGameMenuOption(
                    parent,                                   // e.g. "town"
                    $"dlc_barber_root_{parent}",               // unique option id
                    "{=DLC_CallForBarber}Call For Barber",         // text
                    args =>
                    {

                        args.optionLeaveType = GameMenuOption.LeaveType.Leaderboard;
                        return true;
                    },
                    args =>
                    {
                        var barber = Hero.MainHero.CurrentSettlement.Culture.Barber;
                        CampaignMapConversation.OpenConversation(
                            new ConversationCharacterData(CharacterObject.PlayerCharacter),
                            new ConversationCharacterData(barber)
                        );
                    },
                    false, 300, false, null
                );

            }

        }

    }
}

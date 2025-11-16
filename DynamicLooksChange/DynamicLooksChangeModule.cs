using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace DynamicLooksChange
{
    public class DynamicLooksChangeModule : MBSubModuleBase
    {
        public static string _module;
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(
                new InformationMessage("Dynamic Looks Change Mod loaded successfully."));

            var asm = Assembly.GetExecutingAssembly().Location;
            _module = asm;
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnGameStart(Game game, IGameStarter starter)
        {
            base.OnGameStart(game, starter);
            if (game.GameType is Campaign)
            {
                ((CampaignGameStarter)starter).AddBehavior(new DynamicLooksChangeBehavior());
            }
        }
    }
}

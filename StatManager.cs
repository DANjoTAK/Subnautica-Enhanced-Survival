using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using SurvivalFramework;

namespace EnhancedSurvival
{
    class StatManager
    {
        internal static Stat sleep { get; private set; }
        public const string SLEEP = "sleep";
        internal static Stat tiredness { get; private set; }
        public const string TIREDNESS = "tiredness";
        internal static Stat coffee { get; private set; }
        public const string COFFEE = "coffee";

        internal static Stat vision { get; private set; }
        public const string VISION = "vision";


        internal static void Loaded() {
            sleep = SurvivalFramework.StatManager.RegisterStat(new SurvivalFramework.StatManager.RegisterStatArgs(){name = SLEEP, displayName = "Sleep", value = 100f});
            tiredness = SurvivalFramework.StatManager.RegisterStat(new SurvivalFramework.StatManager.RegisterStatArgs(){name = TIREDNESS, displayName = "Tiredness", value = 0f});
            coffee = SurvivalFramework.StatManager.RegisterStat(new SurvivalFramework.StatManager.RegisterStatArgs(){name = COFFEE, displayName = "Coffee", hidden = true, value = 0f});
            vision = SurvivalFramework.StatManager.GetStat(VISION);
        }
        internal static void Unloaded()
        {
            sleep = null;
            tiredness = null;
            coffee = null;
            vision = null;
        }
    }
}

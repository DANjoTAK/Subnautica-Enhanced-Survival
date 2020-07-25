using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using SurvivalFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EnhancedSurvival
{
    public class Main
    {
        public static HarmonyInstance harmonyInstance { get; private set; }
        public static void Patch()
        {
            if (harmonyInstance != null) return;
            harmonyInstance = HarmonyInstance.Create("enhancedsurvival");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            SurvivalFramework.SceneManager.OnMainSceneStatusChanged += SceneManagerOnOnMainSceneStatusChanged;
            SurvivalFramework.StatManager.OnStatsLoaded += StatManagerOnOnStatsLoaded;
            SurvivalFramework.StatManager.OnStatsUnloaded += StatManagerOnOnStatsUnloaded;
            SettingsManager.Initialize();
        }

        private static void SceneManagerOnOnMainSceneStatusChanged(SceneManager.MainSceneStatusChangedEventArgs e)
        {
            if (e.isMainScene)
            {
                RestManager.MainSceneLoaded();
            }
            else
            {
                RestManager.MainSceneUnloaded();
            }
        }

        private static void StatManagerOnOnStatsLoaded(SurvivalFramework.StatManager.StatsLoadedEventArgs e)
        {
            StatManager.Loaded();
        }
        private static void StatManagerOnOnStatsUnloaded(SurvivalFramework.StatManager.StatsUnloadedEventArgs e)
        {
            StatManager.Unloaded();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using SurvivalFramework;

namespace EnhancedSurvival.Patches
{
    class BedPatcher
    {
        [HarmonyPatch(typeof(Bed))]
        [HarmonyPatch("GetCanSleep")]
        public class BedGetCanSleep
        {
            public static bool Prefix(ref bool __result, Bed __instance)
            {
                __result = RestManager.GetCanSleep(Player.main);
                return false;
            }
        }

        [HarmonyPatch(typeof(Bed))]
        [HarmonyPatch("OnHandClick")]
        private class HandPatch
        {
            private static bool Prefix(Bed __instance, GUIHand hand)
            {
                return RestManager.NotifyGetCanSleep(Player.main);
            }
        }

        [HarmonyPatch(typeof(Bed))]
        [HarmonyPatch("EnterInUseMode")]
        public class BedEnterInUseMode
        {
            public static void Prefix(Bed __instance)
            {
                __instance.kSleepGameTimeDuration = RestManager.sleepDurationGametime;
                __instance.kSleepRealTimeDuration = RestManager.sleepDurationRealtime;
            }

            public static void Postfix(Bed __instance)
            {
                RestManager.sleeping = true;
                RestManager.sleepLocation = RestManager.SleepLocation.Bed;
                RestManager.sleepStartTime = DayNightCycle.main.GetDayNightCycleTime();
                RestManager.sleepBed = __instance;
            }
        }

        [HarmonyPatch(typeof(Bed))]
        [HarmonyPatch("ExitInUseMode")]
        public class BedExitInUseMode
        {
            public static void Postfix(Bed __instance)
            {
                RestManager.sleeping = false;
                RestManager.sleepLocation = RestManager.SleepLocation.None;
                RestManager.sleepStartTime = 0;
                RestManager.sleepBed = null;
            }
        }
    }
}

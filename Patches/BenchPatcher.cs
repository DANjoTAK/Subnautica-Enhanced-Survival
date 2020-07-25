using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace EnhancedSurvival.Patches
{
    class BenchPatcher
    {
        [HarmonyPatch(typeof(Bench))]
        [HarmonyPatch("EnterSittingMode")]
        public class PatchBenchEnterSittingMode
        {
            public static void Postfix(Bench __instance)
            {
                RestManager.sitting = true;
                RestManager.sittingBench = __instance;
                Console.WriteLine("[Enhanced Survival] Now sitting");
            }
        }
        [HarmonyPatch(typeof(Bench))]
        [HarmonyPatch("ExitSittingMode")]
        public class PatchBenchExitSittingMode
        {
            public static void Postfix(Bench __instance)
            {
                RestManager.sitting = false;
                RestManager.sittingBench = null;
                Console.WriteLine("[Enhanced Survival] No longer sitting");
            }
        }
    }
}

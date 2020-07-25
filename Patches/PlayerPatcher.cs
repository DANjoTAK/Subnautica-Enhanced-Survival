using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace EnhancedSurvival.Patches
{
    class PlayerPatcher
    {
        [HarmonyPatch(typeof(Survival))]
        [HarmonyPatch("Eat")]
        public class PlayerEatPatch
        {
            public static void Postfix(Survival __instance, GameObject useObj)
            {
                if ((UnityEngine.Object)useObj != (UnityEngine.Object)null)
                {
                    TechType tt = CraftData.GetTechType(useObj);
                    if (TechType.Coffee == tt)
                    {
                        if (StatManager.coffee == null) return;
                        StatManager.coffee.Value += 12.5f;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Player))]
        [HarmonyPatch("ResetPlayerOnDeath")]
        public class PlayerResetPlayerOnDeath
        {
            public static void Postfix(Player __instance)
            {
                RestManager.ResetOnDeath();
            }
        }
    }
}

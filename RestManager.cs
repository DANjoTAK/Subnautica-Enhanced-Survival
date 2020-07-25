using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rail;
using SurvivalFramework;
using UnityEngine;

namespace EnhancedSurvival
{
    class RestManager
    {
        public static RestManagerComponent component { get; private set; }
        public static GameObject gameObject { get; private set; }

        internal static readonly float sleepDurationGametime = 1440f;
        internal static readonly float sleepDurationRealtime = 60f;

        internal static readonly float sleepFactorNormal = SurvivalHelper.GetChangeFactor(4320, -100);
        internal static readonly float sleepFactorSleepingEfficient = SurvivalHelper.GetChangeFactor(720, 100);
        internal static readonly float sleepFactorSleepingInefficient = SurvivalHelper.GetChangeFactor(1200, 100);
        internal static readonly float tirednessFactorNormal = SurvivalHelper.GetChangeFactor(2880, 100);
        internal static readonly float tirednessFactorSleepingEfficient = SurvivalHelper.GetChangeFactor(180, -100);
        internal static readonly float tirednessFactorSleepingInefficient = SurvivalHelper.GetChangeFactor(300, -100);
        internal static readonly float foodFactorSleeping = SurvivalHelper.GetChangeFactor(60, -4);
        internal static readonly float waterFactorSleeping = SurvivalHelper.GetChangeFactor(60, -5);
        internal static readonly float healthFactorSleepingEfficient = SurvivalHelper.GetChangeFactor(420, 100);
        internal static readonly float healthFactorSleepingInefficient = SurvivalHelper.GetChangeFactor(540, 100);
        internal static readonly float healthFactorDying = SurvivalHelper.GetChangeFactor(180, -100);
        internal static readonly float coffeeFactor = SurvivalHelper.GetChangeFactor(480, -100);

        internal static float lastTime = 0f;

        internal static bool sleeping = false;
        internal static float sleepStartTime = 0f;
        internal static SleepLocation sleepLocation = SleepLocation.None;
        internal static Bed sleepBed = null;

        internal static bool sitting = false;
        internal static SittingLocation sittingLocation = SittingLocation.None;
        internal static Bench sittingBench = null;

        internal static bool keySleepStartDown = false;
        internal static bool keySleepStopDown = false;

        internal static void MainSceneLoaded()
        {
            gameObject = new GameObject("RestManagerComponent");
            component = gameObject.AddComponent<RestManagerComponent>();
        }
        internal static void MainSceneUnloaded()
        {
            gameObject = null;
            component = null;

            sleeping = false;
            sleepStartTime = 0f;
            sleepLocation = SleepLocation.None;
            sleepBed = null;

            sitting = false;
            sittingLocation = SittingLocation.None;
            sittingBench = null;

            keySleepStartDown = false;
            keySleepStopDown = false;
        }

        internal static bool GetCanSleep(Player player)
        {
            if (player.GetMode() != Player.Mode.Normal ||
                (player.IsUnderwater() || DayNightCycle.main.IsInSkipTimeMode() || player.isPiloting || player.IsSwimming() || player.IsUnderwaterForSwimming() || player.movementSpeed != 0))
                return false;
            return true;
        }
        internal static bool NotifyGetCanSleep(Player player)
        {
            if (DayNightCycle.main.IsInSkipTimeMode()) return false;
            if (player.GetMode() != Player.Mode.Normal) return false;
            if (player.isPiloting) return false;
            if (player.movementSpeed != 0f) return false;
            if (player.IsUnderwater() || player.IsSwimming() || player.IsUnderwaterForSwimming()) { ErrorMessage.AddWarning("You can not sleep in the water."); return false; }
            if (SurvivalHelper.IsSurvival() && (StatManager.sleep.RealValue >= 100 || StatManager.tiredness.RealValue < 25 || StatManager.coffee.RealValue >= 20)) {
                if (StatManager.coffee.RealValue >= 20) {
                    ErrorMessage.AddWarning("You shouldn't drink coffee before going to sleep!");
                } else if (StatManager.sleep.RealValue >= 100 || StatManager.tiredness.RealValue < 25) {
                    ErrorMessage.AddWarning("You aren't tired enough!");
                }
                return false;
            }
            Survival pSurvival = player.GetComponent<Survival>();
            if (SurvivalHelper.IsSurvival() && (pSurvival.food <= 20f || pSurvival.water <= 20f)) {
                if (pSurvival.food <= 20f && pSurvival.water <= 20f) {
                    ErrorMessage.AddWarning("You need to eat and drink something before going to sleep!");
                } else if (pSurvival.food <= 20f) {
                    ErrorMessage.AddWarning("You need to eat something before going to sleep!");
                } else if (pSurvival.water <= 20f) {
                    ErrorMessage.AddWarning("You need to drink something before going to sleep!");
                }
                return false;
            }
            return true;
        }
        internal static void StartSleep(Player player)
        {
            if (sleeping) return;
            if (player.cinematicModeActive) return;
            if (DayNightCycle.main.IsInSkipTimeMode()) return;
            if (!GetCanSleep(Player.main)) return;
            player.cinematicModeActive = true;
            MainCameraControl.main.viewModel.localRotation = Quaternion.identity;
            DayNightCycle.main.SkipTime(sleepDurationGametime, sleepDurationRealtime);
            uGUI_PlayerSleep.main.StartSleepScreen();
            sleeping = true;
            sleepLocation = SleepLocation.Floor;
            sleepStartTime = DayNightCycle.main.GetDayNightCycleTime();
        }
        internal static void StopSleep(Player player, bool skipAnims = false)
        {
            if (!sleeping) return;
            //Subscribe(player,false);
            if (sleepLocation == SleepLocation.Bed)
            {
                sleepBed.ExitInUseMode(player, skipAnims);
            }
            else if (sleepLocation == SleepLocation.Floor)
            {
                sleeping = false;
                if (DayNightCycle.main.IsInSkipTimeMode())
                {
                    DayNightCycle.main.StopSkipTimeMode();
                }
                player.timeLastSleep = DayNightCycle.main.timePassedAsFloat;
                uGUI_PlayerSleep.main.StopSleepScreen();

                MainCameraControl.main.lookAroundMode = false;
                player.cinematicModeActive = false;
                sleepLocation = SleepLocation.None;
                sleepStartTime = 0;
            }
        }
        internal static void ResetOnDeath()
        {
            StatManager.coffee.Value = 0f;
            StatManager.tiredness.Value = 0f;
            StatManager.sleep.Value = 100f;
        }


        internal enum SleepLocation
        {
            None,
            Bed,
            Floor
        }
        internal enum SittingLocation
        {
            None,
            Bench,
            Chair
        }

        public class RestManagerComponent : MonoBehaviour
        {
            public void Awake()
            {

            }

            public void Update()
            {
                if (SceneManager.isIngame)
                {
                    if (StatManager.sleep == null) return;
                    if (StatManager.tiredness == null) return;
                    if (StatManager.coffee == null) return;
                    if (StatManager.vision == null) return;
                    if (sleeping)
                    {
                        if (sleepLocation == SleepLocation.Floor && !DayNightCycle.main.IsInSkipTimeMode())
                        {
                            StopSleep(Player.main, false);
                        }
                    }
                    if (InputHelper.IsKeyDown(SettingsManager.keySleepStart) && !keySleepStartDown) {
                        keySleepStartDown = true;
                        if (!sleeping)
                        {
                            if (NotifyGetCanSleep(Player.main)) {
                                if (SettingsManager.keySleepStart == SettingsManager.keySleepStop) keySleepStopDown = true;
                                StartSleep(Player.main);
                            }
                        }
                    } else if (!InputHelper.IsKeyDown(SettingsManager.keySleepStart) && keySleepStartDown) {
                        keySleepStartDown = false;
                    }
                    if (InputHelper.IsKeyDown(SettingsManager.keySleepStop) && !keySleepStopDown) {
                        keySleepStopDown = true;
                        if (sleeping)
                        { 
                            if (SettingsManager.keySleepStart == SettingsManager.keySleepStop) keySleepStartDown = true; 
                            StopSleep(Player.main, false);
                        }
                    } else if (!InputHelper.IsKeyDown(SettingsManager.keySleepStop) && keySleepStopDown) {
                        keySleepStopDown = false;
                    }
                    float oldTime = lastTime;
                    float newTime = SurvivalHelper.GetTime();
                    float timePassed = newTime - oldTime;
                    float timePassedHours = (float)(timePassed / (1d / 24));
                    float timePassedMinutes = (float)(timePassedHours * 60);
                    if (timePassedMinutes >= 1)
                    {
                        lastTime = newTime;
                        if (SurvivalHelper.IsSurvival()) {
                            StatManager.coffee.Value += SurvivalHelper.GetChangeValue(timePassedMinutes, coffeeFactor);
                            float oldCoffeeMod = StatManager.tiredness.GetModifier("coffee");
                            float newCoffeeMod = (StatManager.coffee.RealValue / 100) * -30f;
                            if (Math.Abs(oldCoffeeMod - newCoffeeMod) >= 0.5f) StatManager.tiredness.SetModifier("coffee", Stat.ModifierType.ValueAbsolute, newCoffeeMod);
                        } else {
                            float oldCoffeeMod = StatManager.tiredness.GetModifier("coffee");
                            if (oldCoffeeMod != 0f) StatManager.tiredness.SetModifier("coffee", Stat.ModifierType.ValueAbsolute, 0f);
                        }
                        if (sleeping) {
                            if (sleepLocation == SleepLocation.Bed) {
                                float hpOut = Player.main.liveMixin.health + SurvivalHelper.GetChangeValue(timePassedMinutes, healthFactorSleepingEfficient);
                                hpOut = hpOut > 100 ? 100 : hpOut < 0 ? 0 : hpOut;
                                Player.main.liveMixin.health = hpOut;
                                if (SurvivalHelper.IsSurvival())
                                {
                                    StatManager.sleep.Value +=
                                        SurvivalHelper.GetChangeValue(timePassedMinutes, sleepFactorSleepingEfficient);
                                    StatManager.tiredness.Value += SurvivalHelper.GetChangeValue(timePassedMinutes,
                                        tirednessFactorSleepingEfficient);
                                    if (StatManager.sleep.RealValue >= 100)
                                    {
                                        StopSleep(Player.main, false);
                                    }
                                }
                            } else {
                                float hpOut = Player.main.liveMixin.health + SurvivalHelper.GetChangeValue(timePassedMinutes, healthFactorSleepingInefficient);
                                hpOut = hpOut > 100 ? 100 : hpOut < 0 ? 0 : hpOut;
                                Player.main.liveMixin.health = hpOut;
                                if (SurvivalHelper.IsSurvival())
                                {
                                    StatManager.sleep.Value += SurvivalHelper.GetChangeValue(timePassedMinutes,
                                        sleepFactorSleepingInefficient);
                                    StatManager.tiredness.Value += SurvivalHelper.GetChangeValue(timePassedMinutes,
                                        tirednessFactorSleepingInefficient);
                                    if (StatManager.sleep.RealValue >= 100)
                                    {
                                        StopSleep(Player.main, false);
                                    }
                                }
                            }
                            if (SurvivalHelper.IsSurvival())
                            {
                                Survival pSurvival = Player.main.GetComponent<Survival>();
                                pSurvival.food += SurvivalHelper.GetChangeValue(timePassedMinutes,
                                    foodFactorSleeping);
                                pSurvival.water += SurvivalHelper.GetChangeValue(timePassedMinutes,
                                    waterFactorSleeping);
                                if (pSurvival.food <= 10f || pSurvival.water <= 10f)
                                {
                                    StopSleep(Player.main, false);
                                    if (pSurvival.food <= 10f) pSurvival.foodWarningSounds[1].Play();
                                    else if (pSurvival.water <= 10f) pSurvival.waterWarningSounds[1].Play();
                                }
                            }
                        } else if (sitting) {
                            if (SurvivalHelper.IsSurvival())
                            {
                                StatManager.sleep.Value +=
                                    SurvivalHelper.GetChangeValue(timePassedMinutes, sleepFactorNormal);
                            }
                            // TODO Find a good way to replenish stamina (TBD) and tiredness.
                        } else {
                            if (SurvivalHelper.IsSurvival())
                            {
                                StatManager.sleep.Value +=
                                    SurvivalHelper.GetChangeValue(timePassedMinutes, sleepFactorNormal);
                                StatManager.tiredness.Value +=
                                    SurvivalHelper.GetChangeValue(timePassedMinutes, tirednessFactorNormal);
                            }
                        }
                        if (SurvivalHelper.IsSurvival())
                        {
                            if (StatManager.sleep.RealValue <= 0) {
                                Player.main.liveMixin.TakeDamage(-SurvivalHelper.GetChangeValue(timePassedMinutes, healthFactorDying), default, DamageType.Starve);
                            }
                            //TODO Change modifier type to relative?
                            if (StatManager.tiredness.RealValue >= 75) {
                                float oldVisionMod = StatManager.vision.GetModifier("tiredness");
                                float newVisionMod = 40f + ((StatManager.tiredness.RealValue - 75f) / 25f) * 50f;
                                if (Math.Abs(oldVisionMod - newVisionMod) >= 0.1f) StatManager.vision.SetModifier("tiredness", Stat.ModifierType.ValueAbsolute, -newVisionMod);
                            } else if (StatManager.tiredness.RealValue >= 50) {
                                float oldVisionMod = StatManager.vision.GetModifier("tiredness");
                                float newVisionMod = 10f + ((StatManager.tiredness.RealValue - 50f) / 25f) * 30f;
                                if (Math.Abs(oldVisionMod - newVisionMod) >= 0.1f) StatManager.vision.SetModifier("tiredness", Stat.ModifierType.ValueAbsolute, -newVisionMod);
                            } else if (StatManager.tiredness.RealValue >= 25) {
                                float oldVisionMod = StatManager.vision.GetModifier("tiredness");
                                float newVisionMod = ((StatManager.tiredness.RealValue - 25f) / 25f) * 10f;
                                if (Math.Abs(oldVisionMod - newVisionMod) >= 0.1f) StatManager.vision.SetModifier("tiredness", Stat.ModifierType.ValueAbsolute, -newVisionMod);
                            } else {
                                float oldVisionMod = StatManager.vision.GetModifier("tiredness");
                                if (oldVisionMod != 0f) StatManager.vision.SetModifier("tiredness", Stat.ModifierType.ValueAbsolute, 0f);
                            }
                            //TODO Modify walking and swimming
                        } else {
                            float oldVisionMod = StatManager.vision.GetModifier("tiredness");
                            if (oldVisionMod != 0f) StatManager.vision.SetModifier("tiredness", Stat.ModifierType.ValueAbsolute, 0f);
                        }
                    }
                }
                else
                {
                    lastTime = SurvivalHelper.GetTime();
                }
            }
        }
    }
}

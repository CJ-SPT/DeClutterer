using EFT.Weather;
using HarmonyLib;
using System.Reflection;
using SPT.Reflection.Patching;
using TYR_DeClutterer.Utils;

namespace TYR_DeClutterer.Patches
{ 
    public class WeatherLateUpdatePatch : ModulePatch
    {
        private static bool _everyOtherLateUpdate;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WeatherController), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix(WeatherController __instance, ToDController ___TimeOfDayController)
        {
            if (!Configuration.FramesaverWeatherUpdatesEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            _everyOtherLateUpdate = !_everyOtherLateUpdate;

            if (_everyOtherLateUpdate)
            {
                ___TimeOfDayController.Update();
                __instance.method_4();
            }
            
            return false;
        }
    }

    public class SkyDelayUpdatesPatch : ModulePatch
    {
        private static bool _everyOtherLateUpdate;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TOD_Sky), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix(TOD_Sky __instance)
        {
            if (!Configuration.FramesaverWeatherUpdatesEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            _everyOtherLateUpdate = !_everyOtherLateUpdate;

            if (_everyOtherLateUpdate)
            {
                __instance.method_17();
                __instance.method_18();
                __instance.method_0();
                __instance.method_1();
                __instance.method_2();
                __instance.method_3();
            }
            
            return false;
        }
    }

    internal class WeatherEventControllerDelayUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WeatherEventController).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Configuration.FramesaverWeatherUpdatesEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }
}
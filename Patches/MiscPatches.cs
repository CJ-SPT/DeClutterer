using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Reflection;
using Comfort.Common;
using SPT.Reflection.Patching;
using TYR_DeClutterer.Utils;

namespace TYR_DeClutterer.Patches
{
    public class PhysicsUpdatePatch : ModulePatch
    {
        private static bool _everyOtherFixedUpdate;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EFTPhysicsClass), nameof(EFTPhysicsClass.Update));
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (!Configuration.FramesaverPhysicsEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            _everyOtherFixedUpdate = !_everyOtherFixedUpdate;
            
            if (_everyOtherFixedUpdate)
            {
                EFTPhysicsClass.GClass711.Update();
                EFTPhysicsClass.GClass712.Update();
            }
            
            return false;
        }
    }

    public class PhysicsFixedUpdatePatch : ModulePatch
    {
        private static bool _everyOtherFixedUpdate = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EFTPhysicsClass), nameof(EFTPhysicsClass.FixedUpdate));
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (!Configuration.FramesaverPhysicsEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            if (!Singleton<GameWorld>.Instantiated) 
                return true;
            
            _everyOtherFixedUpdate = !_everyOtherFixedUpdate;
            
            if (_everyOtherFixedUpdate)
            {
                EFTPhysicsClass.GClass711.FixedUpdate();
            }
            
            return false;
        }
    }

    public class RagdollPhysicsLateUpdatePatch : ModulePatch
    {
        private static bool _everyOtherFixedUpdate;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CorpseRagdollTestApplication), "LateUpdate");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (!Configuration.FramesaverPhysicsEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            _everyOtherFixedUpdate = !_everyOtherFixedUpdate;
            
            if (_everyOtherFixedUpdate)
            {
                EFTPhysicsClass.SyncTransforms();
            }
            
            return false;
        }
    }

    public class FlameDamageTriggerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(FlameDamageTrigger), "ProceedDamage");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            if (!Configuration.FramesaverFireAndSmokeEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }
}
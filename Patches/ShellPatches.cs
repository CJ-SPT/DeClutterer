using System.Reflection;
using SPT.Reflection.Patching;
using TYR_DeClutterer.Utils;

namespace TYR_DeClutterer.Patches
{
    // DontSpawnShellsFiringPatch removes the spawning of spent shell casings when firing a gun.
    // Very cool, but it has an expensive update cycle in GameWorld to clean them up.
    internal class DontSpawnShellsFiringPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WeaponManagerClass).GetMethod("method_9", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Configuration.FramesaverShellChangesEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }

    // DontSpawnShellsJamPatch does similar to DontSpawnShellsFiringPatch, but removes the
    // processing for clearing a jam.
    internal class DontSpawnShellsJamPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WeaponManagerClass).GetMethod("SpawnShellAfterJam", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Configuration.FramesaverShellChangesEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            return false;
        }
    }

    internal class DontSpawnShellsAtAllReallyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(WeaponManagerClass).GetMethod("method_4", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix(bool __result)
        {
            if (!Configuration.FramesaverShellChangesEnabledConfig.Value || !Configuration.FramesaverEnabledConfig.Value)
                return true;

            __result = false;
            return false;
        }
    }
}
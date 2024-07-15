using BepInEx.Configuration;

namespace TYR_DeClutterer.Utils
{
    internal class Configuration
    {
        public static ConfigEntry<bool> declutterEnabledConfig;
        public static ConfigEntry<bool> declutterGarbageEnabledConfig;
        public static ConfigEntry<bool> declutterHeapsEnabledConfig;
        public static ConfigEntry<bool> declutterSpentCartridgesEnabledConfig;
        public static ConfigEntry<bool> declutterFakeFoodEnabledConfig;
        public static ConfigEntry<bool> declutterDecalsEnabledConfig;
        public static ConfigEntry<bool> declutterPuddlesEnabledConfig;
        public static ConfigEntry<bool> declutterShardsEnabledConfig;
        public static ConfigEntry<float> declutterScaleOffsetConfig;

        public static ConfigEntry<bool> framesaverEnabledConfig;
        public static ConfigEntry<bool> framesaverPhysicsEnabledConfig;
        public static ConfigEntry<bool> framesaverParticlesEnabledConfig;
        public static ConfigEntry<bool> framesaverShellChangesEnabledConfig;
        public static ConfigEntry<bool> framesaverSoftVegetationEnabledConfig;
        public static ConfigEntry<bool> framesaverReflectionsEnabledConfig;
        public static ConfigEntry<bool> framesaverLightingShadowCascadesEnabledConfig;
        public static ConfigEntry<bool> framesaverWeatherUpdatesEnabledConfig;
        public static ConfigEntry<bool> framesaverTexturesEnabledConfig;
        public static ConfigEntry<bool> framesaverLODEnabledConfig;
        public static ConfigEntry<bool> framesaverFireAndSmokeEnabledConfig;

        public static ConfigEntry<bool> framesaverLateUpdateEnabledConfig;
        public static ConfigEntry<int> framesaverParticleBudgetDividerConfig;

        public static ConfigEntry<int> framesaverPixelLightDividerConfig;
        public static ConfigEntry<int> framesaverShadowDividerConfig;
        public static ConfigEntry<int> framesaverTextureSizeConfig;
        public static ConfigEntry<float> framesaverLODBiasConfig;

        public static void Bind(ConfigFile Config)
        {
            declutterEnabledConfig = Config.Bind(
                "A - De-Clutter Enabler",
                "A - De-Clutterer Enabled",
                true,
                "Enables the De-Clutterer");

            declutterScaleOffsetConfig = Config.Bind(
                "A - De-Clutter Enabler",
                "B - De-Clutterer Scaler",
                1f,
                new ConfigDescription("Larger Scale = Larger the Clutter Removed.",
                new AcceptableValueRange<float>(0.5f, 2f)));

            declutterGarbageEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "A - Garbage & Litter De-Clutter",
                true,
                "De-Clutters things labeled 'garbage' or similar. Smaller garbage piles.");

            declutterHeapsEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "B - Heaps & Piles De-Clutter",
                true,
                "De-Clutters things labeled 'heaps' or similar. Larger garbage piles.");

            declutterSpentCartridgesEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "C - Spent Cartridges De-Clutter",
                true,
                "De-Clutters pre-generated spent ammunition on floor.");

            declutterFakeFoodEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "D - Fake Food De-Clutter",
                true,
                "De-Clutters fake 'food' items.");

            declutterDecalsEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "E - Decal De-Clutter",
                true,
                "De-Clutters decals (Blood, grafiti, etc.)");

            declutterPuddlesEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "F - Puddle De-Clutter",
                true,
                "De-Clutters fake reflective puddles.");

            declutterShardsEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "G - Glass & Tile Shards",
                true,
                "De-Clutters things labeled 'shards' or similar. The things you can step on that make noise.");

            framesaverEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "A - Framesaver Enabled",
                false,
                "Enables Ari's Framesaver methods, with some of my additions.");

            framesaverPhysicsEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "B - Physics Changes",
                false,
                "Experimental physics optimization, runs physics at half speed.");

            framesaverShellChangesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "C - Shell Spawn Changes",
                false,
                "Stops spent cartride shells from spawning.");

            framesaverParticlesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "D - Particle Changes",
                false,
                "Enables particle changes.");

            framesaverFireAndSmokeEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "E - Fire & Smoke Changes",
                false,
                "Removes map-baked Fire and Smoke effects.");

            framesaverSoftVegetationEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "F - Vegetation Changes",
                false,
                "Enables vegetation changes.");

            framesaverReflectionsEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "G - Reflection Changes",
                false,
                "Enables reflection changes.");
            
            framesaverLightingShadowCascadesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "H - Shadow Cascade Changes",
                false,
                "Enables shadow cascade changes.");

            framesaverWeatherUpdatesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "I - Cloud & Weather Changes",
                false,
                "Enables Cloud Shadow & Weather changes.");

            framesaverTexturesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "J - Texture Changes",
                false,
                "Enables texture changes.");

            framesaverLODEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "K - LOD Changes",
                false,
                "Enables LOD changes.");

            framesaverParticleBudgetDividerConfig = Config.Bind(
                "D - Framesaver Settings",
                "A - Particle Quality Divider",
                1,
                new ConfigDescription("1 is default, Higher number = Lower Particle Quality.",
                new AcceptableValueRange<int>(1, 4)));

            framesaverPixelLightDividerConfig = Config.Bind(
                "D - Framesaver Settings",
                "B - Lighting Quality Divider",
                1,
                new ConfigDescription("1 is default, Higher number = Lower Lighting Quality.",
                new AcceptableValueRange<int>(1, 4)));

            framesaverShadowDividerConfig = Config.Bind(
                "D - Framesaver Settings",
                "C - Shadow Quality Divider",
                1,
                new ConfigDescription("1 is default, Higher number = Lower Shadow Quality.",
                new AcceptableValueRange<int>(1, 4)));

            framesaverTextureSizeConfig = Config.Bind(
                "D - Framesaver Settings",
                "D - Texture Size Divider",
                1,
                new ConfigDescription("1 is default, Higher number = Lower Texture Quality.",
                new AcceptableValueRange<int>(1, 6)));

            framesaverLODBiasConfig = Config.Bind(
                "D - Framesaver Settings",
                "E - LOD Bias Reducer",
                1.0f,
                new ConfigDescription("1 is default, Higher number = Lower Model Quality.",
                new AcceptableValueRange<float>(1.0f, 2.0f)));
        }
    }
}
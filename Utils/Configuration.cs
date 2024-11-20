using BepInEx.Configuration;

namespace TYR_DeClutterer.Utils
{
    internal class Configuration
    {
        public static ConfigEntry<bool> DeclutterEnabledConfig;
        public static ConfigEntry<bool> DeclutterGarbageEnabledConfig;
        public static ConfigEntry<bool> DeclutterHeapsEnabledConfig;
        public static ConfigEntry<bool> DeclutterSpentCartridgesEnabledConfig;
        public static ConfigEntry<bool> DeclutterFakeFoodEnabledConfig;
        public static ConfigEntry<bool> DeclutterDecalsEnabledConfig;
        public static ConfigEntry<bool> DeclutterPuddlesEnabledConfig;
        public static ConfigEntry<bool> DeclutterShardsEnabledConfig;
        public static ConfigEntry<float> DeclutterScaleOffsetConfig;

        public static ConfigEntry<bool> FramesaverEnabledConfig;
        public static ConfigEntry<bool> FramesaverPhysicsEnabledConfig;
        public static ConfigEntry<bool> FramesaverParticlesEnabledConfig;
        public static ConfigEntry<bool> FramesaverShellChangesEnabledConfig;
        public static ConfigEntry<bool> FramesaverSoftVegetationEnabledConfig;
        public static ConfigEntry<bool> FramesaverReflectionsEnabledConfig;
        public static ConfigEntry<bool> FramesaverLightingShadowCascadesEnabledConfig;
        public static ConfigEntry<bool> FramesaverWeatherUpdatesEnabledConfig;
        public static ConfigEntry<bool> FramesaverTexturesEnabledConfig;
        public static ConfigEntry<bool> FramesaverLODEnabledConfig;
        public static ConfigEntry<bool> FramesaverFireAndSmokeEnabledConfig;
        
        public static ConfigEntry<int> FramesaverParticleBudgetDividerConfig;

        public static ConfigEntry<int> FramesaverPixelLightDividerConfig;
        public static ConfigEntry<int> FramesaverShadowDividerConfig;
        public static ConfigEntry<int> FramesaverTextureSizeConfig;
        public static ConfigEntry<float> FramesaverLODBiasConfig;

        public static void Bind(ConfigFile Config)
        {
            DeclutterEnabledConfig = Config.Bind(
                "A - De-Clutter Enabler",
                "A - De-Clutterer Enabled",
                true,
                "Enables the De-Clutterer");

            DeclutterScaleOffsetConfig = Config.Bind(
                "A - De-Clutter Enabler",
                "B - De-Clutterer Scaler",
                1f,
                new ConfigDescription("Larger Scale = Larger the Clutter Removed.",
                new AcceptableValueRange<float>(0.5f, 2f)));

            DeclutterGarbageEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "A - Garbage & Litter De-Clutter",
                true,
                "De-Clutters things labeled 'garbage' or similar. Smaller garbage piles.");

            DeclutterHeapsEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "B - Heaps & Piles De-Clutter",
                true,
                "De-Clutters things labeled 'heaps' or similar. Larger garbage piles.");

            DeclutterSpentCartridgesEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "C - Spent Cartridges De-Clutter",
                true,
                "De-Clutters pre-generated spent ammunition on floor.");

            DeclutterFakeFoodEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "D - Fake Food De-Clutter",
                true,
                "De-Clutters fake 'food' items.");

            DeclutterDecalsEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "E - Decal De-Clutter",
                true,
                "De-Clutters decals (Blood, grafiti, etc.)");

            DeclutterPuddlesEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "F - Puddle De-Clutter",
                true,
                "De-Clutters fake reflective puddles.");

            DeclutterShardsEnabledConfig = Config.Bind(
                "B - De-Clutter Settings",
                "G - Glass & Tile Shards",
                true,
                "De-Clutters things labeled 'shards' or similar. The things you can step on that make noise.");

            FramesaverEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "A - Framesaver Enabled",
                false,
                "Enables Ari's Framesaver methods, with some of my additions.");

            FramesaverPhysicsEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "B - Physics Changes",
                false,
                "Experimental physics optimization, runs physics at half speed.");

            FramesaverShellChangesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "C - Shell Spawn Changes",
                false,
                "Stops spent cartride shells from spawning.");

            FramesaverParticlesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "D - Particle Changes",
                false,
                "Enables particle changes.");

            FramesaverFireAndSmokeEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "E - Fire & Smoke Changes",
                false,
                "Removes map-baked Fire and Smoke effects.");

            FramesaverSoftVegetationEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "F - Vegetation Changes",
                false,
                "Enables vegetation changes.");

            FramesaverReflectionsEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "G - Reflection Changes",
                false,
                "Enables reflection changes.");
            
            FramesaverLightingShadowCascadesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "H - Shadow Cascade Changes",
                false,
                "Enables shadow cascade changes.");

            FramesaverWeatherUpdatesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "I - Cloud & Weather Changes",
                false,
                "Enables Cloud Shadow & Weather changes.");

            FramesaverTexturesEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "J - Texture Changes",
                false,
                "Enables texture changes.");

            FramesaverLODEnabledConfig = Config.Bind(
                "C - Framesaver Enabler",
                "K - LOD Changes",
                false,
                "Enables LOD changes.");

            FramesaverParticleBudgetDividerConfig = Config.Bind(
                "D - Framesaver Settings",
                "A - Particle Quality Divider",
                1,
                new ConfigDescription("1 is default, Higher number = Lower Particle Quality.",
                new AcceptableValueRange<int>(1, 4)));

            FramesaverPixelLightDividerConfig = Config.Bind(
                "D - Framesaver Settings",
                "B - Lighting Quality Divider",
                1,
                new ConfigDescription("1 is default, Higher number = Lower Lighting Quality.",
                new AcceptableValueRange<int>(1, 4)));

            FramesaverShadowDividerConfig = Config.Bind(
                "D - Framesaver Settings",
                "C - Shadow Quality Divider",
                1,
                new ConfigDescription("1 is default, Higher number = Lower Shadow Quality.",
                new AcceptableValueRange<int>(1, 4)));

            FramesaverTextureSizeConfig = Config.Bind(
                "D - Framesaver Settings",
                "D - Texture Size Divider",
                1,
                new ConfigDescription("1 is default, Higher number = Lower Texture Quality.",
                new AcceptableValueRange<int>(1, 6)));

            FramesaverLODBiasConfig = Config.Bind(
                "D - Framesaver Settings",
                "E - LOD Bias Reducer",
                1.0f,
                new ConfigDescription("1 is default, Higher number = Lower Model Quality.",
                new AcceptableValueRange<float>(1.0f, 2.0f)));
        }
    }
}
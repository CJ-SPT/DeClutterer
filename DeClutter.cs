using BepInEx;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.Ballistics;
using EFT.Interactive;
using Koenigz.PerfectCulling.EFT;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DrakiaXYZ.VersionChecker;
using TYR_DeClutterer.Patches;
using TYR_DeClutterer.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TYR_DeClutterer
{
    [BepInPlugin("com.TYR.DeClutter", "TYR_DeClutter", "1.2.5")]
    [BepInDependency("com.SPT.custom", "3.10.0")]
    public class DeClutter : BaseUnityPlugin
    {
        public const int TarkovVersion = 33420;
        
        private static string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static bool DefaultsoftParticles;
        public static int DefaultparticleRaycastBudget;
        public static bool DefaultsoftVegetation;
        public static bool DefaultrealtimeReflectionProbes;
        public static int DefaultpixelLightCount;
        public static ShadowQuality DefaultShadows;
        public static int DefaultshadowCascades;
        public static int DefaultmasterTextureLimit;
        public static float DefaultlodBias;

        private static GameWorld _gameWorld;
        private List<GameObject> _allGameObjectsList = [];
        private static List<GameObject> _savedClutterObjects = [];
        private static ClutterNameStruct _cleanUpNames;
        private static bool _deCluttered;

        private static bool MapLoaded() => Singleton<GameWorld>.Instantiated;

        private Dictionary<string, bool> _dontDisableDictionary = new()
        {
            { "item_", true },
            { "weapon_", true },
            { "barter_", true },
            { "mod_", true },
            { "audio", true },
            { "container", true },
            { "trigger", true },
            { "culling", true },
            { "collider", true },
            { "colider", true },
            { "group", true },
            { "manager", true },
            { "scene", true },
            { "player", true },
            { "portal", true },
            { "bakelod", true },
            { "door", true },
            { "shadow", true },
            { "mine", true }
        };

        private Dictionary<string, bool> _clutterNameDictionary = [];

        private void Awake()
        {
            if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception("Invalid EFT Version");
            }
            
            Configuration.Bind(Config);
            
            new PhysicsUpdatePatch().Enable();
            new PhysicsFixedUpdatePatch().Enable();
            new RagdollPhysicsLateUpdatePatch().Enable();
            
            new DontSpawnShellsFiringPatch().Enable();
            new DontSpawnShellsJamPatch().Enable();
            new DontSpawnShellsAtAllReallyPatch().Enable();

            new SkyDelayUpdatesPatch().Enable();
            new WeatherLateUpdatePatch().Enable();
            new WeatherEventControllerDelayUpdatesPatch().Enable();
        }

        private void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            SubscribeConfig();

            Initialize_clutterNameDictionary();
            
            DefaultsoftParticles = QualitySettings.softParticles;
            DefaultparticleRaycastBudget = QualitySettings.particleRaycastBudget;
            DefaultsoftVegetation = QualitySettings.softVegetation;
            DefaultrealtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
            DefaultpixelLightCount = QualitySettings.pixelLightCount;
            DefaultShadows = QualitySettings.shadows;
            DefaultshadowCascades = QualitySettings.shadowCascades;
            DefaultmasterTextureLimit = QualitySettings.masterTextureLimit;
            DefaultlodBias = QualitySettings.lodBias;
        }

        private void Update()
        {
            if (!MapLoaded() || _deCluttered || !Configuration.DeclutterEnabledConfig.Value)
                return;

            _gameWorld = Singleton<GameWorld>.Instance;
            
            if (_gameWorld is null || IsInHideout()) return;
            
            _deCluttered = true;

            DeClutterScene();
            OnApplyVisualsChanged();
        }

        private void Initialize_clutterNameDictionary()
        {
            var cleanUpJsonText = File.ReadAllText(Path.Combine(PluginFolder, "CleanUpNames.json"));
            _cleanUpNames = JsonConvert.DeserializeObject<ClutterNameStruct>(cleanUpJsonText);

            BuildClutterNameDict(null, null);
        }

        private void BuildClutterNameDict(object sender, EventArgs e)
        {
            _clutterNameDictionary.Clear();

            _clutterNameDictionary = Configuration.DeclutterGarbageEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Garbage)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.DeclutterHeapsEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Heaps)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.DeclutterSpentCartridgesEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.SpentCartridges)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.DeclutterFakeFoodEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.FoodDrink)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.DeclutterDecalsEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Decals)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.DeclutterPuddlesEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Puddles)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.DeclutterShardsEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Shards)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;
        }

        private void SubscribeConfig()
        {
            Configuration.DeclutterEnabledConfig.SettingChanged += OnApplyDeclutterSettingChanged;
            Configuration.DeclutterGarbageEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.DeclutterHeapsEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.DeclutterSpentCartridgesEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.DeclutterFakeFoodEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.DeclutterDecalsEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.DeclutterPuddlesEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.DeclutterShardsEnabledConfig.SettingChanged += BuildClutterNameDict;

            Configuration.FramesaverEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverPhysicsEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverShellChangesEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverParticlesEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverSoftVegetationEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverReflectionsEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverWeatherUpdatesEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverTexturesEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverLODEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverParticleBudgetDividerConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverPixelLightDividerConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverShadowDividerConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverTextureSizeConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverLODBiasConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.FramesaverFireAndSmokeEnabledConfig.SettingChanged += OnApplyVisualsChanged;
        }

        private void OnApplyVisualsChanged(object sender, EventArgs e)
        {
            OnApplyVisualsChanged();
        }

        private void OnApplyVisualsChanged()
        {
            if (Configuration.FramesaverEnabledConfig.Value)
            {
                GraphicsUtils.SetParticlesQuality();

                GraphicsUtils.SetSoftVegetationQuality();

                GraphicsUtils.SetReflectionQuality();

                GraphicsUtils.SetLightingShadowQuality();

                GraphicsUtils.SetTextureQuality();

                GraphicsUtils.SetLodBiasQuality();
            }
            else
            {
                GraphicsUtils.SetDefaultQualityForAll();
            }
        }

        private void OnApplyDeclutterSettingChanged(object sender, EventArgs e)
        {
            if (!_deCluttered) return;
            
            if (Configuration.DeclutterEnabledConfig.Value)
            {
                DeClutterEnabled();
            }
            else
            {
                ReClutterEnabled();
            }
        }

        private void DeClutterEnabled()
        {
            foreach (var obj in _savedClutterObjects)
            {
                if (obj.activeSelf == true)
                {
                    obj.SetActive(false);
                }
            }
        }

        private void ReClutterEnabled()
        {
            foreach (var obj in _savedClutterObjects)
            {
                if (obj.activeSelf == false)
                {
                    obj.SetActive(true);
                }
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            _allGameObjectsList.Clear();
            _savedClutterObjects.Clear();
            _deCluttered = false;
        }

        private bool IsInHideout()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            
            return gameWorld is not null && gameWorld is HideoutGameWorld;
        }

        private void DeClutterScene()
        {
            StaticManager.BeginCoroutine(GetAllGameObjectsInSceneCoroutine());
            StaticManager.BeginCoroutine(DeClutterGameObjects());
        }

        private IEnumerator DeClutterGameObjects()
        {
            // Loop until the coroutine has finished
            while (true)
            {
                if (_allGameObjectsList != null && _allGameObjectsList.Count > 0)
                {
                    // Coroutine has finished, and allGameObjectsList is populated
                    var allGameObjectsArray = _allGameObjectsList.ToArray();
                    foreach (var obj in allGameObjectsArray)
                    {
                        if (obj is not null && ShouldDisableObject(obj))
                        {
                            obj.SetActive(false);
                        }
                    }
                }
                
                yield break;
            }
        }

        private IEnumerator GetAllGameObjectsInSceneCoroutine()
        {
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in gameObjects)
            {
                var isLODGroup = obj.GetComponent<LODGroup>() != null;
                var isStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() != null;
                var isParticleSystem = obj.GetComponent<ParticleSystem>() != null;
                var isGoodThing = isLODGroup || isStaticDeferredDecal || isParticleSystem;

                if (Configuration.FramesaverFireAndSmokeEnabledConfig.Value)
                {
                    if (Configuration.DeclutterDecalsEnabledConfig.Value)
                    {
                        isGoodThing = isLODGroup || isStaticDeferredDecal || isParticleSystem;
                    }
                    else
                    {
                        isGoodThing = isLODGroup || isParticleSystem;
                    }
                }
                else
                {
                    if (Configuration.DeclutterDecalsEnabledConfig.Value)
                    {
                        isGoodThing = isLODGroup || isStaticDeferredDecal;
                    }
                    else
                    {
                        isGoodThing = isLODGroup;
                    }
                }

                if (isGoodThing && !IsBadThing(obj))
                {
                    _allGameObjectsList.Add(obj);
                }
            }

            yield break;
        }

        private bool ShouldDisableObject(GameObject obj)
        {
            if (obj is null)
            {
                // Handle the case when obj is null for whatever reason.
                return false;
            }

            var isStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() is not null;
            var isParticleSystem = obj.GetComponent<ParticleSystem>() is not null;
            var isGoodThing = isStaticDeferredDecal || isParticleSystem;
            GameObject childGameMeshObject = null;
            GameObject childGameColliderObject = null;
            var childHasMesh = false;
            var sizeOnY = 3f;
            var childHasCollider = false;
            var foundClutterName = _clutterNameDictionary.Keys
                .Any(key => obj.name.ToLower().Contains(key.ToLower()));

            bool dontDisableName = _dontDisableDictionary.Keys.
                Any(key => obj.name.ToLower().Contains(key.ToLower()));

            if (foundClutterName && !dontDisableName)
            {
                //EFT.UI.ConsoleScreen.LogError("Found Clutter Name" + obj.name);
                foreach (Transform child in obj.transform)
                {
                    childGameMeshObject = child.gameObject;

                    if (IsBadThing(gameObject))
                    {
                        return false;
                    }
                }
                
                foreach (Transform child in obj.transform)
                {
                    childGameMeshObject = child.gameObject;
                    
                    if (child.GetComponent<MeshRenderer>() is not null && !childGameMeshObject.name.ToLower().Contains("shadow") && !childGameMeshObject.name.ToLower().StartsWith("col") && !childGameMeshObject.name.ToLower().EndsWith("der"))
                    {
                        childHasMesh = true;
                        
                        // Exit the loop since we've found what we need
                        break;
                    }
                }
                
                if (!childHasMesh && !isGoodThing)
                {
                    return false;
                }
                
                foreach (Transform child in obj.transform)
                {
                    if ((child.GetComponent<MeshCollider>() is not null || child.GetComponent<BoxCollider>() is not null) && child.GetComponent<BallisticCollider>() is null)
                    {
                        childGameColliderObject = child.gameObject;
                        if (childGameColliderObject is not null && childGameColliderObject.activeSelf)
                        {
                            childHasCollider = true;
                            // Exit the loop since we've found what we need
                            break;
                        }
                    }
                }
                
                if (isGoodThing)
                {
                    sizeOnY = 0.1f;
                }
                else if (childHasMesh)
                {
                    sizeOnY = GetMeshSizeOnY(childGameMeshObject);
                }
                else
                {
                    return false;
                }
                if ((childHasMesh || isGoodThing) && (!childHasCollider || isGoodThing) && sizeOnY <= 2f * Configuration.DeclutterScaleOffsetConfig.Value)
                {
                    _savedClutterObjects.Add(obj);
                    return true;
                }
            }
            return false;
        }

        private bool IsBadThing(GameObject childGameMeshObject)
        {
            var isBadThing = childGameMeshObject.GetComponent<LootableContainer>() != null;
            isBadThing = childGameMeshObject.GetComponent<LootableContainersGroup>() != null;
            isBadThing = childGameMeshObject.GetComponent<ObservedLootItem>() != null;
            isBadThing = childGameMeshObject.GetComponent<LootItem>() != null;
            isBadThing = childGameMeshObject.GetComponent<WeaponModPoolObject>() != null;
            isBadThing = childGameMeshObject.GetComponent<RainCondensator>() != null;
            isBadThing = childGameMeshObject.GetComponent<LocalPlayer>() != null;
            isBadThing = childGameMeshObject.GetComponent<Player>() != null;
            isBadThing = childGameMeshObject.GetComponent<BotOwner>() != null;
            isBadThing = childGameMeshObject.GetComponent<CullingObject>() != null;
            isBadThing = childGameMeshObject.GetComponent<CullingLightObject>() != null;
            isBadThing = childGameMeshObject.GetComponent<CullingGroup>() != null;
            isBadThing = childGameMeshObject.GetComponent<DisablerCullingObject>() != null;
            isBadThing = childGameMeshObject.GetComponent<ObservedCullingManager>() != null;
            isBadThing = childGameMeshObject.GetComponent<PerfectCullingCrossSceneGroup>() != null;
            isBadThing = childGameMeshObject.GetComponent<ScreenDistanceSwitcher>() != null;
            isBadThing = childGameMeshObject.GetComponent<BakedLodContent>() != null;
            isBadThing = childGameMeshObject.GetComponent<GuidComponent>() != null;
            isBadThing = childGameMeshObject.GetComponent<OcclusionPortal>() != null;
            isBadThing = childGameMeshObject.GetComponent<MultisceneSharedOccluder>() != null;
            isBadThing = childGameMeshObject.GetComponent<WindowBreaker>() != null;
            isBadThing = childGameMeshObject.GetComponent<BotSpawner>() != null;

            return isBadThing;
        }

        private float GetMeshSizeOnY(GameObject childGameObject)
        {
            var meshRenderer = childGameObject?.GetComponent<MeshRenderer>();
            
            if (meshRenderer is not null && meshRenderer.enabled)
            {
                var bounds = meshRenderer.bounds;
                return bounds.size.y;
            }
            
            return 0.0f;
        }
    }
}
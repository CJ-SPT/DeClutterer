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
using TYR_DeClutterer.Patches;
using TYR_DeClutterer.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TYR_DeClutterer
{
    [BepInPlugin("com.TYR.DeClutter", "TYR_DeClutter", "1.2.3")]
    public class DeClutter : BaseUnityPlugin
    {
        private static string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static bool DefaultsoftParticles = false;
        public static int DefaultparticleRaycastBudget = 0;
        public static bool DefaultsoftVegetation = false;
        public static bool DefaultrealtimeReflectionProbes = false;
        public static int DefaultpixelLightCount = 0;
        public static ShadowQuality DefaultShadows;
        public static int DefaultshadowCascades = 0;
        public static int DefaultmasterTextureLimit = 0;
        public static float DefaultlodBias = 0f;

        private static GameWorld _gameWorld;
        private List<GameObject> _allGameObjectsList = new List<GameObject>();
        private static List<GameObject> _savedClutterObjects = new List<GameObject>();
        private static ClutterNameStruct _cleanUpNames = new ClutterNameStruct();
        private static bool _deCluttered = false;
        private static bool _applyDeclutter = false;

        private static bool MapLoaded() => Singleton<GameWorld>.Instantiated;

        private Dictionary<string, bool> _dontDisableDictionary = new Dictionary<string, bool>
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

        private Dictionary<string, bool> _clutterNameDictionary = new Dictionary<string, bool>
        {
        };

        private void Awake()
        {
            new PhysicsUpdatePatch().Enable();
            new PhysicsFixedUpdatePatch().Enable();
            new RagdollPhysicsLateUpdatePatch().Enable();

            new AmbientLightOptimizeRenderingPatch().Enable();
            new AmbientLightDisableUpdatesPatch().Enable();
            new AmbientLightDisableLateUpdatesPatch().Enable();

            new DontSpawnShellsFiringPatch().Enable();
            new DontSpawnShellsJamPatch().Enable();
            new DontSpawnShellsAtAllReallyPatch().Enable();

            new SkyDelayUpdatesPatch().Enable();
            new WeatherLateUpdatePatch().Enable();
            new CloudsControllerDelayUpdatesPatch().Enable();
            new WeatherEventControllerDelayUpdatesPatch().Enable();
        }

        private void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Configuration.Bind(Config);
            SubscribeConfig();

            Initialize_clutterNameDictionary();

            _applyDeclutter = Configuration.declutterEnabledConfig.Value;

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
            if (!MapLoaded() || _deCluttered || !Configuration.declutterEnabledConfig.Value)
                return;

            _gameWorld = Singleton<GameWorld>.Instance;
            if (_gameWorld == null || _gameWorld.MainPlayer == null || IsInHideout())
                return;

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

            _clutterNameDictionary = Configuration.declutterGarbageEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Garbage)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.declutterHeapsEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Heaps)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.declutterSpentCartridgesEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.SpentCartridges)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.declutterFakeFoodEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.FoodDrink)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.declutterDecalsEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Decals)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.declutterPuddlesEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Puddles)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;

            _clutterNameDictionary = Configuration.declutterShardsEnabledConfig.Value
                ? _clutterNameDictionary.Concat(_cleanUpNames.Shards)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : _clutterNameDictionary;
        }

        private void SubscribeConfig()
        {
            Configuration.declutterEnabledConfig.SettingChanged += OnApplyDeclutterSettingChanged;
            Configuration.declutterGarbageEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.declutterHeapsEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.declutterSpentCartridgesEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.declutterFakeFoodEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.declutterDecalsEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.declutterPuddlesEnabledConfig.SettingChanged += BuildClutterNameDict;
            Configuration.declutterShardsEnabledConfig.SettingChanged += BuildClutterNameDict;

            Configuration.framesaverEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverPhysicsEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverShellChangesEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverParticlesEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverSoftVegetationEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverReflectionsEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverLightingShadowsEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverWeatherUpdatesEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverTexturesEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverLODEnabledConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverParticleBudgetDividerConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverPixelLightDividerConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverShadowDividerConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverTextureSizeConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverLODBiasConfig.SettingChanged += OnApplyVisualsChanged;
            Configuration.framesaverFireAndSmokeEnabledConfig.SettingChanged += OnApplyVisualsChanged;
        }

        private void OnApplyVisualsChanged(object sender, EventArgs e)
        {
            OnApplyVisualsChanged();
        }

        private void OnApplyVisualsChanged()
        {
            if (Configuration.framesaverEnabledConfig.Value)
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
            _applyDeclutter = Configuration.declutterEnabledConfig.Value;
            if (_deCluttered)
            {
                if (_applyDeclutter)
                {
                    DeClutterEnabled();
                }
                else
                {
                    ReClutterEnabled();
                }
            }
        }

        private void DeClutterEnabled()
        {
            foreach (GameObject obj in _savedClutterObjects)
            {
                if (obj.activeSelf == true)
                {
                    obj.SetActive(false);
                }
            }
        }

        private void ReClutterEnabled()
        {
            foreach (GameObject obj in _savedClutterObjects)
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
            // Check if "bunker_2" is one of the active scene names
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == "bunker_2")
                {
                    //EFT.UI.ConsoleScreen.LogError("bunker_2 loaded, not running de-cluttering.");
                    return true;
                }
            }
            //EFT.UI.ConsoleScreen.LogError("bunker_2 not loaded, de-cluttering.");
            return false;
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
                    GameObject[] allGameObjectsArray = _allGameObjectsList.ToArray();
                    foreach (GameObject obj in allGameObjectsArray)
                    {
                        if (obj != null && ShouldDisableObject(obj))
                        {
                            obj.SetActive(false);
                            //Logger.LogInfo("Clutter Removed " + obj.name);
                            //EFT.UI.ConsoleScreen.LogError("Clutter Removed " + obj.name);
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
                bool isLODGroup = obj.GetComponent<LODGroup>() != null;
                bool isStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() != null;
                bool isParticleSystem = obj.GetComponent<ParticleSystem>() != null;
                bool isGoodThing = isLODGroup || isStaticDeferredDecal || isParticleSystem;

                if (Configuration.framesaverFireAndSmokeEnabledConfig.Value)
                {
                    if (Configuration.declutterDecalsEnabledConfig.Value)
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
                    if (Configuration.declutterDecalsEnabledConfig.Value)
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
            if (obj == null)
            {
                // Handle the case when obj is null for whatever reason.
                return false;
            }

            bool isStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() != null;
            bool isParticleSystem = obj.GetComponent<ParticleSystem>() != null;
            bool isGoodThing = isStaticDeferredDecal || isParticleSystem;
            GameObject childGameMeshObject = null;
            GameObject childGameColliderObject = null;
            bool childHasMesh = false;
            float sizeOnY = 3f;
            bool childHasCollider = false;
            bool foundClutterName = foundClutterName = _clutterNameDictionary.Keys
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
                    if (child.GetComponent<MeshRenderer>() != null && !childGameMeshObject.name.ToLower().Contains("shadow") && !childGameMeshObject.name.ToLower().StartsWith("col") && !childGameMeshObject.name.ToLower().EndsWith("der"))
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
                    if ((child.GetComponent<MeshCollider>() != null || child.GetComponent<BoxCollider>() != null) && child.GetComponent<BallisticCollider>() == null)
                    {
                        childGameColliderObject = child.gameObject;
                        if (childGameColliderObject != null && childGameColliderObject.activeSelf)
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
                if ((childHasMesh || isGoodThing) && (!childHasCollider || isGoodThing) && sizeOnY <= 2f * Configuration.declutterScaleOffsetConfig.Value)
                {
                    _savedClutterObjects.Add(obj);
                    return true;
                }
            }
            return false;
        }

        private bool IsBadThing(GameObject childGameMeshObject)
        {
            bool isBadThing = childGameMeshObject.GetComponent<LootableContainer>() != null;
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
            MeshRenderer meshRenderer = childGameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.enabled)
            {
                Bounds bounds = meshRenderer.bounds;
                return bounds.size.y;
            }
            return 0.0f;
        }
    }
}
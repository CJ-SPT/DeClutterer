using Comfort.Common;
using UnityEngine;

namespace TYR_DeClutterer.Utils
{
    internal static class GraphicsUtils
    {
        public static void SetParticlesQuality()
        {
            if (Configuration.FramesaverParticlesEnabledConfig.Value)
            {
                QualitySettings.softParticles = false;
                if (Configuration.FramesaverParticleBudgetDividerConfig.Value > 1)
                {
                    QualitySettings.particleRaycastBudget = DeClutter.DefaultparticleRaycastBudget / Configuration.FramesaverParticleBudgetDividerConfig.Value;
                }
            }
            else
            {
                QualitySettings.softParticles = DeClutter.DefaultsoftParticles;
                QualitySettings.particleRaycastBudget = DeClutter.DefaultparticleRaycastBudget;
            }
        }

        public static void SetSoftVegetationQuality()
        {
            if (Configuration.FramesaverSoftVegetationEnabledConfig.Value)
            {
                QualitySettings.softVegetation = false;
            }
            else
            {
                QualitySettings.softVegetation = DeClutter.DefaultsoftVegetation;
            }
        }

        public static void SetReflectionQuality()
        {
            if (Configuration.FramesaverReflectionsEnabledConfig.Value)
            {
                QualitySettings.realtimeReflectionProbes = false;
            }
            else
            {
                QualitySettings.realtimeReflectionProbes = DeClutter.DefaultrealtimeReflectionProbes;
            }
        }

        public static void SetLightingShadowQuality()
        {
            if (Configuration.FramesaverLightingShadowCascadesEnabledConfig.Value)
            {
                QualitySettings.shadows = ShadowQuality.HardOnly;
                if (Configuration.FramesaverShadowDividerConfig.Value > 1)
                {
                    QualitySettings.pixelLightCount = 4 / Configuration.FramesaverPixelLightDividerConfig.Value;
                    QualitySettings.shadowCascades = 4 / Configuration.FramesaverShadowDividerConfig.Value;
                }
            }
            else
            {
                QualitySettings.pixelLightCount = DeClutter.DefaultpixelLightCount;
                QualitySettings.shadows = DeClutter.DefaultShadows;
                QualitySettings.shadowCascades = DeClutter.DefaultshadowCascades;
            }
        }

        public static void SetTextureQuality()
        {
            if (Configuration.FramesaverTexturesEnabledConfig.Value)
            {
                if (Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.TextureQuality.Value == 2)
                {
                    QualitySettings.masterTextureLimit = 0;
                }
                else
                {
                    QualitySettings.masterTextureLimit = Configuration.FramesaverTextureSizeConfig.Value;
                }
            }
            else
            {
                if (Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.TextureQuality.Value == 2)
                {
                    QualitySettings.masterTextureLimit = 0;
                }
                else
                {
                    QualitySettings.masterTextureLimit = DeClutter.DefaultmasterTextureLimit;
                }
            }
        }

        public static void SetLodBiasQuality()
        {
            if (Configuration.FramesaverLODEnabledConfig.Value)
            {
                if (Configuration.FramesaverLODBiasConfig.Value > 1.0f)
                {
                    QualitySettings.lodBias = 2.0f / Configuration.FramesaverLODBiasConfig.Value;
                }
            }
            else
            {
                QualitySettings.lodBias = DeClutter.DefaultlodBias;
            }
        }

        public static void SetDefaultQualityForAll()
        {
            QualitySettings.softParticles = DeClutter.DefaultsoftParticles;
            QualitySettings.particleRaycastBudget = DeClutter.DefaultparticleRaycastBudget;
            QualitySettings.softVegetation = DeClutter.DefaultsoftVegetation;
            QualitySettings.realtimeReflectionProbes = DeClutter.DefaultrealtimeReflectionProbes;
            QualitySettings.pixelLightCount = DeClutter.DefaultpixelLightCount;
            QualitySettings.shadows = DeClutter.DefaultShadows;
            QualitySettings.shadowCascades = DeClutter.DefaultshadowCascades;

            if (Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.TextureQuality.Value == 2)
            {
                QualitySettings.masterTextureLimit = 0;
            }
            else
            {
                QualitySettings.masterTextureLimit = DeClutter.DefaultmasterTextureLimit;
            }

            QualitySettings.lodBias = DeClutter.DefaultlodBias;
        }
    }
}
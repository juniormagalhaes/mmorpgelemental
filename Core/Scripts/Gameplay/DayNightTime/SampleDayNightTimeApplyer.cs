using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [ExecuteInEditMode]
    public class SampleDayNightTimeApplyer : MonoBehaviour
    {
        [System.Serializable]
        public class SkyboxSettings
        {
            public Material skyboxMaterial;
            [Range(0f, 1f)]
            public float startTime = 0.2f;
            [Range(0f, 1f)]
            public float endTime = 0.8f;
        }

        [System.Serializable]
        public class PrefabSettings
        {
            public GameObject prefab;
            [Range(0f, 1f)]
            public float startTime = 0.2f;
            [Range(0f, 1f)]
            public float endTime = 0.8f;
        }

        [System.Serializable]
        public class ShadowSettings
        {
            public bool enableShadows = true;
            [Range(0f, 1f)]
            public float startTime = 0.2f;
            [Range(0f, 1f)]
            public float endTime = 0.8f;
        }

        [System.Serializable]
        public class IntensityInterval
        {
            public float startTime;
            public float endTime;
            public float minIntensity;
            public float maxIntensity;
        }

        [Header("Color Settings")]
        public Gradient ambientColor;
        public Gradient directionalColor;
        public Gradient fogColorGradient; // Gradient específica para a cor do fog

        [Header("Required Components")]
        public Light directionalLight;

        [Header("Intensity Settings")]
        public List<IntensityInterval> intensityIntervals = new List<IntensityInterval>();

        [Header("Skybox Settings")]
        public SkyboxSettings[] skyboxSettings; // Configurações de cada Skybox

        [Header("Prefab Settings")]
        public PrefabSettings[] prefabSettings; // Configurações de cada Prefab

        [Header("Shadow Settings")]
        public ShadowSettings shadowSettings; // Configurações para ativar/desativar sombras

        [Header("Fog Settings")]
        public float fogStartDistance = 0.0f;
        public float fogEndDistance = 100.0f;

        [Header("Debugging")]
        [Range(0f, 1f)]
        public float timeOfDayPercent = 0.5f;
        private float dayDuration = 24f; // Adicione essa linha

        private void Update()
        {
            // Update time of day percent while network active only
            if (Application.isPlaying && BaseGameNetworkManager.Singleton.IsNetworkActive)
                timeOfDayPercent = GameInstance.Singleton.DayNightTimeUpdater.TimeOfDay / dayDuration;

            // Set ambient light
            RenderSettings.ambientLight = ambientColor.Evaluate(timeOfDayPercent);

            // Set directional light and rotate it to changes shadow direction
            if (directionalLight != null)
            {
                directionalLight.color = directionalColor.Evaluate(timeOfDayPercent);

                // Ensure that the rotation is valid
                if (!float.IsNaN(timeOfDayPercent))
                {
                    directionalLight.transform.localRotation = Quaternion.Euler(new Vector3(45f, (timeOfDayPercent * 360f) - 360f, 0));
                }

                // Set intensity based on time of day
                directionalLight.intensity = GetIntensityForTime(timeOfDayPercent);

                // Set shadows based on time of day
                if (timeOfDayPercent >= shadowSettings.startTime && timeOfDayPercent <= shadowSettings.endTime)
                {
                    directionalLight.shadows = shadowSettings.enableShadows ? LightShadows.Soft : LightShadows.None;
                }
                else
                {
                    directionalLight.shadows = LightShadows.None;
                }
            }

            // Change Skybox material based on time of day
            if (skyboxSettings.Length > 0)
            {
                foreach (var setting in skyboxSettings)
                {
                    if (timeOfDayPercent >= setting.startTime && timeOfDayPercent <= setting.endTime)
                    {
                        RenderSettings.skybox = setting.skyboxMaterial;
                        break;
                    }
                }
            }

            // Change Prefabs based on time of day
            if (prefabSettings.Length > 0)
            {
                foreach (var setting in prefabSettings)
                {
                    if (timeOfDayPercent >= setting.startTime && timeOfDayPercent <= setting.endTime)
                    {
                        setting.prefab.SetActive(true);
                    }
                    else
                    {
                        setting.prefab.SetActive(false);
                    }
                }
            }

            // Change Fog color based on time of day
            Color fogColor = fogColorGradient.Evaluate(timeOfDayPercent);
            RenderSettings.fogColor = fogColor;

            // Adjust other fog properties
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }

        private float GetIntensityForTime(float timePercent)
        {
            foreach (var interval in intensityIntervals)
            {
                if (timePercent >= interval.startTime && timePercent <= interval.endTime)
                {
                    return Mathf.Lerp(interval.minIntensity, interval.maxIntensity, (timePercent - interval.startTime) / (interval.endTime - interval.startTime));
                }
            }

            return 0f;
        }
    }
}

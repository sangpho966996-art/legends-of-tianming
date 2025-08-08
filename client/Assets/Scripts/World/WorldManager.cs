using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class WorldManager : MonoBehaviour
    {
        [Header("World Settings")]
        public string currentRegion = "Qingze Plains";
        public Transform playerSpawnPoint;
        public float worldBounds = 1000f;
        
        [Header("Environment")]
        public Light sunLight;
        public Gradient dayNightColors;
        public AnimationCurve dayNightIntensity;
        public float dayDuration = 1200f; // 20 minutes real time = 24 hours game time
        
        [Header("Weather")]
        public ParticleSystem rainEffect;
        public ParticleSystem snowEffect;
        public AudioSource weatherAudioSource;
        
        private float currentTimeOfDay = 0.5f; // Start at noon
        private WeatherType currentWeather = WeatherType.Clear;
        private Dictionary<string, RegionData> regions;
        private List<GameObject> spawnedMonsters = new List<GameObject>();

        public float TimeOfDay => currentTimeOfDay;
        public WeatherType Weather => currentWeather;
        public string CurrentRegion => currentRegion;

        private void Start()
        {
            InitializeRegions();
            InitializeEnvironment();
            
            if (playerSpawnPoint == null)
            {
                GameObject spawnObj = new GameObject("PlayerSpawnPoint");
                playerSpawnPoint = spawnObj.transform;
                playerSpawnPoint.position = Vector3.zero;
            }
        }

        private void Update()
        {
            UpdateDayNightCycle();
            UpdateWeather();
        }

        private void InitializeRegions()
        {
            regions = new Dictionary<string, RegionData>
            {
                ["Qingze Plains"] = new RegionData
                {
                    name = "Qingze Plains",
                    levelRange = new Vector2Int(30, 50),
                    description = "Vast grasslands with rolling hills and ancient ruins. Home to wild beasts and wandering spirits.",
                    spawnPoint = Vector3.zero,
                    bounds = new Bounds(Vector3.zero, Vector3.one * 500f),
                    ambientMusic = "qingze_ambient",
                    weatherTypes = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Fog }
                },
                ["Yun City"] = new RegionData
                {
                    name = "Yun City",
                    levelRange = new Vector2Int(50, 70),
                    description = "A bustling martial arts city built on floating islands connected by bridges.",
                    spawnPoint = new Vector3(1000, 100, 0),
                    bounds = new Bounds(new Vector3(1000, 100, 0), Vector3.one * 300f),
                    ambientMusic = "yun_city_ambient",
                    weatherTypes = new WeatherType[] { WeatherType.Clear, WeatherType.Fog, WeatherType.Wind }
                },
                ["Hanlin Peaks"] = new RegionData
                {
                    name = "Hanlin Peaks",
                    levelRange = new Vector2Int(70, 90),
                    description = "Treacherous mountain peaks shrouded in mist, home to powerful cultivators and ancient secrets.",
                    spawnPoint = new Vector3(0, 500, 1000),
                    bounds = new Bounds(new Vector3(0, 500, 1000), Vector3.one * 400f),
                    ambientMusic = "hanlin_ambient",
                    weatherTypes = new WeatherType[] { WeatherType.Snow, WeatherType.Fog, WeatherType.Clear }
                }
            };
        }

        private void InitializeEnvironment()
        {
            if (sunLight == null)
            {
                GameObject lightObj = new GameObject("Sun Light");
                sunLight = lightObj.AddComponent<Light>();
                sunLight.type = LightType.Directional;
                sunLight.shadows = LightShadows.Soft;
            }
            
            UpdateLighting();
        }

        private void UpdateDayNightCycle()
        {
            currentTimeOfDay += Time.deltaTime / dayDuration;
            if (currentTimeOfDay >= 1f)
                currentTimeOfDay -= 1f;
            
            UpdateLighting();
        }

        private void UpdateLighting()
        {
            if (sunLight != null)
            {
                float sunAngle = currentTimeOfDay * 360f - 90f;
                sunLight.transform.rotation = Quaternion.Euler(sunAngle, 30f, 0f);
                
                if (dayNightColors.colorKeys.Length > 0)
                    sunLight.color = dayNightColors.Evaluate(currentTimeOfDay);
                
                sunLight.intensity = dayNightIntensity.Evaluate(currentTimeOfDay);
            }
            
            RenderSettings.ambientLight = Color.Lerp(Color.black, Color.white, dayNightIntensity.Evaluate(currentTimeOfDay) * 0.3f);
        }

        private void UpdateWeather()
        {
            if (Random.Range(0f, 1f) < 0.001f) // 0.1% chance per frame to change weather
            {
                ChangeWeather();
            }
        }

        private void ChangeWeather()
        {
            if (regions.ContainsKey(currentRegion))
            {
                var regionData = regions[currentRegion];
                WeatherType newWeather = regionData.weatherTypes[Random.Range(0, regionData.weatherTypes.Length)];
                
                if (newWeather != currentWeather)
                {
                    SetWeather(newWeather);
                }
            }
        }

        public void SetWeather(WeatherType weather)
        {
            currentWeather = weather;
            
            if (rainEffect != null) rainEffect.Stop();
            if (snowEffect != null) snowEffect.Stop();
            
            switch (weather)
            {
                case WeatherType.Rain:
                    if (rainEffect != null) rainEffect.Play();
                    break;
                case WeatherType.Snow:
                    if (snowEffect != null) snowEffect.Play();
                    break;
            }
            
            Debug.Log($"🌤️ Weather changed to: {weather}");
        }

        public void ChangeRegion(string regionName)
        {
            if (regions.ContainsKey(regionName))
            {
                currentRegion = regionName;
                var regionData = regions[regionName];
                
                playerSpawnPoint.position = regionData.spawnPoint;
                
                var audioManager = GameManager.Instance.GetAudioManager();
                if (audioManager != null)
                {
                    audioManager.PlayAmbient(regionData.ambientMusic);
                }
                
                Debug.Log($"🗺️ Changed to region: {regionName}");
            }
        }

        public RegionData GetCurrentRegionData()
        {
            return regions.ContainsKey(currentRegion) ? regions[currentRegion] : null;
        }

        public Vector3 GetSpawnPosition()
        {
            return playerSpawnPoint.position;
        }

        public bool IsPositionInBounds(Vector3 position)
        {
            var regionData = GetCurrentRegionData();
            return regionData != null && regionData.bounds.Contains(position);
        }

        public Vector3 ClampPositionToBounds(Vector3 position)
        {
            var regionData = GetCurrentRegionData();
            if (regionData != null)
            {
                return regionData.bounds.ClosestPoint(position);
            }
            return position;
        }

        public string GetTimeOfDayString()
        {
            if (currentTimeOfDay < 0.25f) return "Night";
            if (currentTimeOfDay < 0.5f) return "Morning";
            if (currentTimeOfDay < 0.75f) return "Day";
            return "Evening";
        }

        public void SpawnMonster(GameObject monsterPrefab, Vector3 position)
        {
            if (monsterPrefab != null)
            {
                GameObject monster = Instantiate(monsterPrefab, position, Quaternion.identity);
                spawnedMonsters.Add(monster);
            }
        }

        public void DespawnAllMonsters()
        {
            foreach (var monster in spawnedMonsters)
            {
                if (monster != null)
                    Destroy(monster);
            }
            spawnedMonsters.Clear();
        }
    }

    [System.Serializable]
    public class RegionData
    {
        public string name;
        public Vector2Int levelRange;
        public string description;
        public Vector3 spawnPoint;
        public Bounds bounds;
        public string ambientMusic;
        public WeatherType[] weatherTypes;
    }

    public enum WeatherType
    {
        Clear,
        Rain,
        Snow,
        Fog,
        Wind
    }
}

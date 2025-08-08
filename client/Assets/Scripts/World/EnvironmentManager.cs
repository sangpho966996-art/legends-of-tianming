using UnityEngine;
using System.Collections;

namespace LegendsOfTianming.Core
{
    public class EnvironmentManager : MonoBehaviour
    {
        [Header("Lighting")]
        public Light sunLight;
        public Gradient sunColorGradient;
        public AnimationCurve sunIntensityCurve;
        
        [Header("Day/Night Cycle")]
        public float dayDuration = 600f;
        public bool enableDayNightCycle = true;
        
        [Header("Weather")]
        public ParticleSystem rainEffect;
        public ParticleSystem snowEffect;
        public ParticleSystem fogEffect;
        
        [Header("Ambient Audio")]
        public AudioSource ambientAudioSource;
        public AudioClip[] dayAmbientSounds;
        public AudioClip[] nightAmbientSounds;
        public AudioClip[] rainSounds;
        
        private float currentTimeOfDay = 0.5f;
        private WeatherType currentWeather = WeatherType.Clear;
        private Coroutine weatherTransitionCoroutine;

        private void Start()
        {
            InitializeEnvironment();
            if (enableDayNightCycle)
            {
                StartCoroutine(DayNightCycle());
            }
        }

        private void InitializeEnvironment()
        {
            if (sunLight == null)
            {
                sunLight = FindObjectOfType<Light>();
                if (sunLight == null)
                {
                    GameObject lightObj = new GameObject("Sun Light");
                    sunLight = lightObj.AddComponent<Light>();
                    sunLight.type = LightType.Directional;
                    sunLight.shadows = LightShadows.Soft;
                }
            }
            
            SetupWeatherEffects();
            UpdateLighting();
            SetWeather(WeatherType.Clear);
        }

        private void SetupWeatherEffects()
        {
            if (rainEffect == null)
                rainEffect = CreateRainEffect();
                
            if (snowEffect == null)
                snowEffect = CreateSnowEffect();
                
            if (fogEffect == null)
                fogEffect = CreateFogEffect();
        }

        private ParticleSystem CreateRainEffect()
        {
            GameObject rainObj = new GameObject("Rain Effect");
            rainObj.transform.SetParent(transform);
            
            ParticleSystem rain = rainObj.AddComponent<ParticleSystem>();
            var main = rain.main;
            main.startLifetime = 2f;
            main.startSpeed = 10f;
            main.startSize = 0.1f;
            main.startColor = Color.blue;
            main.maxParticles = 1000;
            
            var emission = rain.emission;
            emission.rateOverTime = 500f;
            
            var shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(50f, 1f, 50f);
            
            var velocityOverLifetime = rain.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.y = -10f;
            
            rainObj.transform.position = new Vector3(0, 20f, 0);
            rain.Stop();
            
            return rain;
        }

        private ParticleSystem CreateSnowEffect()
        {
            GameObject snowObj = new GameObject("Snow Effect");
            snowObj.transform.SetParent(transform);
            
            ParticleSystem snow = snowObj.AddComponent<ParticleSystem>();
            var main = snow.main;
            main.startLifetime = 5f;
            main.startSpeed = 2f;
            main.startSize = 0.2f;
            main.startColor = Color.white;
            main.maxParticles = 500;
            
            var emission = snow.emission;
            emission.rateOverTime = 100f;
            
            var shape = snow.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(50f, 1f, 50f);
            
            var velocityOverLifetime = snow.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.y = -2f;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
            
            snowObj.transform.position = new Vector3(0, 20f, 0);
            snow.Stop();
            
            return snow;
        }

        private ParticleSystem CreateFogEffect()
        {
            GameObject fogObj = new GameObject("Fog Effect");
            fogObj.transform.SetParent(transform);
            
            ParticleSystem fog = fogObj.AddComponent<ParticleSystem>();
            var main = fog.main;
            main.startLifetime = 10f;
            main.startSpeed = 0.5f;
            main.startSize = 5f;
            main.startColor = new Color(1f, 1f, 1f, 0.3f);
            main.maxParticles = 100;
            
            var emission = fog.emission;
            emission.rateOverTime = 10f;
            
            var shape = fog.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(100f, 5f, 100f);
            
            fogObj.transform.position = new Vector3(0, 2f, 0);
            fog.Stop();
            
            return fog;
        }

        private IEnumerator DayNightCycle()
        {
            while (enableDayNightCycle)
            {
                currentTimeOfDay += Time.deltaTime / dayDuration;
                if (currentTimeOfDay >= 1f)
                    currentTimeOfDay = 0f;
                    
                UpdateLighting();
                UpdateAmbientAudio();
                
                yield return null;
            }
        }

        private void UpdateLighting()
        {
            if (sunLight == null) return;
            
            float sunAngle = currentTimeOfDay * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 30f, 0f);
            
            if (sunColorGradient != null && sunColorGradient.colorKeys.Length > 0)
            {
                sunLight.color = sunColorGradient.Evaluate(currentTimeOfDay);
            }
            
            if (sunIntensityCurve != null && sunIntensityCurve.keys.Length > 0)
            {
                sunLight.intensity = sunIntensityCurve.Evaluate(currentTimeOfDay);
            }
            else
            {
                sunLight.intensity = Mathf.Clamp01(Mathf.Cos(currentTimeOfDay * 2f * Mathf.PI));
            }
            
            RenderSettings.ambientIntensity = Mathf.Clamp01(sunLight.intensity + 0.2f);
        }

        private void UpdateAmbientAudio()
        {
            if (ambientAudioSource == null) return;
            
            bool isNight = currentTimeOfDay < 0.25f || currentTimeOfDay > 0.75f;
            AudioClip[] soundArray = isNight ? nightAmbientSounds : dayAmbientSounds;
            
            if (soundArray != null && soundArray.Length > 0 && !ambientAudioSource.isPlaying)
            {
                AudioClip randomClip = soundArray[Random.Range(0, soundArray.Length)];
                if (randomClip != null)
                {
                    ambientAudioSource.clip = randomClip;
                    ambientAudioSource.Play();
                }
            }
        }

        public void SetWeather(WeatherType weather)
        {
            if (weatherTransitionCoroutine != null)
            {
                StopCoroutine(weatherTransitionCoroutine);
            }
            
            weatherTransitionCoroutine = StartCoroutine(TransitionWeather(weather));
        }

        private IEnumerator TransitionWeather(WeatherType newWeather)
        {
            StopAllWeatherEffects();
            
            yield return new WaitForSeconds(1f);
            
            currentWeather = newWeather;
            
            switch (newWeather)
            {
                case WeatherType.Rain:
                    if (rainEffect != null)
                        rainEffect.Play();
                    PlayWeatherAudio(rainSounds);
                    break;
                    
                case WeatherType.Snow:
                    if (snowEffect != null)
                        snowEffect.Play();
                    break;
                    
                case WeatherType.Fog:
                    if (fogEffect != null)
                        fogEffect.Play();
                    break;
                    
                case WeatherType.Clear:
                default:
                    break;
            }
            
            Debug.Log($"🌤️ Weather changed to: {newWeather}");
        }

        private void StopAllWeatherEffects()
        {
            if (rainEffect != null && rainEffect.isPlaying)
                rainEffect.Stop();
                
            if (snowEffect != null && snowEffect.isPlaying)
                snowEffect.Stop();
                
            if (fogEffect != null && fogEffect.isPlaying)
                fogEffect.Stop();
        }

        private void PlayWeatherAudio(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0 || ambientAudioSource == null) return;
            
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            if (randomClip != null)
            {
                ambientAudioSource.clip = randomClip;
                ambientAudioSource.loop = true;
                ambientAudioSource.Play();
            }
        }

        public void SetTimeOfDay(float time)
        {
            currentTimeOfDay = Mathf.Clamp01(time);
            UpdateLighting();
        }

        public float GetTimeOfDay()
        {
            return currentTimeOfDay;
        }

        public WeatherType GetCurrentWeather()
        {
            return currentWeather;
        }

        public bool IsNight()
        {
            return currentTimeOfDay < 0.25f || currentTimeOfDay > 0.75f;
        }

        public bool IsDay()
        {
            return !IsNight();
        }
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

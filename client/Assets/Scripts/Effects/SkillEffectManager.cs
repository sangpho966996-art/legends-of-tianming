using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class SkillEffectManager : MonoBehaviour
    {
        [Header("Effect Prefabs")]
        public GameObject[] azureCloudEffects;
        public GameObject[] ironBellEffects;
        public GameObject[] shadowVeilEffects;
        public GameObject[] wanderingSpearEffects;
        public GameObject[] spiritLuteEffects;
        public GameObject[] stoneheartEffects;
        
        [Header("Common Effects")]
        public GameObject damageNumberPrefab;
        public GameObject healNumberPrefab;
        public GameObject criticalHitEffect;
        public GameObject blockEffect;
        
        private Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>();
        private Queue<GameObject> effectPool = new Queue<GameObject>();

        private void Start()
        {
            InitializeEffectPrefabs();
        }

        private void InitializeEffectPrefabs()
        {
            effectPrefabs["swift_strike"] = CreatePlaceholderEffect("SwiftStrike", Color.cyan);
            effectPrefabs["cloud_step"] = CreatePlaceholderEffect("CloudStep", Color.white);
            effectPrefabs["flowing_counter"] = CreatePlaceholderEffect("FlowingCounter", Color.blue);
            effectPrefabs["wind_blade"] = CreatePlaceholderEffect("WindBlade", Color.green);
            effectPrefabs["azure_tempest"] = CreatePlaceholderEffect("AzureTempest", Color.cyan);
            
            effectPrefabs["iron_sweep"] = CreatePlaceholderEffect("IronSweep", Color.gray);
            effectPrefabs["bells_resonance"] = CreatePlaceholderEffect("BellsResonance", Color.yellow);
            effectPrefabs["staff_vault"] = CreatePlaceholderEffect("StaffVault", Color.brown);
            effectPrefabs["earth_shaker"] = CreatePlaceholderEffect("EarthShaker", Color.red);
            effectPrefabs["iron_fortress"] = CreatePlaceholderEffect("IronFortress", Color.gray);
        }

        private GameObject CreatePlaceholderEffect(string effectName, Color color)
        {
            GameObject effect = new GameObject($"Effect_{effectName}");
            
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = color;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.maxParticles = 50;
            
            var emission = particles.emission;
            emission.rateOverTime = 50f;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
            
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            
            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            effect.AddComponent<AutoDestroyEffect>();
            
            return effect;
        }

        public void PlaySkillEffect(string skillId, Vector3 position, Vector3 direction, Transform caster = null)
        {
            if (!effectPrefabs.ContainsKey(skillId))
            {
                Debug.LogWarning($"No effect found for skill: {skillId}");
                return;
            }
            
            GameObject effectPrefab = effectPrefabs[skillId];
            GameObject effect = GetPooledEffect(effectPrefab);
            
            if (effect == null)
            {
                effect = Instantiate(effectPrefab);
            }
            
            effect.transform.position = position;
            effect.transform.LookAt(position + direction);
            effect.SetActive(true);
            
            var particleSystem = effect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
            
            ApplySkillSpecificEffects(skillId, effect, position, direction, caster);
            
            Debug.Log($"✨ Playing effect for skill: {skillId} at {position}");
        }

        private void ApplySkillSpecificEffects(string skillId, GameObject effect, Vector3 position, Vector3 direction, Transform caster)
        {
            switch (skillId)
            {
                case "swift_strike":
                    CreateSwordTrail(effect, caster);
                    break;
                    
                case "cloud_step":
                    CreateDashEffect(effect, position, direction);
                    break;
                    
                case "flowing_counter":
                    CreateParryEffect(effect, position);
                    break;
                    
                case "wind_blade":
                    CreateProjectileEffect(effect, position, direction);
                    break;
                    
                case "azure_tempest":
                    CreateWhirlwindEffect(effect, position);
                    break;
                    
                case "iron_sweep":
                    CreateSweepEffect(effect, position, direction);
                    break;
                    
                case "bells_resonance":
                    CreateSoundWaveEffect(effect, position);
                    break;
                    
                case "staff_vault":
                    CreateVaultEffect(effect, position, direction);
                    break;
                    
                case "earth_shaker":
                    CreateGroundImpactEffect(effect, position);
                    break;
                    
                case "iron_fortress":
                    CreateShieldEffect(effect, caster);
                    break;
            }
        }

        private void CreateSwordTrail(GameObject effect, Transform caster)
        {
            if (caster != null)
            {
                effect.transform.SetParent(caster);
                effect.transform.localPosition = Vector3.forward * 1.5f;
            }
        }

        private void CreateDashEffect(GameObject effect, Vector3 position, Vector3 direction)
        {
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var velocityOverLifetime = particles.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
                velocityOverLifetime.x = direction.x * 10f;
                velocityOverLifetime.z = direction.z * 10f;
            }
        }

        private void CreateParryEffect(GameObject effect, Vector3 position)
        {
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = Color.blue;
                main.startLifetime = 0.5f;
                
                var emission = particles.emission;
                emission.SetBursts(new ParticleSystem.Burst[]
                {
                    new ParticleSystem.Burst(0f, 30)
                });
            }
        }

        private void CreateProjectileEffect(GameObject effect, Vector3 position, Vector3 direction)
        {
            var projectile = effect.AddComponent<ProjectileMovement>();
            projectile.Initialize(direction * 15f, 3f);
        }

        private void CreateWhirlwindEffect(GameObject effect, Vector3 position)
        {
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 3f;
                
                var velocityOverLifetime = particles.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.orbitalX = 2f;
                velocityOverLifetime.orbitalY = 2f;
            }
        }

        private void CreateSweepEffect(GameObject effect, Vector3 position, Vector3 direction)
        {
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 60f;
                shape.radius = 0.1f;
            }
        }

        private void CreateSoundWaveEffect(GameObject effect, Vector3 position)
        {
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = Color.yellow;
                
                var shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 2f;
                
                var sizeOverLifetime = particles.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                AnimationCurve sizeCurve = new AnimationCurve();
                sizeCurve.AddKey(0f, 0.1f);
                sizeCurve.AddKey(1f, 2f);
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            }
        }

        private void CreateVaultEffect(GameObject effect, Vector3 position, Vector3 direction)
        {
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var velocityOverLifetime = particles.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.y = 5f;
                velocityOverLifetime.x = direction.x * 8f;
                velocityOverLifetime.z = direction.z * 8f;
            }
        }

        private void CreateGroundImpactEffect(GameObject effect, Vector3 position)
        {
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = Color.red;
                
                var shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 4f;
                
                var emission = particles.emission;
                emission.SetBursts(new ParticleSystem.Burst[]
                {
                    new ParticleSystem.Burst(0f, 100)
                });
            }
        }

        private void CreateShieldEffect(GameObject effect, Transform caster)
        {
            if (caster != null)
            {
                effect.transform.SetParent(caster);
                effect.transform.localPosition = Vector3.zero;
                
                var particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var main = particles.main;
                    main.startColor = Color.gray;
                    main.startLifetime = 10f;
                    
                    var shape = particles.shape;
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 2f;
                }
            }
        }

        public void ShowDamageNumber(Vector3 position, int damage, bool isCritical = false)
        {
            if (damageNumberPrefab == null) return;
            
            GameObject damageNumber = Instantiate(damageNumberPrefab, position + Vector3.up * 2f, Quaternion.identity);
            
            Text damageText = damageNumber.GetComponent<Text>();
            if (damageText != null)
            {
                damageText.text = damage.ToString();
                damageText.color = isCritical ? Color.yellow : Color.red;
                damageText.fontSize = isCritical ? 24 : 18;
            }
            
            var floatingText = damageNumber.AddComponent<FloatingText>();
            floatingText.Initialize(Vector3.up * 2f, 1f);
        }

        public void ShowHealNumber(Vector3 position, int healAmount)
        {
            if (healNumberPrefab == null) return;
            
            GameObject healNumber = Instantiate(healNumberPrefab, position + Vector3.up * 2f, Quaternion.identity);
            
            Text healText = healNumber.GetComponent<Text>();
            if (healText != null)
            {
                healText.text = $"+{healAmount}";
                healText.color = Color.green;
            }
            
            var floatingText = healNumber.AddComponent<FloatingText>();
            floatingText.Initialize(Vector3.up * 2f, 1f);
        }

        private GameObject GetPooledEffect(GameObject prefab)
        {
            if (effectPool.Count > 0)
            {
                return effectPool.Dequeue();
            }
            return null;
        }

        public void ReturnEffectToPool(GameObject effect)
        {
            effect.SetActive(false);
            effectPool.Enqueue(effect);
        }
    }

    public class AutoDestroyEffect : MonoBehaviour
    {
        public float lifetime = 3f;
        
        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }

    public class ProjectileMovement : MonoBehaviour
    {
        private Vector3 velocity;
        private float lifetime;
        
        public void Initialize(Vector3 vel, float life)
        {
            velocity = vel;
            lifetime = life;
            Destroy(gameObject, lifetime);
        }
        
        private void Update()
        {
            transform.position += velocity * Time.deltaTime;
        }
    }

    public class FloatingText : MonoBehaviour
    {
        private Vector3 velocity;
        private float lifetime;
        
        public void Initialize(Vector3 vel, float life)
        {
            velocity = vel;
            lifetime = life;
            Destroy(gameObject, lifetime);
        }
        
        private void Update()
        {
            transform.position += velocity * Time.deltaTime;
            velocity *= 0.95f;
        }
    }
}

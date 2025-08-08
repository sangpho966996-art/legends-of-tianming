using UnityEngine;
using UnityEngine.AI;

namespace LegendsOfTianming.Core
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterAI : MonoBehaviour
    {
        [Header("AI Settings")]
        public float detectionRange = 10f;
        public float attackRange = 2f;
        public float patrolRadius = 15f;
        public float attackCooldown = 2f;
        public LayerMask playerLayerMask = -1;
        
        [Header("Monster Stats")]
        public int level = 1;
        public int health = 100;
        public int maxHealth = 100;
        public int attackDamage = 20;
        public float moveSpeed = 3.5f;
        
        private NavMeshAgent agent;
        private GameObject currentTarget;
        private Vector3 spawnPosition;
        private float lastAttackTime = 0f;
        private MonsterState currentState = MonsterState.Idle;
        private float stateTimer = 0f;
        private Vector3 patrolTarget;

        public MonsterState State => currentState;
        public bool IsAlive => health > 0;
        public GameObject CurrentTarget => currentTarget;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            spawnPosition = transform.position;
            
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
            
            SetState(MonsterState.Idle);
            
            SetRandomPatrolTarget();
        }

        private void Update()
        {
            if (!IsAlive) return;
            
            stateTimer += Time.deltaTime;
            
            switch (currentState)
            {
                case MonsterState.Idle:
                    HandleIdleState();
                    break;
                case MonsterState.Patrol:
                    HandlePatrolState();
                    break;
                case MonsterState.Chase:
                    HandleChaseState();
                    break;
                case MonsterState.Attack:
                    HandleAttackState();
                    break;
                case MonsterState.Return:
                    HandleReturnState();
                    break;
                case MonsterState.Dead:
                    HandleDeadState();
                    break;
            }
            
            CheckForPlayers();
        }

        private void HandleIdleState()
        {
            if (stateTimer > Random.Range(2f, 5f))
            {
                SetState(MonsterState.Patrol);
            }
        }

        private void HandlePatrolState()
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                SetRandomPatrolTarget();
                agent.SetDestination(patrolTarget);
            }
            
            if (stateTimer > Random.Range(5f, 10f))
            {
                SetState(MonsterState.Idle);
            }
        }

        private void HandleChaseState()
        {
            if (currentTarget == null || !IsTargetValid())
            {
                SetState(MonsterState.Return);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            if (distanceToTarget <= attackRange)
            {
                SetState(MonsterState.Attack);
            }
            else if (distanceToTarget > detectionRange * 2f)
            {
                SetState(MonsterState.Return);
            }
            else
            {
                agent.SetDestination(currentTarget.transform.position);
            }
        }

        private void HandleAttackState()
        {
            if (currentTarget == null || !IsTargetValid())
            {
                SetState(MonsterState.Return);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            
            if (distanceToTarget > attackRange)
            {
                SetState(MonsterState.Chase);
                return;
            }
            
            Vector3 lookDirection = (currentTarget.transform.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }

        private void HandleReturnState()
        {
            agent.SetDestination(spawnPosition);
            
            if (!agent.pathPending && agent.remainingDistance < 1f)
            {
                currentTarget = null;
                health = maxHealth; // Heal when returning to spawn
                SetState(MonsterState.Idle);
            }
        }

        private void HandleDeadState()
        {
            if (stateTimer > 5f) // Respawn after 5 seconds
            {
                Respawn();
            }
        }

        private void CheckForPlayers()
        {
            if (currentState == MonsterState.Dead) return;
            
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, detectionRange, playerLayerMask);
            
            GameObject nearestPlayer = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var collider in playersInRange)
            {
                var character = collider.GetComponent<Character>();
                if (character != null && character.IsAlive())
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPlayer = collider.gameObject;
                    }
                }
            }
            
            if (nearestPlayer != null && currentTarget == null)
            {
                currentTarget = nearestPlayer;
                SetState(MonsterState.Chase);
            }
        }

        private void PerformAttack()
        {
            if (currentTarget == null) return;
            
            var targetCharacter = currentTarget.GetComponent<Character>();
            if (targetCharacter != null)
            {
                targetCharacter.Stats.TakeDamage(attackDamage);
                
                var networkManager = GameManager.Instance.GetNetworkManager();
                if (networkManager.IsConnected)
                {
                    networkManager.SendCombatAction(currentTarget.name, attackDamage, "monster_attack");
                }
                
                Debug.Log($"👹 {gameObject.name} attacked {currentTarget.name} for {attackDamage} damage");
            }
        }

        private bool IsTargetValid()
        {
            if (currentTarget == null) return false;
            
            var character = currentTarget.GetComponent<Character>();
            if (character == null || !character.IsAlive()) return false;
            
            float distanceToSpawn = Vector3.Distance(transform.position, spawnPosition);
            return distanceToSpawn <= patrolRadius * 2f;
        }

        private void SetRandomPatrolTarget()
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += spawnPosition;
            randomDirection.y = spawnPosition.y;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
            {
                patrolTarget = hit.position;
            }
            else
            {
                patrolTarget = spawnPosition;
            }
        }

        private void SetState(MonsterState newState)
        {
            currentState = newState;
            stateTimer = 0f;
            
            switch (newState)
            {
                case MonsterState.Idle:
                    agent.ResetPath();
                    break;
                case MonsterState.Patrol:
                    agent.SetDestination(patrolTarget);
                    break;
                case MonsterState.Chase:
                    if (currentTarget != null)
                        agent.SetDestination(currentTarget.transform.position);
                    break;
                case MonsterState.Attack:
                    agent.ResetPath();
                    break;
                case MonsterState.Return:
                    agent.SetDestination(spawnPosition);
                    break;
                case MonsterState.Dead:
                    agent.ResetPath();
                    break;
            }
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            
            health = Mathf.Max(0, health - damage);
            
            if (health <= 0)
            {
                Die();
            }
            else if (currentState == MonsterState.Idle || currentState == MonsterState.Patrol)
            {
                CheckForPlayers();
            }
            
            Debug.Log($"👹 {gameObject.name} took {damage} damage ({health}/{maxHealth})");
        }

        private void Die()
        {
            SetState(MonsterState.Dead);
            
            DropLoot();
            
            AwardExperience();
            
            Debug.Log($"💀 {gameObject.name} has been defeated!");
        }

        private void DropLoot()
        {
            Debug.Log($"💰 {gameObject.name} dropped loot");
        }

        private void AwardExperience()
        {
            int expReward = level * 10;
            
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, detectionRange, playerLayerMask);
            foreach (var collider in playersInRange)
            {
                var character = collider.GetComponent<Character>();
                if (character != null)
                {
                    Debug.Log($"🎉 {character.characterName} gained {expReward} experience");
                }
            }
        }

        private void Respawn()
        {
            health = maxHealth;
            transform.position = spawnPosition;
            currentTarget = null;
            SetState(MonsterState.Idle);
            
            Debug.Log($"🔄 {gameObject.name} respawned");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spawnPosition, patrolRadius);
        }
    }

    public enum MonsterState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Return,
        Dead
    }
}

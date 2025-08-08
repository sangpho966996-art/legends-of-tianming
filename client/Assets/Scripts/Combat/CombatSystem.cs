using UnityEngine;

namespace LegendsOfTianming.Core
{
    public class CombatSystem : MonoBehaviour
    {
        [Header("Combat Settings")]
        public bool inCombat = false;
        public GameObject currentTarget;
        public float combatTimeout = 10f;
        
        private float lastCombatAction = 0f;
        private Character character;

        private void Start()
        {
            character = GetComponent<Character>();
        }

        private void Update()
        {
            if (inCombat && Time.time - lastCombatAction > combatTimeout)
            {
                ExitCombat();
            }
        }

        public void EnterCombat(GameObject target)
        {
            inCombat = true;
            currentTarget = target;
            lastCombatAction = Time.time;
            
            Debug.Log($"⚔️ {character.characterName} entered combat with {target.name}");
        }

        public void ExitCombat()
        {
            inCombat = false;
            currentTarget = null;
            
            Debug.Log($"🏁 {character.characterName} exited combat");
        }

        public void UpdateCombatAction()
        {
            lastCombatAction = Time.time;
        }

        public bool CanAttack()
        {
            return character.IsAlive() && currentTarget != null;
        }

        public bool IsInCombat()
        {
            return inCombat;
        }

        public GameObject GetCurrentTarget()
        {
            return currentTarget;
        }

        public void SetTarget(GameObject target)
        {
            currentTarget = target;
            if (target != null && !inCombat)
            {
                EnterCombat(target);
            }
        }
    }
}

using UnityEngine;

namespace LegendsOfTianming.Core
{
    public class NPCBehavior : MonoBehaviour
    {
        [Header("NPC Data")]
        public NPC npcData;
        
        [Header("Interaction")]
        public float interactionRange = 3f;
        public GameObject interactionPrompt;
        
        private bool playerInRange = false;
        private GameObject nearbyPlayer = null;

        public void Initialize(NPC npc)
        {
            npcData = npc;
            gameObject.name = $"NPC_{npc.name}";
            
            CreateInteractionPrompt();
        }

        private void CreateInteractionPrompt()
        {
            GameObject promptObj = new GameObject("InteractionPrompt");
            promptObj.transform.SetParent(transform);
            promptObj.transform.localPosition = Vector3.up * 2.5f;
            
            var textRenderer = promptObj.AddComponent<TextMesh>();
            textRenderer.text = $"{npcData.name}\n{npcData.title}";
            textRenderer.fontSize = 20;
            textRenderer.color = Color.white;
            textRenderer.anchor = TextAnchor.MiddleCenter;
            
            interactionPrompt = promptObj;
            interactionPrompt.SetActive(false);
        }

        private void Update()
        {
            CheckForPlayerInteraction();
            HandleInteractionInput();
        }

        private void CheckForPlayerInteraction()
        {
            var playerManager = GameManager.Instance.GetPlayerManager();
            if (playerManager.LocalPlayer == null) return;
            
            float distance = Vector3.Distance(transform.position, playerManager.LocalPlayer.transform.position);
            
            if (distance <= interactionRange && !playerInRange)
            {
                playerInRange = true;
                nearbyPlayer = playerManager.LocalPlayer;
                ShowInteractionPrompt();
            }
            else if (distance > interactionRange && playerInRange)
            {
                playerInRange = false;
                nearbyPlayer = null;
                HideInteractionPrompt();
            }
        }

        private void HandleInteractionInput()
        {
            if (playerInRange && nearbyPlayer != null && Input.GetKeyDown(KeyCode.F))
            {
                InteractWithPlayer();
            }
        }

        private void InteractWithPlayer()
        {
            var npcManager = FindObjectOfType<NPCManager>();
            if (npcManager != null && nearbyPlayer != null)
            {
                var character = nearbyPlayer.GetComponent<Character>();
                if (character != null)
                {
                    npcManager.InteractWithNPC(npcData.id, character.characterId);
                }
            }
        }

        private void ShowInteractionPrompt()
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                
                var textMesh = interactionPrompt.GetComponent<TextMesh>();
                if (textMesh != null)
                {
                    string promptText = GetInteractionPromptText();
                    textMesh.text = $"{npcData.name}\n{npcData.title}\n{promptText}";
                }
            }
        }

        private void HideInteractionPrompt()
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        private string GetInteractionPromptText()
        {
            switch (npcData.npcType)
            {
                case NPCType.QuestGiver:
                    return "[F] Talk - Quests Available";
                case NPCType.Merchant:
                    return "[F] Shop - Buy/Sell Items";
                case NPCType.Trainer:
                    return "[F] Train - Learn Skills";
                case NPCType.Guard:
                    return "[F] Talk - Get Information";
                default:
                    return "[F] Talk";
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var character = other.GetComponent<Character>();
            if (character != null && character.GetComponent<PlayerController>().IsLocalPlayer)
            {
                Debug.Log($"👤 Player approached {npcData.name}");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var character = other.GetComponent<Character>();
            if (character != null && character.GetComponent<PlayerController>().IsLocalPlayer)
            {
                Debug.Log($"👤 Player left {npcData.name}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

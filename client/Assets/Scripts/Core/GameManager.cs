using UnityEngine;
using System.Collections;

namespace LegendsOfTianming.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Configuration")]
        public bool isDebugMode = true;
        public float targetFrameRate = 60f;
        
        [Header("Network Configuration")]
        public string serverUrl = "ws://localhost:3001";
        public bool autoConnect = true;
        
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private NetworkManager networkManager;
        private PlayerManager playerManager;
        private CombatManager combatManager;
        private UIManager uiManager;
        private AudioManager audioManager;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            Application.targetFrameRate = (int)targetFrameRate;
            QualitySettings.vSyncCount = 0;
            
            InitializeManagers();
            
            if (isDebugMode)
            {
                Debug.Log("🎮 Legends of Tianming - Game Initialized");
                Debug.Log($"📊 Target FPS: {targetFrameRate}");
                Debug.Log($"🌐 Server URL: {serverUrl}");
            }
        }

        private void InitializeManagers()
        {
            networkManager = GetComponent<NetworkManager>() ?? gameObject.AddComponent<NetworkManager>();
            playerManager = GetComponent<PlayerManager>() ?? gameObject.AddComponent<PlayerManager>();
            combatManager = GetComponent<CombatManager>() ?? gameObject.AddComponent<CombatManager>();
            uiManager = FindObjectOfType<UIManager>();
            audioManager = GetComponent<AudioManager>() ?? gameObject.AddComponent<AudioManager>();
            
            if (autoConnect)
            {
                StartCoroutine(ConnectToServerDelayed());
            }
        }

        private IEnumerator ConnectToServerDelayed()
        {
            yield return new WaitForSeconds(1f);
            networkManager.ConnectToServer(serverUrl);
        }

        public NetworkManager GetNetworkManager() => networkManager;
        public PlayerManager GetPlayerManager() => playerManager;
        public CombatManager GetCombatManager() => combatManager;
        public UIManager GetUIManager() => uiManager;
        public AudioManager GetAudioManager() => audioManager;

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                networkManager?.Disconnect();
            }
            else if (autoConnect)
            {
                networkManager?.ConnectToServer(serverUrl);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                networkManager?.Disconnect();
            }
            else if (autoConnect)
            {
                networkManager?.ConnectToServer(serverUrl);
            }
        }

        private void OnDestroy()
        {
            networkManager?.Disconnect();
        }
    }
}

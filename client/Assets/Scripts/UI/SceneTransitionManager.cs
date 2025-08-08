using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace LegendsOfTianming.Core
{
    public class SceneTransitionManager : MonoBehaviour
    {
        [Header("Loading Screen")]
        public GameObject loadingScreen;
        public Slider loadingProgressBar;
        public Text loadingText;
        public Image loadingBackground;
        
        [Header("Fade Settings")]
        public float fadeInDuration = 1f;
        public float fadeOutDuration = 1f;
        
        private static SceneTransitionManager instance;
        private bool isTransitioning = false;

        public static SceneTransitionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SceneTransitionManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SceneTransitionManager");
                        instance = go.AddComponent<SceneTransitionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLoadingScreen();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeLoadingScreen()
        {
            if (loadingScreen == null)
            {
                CreateLoadingScreen();
            }
            
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
        }

        private void CreateLoadingScreen()
        {
            GameObject canvas = new GameObject("LoadingCanvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 1000;
            
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvas.AddComponent<GraphicRaycaster>();
            
            loadingScreen = new GameObject("LoadingScreen");
            loadingScreen.transform.SetParent(canvas.transform, false);
            
            RectTransform loadingRect = loadingScreen.AddComponent<RectTransform>();
            loadingRect.anchorMin = Vector2.zero;
            loadingRect.anchorMax = Vector2.one;
            loadingRect.offsetMin = Vector2.zero;
            loadingRect.offsetMax = Vector2.zero;
            
            loadingBackground = loadingScreen.AddComponent<Image>();
            loadingBackground.color = Color.black;
            
            CreateLoadingUI();
            
            DontDestroyOnLoad(canvas);
        }

        private void CreateLoadingUI()
        {
            GameObject progressBarObj = new GameObject("ProgressBar");
            progressBarObj.transform.SetParent(loadingScreen.transform, false);
            
            RectTransform progressRect = progressBarObj.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.2f, 0.1f);
            progressRect.anchorMax = new Vector2(0.8f, 0.15f);
            progressRect.offsetMin = Vector2.zero;
            progressRect.offsetMax = Vector2.zero;
            
            loadingProgressBar = progressBarObj.AddComponent<Slider>();
            loadingProgressBar.minValue = 0f;
            loadingProgressBar.maxValue = 1f;
            loadingProgressBar.value = 0f;
            
            GameObject background = new GameObject("Background");
            background.transform.SetParent(progressBarObj.transform, false);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = Color.gray;
            
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(progressBarObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = Color.blue;
            
            loadingProgressBar.fillRect = fillRect;
            
            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(loadingScreen.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.2f);
            textRect.anchorMax = new Vector2(1f, 0.3f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            loadingText = textObj.AddComponent<Text>();
            loadingText.text = "Loading...";
            loadingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            loadingText.fontSize = 24;
            loadingText.color = Color.white;
            loadingText.alignment = TextAnchor.MiddleCenter;
        }

        public void LoadScene(string sceneName)
        {
            if (isTransitioning) return;
            
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void LoadSceneWithCharacterData(string sceneName, CharacterCreationData characterData)
        {
            if (isTransitioning) return;
            
            PlayerPrefs.SetString("SelectedCharacterClass", characterData.characterClass);
            PlayerPrefs.SetString("SelectedCharacterName", characterData.characterName);
            PlayerPrefs.Save();
            
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isTransitioning = true;
            
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
                yield return StartCoroutine(FadeIn());
            }
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                if (loadingProgressBar != null)
                    loadingProgressBar.value = progress;
                    
                if (loadingText != null)
                    loadingText.text = $"Loading... {(progress * 100):F0}%";
                
                if (asyncLoad.progress >= 0.9f)
                {
                    if (loadingText != null)
                        loadingText.text = "Press any key to continue...";
                        
                    if (Input.anyKeyDown)
                    {
                        asyncLoad.allowSceneActivation = true;
                    }
                }
                
                yield return null;
            }
            
            yield return new WaitForSeconds(0.5f);
            
            if (loadingScreen != null)
            {
                yield return StartCoroutine(FadeOut());
                loadingScreen.SetActive(false);
            }
            
            isTransitioning = false;
            
            InitializeNewScene(sceneName);
        }

        private void InitializeNewScene(string sceneName)
        {
            switch (sceneName)
            {
                case "GameWorld":
                    InitializeGameWorld();
                    break;
                case "CharacterCreation":
                    InitializeCharacterCreation();
                    break;
                case "MainMenu":
                    InitializeMainMenu();
                    break;
            }
        }

        private void InitializeGameWorld()
        {
            string characterClass = PlayerPrefs.GetString("SelectedCharacterClass", "Azure Cloud Sect");
            string characterName = PlayerPrefs.GetString("SelectedCharacterName", "Player");
            
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.InitializePlayer(characterName, characterClass);
            }
            
            Debug.Log($"🌍 Initialized game world with {characterClass} character: {characterName}");
        }

        private void InitializeCharacterCreation()
        {
            Debug.Log("🎭 Initialized character creation scene");
        }

        private void InitializeMainMenu()
        {
            Debug.Log("🏠 Initialized main menu scene");
        }

        private IEnumerator FadeIn()
        {
            if (loadingBackground == null) yield break;
            
            float elapsedTime = 0f;
            Color startColor = loadingBackground.color;
            startColor.a = 0f;
            Color endColor = startColor;
            endColor.a = 1f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                loadingBackground.color = Color.Lerp(startColor, endColor, alpha);
                yield return null;
            }
            
            loadingBackground.color = endColor;
        }

        private IEnumerator FadeOut()
        {
            if (loadingBackground == null) yield break;
            
            float elapsedTime = 0f;
            Color startColor = loadingBackground.color;
            Color endColor = startColor;
            endColor.a = 0f;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeOutDuration);
                loadingBackground.color = Color.Lerp(startColor, endColor, alpha);
                yield return null;
            }
            
            loadingBackground.color = endColor;
        }
    }

    [System.Serializable]
    public class CharacterCreationData
    {
        public string characterName;
        public string characterClass;
        public int strength = 10;
        public int agility = 10;
        public int intelligence = 10;
        public int vitality = 10;
    }
}

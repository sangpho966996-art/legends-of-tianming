using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LegendsOfTianming.Core
{
    public class UIAnimations : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float fadeInDuration = 0.3f;
        public float fadeOutDuration = 0.3f;
        public float slideInDuration = 0.5f;
        public float slideOutDuration = 0.3f;
        
        public static UIAnimations Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void FadeIn(GameObject target, System.Action onComplete = null)
        {
            StartCoroutine(FadeInCoroutine(target, onComplete));
        }

        public void FadeOut(GameObject target, System.Action onComplete = null)
        {
            StartCoroutine(FadeOutCoroutine(target, onComplete));
        }

        public void SlideIn(GameObject target, Vector3 fromPosition, System.Action onComplete = null)
        {
            StartCoroutine(SlideInCoroutine(target, fromPosition, onComplete));
        }

        public void SlideOut(GameObject target, Vector3 toPosition, System.Action onComplete = null)
        {
            StartCoroutine(SlideOutCoroutine(target, toPosition, onComplete));
        }

        public void ScaleIn(GameObject target, System.Action onComplete = null)
        {
            StartCoroutine(ScaleInCoroutine(target, onComplete));
        }

        public void ScaleOut(GameObject target, System.Action onComplete = null)
        {
            StartCoroutine(ScaleOutCoroutine(target, onComplete));
        }

        private IEnumerator FadeInCoroutine(GameObject target, System.Action onComplete)
        {
            CanvasGroup canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.alpha = 0f;
            target.SetActive(true);
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            onComplete?.Invoke();
        }

        private IEnumerator FadeOutCoroutine(GameObject target, System.Action onComplete)
        {
            CanvasGroup canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.alpha = 1f;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeOutDuration));
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            target.SetActive(false);
            onComplete?.Invoke();
        }

        private IEnumerator SlideInCoroutine(GameObject target, Vector3 fromPosition, System.Action onComplete)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            Vector3 targetPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = fromPosition;
            target.SetActive(true);
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / slideInDuration;
                t = EaseOutCubic(t);
                rectTransform.anchoredPosition = Vector3.Lerp(fromPosition, targetPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPosition;
            onComplete?.Invoke();
        }

        private IEnumerator SlideOutCoroutine(GameObject target, Vector3 toPosition, System.Action onComplete)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            Vector3 startPosition = rectTransform.anchoredPosition;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / slideOutDuration;
                t = EaseInCubic(t);
                rectTransform.anchoredPosition = Vector3.Lerp(startPosition, toPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = toPosition;
            target.SetActive(false);
            onComplete?.Invoke();
        }

        private IEnumerator ScaleInCoroutine(GameObject target, System.Action onComplete)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            Vector3 targetScale = rectTransform.localScale;
            rectTransform.localScale = Vector3.zero;
            target.SetActive(true);
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeInDuration;
                t = EaseOutBack(t);
                rectTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
                yield return null;
            }
            
            rectTransform.localScale = targetScale;
            onComplete?.Invoke();
        }

        private IEnumerator ScaleOutCoroutine(GameObject target, System.Action onComplete)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            Vector3 startScale = rectTransform.localScale;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;
                t = EaseInBack(t);
                rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
            
            rectTransform.localScale = Vector3.zero;
            target.SetActive(false);
            onComplete?.Invoke();
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }
            return canvasGroup;
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private float EaseInCubic(float t)
        {
            return t * t * t;
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        public void AnimateButton(Button button)
        {
            StartCoroutine(ButtonPressAnimation(button));
        }

        private IEnumerator ButtonPressAnimation(Button button)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            Vector3 originalScale = rectTransform.localScale;
            Vector3 pressedScale = originalScale * 0.95f;
            
            float duration = 0.1f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                rectTransform.localScale = Vector3.Lerp(originalScale, pressedScale, t);
                yield return null;
            }
            
            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                rectTransform.localScale = Vector3.Lerp(pressedScale, originalScale, t);
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
        }
    }
}

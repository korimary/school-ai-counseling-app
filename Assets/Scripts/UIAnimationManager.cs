using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAnimationManager : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] private float defaultDuration = 0.3f;
    [SerializeField] private AnimationCurve bounceEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeEase = AnimationCurve.Linear(0, 0, 1, 1);

    private static UIAnimationManager _instance;
    public static UIAnimationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIAnimationManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("UIAnimationManager");
                    _instance = go.AddComponent<UIAnimationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    #region 버튼 애니메이션

    /// <summary>
    /// 버튼 클릭 애니메이션
    /// </summary>
    public void AnimateButtonClick(Button button, System.Action onComplete = null)
    {
        if (button == null) return;
        StartCoroutine(ButtonClickCoroutine(button.transform, onComplete));
    }

    private IEnumerator ButtonClickCoroutine(Transform button, System.Action onComplete)
    {
        Vector3 originalScale = button.localScale;
        float duration = 0.15f;

        // 축소
        yield return StartCoroutine(ScaleCoroutine(button, originalScale * 0.9f, duration / 2));
        
        // 확대
        yield return StartCoroutine(ScaleCoroutine(button, originalScale, duration / 2));
        
        onComplete?.Invoke();
    }

    /// <summary>
    /// 버튼 호버 애니메이션
    /// </summary>
    public void AnimateButtonHover(Transform button, bool isHovering)
    {
        StopCoroutine("ButtonHoverCoroutine");
        StartCoroutine(ButtonHoverCoroutine(button, isHovering));
    }

    private IEnumerator ButtonHoverCoroutine(Transform button, bool isHovering)
    {
        Vector3 targetScale = isHovering ? Vector3.one * 1.05f : Vector3.one;
        yield return StartCoroutine(ScaleCoroutine(button, targetScale, 0.2f));
    }

    #endregion

    #region 패널 애니메이션

    /// <summary>
    /// 패널 페이드 인 애니메이션
    /// </summary>
    public void FadeInPanel(GameObject panel, System.Action onComplete = null)
    {
        if (panel == null) return;
        StartCoroutine(FadeInCoroutine(panel, onComplete));
    }

    /// <summary>
    /// 패널 페이드 아웃 애니메이션
    /// </summary>
    public void FadeOutPanel(GameObject panel, System.Action onComplete = null)
    {
        if (panel == null) return;
        StartCoroutine(FadeOutCoroutine(panel, onComplete));
    }

    private IEnumerator FadeInCoroutine(GameObject panel, System.Action onComplete)
    {
        CanvasGroup canvasGroup = GetOrAddCanvasGroup(panel);
        
        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        
        yield return StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f, defaultDuration));
        
        onComplete?.Invoke();
    }

    private IEnumerator FadeOutCoroutine(GameObject panel, System.Action onComplete)
    {
        CanvasGroup canvasGroup = GetOrAddCanvasGroup(panel);
        
        yield return StartCoroutine(FadeCoroutine(canvasGroup, 1f, 0f, defaultDuration));
        
        panel.SetActive(false);
        onComplete?.Invoke();
    }

    /// <summary>
    /// 패널 슬라이드 인 애니메이션
    /// </summary>
    public void SlideInPanel(GameObject panel, SlideDirection direction, System.Action onComplete = null)
    {
        if (panel == null) return;
        StartCoroutine(SlideInCoroutine(panel, direction, onComplete));
    }

    /// <summary>
    /// 패널 슬라이드 아웃 애니메이션
    /// </summary>
    public void SlideOutPanel(GameObject panel, SlideDirection direction, System.Action onComplete = null)
    {
        if (panel == null) return;
        StartCoroutine(SlideOutCoroutine(panel, direction, onComplete));
    }

    private IEnumerator SlideInCoroutine(GameObject panel, SlideDirection direction, System.Action onComplete)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        Vector2 originalPosition = rectTransform.anchoredPosition;
        Vector2 startPosition = GetSlideStartPosition(rectTransform, direction);
        
        panel.SetActive(true);
        rectTransform.anchoredPosition = startPosition;
        
        yield return StartCoroutine(MoveCoroutine(rectTransform, startPosition, originalPosition, defaultDuration));
        
        onComplete?.Invoke();
    }

    private IEnumerator SlideOutCoroutine(GameObject panel, SlideDirection direction, System.Action onComplete)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        Vector2 originalPosition = rectTransform.anchoredPosition;
        Vector2 endPosition = GetSlideEndPosition(rectTransform, direction);
        
        yield return StartCoroutine(MoveCoroutine(rectTransform, originalPosition, endPosition, defaultDuration));
        
        panel.SetActive(false);
        rectTransform.anchoredPosition = originalPosition;
        onComplete?.Invoke();
    }

    #endregion

    #region 텍스트 애니메이션

    /// <summary>
    /// 텍스트 타이핑 애니메이션
    /// </summary>
    public void TypewriterText(TextMeshProUGUI textComponent, string targetText, float charactersPerSecond = 30f, System.Action onComplete = null)
    {
        if (textComponent == null) return;
        StartCoroutine(TypewriterCoroutine(textComponent, targetText, charactersPerSecond, onComplete));
    }

    private IEnumerator TypewriterCoroutine(TextMeshProUGUI textComponent, string targetText, float charactersPerSecond, System.Action onComplete)
    {
        textComponent.text = "";
        
        float delay = 1f / charactersPerSecond;
        
        for (int i = 0; i <= targetText.Length; i++)
        {
            textComponent.text = targetText.Substring(0, i);
            yield return new WaitForSeconds(delay);
        }
        
        onComplete?.Invoke();
    }

    /// <summary>
    /// 텍스트 펄스 애니메이션
    /// </summary>
    public void PulseText(TextMeshProUGUI textComponent, float intensity = 1.2f, float duration = 0.5f)
    {
        if (textComponent == null) return;
        StartCoroutine(PulseTextCoroutine(textComponent.transform, intensity, duration));
    }

    private IEnumerator PulseTextCoroutine(Transform textTransform, float intensity, float duration)
    {
        Vector3 originalScale = textTransform.localScale;
        Vector3 pulseScale = originalScale * intensity;
        
        // 확대
        yield return StartCoroutine(ScaleCoroutine(textTransform, pulseScale, duration / 2));
        
        // 축소
        yield return StartCoroutine(ScaleCoroutine(textTransform, originalScale, duration / 2));
    }

    #endregion

    #region 감정 버튼 특별 애니메이션

    /// <summary>
    /// 감정 버튼 선택 애니메이션
    /// </summary>
    public void AnimateEmotionSelection(Transform emotionButton, Color emotionColor)
    {
        StartCoroutine(EmotionSelectionCoroutine(emotionButton, emotionColor));
    }

    private IEnumerator EmotionSelectionCoroutine(Transform emotionButton, Color emotionColor)
    {
        // 펄스 효과
        Vector3 originalScale = emotionButton.localScale;
        
        // 파티클 효과 시뮬레이션 (별 애니메이션)
        yield return StartCoroutine(StarBurstEffect(emotionButton, emotionColor));
        
        // 바운스 효과
        yield return StartCoroutine(BounceEffect(emotionButton, originalScale));
    }

    private IEnumerator StarBurstEffect(Transform center, Color color)
    {
        // 간단한 별 효과 시뮬레이션
        for (int i = 0; i < 5; i++)
        {
            // 별 이미지가 있다면 여기서 생성하고 애니메이션
            // 현재는 스케일 효과로 대체
            yield return StartCoroutine(ScaleCoroutine(center, center.localScale * 1.1f, 0.1f));
            yield return StartCoroutine(ScaleCoroutine(center, center.localScale, 0.1f));
        }
    }

    private IEnumerator BounceEffect(Transform transform, Vector3 originalScale)
    {
        float duration = 0.6f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // 바운스 곡선 적용
            float bounce = bounceEase.Evaluate(progress);
            float scale = Mathf.Lerp(1.3f, 1f, bounce);
            
            transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        transform.localScale = originalScale;
    }

    #endregion

    #region 배지 획득 애니메이션

    /// <summary>
    /// 배지 획득 축하 애니메이션
    /// </summary>
    public void AnimateBadgeEarned(Transform badgeTransform, System.Action onComplete = null)
    {
        StartCoroutine(BadgeEarnedCoroutine(badgeTransform, onComplete));
    }

    private IEnumerator BadgeEarnedCoroutine(Transform badgeTransform, System.Action onComplete)
    {
        Vector3 originalScale = badgeTransform.localScale;
        
        // 초기 스케일을 0으로 설정
        badgeTransform.localScale = Vector3.zero;
        
        // 팝업 효과
        yield return StartCoroutine(ScaleCoroutine(badgeTransform, originalScale * 1.2f, 0.3f));
        yield return StartCoroutine(ScaleCoroutine(badgeTransform, originalScale, 0.2f));
        
        // 반짝이는 효과
        for (int i = 0; i < 3; i++)
        {
            Image badgeImage = badgeTransform.GetComponent<Image>();
            if (badgeImage != null)
            {
                yield return StartCoroutine(ColorFlashCoroutine(badgeImage, Color.yellow, Color.white, 0.2f));
            }
        }
        
        onComplete?.Invoke();
    }

    #endregion

    #region 성장 그래프 애니메이션

    /// <summary>
    /// 그래프 포인트 드로우 애니메이션
    /// </summary>
    public void AnimateGraphDraw(LineRenderer lineRenderer, Vector3[] points, float duration = 2f)
    {
        StartCoroutine(GraphDrawCoroutine(lineRenderer, points, duration));
    }

    private IEnumerator GraphDrawCoroutine(LineRenderer lineRenderer, Vector3[] points, float duration)
    {
        if (lineRenderer == null || points.Length == 0) yield break;
        
        lineRenderer.positionCount = 0;
        
        float timePerPoint = duration / points.Length;
        
        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.positionCount = i + 1;
            lineRenderer.SetPosition(i, points[i]);
            
            yield return new WaitForSeconds(timePerPoint);
        }
    }

    #endregion

    #region 공통 애니메이션 유틸리티

    private IEnumerator ScaleCoroutine(Transform transform, Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, bounceEase.Evaluate(progress));
            
            yield return null;
        }
        
        transform.localScale = targetScale;
    }

    private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, fadeEase.Evaluate(progress));
            
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }

    private IEnumerator MoveCoroutine(RectTransform rectTransform, Vector2 startPos, Vector2 endPos, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, bounceEase.Evaluate(progress));
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = endPos;
    }

    private IEnumerator ColorFlashCoroutine(Image image, Color flashColor, Color originalColor, float duration)
    {
        yield return StartCoroutine(ColorCoroutine(image, originalColor, flashColor, duration / 2));
        yield return StartCoroutine(ColorCoroutine(image, flashColor, originalColor, duration / 2));
    }

    private IEnumerator ColorCoroutine(Image image, Color startColor, Color endColor, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            image.color = Color.Lerp(startColor, endColor, progress);
            
            yield return null;
        }
        
        image.color = endColor;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject gameObject)
    {
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    private Vector2 GetSlideStartPosition(RectTransform rectTransform, SlideDirection direction)
    {
        Vector2 originalPos = rectTransform.anchoredPosition;
        Vector2 screenSize = rectTransform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
        
        switch (direction)
        {
            case SlideDirection.Left:
                return new Vector2(-screenSize.x, originalPos.y);
            case SlideDirection.Right:
                return new Vector2(screenSize.x, originalPos.y);
            case SlideDirection.Up:
                return new Vector2(originalPos.x, screenSize.y);
            case SlideDirection.Down:
                return new Vector2(originalPos.x, -screenSize.y);
            default:
                return originalPos;
        }
    }

    private Vector2 GetSlideEndPosition(RectTransform rectTransform, SlideDirection direction)
    {
        Vector2 originalPos = rectTransform.anchoredPosition;
        Vector2 screenSize = rectTransform.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
        
        switch (direction)
        {
            case SlideDirection.Left:
                return new Vector2(-screenSize.x, originalPos.y);
            case SlideDirection.Right:
                return new Vector2(screenSize.x, originalPos.y);
            case SlideDirection.Up:
                return new Vector2(originalPos.x, screenSize.y);
            case SlideDirection.Down:
                return new Vector2(originalPos.x, -screenSize.y);
            default:
                return originalPos;
        }
    }

    #endregion

    #region 공개 유틸리티 메서드

    /// <summary>
    /// 간단한 펄스 애니메이션
    /// </summary>
    public void SimplePulse(Transform transform, float intensity = 1.1f, float duration = 0.5f)
    {
        StartCoroutine(SimplePulseCoroutine(transform, intensity, duration));
    }

    private IEnumerator SimplePulseCoroutine(Transform transform, float intensity, float duration)
    {
        Vector3 originalScale = transform.localScale;
        yield return StartCoroutine(ScaleCoroutine(transform, originalScale * intensity, duration / 2));
        yield return StartCoroutine(ScaleCoroutine(transform, originalScale, duration / 2));
    }

    /// <summary>
    /// 무지개 텍스트 효과
    /// </summary>
    public void RainbowText(TextMeshProUGUI textComponent, float duration = 2f)
    {
        StartCoroutine(RainbowTextCoroutine(textComponent, duration));
    }

    private IEnumerator RainbowTextCoroutine(TextMeshProUGUI textComponent, float duration)
    {
        Color[] rainbowColors = {
            Color.red, new Color(1f, 0.5f, 0f), Color.yellow, 
            Color.green, Color.blue, new Color(0.3f, 0f, 0.8f), 
            new Color(0.9f, 0f, 0.9f)
        };
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = (elapsed / duration) * rainbowColors.Length;
            
            int colorIndex = Mathf.FloorToInt(progress) % rainbowColors.Length;
            int nextColorIndex = (colorIndex + 1) % rainbowColors.Length;
            
            float colorProgress = progress - Mathf.Floor(progress);
            
            textComponent.color = Color.Lerp(rainbowColors[colorIndex], rainbowColors[nextColorIndex], colorProgress);
            
            yield return null;
        }
        
        textComponent.color = Color.white;
    }

    #endregion
}

public enum SlideDirection
{
    Left,
    Right,
    Up,
    Down
}
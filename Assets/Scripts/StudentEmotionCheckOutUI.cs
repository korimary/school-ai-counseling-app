using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StudentEmotionCheckOutUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject checkOutPanel;
    public GameObject emotionSelectionPanel;
    public GameObject resolutionPanel;
    public GameObject comparisonPanel;
    public GameObject thankYouPanel;
    public GameObject completionPanel;
    
    [Header("Header")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public Image headerBackground;
    
    [Header("Emotion Selection")]
    public EmotionButton[] emotionButtons;
    public TextMeshProUGUI selectedEmotionText;
    public Image selectedEmotionIcon;
    
    [Header("Intensity Selection")]
    public Slider intensitySlider;
    public TextMeshProUGUI intensityText;
    public Image[] intensityStars;
    public Color starActiveColor = Color.yellow;
    public Color starInactiveColor = Color.gray;
    
    [Header("Resolution Writing")]
    public TMP_InputField resolutionInput;
    public TextMeshProUGUI resolutionPlaceholder;
    public TextMeshProUGUI wordCountText;
    public int maxWordCount = 100;
    
    [Header("Emotion Comparison")]
    public TextMeshProUGUI beforeEmotionText;
    public TextMeshProUGUI afterEmotionText;
    public Image beforeEmotionIcon;
    public Image afterEmotionIcon;
    public TextMeshProUGUI comparisonResultText;
    public GameObject improvementEffect;
    
    [Header("Thank You Message")]
    public TMP_InputField thankYouInput;
    public TextMeshProUGUI thankYouPlaceholder;
    public Button addStickerButton;
    public Transform stickerContainer;
    public GameObject[] stickerPrefabs;
    
    [Header("Action Buttons")]
    public Button nextButton;
    public Button backButton;
    public Button skipButton;
    public Button finishButton;
    
    [Header("Visual Effects")]
    public ParticleSystem celebrationParticles;
    public AnimationCurve bounceAnimation;
    public Gradient rainbowGradient;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip completionSound;
    
    private string selectedEmotion;
    private int selectedIntensity = 3;
    private string todayResolution = "";
    private string thankYouMessage = "";
    
    // Check-in data for comparison
    private string checkInEmotion;
    private int checkInIntensity;
    private List<string> checkInKeywords;
    
    private int currentStep = 0;
    private const int TOTAL_STEPS = 4;
    
    private void Start()
    {
        InitializeUI();
        LoadCheckInData();
        ShowEmotionSelection();
    }
    
    private void InitializeUI()
    {
        // Setup emotion buttons
        SetupEmotionButtons();
        
        // Setup intensity slider
        SetupIntensitySlider();
        
        // Setup text inputs
        SetupTextInputs();
        
        // Setup buttons
        SetupButtons();
        
        // Setup visual elements
        SetupVisualElements();
        
        // Hide all panels initially
        HideAllPanels();
    }
    
    private void SetupEmotionButtons()
    {
        string[] emotions = { "기쁨", "슬픔", "화남", "불안", "신남", "복잡" };
        string[] emojis = { "😊", "😢", "😠", "😰", "😄", "😕" };
        
        for (int i = 0; i < emotionButtons.Length && i < emotions.Length; i++)
        {
            string emotion = emotions[i];
            string emoji = emojis[i];
            
            if (emotionButtons[i] != null)
            {
                emotionButtons[i].SetEmotion(emotion, emoji, GetEmotionColor(emotion));
                emotionButtons[i].onEmotionSelected.RemoveAllListeners();
                emotionButtons[i].onEmotionSelected.AddListener(OnEmotionSelected);
            }
        }
    }
    
    private void SetupIntensitySlider()
    {
        intensitySlider.minValue = 1;
        intensitySlider.maxValue = 5;
        intensitySlider.value = 3;
        intensitySlider.wholeNumbers = true;
        intensitySlider.onValueChanged.AddListener(OnIntensityChanged);
        
        UpdateIntensityDisplay();
    }
    
    private void SetupTextInputs()
    {
        // Resolution input
        resolutionInput.placeholder.GetComponent<TextMeshProUGUI>().text = 
            "오늘 상담 후 다짐이나 계획을 써보세요...\n예: 내일은 더 웃어보자, 친구와 더 많이 이야기하자";
        resolutionInput.onValueChanged.AddListener(OnResolutionChanged);
        resolutionInput.characterLimit = maxWordCount;
        
        // Thank you input
        thankYouInput.placeholder.GetComponent<TextMeshProUGUI>().text = 
            "선생님께 감사 인사를 남겨보세요! (선택사항)\n예: 선생님 감사합니다! 덕분에 기분이 좋아졌어요 💕";
        thankYouInput.onValueChanged.AddListener(OnThankYouChanged);
    }
    
    private void SetupButtons()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        backButton.onClick.AddListener(OnBackClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
        finishButton.onClick.AddListener(OnFinishClicked);
        
        addStickerButton.onClick.AddListener(OnAddStickerClicked);
    }
    
    private void SetupVisualElements()
    {
        titleText.text = "🌈 상담 후 체크아웃 🌈";
        titleText.color = Color.white;
        
        instructionText.text = "상담 후 기분이 어떻게 변했는지 알려주세요!";
        instructionText.color = Color.white;
        
        UpdateHeaderBackground();
    }
    
    private void LoadCheckInData()
    {
        // Load check-in data from PlayerPrefs
        checkInEmotion = PlayerPrefs.GetString("CheckIn_Emotion", "기쁨");
        checkInIntensity = PlayerPrefs.GetInt("CheckIn_Intensity", 3);
        
        string keywordString = PlayerPrefs.GetString("CheckIn_Keywords", "");
        checkInKeywords = new List<string>(keywordString.Split(','));
        
        Debug.Log($"Loaded check-in data: {checkInEmotion}, {checkInIntensity}");
    }
    
    private void ShowEmotionSelection()
    {
        HideAllPanels();
        emotionSelectionPanel.SetActive(true);
        currentStep = 0;
        
        UpdateProgressDisplay();
        StartCoroutine(AnimatePanelEntrance(emotionSelectionPanel));
    }
    
    private void OnEmotionSelected(string emotion)
    {
        selectedEmotion = emotion;
        UpdateSelectedEmotionDisplay();
        
        // Enable next button
        nextButton.interactable = true;
        
        // Play selection animation
        StartCoroutine(EmotionSelectionAnimation());
        
        // Play sound
        PlaySound(successSound);
    }
    
    private void UpdateSelectedEmotionDisplay()
    {
        string emotionName = GetEmotionName(selectedEmotion);
        string emotionEmoji = GetEmotionEmoji(selectedEmotion);
        
        selectedEmotionText.text = $"지금 기분: {emotionName} {emotionEmoji}";
        selectedEmotionText.color = GetEmotionColor(selectedEmotion);
    }
    
    private void OnIntensityChanged(float value)
    {
        selectedIntensity = Mathf.RoundToInt(value);
        UpdateIntensityDisplay();
    }
    
    private void UpdateIntensityDisplay()
    {
        intensityText.text = $"정도: {selectedIntensity}단계";
        
        // Update star display
        for (int i = 0; i < intensityStars.Length; i++)
        {
            intensityStars[i].color = i < selectedIntensity ? starActiveColor : starInactiveColor;
        }
        
        // Add animation to active stars
        StartCoroutine(AnimateStars());
    }
    
    private IEnumerator AnimateStars()
    {
        for (int i = 0; i < selectedIntensity; i++)
        {
            StartCoroutine(StarTwinkle(intensityStars[i]));
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator StarTwinkle(Image star)
    {
        Vector3 originalScale = star.transform.localScale;
        
        // Scale up
        float time = 0f;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 1.3f, time / 0.2f);
            star.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        // Scale back down
        time = 0f;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1.3f, 1f, time / 0.2f);
            star.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        star.transform.localScale = originalScale;
    }
    
    private void OnResolutionChanged(string resolution)
    {
        todayResolution = resolution;
        
        // Update word count
        int wordCount = resolution.Length;
        wordCountText.text = $"{wordCount}/{maxWordCount}";
        
        // Change color based on length
        if (wordCount > maxWordCount * 0.8f)
        {
            wordCountText.color = Color.red;
        }
        else if (wordCount > maxWordCount * 0.6f)
        {
            wordCountText.color = Color.yellow;
        }
        else
        {
            wordCountText.color = Color.green;
        }
    }
    
    private void OnThankYouChanged(string message)
    {
        thankYouMessage = message;
    }
    
    private void OnNextClicked()
    {
        currentStep++;
        
        switch (currentStep)
        {
            case 1:
                ShowResolutionPanel();
                break;
            case 2:
                ShowComparisonPanel();
                break;
            case 3:
                ShowThankYouPanel();
                break;
            case 4:
                ShowCompletionPanel();
                break;
        }
    }
    
    private void OnBackClicked()
    {
        currentStep--;
        
        switch (currentStep)
        {
            case 0:
                ShowEmotionSelection();
                break;
            case 1:
                ShowResolutionPanel();
                break;
            case 2:
                ShowComparisonPanel();
                break;
            case 3:
                ShowThankYouPanel();
                break;
        }
    }
    
    private void OnSkipClicked()
    {
        // Skip current step
        OnNextClicked();
    }
    
    private void OnFinishClicked()
    {
        SaveCheckOutData();
        ShowCompletionPanel();
    }
    
    private void ShowResolutionPanel()
    {
        HideAllPanels();
        resolutionPanel.SetActive(true);
        
        UpdateProgressDisplay();
        StartCoroutine(AnimatePanelEntrance(resolutionPanel));
    }
    
    private void ShowComparisonPanel()
    {
        HideAllPanels();
        comparisonPanel.SetActive(true);
        
        UpdateComparisonDisplay();
        UpdateProgressDisplay();
        StartCoroutine(AnimatePanelEntrance(comparisonPanel));
        StartCoroutine(ShowComparisonAnimation());
    }
    
    private void UpdateComparisonDisplay()
    {
        // Before emotion
        beforeEmotionText.text = $"상담 전: {GetEmotionName(checkInEmotion)} {GetEmotionEmoji(checkInEmotion)}";
        beforeEmotionText.color = GetEmotionColor(checkInEmotion);
        
        // After emotion
        afterEmotionText.text = $"상담 후: {GetEmotionName(selectedEmotion)} {GetEmotionEmoji(selectedEmotion)}";
        afterEmotionText.color = GetEmotionColor(selectedEmotion);
        
        // Comparison result
        UpdateComparisonResult();
    }
    
    private void UpdateComparisonResult()
    {
        int emotionImprovement = GetEmotionImprovement(checkInEmotion, selectedEmotion);
        int intensityChange = selectedIntensity - checkInIntensity;
        
        string resultText = "";
        Color resultColor = Color.white;
        
        if (emotionImprovement > 0 || intensityChange > 0)
        {
            resultText = "🌟 기분이 좋아졌어요! 🌟";
            resultColor = Color.green;
            
            // Show improvement effect
            if (improvementEffect != null)
            {
                improvementEffect.SetActive(true);
            }
            
            // Play celebration particles
            if (celebrationParticles != null)
            {
                celebrationParticles.Play();
            }
        }
        else if (emotionImprovement < 0 || intensityChange < -1)
        {
            resultText = "💙 괜찮아요, 천천히 나아질 거예요 💙";
            resultColor = Color.blue;
        }
        else
        {
            resultText = "😊 상담 받아서 수고했어요! 😊";
            resultColor = Color.yellow;
        }
        
        comparisonResultText.text = resultText;
        comparisonResultText.color = resultColor;
    }
    
    private int GetEmotionImprovement(string before, string after)
    {
        // Simple emotion improvement scoring
        int beforeScore = GetEmotionScore(before);
        int afterScore = GetEmotionScore(after);
        
        return afterScore - beforeScore;
    }
    
    private int GetEmotionScore(string emotion)
    {
        // Higher score = more positive emotion
        switch (emotion)
        {
            case "기쁨": return 5;
            case "신남": return 4;
            case "복잡": return 2;
            case "불안": return 1;
            case "슬픔": return 0;
            case "화남": return 0;
            default: return 2;
        }
    }
    
    private IEnumerator ShowComparisonAnimation()
    {
        // Animate the comparison display
        beforeEmotionText.transform.localScale = Vector3.zero;
        afterEmotionText.transform.localScale = Vector3.zero;
        
        // Show before emotion
        yield return StartCoroutine(ScaleUpAnimation(beforeEmotionText.transform));
        yield return new WaitForSeconds(0.5f);
        
        // Show after emotion
        yield return StartCoroutine(ScaleUpAnimation(afterEmotionText.transform));
        yield return new WaitForSeconds(0.5f);
        
        // Show result
        comparisonResultText.transform.localScale = Vector3.zero;
        yield return StartCoroutine(ScaleUpAnimation(comparisonResultText.transform));
    }
    
    private IEnumerator ScaleUpAnimation(Transform target)
    {
        float time = 0f;
        while (time < 0.3f)
        {
            time += Time.deltaTime;
            float scale = bounceAnimation.Evaluate(time / 0.3f);
            target.localScale = Vector3.one * scale;
            yield return null;
        }
        target.localScale = Vector3.one;
    }
    
    private void ShowThankYouPanel()
    {
        HideAllPanels();
        thankYouPanel.SetActive(true);
        
        UpdateProgressDisplay();
        StartCoroutine(AnimatePanelEntrance(thankYouPanel));
    }
    
    private void OnAddStickerClicked()
    {
        // Add random sticker
        if (stickerPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, stickerPrefabs.Length);
            GameObject sticker = Instantiate(stickerPrefabs[randomIndex], stickerContainer);
            
            // Random position within container
            RectTransform stickerRect = sticker.GetComponent<RectTransform>();
            stickerRect.anchoredPosition = new Vector2(
                Random.Range(-100f, 100f),
                Random.Range(-50f, 50f)
            );
            
            // Add click to remove functionality
            Button stickerButton = sticker.GetComponent<Button>();
            if (stickerButton != null)
            {
                stickerButton.onClick.AddListener(() => RemoveSticker(sticker));
            }
            
            // Animate sticker appearance
            StartCoroutine(AnimateStickerAppearance(sticker));
        }
    }
    
    private IEnumerator AnimateStickerAppearance(GameObject sticker)
    {
        sticker.transform.localScale = Vector3.zero;
        sticker.transform.Rotate(0, 0, Random.Range(-30f, 30f));
        
        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            float scale = bounceAnimation.Evaluate(time / 0.5f);
            sticker.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        sticker.transform.localScale = Vector3.one;
    }
    
    private void RemoveSticker(GameObject sticker)
    {
        StartCoroutine(AnimateStickerRemoval(sticker));
    }
    
    private IEnumerator AnimateStickerRemoval(GameObject sticker)
    {
        float time = 0f;
        Vector3 originalScale = sticker.transform.localScale;
        
        while (time < 0.3f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, time / 0.3f);
            sticker.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        Destroy(sticker);
    }
    
    private void ShowCompletionPanel()
    {
        HideAllPanels();
        completionPanel.SetActive(true);
        
        // Play completion sound
        PlaySound(completionSound);
        
        // Show final celebration
        StartCoroutine(FinalCelebration());
    }
    
    private IEnumerator FinalCelebration()
    {
        // Rainbow text effect
        StartCoroutine(RainbowTextEffect());
        
        // Particle celebration
        if (celebrationParticles != null)
        {
            celebrationParticles.Play();
        }
        
        yield return new WaitForSeconds(3f);
        
        // Auto-close or return to main menu
        // This could transition back to the main counseling app
    }
    
    private IEnumerator RainbowTextEffect()
    {
        TextMeshProUGUI completionText = completionPanel.GetComponentInChildren<TextMeshProUGUI>();
        
        float time = 0f;
        while (time < 5f)
        {
            time += Time.deltaTime;
            float gradient = Mathf.Sin(time * 2f) * 0.5f + 0.5f;
            completionText.color = rainbowGradient.Evaluate(gradient);
            yield return null;
        }
    }
    
    private void SaveCheckOutData()
    {
        // Save check-out data
        PlayerPrefs.SetInt("CheckOut_Emotion", EmotionTypes.GetEmotionIndex(selectedEmotion));
        PlayerPrefs.SetInt("CheckOut_Intensity", selectedIntensity);
        PlayerPrefs.SetString("CheckOut_Resolution", todayResolution);
        PlayerPrefs.SetString("CheckOut_ThankYou", thankYouMessage);
        PlayerPrefs.SetString("CheckOut_Timestamp", System.DateTime.Now.ToString());
        
        // Calculate and save improvement
        int emotionImprovement = GetEmotionImprovement(checkInEmotion, selectedEmotion);
        int intensityChange = selectedIntensity - checkInIntensity;
        PlayerPrefs.SetInt("CheckOut_EmotionImprovement", emotionImprovement);
        PlayerPrefs.SetInt("CheckOut_IntensityChange", intensityChange);
        
        Debug.Log($"Check-out data saved: {selectedEmotion}, {selectedIntensity}, Resolution: {todayResolution}");
    }
    
    private void UpdateProgressDisplay()
    {
        // Update progress indicator if available
        float progress = (float)currentStep / TOTAL_STEPS;
        Debug.Log($"Progress: {currentStep}/{TOTAL_STEPS} ({progress:P})");
    }
    
    private void HideAllPanels()
    {
        checkOutPanel.SetActive(false);
        emotionSelectionPanel.SetActive(false);
        resolutionPanel.SetActive(false);
        comparisonPanel.SetActive(false);
        thankYouPanel.SetActive(false);
        completionPanel.SetActive(false);
    }
    
    private IEnumerator AnimatePanelEntrance(GameObject panel)
    {
        panel.transform.localScale = Vector3.zero;
        
        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            float scale = bounceAnimation.Evaluate(time / 0.5f);
            panel.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        
        panel.transform.localScale = Vector3.one;
    }
    
    private IEnumerator EmotionSelectionAnimation()
    {
        // Flash selected emotion
        for (int i = 0; i < 3; i++)
        {
            selectedEmotionText.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            selectedEmotionText.color = GetEmotionColor(selectedEmotion);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void UpdateHeaderBackground()
    {
        // Create a colorful gradient background
        if (headerBackground != null)
        {
            Color topColor = new Color(0.9f, 0.9f, 0.6f); // Light yellow
            Color bottomColor = new Color(0.6f, 0.9f, 0.6f); // Light green
            
            headerBackground.color = topColor;
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private Color GetEmotionColor(string emotion)
    {
        switch (emotion)
        {
            case "기쁨": return Color.yellow;
            case "슬픔": return Color.blue;
            case "화남": return Color.red;
            case "불안": return Color.magenta;
            case "신남": return new Color(1f, 0.5f, 0f); // Orange
            case "복잡": return Color.gray;
            default: return Color.white;
        }
    }
    
    private string GetEmotionName(string emotion)
    {
        // 이미 한글 이름이므로 그대로 반환
        if (string.IsNullOrEmpty(emotion))
            return "알 수 없음";
        return emotion;
    }
    
    private string GetEmotionEmoji(string emotion)
    {
        switch (emotion)
        {
            case "기쁨": return "😊";
            case "슬픔": return "😢";
            case "화남": return "😠";
            case "불안": return "😰";
            case "신남": return "😄";
            case "복잡": return "😕";
            default: return "🤔";
        }
    }
}
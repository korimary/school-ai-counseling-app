using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StudentEmotionCheckInUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject checkInPanel;
    public GameObject emotionSelectionPanel;
    public GameObject confirmationPanel;
    public GameObject waitingPanel;
    
    [Header("Header")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public Image headerBackground;
    
    [Header("Emotion Selection")]
    public EmotionButton[] emotionButtons;
    public TextMeshProUGUI selectedEmotionText;
    public Image selectedEmotionIcon;
    
    private string selectedEmotion;
    private int selectedIntensity = 3;
    private List<string> selectedKeywords = new List<string>();
    
    [Header("Intensity Selection")]
    public Slider intensitySlider;
    public TextMeshProUGUI intensityText;
    public Image[] intensityStars;
    public Color starActiveColor = Color.yellow;
    public Color starInactiveColor = Color.gray;
    
    [Header("Keyword Input")]
    public TMP_InputField keywordInput;
    public TextMeshProUGUI keywordPlaceholder;
    public Button addKeywordButton;
    public Transform keywordContainer;
    public GameObject keywordTagPrefab;
    
    [Header("Action Buttons")]
    public Button confirmButton;
    public Button cancelButton;
    public Button helpButton;
    
    [Header("Confirmation Screen")]
    public TextMeshProUGUI confirmationText;
    public Image confirmationEmotionIcon;
    public TextMeshProUGUI confirmationDetailsText;
    public Button proceedButton;
    public Button editButton;
    
    [Header("Waiting Screen")]
    public TextMeshProUGUI waitingText;
    public Image waitingAnimation;
    public Button callTeacherButton;
    
    [Header("Visual Settings")]
    public Gradient backgroundGradient;
    public AnimationCurve bounceAnimation;
    public float animationSpeed = 1f;
    
    #pragma warning disable 0414
    private bool isAnimating = false;
    #pragma warning restore 0414
    
    private void Start()
    {
        InitializeUI();
        ShowEmotionSelection();
    }
    
    private void InitializeUI()
    {
        // Setup emotion buttons
        SetupEmotionButtons();
        
        // Setup intensity slider
        SetupIntensitySlider();
        
        // Setup keyword input
        SetupKeywordInput();
        
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
    
    private void SetupKeywordInput()
    {
        keywordInput.placeholder.GetComponent<TextMeshProUGUI>().text = "예: 친구, 숙제, 시험...";
        keywordInput.onEndEdit.AddListener(OnKeywordEntered);
        addKeywordButton.onClick.AddListener(AddCurrentKeyword);
    }
    
    private void SetupButtons()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        helpButton.onClick.AddListener(OnHelpClicked);
        
        proceedButton.onClick.AddListener(OnProceedClicked);
        editButton.onClick.AddListener(OnEditClicked);
        
        callTeacherButton.onClick.AddListener(OnCallTeacherClicked);
    }
    
    private void SetupVisualElements()
    {
        // Setup colorful header
        titleText.text = "🌟 오늘의 기분 체크인 🌟";
        titleText.color = Color.white;
        
        instructionText.text = "상담 전 지금 기분을 알려주세요!";
        instructionText.color = Color.white;
        
        // Setup gradient background
        UpdateBackgroundGradient();
        
        // Setup confirmation elements
        confirmationText.text = "기분을 확인해 주세요! 😊";
        
        // Setup waiting elements
        waitingText.text = "선생님이 곧 만나러 올게요! 💕\n잠깐만 기다려주세요~";
    }
    
    private void ShowEmotionSelection()
    {
        HideAllPanels();
        emotionSelectionPanel.SetActive(true);
        
        // Animate panel entrance
        StartCoroutine(AnimatePanelEntrance(emotionSelectionPanel));
    }
    
    private void OnEmotionSelected(string emotion)
    {
        selectedEmotion = emotion;
        UpdateSelectedEmotionDisplay();
        
        // Enable confirm button
        confirmButton.interactable = true;
        
        // Play selection animation
        StartCoroutine(EmotionSelectionAnimation());
    }
    
    private void UpdateSelectedEmotionDisplay()
    {
        string emotionName = GetEmotionName(selectedEmotion);
        string emotionEmoji = GetEmotionEmoji(selectedEmotion);
        
        selectedEmotionText.text = $"{emotionName} {emotionEmoji}";
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
    
    private void OnKeywordEntered(string keyword)
    {
        if (!string.IsNullOrEmpty(keyword) && Input.GetKeyDown(KeyCode.Return))
        {
            AddKeyword(keyword);
        }
    }
    
    private void AddCurrentKeyword()
    {
        string keyword = keywordInput.text.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            AddKeyword(keyword);
        }
    }
    
    private void AddKeyword(string keyword)
    {
        if (!selectedKeywords.Contains(keyword) && selectedKeywords.Count < 5)
        {
            selectedKeywords.Add(keyword);
            CreateKeywordTag(keyword);
            keywordInput.text = "";
        }
    }
    
    private void CreateKeywordTag(string keyword)
    {
        GameObject tag = Instantiate(keywordTagPrefab, keywordContainer);
        TextMeshProUGUI tagText = tag.GetComponentInChildren<TextMeshProUGUI>();
        tagText.text = keyword;
        
        // Add remove button functionality
        Button removeButton = tag.GetComponentInChildren<Button>();
        removeButton.onClick.AddListener(() => RemoveKeyword(keyword, tag));
        
        // Animate tag appearance
        StartCoroutine(AnimateTagAppearance(tag));
    }
    
    private IEnumerator AnimateTagAppearance(GameObject tag)
    {
        tag.transform.localScale = Vector3.zero;
        
        float time = 0f;
        while (time < 0.3f)
        {
            time += Time.deltaTime;
            float scale = bounceAnimation.Evaluate(time / 0.3f);
            tag.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        
        tag.transform.localScale = Vector3.one;
    }
    
    private void RemoveKeyword(string keyword, GameObject tag)
    {
        selectedKeywords.Remove(keyword);
        StartCoroutine(AnimateTagRemoval(tag));
    }
    
    private IEnumerator AnimateTagRemoval(GameObject tag)
    {
        Vector3 originalScale = tag.transform.localScale;
        
        float time = 0f;
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, time / 0.2f);
            tag.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        Destroy(tag);
    }
    
    private void OnConfirmClicked()
    {
        ShowConfirmation();
    }
    
    private void ShowConfirmation()
    {
        HideAllPanels();
        confirmationPanel.SetActive(true);
        
        // Update confirmation display
        UpdateConfirmationDisplay();
        
        // Animate confirmation panel
        StartCoroutine(AnimatePanelEntrance(confirmationPanel));
    }
    
    private void UpdateConfirmationDisplay()
    {
        string emotionName = GetEmotionName(selectedEmotion);
        string emotionEmoji = GetEmotionEmoji(selectedEmotion);
        
        confirmationText.text = $"선택한 기분: {emotionName} {emotionEmoji}";
        confirmationText.color = GetEmotionColor(selectedEmotion);
        
        string keywordText = selectedKeywords.Count > 0 ? string.Join(", ", selectedKeywords) : "없음";
        confirmationDetailsText.text = $"정도: {selectedIntensity}단계\n키워드: {keywordText}";
    }
    
    private void OnCancelClicked()
    {
        ShowEmotionSelection();
    }
    
    private void OnHelpClicked()
    {
        // Show help dialog
        Debug.Log("도움말 표시");
    }
    
    private void OnProceedClicked()
    {
        // Save check-in data
        SaveCheckInData();
        
        // Show waiting screen
        ShowWaitingScreen();
    }
    
    private void OnEditClicked()
    {
        ShowEmotionSelection();
    }
    
    private void ShowWaitingScreen()
    {
        HideAllPanels();
        waitingPanel.SetActive(true);
        
        // Start waiting animation
        StartCoroutine(WaitingAnimation());
        
        // Animate panel entrance
        StartCoroutine(AnimatePanelEntrance(waitingPanel));
    }
    
    private IEnumerator WaitingAnimation()
    {
        while (waitingPanel.activeInHierarchy)
        {
            // Rotate waiting animation image
            waitingAnimation.transform.Rotate(0, 0, 180f * Time.deltaTime);
            
            // Pulse waiting text
            float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.2f;
            waitingText.transform.localScale = Vector3.one * pulse;
            
            yield return null;
        }
    }
    
    private void OnCallTeacherClicked()
    {
        // Send notification to teacher
        Debug.Log("선생님 호출");
    }
    
    private void SaveCheckInData()
    {
        // Save to PlayerPrefs or send to server
        PlayerPrefs.SetString("CheckIn_Emotion", selectedEmotion);
        PlayerPrefs.SetInt("CheckIn_Intensity", selectedIntensity);
        PlayerPrefs.SetString("CheckIn_Keywords", string.Join(",", selectedKeywords));
        PlayerPrefs.SetString("CheckIn_Timestamp", System.DateTime.Now.ToString());
        
        Debug.Log($"Check-in data saved: {selectedEmotion}, {selectedIntensity}, {string.Join(",", selectedKeywords)}");
    }
    
    private void HideAllPanels()
    {
        checkInPanel.SetActive(false);
        emotionSelectionPanel.SetActive(false);
        confirmationPanel.SetActive(false);
        waitingPanel.SetActive(false);
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
    
    private void UpdateBackgroundGradient()
    {
        if (headerBackground != null)
        {
            // Create a colorful gradient background
            Color topColor = new Color(0.9f, 0.6f, 0.9f); // Light pink
            Color bottomColor = new Color(0.6f, 0.9f, 0.9f); // Light cyan
            
            // This would need a gradient shader or multiple images
            headerBackground.color = topColor;
        }
    }
    
    private Color GetEmotionColor(string emotion)
    {
        switch (emotion)
        {
            case "기쁨": return new Color(1f, 0.9f, 0.3f); // 노란색
            case "슬픔": return new Color(0.3f, 0.5f, 1f); // 파란색
            case "화남": return new Color(1f, 0.3f, 0.3f); // 빨간색
            case "불안": return new Color(0.8f, 0.3f, 1f); // 보라색
            case "신남": return new Color(1f, 0.5f, 0.3f); // 주황색
            case "복잡": return new Color(0.5f, 0.5f, 0.5f); // 회색
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
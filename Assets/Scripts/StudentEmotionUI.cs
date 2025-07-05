using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StudentEmotionUI : MonoBehaviour
{
    // EmotionType 열거형 추가
    public enum EmotionType
    {
        Happy,      // 기쁨
        Sad,        // 슬픔
        Angry,      // 화남
        Anxious,    // 불안
        Excited,    // 신남
        Confused    // 복잡
    }
    
    [Header("UI 패널들")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject emotionPanel;
    [SerializeField] private GameObject intensityPanel;
    [SerializeField] private GameObject keywordPanel;
    [SerializeField] private GameObject summaryPanel;

    [Header("Welcome Panel")]
    [SerializeField] private TextMeshProUGUI welcomeText;
    [SerializeField] private TextMeshProUGUI studentInfoText;
    [SerializeField] private Button startButton;

    [Header("Emotion Panel")]
    [SerializeField] private TextMeshProUGUI emotionTitleText;
    [SerializeField] private Transform emotionButtonContainer;
    [SerializeField] private EmotionButton[] emotionButtons;

    [Header("Intensity Panel")]
    [SerializeField] private TextMeshProUGUI intensityTitleText;
    [SerializeField] private Slider intensitySlider;
    [SerializeField] private TextMeshProUGUI intensityValueText;
    [SerializeField] private Transform starContainer;
    [SerializeField] private Image[] starImages;

    [Header("Keyword Panel")]
    [SerializeField] private TextMeshProUGUI keywordTitleText;
    [SerializeField] private TMP_InputField keywordInput;
    [SerializeField] private Button skipKeywordButton;

    [Header("Summary Panel")]
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("Settings")]
    [SerializeField] private bool isCheckInMode = true;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;

    // 감정 데이터
    private EmotionData currentEmotionData;
    private string selectedEmotion;
    private int selectedIntensity = 1;
    private string enteredKeyword;

    // 패널 상태 관리
    private int currentPanelIndex = 0;
    private GameObject[] panels;
    
    // 텍스트 정리 타이머
    private float lastTextCleanTime = 0f;
    private const float TEXT_CLEAN_INTERVAL = 2f; // 2초마다 체크

    // 감정 종류와 기호
    private readonly Dictionary<string, string> emotionEmojis = new Dictionary<string, string>
    {
        { "기쁨", "^_^" },
        { "슬픔", "T_T" },
        { "화남", ">_<" },
        { "불안", "@_@" },
        { "신남", "o_o" },
        { "복잡", "?_?" }
    };

    // 감정 색상
    private readonly Dictionary<string, Color> emotionColors = new Dictionary<string, Color>
    {
        { "기쁨", new Color(1f, 0.9f, 0.3f) },      // 노란색
        { "슬픔", new Color(0.3f, 0.5f, 1f) },      // 파란색
        { "화남", new Color(1f, 0.3f, 0.3f) },      // 빨간색
        { "불안", new Color(0.8f, 0.3f, 1f) },      // 보라색
        { "신남", new Color(1f, 0.5f, 0.3f) },      // 주황색
        { "복잡", new Color(0.5f, 0.5f, 0.5f) }     // 회색
    };

    private void Start()
    {
        // Canvas 설정 최적화 (화질 문제 해결)
        OptimizeCanvasSettings();
        
        // 즉시 모든 텍스트 정리 (UI 검증 전에 실행)
        CleanAllTextComponentsInScene();
        
        // UI 검증 및 초기화
        if (ValidateRequiredComponents())
        {
            InitializeUI();
            SetupPanels();
            LoadStudentInfo();
        }
        else
        {
            Debug.LogWarning("⚠️ UI 컴포넌트가 연결되지 않았습니다.");
            Debug.Log("📋 해결 방법:");
            Debug.Log("1. Edit Mode에서 Inspector → '🎮 Create Basic UI (Test)' 클릭");
            Debug.Log("2. 그 다음 Inspector → '🔗 Auto Connect Copied UI' 클릭");
            Debug.Log("3. 또는 Play Mode에서 Inspector → '🎨 Create UI for Copying' 실행 후 복사/붙여넣기");
            
            // 컴포넌트를 비활성화하지 않고 계속 실행 (UI 생성 기능을 사용할 수 있도록)
        }
    }
    
    private void Awake()
    {
        // Awake에서도 한번 더 정리 (가장 빠른 시점)
        CleanAllTextComponentsInScene();
    }
    
    private void OnEnable()
    {
        // 컴포넌트가 활성화될 때마다 정리
        StartCoroutine(CleanTextComponentsCoroutine());
    }
    
    private System.Collections.IEnumerator CleanTextComponentsCoroutine()
    {
        // 한 프레임 기다린 후 정리
        yield return null;
        CleanAllTextComponentsInScene();
        
        // 몇 프레임 더 기다린 후 다시 정리
        yield return new WaitForSeconds(0.1f);
        CleanAllTextComponentsInScene();
    }
    
    private void Update()
    {
        // 주기적으로 텍스트 정리 (Play Mode에서만)
        if (Application.isPlaying && Time.time - lastTextCleanTime > TEXT_CLEAN_INTERVAL)
        {
            lastTextCleanTime = Time.time;
            CleanAllTextComponentsInScene();
        }
    }
    
    /// <summary>
    /// Scene의 모든 텍스트 컴포넌트에서 문제가 있는 유니코드 문자들을 강제 제거합니다.
    /// </summary>
    private void CleanAllTextComponentsInScene()
    {
        try
        {
            // 모든 TextMeshProUGUI 컴포넌트 찾기 (비활성화된 것들도 포함)
            TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            
            int cleanedCount = 0;
            foreach (var textComp in allTexts)
            {
                if (textComp != null && textComp.gameObject.scene.isLoaded)
                {
                    // 현재 Scene의 오브젝트만 처리
                    if (textComp.transform.root.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                    {
                        string originalText = textComp.text;
                        if (!string.IsNullOrEmpty(originalText))
                        {
                            // 문제가 있는 유니코드 문자들을 안전한 문자로 교체
                            string cleanText = CleanUnicodeText(originalText);
                            if (cleanText != originalText)
                            {
                                Debug.Log($"텍스트 정리: {textComp.name} - '{originalText}' → '{cleanText}'");
                                textComp.text = cleanText;
                                cleanedCount++;
                            }
                            
                            // 한글 폰트 적용
                            ApplyKoreanFont(textComp);
                        }
                    }
                }
            }
            
            if (cleanedCount > 0)
            {
                Debug.Log($"✅ {cleanedCount}개의 텍스트 컴포넌트를 정리했습니다.");
            }
            
            // Unity Text 컴포넌트도 처리 (구버전 호환성)
            UnityEngine.UI.Text[] legacyTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
            foreach (var textComp in legacyTexts)
            {
                if (textComp != null && textComp.gameObject.scene.isLoaded)
                {
                    if (textComp.transform.root.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                    {
                        string originalText = textComp.text;
                        if (!string.IsNullOrEmpty(originalText))
                        {
                            string cleanText = CleanUnicodeText(originalText);
                            if (cleanText != originalText)
                            {
                                Debug.Log($"레거시 텍스트 정리: {textComp.name} - '{originalText}' → '{cleanText}'");
                                textComp.text = cleanText;
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"텍스트 정리 중 오류: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 기존 Scene에 있는 텍스트 컴포넌트들에서 문제가 있는 유니코드 문자들을 제거합니다.
    /// </summary>
    private void CleanExistingTextComponents()
    {
        CleanAllTextComponentsInScene();
    }
    
    /// <summary>
    /// 텍스트에서 문제가 있는 유니코드 문자들을 안전한 문자로 교체합니다.
    /// </summary>
    private string CleanUnicodeText(string originalText)
    {
        if (string.IsNullOrEmpty(originalText)) return originalText;
        
        // 문제가 있는 문자들을 안전한 문자로 매핑
        var replacementMap = new Dictionary<string, string>
        {
            { "★", "*" },           // 별표
            { "⭐", "*" },           // 별표 이모지
            { "🌟", "*" },          // 별표 이모지
            { "♡", "^_^" },         // 하트
            { "♪", "~" },           // 음표
            { "▲", "^" },           // 삼각형
            { "▽", "v" },           // 역삼각형
            { "◐", "o" },           // 반원
            { "※", "*" },           // 별표
            { "■", "[정리]" },       // 사각형
            { "📝", "[메모]" },      // 메모 이모지
            { "🎉", "!" },          // 축하 이모지
            { "😊", "^_^" },        // 웃는 얼굴
            { "😢", "T_T" },        // 우는 얼굴
            { "😠", ">_<" },        // 화난 얼굴
            { "😰", "@_@" },        // 불안한 얼굴
            { "😄", "^o^" },        // 기쁜 얼굴
            { "😕", "?_?" }         // 복잡한 얼굴
        };
        
        string cleanText = originalText;
        foreach (var replacement in replacementMap)
        {
            cleanText = cleanText.Replace(replacement.Key, replacement.Value);
        }
        
        return cleanText;
    }
    
    /// <summary>
    /// Canvas 설정을 최적화하여 텍스트 화질 문제와 상하 반전 문제를 해결합니다.
    /// </summary>
    private void OptimizeCanvasSettings()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in canvases)
        {
            // Canvas Scaler 최적화
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // 화질 개선을 위한 설정
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                Debug.Log($"Canvas Scaler 최적화됨: {canvas.name}");
            }
            
            // Canvas 렌더링 모드 확인 및 수정
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // 카메라 모드에서 상하 반전 문제 해결
                Camera canvasCamera = canvas.worldCamera;
                if (canvasCamera != null)
                {
                    // 카메라 설정 정규화
                    canvasCamera.orthographic = true;
                    canvasCamera.nearClipPlane = 0.1f;
                    canvasCamera.farClipPlane = 1000f;
                    
                    Debug.Log($"Canvas 카메라 설정 최적화됨: {canvas.name}");
                }
            }
            
            // TextMeshPro 컴포넌트 최적화
            TextMeshProUGUI[] tmpComponents = canvas.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI tmp in tmpComponents)
            {
                if (tmp != null)
                {
                    // 텍스트 렌더링 품질 향상
                    tmp.enableAutoSizing = false;
                    tmp.fontSize = Mathf.Max(tmp.fontSize, 14f); // 최소 폰트 크기 보장
                    tmp.fontSizeMin = 12f;
                    tmp.fontSizeMax = 72f;
                    
                    // 텍스트 정렬 정규화 (상하 반전 방지)
                    if (tmp.alignment == TextAlignmentOptions.TopLeft || 
                        tmp.alignment == TextAlignmentOptions.Top || 
                        tmp.alignment == TextAlignmentOptions.TopRight)
                    {
                        // 상단 정렬 유지
                    }
                    else if (tmp.alignment == TextAlignmentOptions.BottomLeft || 
                             tmp.alignment == TextAlignmentOptions.Bottom || 
                             tmp.alignment == TextAlignmentOptions.BottomRight)
                    {
                        // 하단 정렬 유지
                    }
                    else
                    {
                        // 중앙 정렬로 통일
                        tmp.alignment = TextAlignmentOptions.Center;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Inspector에서 설정해야 할 필수 UI 컴포넌트들의 유효성을 검사합니다.
    /// 모든 필수 컴포넌트가 연결되어 있어야 true를 반환합니다.
    /// </summary>
    /// <returns>모든 필수 UI 컴포넌트가 유효하면 true</returns>
    private bool ValidateRequiredComponents()
    {
        bool isValid = true;
        string missingComponents = "";

        // 필수 패널들 검사
        if (welcomePanel == null) { missingComponents += "WelcomePanel, "; isValid = false; }
        if (emotionPanel == null) { missingComponents += "EmotionPanel, "; isValid = false; }
        if (intensityPanel == null) { missingComponents += "IntensityPanel, "; isValid = false; }
        if (keywordPanel == null) { missingComponents += "KeywordPanel, "; isValid = false; }
        if (summaryPanel == null) { missingComponents += "SummaryPanel, "; isValid = false; }

        // 필수 버튼들 검사
        if (startButton == null) { missingComponents += "StartButton, "; isValid = false; }
        if (nextButton == null) { missingComponents += "NextButton, "; isValid = false; }
        if (previousButton == null) { missingComponents += "PreviousButton, "; isValid = false; }
        if (confirmButton == null) { missingComponents += "ConfirmButton, "; isValid = false; }
        if (backButton == null) { missingComponents += "BackButton, "; isValid = false; }
        if (skipKeywordButton == null) { missingComponents += "SkipKeywordButton, "; isValid = false; }

        // 필수 텍스트 컴포넌트들 검사
        if (welcomeText == null) { missingComponents += "WelcomeText, "; isValid = false; }
        if (studentInfoText == null) { missingComponents += "StudentInfoText, "; isValid = false; }
        if (emotionTitleText == null) { missingComponents += "EmotionTitleText, "; isValid = false; }
        if (intensityTitleText == null) { missingComponents += "IntensityTitleText, "; isValid = false; }
        if (intensityValueText == null) { missingComponents += "IntensityValueText, "; isValid = false; }
        if (keywordTitleText == null) { missingComponents += "KeywordTitleText, "; isValid = false; }
        if (summaryText == null) { missingComponents += "SummaryText, "; isValid = false; }

        // 필수 인터랙션 컴포넌트들 검사
        if (intensitySlider == null) { missingComponents += "IntensitySlider, "; isValid = false; }
        if (keywordInput == null) { missingComponents += "KeywordInput, "; isValid = false; }

        // 컨테이너들 검사
        if (emotionButtonContainer == null) { missingComponents += "EmotionButtonContainer, "; isValid = false; }
        if (starContainer == null) { missingComponents += "StarContainer, "; isValid = false; }

        if (!isValid)
        {
            Debug.LogError($"다음 UI 컴포넌트들이 Inspector에서 연결되지 않았습니다: {missingComponents.TrimEnd(',', ' ')}");
        }

        return isValid;
    }

    private void InitializeUI()
    {
        // 패널 배열 설정
        panels = new GameObject[] { welcomePanel, emotionPanel, intensityPanel, keywordPanel, summaryPanel };

        // 버튼 이벤트 설정
        startButton.onClick.AddListener(StartEmotionCheck);
        nextButton.onClick.AddListener(NextPanel);
        previousButton.onClick.AddListener(PreviousPanel);
        confirmButton.onClick.AddListener(ConfirmEmotionData);
        backButton.onClick.AddListener(GoBackToMainMenu);
        skipKeywordButton.onClick.AddListener(SkipKeyword);

        // 강도 슬라이더 설정
        intensitySlider.onValueChanged.AddListener(OnIntensityChanged);

        // 감정 버튼 설정
        SetupEmotionButtons();

        // 별 이미지 설정
        SetupStarImages();

        // 첫 번째 패널만 활성화
        ShowPanel(0);
    }

    private void SetupPanels()
    {
        // 환영 패널 텍스트 설정
        if (isCheckInMode)
        {
            SetSafeText(welcomeText, "마음쑥쑥에 오신 것을 환영해요!");
            SetSafeText(emotionTitleText, "지금 기분이 어떤가요?");
            SetSafeText(intensityTitleText, "그 기분이 얼마나 강한가요?");
            SetSafeText(keywordTitleText, "오늘 있었던 일이 있나요?\n(선택사항)");
        }
        else
        {
            SetSafeText(welcomeText, "상담이 끝났어요!");
            SetSafeText(emotionTitleText, "지금 기분이 어떤가요?");
            SetSafeText(intensityTitleText, "그 기분이 얼마나 강한가요?");
            SetSafeText(keywordTitleText, "오늘의 다짐을 써주세요!\n(선택사항)");
        }
    }
    
    /// <summary>
    /// 텍스트를 안전하게 설정합니다 (문제가 있는 유니코드 문자들을 제거)
    /// </summary>
    private void SetSafeText(TextMeshProUGUI textComponent, string text)
    {
        if (textComponent != null)
        {
            textComponent.text = CleanUnicodeText(text);
            ApplyKoreanFont(textComponent);
        }
    }

    private void SetupEmotionButtons()
    {
        // emotionButtons가 Inspector에서 설정되지 않은 경우 자동으로 찾기
        if (emotionButtons == null || emotionButtons.Length == 0)
        {
            if (emotionButtonContainer != null)
            {
                emotionButtons = emotionButtonContainer.GetComponentsInChildren<EmotionButton>();
                Debug.Log($"감정 버튼을 자동으로 찾았습니다: {emotionButtons.Length}개");
            }
        }

        string[] emotions = { "기쁨", "슬픔", "화남", "불안", "신남", "복잡" };
        
        for (int i = 0; i < emotions.Length && i < emotionButtons.Length; i++)
        {
            string emotion = emotions[i];
            EmotionButton button = emotionButtons[i];
            
            if (button != null)
            {
                try
                {
                    button.SetEmotion(emotion, emotionEmojis[emotion], emotionColors[emotion]);
                    button.onEmotionSelected.AddListener(OnEmotionSelected);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"감정 버튼 설정 실패: {emotion} - {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"감정 버튼 {i}이 null입니다: {emotion}");
            }
        }
    }

    private void SetupStarImages()
    {
        // starImages가 Inspector에서 설정되지 않은 경우 자동으로 찾기
        if (starImages == null || starImages.Length == 0)
        {
            if (starContainer != null)
            {
                starImages = starContainer.GetComponentsInChildren<Image>();
                Debug.Log($"별 이미지를 자동으로 찾았습니다: {starImages.Length}개");
            }
            else
            {
                Debug.LogWarning("StarContainer가 설정되지 않아 별 이미지를 찾을 수 없습니다.");
                return;
            }
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                starImages[i].color = Color.gray;
            }
        }
    }

    private void LoadStudentInfo()
    {
        if (UserManager.IsStudentMode())
        {
            string studentInfo = $"{UserManager.GetStudentID()}번 {UserManager.GetStudentName()}";
            SetSafeText(studentInfoText, studentInfo);
        }
        else
        {
            SetSafeText(studentInfoText, "학생 정보를 불러올 수 없습니다.");
        }
    }

    private void StartEmotionCheck()
    {
        // 새로운 감정 세션 시작
        if (UserManager.IsStudentMode())
        {
            string classCode = ClassCodeManager.GetCurrentClassCode();
            int studentNumber = UserManager.GetStudentID();
            
            EmotionManager.Instance.StartNewSession(classCode, studentNumber.ToString());
            currentEmotionData = EmotionManager.Instance.GetCurrentSessionData();
        }

        NextPanel();
    }

    public void OnEmotionSelected(string emotion)
    {
        selectedEmotion = emotion;
        
        // 모든 버튼 비활성화
        foreach (var button in emotionButtons)
        {
            if (button != null)
            {
                button.SetSelected(false);
            }
        }

        // 선택된 버튼만 활성화
        foreach (var button in emotionButtons)
        {
            if (button != null && button.GetEmotionName() == emotion)
            {
                button.SetSelected(true);
                break;
            }
        }

        // 다음 버튼 활성화
        nextButton.interactable = true;
        
        Debug.Log($"감정 선택됨: {emotion}");
    }

    private void OnIntensityChanged(float value)
    {
        selectedIntensity = Mathf.RoundToInt(value);
        intensityValueText.text = selectedIntensity.ToString();
        
        // 별 이미지 업데이트
        UpdateStarImages(selectedIntensity);
    }

    private void UpdateStarImages(int intensity)
    {
        if (starImages == null || starImages.Length == 0)
        {
            return;
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                if (i < intensity)
                {
                    starImages[i].color = Color.yellow;
                    // 애니메이션 효과
                    StartCoroutine(AnimateStar(starImages[i]));
                }
                else
                {
                    starImages[i].color = Color.gray;
                }
            }
        }
    }

    private IEnumerator AnimateStar(Image star)
    {
        Vector3 originalScale = star.transform.localScale;
        star.transform.localScale = originalScale * 1.3f;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            star.transform.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, progress);
            yield return null;
        }
        
        star.transform.localScale = originalScale;
    }

    private void SkipKeyword()
    {
        enteredKeyword = "";
        NextPanel();
    }

    private void ShowPanel(int index)
    {
        currentPanelIndex = index;

        // 모든 패널 비활성화
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        // 현재 패널 활성화
        if (index >= 0 && index < panels.Length && panels[index] != null)
        {
            panels[index].SetActive(true);
        }

        // 네비게이션 버튼 상태 업데이트
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        previousButton.interactable = currentPanelIndex > 0;
        
        if (currentPanelIndex == 1) // 감정 선택 패널
        {
            nextButton.interactable = !string.IsNullOrEmpty(selectedEmotion);
        }
        else if (currentPanelIndex == panels.Length - 1) // 마지막 패널
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.interactable = true;
            nextButton.gameObject.SetActive(true);
        }
    }

    private void NextPanel()
    {
        if (currentPanelIndex < panels.Length - 1)
        {
            // 키워드 패널에서 입력된 키워드 저장
            if (currentPanelIndex == 3) // 키워드 패널
            {
                enteredKeyword = keywordInput.text;
            }

            // 요약 패널에서 데이터 업데이트
            if (currentPanelIndex == panels.Length - 2) // 요약 패널 직전
            {
                UpdateSummaryPanel();
            }

            ShowPanel(currentPanelIndex + 1);
        }
    }

    private void PreviousPanel()
    {
        if (currentPanelIndex > 0)
        {
            ShowPanel(currentPanelIndex - 1);
        }
    }

    private void UpdateSummaryPanel()
    {
        if (summaryText != null)
        {
            string summary = $"감정 체크 요약\n\n";
            summary += $"감정: {selectedEmotion} {emotionEmojis[selectedEmotion]}\n";
            summary += $"강도: {selectedIntensity}/5 ";
            
            for (int i = 0; i < selectedIntensity; i++)
            {
                summary += "*";
            }
            
            summary += "\n";
            
            if (!string.IsNullOrEmpty(enteredKeyword))
            {
                summary += $"키워드: {enteredKeyword}\n";
            }
            
            summary += "\n이 정보가 맞나요?";
            
            // 안전한 텍스트 설정
            SetSafeText(summaryText, summary);
        }
    }

    private void ConfirmEmotionData()
    {
        if (currentEmotionData == null)
        {
            Debug.LogError("감정 데이터가 없습니다.");
            return;
        }

        if (isCheckInMode)
        {
            // 체크인 모드: 상담 전 감정 저장
            currentEmotionData = EmotionManager.Instance.CreateEmotionData(
                ClassCodeManager.GetCurrentClassCode(),
                UserManager.GetStudentID().ToString(),
                selectedEmotion,
                selectedIntensity
            );
            
            if (!string.IsNullOrEmpty(enteredKeyword))
            {
                currentEmotionData.keywords = enteredKeyword;
            }
            
            Debug.Log("감정 체크인 완료!");
            
            // 대기 화면으로 이동 (또는 상담 대기)
            SceneManager.LoadScene("SampleScene"); // 임시로 메인 상담 씬으로
        }
        else
        {
            // 체크아웃 모드: 상담 후 감정 업데이트
            if (currentEmotionData != null)
            {
                currentEmotionData.UpdateAfterEmotion(selectedEmotion, selectedIntensity, enteredKeyword, "");
            }
            EmotionManager.Instance.UpdateCurrentEmotionData(
                selectedEmotion,
                enteredKeyword,
                "" // 다짐은 별도 입력 필요
            );
            
            // 감정 데이터 제출
            EmotionManager.Instance.SubmitEmotionData();
            
            Debug.Log("감정 체크아웃 완료!");
            
            // 성장 그래프 씬으로 이동
            SceneManager.LoadScene("StudentGrowthScene");
        }
    }

    private void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    // 체크인/체크아웃 모드 설정
    public void SetCheckInMode(bool isCheckIn)
    {
        isCheckInMode = isCheckIn;
        SetupPanels();
    }

    // 애니메이션 효과들
    private IEnumerator AnimatePanel(GameObject panel, bool fadeIn)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    #region 복사 최적화된 동적 UI 생성 시스템
    
    /// <summary>
    /// Unity Play Mode에서 실행 → UI 생성 → 복사 → Edit Mode에서 붙여넣기 → 자동 연결
    /// 
    /// 사용법:
    /// 1. Play Mode 진입
    /// 2. Inspector에서 "Create UI for Copying" 실행
    /// 3. Hierarchy에서 생성된 UI 복사 (Ctrl+C)
    /// 4. Play Mode 종료  
    /// 5. Canvas에 붙여넣기 (Ctrl+V)
    /// 6. Inspector에서 "Auto Connect Copied UI" 실행
    /// </summary>
    /// <summary>
    /// 간단한 테스트용 UI를 생성합니다. (기존 UI가 없을 때 사용)
    /// </summary>
    /// <summary>
    /// 모든 텍스트 컴포넌트의 문제 문자들을 즉시 정리합니다.
    /// </summary>
    [ContextMenu("🧹 Clean All Text Components")]
    public void ForceCleanAllTextComponents()
    {
        CleanAllTextComponentsInScene();
        Debug.Log("🧹 모든 텍스트 컴포넌트 정리가 완료되었습니다!");
    }
    
    /// <summary>
    /// UI 설정 도우미 - 현재 상태를 확인하고 다음 단계를 안내합니다.
    /// </summary>
    [ContextMenu("❓ UI Setup Helper")]
    public void UISetupHelper()
    {
        Debug.Log("🔍 UI 설정 도우미");
        Debug.Log("===============================");
        
        // 현재 상태 확인
        bool hasUIComponents = ValidateRequiredComponents();
        
        if (hasUIComponents)
        {
            Debug.Log("✅ UI 컴포넌트가 모두 연결되어 있습니다!");
            Debug.Log("🎮 Play Mode에서 테스트하세요!");
        }
        else
        {
            Debug.Log("❌ UI 컴포넌트가 연결되지 않았습니다.");
            Debug.Log("");
            Debug.Log("🚀 빠른 해결 방법 (추천):");
            Debug.Log("1. Edit Mode에서 '🎮 Create Basic UI (Test)' 클릭");
            Debug.Log("2. 그 다음 '🔗 Auto Connect Copied UI' 클릭");
            Debug.Log("3. Play Mode에서 테스트!");
            Debug.Log("");
            Debug.Log("🎨 완전한 UI 생성 방법:");
            Debug.Log("1. Play Mode 진입");
            Debug.Log("2. '🎨 Create UI for Copying' 클릭");
            Debug.Log("3. Hierarchy에서 'StudentEmotionUI_Generated' 복사");
            Debug.Log("4. Play Mode 종료");
            Debug.Log("5. Canvas에 붙여넣기");
            Debug.Log("6. '🔗 Auto Connect Copied UI' 클릭");
        }
        
        Debug.Log("===============================");
    }
    
    [ContextMenu("🎮 Create Basic UI (Test)")]
    public void CreateBasicUIForTesting()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Edit Mode에서만 실행 가능합니다!");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다!");
            return;
        }

        Debug.Log("🎮 기본 테스트 UI 생성 중...");

        // 간단한 패널들만 생성
        CreateBasicPanel(canvas, "WelcomePanel_Copy", new Vector2(0, 0));
        CreateBasicPanel(canvas, "EmotionPanel_Copy", new Vector2(0, -100));
        CreateBasicPanel(canvas, "IntensityPanel_Copy", new Vector2(0, -200));
        CreateBasicPanel(canvas, "KeywordPanel_Copy", new Vector2(0, -300));
        CreateBasicPanel(canvas, "SummaryPanel_Copy", new Vector2(0, -400));

        // 기본 버튼들 생성
        CreateBasicButton(canvas.transform, "NextButton_Copy", new Vector2(100, -500));
        CreateBasicButton(canvas.transform, "PreviousButton_Copy", new Vector2(-100, -500));

        Debug.Log("✅ 기본 UI 생성 완료! 이제 '🔗 Auto Connect Copied UI'를 실행하세요!");
    }

    private void CreateBasicPanel(Canvas canvas, string name, Vector2 position)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 50);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        // 텍스트 추가
        GameObject textGO = new GameObject($"{name.Replace("_Copy", "")}Text_Copy");
        textGO.transform.SetParent(panel.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = name.Replace("Panel_Copy", "").Replace("_Copy", "");
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 14;
        ApplyKoreanFont(text);

        // 패널별 특수 요소들
        if (name.Contains("Welcome"))
        {
            CreateBasicButton(panel.transform, "StartButton_Copy", Vector2.zero);
            CreateBasicText(panel.transform, "StudentInfoText_Copy", new Vector2(0, -30));
        }
        else if (name.Contains("Emotion"))
        {
            CreateBasicContainer(panel.transform, "EmotionButtonContainer_Copy");
        }
        else if (name.Contains("Intensity"))
        {
            CreateBasicSlider(panel.transform, "IntensitySlider_Copy");
            CreateBasicText(panel.transform, "IntensityValueText_Copy", new Vector2(0, -30));
            CreateBasicContainer(panel.transform, "StarContainer_Copy");
        }
        else if (name.Contains("Keyword"))
        {
            CreateBasicInputField(panel.transform, "KeywordInput_Copy");
            CreateBasicButton(panel.transform, "SkipKeywordButton_Copy", new Vector2(0, -30));
        }
        else if (name.Contains("Summary"))
        {
            CreateBasicButton(panel.transform, "ConfirmButton_Copy", new Vector2(-50, -30));
            CreateBasicButton(panel.transform, "BackButton_Copy", new Vector2(50, -30));
        }
    }

    private void CreateBasicButton(Transform parent, string name, Vector2 position)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);
        
        RectTransform rect = button.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(80, 30);
        
        Image image = button.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        Button buttonComp = button.AddComponent<Button>();
        
        // 텍스트 추가
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(button.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = name.Replace("Button_Copy", "").Replace("_Copy", "");
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 12;
        text.color = Color.white;
        ApplyKoreanFont(text);
    }

    private void CreateBasicText(Transform parent, string name, Vector2 position)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        
        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(150, 20);
        
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = name.Replace("Text_Copy", "").Replace("_Copy", "");
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 10;
        ApplyKoreanFont(text);
    }

    private void CreateBasicContainer(Transform parent, string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent, false);
        
        RectTransform rect = container.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -25);
        rect.sizeDelta = new Vector2(150, 20);
    }

    private void CreateBasicSlider(Transform parent, string name)
    {
        GameObject slider = new GameObject(name);
        slider.transform.SetParent(parent, false);
        
        RectTransform rect = slider.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -25);
        rect.sizeDelta = new Vector2(100, 20);
        
        Slider sliderComp = slider.AddComponent<Slider>();
        sliderComp.minValue = 1;
        sliderComp.maxValue = 5;
        sliderComp.value = 3;
    }

    private void CreateBasicInputField(Transform parent, string name)
    {
        GameObject input = new GameObject(name);
        input.transform.SetParent(parent, false);
        
        RectTransform rect = input.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -25);
        rect.sizeDelta = new Vector2(120, 25);
        
        Image image = input.AddComponent<Image>();
        image.color = Color.white;
        
        TMP_InputField inputField = input.AddComponent<TMP_InputField>();
        
        // 텍스트 컴포넌트
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(input.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.fontSize = 10;
        ApplyKoreanFont(text);
        
        inputField.textComponent = text;
    }

    [ContextMenu("🎨 Create UI for Copying")]
    public void CreateUIForCopying()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("Play Mode에서만 실행 가능합니다!");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다!");
            return;
        }

        Debug.Log("🎨 복사 최적화된 UI 생성 시작!");
        
        // 기존 UI 정리
        ClearExistingCopyUI(canvas);
        
        // 복사 친화적 UI 생성
        CreateCopyOptimizedWelcomePanel(canvas);
        CreateCopyOptimizedEmotionPanel(canvas);
        CreateCopyOptimizedIntensityPanel(canvas);
        CreateCopyOptimizedKeywordPanel(canvas);
        CreateCopyOptimizedSummaryPanel(canvas);
        CreateCopyOptimizedNavigationButtons(canvas);
        
        // 생성된 UI에 태그와 메타데이터 추가
        AddCopyMetadata(canvas);
        
        Debug.Log("✅ UI 생성 완료! Hierarchy에서 'StudentEmotionUI_Generated'를 복사하세요!");
    }
    
    /// <summary>
    /// 복사된 UI를 자동으로 연결합니다.
    /// Edit Mode에서 붙여넣기 후 실행하세요.
    /// </summary>
    [ContextMenu("🔗 Auto Connect Copied UI")]
    public void AutoConnectCopiedUI()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Edit Mode에서만 실행 가능합니다!");
            return;
        }

        Debug.Log("🔗 복사된 UI 자동 연결 시작!");
        
        // 먼저 현재 Scene에 어떤 UI 요소들이 있는지 확인
        CheckAvailableUIElements();
        
        try
        {
            // 이름 기반으로 UI 요소 자동 찾기 및 연결
            ConnectUIByNames();
            
            // 연결 상태 검증
            if (ValidateRequiredComponents())
            {
                Debug.Log("✅ UI 자동 연결 성공!");
                Debug.Log("🎉 이제 Play Mode에서 테스트 가능합니다!");
            }
            else
            {
                Debug.LogWarning("⚠️ 일부 컴포넌트 연결 실패.");
                ShowConnectionGuidance();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ UI 연결 실패: {ex.Message}");
            ShowConnectionGuidance();
        }
    }
    
    private void CheckAvailableUIElements()
    {
        Debug.Log("📋 현재 Scene의 UI 요소들을 확인합니다:");
        
        // Canvas 찾기
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Canvas 수: {canvases.Length}");
        
        // GameObject들 찾기
        string[] searchNames = {
            "WelcomePanel_Copy", "EmotionPanel_Copy", "IntensityPanel_Copy", 
            "KeywordPanel_Copy", "SummaryPanel_Copy", "NextButton_Copy", "PreviousButton_Copy",
            "StudentEmotionUI_Generated"
        };
        
        foreach (string name in searchNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                Debug.Log($"✅ 찾음: {name}");
            }
            else
            {
                Debug.Log($"❌ 없음: {name}");
            }
        }
        
        // 모든 GameObject 이름 출력 (Copy가 포함된 것들만)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Debug.Log("Copy가 포함된 GameObject들:");
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("Copy"))
            {
                Debug.Log($"  - {obj.name} (부모: {(obj.transform.parent ? obj.transform.parent.name : "없음")})");
            }
        }
    }
    
    private void ShowConnectionGuidance()
    {
        Debug.Log("📋 UI 연결 가이드:");
        Debug.Log("1. Play Mode로 들어가서 '🎨 Create UI for Copying' 실행");
        Debug.Log("2. Hierarchy에서 'StudentEmotionUI_Generated' 복사 (Ctrl+C)");
        Debug.Log("3. Play Mode 종료");
        Debug.Log("4. Canvas에 붙여넣기 (Ctrl+V)");
        Debug.Log("5. 다시 '🔗 Auto Connect Copied UI' 실행");
        Debug.Log("");
        Debug.Log("또는 Inspector에서 각 UI 요소를 수동으로 드래그해서 연결하세요.");
    }
    
    private void ConnectUIByNames()
    {
        Debug.Log("=== UI 컴포넌트 연결 시작 ===");
        
        // 패널들 연결
        welcomePanel = FindAndConnect("WelcomePanel_Copy", () => GameObject.Find("WelcomePanel_Copy"));
        emotionPanel = FindAndConnect("EmotionPanel_Copy", () => GameObject.Find("EmotionPanel_Copy"));
        intensityPanel = FindAndConnect("IntensityPanel_Copy", () => GameObject.Find("IntensityPanel_Copy"));
        keywordPanel = FindAndConnect("KeywordPanel_Copy", () => GameObject.Find("KeywordPanel_Copy"));
        summaryPanel = FindAndConnect("SummaryPanel_Copy", () => GameObject.Find("SummaryPanel_Copy"));
        
        // Welcome Panel 요소들 - 더 유연한 검색
        welcomeText = FindTextComponent("WelcomeText_Copy", "WelcomePanelText_Copy", "Welcome");
        studentInfoText = FindTextComponent("StudentInfoText_Copy", "StudentInfo", "Info");
        startButton = FindButtonComponent("StartButton_Copy", "Start");
        
        // Emotion Panel 요소들
        emotionTitleText = FindTextComponent("EmotionTitleText_Copy", "EmotionTitle", "Emotion");
        emotionButtonContainer = FindTransformComponent("EmotionButtonContainer_Copy", "EmotionContainer", "ButtonContainer");
        
        // 감정 버튼들 자동 수집 (EmotionButton이 없어도 Button으로 대체)
        if (emotionButtonContainer != null)
        {
            List<EmotionButton> emotionButtonList = new List<EmotionButton>();
            for (int i = 0; i < emotionButtonContainer.childCount; i++)
            {
                Transform child = emotionButtonContainer.GetChild(i);
                EmotionButton btn = child.GetComponent<EmotionButton>();
                if (btn != null) 
                {
                    emotionButtonList.Add(btn);
                }
                else
                {
                    // EmotionButton이 없으면 기본 Button으로 대체
                    Button basicBtn = child.GetComponent<Button>();
                    if (basicBtn != null)
                    {
                        Debug.Log($"EmotionButton 대신 Button 사용: {child.name}");
                    }
                }
            }
            emotionButtons = emotionButtonList.ToArray();
            Debug.Log($"감정 버튼 연결됨: {emotionButtons.Length}개");
        }
        
        // Intensity Panel 요소들
        intensityTitleText = FindTextComponent("IntensityTitleText_Copy", "IntensityTitle", "Intensity");
        intensitySlider = FindSliderComponent("IntensitySlider_Copy", "Slider");
        intensityValueText = FindTextComponent("IntensityValueText_Copy", "IntensityValue", "Value");
        starContainer = FindTransformComponent("StarContainer_Copy", "StarContainer", "Star");
        
        // 별 이미지들 자동 수집
        if (starContainer != null)
        {
            List<Image> starImageList = new List<Image>();
            for (int i = 0; i < starContainer.childCount; i++)
            {
                Image img = starContainer.GetChild(i).GetComponent<Image>();
                if (img != null) starImageList.Add(img);
            }
            starImages = starImageList.ToArray();
            Debug.Log($"별 이미지 연결됨: {starImages.Length}개");
        }
        
        // Keyword Panel 요소들
        keywordTitleText = FindTextComponent("KeywordTitleText_Copy", "KeywordTitle", "Keyword");
        keywordInput = FindInputComponent("KeywordInput_Copy", "Input");
        skipKeywordButton = FindButtonComponent("SkipKeywordButton_Copy", "Skip");
        
        // Summary Panel 요소들
        summaryText = FindTextComponent("SummaryText_Copy", "Summary");
        confirmButton = FindButtonComponent("ConfirmButton_Copy", "Confirm");
        backButton = FindButtonComponent("BackButton_Copy", "Back");
        
        // Navigation 버튼들
        nextButton = FindButtonComponent("NextButton_Copy", "Next");
        previousButton = FindButtonComponent("PreviousButton_Copy", "Previous");
        
        int connectedCount = GetConnectedComponentCount();
        Debug.Log($"=== 연결 완료: {connectedCount}개 컴포넌트 ===");
        
        // 연결 실패한 항목들 상세 리포트
        ReportMissingComponents();
    }
    
    private T FindAndConnect<T>(string name, System.Func<T> finder) where T : class
    {
        T result = finder();
        Debug.Log($"{name}: {(result != null ? "✅" : "❌")}");
        return result;
    }
    
    private TextMeshProUGUI FindTextComponent(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                TextMeshProUGUI comp = found.GetComponent<TextMeshProUGUI>();
                if (comp != null)
                {
                    Debug.Log($"텍스트 컴포넌트 찾음: {name} ✅");
                    return comp;
                }
            }
        }
        Debug.Log($"텍스트 컴포넌트 못찾음: {string.Join(", ", possibleNames)} ❌");
        return null;
    }
    
    private Button FindButtonComponent(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                Button comp = found.GetComponent<Button>();
                if (comp != null)
                {
                    Debug.Log($"버튼 컴포넌트 찾음: {name} ✅");
                    return comp;
                }
            }
        }
        Debug.Log($"버튼 컴포넌트 못찾음: {string.Join(", ", possibleNames)} ❌");
        return null;
    }
    
    private Transform FindTransformComponent(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                Debug.Log($"Transform 컴포넌트 찾음: {name} ✅");
                return found.transform;
            }
        }
        Debug.Log($"Transform 컴포넌트 못찾음: {string.Join(", ", possibleNames)} ❌");
        return null;
    }
    
    private Slider FindSliderComponent(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                Slider comp = found.GetComponent<Slider>();
                if (comp != null)
                {
                    Debug.Log($"슬라이더 컴포넌트 찾음: {name} ✅");
                    return comp;
                }
            }
        }
        Debug.Log($"슬라이더 컴포넌트 못찾음: {string.Join(", ", possibleNames)} ❌");
        return null;
    }
    
    private TMP_InputField FindInputComponent(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                TMP_InputField comp = found.GetComponent<TMP_InputField>();
                if (comp != null)
                {
                    Debug.Log($"입력 필드 컴포넌트 찾음: {name} ✅");
                    return comp;
                }
            }
        }
        Debug.Log($"입력 필드 컴포넌트 못찾음: {string.Join(", ", possibleNames)} ❌");
        return null;
    }
    
    private void ReportMissingComponents()
    {
        Debug.Log("=== 누락된 컴포넌트 리포트 ===");
        
        if (welcomePanel == null) Debug.Log("❌ WelcomePanel");
        if (emotionPanel == null) Debug.Log("❌ EmotionPanel");
        if (intensityPanel == null) Debug.Log("❌ IntensityPanel");
        if (keywordPanel == null) Debug.Log("❌ KeywordPanel");
        if (summaryPanel == null) Debug.Log("❌ SummaryPanel");
        
        if (welcomeText == null) Debug.Log("❌ WelcomeText");
        if (studentInfoText == null) Debug.Log("❌ StudentInfoText");
        if (startButton == null) Debug.Log("❌ StartButton");
        if (emotionTitleText == null) Debug.Log("❌ EmotionTitleText");
        if (intensityTitleText == null) Debug.Log("❌ IntensityTitleText");
        if (intensityValueText == null) Debug.Log("❌ IntensityValueText");
        if (keywordTitleText == null) Debug.Log("❌ KeywordTitleText");
        if (summaryText == null) Debug.Log("❌ SummaryText");
        
        if (intensitySlider == null) Debug.Log("❌ IntensitySlider");
        if (keywordInput == null) Debug.Log("❌ KeywordInput");
        
        if (nextButton == null) Debug.Log("❌ NextButton");
        if (previousButton == null) Debug.Log("❌ PreviousButton");
        if (confirmButton == null) Debug.Log("❌ ConfirmButton");
        if (backButton == null) Debug.Log("❌ BackButton");
        if (skipKeywordButton == null) Debug.Log("❌ SkipKeywordButton");
        
        if (emotionButtonContainer == null) Debug.Log("❌ EmotionButtonContainer");
        if (starContainer == null) Debug.Log("❌ StarContainer");
        
        Debug.Log("================================");
    }
    
    private T FindComponentByPath<T>(string path) where T : Component
    {
        GameObject go = GameObject.Find(path);
        return go?.GetComponent<T>();
    }
    
    private int GetConnectedComponentCount()
    {
        int count = 0;
        if (welcomePanel != null) count++;
        if (emotionPanel != null) count++;
        if (intensityPanel != null) count++;
        if (keywordPanel != null) count++;
        if (summaryPanel != null) count++;
        if (welcomeText != null) count++;
        if (studentInfoText != null) count++;
        if (startButton != null) count++;
        if (emotionTitleText != null) count++;
        if (emotionButtonContainer != null) count++;
        if (emotionButtons != null) count += emotionButtons.Length;
        if (intensityTitleText != null) count++;
        if (intensitySlider != null) count++;
        if (intensityValueText != null) count++;
        if (starContainer != null) count++;
        if (starImages != null) count += starImages.Length;
        if (keywordTitleText != null) count++;
        if (keywordInput != null) count++;
        if (skipKeywordButton != null) count++;
        if (summaryText != null) count++;
        if (confirmButton != null) count++;
        if (backButton != null) count++;
        if (nextButton != null) count++;
        if (previousButton != null) count++;
        return count;
    }
    
    private void ClearExistingCopyUI(Canvas canvas)
    {
        // 기존 생성된 UI 정리 (Copy 태그가 있는 것들만)
        Transform existingUI = canvas.transform.Find("StudentEmotionUI_Generated");
        if (existingUI != null)
        {
            DestroyImmediate(existingUI.gameObject);
        }
    }
    
    private void AddCopyMetadata(Canvas canvas)
    {
        // 생성된 UI들을 하나의 부모 오브젝트로 묶기
        GameObject uiRoot = new GameObject("StudentEmotionUI_Generated");
        uiRoot.transform.SetParent(canvas.transform, false);
        
        // 모든 복사용 UI들을 루트 하위로 이동
        MoveToRoot(uiRoot.transform, "WelcomePanel_Copy");
        MoveToRoot(uiRoot.transform, "EmotionPanel_Copy");
        MoveToRoot(uiRoot.transform, "IntensityPanel_Copy");
        MoveToRoot(uiRoot.transform, "KeywordPanel_Copy");
        MoveToRoot(uiRoot.transform, "SummaryPanel_Copy");
        MoveToRoot(uiRoot.transform, "NextButton_Copy");
        MoveToRoot(uiRoot.transform, "PreviousButton_Copy");
        
        // 메타데이터 컴포넌트 추가
        UIMetadata metadata = uiRoot.AddComponent<UIMetadata>();
        metadata.creationTime = System.DateTime.Now.ToString();
        metadata.version = "1.0";
        metadata.description = "복사 최적화된 StudentEmotionUI";
    }
    
    private void MoveToRoot(Transform root, string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            obj.transform.SetParent(root, false);
        }
    }
    
    // 복사 최적화된 UI 생성 메서드들
    private void CreateCopyOptimizedWelcomePanel(Canvas canvas)
    {
        GameObject panel = CreateCopyUIGameObject("WelcomePanel_Copy", canvas.transform);
        SetupPanelBackground(panel, new Color(0.9f, 0.95f, 1f, 0.8f));
        
        // Welcome Text
        GameObject welcomeTextGO = CreateCopyUIGameObject("WelcomeText_Copy", panel.transform);
        TextMeshProUGUI welcomeTextComp = welcomeTextGO.AddComponent<TextMeshProUGUI>();
        SetupText(welcomeTextComp, "감정 체크인", 48, FontStyles.Bold, TextAlignmentOptions.Center);
        SetupRectTransform(welcomeTextGO, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(600, 100));
        ApplyKoreanFont(welcomeTextComp);
        
        // Student Info Text
        GameObject studentInfoGO = CreateCopyUIGameObject("StudentInfoText_Copy", panel.transform);
        TextMeshProUGUI studentInfoComp = studentInfoGO.AddComponent<TextMeshProUGUI>();
        SetupText(studentInfoComp, "안녕하세요! 오늘의 기분을 알려주세요.", 24, FontStyles.Normal, TextAlignmentOptions.Center);
        SetupRectTransform(studentInfoGO, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(500, 80));
        ApplyKoreanFont(studentInfoComp);
        
        // Start Button
        GameObject startButtonGO = CreateCopyUIGameObject("StartButton_Copy", panel.transform);
        SetupButton(startButtonGO, "시작하기", new Color(0.2f, 0.6f, 1f, 1f));
        SetupRectTransform(startButtonGO, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(300, 80));
    }
    
    private void CreateCopyOptimizedEmotionPanel(Canvas canvas)
    {
        GameObject panel = CreateCopyUIGameObject("EmotionPanel_Copy", canvas.transform);
        SetupPanelBackground(panel, new Color(1f, 0.95f, 0.9f, 0.8f));
        
        // Title
        GameObject titleGO = CreateCopyUIGameObject("EmotionTitleText_Copy", panel.transform);
        TextMeshProUGUI titleComp = titleGO.AddComponent<TextMeshProUGUI>();
        SetupText(titleComp, "지금 기분이 어떤가요?", 36, FontStyles.Bold, TextAlignmentOptions.Center);
        SetupRectTransform(titleGO, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(600, 80));
        ApplyKoreanFont(titleComp);
        
        // Button Container
        GameObject containerGO = CreateCopyUIGameObject("EmotionButtonContainer_Copy", panel.transform);
        SetupRectTransform(containerGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, 300));
        
        GridLayoutGroup gridLayout = containerGO.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(180, 120);
        gridLayout.spacing = new Vector2(20, 20);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        
        // 감정 버튼들 생성
        string[] emotions = { "기쁨", "슬픔", "화남", "불안", "신남", "복잡" };
        string[] emojis = { "^_^", "T_T", ">_<", "@_@", "o_o", "?_?" };
        
        for (int i = 0; i < emotions.Length; i++)
        {
            GameObject buttonGO = CreateCopyUIGameObject($"EmotionButton_{emotions[i]}_Copy", containerGO.transform);
            SetupEmotionButton(buttonGO, emotions[i], emojis[i], emotionColors[emotions[i]]);
        }
    }
    
    private void CreateCopyOptimizedIntensityPanel(Canvas canvas)
    {
        GameObject panel = CreateCopyUIGameObject("IntensityPanel_Copy", canvas.transform);
        SetupPanelBackground(panel, new Color(0.95f, 1f, 0.9f, 0.8f));
        
        // Title
        GameObject titleGO = CreateCopyUIGameObject("IntensityTitleText_Copy", panel.transform);
        TextMeshProUGUI titleComp = titleGO.AddComponent<TextMeshProUGUI>();
        SetupText(titleComp, "얼마나 강하게 느끼나요?", 36, FontStyles.Bold, TextAlignmentOptions.Center);
        SetupRectTransform(titleGO, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(600, 80));
        ApplyKoreanFont(titleComp);
        
        // Slider
        GameObject sliderGO = CreateCopyUIGameObject("IntensitySlider_Copy", panel.transform);
        Slider slider = sliderGO.AddComponent<Slider>();
        SetupSlider(slider);
        SetupRectTransform(sliderGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400, 40));
        
        // Value Text
        GameObject valueTextGO = CreateCopyUIGameObject("IntensityValueText_Copy", panel.transform);
        TextMeshProUGUI valueTextComp = valueTextGO.AddComponent<TextMeshProUGUI>();
        SetupText(valueTextComp, "3/5", 32, FontStyles.Bold, TextAlignmentOptions.Center);
        SetupRectTransform(valueTextGO, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(200, 60));
        ApplyKoreanFont(valueTextComp);
        
        // Star Container
        GameObject starContainerGO = CreateCopyUIGameObject("StarContainer_Copy", panel.transform);
        SetupRectTransform(starContainerGO, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(300, 50));
        
        HorizontalLayoutGroup starLayout = starContainerGO.AddComponent<HorizontalLayoutGroup>();
        starLayout.spacing = 10;
        starLayout.childAlignment = TextAnchor.MiddleCenter;
        
        // 별 이미지들
        for (int i = 0; i < 5; i++)
        {
            GameObject starGO = CreateCopyUIGameObject($"Star_{i + 1}_Copy", starContainerGO.transform);
            Image starImage = starGO.AddComponent<Image>();
            starImage.color = i < 3 ? Color.yellow : Color.gray;
            SetupRectTransform(starGO, Vector2.zero, Vector2.zero, new Vector2(40, 40));
        }
    }
    
    private void CreateCopyOptimizedKeywordPanel(Canvas canvas)
    {
        GameObject panel = CreateCopyUIGameObject("KeywordPanel_Copy", canvas.transform);
        SetupPanelBackground(panel, new Color(1f, 0.9f, 1f, 0.8f));
        
        // Title
        GameObject titleGO = CreateCopyUIGameObject("KeywordTitleText_Copy", panel.transform);
        TextMeshProUGUI titleComp = titleGO.AddComponent<TextMeshProUGUI>();
        SetupText(titleComp, "한 단어로 표현하면?", 36, FontStyles.Bold, TextAlignmentOptions.Center);
        SetupRectTransform(titleGO, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(600, 80));
        ApplyKoreanFont(titleComp);
        
        // Input Field
        GameObject inputGO = CreateCopyUIGameObject("KeywordInput_Copy", panel.transform);
        SetupInputField(inputGO, "예: 피곤함, 설렘, 걱정 등");
        SetupRectTransform(inputGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400, 60));
        
        // Skip Button
        GameObject skipButtonGO = CreateCopyUIGameObject("SkipKeywordButton_Copy", panel.transform);
        SetupButton(skipButtonGO, "건너뛰기", new Color(0.7f, 0.7f, 0.7f, 1f));
        SetupRectTransform(skipButtonGO, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(200, 50));
    }
    
    private void CreateCopyOptimizedSummaryPanel(Canvas canvas)
    {
        GameObject panel = CreateCopyUIGameObject("SummaryPanel_Copy", canvas.transform);
        SetupPanelBackground(panel, new Color(0.9f, 1f, 0.95f, 0.8f));
        
        // Summary Text
        GameObject summaryTextGO = CreateCopyUIGameObject("SummaryText_Copy", panel.transform);
        TextMeshProUGUI summaryTextComp = summaryTextGO.AddComponent<TextMeshProUGUI>();
        SetupText(summaryTextComp, "감정 체크 요약", 24, FontStyles.Normal, TextAlignmentOptions.Center);
        SetupRectTransform(summaryTextGO, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(600, 300));
        ApplyKoreanFont(summaryTextComp);
        
        // Confirm Button
        GameObject confirmButtonGO = CreateCopyUIGameObject("ConfirmButton_Copy", panel.transform);
        SetupButton(confirmButtonGO, "확인", new Color(0.2f, 0.8f, 0.2f, 1f));
        SetupRectTransform(confirmButtonGO, new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), new Vector2(250, 60));
        
        // Back Button
        GameObject backButtonGO = CreateCopyUIGameObject("BackButton_Copy", panel.transform);
        SetupButton(backButtonGO, "메인으로", new Color(0.6f, 0.6f, 0.6f, 1f));
        SetupRectTransform(backButtonGO, new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(200, 50));
    }
    
    private void CreateCopyOptimizedNavigationButtons(Canvas canvas)
    {
        // Next Button
        GameObject nextButtonGO = CreateCopyUIGameObject("NextButton_Copy", canvas.transform);
        SetupButton(nextButtonGO, "다음", new Color(0.2f, 0.6f, 1f, 1f));
        SetupRectTransform(nextButtonGO, new Vector2(0.8f, 0.1f), new Vector2(0.8f, 0.1f), new Vector2(120, 50));
        
        // Previous Button
        GameObject previousButtonGO = CreateCopyUIGameObject("PreviousButton_Copy", canvas.transform);
        SetupButton(previousButtonGO, "이전", new Color(0.6f, 0.6f, 0.6f, 1f));
        SetupRectTransform(previousButtonGO, new Vector2(0.2f, 0.1f), new Vector2(0.2f, 0.1f), new Vector2(120, 50));
    }
    
    // 유틸리티 메서드들
    private GameObject CreateCopyUIGameObject(string name, Transform parent = null)
    {
        GameObject go = new GameObject(name);
        RectTransform rect = go.AddComponent<RectTransform>();
        
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }
        
        return go;
    }
    
    private void SetupPanelBackground(GameObject panel, Color color)
    {
        Image image = panel.AddComponent<Image>();
        image.color = color;
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        // 기본적으로 비활성화
        panel.SetActive(false);
    }
    
    private void SetupRectTransform(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = Vector2.zero;
    }
    
    private void SetupText(TextMeshProUGUI text, string content, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = new Color(0.2f, 0.4f, 0.8f, 1f);
    }
    
    private void SetupButton(GameObject buttonGO, string text, Color backgroundColor)
    {
        Image image = buttonGO.AddComponent<Image>();
        image.color = backgroundColor;
        
        Button button = buttonGO.AddComponent<Button>();
        
        // Button Text
        GameObject textGO = CreateCopyUIGameObject("Text", buttonGO.transform);
        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        SetupText(textComp, text, 20, FontStyles.Normal, TextAlignmentOptions.Center);
        textComp.color = Color.white;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        ApplyKoreanFont(textComp);
    }
    
    private void SetupEmotionButton(GameObject buttonGO, string emotion, string emoji, Color color)
    {
        Image image = buttonGO.AddComponent<Image>();
        image.color = color;
        
        Button button = buttonGO.AddComponent<Button>();
        
        // EmotionButton 컴포넌트가 있다면 추가, 없다면 기본 Button으로
        try
        {
            EmotionButton emotionButton = buttonGO.AddComponent<EmotionButton>();
        }
        catch
        {
            Debug.LogWarning($"EmotionButton 컴포넌트를 찾을 수 없습니다. 기본 Button을 사용합니다: {emotion}");
        }
        
        // Button Text
        GameObject textGO = CreateCopyUIGameObject("Text", buttonGO.transform);
        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        SetupText(textComp, $"{emoji}\n{emotion}", 24, FontStyles.Bold, TextAlignmentOptions.Center);
        textComp.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        ApplyKoreanFont(textComp);
    }
    
    private void SetupSlider(Slider slider)
    {
        slider.minValue = 1;
        slider.maxValue = 5;
        slider.wholeNumbers = true;
        slider.value = 3;
        
        // Slider Background
        GameObject bgGO = CreateCopyUIGameObject("Background", slider.transform);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        slider.targetGraphic = bgImage;
        
        // Slider Handle
        GameObject handleAreaGO = CreateCopyUIGameObject("Handle Slide Area", slider.transform);
        RectTransform handleAreaRect = handleAreaGO.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = Vector2.zero;
        handleAreaRect.anchoredPosition = Vector2.zero;
        
        GameObject handleGO = CreateCopyUIGameObject("Handle", handleAreaGO.transform);
        Image handleImage = handleGO.AddComponent<Image>();
        handleImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        RectTransform handleRect = handleGO.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(30, 30);
        
        slider.handleRect = handleRect;
    }
    
    private void SetupInputField(GameObject inputGO, string placeholder)
    {
        Image image = inputGO.AddComponent<Image>();
        image.color = Color.white;
        
        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();
        
        // Text Component
        GameObject textGO = CreateCopyUIGameObject("Text", inputGO.transform);
        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        inputField.textComponent = textComp;
        
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        
        textComp.fontSize = 24;
        textComp.color = Color.black;
        ApplyKoreanFont(textComp);
        
        // Placeholder
        GameObject placeholderGO = CreateCopyUIGameObject("Placeholder", inputGO.transform);
        TextMeshProUGUI placeholderComp = placeholderGO.AddComponent<TextMeshProUGUI>();
        inputField.placeholder = placeholderComp;
        
        RectTransform placeholderRect = placeholderGO.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.offsetMin = new Vector2(10, 5);
        placeholderRect.offsetMax = new Vector2(-10, -5);
        
        placeholderComp.text = placeholder;
        placeholderComp.fontSize = 20;
        placeholderComp.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        ApplyKoreanFont(placeholderComp);
    }
    
    /// <summary>
    /// [사용 중지됨] 모든 동적 UI 생성 메서드들이 제거되었습니다.
    /// Unity Inspector에서 UI를 직접 설정하세요.
    /// 
    /// 사용법:
    /// 1. Unity에서 StudentEmotionScene을 열고
    /// 2. Canvas 하위에 필요한 UI 요소들을 수동으로 생성
    /// 3. StudentEmotionUI 컴포넌트의 Inspector에서 각 UI 요소들을 연결
    /// 4. 프리팹으로 저장하여 재사용
    /// 
    /// 필요한 UI 구조:
    /// Canvas
    /// ├── WelcomePanel
    /// │   ├── WelcomeText (TextMeshProUGUI)
    /// │   ├── StudentInfoText (TextMeshProUGUI)
    /// │   └── StartButton (Button)
    /// ├── EmotionPanel
    /// │   ├── EmotionTitleText (TextMeshProUGUI)
    /// │   └── EmotionButtonContainer (Transform)
    /// │       ├── EmotionButton_Happy (EmotionButton)
    /// │       ├── EmotionButton_Sad (EmotionButton)
    /// │       ├── EmotionButton_Angry (EmotionButton)
    /// │       ├── EmotionButton_Anxious (EmotionButton)
    /// │       ├── EmotionButton_Excited (EmotionButton)
    /// │       └── EmotionButton_Confused (EmotionButton)
    /// ├── IntensityPanel
    /// │   ├── IntensityTitleText (TextMeshProUGUI)
    /// │   ├── IntensitySlider (Slider)
    /// │   ├── IntensityValueText (TextMeshProUGUI)
    /// │   └── StarContainer (Transform)
    /// │       ├── Star_1 (Image)
    /// │       ├── Star_2 (Image)
    /// │       ├── Star_3 (Image)
    /// │       ├── Star_4 (Image)
    /// │       └── Star_5 (Image)
    /// ├── KeywordPanel
    /// │   ├── KeywordTitleText (TextMeshProUGUI)
    /// │   ├── KeywordInput (TMP_InputField)
    /// │   └── SkipKeywordButton (Button)
    /// ├── SummaryPanel
    /// │   ├── SummaryText (TextMeshProUGUI)
    /// │   ├── ConfirmButton (Button)
    /// │   └── BackButton (Button)
    /// ├── NextButton (Button)
    /// └── PreviousButton (Button)
    /// </summary>
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private void CreateWelcomePanel(Canvas canvas) => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private void CreateEmotionPanel(Canvas canvas) => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private void CreateIntensityPanel(Canvas canvas) => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private void CreateKeywordPanel(Canvas canvas) => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private void CreateSummaryPanel(Canvas canvas) => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private void CreateNavigationButtons(Canvas canvas) => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private GameObject CreateUIGameObject(string name, Transform parent = null) => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private T GetOrAddComponent<T>(GameObject go) where T : Component => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private void ClearExistingUI(Canvas canvas) => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    
    [System.Obsolete("동적 UI 생성은 더 이상 사용되지 않습니다.", true)]
    private void SetupPanelArray() => throw new System.NotSupportedException("Unity Inspector에서 UI를 직접 설정하세요.");
    #endregion

    #region 한글 폰트 관련 메서드들 (유지됨)
    // 한글 폰트 적용 공통 메서드 (개선된 버전)
    private void ApplyKoreanFont(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        TMPro.TMP_FontAsset koreanFont = FindKoreanFont();
        
        if (koreanFont != null)
        {
            textComponent.font = koreanFont;
            Debug.Log($"한글 폰트 적용 성공: {textComponent.name}");
        }
        else
        {
            Debug.LogWarning($"한글 폰트를 찾을 수 없습니다. {textComponent.name}에 기본 폰트 사용");
            // 기본 TextMeshPro 폰트 사용 - null 체크 추가
            var defaultFont = Resources.GetBuiltinResource<TMPro.TMP_FontAsset>("LiberationSans SDF");
            if (defaultFont != null)
            {
                textComponent.font = defaultFont;
            }
            else
            {
                // TMP_Settings의 기본 폰트 사용
                if (TMPro.TMP_Settings.defaultFontAsset != null)
                {
                    textComponent.font = TMPro.TMP_Settings.defaultFontAsset;
                }
            }
        }
        
        // 부족한 문자가 있을 경우를 대비한 fallback 설정
        textComponent.enableAutoSizing = true;
        textComponent.fontSizeMin = 12;
        textComponent.fontSizeMax = textComponent.fontSize;
        
        // 텍스트 오버플로우 처리
        textComponent.overflowMode = TextOverflowModes.Ellipsis;
        
        // 문자 누락 방지를 위한 추가 설정
        textComponent.parseCtrlCharacters = false; // 제어 문자 파싱 비활성화
        textComponent.richText = false; // 리치 텍스트 비활성화로 안정성 향상
    }
    
    // 한글 폰트를 찾는 메서드
    private TMPro.TMP_FontAsset FindKoreanFont()
    {
        TMPro.TMP_FontAsset koreanFont = null;
        
        // 방법 1: Resources 폴더에서 로드
        string[] possiblePaths = {
            "Fonts & Materials/NotoSansKR-Bold SDF",
            "NotoSansKR-Bold SDF",
            "NotoSansKR-Bold",
            "Fonts/NotoSansKR-Bold SDF",
            "TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Bold SDF"
        };
        
        foreach (string path in possiblePaths)
        {
            koreanFont = Resources.Load<TMPro.TMP_FontAsset>(path);
            if (koreanFont != null)
            {
                Debug.Log($"한글 폰트 찾음: {path}");
                return koreanFont;
            }
        }
        
        // 방법 2: TMP_Settings에서 기본 폰트 사용
        if (TMPro.TMP_Settings.defaultFontAsset != null)
        {
            string fontName = TMPro.TMP_Settings.defaultFontAsset.name;
            if (fontName.Contains("Noto") || fontName.Contains("Korean") || fontName.Contains("한글"))
            {
                Debug.Log($"TMP_Settings에서 한글 폰트 찾음: {fontName}");
                return TMPro.TMP_Settings.defaultFontAsset;
            }
        }
        
        // 방법 3: Resources.FindObjectsOfTypeAll로 전체 검색
        TMPro.TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>();
        foreach (var font in allFonts)
        {
            if (font.name.Contains("NotoSans") || font.name.Contains("Korean") || font.name.Contains("한글"))
            {
                Debug.Log($"전체 검색에서 한글 폰트 찾음: {font.name}");
                return font;
            }
        }
        
        Debug.LogWarning("한글 폰트를 찾을 수 없습니다.");
        return null;
    }
    
    // 폰트 설정 검증 메서드
    private void VerifyKoreanFontSupport()
    {
        TMPro.TMP_FontAsset koreanFont = FindKoreanFont();
        if (koreanFont != null)
        {
            Debug.Log($"한글 폰트 사용 가능: {koreanFont.name}");
            
            // 한글 문자 지원 테스트
            char testChar = '지'; // '지' 문자
            if (koreanFont.HasCharacter(testChar))
            {
                Debug.Log("한글 문자 지원 확인됨");
            }
            else
            {
                Debug.LogWarning("폰트에서 한글 문자를 지원하지 않습니다.");
            }
        }
    }
    #endregion
}
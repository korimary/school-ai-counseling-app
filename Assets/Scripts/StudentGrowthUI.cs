using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StudentGrowthUI : MonoBehaviour
{
    // === 복사 최적화 동적 UI 생성 시스템 ===
    // StudentEmotionUI와 동일한 방식으로 복사-붙여넣기 최적화된 UI 생성
    [Header("🎨 UI 생성 도구 (Inspector 전용)")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private bool autoFindCanvas = true;
    [Header("UI 패널들")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject growthGraphPanel;
    [SerializeField] private GameObject achievementsPanel;
    [SerializeField] private GameObject summaryPanel;

    [Header("환영 패널")]
    [SerializeField] private TextMeshProUGUI welcomeText;
    [SerializeField] private TextMeshProUGUI studentInfoText;
    [SerializeField] private Button startButton;

    [Header("성장 그래프 패널")]
    [SerializeField] private Transform graphContainer;
    [SerializeField] private GameObject graphPointPrefab;
    [SerializeField] private LineRenderer graphLine;
    [SerializeField] private TextMeshProUGUI graphTitleText;
    [SerializeField] private Transform legendContainer;
    [SerializeField] private GameObject legendItemPrefab;

    [Header("월별 통계")]
    [SerializeField] private TextMeshProUGUI monthlyStatsText;
    [SerializeField] private Transform monthlyBarsContainer;
    [SerializeField] private GameObject monthlyBarPrefab;

    [Header("성취 패널")]
    [SerializeField] private Transform badgeContainer;
    [SerializeField] private GameObject badgeItemPrefab;
    [SerializeField] private TextMeshProUGUI achievementTitleText;
    [SerializeField] private TextMeshProUGUI totalBadgesText;

    [Header("요약 패널")]
    [SerializeField] private TextMeshProUGUI summaryTitleText;
    [SerializeField] private TextMeshProUGUI totalSessionsText;
    [SerializeField] private TextMeshProUGUI improvementText;
    [SerializeField] private TextMeshProUGUI favoriteEmotionText;
    [SerializeField] private TextMeshProUGUI encouragementText;

    [Header("네비게이션")]
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("색상 및 설정")]
    [SerializeField] private Color[] emotionColors = new Color[]
    {
        new Color(1f, 0.9f, 0.3f),    // 기쁨 - 노란색
        new Color(0.3f, 0.5f, 1f),    // 슬픔 - 파란색
        new Color(1f, 0.3f, 0.3f),    // 화남 - 빨간색
        new Color(0.8f, 0.3f, 1f),    // 불안 - 보라색
        new Color(1f, 0.5f, 0.3f),    // 신남 - 주황색
        new Color(0.5f, 0.5f, 0.5f)  // 복잡 - 회색
    };

    [SerializeField] private Color activeTabColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color inactiveTabColor = Color.white;

    // 데이터
    private List<EmotionData> studentEmotionData;
    private string currentClassCode;
    private int currentStudentNumber;
    private int currentPanelIndex = 0;
    private GameObject[] panels;

    // 감정별 색상 매핑
    private readonly Dictionary<string, Color> emotionColorMap = new Dictionary<string, Color>
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
        Debug.Log("🎬 StudentGrowthUI Start() 시작");
        
        // Canvas 설정 최적화 (화질 및 렌더링 문제 해결)
        OptimizeCanvasSettings();
        
        // UI 검증 및 초기화
        bool isValid = ValidateRequiredComponents();
        Debug.Log($"🔍 UI 컴포넌트 검증 결과: {isValid}");
        
        if (isValid)
        {
            Debug.Log("✅ UI 컴포넌트 검증 성공 - 초기화 진행");
            InitializeUI();
            LoadStudentData();
        }
        else
        {
            Debug.LogWarning("⚠️ StudentGrowthUI 컴포넌트가 연결되지 않았습니다.");
            Debug.Log("📋 해결 방법:");
            Debug.Log("1. Play Mode에서 Inspector → '🎨 Create UI for Copying' 실행");
            Debug.Log("2. 생성된 UI를 복사하여 Edit Mode에서 Canvas에 붙여넣기");
            Debug.Log("3. Inspector에서 _Copy 요소들을 해당 필드에 연결");
            
            // 검증 실패해도 기본 UI라도 표시하기 위해 강제 초기화
            Debug.Log("🚨 강제 초기화 시도");
            ForceShowWelcomePanel();
        }
    }

    private void InitializeUI()
    {
        Debug.Log("🎮 InitializeUI() 시작");
        
        // 패널 배열 설정
        panels = new GameObject[] { welcomePanel, growthGraphPanel, achievementsPanel, summaryPanel };
        Debug.Log($"📦 패널 배열 설정 완료 - 총 {panels.Length}개 패널");
        
        // 각 패널 상태 확인
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                Debug.Log($"패널 {i}: {panels[i].name} - 활성화: {panels[i].activeInHierarchy}");
            }
            else
            {
                Debug.LogError($"패널 {i}: NULL!");
            }
        }

        // 버튼 이벤트 설정
        if (startButton != null)
        {
            startButton.onClick.AddListener(() => ShowPanel(1));
            Debug.Log("✅ StartButton 이벤트 설정됨");
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
            Debug.Log("✅ BackButton 이벤트 설정됨");
        }
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextPanel);
            Debug.Log("✅ NextButton 이벤트 설정됨");
        }
        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousPanel);
            Debug.Log("✅ PreviousButton 이벤트 설정됨");
        }

        // 탭 버튼 이벤트 설정
        if (tabButtons != null && tabButtons.Length > 0)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                if (tabButtons[i] != null)
                {
                    tabButtons[i].onClick.AddListener(() => ShowPanel(index + 1)); // 0은 환영 패널이므로 +1
                    Debug.Log($"✅ TabButton {i} 이벤트 설정됨");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ TabButtons가 null이거나 비어있음");
        }

        // 첫 번째 패널 표시
        Debug.Log("🎬 첫 번째 패널(환영 패널) 표시 시도");
        ShowPanel(0);
    }

    private void LoadStudentData()
    {
        if (!UserManager.IsStudentMode())
        {
            Debug.LogError("학생 모드가 아닙니다.");
            return;
        }

        currentClassCode = ClassCodeManager.GetCurrentClassCode();
        currentStudentNumber = UserManager.GetStudentID();

        if (string.IsNullOrEmpty(currentClassCode) || currentStudentNumber <= 0)
        {
            Debug.LogError("학생 정보가 올바르지 않습니다.");
            return;
        }

        // 환영 메시지 설정
        string studentName = UserManager.GetStudentName();
        welcomeText.text = $"안녕하세요, {studentName}님!";
        studentInfoText.text = $"지금까지의 성장을 함께 살펴봐요!";

        // 구글 시트에서 학생 데이터 읽기
        GoogleSheetsManager.Instance.ReadStudentEmotionData(currentClassCode, currentStudentNumber, OnStudentDataLoaded);
    }

    private void OnStudentDataLoaded(List<EmotionData> emotionData)
    {
        studentEmotionData = emotionData ?? new List<EmotionData>();
        
        Debug.Log($"학생 감정 데이터 로드됨: {studentEmotionData.Count}개");

        if (studentEmotionData.Count == 0)
        {
            ShowNoDataMessage();
            return;
        }

        // 각 패널 업데이트
        UpdateGrowthGraphPanel();
        UpdateAchievementsPanel();
        UpdateSummaryPanel();
    }

    private void ShowPanel(int index)
    {
        Debug.Log($"🎬 ShowPanel({index}) 호출됨");
        
        if (panels == null)
        {
            Debug.LogError("❌ panels 배열이 null입니다!");
            return;
        }
        
        if (panels.Length == 0)
        {
            Debug.LogError("❌ panels 배열이 비어있습니다!");
            return;
        }
        
        currentPanelIndex = index;
        Debug.Log($"📊 현재 패널 인덱스: {currentPanelIndex}");

        // 모든 패널 비활성화
        Debug.Log("🔄 모든 패널 비활성화 중...");
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                bool wasActive = panels[i].activeInHierarchy;
                panels[i].SetActive(false);
                Debug.Log($"패널 {i} ({panels[i].name}): {wasActive} → false");
            }
            else
            {
                Debug.LogWarning($"⚠️ 패널 {i}이 null입니다!");
            }
        }

        // 선택된 패널 활성화
        if (index >= 0 && index < panels.Length && panels[index] != null)
        {
            panels[index].SetActive(true);
            Debug.Log($"✅ 패널 {index} ({panels[index].name}) 활성화됨");
            
            // 패널이 실제로 활성화되었는지 확인
            bool isReallyActive = panels[index].activeInHierarchy;
            Debug.Log($"🔍 패널 {index} 실제 활성화 상태: {isReallyActive}");
            
            // 부모 오브젝트들도 활성화되어 있는지 확인 및 Canvas 자동 수정
            Debug.Log("🔍 부모 계층 구조 분석:");
            Transform parent = panels[index].transform.parent;
            int level = 0;
            while (parent != null)
            {
                Debug.Log($"   레벨 {level}: '{parent.name}' (활성화: {parent.gameObject.activeInHierarchy})");
                parent = parent.parent;
                level++;
            }
            
            // 패널이 Canvas 밖에 있다면 자동으로 Canvas로 이동
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null && panels[index].GetComponentInParent<Canvas>() == null)
            {
                Debug.LogWarning($"⚠️ 패널 '{panels[index].name}'이 Canvas 밖에 있습니다. Canvas로 이동합니다.");
                panels[index].transform.SetParent(mainCanvas.transform, false);
                Debug.Log($"✅ 패널을 Canvas '{mainCanvas.name}'로 이동완료");
            }
            
            // Canvas 컴포넌트 확인
            Canvas canvas = panels[index].GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"🖼️ Canvas '{canvas.name}' 발견:");
                Debug.Log($"   - Canvas 활성화: {canvas.gameObject.activeInHierarchy}");
                Debug.Log($"   - Canvas 렌더 모드: {canvas.renderMode}");
                Debug.Log($"   - Canvas enabled: {canvas.enabled}");
                
                // Camera 확인 (Screen Space Camera 모드인 경우)
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    Camera cam = canvas.worldCamera;
                    if (cam != null)
                    {
                        Debug.Log($"   - Camera '{cam.name}' 활성화: {cam.gameObject.activeInHierarchy}");
                        Debug.Log($"   - Camera enabled: {cam.enabled}");
                    }
                    else
                    {
                        Debug.LogError("   - ❌ Canvas가 Screen Space Camera 모드인데 Camera가 null!");
                    }
                }
            }
            else
            {
                Debug.LogError("❌ 패널의 부모에서 Canvas를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError($"❌ 패널 {index} 활성화 실패! (범위: 0-{panels.Length-1}, null체크: {(index >= 0 && index < panels.Length ? panels[index] != null : "범위초과")})");
        }

        // 탭 버튼 색상 업데이트
        UpdateTabButtons();

        // 네비게이션 버튼 상태 업데이트
        UpdateNavigationButtons();
        
        Debug.Log($"🎬 ShowPanel({index}) 완료");
    }

    private void UpdateTabButtons()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] != null)
            {
                Image buttonImage = tabButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    bool isActive = (currentPanelIndex - 1) == i; // 환영 패널 제외
                    buttonImage.color = isActive ? activeTabColor : inactiveTabColor;
                }
            }
        }
    }

    private void UpdateNavigationButtons()
    {
        previousButton.interactable = currentPanelIndex > 0;
        nextButton.interactable = currentPanelIndex < panels.Length - 1;
    }

    private void UpdateGrowthGraphPanel()
    {
        if (studentEmotionData == null || studentEmotionData.Count == 0)
        {
            return;
        }

        // 그래프 제목 설정
        graphTitleText.text = "나의 감정 변화 그래프";

        // 시간순으로 정렬
        var sortedData = studentEmotionData.OrderBy(d => d.timestamp).ToList();
        
        // 완료된 세션만 표시
        var completeData = sortedData.Where(d => d.IsComplete()).ToList();

        if (completeData.Count == 0)
        {
            monthlyStatsText.text = "아직 완료된 상담이 없어요. 더 참여해보세요!"
            return;
        }

        // 그래프 포인트 생성
        CreateGraphPoints(completeData);

        // 월별 통계 생성
        CreateMonthlyStats(completeData);

        // 범례 생성
        CreateLegend();
    }

    private void CreateGraphPoints(List<EmotionData> data)
    {
        // 기존 포인트 제거
        foreach (Transform child in graphContainer)
        {
            Destroy(child.gameObject);
        }

        if (data.Count == 0) return;

        List<Vector3> linePoints = new List<Vector3>();
        float containerWidth = graphContainer.GetComponent<RectTransform>().rect.width;
        float containerHeight = graphContainer.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < data.Count; i++)
        {
            EmotionData emotionData = data[i];
            
            // X 위치 (시간 순서)
            float xPos = (float)i / (data.Count - 1) * containerWidth;
            
            // Y 위치 (감정 개선도: -4 ~ +4 범위를 0 ~ containerHeight로 매핑)
            float improvement = emotionData.GetEmotionChange();
            float yPos = ((improvement + 4f) / 8f) * containerHeight;

            // 포인트 생성
            GameObject point = Instantiate(graphPointPrefab, graphContainer);
            RectTransform pointRect = point.GetComponent<RectTransform>();
            pointRect.anchoredPosition = new Vector2(xPos, yPos);

            // 포인트 색상 설정
            Image pointImage = point.GetComponent<Image>();
            if (pointImage != null && emotionColorMap.ContainsKey(emotionData.beforeEmotion))
            {
                pointImage.color = emotionColorMap[emotionData.beforeEmotion];
            }

            // 툴팁 설정
            GraphPoint graphPoint = point.GetComponent<GraphPoint>();
            if (graphPoint != null)
            {
                graphPoint.Setup(emotionData);
            }

            // 라인 포인트 추가
            linePoints.Add(new Vector3(xPos, yPos, 0));
        }

        // 라인 렌더러 설정
        if (graphLine != null && linePoints.Count > 1)
        {
            graphLine.positionCount = linePoints.Count;
            graphLine.SetPositions(linePoints.ToArray());
        }
    }

    private void CreateMonthlyStats(List<EmotionData> data)
    {
        // 기존 막대 제거
        foreach (Transform child in monthlyBarsContainer)
        {
            Destroy(child.gameObject);
        }

        // 월별 그룹화
        var monthlyGroups = data.GroupBy(d => d.timestamp.Substring(0, 7)) // "2024-01" 형식
                               .OrderBy(g => g.Key)
                               .ToList();

        if (monthlyGroups.Count == 0)
        {
            monthlyStatsText.text = "월별 통계가 없습니다.";
            return;
        }

        float maxImprovement = 0f;
        Dictionary<string, float> monthlyAverages = new Dictionary<string, float>();

        // 월별 평균 개선도 계산
        foreach (var group in monthlyGroups)
        {
            float avgImprovement = (float)group.Average(d => d.GetEmotionChange());
            monthlyAverages[group.Key] = avgImprovement;
            maxImprovement = Mathf.Max(maxImprovement, Mathf.Abs(avgImprovement));
        }

        // 막대 그래프 생성
        foreach (var monthAvg in monthlyAverages)
        {
            GameObject bar = Instantiate(monthlyBarPrefab, monthlyBarsContainer);
            MonthlyBar barComponent = bar.GetComponent<MonthlyBar>();
            
            if (barComponent != null)
            {
                float normalizedHeight = maxImprovement > 0 ? Mathf.Abs(monthAvg.Value) / maxImprovement : 0f;
                Color barColor = monthAvg.Value >= 0 ? Color.green : Color.red;
                barComponent.Setup(monthAvg.Key, monthAvg.Value, normalizedHeight, barColor);
            }
        }

        // 통계 텍스트 업데이트
        float totalImprovement = monthlyAverages.Values.Average();
        string improvementText = totalImprovement > 0 ? "개선되고 있어요!" : "조금 더 노력해봐요!";
        monthlyStatsText.text = $"월평균 성장도: {totalImprovement:F1} - {improvementText}";
    }

    private void CreateLegend()
    {
        // 기존 범례 제거
        foreach (Transform child in legendContainer)
        {
            Destroy(child.gameObject);
        }

        // 감정별 범례 생성
        foreach (var emotionColor in emotionColorMap)
        {
            GameObject legendItem = Instantiate(legendItemPrefab, legendContainer);
            LegendItem legendComponent = legendItem.GetComponent<LegendItem>();
            
            if (legendComponent != null)
            {
                legendComponent.Setup(emotionColor.Key, emotionColor.Value);
            }
        }
    }

    private void UpdateAchievementsPanel()
    {
        achievementTitleText.text = "나의 성장 배지";

        // 배지 시스템 데이터 가져오기
        var badges = BadgeSystem.Instance.GetStudentBadges(currentStudentNumber);
        
        // 기존 배지 아이템 제거
        foreach (Transform child in badgeContainer)
        {
            Destroy(child.gameObject);
        }

        int earnedBadges = 0;
        foreach (var badge in badges)
        {
            GameObject badgeItem = Instantiate(badgeItemPrefab, badgeContainer);
            BadgeItem badgeComponent = badgeItem.GetComponent<BadgeItem>();
            
            if (badgeComponent != null)
            {
                badgeComponent.Setup(badge);
                if (badge.isEarned) earnedBadges++;
            }
        }

        totalBadgesText.text = $"획득한 배지: {earnedBadges}/{badges.Count}개";
    }

    private void UpdateSummaryPanel()
    {
        if (studentEmotionData == null || studentEmotionData.Count == 0)
        {
            return;
        }

        summaryTitleText.text = "나의 성장 요약";

        // 총 세션 수
        int totalSessions = studentEmotionData.Count;
        int completeSessions = studentEmotionData.Count(d => d.IsComplete());
        totalSessionsText.text = $"참여한 상담: {completeSessions}/{totalSessions}회";

        // 전체 개선도
        if (completeSessions > 0)
        {
            float avgImprovement = studentEmotionData.Where(d => d.IsComplete())
                                                    .Average(d => d.GetEmotionChange());
            
            if (avgImprovement > 0)
            {
                improvementText.text = $"평균 성장도: +{avgImprovement:F1}";
                improvementText.color = Color.green;
            }
            else if (avgImprovement < 0)
            {
                improvementText.text = $"평균 성장도: {avgImprovement:F1}";
                improvementText.color = Color.red;
            }
            else
            {
                improvementText.text = "평균 성장도: 0.0 ➡️";
                improvementText.color = Color.gray;
            }
        }

        // 가장 많이 나타난 감정
        var emotionCounts = studentEmotionData.GroupBy(d => d.beforeEmotion)
                                             .OrderByDescending(g => g.Count())
                                             .FirstOrDefault();
        
        if (emotionCounts != null)
        {
            favoriteEmotionText.text = $"가장 많은 감정: {emotionCounts.Key} ({emotionCounts.Count()}회)";
        }

        // 격려 메시지
        SetEncouragementMessage(completeSessions, studentEmotionData.Where(d => d.IsComplete()).Average(d => d.GetEmotionChange()));
    }

    private void SetEncouragementMessage(int sessions, float avgImprovement)
    {
        string[] positiveMessages = {
            "정말 잘하고 있어요!",
            "멋진 성장을 보여주고 있어요!",
            "계속 이렇게 노력해주세요!",
            "당신은 정말 대단해요!"
        };

        string[] encouragingMessages = {
            "조금씩 나아지고 있어요!",
            "포기하지 말고 계속해봐요!",
            "작은 변화도 큰 성장이에요!",
            "천천히 함께 성장해나가요! 🤗"
        };

        if (sessions >= 5 && avgImprovement > 0.5f)
        {
            encouragementText.text = positiveMessages[Random.Range(0, positiveMessages.Length)];
            encouragementText.color = Color.green;
        }
        else
        {
            encouragementText.text = encouragingMessages[Random.Range(0, encouragingMessages.Length)];
            encouragementText.color = new Color(0.2f, 0.7f, 1f);
        }
    }

    private void ShowNoDataMessage()
    {
        graphTitleText.text = "아직 상담 기록이 없어요";
        monthlyStatsText.text = "선생님과 상담을 시작해보세요!";
        achievementTitleText.text = "첫 상담을 완료하면 배지를 받을 수 있어요!";
        summaryTitleText.text = "상담에 참여해서 성장 기록을 만들어봐요!";
    }

    private void NextPanel()
    {
        if (currentPanelIndex < panels.Length - 1)
        {
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

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// 외부에서 특정 패널로 이동
    /// </summary>
    public void NavigateToPanel(int panelIndex)
    {
        ShowPanel(panelIndex);
    }
    
    // ===== 복사 최적화 동적 UI 생성 시스템 =====
    // StudentEmotionUI와 동일한 방식으로 구현
    
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
            }
            
            // Canvas 렌더링 모드 확인 및 수정
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // 카메라 모드에서 상하 반전 문제 해결
                Camera canvasCamera = canvas.worldCamera;
                if (canvasCamera != null)
                {
                    canvasCamera.orthographic = true;
                    canvasCamera.nearClipPlane = 0.1f;
                    canvasCamera.farClipPlane = 1000f;
                }
            }
            
            // TextMeshPro 컴포넌트 최적화
            TextMeshProUGUI[] tmpComponents = canvas.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI tmp in tmpComponents)
            {
                if (tmp != null)
                {
                    tmp.enableAutoSizing = false;
                    tmp.fontSize = Mathf.Max(tmp.fontSize, 14f);
                    tmp.fontSizeMin = 12f;
                    tmp.fontSizeMax = 72f;
                }
            }
        }
    }
    
    /// <summary>
    /// 필수 UI 컴포넌트들의 유효성을 검사합니다.
    /// </summary>
    private bool ValidateRequiredComponents()
    {
        bool isValid = true;
        string missingComponents = "";
        
        // 필수 패널들 검사
        if (welcomePanel == null) { isValid = false; missingComponents += "WelcomePanel, "; }
        if (growthGraphPanel == null) { isValid = false; missingComponents += "GrowthGraphPanel, "; }
        if (achievementsPanel == null) { isValid = false; missingComponents += "AchievementsPanel, "; }
        if (summaryPanel == null) { isValid = false; missingComponents += "SummaryPanel, "; }
        
        // 필수 버튼들 검사
        if (startButton == null) { isValid = false; missingComponents += "StartButton, "; }
        if (backButton == null) { isValid = false; missingComponents += "BackButton, "; }
        if (nextButton == null) { isValid = false; missingComponents += "NextButton, "; }
        if (previousButton == null) { isValid = false; missingComponents += "PreviousButton, "; }
        
        if (!isValid)
        {
            Debug.LogError($"❌ 누락된 컴포넌트들: {missingComponents}");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// 강제로 환영 패널을 표시합니다 (검증 실패 시 사용).
    /// </summary>
    private void ForceShowWelcomePanel()
    {
        // Canvas 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas를 찾을 수 없습니다!");
            return;
        }
        
        // 임시 환영 패널 생성
        GameObject tempPanel = new GameObject("TempWelcomePanel");
        tempPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = tempPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        UnityEngine.UI.Image bg = tempPanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.2f, 0.3f, 0.5f, 0.9f);
        
        // 임시 텍스트 추가
        GameObject tempText = new GameObject("TempText");
        tempText.transform.SetParent(tempPanel.transform, false);
        
        RectTransform textRect = tempText.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(600, 200);
        
        TextMeshProUGUI text = tempText.AddComponent<TextMeshProUGUI>();
        text.text = "StudentGrowthUI 임시 화면\n\nUI 컴포넌트가 연결되지 않았습니다.\n\n해결 방법:\n1. Inspector에서 UI 요소들을 연결하거나\n2. 'Create UI for Copying' 기능을 사용하세요.";
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        
        Debug.Log("🚨 임시 환영 패널 생성됨");
    }
    
    /// <summary>
    /// 복사-붙여넣기에 최적화된 UI를 동적으로 생성합니다.
    /// Play Mode에서 실행 후 생성된 UI를 복사하여 Edit Mode에서 붙여넣기하세요.
    /// </summary>
    [UnityEngine.ContextMenu("🎨 Create UI for Copying")]
    public void CreateUIForCopying()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("❌ Play Mode에서만 실행 가능합니다!");
            return;
        }
        
        Debug.Log("🎨 StudentGrowthUI 복사 최적화 UI 생성 시작...");
        
        // Canvas 찾기
        Canvas canvas = FindCanvas();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas를 찾을 수 없습니다!");
            return;
        }
        
        // 기존 _Copy UI 제거
        RemoveExistingCopyUI(canvas);
        
        // UI 생성
        CreateStudentGrowthUI(canvas);
        
        // 메타데이터 추가
        AddUIMetadata(canvas);
        
        Debug.Log("✅ StudentGrowthUI 복사용 UI 생성 완료!");
        Debug.Log("📋 다음 단계:");
        Debug.Log("1. Hierarchy에서 생성된 _Copy UI들을 선택하여 복사 (Ctrl+C)");
        Debug.Log("2. Play Mode 중지");
        Debug.Log("3. Edit Mode에서 Canvas에 붙여넣기 (Ctrl+V)");
        Debug.Log("4. Inspector에서 _Copy 요소들을 해당 필드에 연결");
    }
    
    /// <summary>
    /// Canvas를 찾습니다.
    /// </summary>
    private Canvas FindCanvas()
    {
        if (targetCanvas != null)
        {
            return targetCanvas;
        }
        
        if (autoFindCanvas)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return canvas;
                }
            }
            
            if (canvases.Length > 0)
            {
                return canvases[0];
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 기존 _Copy UI를 제거합니다.
    /// </summary>
    private void RemoveExistingCopyUI(Canvas canvas)
    {
        Transform[] children = canvas.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.name.EndsWith("_Copy"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
    
    /// <summary>
    /// StudentGrowthUI를 생성합니다.
    /// </summary>
    private void CreateStudentGrowthUI(Canvas canvas)
    {
        // 1. 환영 패널 생성
        CreateWelcomePanel(canvas.transform);
        
        // 2. 성장 그래프 패널 생성
        CreateGrowthGraphPanel(canvas.transform);
        
        // 3. 성취 패널 생성
        CreateAchievementsPanel(canvas.transform);
        
        // 4. 요약 패널 생성
        CreateSummaryPanel(canvas.transform);
        
        // 5. 네비게이션 버튼들 생성
        CreateNavigationButtons(canvas.transform);
    }
    
    /// <summary>
    /// 환영 패널을 생성합니다.
    /// </summary>
    private void CreateWelcomePanel(Transform parent)
    {
        GameObject panel = CreateUIGameObject("WelcomePanel_Copy", parent);
        panel.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        SetupRectTransform(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        
        // 환영 텍스트
        GameObject welcomeTextObj = CreateUIGameObject("WelcomeText_Copy", panel.transform);
        TextMeshProUGUI welcomeText = welcomeTextObj.AddComponent<TextMeshProUGUI>();
        welcomeText.text = "나의 성장 기록";
        welcomeText.fontSize = 48;
        welcomeText.alignment = TextAlignmentOptions.Center;
        welcomeText.color = Color.yellow;
        welcomeText.fontStyle = FontStyles.Bold;
        SetupRectTransform(welcomeTextObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(400, 60));
        
        // 학생 정보 텍스트
        GameObject studentInfoObj = CreateUIGameObject("StudentInfoText_Copy", panel.transform);
        TextMeshProUGUI studentInfo = studentInfoObj.AddComponent<TextMeshProUGUI>();
        studentInfo.text = "안녕하세요! 함께 성장해온 과정을 확인해보세요.";
        studentInfo.fontSize = 24;
        studentInfo.alignment = TextAlignmentOptions.Center;
        studentInfo.color = Color.white;
        SetupRectTransform(studentInfoObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 40));
        
        // 시작 버튼
        GameObject startButtonObj = CreateUIGameObject("StartButton_Copy", panel.transform);
        Button startBtn = startButtonObj.AddComponent<Button>();
        Image startBtnImage = startButtonObj.AddComponent<Image>();
        startBtnImage.color = new Color(0.2f, 0.8f, 0.4f);
        SetupRectTransform(startButtonObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(200, 50));
        
        // 시작 버튼 텍스트
        GameObject startTextObj = CreateUIGameObject("Text_Copy", startButtonObj.transform);
        TextMeshProUGUI startText = startTextObj.AddComponent<TextMeshProUGUI>();
        startText.text = "시작하기";
        startText.fontSize = 16;
        startText.alignment = TextAlignmentOptions.Center;
        startText.color = Color.white;
        SetupRectTransform(startTextObj.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }
    
    /// <summary>
    /// 성장 그래프 패널을 생성합니다.
    /// </summary>
    private void CreateGrowthGraphPanel(Transform parent)
    {
        GameObject panel = CreateUIGameObject("GrowthGraphPanel_Copy", parent);
        panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 0.9f);
        SetupRectTransform(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        
        // 그래프 제목
        GameObject titleObj = CreateUIGameObject("GraphTitleText_Copy", panel.transform);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "감정 변화 그래프";
        title.fontSize = 24;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        SetupRectTransform(titleObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(400, 40));
        
        // 그래프 컨테이너
        GameObject graphContainer = CreateUIGameObject("GraphContainer_Copy", panel.transform);
        graphContainer.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.8f);
        SetupRectTransform(graphContainer.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(600, 300));
        
        // 월별 통계 텍스트
        GameObject statsObj = CreateUIGameObject("MonthlyStatsText_Copy", panel.transform);
        TextMeshProUGUI stats = statsObj.AddComponent<TextMeshProUGUI>();
        stats.text = "월별 통계가 여기에 표시됩니다.";
        stats.fontSize = 14;
        stats.alignment = TextAlignmentOptions.Center;
        stats.color = new Color(0.8f, 0.8f, 0.8f);
        SetupRectTransform(statsObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(500, 60));
        
        // 월별 바 컨테이너
        GameObject barsContainer = CreateUIGameObject("MonthlyBarsContainer_Copy", panel.transform);
        SetupRectTransform(barsContainer.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.35f), Vector2.zero, new Vector2(600, 100));
        
        // 범례 컨테이너
        GameObject legendContainer = CreateUIGameObject("LegendContainer_Copy", panel.transform);
        SetupRectTransform(legendContainer.GetComponent<RectTransform>(), 
            new Vector2(0.85f, 0.6f), new Vector2(0.85f, 0.6f), Vector2.zero, new Vector2(150, 200));
    }
    
    /// <summary>
    /// 성취 패널을 생성합니다.
    /// </summary>
    private void CreateAchievementsPanel(Transform parent)
    {
        GameObject panel = CreateUIGameObject("AchievementsPanel_Copy", parent);
        panel.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.4f, 0.9f);
        SetupRectTransform(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        
        // 성취 제목
        GameObject titleObj = CreateUIGameObject("AchievementTitleText_Copy", panel.transform);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "획득한 배지";
        title.fontSize = 24;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        SetupRectTransform(titleObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(400, 40));
        
        // 총 배지 수 텍스트
        GameObject totalBadgesObj = CreateUIGameObject("TotalBadgesText_Copy", panel.transform);
        TextMeshProUGUI totalBadges = totalBadgesObj.AddComponent<TextMeshProUGUI>();
        totalBadges.text = "총 0개의 배지를 획득했습니다!";
        totalBadges.fontSize = 16;
        totalBadges.alignment = TextAlignmentOptions.Center;
        totalBadges.color = new Color(0.9f, 0.9f, 0.9f);
        SetupRectTransform(totalBadgesObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), Vector2.zero, new Vector2(400, 30));
        
        // 배지 컨테이너
        GameObject badgeContainer = CreateUIGameObject("BadgeContainer_Copy", panel.transform);
        SetupRectTransform(badgeContainer.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 400));
        
        // GridLayoutGroup 추가
        GridLayoutGroup gridLayout = badgeContainer.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(120, 120);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
    }
    
    /// <summary>
    /// 요약 패널을 생성합니다.
    /// </summary>
    private void CreateSummaryPanel(Transform parent)
    {
        GameObject panel = CreateUIGameObject("SummaryPanel_Copy", parent);
        panel.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.3f, 0.9f);
        SetupRectTransform(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        
        // 요약 제목
        GameObject titleObj = CreateUIGameObject("SummaryTitleText_Copy", panel.transform);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "성장 요약";
        title.fontSize = 24;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        SetupRectTransform(titleObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(400, 40));
        
        // 통계 정보들
        CreateSummaryStatText(panel.transform, "TotalSessionsText_Copy", "총 세션: 0회", 0.75f);
        CreateSummaryStatText(panel.transform, "ImprovementText_Copy", "감정 개선도: 0%", 0.65f);
        CreateSummaryStatText(panel.transform, "FavoriteEmotionText_Copy", "주요 감정: 기쁨", 0.55f);
        
        // 격려 메시지
        GameObject encouragementObj = CreateUIGameObject("EncouragementText_Copy", panel.transform);
        TextMeshProUGUI encouragement = encouragementObj.AddComponent<TextMeshProUGUI>();
        encouragement.text = "계속해서 성장하고 있어요!";
        encouragement.fontSize = 18;
        encouragement.alignment = TextAlignmentOptions.Center;
        encouragement.color = new Color(0.9f, 0.9f, 0.4f);
        SetupRectTransform(encouragementObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(500, 60));
    }
    
    /// <summary>
    /// 요약 통계 텍스트를 생성합니다.
    /// </summary>
    private void CreateSummaryStatText(Transform parent, string name, string text, float yPosition)
    {
        GameObject textObj = CreateUIGameObject(name, parent);
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 16;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        SetupRectTransform(textObj.GetComponent<RectTransform>(), 
            new Vector2(0.5f, yPosition), new Vector2(0.5f, yPosition), Vector2.zero, new Vector2(400, 30));
    }
    
    /// <summary>
    /// 네비게이션 버튼들을 생성합니다.
    /// </summary>
    private void CreateNavigationButtons(Transform parent)
    {
        // 뒤로 가기 버튼
        GameObject backBtn = CreateUIGameObject("BackButton_Copy", parent);
        Button backButton = backBtn.AddComponent<Button>();
        Image backImage = backBtn.AddComponent<Image>();
        backImage.color = new Color(0.6f, 0.3f, 0.3f);
        SetupRectTransform(backBtn.GetComponent<RectTransform>(), 
            new Vector2(0.1f, 0.1f), new Vector2(0.1f, 0.1f), Vector2.zero, new Vector2(120, 40));
        
        CreateButtonText(backBtn.transform, "뒤로");
        
        // 다음 버튼
        GameObject nextBtn = CreateUIGameObject("NextButton_Copy", parent);
        Button nextButton = nextBtn.AddComponent<Button>();
        Image nextImage = nextBtn.AddComponent<Image>();
        nextImage.color = new Color(0.3f, 0.6f, 0.3f);
        SetupRectTransform(nextBtn.GetComponent<RectTransform>(), 
            new Vector2(0.9f, 0.1f), new Vector2(0.9f, 0.1f), Vector2.zero, new Vector2(120, 40));
        
        CreateButtonText(nextBtn.transform, "다음");
        
        // 이전 버튼
        GameObject prevBtn = CreateUIGameObject("PreviousButton_Copy", parent);
        Button prevButton = prevBtn.AddComponent<Button>();
        Image prevImage = prevBtn.AddComponent<Image>();
        prevImage.color = new Color(0.5f, 0.5f, 0.3f);
        SetupRectTransform(prevBtn.GetComponent<RectTransform>(), 
            new Vector2(0.3f, 0.1f), new Vector2(0.3f, 0.1f), Vector2.zero, new Vector2(120, 40));
        
        CreateButtonText(prevBtn.transform, "이전");
        
        // 탭 버튼들
        CreateTabButtons(parent);
    }
    
    /// <summary>
    /// 탭 버튼들을 생성합니다.
    /// </summary>
    private void CreateTabButtons(Transform parent)
    {
        string[] tabNames = { "그래프", "성취", "요약" };
        float[] positions = { 0.3f, 0.5f, 0.7f };
        
        for (int i = 0; i < tabNames.Length; i++)
        {
            GameObject tabBtn = CreateUIGameObject($"TabButton{i}_Copy", parent);
            Button tabButton = tabBtn.AddComponent<Button>();
            Image tabImage = tabBtn.AddComponent<Image>();
            tabImage.color = new Color(0.3f, 0.3f, 0.6f);
            SetupRectTransform(tabBtn.GetComponent<RectTransform>(), 
                new Vector2(positions[i], 0.95f), new Vector2(positions[i], 0.95f), Vector2.zero, new Vector2(100, 35));
            
            CreateButtonText(tabBtn.transform, tabNames[i]);
        }
    }
    
    /// <summary>
    /// 버튼 텍스트를 생성합니다.
    /// </summary>
    private void CreateButtonText(Transform parent, string text)
    {
        GameObject textObj = CreateUIGameObject("Text_Copy", parent);
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 14;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        SetupRectTransform(textObj.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }
    
    /// <summary>
    /// UI GameObject를 생성합니다.
    /// </summary>
    private GameObject CreateUIGameObject(string name, Transform parent = null)
    {
        GameObject go = new GameObject(name);
        RectTransform rect = go.AddComponent<RectTransform>();
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }
        return go;
    }
    
    /// <summary>
    /// RectTransform을 설정합니다.
    /// </summary>
    private void SetupRectTransform(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }
    
    /// <summary>
    /// UI 메타데이터를 추가합니다.
    /// </summary>
    private void AddUIMetadata(Canvas canvas)
    {
        GameObject metadataObj = CreateUIGameObject("StudentGrowthUI_Generated", canvas.transform);
        UIMetadata metadata = metadataObj.AddComponent<UIMetadata>();
        metadata.creationTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        metadata.version = "1.0.0";
        metadata.description = "StudentGrowthUI 복사 최적화 시스템으로 생성된 UI";
        metadata.creationNotes = "Play Mode에서 생성 → 복사 → Edit Mode에서 붙여넣기 → Inspector 연결";
    }
    
    /// <summary>
    /// _Copy 요소들을 자동으로 연결합니다.
    /// </summary>
    [UnityEngine.ContextMenu("🔗 Auto Connect Copied UI")]
    public void AutoConnectCopiedUI()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("❌ Edit Mode에서만 실행 가능합니다!");
            return;
        }
        
        Debug.Log("🔗 StudentGrowthUI 자동 연결 시작...");
        
        // 각 필드별로 _Copy 요소 연결 시도
        ConnectCopiedComponents();
        
        Debug.Log("✅ StudentGrowthUI 자동 연결 완료!");
    }
    
    /// <summary>
    /// 복사된 컴포넌트들을 연결합니다.
    /// </summary>
    private void ConnectCopiedComponents()
    {
        // 패널들 연결
        welcomePanel = FindCopyGameObject("WelcomePanel_Copy");
        growthGraphPanel = FindCopyGameObject("GrowthGraphPanel_Copy");
        achievementsPanel = FindCopyGameObject("AchievementsPanel_Copy");
        summaryPanel = FindCopyGameObject("SummaryPanel_Copy");
        
        // 환영 패널 컴포넌트들 연결
        welcomeText = FindCopyComponent<TextMeshProUGUI>("WelcomeText_Copy");
        studentInfoText = FindCopyComponent<TextMeshProUGUI>("StudentInfoText_Copy");
        startButton = FindCopyComponent<Button>("StartButton_Copy");
        
        // 그래프 패널 컴포넌트들 연결
        if (growthGraphPanel != null)
        {
            graphContainer = FindCopyComponent<Transform>("GraphContainer_Copy");
            graphTitleText = FindCopyComponent<TextMeshProUGUI>("GraphTitleText_Copy");
            monthlyStatsText = FindCopyComponent<TextMeshProUGUI>("MonthlyStatsText_Copy");
            monthlyBarsContainer = FindCopyComponent<Transform>("MonthlyBarsContainer_Copy");
            legendContainer = FindCopyComponent<Transform>("LegendContainer_Copy");
        }
        
        // 성취 패널 컴포넌트들 연결
        if (achievementsPanel != null)
        {
            achievementTitleText = FindCopyComponent<TextMeshProUGUI>("AchievementTitleText_Copy");
            totalBadgesText = FindCopyComponent<TextMeshProUGUI>("TotalBadgesText_Copy");
            badgeContainer = FindCopyComponent<Transform>("BadgeContainer_Copy");
        }
        
        // 요약 패널 컴포넌트들 연결
        if (summaryPanel != null)
        {
            summaryTitleText = FindCopyComponent<TextMeshProUGUI>("SummaryTitleText_Copy");
            totalSessionsText = FindCopyComponent<TextMeshProUGUI>("TotalSessionsText_Copy");
            improvementText = FindCopyComponent<TextMeshProUGUI>("ImprovementText_Copy");
            favoriteEmotionText = FindCopyComponent<TextMeshProUGUI>("FavoriteEmotionText_Copy");
            encouragementText = FindCopyComponent<TextMeshProUGUI>("EncouragementText_Copy");
        }
        
        // 네비게이션 버튼들 연결
        backButton = FindCopyComponent<Button>("BackButton_Copy");
        nextButton = FindCopyComponent<Button>("NextButton_Copy");
        previousButton = FindCopyComponent<Button>("PreviousButton_Copy");
        
        // 탭 버튼들 연결
        List<Button> tabButtonList = new List<Button>();
        for (int i = 0; i < 3; i++)
        {
            Button tabBtn = FindCopyComponent<Button>($"TabButton{i}_Copy");
            if (tabBtn != null)
            {
                tabButtonList.Add(tabBtn);
            }
        }
        tabButtons = tabButtonList.ToArray();
    }
    
    /// <summary>
    /// _Copy GameObject를 찾습니다.
    /// </summary>
    private GameObject FindCopyGameObject(string name)
    {
        return GameObject.Find(name);
    }
    
    /// <summary>
    /// _Copy 컴포넌트를 찾습니다.
    /// </summary>
    private T FindCopyComponent<T>(string name) where T : Component
    {
        GameObject found = GameObject.Find(name);
        if (found != null)
        {
            return found.GetComponent<T>();
        }
        return null;
    }
}

/// <summary>
/// 그래프 포인트 컴포넌트
/// </summary>
public class GraphPoint : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private GameObject tooltipPanel;

    private EmotionData emotionData;

    public void Setup(EmotionData data)
    {
        emotionData = data;
    }

    public void OnPointerEnter()
    {
        if (tooltipPanel != null && emotionData != null)
        {
            tooltipPanel.SetActive(true);
            if (tooltipText != null)
            {
                tooltipText.text = $"{emotionData.beforeEmotion} → {emotionData.afterEmotion}\n변화: {emotionData.GetEmotionChange():+0;-0;0}";
            }
        }
    }

    public void OnPointerExit()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}

/// <summary>
/// 월별 막대 컴포넌트
/// </summary>
public class MonthlyBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI monthText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Image barImage;

    public void Setup(string month, float value, float normalizedHeight, Color barColor)
    {
        monthText.text = month.Substring(5); // "01", "02" 등
        valueText.text = value > 0 ? $"+{value:F1}" : $"{value:F1}";
        
        barImage.color = barColor;
        
        // 막대 높이 애니메이션
        StartCoroutine(AnimateBarHeight(normalizedHeight));
    }

    private IEnumerator AnimateBarHeight(float targetHeight)
    {
        RectTransform barRect = barImage.GetComponent<RectTransform>();
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float currentHeight = Mathf.Lerp(0f, targetHeight * 100f, progress);
            
            barRect.sizeDelta = new Vector2(barRect.sizeDelta.x, currentHeight);
            yield return null;
        }
        
        barRect.sizeDelta = new Vector2(barRect.sizeDelta.x, targetHeight * 100f);
    }
}

/// <summary>
/// 범례 아이템 컴포넌트
/// </summary>
public class LegendItem : MonoBehaviour
{
    [SerializeField] private Image colorImage;
    [SerializeField] private TextMeshProUGUI emotionText;

    public void Setup(string emotion, Color color)
    {
        emotionText.text = emotion;
        colorImage.color = color;
    }
}
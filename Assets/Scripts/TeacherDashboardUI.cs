using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TeacherDashboardUI : MonoBehaviour
{
    [Header("대시보드 패널들")]
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private GameObject emotionStatsPanel;
    [SerializeField] private GameObject studentListPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("개요 패널")]
    [SerializeField] private TextMeshProUGUI classCodeText;
    [SerializeField] private TextMeshProUGUI totalStudentsText;
    [SerializeField] private TextMeshProUGUI totalSessionsText;
    [SerializeField] private TextMeshProUGUI completeSessionsText;
    [SerializeField] private TextMeshProUGUI averageImprovementText;
    [SerializeField] private Button refreshButton;

    [Header("감정 통계 패널")]
    [SerializeField] private Transform emotionBarContainer;
    [SerializeField] private GameObject emotionBarPrefab;
    [SerializeField] private TextMeshProUGUI mostCommonEmotionText;
    [SerializeField] private TextMeshProUGUI bestImprovementEmotionText;

    [Header("학생 목록 패널")]
    [SerializeField] private Transform studentItemContainer;
    [SerializeField] private GameObject studentItemPrefab;
    [SerializeField] private TMP_InputField searchStudentInput;
    [SerializeField] private Button addStudentButton;

    [Header("네비게이션")]
    [SerializeField] private Button overviewTabButton;
    [SerializeField] private Button emotionStatsTabButton;
    [SerializeField] private Button studentListTabButton;
    [SerializeField] private Button settingsTabButton;
    [SerializeField] private Button backToMenuButton;

    [Header("색상 설정")]
    [SerializeField] private Color activeTabColor = new Color(0.2f, 0.7f, 1f);
    [SerializeField] private Color inactiveTabColor = Color.white;
    [SerializeField] private Color positiveColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color negativeColor = new Color(0.8f, 0.3f, 0.3f);
    [SerializeField] private Color neutralColor = new Color(0.7f, 0.7f, 0.7f);

    // 데이터
    private ClassStatistics currentStats;
    private List<EmotionData> currentClassData;
    private string currentClassCode;
    private int currentActiveTab = 0; // 0: 개요, 1: 감정통계, 2: 학생목록, 3: 설정

    // 감정별 색상 매핑
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
        InitializeUI();
        LoadClassData();
    }

    private void InitializeUI()
    {
        // 탭 버튼 이벤트 설정
        overviewTabButton.onClick.AddListener(() => ShowTab(0));
        emotionStatsTabButton.onClick.AddListener(() => ShowTab(1));
        studentListTabButton.onClick.AddListener(() => ShowTab(2));
        settingsTabButton.onClick.AddListener(() => ShowTab(3));

        // 기타 버튼 이벤트 설정
        refreshButton.onClick.AddListener(RefreshData);
        addStudentButton.onClick.AddListener(ShowAddStudentDialog);
        backToMenuButton.onClick.AddListener(BackToMainMenu);

        // 검색 입력 이벤트
        searchStudentInput.onValueChanged.AddListener(OnSearchInputChanged);

        // 기본 탭 표시
        ShowTab(0);
    }

    private void LoadClassData()
    {
        if (!UserManager.IsTeacherMode())
        {
            Debug.LogError("교사 모드가 아닙니다.");
            return;
        }

        currentClassCode = ClassCodeManager.GetCurrentClassCode();
        
        if (string.IsNullOrEmpty(currentClassCode))
        {
            Debug.LogError("현재 클래스 코드가 없습니다.");
            ShowError("클래스 코드를 먼저 설정해주세요.");
            return;
        }

        classCodeText.text = $"클래스: {currentClassCode}";

        // 구글 시트에서 데이터 읽기
        GoogleSheetsManager.Instance.ReadClassStatistics(currentClassCode, OnStatisticsLoaded);
        GoogleSheetsManager.Instance.ReadClassEmotionData(currentClassCode, OnClassDataLoaded);
    }

    private void OnStatisticsLoaded(ClassStatistics stats)
    {
        currentStats = stats;
        UpdateOverviewPanel();
        UpdateEmotionStatsPanel();
    }

    private void OnClassDataLoaded(List<EmotionData> classData)
    {
        currentClassData = classData;
        UpdateStudentListPanel();
    }

    private void ShowTab(int tabIndex)
    {
        currentActiveTab = tabIndex;

        // 모든 패널 비활성화
        overviewPanel.SetActive(false);
        emotionStatsPanel.SetActive(false);
        studentListPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // 모든 탭 버튼 색상 초기화
        SetTabButtonColor(overviewTabButton, false);
        SetTabButtonColor(emotionStatsTabButton, false);
        SetTabButtonColor(studentListTabButton, false);
        SetTabButtonColor(settingsTabButton, false);

        // 선택된 탭 활성화
        switch (tabIndex)
        {
            case 0:
                overviewPanel.SetActive(true);
                SetTabButtonColor(overviewTabButton, true);
                break;
            case 1:
                emotionStatsPanel.SetActive(true);
                SetTabButtonColor(emotionStatsTabButton, true);
                break;
            case 2:
                studentListPanel.SetActive(true);
                SetTabButtonColor(studentListTabButton, true);
                break;
            case 3:
                settingsPanel.SetActive(true);
                SetTabButtonColor(settingsTabButton, true);
                break;
        }
    }

    private void SetTabButtonColor(Button button, bool isActive)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isActive ? activeTabColor : inactiveTabColor;
        }
    }

    private void UpdateOverviewPanel()
    {
        if (currentStats == null)
        {
            return;
        }

        totalStudentsText.text = $"총 학생 수: {currentStats.totalStudents}명";
        totalSessionsText.text = $"전체 세션: {currentStats.totalSessions}회";
        completeSessionsText.text = $"완료 세션: {currentStats.completeSessions}회";
        
        string improvementText;
        Color improvementColor;
        
        if (currentStats.overallImprovement > 0)
        {
            improvementText = $"평균 개선도: +{currentStats.overallImprovement:F1} ⬆️";
            improvementColor = positiveColor;
        }
        else if (currentStats.overallImprovement < 0)
        {
            improvementText = $"평균 개선도: {currentStats.overallImprovement:F1} ⬇️";
            improvementColor = negativeColor;
        }
        else
        {
            improvementText = "평균 개선도: 변화없음 ➡️";
            improvementColor = neutralColor;
        }
        
        averageImprovementText.text = improvementText;
        averageImprovementText.color = improvementColor;
    }

    private void UpdateEmotionStatsPanel()
    {
        if (currentStats == null || currentStats.emotionCounts == null)
        {
            return;
        }

        // 기존 감정 바 제거
        foreach (Transform child in emotionBarContainer)
        {
            Destroy(child.gameObject);
        }

        // 감정별 통계 바 생성
        int maxCount = currentStats.emotionCounts.Values.Max();
        
        foreach (var emotionCount in currentStats.emotionCounts.OrderByDescending(x => x.Value))
        {
            GameObject barItem = Instantiate(emotionBarPrefab, emotionBarContainer);
            EmotionBarItem barComponent = barItem.GetComponent<EmotionBarItem>();
            
            if (barComponent != null)
            {
                float percentage = maxCount > 0 ? (float)emotionCount.Value / maxCount : 0f;
                Color barColor = emotionColors.ContainsKey(emotionCount.Key) ? 
                                emotionColors[emotionCount.Key] : neutralColor;
                
                float improvement = currentStats.emotionImprovements.ContainsKey(emotionCount.Key) ?
                                  currentStats.emotionImprovements[emotionCount.Key] : 0f;
                
                barComponent.Setup(emotionCount.Key, emotionCount.Value, percentage, barColor, improvement);
            }
        }

        // 가장 많은 감정과 가장 개선된 감정 표시
        if (currentStats.emotionCounts.Count > 0)
        {
            var mostCommon = currentStats.emotionCounts.OrderByDescending(x => x.Value).First();
            mostCommonEmotionText.text = $"가장 많은 감정: {mostCommon.Key} ({mostCommon.Value}회)";
        }

        if (currentStats.emotionImprovements.Count > 0)
        {
            var bestImprovement = currentStats.emotionImprovements.OrderByDescending(x => x.Value).First();
            bestImprovementEmotionText.text = $"가장 개선된 감정: {bestImprovement.Key} (+{bestImprovement.Value:F1})";
        }
    }

    private void UpdateStudentListPanel()
    {
        if (currentClassData == null)
        {
            return;
        }

        // 기존 학생 아이템 제거
        foreach (Transform child in studentItemContainer)
        {
            Destroy(child.gameObject);
        }

        // 학생별 데이터 그룹화
        var studentGroups = currentClassData.GroupBy(d => d.studentNumber)
                                          .OrderBy(g => g.Key);

        foreach (var studentGroup in studentGroups)
        {
            int studentNumber = int.TryParse(studentGroup.Key, out int parsedNumber) ? parsedNumber : 0;
            var studentData = studentGroup.ToList();
            
            GameObject studentItem = Instantiate(studentItemPrefab, studentItemContainer);
            StudentDashboardItem itemComponent = studentItem.GetComponent<StudentDashboardItem>();
            
            if (itemComponent != null)
            {
                // 학생 통계 계산
                int totalSessions = studentData.Count;
                int completeSessions = studentData.Count(d => d.IsComplete());
                float avgImprovement = completeSessions > 0 ? 
                    studentData.Where(d => d.IsComplete()).Average(d => d.GetEmotionChange()) : 0f;
                
                string mostRecentEmotion = studentData.OrderByDescending(d => d.timestamp)
                                                    .FirstOrDefault()?.beforeEmotion ?? "없음";
                
                itemComponent.Setup(studentNumber, totalSessions, completeSessions, 
                                   avgImprovement, mostRecentEmotion, OnStudentItemClicked);
            }
        }
    }

    private void OnStudentItemClicked(int studentNumber)
    {
        Debug.Log($"학생 {studentNumber}번 상세 정보 보기");
        ShowStudentDetails(studentNumber);
    }
    
    private void ShowStudentDetails(int studentNumber)
    {
        // 학생 상세 정보 표시
        var studentInfo = GetStudentInfo(studentNumber);
        if (studentInfo != null)
        {
            // 임시로 디버그 로그로 표시, 나중에 UI 팝업으로 변경 가능
            Debug.Log($"학생 {studentNumber}번 상세 정보:");
            Debug.Log($"- 이름: {studentInfo.name}");
            
            // 감정 데이터에서 추가 정보 가져오기
            var emotionHistory = GetStudentEmotionHistory(studentNumber);
            if (emotionHistory != null && emotionHistory.Count > 0)
            {
                var lastEmotion = emotionHistory[emotionHistory.Count - 1];
                Debug.Log($"- 최근 감정 상태: {lastEmotion.afterEmotion}");
                Debug.Log($"- 상담 기록 수: {emotionHistory.Count}");
            }
            else
            {
                Debug.Log("- 최근 감정 상태: 기록 없음");
                Debug.Log("- 상담 기록 수: 0");
            }
            
            // 상세 정보 화면으로 이동하거나 팝업 표시
            // SceneManager.LoadScene("StudentDetailScene");
        }
        else
        {
            Debug.LogWarning($"학생 {studentNumber}번 정보를 찾을 수 없습니다.");
        }
    }
    
    private StudentInfo GetStudentInfo(int studentNumber)
    {
        // 기존 StudentDataManager를 통해 학생 정보 조회
        var schoolData = StudentDataManager.LoadSchoolData();
        if (schoolData != null && schoolData.students != null)
        {
            foreach (var student in schoolData.students)
            {
                if (student.number == studentNumber)
                {
                    return student;
                }
            }
        }
        return null;
    }
    
    private List<EmotionData> GetStudentEmotionHistory(int studentNumber)
    {
        // EmotionManager에서 해당 학생의 감정 기록 가져오기
        if (EmotionManager.Instance != null)
        {
            var allEmotions = EmotionManager.Instance.GetEmotionHistory();
            var studentEmotions = new List<EmotionData>();
            
            foreach (var emotion in allEmotions)
            {
                if (emotion.GetStudentNumberAsInt() == studentNumber)
                {
                    studentEmotions.Add(emotion);
                }
            }
            
            return studentEmotions;
        }
        return new List<EmotionData>();
    }

    private void OnSearchInputChanged(string searchText)
    {
        // 검색 기능 구현
        foreach (Transform child in studentItemContainer)
        {
            StudentDashboardItem item = child.GetComponent<StudentDashboardItem>();
            if (item != null)
            {
                bool shouldShow = string.IsNullOrEmpty(searchText) || 
                                item.GetStudentNumber().ToString().Contains(searchText);
                child.gameObject.SetActive(shouldShow);
            }
        }
    }

    private void RefreshData()
    {
        Debug.Log("데이터 새로고침 중...");
        LoadClassData();
    }

    private void ShowAddStudentDialog()
    {
        Debug.Log("학생 추가 다이얼로그 표시");
        ShowAddStudentPopup();
    }
    
    private void ShowAddStudentPopup()
    {
        // 학생 추가 팝업 구현
        // 임시로 간단한 입력 다이얼로그 시뮬레이션
        Debug.Log("학생 추가 팝업 표시됨");
        
        // 실제 구현 시에는 UI 팝업을 생성하거나 InputField 다이얼로그 표시
        // 예: GameObject.Instantiate(addStudentPopupPrefab);
        
        // 임시 구현: 랜덤 학생 추가
        AddNewStudent();
    }
    
    private void AddNewStudent()
    {
        // 새 학생 추가 로직
        int newStudentNumber = UnityEngine.Random.Range(1000, 9999);
        string newStudentName = $"학생{newStudentNumber}";
        
        Debug.Log($"새 학생 추가: {newStudentNumber}번 {newStudentName}");
        
        // 학생 데이터 저장 로직
        // StudentDataManager.Instance.AddStudent(newStudentNumber, newStudentName);
        
        // UI 새로고침
        LoadClassData();
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    private void ShowError(string message)
    {
        Debug.LogError(message);
        ShowErrorPopup(message);
    }
    
    private void ShowErrorPopup(string errorMessage)
    {
        // 간소화된 에러 처리 - 안전한 Debug 로그 사용
        Debug.LogError($"TeacherDashboard UI 오류: {errorMessage}");
        
        // 추후 필요시 UI 팝업이나 다른 에러 표시 방법을 추가할 수 있음
        // 예: GameObject.Instantiate(errorPopupPrefab);
    }

    // 애니메이션 효과
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

    /// <summary>
    /// 외부에서 특정 탭으로 이동
    /// </summary>
    public void NavigateToTab(int tabIndex)
    {
        ShowTab(tabIndex);
    }

    /// <summary>
    /// 현재 클래스 통계 가져오기
    /// </summary>
    public ClassStatistics GetCurrentStatistics()
    {
        return currentStats;
    }
}

/// <summary>
/// 감정 바 아이템 컴포넌트
/// </summary>
public class EmotionBarItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI emotionNameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI improvementText;
    [SerializeField] private Image barFillImage;
    [SerializeField] private Image backgroundImage;

    public void Setup(string emotionName, int count, float fillPercentage, Color barColor, float improvement)
    {
        emotionNameText.text = emotionName;
        countText.text = $"{count}회";
        
        // 개선도 표시
        if (improvement > 0)
        {
            improvementText.text = $"+{improvement:F1}";
            improvementText.color = Color.green;
        }
        else if (improvement < 0)
        {
            improvementText.text = $"{improvement:F1}";
            improvementText.color = Color.red;
        }
        else
        {
            improvementText.text = "0.0";
            improvementText.color = Color.gray;
        }

        // 바 채우기 애니메이션
        barFillImage.color = barColor;
        StartCoroutine(AnimateFill(fillPercentage));
    }

    private IEnumerator AnimateFill(float targetFill)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            barFillImage.fillAmount = Mathf.Lerp(0f, targetFill, progress);
            yield return null;
        }
        
        barFillImage.fillAmount = targetFill;
    }
}

/// <summary>
/// 학생 대시보드 아이템 컴포넌트
/// </summary>
public class StudentDashboardItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI studentNumberText;
    [SerializeField] private TextMeshProUGUI sessionCountText;
    [SerializeField] private TextMeshProUGUI improvementText;
    [SerializeField] private TextMeshProUGUI recentEmotionText;
    [SerializeField] private Button detailButton;
    [SerializeField] private Image statusIndicator;

    private int studentNumber;
    private System.Action<int> onDetailClicked;

    public void Setup(int number, int totalSessions, int completeSessions, 
                     float avgImprovement, string recentEmotion, System.Action<int> onDetailClick)
    {
        studentNumber = number;
        onDetailClicked = onDetailClick;

        studentNumberText.text = $"{number}번";
        sessionCountText.text = $"{completeSessions}/{totalSessions}";
        recentEmotionText.text = recentEmotion;

        // 개선도 표시
        if (avgImprovement > 0)
        {
            improvementText.text = $"+{avgImprovement:F1}";
            improvementText.color = Color.green;
            statusIndicator.color = Color.green;
        }
        else if (avgImprovement < 0)
        {
            improvementText.text = $"{avgImprovement:F1}";
            improvementText.color = Color.red;
            statusIndicator.color = Color.red;
        }
        else
        {
            improvementText.text = "0.0";
            improvementText.color = Color.gray;
            statusIndicator.color = Color.gray;
        }

        detailButton.onClick.AddListener(() => onDetailClicked?.Invoke(studentNumber));
    }

    public int GetStudentNumber()
    {
        return studentNumber;
    }
}
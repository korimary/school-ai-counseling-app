using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StudentDetailPopup : MonoBehaviour
{
    [Header("학생 정보")]
    [SerializeField] private TextMeshProUGUI studentNumberText;
    [SerializeField] private TextMeshProUGUI studentNameText;
    [SerializeField] private TextMeshProUGUI totalSessionsText;
    [SerializeField] private TextMeshProUGUI completedSessionsText;
    [SerializeField] private TextMeshProUGUI averageImprovementText;
    [SerializeField] private TextMeshProUGUI currentEmotionText;
    
    [Header("감정 히스토리")]
    [SerializeField] private Transform emotionHistoryContainer;
    [SerializeField] private GameObject emotionHistoryItemPrefab;
    [SerializeField] private Button showMoreHistoryButton;
    [SerializeField] private int initialHistoryDisplayCount = 5;
    
    [Header("감정 차트")]
    [SerializeField] private Transform emotionChartContainer;
    [SerializeField] private GameObject emotionChartBarPrefab;
    
    [Header("개선도 그래프")]
    [SerializeField] private LineRenderer improvementLineRenderer;
    [SerializeField] private Transform improvementGraphContainer;
    [SerializeField] private TextMeshProUGUI improvementTrendText;
    
    [Header("UI 컨트롤")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private GameObject loadingIndicator;
    
    [Header("색상 설정")]
    [SerializeField] private Color positiveColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color negativeColor = new Color(0.8f, 0.3f, 0.3f);
    [SerializeField] private Color neutralColor = new Color(0.7f, 0.7f, 0.7f);
    
    private StudentInfo currentStudent;
    private List<EmotionData> studentEmotionHistory;
    private int currentHistoryDisplayCount;
    private bool isShowingAllHistory = false;
    
    private readonly Dictionary<string, Color> emotionColors = new Dictionary<string, Color>
    {
        { "기쁨", new Color(1f, 0.9f, 0.3f) },
        { "슬픔", new Color(0.3f, 0.5f, 1f) },
        { "화남", new Color(1f, 0.3f, 0.3f) },
        { "불안", new Color(0.8f, 0.3f, 1f) },
        { "신남", new Color(1f, 0.5f, 0.3f) },
        { "복잡", new Color(0.5f, 0.5f, 0.5f) }
    };
    
    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
            
        if (exportButton != null)
            exportButton.onClick.AddListener(ExportStudentData);
            
        if (showMoreHistoryButton != null)
            showMoreHistoryButton.onClick.AddListener(ToggleHistoryDisplay);
            
        currentHistoryDisplayCount = initialHistoryDisplayCount;
    }
    
    public void ShowStudentDetails(int studentNumber)
    {
        gameObject.SetActive(true);
        StartCoroutine(LoadStudentDetails(studentNumber));
    }
    
    private IEnumerator LoadStudentDetails(int studentNumber)
    {
        ShowLoading(true);
        
        // 학생 기본 정보 로드
        currentStudent = GetStudentInfo(studentNumber);
        
        // 감정 히스토리 로드
        studentEmotionHistory = GetStudentEmotionHistory(studentNumber);
        
        yield return null; // 프레임 대기
        
        if (currentStudent != null)
        {
            UpdateBasicInfo();
            UpdateEmotionHistory();
            UpdateEmotionChart();
            UpdateImprovementGraph();
        }
        else
        {
            Debug.LogError($"학생 {studentNumber}번 정보를 찾을 수 없습니다.");
            ShowError("학생 정보를 찾을 수 없습니다.");
        }
        
        ShowLoading(false);
    }
    
    private void UpdateBasicInfo()
    {
        studentNumberText.text = $"{currentStudent.number}번";
        studentNameText.text = currentStudent.name;
        
        int totalSessions = studentEmotionHistory.Count;
        int completedSessions = studentEmotionHistory.Count(e => e.IsComplete());
        
        totalSessionsText.text = $"총 상담: {totalSessions}회";
        completedSessionsText.text = $"완료: {completedSessions}회";
        
        // 평균 개선도 계산
        float avgImprovement = 0f;
        if (completedSessions > 0)
        {
            avgImprovement = studentEmotionHistory
                .Where(e => e.IsComplete())
                .Average(e => e.GetEmotionChange());
        }
        
        UpdateImprovementText(avgImprovement);
        
        // 현재 감정 상태
        var lastEmotion = studentEmotionHistory
            .OrderByDescending(e => e.timestamp)
            .FirstOrDefault();
            
        if (lastEmotion != null)
        {
            string currentEmotion = lastEmotion.IsComplete() ? 
                lastEmotion.afterEmotion : lastEmotion.beforeEmotion;
            currentEmotionText.text = $"현재 감정: {currentEmotion}";
            
            if (emotionColors.ContainsKey(currentEmotion))
            {
                currentEmotionText.color = emotionColors[currentEmotion];
            }
        }
        else
        {
            currentEmotionText.text = "현재 감정: 기록 없음";
            currentEmotionText.color = neutralColor;
        }
    }
    
    private void UpdateEmotionHistory()
    {
        // 기존 아이템 제거
        foreach (Transform child in emotionHistoryContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 히스토리 표시
        var sortedHistory = studentEmotionHistory
            .OrderByDescending(e => e.timestamp)
            .ToList();
            
        int displayCount = isShowingAllHistory ? 
            sortedHistory.Count : 
            Mathf.Min(currentHistoryDisplayCount, sortedHistory.Count);
            
        for (int i = 0; i < displayCount; i++)
        {
            var emotion = sortedHistory[i];
            CreateHistoryItem(emotion);
        }
        
        // 더보기 버튼 표시 여부
        if (showMoreHistoryButton != null)
        {
            bool hasMoreItems = sortedHistory.Count > initialHistoryDisplayCount;
            showMoreHistoryButton.gameObject.SetActive(hasMoreItems);
            
            if (hasMoreItems)
            {
                var buttonText = showMoreHistoryButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isShowingAllHistory ? "접기" : "더보기";
                }
            }
        }
    }
    
    private void CreateHistoryItem(EmotionData emotion)
    {
        if (emotionHistoryItemPrefab == null) return;
        
        GameObject item = Instantiate(emotionHistoryItemPrefab, emotionHistoryContainer);
        
        // 날짜
        var dateText = item.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        if (dateText != null)
        {
            System.DateTime date = System.DateTime.Parse(emotion.timestamp);
            dateText.text = date.ToString("MM/dd HH:mm");
        }
        
        // 감정 변화
        var emotionChangeText = item.transform.Find("EmotionChangeText")?.GetComponent<TextMeshProUGUI>();
        if (emotionChangeText != null)
        {
            if (emotion.IsComplete())
            {
                emotionChangeText.text = $"{emotion.beforeEmotion} → {emotion.afterEmotion}";
                
                float change = emotion.GetEmotionChange();
                if (change > 0)
                    emotionChangeText.color = positiveColor;
                else if (change < 0)
                    emotionChangeText.color = negativeColor;
                else
                    emotionChangeText.color = neutralColor;
            }
            else
            {
                emotionChangeText.text = $"{emotion.beforeEmotion} (진행중)";
                emotionChangeText.color = neutralColor;
            }
        }
        
        // 개선도
        var improvementText = item.transform.Find("ImprovementText")?.GetComponent<TextMeshProUGUI>();
        if (improvementText != null && emotion.IsComplete())
        {
            float change = emotion.GetEmotionChange();
            if (change > 0)
                improvementText.text = $"+{change:F1}";
            else
                improvementText.text = $"{change:F1}";
                
            improvementText.color = change > 0 ? positiveColor : 
                                  change < 0 ? negativeColor : neutralColor;
        }
    }
    
    private void UpdateEmotionChart()
    {
        // 기존 차트 제거
        foreach (Transform child in emotionChartContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 감정별 통계 계산
        Dictionary<string, int> emotionCounts = new Dictionary<string, int>();
        
        foreach (var emotion in studentEmotionHistory)
        {
            CountEmotion(emotionCounts, emotion.beforeEmotion);
            if (emotion.IsComplete())
            {
                CountEmotion(emotionCounts, emotion.afterEmotion);
            }
        }
        
        // 차트 생성
        if (emotionCounts.Count > 0)
        {
            int maxCount = emotionCounts.Values.Max();
            
            foreach (var kvp in emotionCounts.OrderByDescending(x => x.Value))
            {
                CreateChartBar(kvp.Key, kvp.Value, maxCount);
            }
        }
    }
    
    private void CountEmotion(Dictionary<string, int> counts, string emotion)
    {
        if (string.IsNullOrEmpty(emotion)) return;
        
        if (counts.ContainsKey(emotion))
            counts[emotion]++;
        else
            counts[emotion] = 1;
    }
    
    private void CreateChartBar(string emotionName, int count, int maxCount)
    {
        if (emotionChartBarPrefab == null) return;
        
        GameObject bar = Instantiate(emotionChartBarPrefab, emotionChartContainer);
        
        var nameText = bar.transform.Find("EmotionNameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = emotionName;
            
        var countText = bar.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
        if (countText != null)
            countText.text = count.ToString();
            
        var fillImage = bar.transform.Find("FillBar")?.GetComponent<Image>();
        if (fillImage != null)
        {
            float fillAmount = maxCount > 0 ? (float)count / maxCount : 0f;
            fillImage.fillAmount = fillAmount;
            
            if (emotionColors.ContainsKey(emotionName))
                fillImage.color = emotionColors[emotionName];
        }
    }
    
    private void UpdateImprovementGraph()
    {
        var completedEmotions = studentEmotionHistory
            .Where(e => e.IsComplete())
            .OrderBy(e => e.timestamp)
            .ToList();
            
        if (completedEmotions.Count < 2)
        {
            improvementLineRenderer.gameObject.SetActive(false);
            improvementTrendText.text = "데이터 부족";
            return;
        }
        
        // 개선도 계산
        List<float> improvements = new List<float>();
        float cumulativeImprovement = 0f;
        
        foreach (var emotion in completedEmotions)
        {
            cumulativeImprovement += emotion.GetEmotionChange();
            improvements.Add(cumulativeImprovement);
        }
        
        // 라인 렌더러 설정
        improvementLineRenderer.gameObject.SetActive(true);
        improvementLineRenderer.positionCount = improvements.Count;
        
        float graphWidth = improvementGraphContainer.GetComponent<RectTransform>().rect.width;
        float graphHeight = improvementGraphContainer.GetComponent<RectTransform>().rect.height;
        
        float maxImprovement = improvements.Max();
        float minImprovement = improvements.Min();
        float range = maxImprovement - minImprovement;
        
        if (range < 1f) range = 1f;
        
        for (int i = 0; i < improvements.Count; i++)
        {
            float x = (i / (float)(improvements.Count - 1)) * graphWidth;
            float y = ((improvements[i] - minImprovement) / range) * graphHeight;
            
            Vector3 position = new Vector3(x, y, 0);
            improvementLineRenderer.SetPosition(i, position);
        }
        
        // 추세 분석
        float trend = improvements.Last() - improvements.First();
        if (trend > 0)
        {
            improvementTrendText.text = "상승 추세 ^";
            improvementTrendText.color = positiveColor;
        }
        else if (trend < 0)
        {
            improvementTrendText.text = "하락 추세 v";
            improvementTrendText.color = negativeColor;
        }
        else
        {
            improvementTrendText.text = "변화 없음 →";
            improvementTrendText.color = neutralColor;
        }
    }
    
    private void UpdateImprovementText(float improvement)
    {
        if (improvement > 0)
        {
            averageImprovementText.text = $"평균 개선도: +{improvement:F1}";
            averageImprovementText.color = positiveColor;
        }
        else if (improvement < 0)
        {
            averageImprovementText.text = $"평균 개선도: {improvement:F1}";
            averageImprovementText.color = negativeColor;
        }
        else
        {
            averageImprovementText.text = "평균 개선도: 0.0";
            averageImprovementText.color = neutralColor;
        }
    }
    
    private StudentInfo GetStudentInfo(int studentNumber)
    {
        var schoolData = StudentDataManager.LoadSchoolData();
        if (schoolData != null && schoolData.students != null)
        {
            return schoolData.students.FirstOrDefault(s => s.number == studentNumber);
        }
        return null;
    }
    
    private List<EmotionData> GetStudentEmotionHistory(int studentNumber)
    {
        if (EmotionManager.Instance != null)
        {
            return EmotionManager.Instance.GetStudentEmotionData(studentNumber.ToString());
        }
        return new List<EmotionData>();
    }
    
    private void ToggleHistoryDisplay()
    {
        isShowingAllHistory = !isShowingAllHistory;
        UpdateEmotionHistory();
    }
    
    private void ExportStudentData()
    {
        Debug.Log($"학생 {currentStudent.number}번 데이터 내보내기");
        // TODO: 실제 내보내기 기능 구현 (CSV, PDF 등)
    }
    
    private void ClosePopup()
    {
        gameObject.SetActive(false);
    }
    
    private void ShowLoading(bool show)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(show);
    }
    
    private void ShowError(string message)
    {
        Debug.LogError(message);
        // TODO: 에러 팝업 표시
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

[Serializable]
public class Badge
{
    public string id;
    public string name;
    public string description;
    public string emoji;
    public BadgeCategory category;
    public BadgeRarity rarity;
    public bool isEarned;
    public DateTime earnedDate;
    public BadgeCondition condition;

    public Badge(string id, string name, string description, string emoji, 
                BadgeCategory category, BadgeRarity rarity, BadgeCondition condition)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.emoji = emoji;
        this.category = category;
        this.rarity = rarity;
        this.condition = condition;
        this.isEarned = false;
    }

    public void Earn()
    {
        isEarned = true;
        earnedDate = DateTime.Now;
    }
}

[Serializable]
public class BadgeCondition
{
    public BadgeConditionType type;
    public int targetValue;
    public string targetEmotion;
    public int targetConsecutive;

    public BadgeCondition(BadgeConditionType type, int targetValue, string targetEmotion = "", int targetConsecutive = 0)
    {
        this.type = type;
        this.targetValue = targetValue;
        this.targetEmotion = targetEmotion;
        this.targetConsecutive = targetConsecutive;
    }
}

public enum BadgeCategory
{
    Participation,  // 참여
    Improvement,    // 개선
    Consistency,    // 일관성
    Special,        // 특별
    Achievement     // 성취
}

public enum BadgeRarity
{
    Common,     // 일반
    Uncommon,   // 특별
    Rare,       // 희귀
    Epic,       // 영웅
    Legendary   // 전설
}

public enum BadgeConditionType
{
    TotalSessions,          // 총 세션 수
    ConsecutiveSessions,    // 연속 세션
    EmotionImprovement,     // 감정 개선
    SpecificEmotion,        // 특정 감정
    WeeklyStreak,          // 주간 연속
    MonthlyActive,         // 월간 활동
    BigImprovement,        // 큰 개선
    ConsistentMood         // 안정적 감정
}

public class BadgeSystem : MonoBehaviour
{
    [Header("배지 시스템 설정")]
    [SerializeField] private bool enableDebugMode = true;
    #pragma warning disable 0414
    [SerializeField] private float checkInterval = 30f; // 배지 체크 간격 (초)
    #pragma warning restore 0414

    private static BadgeSystem _instance;
    public static BadgeSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BadgeSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("BadgeSystem");
                    _instance = go.AddComponent<BadgeSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // 이벤트 시스템
    public static event System.Action<Badge> OnBadgeEarned;
    public static event System.Action<int, List<Badge>> OnStudentBadgesUpdated;

    // 배지 데이터
    private List<Badge> allBadges;
    private Dictionary<int, List<Badge>> studentBadges; // 학생번호 -> 배지 리스트

    // 저장 키
    private const string STUDENT_BADGES_KEY = "StudentBadges";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        InitializeBadges();
        LoadStudentBadges();
        
        // 감정 데이터 변경 이벤트 구독
        if (EmotionManager.Instance != null)
        {
            EmotionManager.Instance.OnEmotionDataSubmitted += OnEmotionDataSubmitted;
        }
        
        Debug.Log("BadgeSystem 초기화 완료");
    }

    private void InitializeBadges()
    {
        allBadges = new List<Badge>();

        // 참여 배지들
        allBadges.Add(new Badge("first_session", "첫 걸음", "첫 번째 상담 완료", "🌱", 
            BadgeCategory.Participation, BadgeRarity.Common, 
            new BadgeCondition(BadgeConditionType.TotalSessions, 1)));

        allBadges.Add(new Badge("sessions_5", "대화의 시작", "5회 상담 완료", "💬", 
            BadgeCategory.Participation, BadgeRarity.Common, 
            new BadgeCondition(BadgeConditionType.TotalSessions, 5)));

        allBadges.Add(new Badge("sessions_10", "소통 전문가", "10회 상담 완료", "🗣️", 
            BadgeCategory.Participation, BadgeRarity.Uncommon, 
            new BadgeCondition(BadgeConditionType.TotalSessions, 10)));

        allBadges.Add(new Badge("sessions_20", "마음 나누기 달인", "20회 상담 완료", "💝", 
            BadgeCategory.Participation, BadgeRarity.Rare, 
            new BadgeCondition(BadgeConditionType.TotalSessions, 20)));

        // 개선 배지들
        allBadges.Add(new Badge("first_improvement", "기분 전환", "첫 번째 감정 개선", "😊", 
            BadgeCategory.Improvement, BadgeRarity.Common, 
            new BadgeCondition(BadgeConditionType.EmotionImprovement, 1)));

        allBadges.Add(new Badge("big_improvement", "마음의 변화", "한 번에 3점 이상 개선", "🌈", 
            BadgeCategory.Improvement, BadgeRarity.Uncommon, 
            new BadgeCondition(BadgeConditionType.BigImprovement, 3)));

        allBadges.Add(new Badge("super_improvement", "감정 마스터", "한 번에 4점 이상 개선", "✨", 
            BadgeCategory.Improvement, BadgeRarity.Rare, 
            new BadgeCondition(BadgeConditionType.BigImprovement, 4)));

        // 일관성 배지들
        allBadges.Add(new Badge("streak_3", "꾸준함의 시작", "3일 연속 상담", "🔥", 
            BadgeCategory.Consistency, BadgeRarity.Common, 
            new BadgeCondition(BadgeConditionType.ConsecutiveSessions, 3)));

        allBadges.Add(new Badge("streak_7", "일주일 챌린지", "7일 연속 상담", "⭐", 
            BadgeCategory.Consistency, BadgeRarity.Uncommon, 
            new BadgeCondition(BadgeConditionType.ConsecutiveSessions, 7)));

        allBadges.Add(new Badge("weekly_streak", "주간 달인", "한 주 동안 꾸준히 참여", "🏆", 
            BadgeCategory.Consistency, BadgeRarity.Rare, 
            new BadgeCondition(BadgeConditionType.WeeklyStreak, 1)));

        // 감정별 특별 배지들
        allBadges.Add(new Badge("joy_master", "기쁨 전문가", "기쁨 감정 5회 체크", "😄", 
            BadgeCategory.Special, BadgeRarity.Uncommon, 
            new BadgeCondition(BadgeConditionType.SpecificEmotion, 5, "기쁨")));

        allBadges.Add(new Badge("brave_heart", "용감한 마음", "불안 감정 극복 3회", "💪", 
            BadgeCategory.Special, BadgeRarity.Rare, 
            new BadgeCondition(BadgeConditionType.SpecificEmotion, 3, "불안")));

        allBadges.Add(new Badge("calm_mind", "평온한 마음", "복잡한 감정 정리 3회", "🧘", 
            BadgeCategory.Special, BadgeRarity.Rare, 
            new BadgeCondition(BadgeConditionType.SpecificEmotion, 3, "복잡")));

        // 성취 배지들
        allBadges.Add(new Badge("monthly_active", "이달의 활동가", "한 달간 활발한 참여", "📅", 
            BadgeCategory.Achievement, BadgeRarity.Epic, 
            new BadgeCondition(BadgeConditionType.MonthlyActive, 1)));

        allBadges.Add(new Badge("emotion_stable", "감정 안정", "연속 5회 안정적 감정", "⚖️", 
            BadgeCategory.Achievement, BadgeRarity.Epic, 
            new BadgeCondition(BadgeConditionType.ConsistentMood, 5)));

        allBadges.Add(new Badge("growth_legend", "성장의 전설", "모든 기본 배지 획득", "👑", 
            BadgeCategory.Achievement, BadgeRarity.Legendary, 
            new BadgeCondition(BadgeConditionType.TotalSessions, 50)));

        Debug.Log($"총 {allBadges.Count}개의 배지가 초기화되었습니다.");
    }

    private void OnEmotionDataSubmitted(EmotionData emotionData, bool success)
    {
        if (success && emotionData != null && emotionData.IsValid())
        {
            CheckAndAwardBadges(emotionData.GetStudentNumberAsInt());
        }
    }

    /// <summary>
    /// 특정 학생의 배지 조건 체크 및 수여
    /// </summary>
    public void CheckAndAwardBadges(int studentNumber)
    {
        string classCode = ClassCodeManager.GetCurrentClassCode();
        
        GoogleSheetsManager.Instance.ReadStudentEmotionData(classCode, studentNumber, (studentData) =>
        {
            if (studentData != null)
            {
                CheckBadgeConditions(studentNumber, studentData);
            }
        });
    }

    private void CheckBadgeConditions(int studentNumber, List<EmotionData> studentData)
    {
        if (!studentBadges.ContainsKey(studentNumber))
        {
            studentBadges[studentNumber] = new List<Badge>();
            
            // 모든 배지의 복사본 생성
            foreach (var badge in allBadges)
            {
                Badge studentBadge = new Badge(badge.id, badge.name, badge.description, 
                                              badge.emoji, badge.category, badge.rarity, badge.condition);
                studentBadges[studentNumber].Add(studentBadge);
            }
        }

        List<Badge> earnedBadges = new List<Badge>();
        
        foreach (var badge in studentBadges[studentNumber])
        {
            if (!badge.isEarned && CheckBadgeCondition(badge, studentData))
            {
                badge.Earn();
                earnedBadges.Add(badge);
                
                // 배지 획득 사운드 재생
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBadgeEarned();
                }
                
                if (enableDebugMode)
                {
                    Debug.Log($"배지 획득! 학생 {studentNumber}: {badge.name} {badge.emoji}");
                }
                
                OnBadgeEarned?.Invoke(badge);
            }
        }

        if (earnedBadges.Count > 0)
        {
            SaveStudentBadges();
            OnStudentBadgesUpdated?.Invoke(studentNumber, studentBadges[studentNumber]);
        }
    }

    private bool CheckBadgeCondition(Badge badge, List<EmotionData> studentData)
    {
        BadgeCondition condition = badge.condition;
        
        switch (condition.type)
        {
            case BadgeConditionType.TotalSessions:
                return studentData.Count >= condition.targetValue;
                
            case BadgeConditionType.ConsecutiveSessions:
                return CheckConsecutiveSessions(studentData, condition.targetValue);
                
            case BadgeConditionType.EmotionImprovement:
                return studentData.Count(d => d.IsComplete() && d.GetEmotionChange() > 0) >= condition.targetValue;
                
            case BadgeConditionType.SpecificEmotion:
                return studentData.Count(d => d.beforeEmotion == condition.targetEmotion) >= condition.targetValue;
                
            case BadgeConditionType.BigImprovement:
                return studentData.Any(d => d.IsComplete() && d.GetEmotionChange() >= condition.targetValue);
                
            case BadgeConditionType.WeeklyStreak:
                return CheckWeeklyStreak(studentData);
                
            case BadgeConditionType.MonthlyActive:
                return CheckMonthlyActive(studentData);
                
            case BadgeConditionType.ConsistentMood:
                return CheckConsistentMood(studentData, condition.targetValue);
                
            default:
                return false;
        }
    }

    private bool CheckConsecutiveSessions(List<EmotionData> data, int targetDays)
    {
        if (data.Count < targetDays) return false;
        
        var sortedData = data.OrderByDescending(d => d.timestamp).ToList();
        
        for (int i = 0; i < sortedData.Count - targetDays + 1; i++)
        {
            bool isConsecutive = true;
            DateTime baseDate = DateTime.Parse(sortedData[i].timestamp);
            
            for (int j = 1; j < targetDays; j++)
            {
                DateTime checkDate = DateTime.Parse(sortedData[i + j].timestamp);
                if ((baseDate - checkDate).TotalDays > j + 1)
                {
                    isConsecutive = false;
                    break;
                }
            }
            
            if (isConsecutive) return true;
        }
        
        return false;
    }

    private bool CheckWeeklyStreak(List<EmotionData> data)
    {
        DateTime now = DateTime.Now;
        DateTime weekStart = now.AddDays(-(int)now.DayOfWeek);
        DateTime weekEnd = weekStart.AddDays(7);
        
        var weeklyData = data.Where(d => 
        {
            DateTime sessionDate = DateTime.Parse(d.timestamp);
            return sessionDate >= weekStart && sessionDate < weekEnd;
        }).ToList();
        
        return weeklyData.Count >= 5; // 주 5회 이상
    }

    private bool CheckMonthlyActive(List<EmotionData> data)
    {
        DateTime now = DateTime.Now;
        DateTime monthStart = new DateTime(now.Year, now.Month, 1);
        DateTime monthEnd = monthStart.AddMonths(1);
        
        var monthlyData = data.Where(d => 
        {
            DateTime sessionDate = DateTime.Parse(d.timestamp);
            return sessionDate >= monthStart && sessionDate < monthEnd;
        }).ToList();
        
        return monthlyData.Count >= 15; // 월 15회 이상
    }

    private bool CheckConsistentMood(List<EmotionData> data, int targetConsecutive)
    {
        if (data.Count < targetConsecutive) return false;
        
        var completeData = data.Where(d => d.IsComplete())
                               .OrderByDescending(d => d.timestamp)
                               .Take(targetConsecutive)
                               .ToList();
        
        if (completeData.Count < targetConsecutive) return false;
        
        // 모든 세션에서 감정이 개선되거나 안정적인지 확인
        return completeData.All(d => d.GetEmotionChange() >= 0);
    }

    /// <summary>
    /// 학생의 모든 배지 가져오기
    /// </summary>
    public List<Badge> GetStudentBadges(int studentNumber)
    {
        if (studentBadges.ContainsKey(studentNumber))
        {
            return studentBadges[studentNumber];
        }
        
        // 학생 배지가 없으면 기본 배지 세트 생성
        studentBadges[studentNumber] = new List<Badge>();
        foreach (var badge in allBadges)
        {
            Badge studentBadge = new Badge(badge.id, badge.name, badge.description, 
                                          badge.emoji, badge.category, badge.rarity, badge.condition);
            studentBadges[studentNumber].Add(studentBadge);
        }
        
        return studentBadges[studentNumber];
    }

    /// <summary>
    /// 학생의 획득한 배지만 가져오기
    /// </summary>
    public List<Badge> GetEarnedBadges(int studentNumber)
    {
        var badges = GetStudentBadges(studentNumber);
        return badges.Where(b => b.isEarned).ToList();
    }

    /// <summary>
    /// 배지 통계 가져오기
    /// </summary>
    public BadgeStatistics GetBadgeStatistics(int studentNumber)
    {
        var badges = GetStudentBadges(studentNumber);
        var earnedBadges = badges.Where(b => b.isEarned).ToList();
        
        return new BadgeStatistics
        {
            totalBadges = badges.Count,
            earnedBadges = earnedBadges.Count,
            commonBadges = earnedBadges.Count(b => b.rarity == BadgeRarity.Common),
            uncommonBadges = earnedBadges.Count(b => b.rarity == BadgeRarity.Uncommon),
            rareBadges = earnedBadges.Count(b => b.rarity == BadgeRarity.Rare),
            epicBadges = earnedBadges.Count(b => b.rarity == BadgeRarity.Epic),
            legendaryBadges = earnedBadges.Count(b => b.rarity == BadgeRarity.Legendary),
            latestBadge = earnedBadges.OrderByDescending(b => b.earnedDate).FirstOrDefault()
        };
    }

    /// <summary>
    /// 학생 배지 데이터 저장
    /// </summary>
    public void SaveStudentBadges()
    {
        try
        {
            var saveData = new StudentBadgeSaveData();
            saveData.studentBadges = new List<StudentBadgeData>();
            
            foreach (var kvp in studentBadges)
            {
                var studentBadgeData = new StudentBadgeData
                {
                    studentNumber = kvp.Key,
                    badges = kvp.Value
                };
                saveData.studentBadges.Add(studentBadgeData);
            }
            
            string json = JsonUtility.ToJson(saveData, true);
            PlayerPrefs.SetString(STUDENT_BADGES_KEY, json);
            PlayerPrefs.Save();
            
            if (enableDebugMode)
            {
                Debug.Log("학생 배지 데이터 저장 완료");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"배지 데이터 저장 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 학생 배지 데이터 불러오기
    /// </summary>
    public void LoadStudentBadges()
    {
        try
        {
            studentBadges = new Dictionary<int, List<Badge>>();
            
            string json = PlayerPrefs.GetString(STUDENT_BADGES_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                StudentBadgeSaveData saveData = JsonUtility.FromJson<StudentBadgeSaveData>(json);
                
                if (saveData.studentBadges != null)
                {
                    foreach (var studentData in saveData.studentBadges)
                    {
                        studentBadges[studentData.studentNumber] = studentData.badges;
                    }
                }
            }
            
            if (enableDebugMode)
            {
                Debug.Log($"학생 배지 데이터 불러오기 완료: {studentBadges.Count}명");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"배지 데이터 불러오기 실패: {e.Message}");
            studentBadges = new Dictionary<int, List<Badge>>();
        }
    }

    /// <summary>
    /// 모든 배지 데이터 초기화
    /// </summary>
    public void ResetAllBadges()
    {
        studentBadges.Clear();
        PlayerPrefs.DeleteKey(STUDENT_BADGES_KEY);
        PlayerPrefs.Save();
        
        Debug.Log("모든 배지 데이터가 초기화되었습니다.");
    }

    private void OnDestroy()
    {
        SaveStudentBadges();
        
        if (EmotionManager.Instance != null)
        {
            EmotionManager.Instance.OnEmotionDataSubmitted -= OnEmotionDataSubmitted;
        }
    }
}

/// <summary>
/// 배지 통계
/// </summary>
[Serializable]
public class BadgeStatistics
{
    public int totalBadges;
    public int earnedBadges;
    public int commonBadges;
    public int uncommonBadges;
    public int rareBadges;
    public int epicBadges;
    public int legendaryBadges;
    public Badge latestBadge;
    
    public float CompletionPercentage => totalBadges > 0 ? (float)earnedBadges / totalBadges * 100f : 0f;
}

/// <summary>
/// 배지 저장 데이터 구조
/// </summary>
[Serializable]
public class StudentBadgeSaveData
{
    public List<StudentBadgeData> studentBadges;
}

[Serializable]
public class StudentBadgeData
{
    public int studentNumber;
    public List<Badge> badges;
}

/// <summary>
/// 배지 아이템 UI 컴포넌트
/// </summary>
public class BadgeItem : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image badgeImage;
    [SerializeField] private TextMeshProUGUI badgeNameText;
    [SerializeField] private TextMeshProUGUI badgeEmojiText;
    [SerializeField] private TextMeshProUGUI badgeDescriptionText;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private UnityEngine.UI.Image rarityBorder;

    // 레어도별 색상
    private readonly Dictionary<BadgeRarity, Color> rarityColors = new Dictionary<BadgeRarity, Color>
    {
        { BadgeRarity.Common, Color.gray },
        { BadgeRarity.Uncommon, Color.green },
        { BadgeRarity.Rare, Color.blue },
        { BadgeRarity.Epic, new Color(0.6f, 0f, 0.8f) },
        { BadgeRarity.Legendary, new Color(1f, 0.8f, 0f) }
    };

    public void Setup(Badge badge)
    {
        badgeNameText.text = badge.name;
        badgeEmojiText.text = badge.emoji;
        badgeDescriptionText.text = badge.description;
        
        // 레어도 테두리 색상
        if (rarityBorder != null && rarityColors.ContainsKey(badge.rarity))
        {
            rarityBorder.color = rarityColors[badge.rarity];
        }
        
        // 획득 여부에 따른 표시
        if (badge.isEarned)
        {
            lockOverlay.SetActive(false);
            badgeImage.color = Color.white;
            
            // 획득 효과
            StartCoroutine(EarnedEffect());
        }
        else
        {
            lockOverlay.SetActive(true);
            badgeImage.color = Color.gray;
        }
    }

    private IEnumerator EarnedEffect()
    {
        // 간단한 반짝이는 효과
        for (int i = 0; i < 3; i++)
        {
            badgeImage.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            badgeImage.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
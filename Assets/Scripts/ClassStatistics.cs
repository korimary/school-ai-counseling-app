using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클래스 전체 감정 통계 데이터
/// </summary>
[System.Serializable]
public class ClassStatistics
{
    [Header("Basic Info")]
    public string classCode;
    public string dateRange;
    public int totalStudents;
    public int activeStudents;
    public int totalSessions;
    public int completeSessions;
    
    [Header("Emotion Statistics")]
    public Dictionary<string, int> emotionCounts;
    public Dictionary<string, float> emotionPercentages;
    public Dictionary<int, int> intensityDistribution;
    public Dictionary<string, float> emotionImprovements;
    
    [Header("Improvement Metrics")]
    public float averageImprovement;
    public float overallImprovement; // UI에서 사용하는 명칭
    public int improvementCount;
    public int worsedCount;
    public int stableCount;
    
    [Header("Trend Data")]
    public List<DailyEmotionData> dailyTrends;
    public float weeklyTrend;
    public float monthlyTrend;
    
    [Header("Alert Metrics")]
    public int highRiskStudents; // 지속적으로 부정 감정을 보이는 학생
    public int improvingStudents; // 개선되고 있는 학생
    public List<string> concernedStudents; // 주의 깊게 봐야 할 학생 목록
    
    /// <summary>
    /// 기본 생성자
    /// </summary>
    public ClassStatistics()
    {
        classCode = "";
        dateRange = "";
        totalStudents = 0;
        activeStudents = 0;
        totalSessions = 0;
        completeSessions = 0;
        
        emotionCounts = new Dictionary<string, int>();
        emotionPercentages = new Dictionary<string, float>();
        intensityDistribution = new Dictionary<int, int>();
        emotionImprovements = new Dictionary<string, float>();
        
        averageImprovement = 0f;
        overallImprovement = 0f;
        improvementCount = 0;
        worsedCount = 0;
        stableCount = 0;
        
        dailyTrends = new List<DailyEmotionData>();
        weeklyTrend = 0f;
        monthlyTrend = 0f;
        
        highRiskStudents = 0;
        improvingStudents = 0;
        concernedStudents = new List<string>();
    }
    
    /// <summary>
    /// EmotionData 리스트로부터 통계 생성
    /// </summary>
    public ClassStatistics(string classCode, List<EmotionData> emotionDataList)
    {
        this.classCode = classCode;
        this.emotionCounts = new Dictionary<string, int>();
        this.emotionPercentages = new Dictionary<string, float>();
        this.intensityDistribution = new Dictionary<int, int>();
        this.emotionImprovements = new Dictionary<string, float>();
        this.dailyTrends = new List<DailyEmotionData>();
        this.concernedStudents = new List<string>();
        
        CalculateStatistics(emotionDataList);
    }
    
    /// <summary>
    /// 감정 데이터로부터 통계 계산
    /// </summary>
    public void CalculateStatistics(List<EmotionData> emotionDataList)
    {
        if (emotionDataList == null || emotionDataList.Count == 0)
        {
            Debug.LogWarning("EmotionData list is empty or null");
            return;
        }
        
        // 기본 정보 초기화
        HashSet<string> uniqueStudents = new HashSet<string>();
        HashSet<string> activeStudentSet = new HashSet<string>();
        
        // 감정 카운트 초기화
        emotionCounts.Clear();
        intensityDistribution.Clear();
        emotionImprovements.Clear();
        
        float totalImprovementSum = 0f;
        int improvementDataCount = 0;
        
        // 세션 카운트
        totalSessions = emotionDataList.Count;
        completeSessions = 0;
        
        foreach (var data in emotionDataList)
        {
            // 학생 수 계산
            uniqueStudents.Add(data.studentNumber);
            
            if (data.IsComplete())
            {
                activeStudentSet.Add(data.studentNumber);
                completeSessions++;
                
                // 감정 개선도 계산
                float improvement = data.GetEmotionChange();
                totalImprovementSum += improvement;
                improvementDataCount++;
                
                if (improvement > 0.5f)
                    improvementCount++;
                else if (improvement < -0.5f)
                    worsedCount++;
                else
                    stableCount++;
                
                // 감정별 개선도 계산
                if (!string.IsNullOrEmpty(data.beforeEmotion))
                {
                    if (emotionImprovements.ContainsKey(data.beforeEmotion))
                        emotionImprovements[data.beforeEmotion] += improvement;
                    else
                        emotionImprovements[data.beforeEmotion] = improvement;
                }
            }
            
            // 체크인 감정 카운트
            if (!string.IsNullOrEmpty(data.beforeEmotion))
            {
                CountEmotion(data.beforeEmotion);
                CountIntensity(data.beforeIntensity);
            }
            
            // 체크아웃 감정 카운트
            if (!string.IsNullOrEmpty(data.afterEmotion))
            {
                CountEmotion(data.afterEmotion);
                CountIntensity(data.afterIntensity);
            }
        }
        
        // 통계 계산
        totalStudents = uniqueStudents.Count;
        activeStudents = activeStudentSet.Count;
        averageImprovement = improvementDataCount > 0 ? totalImprovementSum / improvementDataCount : 0f;
        overallImprovement = averageImprovement; // UI 호환성
        
        // 감정별 개선도 평균 계산
        var emotionKeys = new List<string>(emotionImprovements.Keys);
        foreach (var emotion in emotionKeys)
        {
            int emotionCount = emotionCounts.ContainsKey(emotion) ? emotionCounts[emotion] : 1;
            emotionImprovements[emotion] = emotionImprovements[emotion] / emotionCount;
        }
        
        // 퍼센티지 계산
        CalculatePercentages();
        
        // 주의 학생 식별
        IdentifyConcernedStudents(emotionDataList);
        
        // 날짜 범위 설정
        if (emotionDataList.Count > 0)
        {
            dateRange = $"{GetEarliestDate(emotionDataList)} ~ {GetLatestDate(emotionDataList)}";
        }
    }
    
    private void CountEmotion(string emotion)
    {
        if (emotionCounts.ContainsKey(emotion))
            emotionCounts[emotion]++;
        else
            emotionCounts[emotion] = 1;
    }
    
    private void CountIntensity(int intensity)
    {
        if (intensityDistribution.ContainsKey(intensity))
            intensityDistribution[intensity]++;
        else
            intensityDistribution[intensity] = 1;
    }
    
    private void CalculatePercentages()
    {
        emotionPercentages.Clear();
        int totalEmotions = 0;
        
        foreach (var count in emotionCounts.Values)
        {
            totalEmotions += count;
        }
        
        if (totalEmotions > 0)
        {
            foreach (var kvp in emotionCounts)
            {
                emotionPercentages[kvp.Key] = (float)kvp.Value / totalEmotions * 100f;
            }
        }
    }
    
    private void IdentifyConcernedStudents(List<EmotionData> emotionDataList)
    {
        Dictionary<string, List<float>> studentImprovements = new Dictionary<string, List<float>>();
        
        foreach (var data in emotionDataList)
        {
            if (data.IsComplete())
            {
                if (!studentImprovements.ContainsKey(data.studentNumber))
                    studentImprovements[data.studentNumber] = new List<float>();
                
                studentImprovements[data.studentNumber].Add(data.GetEmotionChange());
            }
        }
        
        // 지속적으로 부정적인 학생들 식별
        foreach (var kvp in studentImprovements)
        {
            if (kvp.Value.Count >= 3) // 최소 3번의 데이터
            {
                float avgImprovement = 0f;
                foreach (float improvement in kvp.Value)
                    avgImprovement += improvement;
                avgImprovement /= kvp.Value.Count;
                
                if (avgImprovement < -1f) // 지속적으로 악화
                {
                    highRiskStudents++;
                    concernedStudents.Add(kvp.Key);
                }
                else if (avgImprovement > 1f) // 지속적으로 개선
                {
                    improvingStudents++;
                }
            }
        }
    }
    
    private string GetEarliestDate(List<EmotionData> dataList)
    {
        DateTime earliest = DateTime.MaxValue;
        foreach (var data in dataList)
        {
            if (DateTime.TryParse(data.timestamp, out DateTime date))
            {
                if (date < earliest)
                    earliest = date;
            }
        }
        return earliest == DateTime.MaxValue ? "Unknown" : earliest.ToString("yyyy-MM-dd");
    }
    
    private string GetLatestDate(List<EmotionData> dataList)
    {
        DateTime latest = DateTime.MinValue;
        foreach (var data in dataList)
        {
            if (DateTime.TryParse(data.timestamp, out DateTime date))
            {
                if (date > latest)
                    latest = date;
            }
        }
        return latest == DateTime.MinValue ? "Unknown" : latest.ToString("yyyy-MM-dd");
    }
    
    /// <summary>
    /// 가장 많이 나타나는 감정 반환
    /// </summary>
    public string GetMostCommonEmotion()
    {
        string mostCommon = "";
        int maxCount = 0;
        
        foreach (var kvp in emotionCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mostCommon = kvp.Key;
            }
        }
        
        return mostCommon;
    }
    
    /// <summary>
    /// 개선 비율 반환 (0-100%)
    /// </summary>
    public float GetImprovementRate()
    {
        int totalStudentsWithData = improvementCount + worsedCount + stableCount;
        if (totalStudentsWithData == 0) return 0f;
        
        return (float)improvementCount / totalStudentsWithData * 100f;
    }
    
    /// <summary>
    /// JSON 변환
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
    
    /// <summary>
    /// JSON에서 생성
    /// </summary>
    public static ClassStatistics FromJson(string json)
    {
        try
        {
            return JsonUtility.FromJson<ClassStatistics>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing ClassStatistics from JSON: {e.Message}");
            return new ClassStatistics();
        }
    }
}

/// <summary>
/// 일별 감정 데이터
/// </summary>
[System.Serializable]
public class DailyEmotionData
{
    public string date;
    public float averagePositivity;
    public int totalCheckins;
    public Dictionary<string, int> emotionBreakdown;
    
    public DailyEmotionData()
    {
        date = "";
        averagePositivity = 0f;
        totalCheckins = 0;
        emotionBreakdown = new Dictionary<string, int>();
    }
    
    public DailyEmotionData(string date, float averagePositivity, int totalCheckins)
    {
        this.date = date;
        this.averagePositivity = averagePositivity;
        this.totalCheckins = totalCheckins;
        this.emotionBreakdown = new Dictionary<string, int>();
    }
}
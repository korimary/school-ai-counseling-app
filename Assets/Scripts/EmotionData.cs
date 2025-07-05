using System;
using UnityEngine;

[System.Serializable]
public class EmotionData
{
    [Header("Required Fields")]
    public string timestamp;
    public string classCode;
    public string studentNumber; // 호환성을 위해 string 유지, int로 변환하는 속성 추가
    public string beforeEmotion;
    public string afterEmotion;
    
    [Header("Emotion Intensity")]
    [Range(1, 5)]
    public int emotionIntensity = 1; // 기존 호환성
    [Range(1, 5)]
    public int beforeIntensity = 1;  // 체크인 시 감정 강도
    [Range(1, 5)]
    public int afterIntensity = 1;   // 체크아웃 시 감정 강도
    
    [Header("Optional Fields")]
    public string keywords;
    public string todayResolution;
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public EmotionData()
    {
        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        classCode = "";
        studentNumber = "";
        beforeEmotion = "";
        afterEmotion = "";
        emotionIntensity = 1;
        beforeIntensity = 1;
        afterIntensity = 1;
        keywords = "";
        todayResolution = "";
    }
    
    /// <summary>
    /// Constructor with required parameters
    /// </summary>
    public EmotionData(string classCode, string studentNumber, string beforeEmotion, int emotionIntensity)
    {
        this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.classCode = classCode;
        this.studentNumber = studentNumber;
        this.beforeEmotion = beforeEmotion;
        this.afterEmotion = "";
        this.emotionIntensity = Mathf.Clamp(emotionIntensity, 1, 5);
        this.beforeIntensity = this.emotionIntensity; // 동기화
        this.afterIntensity = 1;
        this.keywords = "";
        this.todayResolution = "";
    }
    
    /// <summary>
    /// Full constructor
    /// </summary>
    public EmotionData(string classCode, string studentNumber, string beforeEmotion, string afterEmotion, 
                      int emotionIntensity, string keywords = "", string todayResolution = "")
    {
        this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.classCode = classCode;
        this.studentNumber = studentNumber;
        this.beforeEmotion = beforeEmotion;
        this.afterEmotion = afterEmotion;
        this.emotionIntensity = Mathf.Clamp(emotionIntensity, 1, 5);
        this.beforeIntensity = this.emotionIntensity; // 동기화
        this.afterIntensity = this.emotionIntensity; // 동기화
        this.keywords = keywords ?? "";
        this.todayResolution = todayResolution ?? "";
    }
    
    /// <summary>
    /// Copy constructor
    /// </summary>
    public EmotionData(EmotionData other)
    {
        this.timestamp = other.timestamp;
        this.classCode = other.classCode;
        this.studentNumber = other.studentNumber;
        this.beforeEmotion = other.beforeEmotion;
        this.afterEmotion = other.afterEmotion;
        this.emotionIntensity = other.emotionIntensity;
        this.beforeIntensity = other.beforeIntensity;
        this.afterIntensity = other.afterIntensity;
        this.keywords = other.keywords;
        this.todayResolution = other.todayResolution;
    }
    
    /// <summary>
    /// Update timestamp to current time
    /// </summary>
    public void UpdateTimestamp()
    {
        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// Update timestamp with custom format
    /// </summary>
    public void UpdateTimestamp(string format)
    {
        timestamp = DateTime.Now.ToString(format);
    }
    
    /// <summary>
    /// Set emotion intensity with validation
    /// </summary>
    public void SetEmotionIntensity(int intensity)
    {
        emotionIntensity = Mathf.Clamp(intensity, 1, 5);
    }
    
    /// <summary>
    /// Check if required fields are filled
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(classCode) && 
               !string.IsNullOrEmpty(studentNumber) && 
               !string.IsNullOrEmpty(beforeEmotion) &&
               emotionIntensity >= 1 && emotionIntensity <= 5;
    }
    
    /// <summary>
    /// Check if this is a complete emotion check (has both before and after emotions)
    /// </summary>
    public bool IsCompleteCheck()
    {
        return IsValid() && !string.IsNullOrEmpty(afterEmotion);
    }
    
    /// <summary>
    /// Get emotion intensity as text
    /// </summary>
    public string GetEmotionIntensityText()
    {
        switch (emotionIntensity)
        {
            case 1: return "매우 낮음"; // Very Low
            case 2: return "낮음";     // Low
            case 3: return "보통";     // Medium
            case 4: return "높음";     // High
            case 5: return "매우 높음"; // Very High
            default: return "보통";
        }
    }
    
    /// <summary>
    /// Get emotion intensity as text (English)
    /// </summary>
    public string GetEmotionIntensityTextEN()
    {
        switch (emotionIntensity)
        {
            case 1: return "Very Low";
            case 2: return "Low";
            case 3: return "Medium";
            case 4: return "High";
            case 5: return "Very High";
            default: return "Medium";
        }
    }
    
    /// <summary>
    /// Convert to JSON string
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
    
    /// <summary>
    /// Create from JSON string
    /// </summary>
    public static EmotionData FromJson(string json)
    {
        try
        {
            return JsonUtility.FromJson<EmotionData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing EmotionData from JSON: {e.Message}");
            return new EmotionData();
        }
    }
    
    /// <summary>
    /// Get a summary string of the emotion data
    /// </summary>
    public string GetSummary()
    {
        string summary = $"[{timestamp}] {classCode}-{studentNumber}: {beforeEmotion}";
        if (!string.IsNullOrEmpty(afterEmotion))
        {
            summary += $" → {afterEmotion}";
        }
        summary += $" (강도: {emotionIntensity})";
        
        if (!string.IsNullOrEmpty(keywords))
        {
            summary += $" | 키워드: {keywords}";
        }
        
        return summary;
    }
    
    /// <summary>
    /// Clear all data
    /// </summary>
    public void Clear()
    {
        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        classCode = "";
        studentNumber = "";
        beforeEmotion = "";
        afterEmotion = "";
        emotionIntensity = 1;
        beforeIntensity = 1;
        afterIntensity = 1;
        keywords = "";
        todayResolution = "";
    }
    
    /// <summary>
    /// Clone this emotion data
    /// </summary>
    public EmotionData Clone()
    {
        return new EmotionData(this);
    }
    
    // ============================================
    // 호환성 및 확장 메서드들
    // ============================================
    
    /// <summary>
    /// StudentNumber를 int로 가져오기 (호환성)
    /// </summary>
    public int GetStudentNumberAsInt()
    {
        if (int.TryParse(studentNumber, out int result))
            return result;
        return 0;
    }
    
    /// <summary>
    /// StudentNumber를 int로 설정하기 (호환성)
    /// </summary>
    public void SetStudentNumber(int number)
    {
        studentNumber = number.ToString();
    }
    
    /// <summary>
    /// 체크아웃 시 감정 데이터 업데이트
    /// </summary>
    public void UpdateAfterEmotion(string afterEmotion, int afterIntensity, string keywords = "", string todayResolution = "")
    {
        this.afterEmotion = afterEmotion ?? "";
        this.afterIntensity = Mathf.Clamp(afterIntensity, 1, 5);
        
        if (!string.IsNullOrEmpty(keywords))
            this.keywords = keywords;
            
        if (!string.IsNullOrEmpty(todayResolution))
            this.todayResolution = todayResolution;
            
        UpdateTimestamp();
    }
    
    /// <summary>
    /// 완료된 감정 체크인지 확인 (체크인과 체크아웃 모두 완료)
    /// </summary>
    public bool IsComplete()
    {
        return IsValid() && 
               !string.IsNullOrEmpty(afterEmotion) && 
               afterIntensity >= 1 && afterIntensity <= 5;
    }
    
    /// <summary>
    /// 감정 변화량 계산 (양수면 개선, 음수면 악화)
    /// </summary>
    public float GetEmotionChange()
    {
        if (!IsComplete())
            return 0f;
            
        // 긍정적 감정의 경우 강도 증가가 좋음
        if (EmotionTypes.IsPositiveEmotion(beforeEmotion) && EmotionTypes.IsPositiveEmotion(afterEmotion))
        {
            return afterIntensity - beforeIntensity; // 긍정 감정 강도 증가
        }
        // 부정적 감정의 경우 강도 감소가 좋음
        else if (EmotionTypes.IsNegativeEmotion(beforeEmotion) && EmotionTypes.IsNegativeEmotion(afterEmotion))
        {
            return beforeIntensity - afterIntensity; // 부정 감정 강도 감소
        }
        // 부정에서 긍정으로 변화
        else if (EmotionTypes.IsNegativeEmotion(beforeEmotion) && EmotionTypes.IsPositiveEmotion(afterEmotion))
        {
            return beforeIntensity + afterIntensity; // 큰 개선
        }
        // 긍정에서 부정으로 변화  
        else if (EmotionTypes.IsPositiveEmotion(beforeEmotion) && EmotionTypes.IsNegativeEmotion(afterEmotion))
        {
            return -(beforeIntensity + afterIntensity); // 큰 악화
        }
        
        return 0f; // 중립적 변화
    }
    
    /// <summary>
    /// 감정 변화 상태를 텍스트로 반환
    /// </summary>
    public string GetEmotionChangeStatus()
    {
        if (!IsComplete())
            return "미완료";
            
        float change = GetEmotionChange();
        
        if (change > 2f)
            return "크게 개선됨";
        else if (change > 0f)
            return "개선됨";
        else if (change == 0f)
            return "변화 없음";
        else if (change > -2f)
            return "약간 악화됨";
        else
            return "크게 악화됨";
    }
    
    /// <summary>
    /// 기존 emotionIntensity와 beforeIntensity 동기화
    /// </summary>
    public void SyncIntensities()
    {
        if (beforeIntensity == 1 && emotionIntensity != 1)
        {
            beforeIntensity = emotionIntensity; // 기존 데이터 호환성
        }
        else if (emotionIntensity == 1 && beforeIntensity != 1)
        {
            emotionIntensity = beforeIntensity; // 새 필드를 기존 필드로 동기화
        }
    }
}

/// <summary>
/// Predefined emotion types for easy selection
/// </summary>
public static class EmotionTypes
{
    public static readonly string[] PositiveEmotions = {
        "기쁨", "행복", "만족", "감사", "희망", "자신감", "평온", "사랑", "즐거움", "신남"
    };
    
    public static readonly string[] NegativeEmotions = {
        "슬픔", "화남", "불안", "걱정", "스트레스", "피곤", "우울", "짜증", "실망", "두려움"
    };
    
    public static readonly string[] NeutralEmotions = {
        "보통", "평범", "무감정", "담담", "조용함"
    };
    
    /// <summary>
    /// Get all emotion types
    /// </summary>
    public static string[] GetAllEmotions()
    {
        var allEmotions = new string[PositiveEmotions.Length + NegativeEmotions.Length + NeutralEmotions.Length];
        PositiveEmotions.CopyTo(allEmotions, 0);
        NegativeEmotions.CopyTo(allEmotions, PositiveEmotions.Length);
        NeutralEmotions.CopyTo(allEmotions, PositiveEmotions.Length + NegativeEmotions.Length);
        return allEmotions;
    }
    
    /// <summary>
    /// Check if emotion is positive
    /// </summary>
    public static bool IsPositiveEmotion(string emotion)
    {
        return Array.IndexOf(PositiveEmotions, emotion) >= 0;
    }
    
    /// <summary>
    /// Check if emotion is negative
    /// </summary>
    public static bool IsNegativeEmotion(string emotion)
    {
        return Array.IndexOf(NegativeEmotions, emotion) >= 0;
    }
    
    /// <summary>
    /// Check if emotion is neutral
    /// </summary>
    public static bool IsNeutralEmotion(string emotion)
    {
        return Array.IndexOf(NeutralEmotions, emotion) >= 0;
    }
    
    /// <summary>
    /// Get emotion index for storage
    /// </summary>
    public static int GetEmotionIndex(string emotion)
    {
        if (string.IsNullOrEmpty(emotion))
            return -1;
            
        var allEmotions = GetAllEmotions();
        return Array.IndexOf(allEmotions, emotion);
    }
    
    /// <summary>
    /// Get emotion by index
    /// </summary>
    public static string GetEmotionByIndex(int index)
    {
        var allEmotions = GetAllEmotions();
        if (index >= 0 && index < allEmotions.Length)
            return allEmotions[index];
        return "";
    }
}
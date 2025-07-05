using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class EmotionManager : MonoBehaviour
{
    [Header("Configuration")]
    public string localStoragePath = "EmotionData";
    public bool enableLocalStorage = true;
    public bool enableAutoSubmission = true;
    public float autoSubmissionInterval = 300f; // 5 minutes
    
    [Header("Current Session")]
    public EmotionData currentEmotionData;
    
    private List<EmotionData> emotionDataHistory = new List<EmotionData>();
    private List<EmotionData> pendingSubmissions = new List<EmotionData>();
    
    private static EmotionManager instance;
    public static EmotionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EmotionManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EmotionManager");
                    instance = go.AddComponent<EmotionManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events
    public event System.Action<EmotionData> OnEmotionDataCreated;
    public event System.Action<EmotionData> OnEmotionDataUpdated;
    public event System.Action<EmotionData, bool> OnEmotionDataSubmitted;
    public event System.Action<List<EmotionData>> OnEmotionHistoryLoaded;
    public event System.Action<bool> OnDataLoadComplete;
    public event System.Action<string> OnDataLoadError;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadEmotionHistory();
        
        if (enableAutoSubmission)
        {
            StartCoroutine(AutoSubmissionCoroutine());
        }
    }
    
    private void Initialize()
    {
        currentEmotionData = new EmotionData();
        
        // Setup storage path
        if (enableLocalStorage)
        {
            string fullPath = Path.Combine(Application.persistentDataPath, localStoragePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }
    }
    
    /// <summary>
    /// Create new emotion data
    /// </summary>
    public EmotionData CreateEmotionData(string classCode, string studentNumber, string beforeEmotion, int emotionIntensity)
    {
        EmotionData newData = new EmotionData(classCode, studentNumber, beforeEmotion, emotionIntensity);
        
        if (ValidateEmotionData(newData))
        {
            currentEmotionData = newData;
            emotionDataHistory.Add(newData);
            
            OnEmotionDataCreated?.Invoke(newData);
            
            if (enableLocalStorage)
            {
                SaveEmotionDataLocally(newData);
            }
            
            return newData;
        }
        
        Debug.LogError("Failed to create emotion data - validation failed");
        return null;
    }
    
    /// <summary>
    /// Update current emotion data
    /// </summary>
    public bool UpdateCurrentEmotionData(string afterEmotion = null, string keywords = null, string todayResolution = null)
    {
        if (currentEmotionData == null)
        {
            Debug.LogError("No current emotion data to update");
            return false;
        }
        
        if (!string.IsNullOrEmpty(afterEmotion))
        {
            currentEmotionData.afterEmotion = afterEmotion;
        }
        
        if (!string.IsNullOrEmpty(keywords))
        {
            currentEmotionData.keywords = keywords;
        }
        
        if (!string.IsNullOrEmpty(todayResolution))
        {
            currentEmotionData.todayResolution = todayResolution;
        }
        
        currentEmotionData.UpdateTimestamp();
        
        OnEmotionDataUpdated?.Invoke(currentEmotionData);
        
        if (enableLocalStorage)
        {
            SaveEmotionDataLocally(currentEmotionData);
        }
        
        return true;
    }
    
    /// <summary>
    /// Submit emotion data to Google Sheets
    /// </summary>
    public void SubmitEmotionData(EmotionData emotionData = null, bool addToPending = true)
    {
        EmotionData dataToSubmit = emotionData ?? currentEmotionData;
        
        if (dataToSubmit == null)
        {
            Debug.LogError("No emotion data to submit");
            return;
        }
        
        if (!ValidateEmotionData(dataToSubmit))
        {
            Debug.LogError("Invalid emotion data - cannot submit");
            return;
        }
        
        if (addToPending)
        {
            pendingSubmissions.Add(dataToSubmit);
        }
        
        GoogleSheetsManager.Instance.SendEmotionData(dataToSubmit, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"Emotion data submitted successfully: {message}");
                if (addToPending)
                {
                    pendingSubmissions.Remove(dataToSubmit);
                }
            }
            else
            {
                Debug.LogError($"Failed to submit emotion data: {message}");
            }
            
            OnEmotionDataSubmitted?.Invoke(dataToSubmit, success);
        });
    }
    
    /// <summary>
    /// Submit all pending emotion data
    /// </summary>
    public void SubmitAllPendingData()
    {
        if (pendingSubmissions.Count == 0)
        {
            Debug.Log("No pending submissions to process");
            return;
        }
        
        List<EmotionData> dataToSubmit = new List<EmotionData>(pendingSubmissions);
        
        GoogleSheetsManager.Instance.SendBatchEmotionData(dataToSubmit, 
            (current, total) =>
            {
                Debug.Log($"Submitting emotion data: {current}/{total}");
            },
            (success) =>
            {
                if (success)
                {
                    Debug.Log("All pending emotion data submitted successfully");
                    pendingSubmissions.Clear();
                }
                else
                {
                    Debug.LogError("Some emotion data failed to submit");
                }
            });
    }
    
    /// <summary>
    /// Validate emotion data
    /// </summary>
    public bool ValidateEmotionData(EmotionData data)
    {
        if (data == null)
        {
            Debug.LogError("Emotion data is null");
            return false;
        }
        
        if (string.IsNullOrEmpty(data.classCode))
        {
            Debug.LogError("Class code is required");
            return false;
        }
        
        if (string.IsNullOrEmpty(data.studentNumber))
        {
            Debug.LogError("Student number is required");
            return false;
        }
        
        if (string.IsNullOrEmpty(data.beforeEmotion))
        {
            Debug.LogError("Before emotion is required");
            return false;
        }
        
        if (data.emotionIntensity < 1 || data.emotionIntensity > 5)
        {
            Debug.LogError("Emotion intensity must be between 1 and 5");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Save emotion data locally
    /// </summary>
    private void SaveEmotionDataLocally(EmotionData data)
    {
        if (!enableLocalStorage || data == null)
            return;
        
        try
        {
            string fileName = $"emotion_{data.timestamp.Replace(":", "-").Replace(" ", "_")}.json";
            string fullPath = Path.Combine(Application.persistentDataPath, localStoragePath, fileName);
            
            string json = data.ToJson();
            File.WriteAllText(fullPath, json);
            
            Debug.Log($"Emotion data saved locally: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving emotion data locally: {e.Message}");
        }
    }
    
    /// <summary>
    /// Load emotion history from local storage
    /// </summary>
    private void LoadEmotionHistory()
    {
        if (!enableLocalStorage)
            return;
        
        try
        {
            string fullPath = Path.Combine(Application.persistentDataPath, localStoragePath);
            if (!Directory.Exists(fullPath))
                return;
            
            string[] files = Directory.GetFiles(fullPath, "emotion_*.json");
            emotionDataHistory.Clear();
            
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                EmotionData data = EmotionData.FromJson(json);
                if (data != null && data.IsValid())
                {
                    emotionDataHistory.Add(data);
                }
            }
            
            // Sort by timestamp
            emotionDataHistory.Sort((a, b) => DateTime.Compare(
                DateTime.Parse(a.timestamp), DateTime.Parse(b.timestamp)));
            
            Debug.Log($"Loaded {emotionDataHistory.Count} emotion data entries from local storage");
            OnEmotionHistoryLoaded?.Invoke(emotionDataHistory);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading emotion history: {e.Message}");
        }
    }
    
    /// <summary>
    /// Get emotion history
    /// </summary>
    public List<EmotionData> GetEmotionHistory()
    {
        return new List<EmotionData>(emotionDataHistory);
    }
    
    /// <summary>
    /// Get emotion history for specific student
    /// </summary>
    public List<EmotionData> GetEmotionHistory(string classCode, string studentNumber)
    {
        List<EmotionData> studentHistory = new List<EmotionData>();
        
        foreach (var data in emotionDataHistory)
        {
            if (data.classCode == classCode && data.studentNumber == studentNumber)
            {
                studentHistory.Add(data);
            }
        }
        
        return studentHistory;
    }
    
    /// <summary>
    /// Get emotion history for specific date
    /// </summary>
    public List<EmotionData> GetEmotionHistoryByDate(DateTime date)
    {
        List<EmotionData> dateHistory = new List<EmotionData>();
        
        foreach (var data in emotionDataHistory)
        {
            if (DateTime.TryParse(data.timestamp, out DateTime dataDate))
            {
                if (dataDate.Date == date.Date)
                {
                    dateHistory.Add(data);
                }
            }
        }
        
        return dateHistory;
    }
    
    /// <summary>
    /// Clear all emotion data
    /// </summary>
    public void ClearAllData()
    {
        emotionDataHistory.Clear();
        pendingSubmissions.Clear();
        currentEmotionData = new EmotionData();
        
        if (enableLocalStorage)
        {
            try
            {
                string fullPath = Path.Combine(Application.persistentDataPath, localStoragePath);
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                    Directory.CreateDirectory(fullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error clearing local data: {e.Message}");
            }
        }
        
        Debug.Log("All emotion data cleared");
    }
    
    /// <summary>
    /// Auto submission coroutine
    /// </summary>
    private IEnumerator AutoSubmissionCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSubmissionInterval);
            
            if (pendingSubmissions.Count > 0)
            {
                Debug.Log("Auto-submitting pending emotion data");
                SubmitAllPendingData();
            }
        }
    }
    
    /// <summary>
    /// Get statistics for emotion data
    /// </summary>
    public EmotionStatistics GetEmotionStatistics()
    {
        return new EmotionStatistics(emotionDataHistory);
    }
    
    /// <summary>
    /// Start new emotion session
    /// </summary>
    public void StartNewSession(string classCode, string studentNumber)
    {
        currentEmotionData = new EmotionData();
        currentEmotionData.classCode = classCode;
        currentEmotionData.studentNumber = studentNumber;
        currentEmotionData.UpdateTimestamp();
        
        Debug.Log($"Started new emotion session for {classCode}-{studentNumber}");
    }
    
    /// <summary>
    /// Get current session data
    /// </summary>
    public EmotionData GetCurrentSessionData()
    {
        return currentEmotionData;
    }
    
    /// <summary>
    /// End current emotion session
    /// </summary>
    public void EndCurrentSession()
    {
        if (currentEmotionData != null && currentEmotionData.IsValid())
        {
            SubmitEmotionData(currentEmotionData);
        }
        
        currentEmotionData = new EmotionData();
    }
    
    /// <summary>
    /// Load local data from persistent storage
    /// </summary>
    public void LoadLocalData()
    {
        try
        {
            LoadEmotionHistory();
            OnDataLoadComplete?.Invoke(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load local data: {e.Message}");
            OnDataLoadError?.Invoke(e.Message);
            OnDataLoadComplete?.Invoke(false);
        }
    }
    
    /// <summary>
    /// Get emotion data for specific student
    /// </summary>
    public List<EmotionData> GetStudentEmotionData(string classCode, int studentNumber)
    {
        List<EmotionData> studentData = new List<EmotionData>();
        
        foreach (var data in emotionDataHistory)
        {
            if (data.classCode == classCode && data.GetStudentNumberAsInt() == studentNumber)
            {
                studentData.Add(data);
            }
        }
        
        return studentData;
    }
    
    /// <summary>
    /// Check if data exists for specific student
    /// </summary>
    public bool HasDataForStudent(string classCode, int studentNumber)
    {
        foreach (var data in emotionDataHistory)
        {
            if (data.classCode == classCode && data.GetStudentNumberAsInt() == studentNumber)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Get emotion data for specific student (overload for string studentNumber)
    /// </summary>
    public List<EmotionData> GetStudentEmotionData(string studentNumber)
    {
        List<EmotionData> studentData = new List<EmotionData>();
        
        foreach (var data in emotionDataHistory)
        {
            if (data.studentNumber == studentNumber)
            {
                studentData.Add(data);
            }
        }
        
        return studentData;
    }
    
    /// <summary>
    /// Get pending submission count
    /// </summary>
    public int GetPendingSubmissionCount()
    {
        return pendingSubmissions.Count;
    }
    
    /// <summary>
    /// Clear all local data
    /// </summary>
    public void ClearAllLocalData()
    {
        emotionDataHistory.Clear();
        pendingSubmissions.Clear();
        currentEmotionData = new EmotionData();
        
        // Delete local files
        try
        {
            string fullPath = Path.Combine(Application.persistentDataPath, localStoragePath);
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error clearing local data files: {e.Message}");
        }
        
        Debug.Log("All local emotion data cleared");
    }
}

/// <summary>
/// Statistics class for emotion data analysis
/// </summary>
[System.Serializable]
public class EmotionStatistics
{
    public int totalEntries;
    public int completeEntries;
    public Dictionary<string, int> emotionFrequency;
    public Dictionary<int, int> intensityFrequency;
    public float averageIntensity;
    
    public EmotionStatistics(List<EmotionData> emotionDataList)
    {
        totalEntries = emotionDataList.Count;
        completeEntries = 0;
        emotionFrequency = new Dictionary<string, int>();
        intensityFrequency = new Dictionary<int, int>();
        
        float totalIntensity = 0f;
        
        foreach (var data in emotionDataList)
        {
            if (data.IsComplete())
            {
                completeEntries++;
            }
            
            // Count before emotions
            if (!string.IsNullOrEmpty(data.beforeEmotion))
            {
                if (emotionFrequency.ContainsKey(data.beforeEmotion))
                {
                    emotionFrequency[data.beforeEmotion]++;
                }
                else
                {
                    emotionFrequency[data.beforeEmotion] = 1;
                }
            }
            
            // Count after emotions
            if (!string.IsNullOrEmpty(data.afterEmotion))
            {
                if (emotionFrequency.ContainsKey(data.afterEmotion))
                {
                    emotionFrequency[data.afterEmotion]++;
                }
                else
                {
                    emotionFrequency[data.afterEmotion] = 1;
                }
            }
            
            // Count intensity
            if (intensityFrequency.ContainsKey(data.emotionIntensity))
            {
                intensityFrequency[data.emotionIntensity]++;
            }
            else
            {
                intensityFrequency[data.emotionIntensity] = 1;
            }
            
            totalIntensity += data.emotionIntensity;
        }
        
        averageIntensity = totalEntries > 0 ? totalIntensity / totalEntries : 0f;
    }
}
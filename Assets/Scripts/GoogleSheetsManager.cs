using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class GoogleSheetsManager : MonoBehaviour
{
    // Google Form URL - 런타임에서 설정됨
    private string googleFormURL = "";
    
    // Google Form Entry IDs - 런타임에서 설정됨
    private string ENTRY_TIMESTAMP = "";
    private string ENTRY_CLASS_CODE = "";
    private string ENTRY_STUDENT_NUMBER = "";
    private string ENTRY_BEFORE_EMOTION = "";
    private string ENTRY_AFTER_EMOTION = "";
    private string ENTRY_EMOTION_INTENSITY = "";
    private string ENTRY_KEYWORDS = "";
    private string ENTRY_RESOLUTION = "";
    
    private static GoogleSheetsManager instance;
    public static GoogleSheetsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GoogleSheetsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GoogleSheetsManager");
                    instance = go.AddComponent<GoogleSheetsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Send emotion data to Google Forms
    /// </summary>
    public void SendEmotionData(EmotionData emotionData, System.Action<bool, string> onComplete = null)
    {
        if (!IsConfigured())
        {
            onComplete?.Invoke(false, "Google Forms가 설정되지 않았습니다. 관리자에게 문의하세요.");
            return;
        }
        StartCoroutine(SendDataToGoogleForm(emotionData, onComplete));
    }
    
    /// <summary>
    /// Coroutine to handle form submission
    /// </summary>
    private IEnumerator SendDataToGoogleForm(EmotionData emotionData, System.Action<bool, string> onComplete)
    {
        // Validate data before sending
        if (!ValidateEmotionData(emotionData))
        {
            onComplete?.Invoke(false, "Invalid emotion data");
            yield break;
        }
        
        // Create form data
        WWWForm form = new WWWForm();
        
        // Add data to form with proper encoding for Korean text
        AddFormField(form, ENTRY_TIMESTAMP, emotionData.timestamp);
        AddFormField(form, ENTRY_CLASS_CODE, emotionData.classCode);
        AddFormField(form, ENTRY_STUDENT_NUMBER, emotionData.studentNumber);
        AddFormField(form, ENTRY_BEFORE_EMOTION, emotionData.beforeEmotion);
        AddFormField(form, ENTRY_AFTER_EMOTION, emotionData.afterEmotion);
        AddFormField(form, ENTRY_EMOTION_INTENSITY, emotionData.emotionIntensity.ToString());
        
        // Optional fields
        if (!string.IsNullOrEmpty(emotionData.keywords))
        {
            AddFormField(form, ENTRY_KEYWORDS, emotionData.keywords);
        }
        
        if (!string.IsNullOrEmpty(emotionData.todayResolution))
        {
            AddFormField(form, ENTRY_RESOLUTION, emotionData.todayResolution);
        }
        
        // Create UnityWebRequest
        using (UnityWebRequest www = UnityWebRequest.Post(googleFormURL, form))
        {
            // Set timeout
            www.timeout = 30;
            
            // Send request
            yield return www.SendWebRequest();
            
            // Handle response
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Emotion data sent successfully!");
                onComplete?.Invoke(true, "Data submitted successfully");
            }
            else
            {
                string errorMessage = $"Error sending emotion data: {www.error}";
                Debug.LogError(errorMessage);
                onComplete?.Invoke(false, errorMessage);
            }
        }
    }
    
    /// <summary>
    /// Add form field with proper Korean text encoding
    /// </summary>
    private void AddFormField(WWWForm form, string fieldName, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
            
        // Encode Korean text properly
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        form.AddField(fieldName, Encoding.UTF8.GetString(bytes));
    }
    
    /// <summary>
    /// Validate emotion data before submission
    /// </summary>
    private bool ValidateEmotionData(EmotionData data)
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
    /// Send batch data to Google Sheets
    /// </summary>
    public void SendBatchEmotionData(List<EmotionData> emotionDataList, System.Action<int, int> onProgress = null, System.Action<bool> onComplete = null)
    {
        StartCoroutine(SendBatchDataCoroutine(emotionDataList, onProgress, onComplete));
    }
    
    private IEnumerator SendBatchDataCoroutine(List<EmotionData> emotionDataList, System.Action<int, int> onProgress, System.Action<bool> onComplete)
    {
        int successCount = 0;
        int totalCount = emotionDataList.Count;
        
        for (int i = 0; i < emotionDataList.Count; i++)
        {
            bool success = false;
            
            SendEmotionData(emotionDataList[i], (result, message) =>
            {
                success = result;
                if (result) successCount++;
            });
            
            // Wait for the current request to complete
            yield return new WaitUntil(() => success || !string.IsNullOrEmpty(message));
            
            // Report progress
            onProgress?.Invoke(i + 1, totalCount);
            
            // Add delay between requests to avoid overwhelming the server
            yield return new WaitForSeconds(0.5f);
        }
        
        onComplete?.Invoke(successCount == totalCount);
    }
    
    private string message = "";
    
    /// <summary>
    /// Update Google Form configuration
    /// </summary>
    public void UpdateFormConfiguration(string formURL, Dictionary<string, string> entryIds)
    {
        googleFormURL = formURL;
        
        // Update entry IDs if provided
        if (entryIds != null)
        {
            if (entryIds.ContainsKey("timestamp")) ENTRY_TIMESTAMP = entryIds["timestamp"];
            if (entryIds.ContainsKey("classCode")) ENTRY_CLASS_CODE = entryIds["classCode"];
            if (entryIds.ContainsKey("studentNumber")) ENTRY_STUDENT_NUMBER = entryIds["studentNumber"];
            if (entryIds.ContainsKey("beforeEmotion")) ENTRY_BEFORE_EMOTION = entryIds["beforeEmotion"];
            if (entryIds.ContainsKey("afterEmotion")) ENTRY_AFTER_EMOTION = entryIds["afterEmotion"];
            if (entryIds.ContainsKey("emotionIntensity")) ENTRY_EMOTION_INTENSITY = entryIds["emotionIntensity"];
            if (entryIds.ContainsKey("keywords")) ENTRY_KEYWORDS = entryIds["keywords"];
            if (entryIds.ContainsKey("resolution")) ENTRY_RESOLUTION = entryIds["resolution"];
        }
        
        Debug.Log($"Form URL updated to: {formURL}");
    }
    
    /// <summary>
    /// Google Forms 설정 상태 확인
    /// </summary>
    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(googleFormURL) && 
               !string.IsNullOrEmpty(ENTRY_TIMESTAMP) && 
               !string.IsNullOrEmpty(ENTRY_CLASS_CODE) && 
               !string.IsNullOrEmpty(ENTRY_STUDENT_NUMBER) && 
               !string.IsNullOrEmpty(ENTRY_BEFORE_EMOTION);
    }
    
    /// <summary>
    /// Test connection to Google Form
    /// </summary>
    public void TestConnection(System.Action<bool, string> onComplete)
    {
        if (!IsConfigured())
        {
            onComplete?.Invoke(false, "Google Forms가 설정되지 않았습니다.");
            return;
        }
        StartCoroutine(TestConnectionCoroutine(onComplete));
    }
    
    private IEnumerator TestConnectionCoroutine(System.Action<bool, string> onComplete)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(googleFormURL))
        {
            www.timeout = 10;
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(true, "Connection successful");
            }
            else
            {
                onComplete?.Invoke(false, $"Connection failed: {www.error}");
            }
        }
    }
    
    /// <summary>
    /// 클래스별 감정 데이터 읽기 (시뮬레이션)
    /// 실제 구현시에는 Google Sheets API를 사용
    /// </summary>
    public void ReadClassEmotionData(string classCode, System.Action<List<EmotionData>> onComplete)
    {
        StartCoroutine(ReadClassEmotionDataCoroutine(classCode, onComplete));
    }
    
    private IEnumerator ReadClassEmotionDataCoroutine(string classCode, System.Action<List<EmotionData>> onComplete)
    {
        // 임시 데이터 - 실제로는 Google Sheets에서 읽어옴
        List<EmotionData> classData = new List<EmotionData>();
        
        // EmotionManager에서 로컬 데이터 가져오기
        if (EmotionManager.Instance != null)
        {
            var allData = EmotionManager.Instance.GetEmotionHistory();
            foreach (var data in allData)
            {
                if (data.classCode == classCode)
                {
                    classData.Add(data);
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f); // 네트워크 지연 시뮬레이션
        
        Debug.Log($"Loaded {classData.Count} emotion data entries for class {classCode}");
        onComplete?.Invoke(classData);
    }
    
    /// <summary>
    /// 특정 학생의 감정 데이터 읽기
    /// </summary>
    public void ReadStudentEmotionData(string classCode, int studentNumber, System.Action<List<EmotionData>> onComplete)
    {
        StartCoroutine(ReadStudentEmotionDataCoroutine(classCode, studentNumber, onComplete));
    }
    
    private IEnumerator ReadStudentEmotionDataCoroutine(string classCode, int studentNumber, System.Action<List<EmotionData>> onComplete)
    {
        List<EmotionData> studentData = new List<EmotionData>();
        
        if (EmotionManager.Instance != null)
        {
            var allData = EmotionManager.Instance.GetEmotionHistory();
            foreach (var data in allData)
            {
                if (data.classCode == classCode && data.GetStudentNumberAsInt() == studentNumber)
                {
                    studentData.Add(data);
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"Loaded {studentData.Count} emotion data entries for student {studentNumber} in class {classCode}");
        onComplete?.Invoke(studentData);
    }
    
    /// <summary>
    /// 클래스 통계 읽기
    /// </summary>
    public void ReadClassStatistics(string classCode, System.Action<ClassStatistics> onComplete)
    {
        StartCoroutine(ReadClassStatisticsCoroutine(classCode, onComplete));
    }
    
    private IEnumerator ReadClassStatisticsCoroutine(string classCode, System.Action<ClassStatistics> onComplete)
    {
        // 클래스 감정 데이터 먼저 가져오기
        List<EmotionData> classData = new List<EmotionData>();
        
        if (EmotionManager.Instance != null)
        {
            var allData = EmotionManager.Instance.GetEmotionHistory();
            foreach (var data in allData)
            {
                if (data.classCode == classCode)
                {
                    classData.Add(data);
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f); // 네트워크 지연 시뮬레이션
        
        // 통계 생성
        ClassStatistics stats = new ClassStatistics(classCode, classData);
        
        Debug.Log($"Generated statistics for class {classCode}: {stats.totalStudents} students");
        onComplete?.Invoke(stats);
    }
}
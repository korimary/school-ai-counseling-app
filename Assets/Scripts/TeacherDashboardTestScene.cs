using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TeacherDashboard UI 테스트를 위한 씬 설정 클래스
/// 빈 씬에서 이 스크립트를 실행하면 완전한 테스트 환경이 구성됩니다.
/// </summary>
public class TeacherDashboardTestScene : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private bool createTestData = true;
    [SerializeField] private bool autoStartUI = true;
    
    void Start()
    {
        StartCoroutine(SetupTestScene());
    }
    
    private IEnumerator SetupTestScene()
    {
        Debug.Log("=== TeacherDashboard 테스트 씬 설정 시작 ===");
        
        // 1. 테스트 데이터 생성
        if (createTestData)
        {
            CreateTestData();
            yield return new WaitForSeconds(0.1f);
        }
        
        // 2. 필수 매니저들 생성
        SetupManagers();
        yield return new WaitForSeconds(0.1f);
        
        // 3. UI 생성
        if (autoStartUI)
        {
            SetupUI();
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("=== 테스트 씬 설정 완료 ===");
        Debug.Log("F1: UI 생성, F2: UI 삭제");
    }
    
    private void CreateTestData()
    {
        Debug.Log("테스트 데이터 생성 중...");
        
        // 테스트용 학교 데이터 생성
        SchoolData testSchoolData = new SchoolData();
        testSchoolData.teacherInfo = new TeacherInfo
        {
            teacherName = "김선생",
            className = "3학년 1반",
            schoolName = "테스트 초등학교",
            grade = "3학년",
            classCode = "3-1반-1234"
        };
        
        testSchoolData.students = new List<StudentInfo>
        {
            new StudentInfo(1, "김철수"),
            new StudentInfo(2, "이영희"),
            new StudentInfo(3, "박민수"),
            new StudentInfo(4, "최지은"),
            new StudentInfo(5, "정태윤")
        };
        
        // 데이터 저장
        StudentDataManager.SaveSchoolData(testSchoolData);
        
        // 클래스 코드 설정
        ClassCodeManager.SetCurrentClassCode("3-1반-1234", 
            ClassCodeManager.CreateClassCodeDataFromSchoolData(testSchoolData, "3-1반-1234"));
        
        // 테스트용 감정 데이터 생성
        CreateTestEmotionData();
        
        Debug.Log("테스트 데이터 생성 완료");
    }
    
    private void CreateTestEmotionData()
    {
        if (EmotionManager.Instance == null) return;
        
        string[] emotions = { "기쁨", "슬픔", "화남", "불안", "신남" };
        System.Random random = new System.Random();
        
        // 각 학생별로 랜덤 감정 데이터 생성
        for (int studentNum = 1; studentNum <= 5; studentNum++)
        {
            for (int session = 0; session < random.Next(3, 8); session++)
            {
                string beforeEmotion = emotions[random.Next(emotions.Length)];
                string afterEmotion = emotions[random.Next(emotions.Length)];
                int intensity = random.Next(1, 6);
                
                EmotionData emotionData = new EmotionData("3-1반-1234", studentNum.ToString(), 
                    beforeEmotion, afterEmotion, intensity);
                
                // 타임스탬프를 과거로 설정
                System.DateTime pastTime = System.DateTime.Now.AddDays(-random.Next(1, 30));
                emotionData.timestamp = pastTime.ToString("yyyy-MM-dd HH:mm:ss");
                
                EmotionManager.Instance.SubmitEmotionData(emotionData);
            }
        }
        
        Debug.Log("테스트 감정 데이터 생성 완료");
    }
    
    private void SetupManagers()
    {
        Debug.Log("매니저들 설정 중...");
        
        // EmotionManager 확인/생성
        if (EmotionManager.Instance == null)
        {
            GameObject emotionManagerObj = new GameObject("EmotionManager");
            emotionManagerObj.AddComponent<EmotionManager>();
            DontDestroyOnLoad(emotionManagerObj);
        }
        
        // GoogleSheetsManager 확인/생성
        if (GoogleSheetsManager.Instance == null)
        {
            GameObject googleManagerObj = new GameObject("GoogleSheetsManager");
            googleManagerObj.AddComponent<GoogleSheetsManager>();
            DontDestroyOnLoad(googleManagerObj);
        }
        
        // UserManager 모드 설정
        UserManager.SetTeacherMode(true);
        
        Debug.Log("매니저 설정 완료");
    }
    
    private void SetupUI()
    {
        Debug.Log("UI 설정 중...");
        
        // TeacherDashboardMaster가 없으면 생성
        TeacherDashboardMaster master = FindObjectOfType<TeacherDashboardMaster>();
        if (master == null)
        {
            GameObject masterObj = new GameObject("TeacherDashboardMaster");
            master = masterObj.AddComponent<TeacherDashboardMaster>();
        }
        
        Debug.Log("UI 설정 완료 - 자동 생성 시작");
    }
    
    /// <summary>
    /// 테스트 데이터 초기화
    /// </summary>
    [ContextMenu("Clear Test Data")]
    public void ClearTestData()
    {
        StudentDataManager.ClearSchoolData();
        ClassCodeManager.ClearClassCode();
        
        if (EmotionManager.Instance != null)
        {
            EmotionManager.Instance.ClearAllLocalData();
        }
        
        Debug.Log("테스트 데이터가 모두 삭제되었습니다.");
    }
    
    /// <summary>
    /// 추가 테스트 데이터 생성
    /// </summary>
    [ContextMenu("Add More Test Data")]
    public void AddMoreTestData()
    {
        CreateTestEmotionData();
        Debug.Log("추가 테스트 데이터가 생성되었습니다.");
    }
    
    void Update()
    {
        // 디버그 키
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ClearTestData();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            AddMoreTestData();
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            // 현재 데이터 상태 출력
            Debug.Log("=== 현재 데이터 상태 ===");
            var schoolData = StudentDataManager.LoadSchoolData();
            if (schoolData != null)
            {
                Debug.Log($"교사: {schoolData.teacherInfo.teacherName}");
                Debug.Log($"학급: {schoolData.teacherInfo.className}");
                Debug.Log($"학생 수: {schoolData.students?.Count ?? 0}명");
            }
            
            if (EmotionManager.Instance != null)
            {
                var emotions = EmotionManager.Instance.GetEmotionHistory();
                Debug.Log($"감정 기록: {emotions.Count}개");
            }
            
            Debug.Log($"클래스 코드: {ClassCodeManager.GetCurrentClassCode()}");
        }
    }
    
    void OnGUI()
    {
        // 화면 상단에 간단한 가이드 표시
        GUI.Label(new Rect(10, 10, 400, 100), 
            "TeacherDashboard 테스트 모드\n" +
            "F1: UI 생성  F2: UI 삭제\n" +
            "F3: 데이터 삭제  F4: 추가 데이터 생성\n" +
            "F5: 현재 상태 출력");
    }
}
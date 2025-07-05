using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelUI : MonoBehaviour
{
    [Header("교사 정보")]
    [SerializeField] private TMP_InputField teacherNameInput;
    [SerializeField] private TMP_InputField classNameInput;
    [SerializeField] private TextMeshProUGUI currentClassCodeText;
    [SerializeField] private Button regenerateClassCodeButton;
    [SerializeField] private Button saveTeacherInfoButton;
    
    [Header("Google Sheets 설정")]
    [SerializeField] private TMP_InputField googleFormUrlInput;
    [SerializeField] private TMP_InputField entryIdBeforeInput;
    [SerializeField] private TMP_InputField entryIdAfterInput;
    [SerializeField] private TMP_InputField entryIdKeywordsInput;
    [SerializeField] private Button testConnectionButton;
    [SerializeField] private Button saveGoogleSettingsButton;
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    
    [Header("데이터 관리")]
    [SerializeField] private TextMeshProUGUI dataStatisticsText;
    [SerializeField] private Button exportDataButton;
    [SerializeField] private Button clearLocalDataButton;
    [SerializeField] private Button clearAllDataButton;
    
    [Header("확인 다이얼로그")]
    [SerializeField] private GameObject confirmDialog;
    [SerializeField] private TextMeshProUGUI confirmMessageText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    private System.Action pendingAction;
    private TeacherInfo currentTeacherInfo;
    
    private void Start()
    {
        InitializeUI();
        LoadCurrentSettings();
    }
    
    private void InitializeUI()
    {
        // 버튼 이벤트 설정
        if (regenerateClassCodeButton != null)
            regenerateClassCodeButton.onClick.AddListener(OnRegenerateClassCode);
            
        if (saveTeacherInfoButton != null)
            saveTeacherInfoButton.onClick.AddListener(OnSaveTeacherInfo);
            
        if (testConnectionButton != null)
            testConnectionButton.onClick.AddListener(OnTestGoogleConnection);
            
        if (saveGoogleSettingsButton != null)
            saveGoogleSettingsButton.onClick.AddListener(OnSaveGoogleSettings);
            
        if (exportDataButton != null)
            exportDataButton.onClick.AddListener(OnExportData);
            
        if (clearLocalDataButton != null)
            clearLocalDataButton.onClick.AddListener(OnClearLocalData);
            
        if (clearAllDataButton != null)
            clearAllDataButton.onClick.AddListener(OnClearAllData);
            
        // 확인 다이얼로그 버튼
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmYes);
            
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(OnConfirmNo);
            
        // 입력 필드 이벤트
        if (teacherNameInput != null)
            teacherNameInput.onValueChanged.AddListener(_ => OnTeacherInfoChanged());
            
        if (classNameInput != null)
            classNameInput.onValueChanged.AddListener(_ => OnTeacherInfoChanged());
    }
    
    private void LoadCurrentSettings()
    {
        // 교사 정보 로드
        var schoolData = StudentDataManager.LoadSchoolData();
        if (schoolData != null && schoolData.teacherInfo != null)
        {
            currentTeacherInfo = schoolData.teacherInfo;
            
            if (teacherNameInput != null)
                teacherNameInput.text = currentTeacherInfo.teacherName;
                
            if (classNameInput != null)
                classNameInput.text = currentTeacherInfo.className;
        }
        
        // 클래스 코드 표시
        string classCode = ClassCodeManager.GetCurrentClassCode();
        if (currentClassCodeText != null)
            currentClassCodeText.text = $"현재 클래스 코드: {classCode}";
            
        // Google Sheets 설정 로드
        LoadGoogleSheetsSettings();
        
        // 데이터 통계 업데이트
        UpdateDataStatistics();
    }
    
    private void LoadGoogleSheetsSettings()
    {
        if (GoogleSheetsManager.Instance != null)
        {
            // 저장된 설정이 있다면 로드
            string savedUrl = PlayerPrefs.GetString("GoogleFormUrl", "");
            string savedEntryBefore = PlayerPrefs.GetString("GoogleEntryBefore", "");
            string savedEntryAfter = PlayerPrefs.GetString("GoogleEntryAfter", "");
            string savedEntryKeywords = PlayerPrefs.GetString("GoogleEntryKeywords", "");
            
            if (googleFormUrlInput != null)
                googleFormUrlInput.text = savedUrl;
                
            if (entryIdBeforeInput != null)
                entryIdBeforeInput.text = savedEntryBefore;
                
            if (entryIdAfterInput != null)
                entryIdAfterInput.text = savedEntryAfter;
                
            if (entryIdKeywordsInput != null)
                entryIdKeywordsInput.text = savedEntryKeywords;
                
            UpdateConnectionStatus(GoogleSheetsManager.Instance.IsConfigured());
        }
    }
    
    private void UpdateDataStatistics()
    {
        if (dataStatisticsText == null) return;
        
        int localEmotionCount = 0;
        int pendingSubmissions = 0;
        
        if (EmotionManager.Instance != null)
        {
            var emotionHistory = EmotionManager.Instance.GetEmotionHistory();
            localEmotionCount = emotionHistory.Count;
            pendingSubmissions = EmotionManager.Instance.GetPendingSubmissionCount();
        }
        
        var schoolData = StudentDataManager.LoadSchoolData();
        int studentCount = schoolData?.students?.Count ?? 0;
        
        dataStatisticsText.text = $"저장된 감정 기록: {localEmotionCount}개\n" +
                                 $"미전송 데이터: {pendingSubmissions}개\n" +
                                 $"등록된 학생: {studentCount}명";
    }
    
    private void OnTeacherInfoChanged()
    {
        if (saveTeacherInfoButton != null)
            saveTeacherInfoButton.interactable = true;
    }
    
    private void OnSaveTeacherInfo()
    {
        if (string.IsNullOrEmpty(teacherNameInput.text) || string.IsNullOrEmpty(classNameInput.text))
        {
            ShowMessage("교사명과 학급명을 모두 입력해주세요.", false);
            return;
        }
        
        var schoolData = StudentDataManager.LoadSchoolData();
        if (schoolData == null)
        {
            schoolData = new SchoolData();
        }
        
        if (schoolData.teacherInfo == null)
        {
            schoolData.teacherInfo = new TeacherInfo();
        }
        
        schoolData.teacherInfo.teacherName = teacherNameInput.text.Trim();
        schoolData.teacherInfo.className = classNameInput.text.Trim();
        
        StudentDataManager.SaveSchoolData(schoolData);
        ShowMessage("교사 정보가 저장되었습니다.", true);
        if (saveTeacherInfoButton != null)
            saveTeacherInfoButton.interactable = false;
    }
    
    private void OnRegenerateClassCode()
    {
        ShowConfirmDialog("클래스 코드를 재생성하시겠습니까?\n기존 코드로는 접속할 수 없게 됩니다.", () =>
        {
            string newCode = ClassCodeManager.GenerateNewClassCode();
            
            var schoolData = StudentDataManager.LoadSchoolData();
            if (schoolData != null && schoolData.teacherInfo != null)
            {
                schoolData.teacherInfo.classCode = newCode;
                StudentDataManager.SaveSchoolData(schoolData);
            }
            
            if (currentClassCodeText != null)
                currentClassCodeText.text = $"현재 클래스 코드: {newCode}";
                
            ShowMessage($"새 클래스 코드: {newCode}", true);
        });
    }
    
    private void OnTestGoogleConnection()
    {
        if (string.IsNullOrEmpty(googleFormUrlInput.text))
        {
            ShowMessage("Google Form URL을 입력해주세요.", false);
            return;
        }
        
        // Google Sheets 연결 테스트
        StartCoroutine(TestGoogleConnection(googleFormUrlInput.text, entryIdBeforeInput.text, entryIdAfterInput.text, entryIdKeywordsInput.text));
    }
    
    private IEnumerator TestGoogleConnection(string formUrl, string entryBefore, string entryAfter, string entryKeywords)
    {
        if (connectionStatusText != null)
            connectionStatusText.text = "연결 테스트 중...";
            
        // 임시 설정 적용
        GoogleSheetsManager.Instance.SetGoogleFormConfig(
            formUrl,
            entryBefore,
            entryAfter,
            entryKeywords
        );
        
        // 테스트 데이터 전송
        var testData = new EmotionData("TEST", "0", "테스트", 3);
        
        bool success = false;
        string message = "";
        GoogleSheetsManager.Instance.SendEmotionData(testData, (result, msg) =>
        {
            success = result;
            message = msg;
        });
        
        yield return new WaitForSeconds(2f);
        
        UpdateConnectionStatus(success);
        
        if (success)
        {
            ShowMessage("Google Sheets 연결 성공!", true);
        }
        else
        {
            ShowMessage($"Google Sheets 연결 실패: {message}", false);
        }
    }
    
    private void OnSaveGoogleSettings()
    {
        // Google Sheets 설정 저장
        PlayerPrefs.SetString("GoogleFormUrl", googleFormUrlInput.text);
        PlayerPrefs.SetString("GoogleEntryBefore", entryIdBeforeInput.text);
        PlayerPrefs.SetString("GoogleEntryAfter", entryIdAfterInput.text);
        PlayerPrefs.SetString("GoogleEntryKeywords", entryIdKeywordsInput.text);
        PlayerPrefs.Save();
        
        // GoogleSheetsManager에 설정 적용
        GoogleSheetsManager.Instance.SetGoogleFormConfig(
            googleFormUrlInput.text,
            entryIdBeforeInput.text,
            entryIdAfterInput.text,
            entryIdKeywordsInput.text
        );
        
        ShowMessage("Google Sheets 설정이 저장되었습니다.", true);
    }
    
    private void OnExportData()
    {
        ShowMessage("데이터 내보내기 기능은 준비 중입니다.", false);
        // TODO: CSV 또는 JSON으로 데이터 내보내기 구현
    }
    
    private void OnClearLocalData()
    {
        ShowConfirmDialog("로컬에 저장된 감정 데이터를 모두 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.", () =>
        {
            if (EmotionManager.Instance != null)
            {
                EmotionManager.Instance.ClearAllLocalData();
                UpdateDataStatistics();
                ShowMessage("로컬 데이터가 삭제되었습니다.", true);
            }
        });
    }
    
    private void OnClearAllData()
    {
        ShowConfirmDialog("모든 데이터(교사 정보, 학생 정보, 감정 기록)를 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다!", () =>
        {
            // 모든 데이터 삭제
            StudentDataManager.ClearSchoolData();
            
            if (EmotionManager.Instance != null)
            {
                EmotionManager.Instance.ClearAllLocalData();
            }
            
            // PlayerPrefs 초기화
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            ShowMessage("모든 데이터가 삭제되었습니다. 앱을 재시작해주세요.", true);
            
            // 3초 후 메인 메뉴로 이동
            StartCoroutine(ReturnToMainMenuAfterDelay(3f));
        });
    }
    
    private IEnumerator ReturnToMainMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
    
    private void ShowConfirmDialog(string message, System.Action onConfirm)
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(true);
            
            if (confirmMessageText != null)
                confirmMessageText.text = message;
                
            pendingAction = onConfirm;
        }
    }
    
    private void OnConfirmYes()
    {
        if (confirmDialog != null)
            confirmDialog.SetActive(false);
            
        pendingAction?.Invoke();
        pendingAction = null;
    }
    
    private void OnConfirmNo()
    {
        if (confirmDialog != null)
            confirmDialog.SetActive(false);
            
        pendingAction = null;
    }
    
    private void UpdateConnectionStatus(bool isConnected)
    {
        if (connectionStatusText != null)
        {
            if (isConnected)
            {
                connectionStatusText.text = "연결됨";
                connectionStatusText.color = Color.green;
            }
            else
            {
                connectionStatusText.text = "연결 안됨";
                connectionStatusText.color = Color.red;
            }
        }
    }
    
    private void ShowMessage(string message, bool isSuccess)
    {
        Debug.Log($"[Settings] {message}");
        // TODO: 실제 메시지 팝업 표시
    }
}
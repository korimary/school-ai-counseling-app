using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class RecordsViewUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform studentListContainer;
    public GameObject studentButtonPrefab;
    public GameObject recordsPanel;
    public TextMeshProUGUI recordsText;
    public ScrollRect recordsScrollRect;
    public Button backButton;
    public Button shareButton;
    public TextMeshProUGUI studentNameText;
    
    private SchoolData schoolData;
    private DataSaver dataSaver;
    private int currentStudentNumber = -1;
    private string currentRecordsText = "";
    
    void Start()
    {
        dataSaver = FindObjectOfType<DataSaver>();
        if (dataSaver == null)
        {
            GameObject dataSaverObj = new GameObject("DataSaver");
            dataSaver = dataSaverObj.AddComponent<DataSaver>();
        }
        
        schoolData = StudentDataManager.LoadSchoolData();
        
        if (schoolData == null)
        {
            Debug.LogWarning("No school data found! 테스트용 임시 데이터를 생성합니다.");
            
            // 테스트용 임시 데이터 생성
            schoolData = new SchoolData();
            schoolData.teacherInfo.schoolName = "테스트중학교";
            schoolData.teacherInfo.grade = "1학년";
            schoolData.teacherInfo.className = "3반";
            schoolData.teacherInfo.teacherName = "김선생님";
            schoolData.teacherInfo.studentCount = 3;
            
            schoolData.students.Add(new StudentInfo(1, "김철수"));
            schoolData.students.Add(new StudentInfo(2, "이영희"));
            schoolData.students.Add(new StudentInfo(3, "박민수"));
        }
        
        SetupUI();
        CreateStudentButtons();
        
        // 초기에는 기록 패널 숨기기
        recordsPanel.SetActive(false);
    }
    
    void SetupUI()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        shareButton.onClick.AddListener(OnShareClicked);
    }
    
    void OnBackButtonClicked()
    {
        // RecordsPanel이 활성화되어 있으면 MainPanel로 돌아가기
        if (recordsPanel.activeInHierarchy)
        {
            recordsPanel.SetActive(false);
            studentListContainer.transform.parent.parent.gameObject.SetActive(true); // MainPanel 다시 표시
            Debug.Log("[RecordsViewUI] MainPanel로 돌아감");
        }
        else
        {
            // MainPanel에서는 메인 씬으로 이동
            Debug.Log("[RecordsViewUI] 메인 씬으로 이동");
            SceneManager.LoadScene("SampleScene");
        }
    }
    
    void CreateStudentButtons()
    {
        Debug.Log("[RecordsViewUI] CreateStudentButtons 호출됨");
        
        // Layout Group 설정 확인 및 추가 (겹침 문제 해결)
        VerticalLayoutGroup layoutGroup = studentListContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = studentListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = 10f; // 버튼 간격
            layoutGroup.padding = new RectOffset(20, 20, 20, 20); // 여백
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            Debug.Log("[RecordsViewUI] VerticalLayoutGroup 추가됨");
        }
        
        // ContentSizeFitter 설정 (크기 자동 조정)
        ContentSizeFitter sizeFitter = studentListContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = studentListContainer.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Debug.Log("[RecordsViewUI] ContentSizeFitter 추가됨");
        }
        
        // 기존 버튼 제거
        foreach (Transform child in studentListContainer)
        {
            Destroy(child.gameObject);
        }
        
        Debug.Log($"[RecordsViewUI] {schoolData.students.Count}개 학생 버튼 생성 시작");
        
        // 학생별 버튼 생성
        foreach (var student in schoolData.students.OrderBy(s => s.number))
        {
            GameObject buttonObj = Instantiate(studentButtonPrefab, studentListContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            // LayoutElement 추가 (고정 높이 설정)
            LayoutElement layoutElement = buttonObj.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = buttonObj.AddComponent<LayoutElement>();
            }
            layoutElement.preferredHeight = 60f;
            layoutElement.flexibleWidth = 1f;
            
            // 버튼 텍스트 설정
            buttonText.text = $"{student.number}번 {student.name}";
            
            // 클릭 이벤트 설정
            int studentNumber = student.number;
            string studentName = student.name;
            button.onClick.AddListener(() => ShowStudentRecords(studentNumber, studentName));
            
            // 해당 학생의 기록 개수 표시
            var records = dataSaver.LoadAllSummaries(studentNumber);
            if (records.Length > 0)
            {
                buttonText.text += $" ({records.Length}건)";
                buttonText.color = Color.white;
            }
            else
            {
                buttonText.text += " (기록 없음)";
                buttonText.color = Color.gray;
            }
            
            Debug.Log($"[RecordsViewUI] {student.number}번 {student.name} 버튼 생성 완료");
        }
        
        // Layout 강제 업데이트
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(studentListContainer.GetComponent<RectTransform>());
        
        Debug.Log("[RecordsViewUI] 모든 학생 버튼 생성 및 레이아웃 설정 완료");
    }
    
    void ShowStudentRecords(int studentNumber, string studentName)
    {
        Debug.Log($"[RecordsViewUI] {studentNumber}번 {studentName} 학생 기록 조회");
        
        currentStudentNumber = studentNumber;
        studentNameText.text = $"{studentNumber}번 {studentName} 상담 기록";
        
        // 해당 학생의 모든 기록 불러오기
        var records = dataSaver.LoadAllSummaries(studentNumber);
        
        Debug.Log($"[RecordsViewUI] {studentName} 학생의 기록 {records.Length}건 발견");
        
        if (records.Length == 0)
        {
            recordsText.text = "아직 상담 기록이 없습니다.";
            currentRecordsText = "";
        }
        else
        {
            // 날짜 역순으로 정렬 (최신 기록이 위로)
            var sortedRecords = records.OrderByDescending(r => r.timestamp).ToArray();
            
            string recordsContent = $"=== 총 {records.Length}건의 상담 기록 ===\n\n";
            
            foreach (var record in sortedRecords)
            {
                recordsContent += $"날짜: {record.timestamp}\n";
                recordsContent += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
                recordsContent += $"{record.summary}\n";
                recordsContent += "\n[원본 녹음 내용]\n";
                recordsContent += $"{record.transcription}\n";
                recordsContent += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
            }
            
            recordsText.text = recordsContent;
            currentRecordsText = recordsContent;
        }
        
        // MainPanel 숨기고 RecordsPanel 표시
        studentListContainer.transform.parent.parent.gameObject.SetActive(false); // MainPanel 숨기기
        recordsPanel.SetActive(true);
        
        // 스크롤 위치 초기화
        if (recordsScrollRect != null)
        {
            recordsScrollRect.verticalNormalizedPosition = 1f;
        }
        
        Debug.Log("[RecordsViewUI] 개별 학생 기록 화면 표시 완료");
    }
    
    void OnShareClicked()
    {
        if (string.IsNullOrEmpty(currentRecordsText))
        {
            Debug.Log("No records to share");
            return;
        }
        
        // 클립보드 복사 (Native Share 플러그인 전까지 임시)
        GUIUtility.systemCopyBuffer = currentRecordsText;
        
        // 알림 표시
        studentNameText.text += " (클립보드에 복사됨!)";
        
        // 3초 후 원래 텍스트로 복원
        Invoke(nameof(RestoreStudentNameText), 3f);
    }
    
    void RestoreStudentNameText()
    {
        if (currentStudentNumber > 0 && schoolData != null)
        {
            var student = schoolData.students.Find(s => s.number == currentStudentNumber);
            if (student != null)
            {
                studentNameText.text = $"{student.number}번 {student.name} 상담 기록";
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class InitialSetupUI : MonoBehaviour
{
    [Header("교사 정보 입력 패널")]
    public GameObject teacherInfoPanel;
    public TMP_InputField schoolNameInput;
    public TMP_InputField gradeInput;
    public TMP_InputField classNameInput;
    public TMP_InputField teacherNameInput;
    public TMP_InputField studentCountInput;
    public Button nextButton;
    
    [Header("학생 정보 입력 패널")]
    public GameObject studentInfoPanel;
    public Transform studentListContainer;
    public GameObject studentInputPrefab;
    public Button saveButton;
    
    [Header("UI 요소")]
    public TextMeshProUGUI errorText;
    
    private SchoolData schoolData;
    private List<GameObject> studentInputFields = new List<GameObject>();
    
    void Start()
    {
        schoolData = new SchoolData();
        
        // 처음에는 교사 정보 패널만 표시
        teacherInfoPanel.SetActive(true);
        studentInfoPanel.SetActive(false);
        
        // 버튼 리스너 추가
        nextButton.onClick.AddListener(OnNextButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        
        // 에러 텍스트 숨기기
        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }
    
    void OnNextButtonClick()
    {
        // 입력 검증
        if (string.IsNullOrEmpty(schoolNameInput.text) ||
            string.IsNullOrEmpty(gradeInput.text) ||
            string.IsNullOrEmpty(classNameInput.text) ||
            string.IsNullOrEmpty(teacherNameInput.text) ||
            string.IsNullOrEmpty(studentCountInput.text))
        {
            ShowError("모든 항목을 입력해주세요.");
            return;
        }
        
        int studentCount;
        if (!int.TryParse(studentCountInput.text, out studentCount) || studentCount <= 0)
        {
            ShowError("학생 수는 1명 이상이어야 합니다.");
            return;
        }
        
        // 교사 정보 저장
        schoolData.teacherInfo.schoolName = schoolNameInput.text;
        schoolData.teacherInfo.grade = gradeInput.text;
        schoolData.teacherInfo.className = classNameInput.text;
        schoolData.teacherInfo.teacherName = teacherNameInput.text;
        schoolData.teacherInfo.studentCount = studentCount;
        
        // 학생 입력 필드 생성
        CreateStudentInputFields(studentCount);
        
        // 패널 전환
        teacherInfoPanel.SetActive(false);
        studentInfoPanel.SetActive(true);
    }
    
    void CreateStudentInputFields(int count)
    {
        Debug.Log($"[DEBUG] CreateStudentInputFields 호출됨! 학생 수: {count}");
        
        // Layout Group 설정 확인 및 추가 (겹침 문제 해결)
        VerticalLayoutGroup layoutGroup = studentListContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = studentListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = 15f; // 간격 설정
            layoutGroup.padding = new RectOffset(20, 20, 20, 20); // 여백 설정
            Debug.Log("[DEBUG] VerticalLayoutGroup 추가됨");
        }
        
        // ContentSizeFitter 설정 (크기 자동 조정)
        ContentSizeFitter sizeFitter = studentListContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = studentListContainer.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Debug.Log("[DEBUG] ContentSizeFitter 추가됨");
        }
        
        // 기존 필드 제거
        foreach (var field in studentInputFields)
        {
            Destroy(field);
        }
        studentInputFields.Clear();
        
        // 새 필드 생성
        for (int i = 0; i < count; i++)
        {
            GameObject studentInput = Instantiate(studentInputPrefab, studentListContainer);
            
            if (studentInput == null)
            {
                Debug.LogError($"[DEBUG] {i + 1}번 학생 필드 생성 실패!");
                continue;
            }
            
            // LayoutElement 추가 (고정 높이 설정)
            LayoutElement layoutElement = studentInput.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = studentInput.AddComponent<LayoutElement>();
            }
            layoutElement.preferredHeight = 80f;
            layoutElement.flexibleWidth = 1f;
            
            // 번호 표시
            TextMeshProUGUI numberText = studentInput.transform.Find("NumberText").GetComponent<TextMeshProUGUI>();
            if (numberText != null)
            {
                numberText.text = $"{i + 1}번";
            }
            
            // InputField 한글 폰트 설정
            TMP_InputField nameInput = studentInput.transform.Find("NameInput").GetComponent<TMP_InputField>();
            if (nameInput != null && nameInput.textComponent != null)
            {
                var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
                if (koreanFont != null)
                {
                    nameInput.textComponent.font = koreanFont;
                }
            }
            
            // 입력 필드 참조 저장
            studentInputFields.Add(studentInput);
        }
        
        // Layout 강제 업데이트
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(studentListContainer.GetComponent<RectTransform>());
        
        Debug.Log($"[DEBUG] {count}개 학생 필드 생성 및 레이아웃 설정 완료");
    }
    
    void OnSaveButtonClick()
    {
        Debug.Log("[DEBUG] 저장 버튼 클릭됨!");
        
        schoolData.students.Clear();
        
        // 모든 학생 정보 수집
        for (int i = 0; i < studentInputFields.Count; i++)
        {
            TMP_InputField nameInput = studentInputFields[i].transform.Find("NameInput").GetComponent<TMP_InputField>();
            
            if (nameInput == null)
            {
                Debug.LogError($"[DEBUG] {i + 1}번 학생의 NameInput을 찾을 수 없습니다!");
                ShowError($"{i + 1}번 학생 입력 필드에 오류가 있습니다.");
                return;
            }
            
            if (string.IsNullOrEmpty(nameInput.text.Trim()))
            {
                ShowError($"{i + 1}번 학생의 이름을 입력해주세요.");
                return;
            }
            
            StudentInfo student = new StudentInfo(i + 1, nameInput.text.Trim());
            schoolData.students.Add(student);
            Debug.Log($"[DEBUG] {i + 1}번 {nameInput.text.Trim()} 학생 정보 추가됨");
        }
        
        try
        {
            // 데이터 저장
            StudentDataManager.SaveSchoolData(schoolData);
            Debug.Log("[DEBUG] 학교 데이터 저장 완료!");
            
            // 저장 성공 메시지
            if (errorText != null)
            {
                errorText.text = "저장 완료! 메인 화면으로 이동합니다...";
                errorText.color = Color.green;
                errorText.gameObject.SetActive(true);
            }
            
            // 잠시 대기 후 메인 씬으로 이동
            Invoke(nameof(LoadMainScene), 1f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DEBUG] 데이터 저장 실패: {e.Message}");
            ShowError("데이터 저장 중 오류가 발생했습니다: " + e.Message);
        }
    }
    
    void LoadMainScene()
    {
        Debug.Log("[DEBUG] 메인 씬으로 이동 중...");
        SceneManager.LoadScene("SampleScene");
    }
    
    void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
            
            // 3초 후 에러 메시지 숨기기
            CancelInvoke(nameof(HideError));
            Invoke(nameof(HideError), 3f);
        }
    }
    
    void HideError()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }
}
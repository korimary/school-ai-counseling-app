using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddStudentPopup : MonoBehaviour
{
    [Header("입력 필드")]
    [SerializeField] private TMP_InputField studentNumberInput;
    [SerializeField] private TMP_InputField studentNameInput;
    
    [Header("버튼")]
    [SerializeField] private Button addButton;
    [SerializeField] private Button cancelButton;
    
    [Header("메시지")]
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private GameObject errorMessagePanel;
    
    [Header("애니메이션")]
    [SerializeField] private float animationDuration = 0.3f;
    
    private System.Action onStudentAdded;
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        // 버튼 이벤트 설정
        if (addButton != null)
            addButton.onClick.AddListener(OnAddButtonClicked);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            
        // 입력 필드 이벤트 설정
        if (studentNumberInput != null)
        {
            studentNumberInput.onValueChanged.AddListener(OnInputChanged);
            studentNumberInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            studentNumberInput.characterLimit = 4;
        }
        
        if (studentNameInput != null)
        {
            studentNameInput.onValueChanged.AddListener(OnInputChanged);
            studentNameInput.characterLimit = 20;
        }
        
        // 초기 상태 설정
        HideErrorMessage();
    }
    
    public void ShowPopup(System.Action onAddedCallback = null)
    {
        onStudentAdded = onAddedCallback;
        gameObject.SetActive(true);
        
        // 입력 필드 초기화
        if (studentNumberInput != null)
            studentNumberInput.text = "";
            
        if (studentNameInput != null)
            studentNameInput.text = "";
            
        HideErrorMessage();
        
        // 페이드인 애니메이션
        StartCoroutine(FadeIn());
        
        // 첫 번째 입력 필드에 포커스
        if (studentNumberInput != null)
        {
            studentNumberInput.Select();
            studentNumberInput.ActivateInputField();
        }
    }
    
    private void OnAddButtonClicked()
    {
        if (!ValidateInput())
            return;
            
        int studentNumber = int.Parse(studentNumberInput.text);
        string studentName = studentNameInput.text.Trim();
        
        // 중복 체크
        if (IsStudentNumberExists(studentNumber))
        {
            ShowErrorMessage($"이미 존재하는 번호입니다: {studentNumber}번");
            return;
        }
        
        // 학생 추가
        AddStudent(studentNumber, studentName);
    }
    
    private void OnCancelButtonClicked()
    {
        ClosePopup();
    }
    
    private void OnInputChanged(string value)
    {
        // 입력이 변경되면 에러 메시지 숨기기
        HideErrorMessage();
        
        // 추가 버튼 활성화 상태 업데이트
        UpdateAddButtonState();
    }
    
    private bool ValidateInput()
    {
        // 번호 검증
        if (string.IsNullOrEmpty(studentNumberInput.text))
        {
            ShowErrorMessage("학생 번호를 입력해주세요.");
            studentNumberInput.Select();
            return false;
        }
        
        if (!int.TryParse(studentNumberInput.text, out int studentNumber))
        {
            ShowErrorMessage("올바른 번호를 입력해주세요.");
            studentNumberInput.Select();
            return false;
        }
        
        if (studentNumber < 1 || studentNumber > 9999)
        {
            ShowErrorMessage("번호는 1~9999 사이여야 합니다.");
            studentNumberInput.Select();
            return false;
        }
        
        // 이름 검증
        string studentName = studentNameInput.text.Trim();
        if (string.IsNullOrEmpty(studentName))
        {
            ShowErrorMessage("학생 이름을 입력해주세요.");
            studentNameInput.Select();
            return false;
        }
        
        if (studentName.Length < 2)
        {
            ShowErrorMessage("이름은 2자 이상이어야 합니다.");
            studentNameInput.Select();
            return false;
        }
        
        return true;
    }
    
    private bool IsStudentNumberExists(int studentNumber)
    {
        var schoolData = StudentDataManager.LoadSchoolData();
        if (schoolData != null && schoolData.students != null)
        {
            return schoolData.students.Any(s => s.number == studentNumber);
        }
        return false;
    }
    
    private void AddStudent(int studentNumber, string studentName)
    {
        // 학교 데이터 로드
        var schoolData = StudentDataManager.LoadSchoolData();
        if (schoolData == null)
        {
            ShowErrorMessage("학교 데이터를 불러올 수 없습니다.");
            return;
        }
        
        // 학생 리스트가 없으면 생성
        if (schoolData.students == null)
            schoolData.students = new List<StudentInfo>();
            
        // 새 학생 추가
        StudentInfo newStudent = new StudentInfo(studentNumber, studentName);
        
        schoolData.students.Add(newStudent);
        
        // 번호순으로 정렬
        schoolData.students = schoolData.students.OrderBy(s => s.number).ToList();
        
        // 저장
        StudentDataManager.SaveSchoolData(schoolData);
        Debug.Log($"학생 추가 성공: {studentNumber}번 {studentName}");
        
        // 콜백 호출
        onStudentAdded?.Invoke();
        
        // 팝업 닫기
        ClosePopup();
    }
    
    private void UpdateAddButtonState()
    {
        if (addButton != null)
        {
            bool isValid = !string.IsNullOrEmpty(studentNumberInput.text) &&
                          !string.IsNullOrEmpty(studentNameInput.text.Trim());
            addButton.interactable = isValid;
        }
    }
    
    private void ShowErrorMessage(string message)
    {
        if (errorMessageText != null)
            errorMessageText.text = message;
            
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(true);
    }
    
    private void HideErrorMessage()
    {
        if (errorMessagePanel != null)
            errorMessagePanel.SetActive(false);
    }
    
    private void ClosePopup()
    {
        StartCoroutine(FadeOutAndClose());
    }
    
    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOutAndClose()
    {
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / animationDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    
    // 엔터 키로 추가하기
    private void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            if (studentNumberInput.isFocused || studentNameInput.isFocused)
            {
                OnAddButtonClicked();
            }
        }
    }
}
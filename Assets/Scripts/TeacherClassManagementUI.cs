using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TeacherClassManagementUI : MonoBehaviour
{
    [Header("클래스 코드 관리 UI")]
    public TextMeshProUGUI currentClassCodeText;
    public Button generateCodeButton;
    public Button refreshCodeButton;
    public TextMeshProUGUI generateCodeButtonText;
    public TextMeshProUGUI refreshCodeButtonText;
    
    [Header("클래스 정보 UI")]
    public TextMeshProUGUI classInfoText;
    public TextMeshProUGUI teacherNameText;
    public TextMeshProUGUI schoolInfoText;
    
    [Header("학생 목록 UI")]
    public TextMeshProUGUI studentListTitle;
    public Transform studentListContainer;
    public GameObject studentItemPrefab;
    public ScrollRect studentScrollRect;
    
    [Header("학생 추가/제거 UI")]
    public GameObject addStudentPanel;
    public TMP_InputField addStudentNumberInput;
    public TMP_InputField addStudentNameInput;
    public Button addStudentButton;
    public Button cancelAddButton;
    public Button showAddPanelButton;
    
    [Header("메시지 UI")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI statusText;
    
    [Header("UI 설정")]
    public Color primaryColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    public Color successColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color errorColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    public Color warningColor = new Color(0.9f, 0.6f, 0.2f, 1f);
    
    private List<GameObject> studentItems = new List<GameObject>();
    private SchoolData currentSchoolData;
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeUI();
        LoadSchoolData();
        RefreshUI();
    }
    
    private void InitializeUI()
    {
        SetupButtonListeners();
        SetupUI();
        
        // 학생 추가 패널 초기에 숨기기
        if (addStudentPanel != null)
        {
            addStudentPanel.SetActive(false);
        }
        
        isInitialized = true;
        Debug.Log("TeacherClassManagementUI 초기화 완료");
    }
    
    private void SetupButtonListeners()
    {
        if (generateCodeButton != null)
        {
            generateCodeButton.onClick.AddListener(OnGenerateCodeButtonClick);
        }
        
        if (refreshCodeButton != null)
        {
            refreshCodeButton.onClick.AddListener(OnRefreshCodeButtonClick);
        }
        
        if (addStudentButton != null)
        {
            addStudentButton.onClick.AddListener(OnAddStudentButtonClick);
        }
        
        if (cancelAddButton != null)
        {
            cancelAddButton.onClick.AddListener(OnCancelAddButtonClick);
        }
        
        if (showAddPanelButton != null)
        {
            showAddPanelButton.onClick.AddListener(OnShowAddPanelButtonClick);
        }
    }
    
    private void SetupUI()
    {
        // 버튼 텍스트 설정
        if (generateCodeButtonText != null)
        {
            generateCodeButtonText.text = "새 클래스 코드 생성";
            generateCodeButtonText.color = Color.white;
        }
        
        if (refreshCodeButtonText != null)
        {
            refreshCodeButtonText.text = "새로고침";
            refreshCodeButtonText.color = Color.white;
        }
        
        // 학생 목록 제목
        if (studentListTitle != null)
        {
            studentListTitle.text = "학생 목록";
            studentListTitle.fontSize = 32;
            studentListTitle.fontStyle = FontStyles.Bold;
        }
        
        // 버튼 색상 설정
        SetButtonColor(generateCodeButton, primaryColor);
        SetButtonColor(refreshCodeButton, new Color(0.6f, 0.6f, 0.6f, 1f));
        SetButtonColor(addStudentButton, successColor);
        SetButtonColor(cancelAddButton, errorColor);
        SetButtonColor(showAddPanelButton, primaryColor);
        
        // 입력 필드 설정
        SetupInputFields();
        
        // 메시지 텍스트 숨기기
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
    
    private void SetupInputFields()
    {
        if (addStudentNumberInput != null)
        {
            addStudentNumberInput.placeholder.GetComponent<TextMeshProUGUI>().text = "학생 번호";
            addStudentNumberInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            addStudentNumberInput.characterLimit = 2;
            SetKoreanFont(addStudentNumberInput);
        }
        
        if (addStudentNameInput != null)
        {
            addStudentNameInput.placeholder.GetComponent<TextMeshProUGUI>().text = "학생 이름";
            addStudentNameInput.characterLimit = 10;
            SetKoreanFont(addStudentNameInput);
        }
    }
    
    private void SetKoreanFont(TMP_InputField inputField)
    {
        if (inputField != null && inputField.textComponent != null)
        {
            var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
            if (koreanFont != null)
            {
                inputField.textComponent.font = koreanFont;
                if (inputField.placeholder != null)
                {
                    inputField.placeholder.GetComponent<TextMeshProUGUI>().font = koreanFont;
                }
            }
        }
    }
    
    private void SetButtonColor(Button button, Color color)
    {
        if (button != null)
        {
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = color;
            }
        }
    }
    
    private void LoadSchoolData()
    {
        currentSchoolData = StudentDataManager.LoadSchoolData();
        
        if (currentSchoolData == null)
        {
            ShowMessage("학교 데이터를 찾을 수 없습니다.\n초기 설정을 다시 진행해주세요.", errorColor);
            return;
        }
        
        Debug.Log($"학교 데이터 로드 완료: {currentSchoolData.teacherInfo.className}");
    }
    
    private void RefreshUI()
    {
        if (!isInitialized || currentSchoolData == null)
            return;
        
        UpdateClassCodeDisplay();
        UpdateClassInfoDisplay();
        UpdateStudentList();
        UpdateStatusDisplay();
    }
    
    private void UpdateClassCodeDisplay()
    {
        string currentCode = ClassCodeManager.GetCurrentClassCode();
        
        if (currentClassCodeText != null)
        {
            if (string.IsNullOrEmpty(currentCode))
            {
                currentClassCodeText.text = "클래스 코드: 없음";
                currentClassCodeText.color = warningColor;
            }
            else
            {
                currentClassCodeText.text = $"클래스 코드: {currentCode}";
                currentClassCodeText.color = primaryColor;
            }
            
            currentClassCodeText.fontSize = 36;
            currentClassCodeText.fontStyle = FontStyles.Bold;
        }
    }
    
    private void UpdateClassInfoDisplay()
    {
        if (classInfoText != null)
        {
            classInfoText.text = $"{currentSchoolData.teacherInfo.grade} {currentSchoolData.teacherInfo.className}";
            classInfoText.fontSize = 28;
        }
        
        if (teacherNameText != null)
        {
            teacherNameText.text = $"담임: {currentSchoolData.teacherInfo.teacherName}";
            teacherNameText.fontSize = 24;
        }
        
        if (schoolInfoText != null)
        {
            schoolInfoText.text = $"{currentSchoolData.teacherInfo.schoolName}";
            schoolInfoText.fontSize = 22;
        }
    }
    
    private void UpdateStudentList()
    {
        if (studentListContainer == null)
            return;
        
        // 기존 학생 항목 제거
        foreach (var item in studentItems)
        {
            if (item != null)
                Destroy(item);
        }
        studentItems.Clear();
        
        // 학생 목록 생성
        if (currentSchoolData.students != null)
        {
            foreach (var student in currentSchoolData.students)
            {
                CreateStudentItem(student);
            }
        }
        
        // 레이아웃 강제 업데이트
        Canvas.ForceUpdateCanvases();
        if (studentScrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(studentListContainer.GetComponent<RectTransform>());
        }
    }
    
    private void CreateStudentItem(StudentInfo student)
    {
        GameObject studentItem;
        
        // 프리팹이 있으면 사용, 없으면 직접 생성
        if (studentItemPrefab != null)
        {
            studentItem = Instantiate(studentItemPrefab, studentListContainer);
        }
        else
        {
            studentItem = CreateStudentItemFromScratch(student);
        }
        
        if (studentItem == null)
            return;
        
        // 학생 정보 설정
        SetupStudentItemContent(studentItem, student);
        
        studentItems.Add(studentItem);
    }
    
    private GameObject CreateStudentItemFromScratch(StudentInfo student)
    {
        GameObject studentItem = new GameObject($"Student_{student.number}");
        studentItem.transform.SetParent(studentListContainer, false);
        
        // 배경 이미지 추가
        Image backgroundImage = studentItem.AddComponent<Image>();
        backgroundImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        
        // 레이아웃 설정
        RectTransform rectTransform = studentItem.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, 60);
        
        LayoutElement layoutElement = studentItem.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 60;
        layoutElement.flexibleWidth = 1;
        
        // 수평 레이아웃 그룹 추가
        HorizontalLayoutGroup horizontalLayout = studentItem.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.childControlHeight = true;
        horizontalLayout.childControlWidth = false;
        horizontalLayout.childForceExpandHeight = false;
        horizontalLayout.childForceExpandWidth = false;
        horizontalLayout.spacing = 10;
        horizontalLayout.padding = new RectOffset(15, 15, 10, 10);
        
        // 학생 번호 텍스트
        GameObject numberTextGO = new GameObject("NumberText");
        numberTextGO.transform.SetParent(studentItem.transform, false);
        TextMeshProUGUI numberText = numberTextGO.AddComponent<TextMeshProUGUI>();
        numberText.text = $"{student.number}번";
        numberText.fontSize = 24;
        numberText.color = primaryColor;
        numberText.fontStyle = FontStyles.Bold;
        
        LayoutElement numberLayout = numberTextGO.AddComponent<LayoutElement>();
        numberLayout.preferredWidth = 80;
        
        // 학생 이름 텍스트
        GameObject nameTextGO = new GameObject("NameText");
        nameTextGO.transform.SetParent(studentItem.transform, false);
        TextMeshProUGUI nameText = nameTextGO.AddComponent<TextMeshProUGUI>();
        nameText.text = student.name;
        nameText.fontSize = 24;
        nameText.color = Color.black;
        
        LayoutElement nameLayout = nameTextGO.AddComponent<LayoutElement>();
        nameLayout.flexibleWidth = 1;
        
        // 삭제 버튼
        GameObject deleteButtonGO = new GameObject("DeleteButton");
        deleteButtonGO.transform.SetParent(studentItem.transform, false);
        Image deleteButtonImage = deleteButtonGO.AddComponent<Image>();
        deleteButtonImage.color = errorColor;
        Button deleteButton = deleteButtonGO.AddComponent<Button>();
        
        // 삭제 버튼 텍스트
        GameObject deleteTextGO = new GameObject("Text");
        deleteTextGO.transform.SetParent(deleteButtonGO.transform, false);
        TextMeshProUGUI deleteText = deleteTextGO.AddComponent<TextMeshProUGUI>();
        deleteText.text = "삭제";
        deleteText.fontSize = 20;
        deleteText.color = Color.white;
        deleteText.alignment = TextAlignmentOptions.Center;
        
        RectTransform deleteTextRect = deleteTextGO.GetComponent<RectTransform>();
        deleteTextRect.anchorMin = Vector2.zero;
        deleteTextRect.anchorMax = Vector2.one;
        deleteTextRect.sizeDelta = Vector2.zero;
        deleteTextRect.anchoredPosition = Vector2.zero;
        
        LayoutElement deleteLayout = deleteButtonGO.AddComponent<LayoutElement>();
        deleteLayout.preferredWidth = 60;
        
        // 삭제 버튼 이벤트
        deleteButton.onClick.AddListener(() => OnRemoveStudentButtonClick(student.number));
        
        return studentItem;
    }
    
    private void SetupStudentItemContent(GameObject studentItem, StudentInfo student)
    {
        // 프리팹 사용 시 내용 설정
        TextMeshProUGUI numberText = studentItem.transform.Find("NumberText")?.GetComponent<TextMeshProUGUI>();
        if (numberText != null)
        {
            numberText.text = $"{student.number}번";
        }
        
        TextMeshProUGUI nameText = studentItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = student.name;
        }
        
        Button deleteButton = studentItem.transform.Find("DeleteButton")?.GetComponent<Button>();
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => OnRemoveStudentButtonClick(student.number));
        }
    }
    
    private void UpdateStatusDisplay()
    {
        if (statusText != null)
        {
            int studentCount = currentSchoolData.students?.Count ?? 0;
            statusText.text = $"총 학생 수: {studentCount}명";
            statusText.fontSize = 20;
        }
    }
    
    private void OnGenerateCodeButtonClick()
    {
        if (currentSchoolData == null)
        {
            ShowMessage("학교 데이터를 먼저 로드해주세요.", errorColor);
            return;
        }
        
        // 새 클래스 코드 생성
        string newCode = ClassCodeManager.GenerateClassCode(currentSchoolData.teacherInfo.className);
        
        if (string.IsNullOrEmpty(newCode))
        {
            ShowMessage("클래스 코드 생성에 실패했습니다.", errorColor);
            return;
        }
        
        // 클래스 코드 데이터 생성 및 저장
        ClassCodeData codeData = ClassCodeManager.CreateClassCodeDataFromSchoolData(currentSchoolData, newCode);
        ClassCodeManager.SetCurrentClassCode(newCode, codeData);
        
        ShowMessage($"새 클래스 코드가 생성되었습니다: {newCode}", successColor);
        
        // UI 업데이트
        UpdateClassCodeDisplay();
        
        Debug.Log($"새 클래스 코드 생성됨: {newCode}");
    }
    
    private void OnRefreshCodeButtonClick()
    {
        LoadSchoolData();
        RefreshUI();
        ShowMessage("데이터가 새로고침되었습니다.", successColor);
    }
    
    private void OnShowAddPanelButtonClick()
    {
        if (addStudentPanel != null)
        {
            addStudentPanel.SetActive(true);
        }
        
        // 입력 필드 초기화
        if (addStudentNumberInput != null)
        {
            addStudentNumberInput.text = "";
        }
        
        if (addStudentNameInput != null)
        {
            addStudentNameInput.text = "";
        }
    }
    
    private void OnCancelAddButtonClick()
    {
        if (addStudentPanel != null)
        {
            addStudentPanel.SetActive(false);
        }
    }
    
    private void OnAddStudentButtonClick()
    {
        string numberText = addStudentNumberInput?.text?.Trim() ?? "";
        string nameText = addStudentNameInput?.text?.Trim() ?? "";
        
        // 입력 검증
        if (string.IsNullOrEmpty(numberText))
        {
            ShowMessage("학생 번호를 입력해주세요.", errorColor);
            return;
        }
        
        if (string.IsNullOrEmpty(nameText))
        {
            ShowMessage("학생 이름을 입력해주세요.", errorColor);
            return;
        }
        
        if (!int.TryParse(numberText, out int studentNumber))
        {
            ShowMessage("올바른 학생 번호를 입력해주세요.", errorColor);
            return;
        }
        
        if (studentNumber <= 0)
        {
            ShowMessage("학생 번호는 1 이상이어야 합니다.", errorColor);
            return;
        }
        
        // 중복 확인
        if (IsStudentNumberExists(studentNumber))
        {
            ShowMessage($"{studentNumber}번 학생이 이미 존재합니다.", errorColor);
            return;
        }
        
        // 학생 추가
        StudentInfo newStudent = new StudentInfo(studentNumber, nameText);
        currentSchoolData.students.Add(newStudent);
        
        // 데이터 저장
        StudentDataManager.SaveSchoolData(currentSchoolData);
        
        // 클래스 코드 데이터도 업데이트
        string currentCode = ClassCodeManager.GetCurrentClassCode();
        if (!string.IsNullOrEmpty(currentCode))
        {
            ClassCodeManager.AddStudentToClass(currentCode, newStudent);
        }
        
        // UI 업데이트
        RefreshUI();
        
        // 패널 닫기
        if (addStudentPanel != null)
        {
            addStudentPanel.SetActive(false);
        }
        
        ShowMessage($"{newStudent.number}번 {newStudent.name} 학생이 추가되었습니다.", successColor);
        
        Debug.Log($"학생 추가됨: {newStudent.number}번 {newStudent.name}");
    }
    
    private void OnRemoveStudentButtonClick(int studentNumber)
    {
        // 확인 다이얼로그 (간단한 구현)
        if (!ShowConfirmDialog($"{studentNumber}번 학생을 삭제하시겠습니까?"))
        {
            return;
        }
        
        // 학생 찾기 및 제거
        StudentInfo studentToRemove = null;
        foreach (var student in currentSchoolData.students)
        {
            if (student.number == studentNumber)
            {
                studentToRemove = student;
                break;
            }
        }
        
        if (studentToRemove == null)
        {
            ShowMessage($"{studentNumber}번 학생을 찾을 수 없습니다.", errorColor);
            return;
        }
        
        // 학생 제거
        currentSchoolData.students.Remove(studentToRemove);
        
        // 데이터 저장
        StudentDataManager.SaveSchoolData(currentSchoolData);
        
        // 클래스 코드 데이터도 업데이트
        string currentCode = ClassCodeManager.GetCurrentClassCode();
        if (!string.IsNullOrEmpty(currentCode))
        {
            ClassCodeManager.RemoveStudentFromClass(currentCode, studentNumber);
        }
        
        // UI 업데이트
        RefreshUI();
        
        ShowMessage($"{studentToRemove.number}번 {studentToRemove.name} 학생이 삭제되었습니다.", successColor);
        
        Debug.Log($"학생 제거됨: {studentToRemove.number}번 {studentToRemove.name}");
    }
    
    private bool IsStudentNumberExists(int studentNumber)
    {
        if (currentSchoolData?.students == null)
            return false;
        
        foreach (var student in currentSchoolData.students)
        {
            if (student.number == studentNumber)
                return true;
        }
        
        return false;
    }
    
    private bool ShowConfirmDialog(string message)
    {
        // 간단한 확인 다이얼로그 구현
        // 실제 프로젝트에서는 더 정교한 다이얼로그를 구현할 수 있음
        Debug.Log($"확인 다이얼로그: {message}");
        return true; // 임시로 항상 true 반환
    }
    
    private void ShowMessage(string message, Color color)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
            messageText.gameObject.SetActive(true);
            
            // 3초 후 메시지 숨기기
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), 3f);
        }
        
        Debug.Log($"메시지: {message}");
    }
    
    private void HideMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
    
    // 외부에서 호출할 수 있는 새로고침 메서드
    public void RefreshData()
    {
        LoadSchoolData();
        RefreshUI();
    }
    
    // 클래스 코드 생성 상태 확인
    public bool HasClassCode()
    {
        return !string.IsNullOrEmpty(ClassCodeManager.GetCurrentClassCode());
    }
    
    // 현재 클래스 코드 반환
    public string GetCurrentClassCode()
    {
        return ClassCodeManager.GetCurrentClassCode();
    }
    
    // 학생 수 반환
    public int GetStudentCount()
    {
        return currentSchoolData?.students?.Count ?? 0;
    }
}
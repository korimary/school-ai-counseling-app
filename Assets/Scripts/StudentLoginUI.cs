using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StudentLoginUI : MonoBehaviour
{
    [Header("로그인 UI 요소")]
    public TMP_InputField classCodeInput;
    public TMP_InputField studentNumberInput;
    public Button loginButton;
    public Button backButton;
    
    [Header("UI 텍스트")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI classCodeLabel;
    public TextMeshProUGUI studentNumberLabel;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI loginButtonText;
    public TextMeshProUGUI backButtonText;
    
    [Header("UI 설정")]
    public Color primaryColor = new Color(0.9f, 0.4f, 0.4f, 1f);
    public Color errorColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    public Color successColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    
    private bool isLoggingIn = false;
    
    void Start()
    {
        // UI가 없으면 자동으로 생성
        if (titleText == null || classCodeInput == null || studentNumberInput == null || 
            loginButton == null || backButton == null || errorText == null)
        {
            Debug.Log("UI 요소가 없어서 자동 생성합니다!");
            CreateUILayout();
        }
        else
        {
            SetupUI();
            SetupButtonListeners();
        }
    }
    
    private void SetupUI()
    {
        // 타이틀 설정
        if (titleText != null)
        {
            titleText.text = "학생 로그인";
            titleText.fontSize = 60;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = primaryColor;
        }
        
        // 라벨 텍스트 설정
        if (classCodeLabel != null)
        {
            classCodeLabel.text = "클래스 코드";
            classCodeLabel.fontSize = 28;
        }
        
        if (studentNumberLabel != null)
        {
            studentNumberLabel.text = "학생 번호";
            studentNumberLabel.fontSize = 28;
        }
        
        // 입력 필드 설정
        SetupInputFields();
        
        // 버튼 텍스트 설정
        if (loginButtonText != null)
        {
            loginButtonText.text = "로그인";
            loginButtonText.fontSize = 32;
            loginButtonText.color = Color.white;
        }
        
        if (backButtonText != null)
        {
            backButtonText.text = "뒤로가기";
            backButtonText.fontSize = 28;
            backButtonText.color = Color.white;
        }
        
        // 버튼 색상 설정
        if (loginButton != null)
        {
            var loginButtonImage = loginButton.GetComponent<Image>();
            if (loginButtonImage != null)
            {
                loginButtonImage.color = primaryColor;
            }
        }
        
        if (backButton != null)
        {
            var backButtonImage = backButton.GetComponent<Image>();
            if (backButtonImage != null)
            {
                backButtonImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }
        }
        
        // 에러 텍스트 숨기기
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }
    
    private void SetupInputFields()
    {
        // 클래스 코드 입력 필드 설정
        if (classCodeInput != null)
        {
            if (classCodeInput.placeholder != null)
            {
                var placeholderText = classCodeInput.placeholder.GetComponent<TextMeshProUGUI>();
                if (placeholderText != null)
                    placeholderText.text = "예: 바다반-1234";
            }
            classCodeInput.characterLimit = 20;
            
            // 한글 폰트 설정
            SetKoreanFont(classCodeInput);
            
            // 입력 검증 추가
            classCodeInput.onValueChanged.AddListener(OnClassCodeChanged);
        }
        
        // 학생 번호 입력 필드 설정
        if (studentNumberInput != null)
        {
            studentNumberInput.placeholder.GetComponent<TextMeshProUGUI>().text = "예: 1";
            studentNumberInput.characterLimit = 2;
            studentNumberInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            
            // 한글 폰트 설정
            SetKoreanFont(studentNumberInput);
            
            // 입력 검증 추가
            studentNumberInput.onValueChanged.AddListener(OnStudentNumberChanged);
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
                    var placeholderText = inputField.placeholder.GetComponent<TextMeshProUGUI>();
                    if (placeholderText != null)
                        placeholderText.font = koreanFont;
                }
            }
        }
    }
    
    private void SetupButtonListeners()
    {
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnLoginButtonClick);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }
    }
    
    private void OnClassCodeChanged(string value)
    {
        ValidateInput();
    }
    
    private void OnStudentNumberChanged(string value)
    {
        ValidateInput();
    }
    
    private void ValidateInput()
    {
        bool isValid = !string.IsNullOrEmpty(classCodeInput.text) && 
                      !string.IsNullOrEmpty(studentNumberInput.text);
        
        if (loginButton != null)
        {
            loginButton.interactable = isValid && !isLoggingIn;
        }
    }
    
    private void OnLoginButtonClick()
    {
        if (isLoggingIn)
            return;
        
        string classCode = classCodeInput.text.Trim();
        string studentNumberText = studentNumberInput.text.Trim();
        
        // 입력 검증
        if (string.IsNullOrEmpty(classCode))
        {
            ShowError("클래스 코드를 입력해주세요.");
            return;
        }
        
        if (string.IsNullOrEmpty(studentNumberText))
        {
            ShowError("학생 번호를 입력해주세요.");
            return;
        }
        
        if (!int.TryParse(studentNumberText, out int studentNumber))
        {
            ShowError("올바른 학생 번호를 입력해주세요.");
            return;
        }
        
        if (studentNumber <= 0)
        {
            ShowError("학생 번호는 1 이상이어야 합니다.");
            return;
        }
        
        // 로그인 처리
        ProcessLogin(classCode, studentNumber);
    }
    
    private void ProcessLogin(string classCode, int studentNumber)
    {
        isLoggingIn = true;
        
        // 로그인 버튼 비활성화
        if (loginButton != null)
        {
            loginButton.interactable = false;
        }
        
        // 로딩 표시
        if (loginButtonText != null)
        {
            loginButtonText.text = "로그인 중...";
        }
        
        // 클래스 코드 검증
        if (!ClassCodeManager.ValidateClassCode(classCode))
        {
            ShowError("올바른 클래스 코드 형식이 아닙니다.\n(예: 바다반-1234)");
            ResetLoginButton();
            return;
        }
        
        // 학생 로그인 검증
        StudentInfo studentInfo;
        if (!ClassCodeManager.ValidateStudentLogin(classCode, studentNumber, out studentInfo))
        {
            ShowError("클래스 코드 또는 학생 번호가 올바르지 않습니다.\n선생님께 확인해주세요.");
            ResetLoginButton();
            return;
        }
        
        // 로그인 성공
        OnLoginSuccess(studentInfo);
    }
    
    private void OnLoginSuccess(StudentInfo studentInfo)
    {
        Debug.Log($"학생 로그인 성공: {studentInfo.number}번 {studentInfo.name}");
        
        // UserManager에 학생 정보 설정
        UserManager.SetStudentInfo(studentInfo.number, studentInfo.name);
        
        // 성공 메시지 표시
        if (errorText != null)
        {
            errorText.text = $"로그인 성공!\n{studentInfo.name}님 안녕하세요.";
            errorText.color = successColor;
            errorText.gameObject.SetActive(true);
        }
        
        // 로그인 버튼 텍스트 변경
        if (loginButtonText != null)
        {
            loginButtonText.text = "로그인 완료";
        }
        
        // 1초 후 StudentEmotionScene으로 이동
        Invoke(nameof(LoadStudentEmotionScene), 1.5f);
    }
    
    private void LoadStudentEmotionScene()
    {
        Debug.Log("StudentEmotionScene으로 이동 중...");
        SceneManager.LoadScene("StudentEmotionScene");
    }
    
    private void ResetLoginButton()
    {
        isLoggingIn = false;
        
        if (loginButton != null)
        {
            loginButton.interactable = true;
        }
        
        if (loginButtonText != null)
        {
            loginButtonText.text = "로그인";
        }
    }
    
    private void OnBackButtonClick()
    {
        Debug.Log("뒤로가기 버튼 클릭");
        
        // UserManager에서 학생 모드 초기화
        UserManager.SetUserMode(UserManager.UserMode.None);
        
        // 메인 메뉴로 이동
        SceneManager.LoadScene("MainMenuScene");
    }
    
    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = errorColor;
            errorText.gameObject.SetActive(true);
            
            // 5초 후 에러 메시지 숨기기
            CancelInvoke(nameof(HideError));
            Invoke(nameof(HideError), 5f);
        }
        
        Debug.LogWarning($"학생 로그인 오류: {message}");
    }
    
    private void HideError()
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }
    
    // UI 레이아웃을 프로그래밍적으로 생성하는 헬퍼 메서드
    [ContextMenu("Create Complete UI Layout")]
    public void CreateUILayout()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log("StudentLoginScene UI 생성 시작!");
        
        // 기존 UI 정리
        ClearExistingUI(canvas);
        
        // 배경 패널 생성
        CreateLoginPanel(canvas);
        
        // 타이틀 생성
        CreateTitle(canvas);
        
        // 라벨 생성
        CreateLabels(canvas);
        
        // 로그인 폼 생성
        CreateLoginForm(canvas);
        
        // 버튼 생성
        CreateButtons(canvas);
        
        // 에러 텍스트 생성
        CreateErrorText(canvas);
        
        // 로딩 패널 생성
        CreateLoadingPanel(canvas);
        
        // UI 설정 적용
        SetupUI();
        SetupButtonListeners();
        
        Debug.Log("StudentLoginScene UI 생성 완료! 🎉");
    }
    
    private void ClearExistingUI(Canvas canvas)
    {
        // StudentLoginManager 제외하고 모든 UI 요소 제거
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvas.transform.GetChild(i);
            if (child.name != "StudentLoginManager")
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
    
    private void CreateLoginPanel(Canvas canvas)
    {
        GameObject loginPanelGO = new GameObject("LoginPanel");
        loginPanelGO.transform.SetParent(canvas.transform, false);
        
        Image loginPanelImage = loginPanelGO.AddComponent<Image>();
        loginPanelImage.color = new Color(0.95f, 0.95f, 0.95f, 0.8f);
        
        RectTransform loginPanelRect = loginPanelGO.GetComponent<RectTransform>();
        loginPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        loginPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        loginPanelRect.sizeDelta = new Vector2(500, 600);
        loginPanelRect.anchoredPosition = Vector2.zero;
    }
    
    private void CreateTitle(Canvas canvas)
    {
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(canvas.transform, false);
        titleText = titleGO.AddComponent<TextMeshProUGUI>();
        
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(400, 80);
        titleRect.anchoredPosition = Vector2.zero;
        
        titleText.text = "학생 로그인";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.2f, 0.4f, 0.8f, 1f);
        
        // 한글 폰트 적용
        var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
        if (koreanFont != null)
            titleText.font = koreanFont;
    }
    
    private void CreateLabels(Canvas canvas)
    {
        // 클래스 코드 라벨
        GameObject classCodeLabelGO = new GameObject("ClassCodeLabel");
        classCodeLabelGO.transform.SetParent(canvas.transform, false);
        classCodeLabel = classCodeLabelGO.AddComponent<TextMeshProUGUI>();
        
        RectTransform classCodeLabelRect = classCodeLabelGO.GetComponent<RectTransform>();
        classCodeLabelRect.anchorMin = new Vector2(0.5f, 0.7f);
        classCodeLabelRect.anchorMax = new Vector2(0.5f, 0.7f);
        classCodeLabelRect.sizeDelta = new Vector2(400, 40);
        classCodeLabelRect.anchoredPosition = new Vector2(-150, 20);
        
        classCodeLabel.text = "클래스 코드";
        classCodeLabel.fontSize = 24;
        classCodeLabel.alignment = TextAlignmentOptions.Left;
        classCodeLabel.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        // 학생 번호 라벨
        GameObject studentNumberLabelGO = new GameObject("StudentNumberLabel");
        studentNumberLabelGO.transform.SetParent(canvas.transform, false);
        studentNumberLabel = studentNumberLabelGO.AddComponent<TextMeshProUGUI>();
        
        RectTransform studentNumberLabelRect = studentNumberLabelGO.GetComponent<RectTransform>();
        studentNumberLabelRect.anchorMin = new Vector2(0.5f, 0.55f);
        studentNumberLabelRect.anchorMax = new Vector2(0.5f, 0.55f);
        studentNumberLabelRect.sizeDelta = new Vector2(400, 40);
        studentNumberLabelRect.anchoredPosition = new Vector2(-150, 20);
        
        studentNumberLabel.text = "학생 번호";
        studentNumberLabel.fontSize = 24;
        studentNumberLabel.alignment = TextAlignmentOptions.Left;
        studentNumberLabel.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        // 한글 폰트 적용
        var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
        if (koreanFont != null)
        {
            classCodeLabel.font = koreanFont;
            studentNumberLabel.font = koreanFont;
        }
    }
    
    private void CreateLoginForm(Canvas canvas)
    {
        // 클래스 코드 입력 필드
        GameObject classCodeGO = new GameObject("ClassCodeInput");
        classCodeGO.transform.SetParent(canvas.transform, false);
        
        Image classCodeImage = classCodeGO.AddComponent<Image>();
        classCodeImage.color = Color.white;
        classCodeImage.sprite = null;
        classCodeInput = classCodeGO.AddComponent<TMP_InputField>();
        
        RectTransform classCodeRect = classCodeGO.GetComponent<RectTransform>();
        classCodeRect.anchorMin = new Vector2(0.5f, 0.65f);
        classCodeRect.anchorMax = new Vector2(0.5f, 0.65f);
        classCodeRect.sizeDelta = new Vector2(400, 50);
        classCodeRect.anchoredPosition = Vector2.zero;
        
        // 텍스트 컴포넌트 추가
        GameObject classCodeTextGO = new GameObject("Text");
        classCodeTextGO.transform.SetParent(classCodeGO.transform, false);
        TextMeshProUGUI classCodeTextComponent = classCodeTextGO.AddComponent<TextMeshProUGUI>();
        classCodeInput.textComponent = classCodeTextComponent;
        
        RectTransform classCodeTextRect = classCodeTextGO.GetComponent<RectTransform>();
        classCodeTextRect.anchorMin = Vector2.zero;
        classCodeTextRect.anchorMax = Vector2.one;
        classCodeTextRect.sizeDelta = Vector2.zero;
        classCodeTextRect.offsetMin = new Vector2(10, 5);
        classCodeTextRect.offsetMax = new Vector2(-10, -5);
        
        classCodeTextComponent.fontSize = 20;
        classCodeTextComponent.color = Color.black;
        
        // 플레이스홀더 추가
        GameObject classCodePlaceholderGO = new GameObject("Placeholder");
        classCodePlaceholderGO.transform.SetParent(classCodeGO.transform, false);
        TextMeshProUGUI classCodePlaceholderComponent = classCodePlaceholderGO.AddComponent<TextMeshProUGUI>();
        classCodePlaceholderComponent.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        classCodeInput.placeholder = classCodePlaceholderComponent;
        
        RectTransform classCodePlaceholderRect = classCodePlaceholderGO.GetComponent<RectTransform>();
        classCodePlaceholderRect.anchorMin = Vector2.zero;
        classCodePlaceholderRect.anchorMax = Vector2.one;
        classCodePlaceholderRect.sizeDelta = Vector2.zero;
        classCodePlaceholderRect.offsetMin = new Vector2(10, 5);
        classCodePlaceholderRect.offsetMax = new Vector2(-10, -5);
        
        classCodePlaceholderComponent.text = "예: 바다반-1234";
        classCodePlaceholderComponent.fontSize = 20;
        
        // 학생 번호 입력 필드
        GameObject studentNumberGO = new GameObject("StudentNumberInput");
        studentNumberGO.transform.SetParent(canvas.transform, false);
        
        Image studentNumberImage = studentNumberGO.AddComponent<Image>();
        studentNumberImage.color = Color.white;
        studentNumberImage.sprite = null;
        studentNumberInput = studentNumberGO.AddComponent<TMP_InputField>();
        
        RectTransform studentNumberRect = studentNumberGO.GetComponent<RectTransform>();
        studentNumberRect.anchorMin = new Vector2(0.5f, 0.5f);
        studentNumberRect.anchorMax = new Vector2(0.5f, 0.5f);
        studentNumberRect.sizeDelta = new Vector2(400, 50);
        studentNumberRect.anchoredPosition = Vector2.zero;
        
        // 텍스트 컴포넌트 추가
        GameObject studentNumberTextGO = new GameObject("Text");
        studentNumberTextGO.transform.SetParent(studentNumberGO.transform, false);
        TextMeshProUGUI studentNumberTextComponent = studentNumberTextGO.AddComponent<TextMeshProUGUI>();
        studentNumberInput.textComponent = studentNumberTextComponent;
        
        RectTransform studentNumberTextRect = studentNumberTextGO.GetComponent<RectTransform>();
        studentNumberTextRect.anchorMin = Vector2.zero;
        studentNumberTextRect.anchorMax = Vector2.one;
        studentNumberTextRect.sizeDelta = Vector2.zero;
        studentNumberTextRect.offsetMin = new Vector2(10, 5);
        studentNumberTextRect.offsetMax = new Vector2(-10, -5);
        
        studentNumberTextComponent.fontSize = 20;
        studentNumberTextComponent.color = Color.black;
        
        // 플레이스홀더 추가
        GameObject studentNumberPlaceholderGO = new GameObject("Placeholder");
        studentNumberPlaceholderGO.transform.SetParent(studentNumberGO.transform, false);
        TextMeshProUGUI studentNumberPlaceholderComponent = studentNumberPlaceholderGO.AddComponent<TextMeshProUGUI>();
        studentNumberPlaceholderComponent.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        studentNumberInput.placeholder = studentNumberPlaceholderComponent;
        
        RectTransform studentNumberPlaceholderRect = studentNumberPlaceholderGO.GetComponent<RectTransform>();
        studentNumberPlaceholderRect.anchorMin = Vector2.zero;
        studentNumberPlaceholderRect.anchorMax = Vector2.one;
        studentNumberPlaceholderRect.sizeDelta = Vector2.zero;
        studentNumberPlaceholderRect.offsetMin = new Vector2(10, 5);
        studentNumberPlaceholderRect.offsetMax = new Vector2(-10, -5);
        
        studentNumberPlaceholderComponent.text = "예: 1";
        studentNumberPlaceholderComponent.fontSize = 20;
        
        // 설정
        studentNumberInput.characterLimit = 2;
        studentNumberInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        
        // 한글 폰트 적용
        var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
        if (koreanFont != null)
        {
            classCodeTextComponent.font = koreanFont;
            classCodePlaceholderComponent.font = koreanFont;
            studentNumberTextComponent.font = koreanFont;
            studentNumberPlaceholderComponent.font = koreanFont;
        }
    }
    
    private void CreateButtons(Canvas canvas)
    {
        // 로그인 버튼
        GameObject loginButtonGO = new GameObject("LoginButton");
        loginButtonGO.transform.SetParent(canvas.transform, false);
        
        Image loginButtonImage = loginButtonGO.AddComponent<Image>();
        loginButtonImage.color = new Color(0.2f, 0.6f, 1f, 1f); // 파란색
        loginButton = loginButtonGO.AddComponent<Button>();
        
        RectTransform loginButtonRect = loginButtonGO.GetComponent<RectTransform>();
        loginButtonRect.anchorMin = new Vector2(0.5f, 0.3f);
        loginButtonRect.anchorMax = new Vector2(0.5f, 0.3f);
        loginButtonRect.sizeDelta = new Vector2(300, 60);
        loginButtonRect.anchoredPosition = Vector2.zero;
        
        // 버튼 텍스트 추가
        GameObject loginButtonTextGO = new GameObject("Text");
        loginButtonTextGO.transform.SetParent(loginButtonGO.transform, false);
        loginButtonText = loginButtonTextGO.AddComponent<TextMeshProUGUI>();
        loginButtonText.text = "로그인";
        loginButtonText.fontSize = 28;
        loginButtonText.fontStyle = FontStyles.Bold;
        loginButtonText.color = Color.white;
        loginButtonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform loginButtonTextRect = loginButtonTextGO.GetComponent<RectTransform>();
        loginButtonTextRect.anchorMin = Vector2.zero;
        loginButtonTextRect.anchorMax = Vector2.one;
        loginButtonTextRect.sizeDelta = Vector2.zero;
        loginButtonTextRect.anchoredPosition = Vector2.zero;
        
        // 뒤로가기 버튼
        GameObject backButtonGO = new GameObject("BackButton");
        backButtonGO.transform.SetParent(canvas.transform, false);
        
        Image backButtonImage = backButtonGO.AddComponent<Image>();
        backButtonImage.color = new Color(0.6f, 0.6f, 0.6f, 1f); // 회색
        backButton = backButtonGO.AddComponent<Button>();
        
        RectTransform backButtonRect = backButtonGO.GetComponent<RectTransform>();
        backButtonRect.anchorMin = new Vector2(0.5f, 0.15f);
        backButtonRect.anchorMax = new Vector2(0.5f, 0.15f);
        backButtonRect.sizeDelta = new Vector2(200, 50);
        backButtonRect.anchoredPosition = Vector2.zero;
        
        // 버튼 텍스트 추가
        GameObject backButtonTextGO = new GameObject("Text");
        backButtonTextGO.transform.SetParent(backButtonGO.transform, false);
        backButtonText = backButtonTextGO.AddComponent<TextMeshProUGUI>();
        backButtonText.text = "뒤로가기";
        backButtonText.fontSize = 24;
        backButtonText.color = Color.white;
        backButtonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform backButtonTextRect = backButtonTextGO.GetComponent<RectTransform>();
        backButtonTextRect.anchorMin = Vector2.zero;
        backButtonTextRect.anchorMax = Vector2.one;
        backButtonTextRect.sizeDelta = Vector2.zero;
        backButtonTextRect.anchoredPosition = Vector2.zero;
        
        // 한글 폰트 적용
        var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
        if (koreanFont != null)
        {
            loginButtonText.font = koreanFont;
            backButtonText.font = koreanFont;
        }
    }
    
    private void CreateErrorText(Canvas canvas)
    {
        GameObject errorTextGO = new GameObject("ErrorText");
        errorTextGO.transform.SetParent(canvas.transform, false);
        errorText = errorTextGO.AddComponent<TextMeshProUGUI>();
        
        RectTransform errorTextRect = errorTextGO.GetComponent<RectTransform>();
        errorTextRect.anchorMin = new Vector2(0.5f, 0.4f);
        errorTextRect.anchorMax = new Vector2(0.5f, 0.4f);
        errorTextRect.sizeDelta = new Vector2(450, 80);
        errorTextRect.anchoredPosition = Vector2.zero;
        
        errorText.text = "";
        errorText.fontSize = 20;
        errorText.color = new Color(0.8f, 0.2f, 0.2f, 1f); // 빨간색
        errorText.alignment = TextAlignmentOptions.Center;
        errorText.gameObject.SetActive(false); // 처음에는 숨김
        
        // 한글 폰트 적용
        var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
        if (koreanFont != null)
            errorText.font = koreanFont;
    }
    
    private void CreateLoadingPanel(Canvas canvas)
    {
        GameObject loadingPanelGO = new GameObject("LoadingPanel");
        loadingPanelGO.transform.SetParent(canvas.transform, false);
        
        Image loadingPanelImage = loadingPanelGO.AddComponent<Image>();
        loadingPanelImage.color = new Color(0f, 0f, 0f, 0.7f); // 반투명 검은색
        
        RectTransform loadingPanelRect = loadingPanelGO.GetComponent<RectTransform>();
        loadingPanelRect.anchorMin = Vector2.zero;
        loadingPanelRect.anchorMax = Vector2.one;
        loadingPanelRect.sizeDelta = Vector2.zero;
        loadingPanelRect.anchoredPosition = Vector2.zero;
        
        // 로딩 텍스트
        GameObject loadingTextGO = new GameObject("LoadingText");
        loadingTextGO.transform.SetParent(loadingPanelGO.transform, false);
        TextMeshProUGUI loadingText = loadingTextGO.AddComponent<TextMeshProUGUI>();
        
        RectTransform loadingTextRect = loadingTextGO.GetComponent<RectTransform>();
        loadingTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        loadingTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        loadingTextRect.sizeDelta = new Vector2(300, 80);
        loadingTextRect.anchoredPosition = Vector2.zero;
        
        loadingText.text = "로그인 중...";
        loadingText.fontSize = 32;
        loadingText.fontStyle = FontStyles.Bold;
        loadingText.color = Color.white;
        loadingText.alignment = TextAlignmentOptions.Center;
        
        // 한글 폰트 적용
        var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
        if (koreanFont != null)
            loadingText.font = koreanFont;
        
        // 처음에는 숨김
        loadingPanelGO.SetActive(false);
    }
}
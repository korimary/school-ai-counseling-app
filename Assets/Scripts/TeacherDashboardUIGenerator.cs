using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeacherDashboardUIGenerator : MonoBehaviour
{
    [Header("UI Settings")]
    public Canvas targetCanvas;
    public RectTransform uiRoot;
    
    [Header("Color Scheme")]
    public Color primaryColor = new Color(0.2f, 0.6f, 1f);
    public Color secondaryColor = new Color(0.9f, 0.9f, 0.9f);
    public Color accentColor = new Color(1f, 0.5f, 0.2f);
    public Color backgroundColor = new Color(0.95f, 0.95f, 0.95f);
    public Color textColor = new Color(0.2f, 0.2f, 0.2f);
    
    private TeacherDashboardUI dashboardUI;
    
    void Start()
    {
        // Canvas가 없으면 찾기
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();
            
        if (targetCanvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다.");
            return;
        }
        
        // UI Root 생성
        CreateUIRoot();
        
        // 대시보드 UI 생성
        CreateTeacherDashboardUI();
        
        // TeacherDashboardUI 컴포넌트 연결
        ConnectTeacherDashboardUI();
    }
    
    private void CreateUIRoot()
    {
        GameObject rootObj = new GameObject("TeacherDashboardUI");
        uiRoot = rootObj.AddComponent<RectTransform>();
        uiRoot.SetParent(targetCanvas.transform, false);
        
        // 전체 화면으로 설정
        uiRoot.anchorMin = Vector2.zero;
        uiRoot.anchorMax = Vector2.one;
        uiRoot.sizeDelta = Vector2.zero;
        uiRoot.anchoredPosition = Vector2.zero;
        
        // TeacherDashboardUI 컴포넌트 추가
        dashboardUI = rootObj.AddComponent<TeacherDashboardUI>();
        
        // 배경 이미지 추가
        Image bgImage = rootObj.AddComponent<Image>();
        bgImage.color = backgroundColor;
    }
    
    private void CreateTeacherDashboardUI()
    {
        // 메인 컨테이너 생성
        CreateMainContainer();
        
        // 헤더 생성
        CreateHeader();
        
        // 탭 네비게이션 생성
        CreateTabNavigation();
        
        // 패널들 생성
        CreatePanels();
        
        // 팝업들 생성
        CreatePopups();
    }
    
    private void CreateMainContainer()
    {
        GameObject mainContainer = new GameObject("MainContainer");
        RectTransform mainRect = mainContainer.AddComponent<RectTransform>();
        mainRect.SetParent(uiRoot, false);
        
        // 패딩 적용
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.sizeDelta = new Vector2(-40, -40);
        mainRect.anchoredPosition = Vector2.zero;
        
        // 세로 레이아웃 그룹
        VerticalLayoutGroup vlg = mainContainer.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 20;
        vlg.padding = new RectOffset(20, 20, 20, 20);
    }
    
    private void CreateHeader()
    {
        GameObject header = new GameObject("Header");
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.SetParent(uiRoot.Find("MainContainer"), false);
        
        // 헤더 크기
        headerRect.sizeDelta = new Vector2(0, 80);
        
        // 헤더 배경
        Image headerBg = header.AddComponent<Image>();
        headerBg.color = primaryColor;
        
        // 타이틀 텍스트
        GameObject titleObj = new GameObject("TitleText");
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.SetParent(headerRect, false);
        titleRect.anchorMin = new Vector2(0, 0.5f);
        titleRect.anchorMax = new Vector2(1, 0.5f);
        titleRect.sizeDelta = new Vector2(-40, 50);
        titleRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "교사 대시보드";
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        FontManager.ApplyDefaultKoreanFont(titleText);
        
        // 뒤로가기 버튼
        CreateBackButton(headerRect);
    }
    
    private void CreateBackButton(RectTransform parent)
    {
        GameObject backButton = new GameObject("BackButton");
        RectTransform backRect = backButton.AddComponent<RectTransform>();
        backRect.SetParent(parent, false);
        backRect.anchorMin = new Vector2(0, 0.5f);
        backRect.anchorMax = new Vector2(0, 0.5f);
        backRect.sizeDelta = new Vector2(100, 50);
        backRect.anchoredPosition = new Vector2(70, 0);
        
        Button button = backButton.AddComponent<Button>();
        Image buttonImage = backButton.AddComponent<Image>();
        buttonImage.color = accentColor;
        
        // 버튼 텍스트
        GameObject buttonTextObj = new GameObject("Text");
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.SetParent(backRect, false);
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "< 메뉴";
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        FontManager.ApplyDefaultKoreanFont(buttonText);
        
        // TeacherDashboardUI의 backToMenuButton에 할당
        SetFieldValue("backToMenuButton", button);
    }
    
    private void CreateTabNavigation()
    {
        GameObject tabContainer = new GameObject("TabNavigation");
        RectTransform tabRect = tabContainer.AddComponent<RectTransform>();
        tabRect.SetParent(uiRoot.Find("MainContainer"), false);
        tabRect.sizeDelta = new Vector2(0, 60);
        
        // 탭 배경
        Image tabBg = tabContainer.AddComponent<Image>();
        tabBg.color = secondaryColor;
        
        // 가로 레이아웃 그룹
        HorizontalLayoutGroup hlg = tabContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 5;
        hlg.padding = new RectOffset(10, 10, 5, 5);
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        
        // 탭 버튼들 생성
        string[] tabNames = { "개요", "감정통계", "학생목록", "설정" };
        string[] fieldNames = { "overviewTabButton", "emotionStatsTabButton", "studentListTabButton", "settingsTabButton" };
        
        for (int i = 0; i < tabNames.Length; i++)
        {
            Button tabButton = CreateTabButton(tabRect, tabNames[i]);
            SetFieldValue(fieldNames[i], tabButton);
        }
    }
    
    private Button CreateTabButton(RectTransform parent, string text)
    {
        GameObject buttonObj = new GameObject($"Tab_{text}");
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.SetParent(parent, false);
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.white;
        
        // 버튼 텍스트
        GameObject textObj = new GameObject("Text");
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(buttonRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 18;
        buttonText.color = textColor;
        buttonText.alignment = TextAlignmentOptions.Center;
        FontManager.ApplyDefaultKoreanFont(buttonText);
        
        return button;
    }
    
    private void CreatePanels()
    {
        GameObject panelContainer = new GameObject("PanelContainer");
        RectTransform panelRect = panelContainer.AddComponent<RectTransform>();
        panelRect.SetParent(uiRoot.Find("MainContainer"), false);
        
        // 남은 공간 모두 사용
        LayoutElement layoutElement = panelContainer.AddComponent<LayoutElement>();
        layoutElement.flexibleHeight = 1;
        
        // 각 패널 생성
        CreateOverviewPanel(panelRect);
        CreateEmotionStatsPanel(panelRect);
        CreateStudentListPanel(panelRect);
        CreateSettingsPanel(panelRect);
    }
    
    private void CreateOverviewPanel(RectTransform parent)
    {
        GameObject panel = CreatePanel(parent, "OverviewPanel");
        
        // 세로 레이아웃
        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childForceExpandWidth = true;
        
        // 클래스 코드 텍스트
        TextMeshProUGUI classCodeText = CreateInfoText(panel.transform, "클래스: 로딩중...", 24);
        SetFieldValue("classCodeText", classCodeText);
        
        // 통계 컨테이너
        GameObject statsContainer = new GameObject("StatsContainer");
        RectTransform statsRect = statsContainer.AddComponent<RectTransform>();
        statsRect.SetParent(panel.transform, false);
        
        GridLayoutGroup grid = statsContainer.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(250, 100);
        grid.spacing = new Vector2(20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        
        // 통계 카드들 생성
        TextMeshProUGUI totalStudentsText = CreateStatCard(statsContainer.transform, "총 학생 수", "0명");
        TextMeshProUGUI totalSessionsText = CreateStatCard(statsContainer.transform, "전체 세션", "0회");
        TextMeshProUGUI completeSessionsText = CreateStatCard(statsContainer.transform, "완료 세션", "0회");
        TextMeshProUGUI averageImprovementText = CreateStatCard(statsContainer.transform, "평균 개선도", "0.0");
        
        SetFieldValue("totalStudentsText", totalStudentsText);
        SetFieldValue("totalSessionsText", totalSessionsText);
        SetFieldValue("completeSessionsText", completeSessionsText);
        SetFieldValue("averageImprovementText", averageImprovementText);
        
        // 새로고침 버튼
        Button refreshButton = CreateActionButton(panel.transform, "새로고침", primaryColor);
        SetFieldValue("refreshButton", refreshButton);
        
        SetFieldValue("overviewPanel", panel);
    }
    
    private void CreateEmotionStatsPanel(RectTransform parent)
    {
        GameObject panel = CreatePanel(parent, "EmotionStatsPanel");
        panel.SetActive(false);
        
        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childForceExpandWidth = true;
        
        // 감정 통계 헤더
        CreateInfoText(panel.transform, "감정 통계", 24);
        
        // 감정 바 컨테이너
        GameObject barContainer = new GameObject("EmotionBarContainer");
        RectTransform barRect = barContainer.AddComponent<RectTransform>();
        barRect.SetParent(panel.transform, false);
        
        VerticalLayoutGroup barVlg = barContainer.AddComponent<VerticalLayoutGroup>();
        barVlg.spacing = 10;
        barVlg.childForceExpandWidth = true;
        
        ScrollRect scrollRect = barContainer.AddComponent<ScrollRect>();
        scrollRect.content = barRect;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        
        LayoutElement barLayoutElement = barContainer.AddComponent<LayoutElement>();
        barLayoutElement.flexibleHeight = 1;
        
        SetFieldValue("emotionBarContainer", barRect);
        
        // 요약 정보
        TextMeshProUGUI mostCommonText = CreateInfoText(panel.transform, "가장 많은 감정: 없음", 16);
        TextMeshProUGUI bestImprovementText = CreateInfoText(panel.transform, "가장 개선된 감정: 없음", 16);
        
        SetFieldValue("mostCommonEmotionText", mostCommonText);
        SetFieldValue("bestImprovementEmotionText", bestImprovementText);
        
        SetFieldValue("emotionStatsPanel", panel);
    }
    
    private void CreateStudentListPanel(RectTransform parent)
    {
        GameObject panel = CreatePanel(parent, "StudentListPanel");
        panel.SetActive(false);
        
        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childForceExpandWidth = true;
        
        // 헤더 컨테이너
        GameObject headerContainer = new GameObject("HeaderContainer");
        RectTransform headerRect = headerContainer.AddComponent<RectTransform>();
        headerRect.SetParent(panel.transform, false);
        
        HorizontalLayoutGroup hlg = headerContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childForceExpandWidth = false;
        
        // 검색 입력 필드
        TMP_InputField searchInput = CreateInputField(headerContainer.transform, "학생 번호 검색...");
        SetFieldValue("searchStudentInput", searchInput);
        
        // 학생 추가 버튼
        Button addButton = CreateActionButton(headerContainer.transform, "학생 추가", accentColor);
        SetFieldValue("addStudentButton", addButton);
        
        // 학생 목록 컨테이너
        GameObject listContainer = new GameObject("StudentItemContainer");
        RectTransform listRect = listContainer.AddComponent<RectTransform>();
        listRect.SetParent(panel.transform, false);
        
        VerticalLayoutGroup listVlg = listContainer.AddComponent<VerticalLayoutGroup>();
        listVlg.spacing = 10;
        listVlg.childForceExpandWidth = true;
        
        ScrollRect listScrollRect = listContainer.AddComponent<ScrollRect>();
        listScrollRect.content = listRect;
        listScrollRect.vertical = true;
        listScrollRect.horizontal = false;
        
        LayoutElement listLayoutElement = listContainer.AddComponent<LayoutElement>();
        listLayoutElement.flexibleHeight = 1;
        
        SetFieldValue("studentItemContainer", listRect);
        
        SetFieldValue("studentListPanel", panel);
    }
    
    private void CreateSettingsPanel(RectTransform parent)
    {
        GameObject panel = CreatePanel(parent, "SettingsPanel");
        panel.SetActive(false);
        
        VerticalLayoutGroup vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childForceExpandWidth = true;
        
        CreateInfoText(panel.transform, "설정", 24);
        CreateInfoText(panel.transform, "설정 기능은 준비 중입니다.", 16);
        
        SetFieldValue("settingsPanel", panel);
    }
    
    private void CreatePopups()
    {
        // 학생 상세 정보 팝업
        CreateStudentDetailPopup();
        
        // 학생 추가 팝업
        CreateAddStudentPopup();
    }
    
    private void CreateStudentDetailPopup()
    {
        GameObject popup = new GameObject("StudentDetailPopup");
        RectTransform popupRect = popup.AddComponent<RectTransform>();
        popupRect.SetParent(targetCanvas.transform, false);
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.sizeDelta = Vector2.zero;
        popupRect.anchoredPosition = Vector2.zero;
        
        // 배경 오버레이
        Image overlay = popup.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.5f);
        
        // 팝업 컨테이너
        GameObject container = new GameObject("Container");
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.SetParent(popupRect, false);
        containerRect.anchorMin = new Vector2(0.1f, 0.1f);
        containerRect.anchorMax = new Vector2(0.9f, 0.9f);
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        
        Image containerBg = container.AddComponent<Image>();
        containerBg.color = Color.white;
        
        // StudentDetailPopup 컴포넌트 추가
        StudentDetailPopup detailPopup = popup.AddComponent<StudentDetailPopup>();
        
        // 닫기 버튼
        Button closeButton = CreateActionButton(container.transform, "닫기", Color.red);
        
        popup.SetActive(false);
        SetFieldValue("studentDetailPopup", detailPopup);
    }
    
    private void CreateAddStudentPopup()
    {
        GameObject popup = new GameObject("AddStudentPopup");
        RectTransform popupRect = popup.AddComponent<RectTransform>();
        popupRect.SetParent(targetCanvas.transform, false);
        popupRect.anchorMin = Vector2.zero;
        popupRect.anchorMax = Vector2.one;
        popupRect.sizeDelta = Vector2.zero;
        popupRect.anchoredPosition = Vector2.zero;
        
        // 배경 오버레이
        Image overlay = popup.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.5f);
        
        // 팝업 컨테이너
        GameObject container = new GameObject("Container");
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.SetParent(popupRect, false);
        containerRect.anchorMin = new Vector2(0.3f, 0.3f);
        containerRect.anchorMax = new Vector2(0.7f, 0.7f);
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        
        Image containerBg = container.AddComponent<Image>();
        containerBg.color = Color.white;
        
        // 세로 레이아웃
        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childForceExpandWidth = true;
        
        // 제목
        CreateInfoText(container.transform, "학생 추가", 20);
        
        // 입력 필드들
        TMP_InputField numberInput = CreateInputField(container.transform, "학생 번호");
        TMP_InputField nameInput = CreateInputField(container.transform, "학생 이름");
        
        // 버튼 컨테이너
        GameObject buttonContainer = new GameObject("ButtonContainer");
        RectTransform buttonRect = buttonContainer.AddComponent<RectTransform>();
        buttonRect.SetParent(container.transform, false);
        
        HorizontalLayoutGroup hlg = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childForceExpandWidth = true;
        
        Button addButton = CreateActionButton(buttonContainer.transform, "추가", primaryColor);
        Button cancelButton = CreateActionButton(buttonContainer.transform, "취소", Color.gray);
        
        // AddStudentPopup 컴포넌트 추가
        AddStudentPopup addPopup = popup.AddComponent<AddStudentPopup>();
        
        popup.SetActive(false);
        SetFieldValue("addStudentPopup", addPopup);
    }
    
    // 헬퍼 메서드들
    private GameObject CreatePanel(RectTransform parent, string name)
    {
        GameObject panel = new GameObject(name);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.SetParent(parent, false);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = Color.white;
        
        return panel;
    }
    
    private TextMeshProUGUI CreateInfoText(Transform parent, string text, float fontSize)
    {
        GameObject textObj = new GameObject("InfoText");
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(parent, false);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = TextAlignmentOptions.Left;
        
        // 기본 한국어 폰트 적용
        FontManager.ApplyDefaultKoreanFont(textComponent);
        
        LayoutElement layoutElement = textObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = fontSize + 10;
        
        return textComponent;
    }
    
    private TextMeshProUGUI CreateStatCard(Transform parent, string label, string value)
    {
        GameObject card = new GameObject($"StatCard_{label}");
        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.SetParent(parent, false);
        
        Image cardBg = card.AddComponent<Image>();
        cardBg.color = secondaryColor;
        
        VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        
        // 라벨
        GameObject labelObj = new GameObject("Label");
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.SetParent(cardRect, false);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 14;
        labelText.color = textColor;
        labelText.alignment = TextAlignmentOptions.Center;
        FontManager.ApplyDefaultKoreanFont(labelText);
        
        // 값
        GameObject valueObj = new GameObject("Value");
        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.SetParent(cardRect, false);
        
        TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = value;
        valueText.fontSize = 20;
        valueText.fontStyle = FontStyles.Bold;
        valueText.color = primaryColor;
        valueText.alignment = TextAlignmentOptions.Center;
        FontManager.ApplyDefaultKoreanFont(valueText);
        
        return valueText;
    }
    
    private Button CreateActionButton(Transform parent, string text, Color color)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.SetParent(parent, false);
        
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = color;
        
        LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 50;
        layoutElement.preferredWidth = 120;
        
        // 버튼 텍스트
        GameObject textObj = new GameObject("Text");
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(buttonRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        FontManager.ApplyDefaultKoreanFont(buttonText);
        
        return button;
    }
    
    private TMP_InputField CreateInputField(Transform parent, string placeholder)
    {
        GameObject inputObj = new GameObject($"InputField_{placeholder}");
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.SetParent(parent, false);
        
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = Color.white;
        
        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        
        LayoutElement layoutElement = inputObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 40;
        layoutElement.flexibleWidth = 1;
        
        // 텍스트 영역
        GameObject textArea = new GameObject("TextArea");
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.SetParent(inputRect, false);
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = new Vector2(-20, -10);
        textAreaRect.anchoredPosition = Vector2.zero;
        
        // 플레이스홀더
        GameObject placeholderObj = new GameObject("Placeholder");
        RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.SetParent(textAreaRect, false);
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 14;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
        placeholderText.alignment = TextAlignmentOptions.Left;
        FontManager.ApplyDefaultKoreanFont(placeholderText);
        
        // 입력 텍스트
        GameObject inputTextObj = new GameObject("Text");
        RectTransform inputTextRect = inputTextObj.AddComponent<RectTransform>();
        inputTextRect.SetParent(textAreaRect, false);
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.sizeDelta = Vector2.zero;
        inputTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        inputText.fontSize = 14;
        inputText.color = textColor;
        inputText.alignment = TextAlignmentOptions.Left;
        FontManager.ApplyDefaultKoreanFont(inputText);
        
        // InputField 설정
        inputField.textViewport = textAreaRect;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        
        return inputField;
    }
    
    private void ConnectTeacherDashboardUI()
    {
        // 모든 필드가 올바르게 연결되었는지 확인
        if (dashboardUI != null)
        {
            Debug.Log("TeacherDashboardUI 컴포넌트가 생성되고 연결되었습니다.");
        }
    }
    
    private void SetFieldValue(string fieldName, object value)
    {
        if (dashboardUI == null) return;
        
        var field = dashboardUI.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
            
        if (field != null)
        {
            field.SetValue(dashboardUI, value);
            Debug.Log($"연결됨: {fieldName} -> {value?.GetType().Name}");
        }
        else
        {
            Debug.LogWarning($"필드를 찾을 수 없습니다: {fieldName}");
        }
    }
}
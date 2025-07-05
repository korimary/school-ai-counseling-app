using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPrefabGenerator : MonoBehaviour
{
    [Header("생성된 프리팹들")]
    public GameObject emotionBarPrefab;
    public GameObject studentItemPrefab;
    
    [Header("색상 설정")]
    public Color primaryColor = new Color(0.2f, 0.6f, 1f);
    public Color secondaryColor = new Color(0.9f, 0.9f, 0.9f);
    public Color textColor = new Color(0.2f, 0.2f, 0.2f);
    
    void Start()
    {
        CreatePrefabs();
        AssignPrefabsToTeacherDashboard();
    }
    
    private void CreatePrefabs()
    {
        CreateEmotionBarPrefab();
        CreateStudentItemPrefab();
    }
    
    private void CreateEmotionBarPrefab()
    {
        // 메인 컨테이너
        GameObject emotionBar = new GameObject("EmotionBarItem");
        RectTransform barRect = emotionBar.AddComponent<RectTransform>();
        
        // 크기 설정
        barRect.sizeDelta = new Vector2(400, 60);
        
        // 배경 이미지
        Image backgroundImage = emotionBar.AddComponent<Image>();
        backgroundImage.color = secondaryColor;
        
        // 가로 레이아웃 그룹
        HorizontalLayoutGroup hlg = emotionBar.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.padding = new RectOffset(15, 15, 10, 10);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandHeight = true;
        
        // 감정 이름 텍스트
        GameObject emotionNameObj = new GameObject("EmotionNameText");
        RectTransform emotionNameRect = emotionNameObj.AddComponent<RectTransform>();
        emotionNameRect.SetParent(barRect, false);
        
        TextMeshProUGUI emotionNameText = emotionNameObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(emotionNameText);
        emotionNameText.text = "기쁨";
        emotionNameText.fontSize = 16;
        emotionNameText.color = textColor;
        emotionNameText.alignment = TextAlignmentOptions.Left;
        
        LayoutElement emotionNameLayout = emotionNameObj.AddComponent<LayoutElement>();
        emotionNameLayout.preferredWidth = 80;
        
        // 바 컨테이너
        GameObject barContainer = new GameObject("BarContainer");
        RectTransform barContainerRect = barContainer.AddComponent<RectTransform>();
        barContainerRect.SetParent(barRect, false);
        
        LayoutElement barContainerLayout = barContainer.AddComponent<LayoutElement>();
        barContainerLayout.flexibleWidth = 1;
        barContainerLayout.preferredHeight = 20;
        
        // 바 배경
        Image barBackground = barContainer.AddComponent<Image>();
        barBackground.color = Color.gray;
        
        // 바 채우기
        GameObject fillBar = new GameObject("FillBar");
        RectTransform fillRect = fillBar.AddComponent<RectTransform>();
        fillRect.SetParent(barContainerRect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        Image barFillImage = fillBar.AddComponent<Image>();
        barFillImage.color = primaryColor;
        barFillImage.type = Image.Type.Filled;
        barFillImage.fillMethod = Image.FillMethod.Horizontal;
        
        // 카운트 텍스트
        GameObject countObj = new GameObject("CountText");
        RectTransform countRect = countObj.AddComponent<RectTransform>();
        countRect.SetParent(barRect, false);
        
        TextMeshProUGUI countText = countObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(countText);
        countText.text = "0회";
        countText.fontSize = 14;
        countText.color = textColor;
        countText.alignment = TextAlignmentOptions.Center;
        
        LayoutElement countLayout = countObj.AddComponent<LayoutElement>();
        countLayout.preferredWidth = 50;
        
        // 개선도 텍스트
        GameObject improvementObj = new GameObject("ImprovementText");
        RectTransform improvementRect = improvementObj.AddComponent<RectTransform>();
        improvementRect.SetParent(barRect, false);
        
        TextMeshProUGUI improvementText = improvementObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(improvementText);
        improvementText.text = "+0.0";
        improvementText.fontSize = 14;
        improvementText.color = Color.green;
        improvementText.alignment = TextAlignmentOptions.Center;
        
        LayoutElement improvementLayout = improvementObj.AddComponent<LayoutElement>();
        improvementLayout.preferredWidth = 60;
        
        // EmotionBarItem 컴포넌트 추가
        EmotionBarItem barComponent = emotionBar.AddComponent<EmotionBarItem>();
        
        // 필드 연결 (리플렉션 사용)
        SetPrivateField(barComponent, "emotionNameText", emotionNameText);
        SetPrivateField(barComponent, "countText", countText);
        SetPrivateField(barComponent, "improvementText", improvementText);
        SetPrivateField(barComponent, "barFillImage", barFillImage);
        SetPrivateField(barComponent, "backgroundImage", backgroundImage);
        
        emotionBarPrefab = emotionBar;
        Debug.Log("EmotionBarPrefab 생성 완료");
    }
    
    private void CreateStudentItemPrefab()
    {
        // 메인 컨테이너
        GameObject studentItem = new GameObject("StudentDashboardItem");
        RectTransform itemRect = studentItem.AddComponent<RectTransform>();
        
        // 크기 설정
        itemRect.sizeDelta = new Vector2(500, 80);
        
        // 배경 이미지
        Image backgroundImage = studentItem.AddComponent<Image>();
        backgroundImage.color = Color.white;
        
        // 테두리 효과를 위한 Outline 추가
        Outline outline = studentItem.AddComponent<Outline>();
        outline.effectColor = secondaryColor;
        outline.effectDistance = new Vector2(2, 2);
        
        // 가로 레이아웃 그룹
        HorizontalLayoutGroup hlg = studentItem.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 15;
        hlg.padding = new RectOffset(20, 20, 15, 15);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandHeight = true;
        
        // 상태 인디케이터
        GameObject statusObj = new GameObject("StatusIndicator");
        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.SetParent(itemRect, false);
        
        Image statusIndicator = statusObj.AddComponent<Image>();
        statusIndicator.color = Color.green;
        
        LayoutElement statusLayout = statusObj.AddComponent<LayoutElement>();
        statusLayout.preferredWidth = 10;
        statusLayout.preferredHeight = 50;
        
        // 학생 번호 텍스트
        GameObject numberObj = new GameObject("StudentNumberText");
        RectTransform numberRect = numberObj.AddComponent<RectTransform>();
        numberRect.SetParent(itemRect, false);
        
        TextMeshProUGUI studentNumberText = numberObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(studentNumberText);
        studentNumberText.text = "1번";
        studentNumberText.fontSize = 18;
        studentNumberText.fontStyle = FontStyles.Bold;
        studentNumberText.color = primaryColor;
        studentNumberText.alignment = TextAlignmentOptions.Left;
        
        LayoutElement numberLayout = numberObj.AddComponent<LayoutElement>();
        numberLayout.preferredWidth = 60;
        
        // 세션 카운트 컨테이너
        GameObject sessionContainer = new GameObject("SessionContainer");
        RectTransform sessionRect = sessionContainer.AddComponent<RectTransform>();
        sessionRect.SetParent(itemRect, false);
        
        VerticalLayoutGroup sessionVlg = sessionContainer.AddComponent<VerticalLayoutGroup>();
        sessionVlg.spacing = 2;
        sessionVlg.childAlignment = TextAnchor.MiddleLeft;
        
        LayoutElement sessionLayout = sessionContainer.AddComponent<LayoutElement>();
        sessionLayout.preferredWidth = 80;
        
        // 세션 라벨
        GameObject sessionLabelObj = new GameObject("SessionLabel");
        TextMeshProUGUI sessionLabel = sessionLabelObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(sessionLabel);
        sessionLabel.text = "세션";
        sessionLabel.fontSize = 12;
        sessionLabel.color = textColor;
        sessionLabelObj.transform.SetParent(sessionContainer.transform, false);
        
        // 세션 카운트
        GameObject sessionCountObj = new GameObject("SessionCountText");
        TextMeshProUGUI sessionCountText = sessionCountObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(sessionCountText);
        sessionCountText.text = "0/0";
        sessionCountText.fontSize = 16;
        sessionCountText.fontStyle = FontStyles.Bold;
        sessionCountText.color = textColor;
        sessionCountObj.transform.SetParent(sessionContainer.transform, false);
        
        // 개선도 컨테이너
        GameObject improvementContainer = new GameObject("ImprovementContainer");
        RectTransform improvementRect = improvementContainer.AddComponent<RectTransform>();
        improvementRect.SetParent(itemRect, false);
        
        VerticalLayoutGroup improvementVlg = improvementContainer.AddComponent<VerticalLayoutGroup>();
        improvementVlg.spacing = 2;
        improvementVlg.childAlignment = TextAnchor.MiddleLeft;
        
        LayoutElement improvementLayout = improvementContainer.AddComponent<LayoutElement>();
        improvementLayout.preferredWidth = 80;
        
        // 개선도 라벨
        GameObject improvementLabelObj = new GameObject("ImprovementLabel");
        TextMeshProUGUI improvementLabel = improvementLabelObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(improvementLabel);
        improvementLabel.text = "개선도";
        improvementLabel.fontSize = 12;
        improvementLabel.color = textColor;
        improvementLabelObj.transform.SetParent(improvementContainer.transform, false);
        
        // 개선도 텍스트
        GameObject improvementTextObj = new GameObject("ImprovementText");
        TextMeshProUGUI improvementText = improvementTextObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(improvementText);
        improvementText.text = "0.0";
        improvementText.fontSize = 16;
        improvementText.fontStyle = FontStyles.Bold;
        improvementText.color = Color.green;
        improvementTextObj.transform.SetParent(improvementContainer.transform, false);
        
        // 최근 감정 컨테이너
        GameObject emotionContainer = new GameObject("EmotionContainer");
        RectTransform emotionRect = emotionContainer.AddComponent<RectTransform>();
        emotionRect.SetParent(itemRect, false);
        
        VerticalLayoutGroup emotionVlg = emotionContainer.AddComponent<VerticalLayoutGroup>();
        emotionVlg.spacing = 2;
        emotionVlg.childAlignment = TextAnchor.MiddleLeft;
        
        LayoutElement emotionLayout = emotionContainer.AddComponent<LayoutElement>();
        emotionLayout.flexibleWidth = 1;
        
        // 감정 라벨
        GameObject emotionLabelObj = new GameObject("EmotionLabel");
        TextMeshProUGUI emotionLabel = emotionLabelObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(emotionLabel);
        emotionLabel.text = "최근 감정";
        emotionLabel.fontSize = 12;
        emotionLabel.color = textColor;
        emotionLabelObj.transform.SetParent(emotionContainer.transform, false);
        
        // 감정 텍스트
        GameObject recentEmotionObj = new GameObject("RecentEmotionText");
        TextMeshProUGUI recentEmotionText = recentEmotionObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(recentEmotionText);
        recentEmotionText.text = "기쁨";
        recentEmotionText.fontSize = 16;
        recentEmotionText.fontStyle = FontStyles.Bold;
        recentEmotionText.color = textColor;
        recentEmotionObj.transform.SetParent(emotionContainer.transform, false);
        
        // 상세보기 버튼
        GameObject detailButtonObj = new GameObject("DetailButton");
        RectTransform detailRect = detailButtonObj.AddComponent<RectTransform>();
        detailRect.SetParent(itemRect, false);
        
        Button detailButton = detailButtonObj.AddComponent<Button>();
        Image detailButtonImage = detailButtonObj.AddComponent<Image>();
        detailButtonImage.color = primaryColor;
        
        LayoutElement detailLayout = detailButtonObj.AddComponent<LayoutElement>();
        detailLayout.preferredWidth = 80;
        detailLayout.preferredHeight = 40;
        
        // 버튼 텍스트
        GameObject buttonTextObj = new GameObject("ButtonText");
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.SetParent(detailRect, false);
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        FontManager.ApplyDefaultKoreanFont(buttonText);
        buttonText.text = "상세보기";
        buttonText.fontSize = 12;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        // StudentDashboardItem 컴포넌트 추가
        StudentDashboardItem itemComponent = studentItem.AddComponent<StudentDashboardItem>();
        
        // 필드 연결 (리플렉션 사용)
        SetPrivateField(itemComponent, "studentNumberText", studentNumberText);
        SetPrivateField(itemComponent, "sessionCountText", sessionCountText);
        SetPrivateField(itemComponent, "improvementText", improvementText);
        SetPrivateField(itemComponent, "recentEmotionText", recentEmotionText);
        SetPrivateField(itemComponent, "detailButton", detailButton);
        SetPrivateField(itemComponent, "statusIndicator", statusIndicator);
        
        studentItemPrefab = studentItem;
        Debug.Log("StudentItemPrefab 생성 완료");
    }
    
    private void AssignPrefabsToTeacherDashboard()
    {
        TeacherDashboardUI dashboardUI = FindObjectOfType<TeacherDashboardUI>();
        if (dashboardUI != null)
        {
            SetPrivateField(dashboardUI, "emotionBarPrefab", emotionBarPrefab);
            SetPrivateField(dashboardUI, "studentItemPrefab", studentItemPrefab);
            Debug.Log("프리팹들이 TeacherDashboardUI에 할당되었습니다.");
        }
        else
        {
            Debug.LogWarning("TeacherDashboardUI를 찾을 수 없습니다.");
        }
    }
    
    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
            
        if (field != null)
        {
            field.SetValue(target, value);
            Debug.Log($"필드 연결 성공: {fieldName}");
        }
        else
        {
            Debug.LogWarning($"필드를 찾을 수 없습니다: {fieldName} in {target.GetType().Name}");
        }
    }
}
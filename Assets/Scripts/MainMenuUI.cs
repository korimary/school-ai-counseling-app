using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public Button teacherModeButton;
    public Button studentModeButton;
    public TextMeshProUGUI teacherButtonText;
    public TextMeshProUGUI studentButtonText;
    
    [Header("UI Settings")]
    public Color primaryColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    public Color secondaryColor = new Color(0.9f, 0.4f, 0.4f, 1f);

    void Start()
    {
        // Inspector에서 UI 요소들을 직접 연결했는지 확인
        if (titleText == null || teacherModeButton == null || studentModeButton == null)
        {
            Debug.LogWarning("UI 요소들이 Inspector에 연결되지 않았습니다. 자동으로 생성합니다.");
            CreateUILayout();
        }
        else
        {
            Debug.Log("Inspector에서 연결된 UI 요소들을 사용합니다.");
        }
        
        SetupUI();
    }

    private void SetupUI()
    {
        // 타이틀 설정
        if (titleText != null)
        {
            titleText.text = "마음쑥쑥";
            titleText.fontSize = 80;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = primaryColor;
        }

        // 교사 모드 버튼 설정
        if (teacherModeButton != null)
        {
            teacherModeButton.onClick.AddListener(OnTeacherModeClicked);
            var teacherButtonImage = teacherModeButton.GetComponent<Image>();
            if (teacherButtonImage != null)
            {
                teacherButtonImage.color = primaryColor;
            }
        }

        if (teacherButtonText != null)
        {
            teacherButtonText.text = "교사 모드";
            teacherButtonText.fontSize = 36;
            teacherButtonText.color = Color.white;
        }

        // 학생 모드 버튼 설정
        if (studentModeButton != null)
        {
            studentModeButton.onClick.AddListener(OnStudentModeClicked);
            var studentButtonImage = studentModeButton.GetComponent<Image>();
            if (studentButtonImage != null)
            {
                studentButtonImage.color = secondaryColor;
            }
        }

        if (studentButtonText != null)
        {
            studentButtonText.text = "학생 모드";
            studentButtonText.fontSize = 36;
            studentButtonText.color = Color.white;
        }

        // 버튼 호버 효과 추가
        AddButtonHoverEffect(teacherModeButton);
        AddButtonHoverEffect(studentModeButton);
    }

    private void OnTeacherModeClicked()
    {
        Debug.Log("교사 모드 선택됨");
        
        // UserManager를 통해 사용자 모드 설정
        UserManager.SetUserMode(UserManager.UserMode.Teacher);
        
        // StartupScene으로 이동
        SceneManager.LoadScene("StartupScene");
    }

    private void OnStudentModeClicked()
    {
        Debug.Log("학생 모드 선택됨");
        
        // UserManager를 통해 사용자 모드 설정
        UserManager.SetUserMode(UserManager.UserMode.Student);
        
        // StudentLoginScene으로 이동
        SceneManager.LoadScene("StudentLoginScene");
    }

    private void AddButtonHoverEffect(Button button)
    {
        if (button == null) return;

        // Button의 ColorBlock 설정으로 호버 효과 추가
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        
        button.colors = colors;
    }

    // UI 레이아웃을 생성하는 헬퍼 메서드 (에디터에서 수동 설정 대신 사용 가능)
    public void CreateUILayout()
    {
        // Canvas 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다!");
            return;
        }
        
        // 한글 폰트 로드
        TMP_FontAsset koreanFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
        if (koreanFont == null)
        {
            Debug.LogWarning("한글 폰트를 찾을 수 없습니다!");
        }
        else
        {
            Debug.Log($"한글 폰트 로드 성공: {koreanFont.name}");
        }

        // 타이틀 생성
        if (titleText == null)
        {
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvas.transform, false);
            titleText = titleGO.AddComponent<TextMeshProUGUI>();
            
            // 한글 폰트 설정
            if (koreanFont != null)
            {
                titleText.font = koreanFont;
            }
            
            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.sizeDelta = new Vector2(600, 150);
            titleRect.anchoredPosition = new Vector2(0, 0);
            
            titleText.alignment = TextAlignmentOptions.Center;
        }

        // 교사 모드 버튼 생성
        if (teacherModeButton == null)
        {
            GameObject teacherButtonGO = new GameObject("TeacherModeButton");
            teacherButtonGO.transform.SetParent(canvas.transform, false);
            
            Image teacherImage = teacherButtonGO.AddComponent<Image>();
            teacherModeButton = teacherButtonGO.AddComponent<Button>();
            
            RectTransform teacherRect = teacherButtonGO.GetComponent<RectTransform>();
            teacherRect.anchorMin = new Vector2(0.5f, 0.5f);
            teacherRect.anchorMax = new Vector2(0.5f, 0.5f);
            teacherRect.sizeDelta = new Vector2(300, 100);
            teacherRect.anchoredPosition = new Vector2(-170, -50);

            GameObject teacherTextGO = new GameObject("Text");
            teacherTextGO.transform.SetParent(teacherButtonGO.transform, false);
            teacherButtonText = teacherTextGO.AddComponent<TextMeshProUGUI>();
            
            // 한글 폰트 설정
            if (koreanFont != null)
            {
                teacherButtonText.font = koreanFont;
            }
            
            RectTransform teacherTextRect = teacherTextGO.GetComponent<RectTransform>();
            teacherTextRect.anchorMin = Vector2.zero;
            teacherTextRect.anchorMax = Vector2.one;
            teacherTextRect.sizeDelta = Vector2.zero;
            teacherTextRect.anchoredPosition = Vector2.zero;
            
            teacherButtonText.alignment = TextAlignmentOptions.Center;
        }

        // 학생 모드 버튼 생성
        if (studentModeButton == null)
        {
            GameObject studentButtonGO = new GameObject("StudentModeButton");
            studentButtonGO.transform.SetParent(canvas.transform, false);
            
            Image studentImage = studentButtonGO.AddComponent<Image>();
            studentModeButton = studentButtonGO.AddComponent<Button>();
            
            RectTransform studentRect = studentButtonGO.GetComponent<RectTransform>();
            studentRect.anchorMin = new Vector2(0.5f, 0.5f);
            studentRect.anchorMax = new Vector2(0.5f, 0.5f);
            studentRect.sizeDelta = new Vector2(300, 100);
            studentRect.anchoredPosition = new Vector2(170, -50);

            GameObject studentTextGO = new GameObject("Text");
            studentTextGO.transform.SetParent(studentButtonGO.transform, false);
            studentButtonText = studentTextGO.AddComponent<TextMeshProUGUI>();
            
            // 한글 폰트 설정
            if (koreanFont != null)
            {
                studentButtonText.font = koreanFont;
            }
            
            RectTransform studentTextRect = studentTextGO.GetComponent<RectTransform>();
            studentTextRect.anchorMin = Vector2.zero;
            studentTextRect.anchorMax = Vector2.one;
            studentTextRect.sizeDelta = Vector2.zero;
            studentTextRect.anchoredPosition = Vector2.zero;
            
            studentButtonText.alignment = TextAlignmentOptions.Center;
            
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class EmotionSelectedEvent : UnityEvent<string> { }

public class EmotionButton : MonoBehaviour
{
    [Header("기존 인터페이스 호환")]
    public EmotionSelectedEvent onEmotionSelected = new EmotionSelectedEvent();
    
    [Header("감정 설정")]
    public string emotionType;
    public string emotionEmoji;
    public Color emotionColor = Color.white;
    
    [Header("UI 컴포넌트")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI buttonText;
    
    [Header("색상 설정")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color hoverColor = Color.yellow;
    
    private bool isSelected = false;
    
    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (button == null)
            button = GetComponent<Button>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    private void Start()
    {
        // 버튼 클릭 이벤트 설정
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        
        // 초기 색상 설정
        SetButtonColor(normalColor);
    }
    
    // 기존 인터페이스 호환: SetEmotion(emotion, emoji, color)
    public void SetEmotion(string emotion, string emoji, Color color)
    {
        emotionType = emotion;
        emotionEmoji = emoji;
        emotionColor = color;
        normalColor = color;
        
        // 텍스트 업데이트
        if (buttonText != null)
        {
            buttonText.text = $"{emoji}\n{emotion}";
        }
        
        // 색상 업데이트
        SetButtonColor(normalColor);
        
        // 한글 폰트 적용
        ApplyKoreanFont();
    }
    
    // 새 인터페이스: SetEmotionType(emotion)
    public void SetEmotionType(string emotion)
    {
        string emoji = GetEmotionEmoji(emotion);
        Color color = GetEmotionColor(emotion);
        SetEmotion(emotion, emoji, color);
    }
    
    // 새 인터페이스: SetEmotionUI (호환성 유지)
    public void SetEmotionUI(StudentEmotionUI ui)
    {
        // UI 참조는 onEmotionSelected 이벤트로 대체됨
        // 별도 처리 불필요
    }
    
    // 기존 인터페이스 호환: GetEmotionName()
    public string GetEmotionName()
    {
        return emotionType;
    }
    
    private void OnButtonClick()
    {
        // 다른 버튼들 선택 해제
        EmotionButton[] allButtons = FindObjectsOfType<EmotionButton>();
        foreach (EmotionButton btn in allButtons)
        {
            btn.SetSelected(false);
        }
        
        // 현재 버튼 선택
        SetSelected(true);
        
        // 이벤트 발생 (기존 인터페이스 호환)
        onEmotionSelected.Invoke(emotionType);
        
        Debug.Log($"감정 선택: {emotionType}");
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selected)
        {
            SetButtonColor(selectedColor);
        }
        else
        {
            SetButtonColor(normalColor);
        }
    }
    
    private void SetButtonColor(Color color)
    {
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }
    
    private void ApplyKoreanFont()
    {
        if (buttonText != null)
        {
            var koreanFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/NotoSansKR-Bold SDF");
            if (koreanFont != null)
            {
                buttonText.font = koreanFont;
            }
        }
    }
    
    private string GetEmotionEmoji(string emotion)
    {
        switch (emotion)
        {
            case "기쁨": return "😊";
            case "슬픔": return "😢";
            case "화남": return "😠";
            case "불안": return "😰";
            case "신남": return "😄";
            case "복잡": return "😕";
            default: return "😐";
        }
    }
    
    private Color GetEmotionColor(string emotion)
    {
        switch (emotion)
        {
            case "기쁨": return new Color(1f, 0.9f, 0.3f); // 노란색
            case "슬픔": return new Color(0.3f, 0.5f, 1f); // 파란색
            case "화남": return new Color(1f, 0.3f, 0.3f); // 빨간색
            case "불안": return new Color(0.8f, 0.3f, 1f); // 보라색
            case "신남": return new Color(1f, 0.5f, 0.3f); // 주황색
            case "복잡": return new Color(0.5f, 0.5f, 0.5f); // 회색
            default: return Color.white;
        }
    }
    
    // 마우스 호버 효과 (선택사항)
    public void OnPointerEnter()
    {
        if (!isSelected)
        {
            SetButtonColor(hoverColor);
        }
    }
    
    public void OnPointerExit()
    {
        if (!isSelected)
        {
            SetButtonColor(normalColor);
        }
    }
}
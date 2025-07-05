using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 이모지를 이미지로 대체하여 표시하는 헬퍼 클래스
/// 폰트 지원 문제를 해결하면서 예쁜 이모지 사용 가능
/// </summary>
public class EmojiHelper : MonoBehaviour
{
    [Header("이모지 스프라이트")]
    public Sprite chartEmoji;      // 📈
    public Sprite starEmoji;       // 🌟
    public Sprite muscleEmoji;     // 💪
    public Sprite targetEmoji;     // 🎯
    public Sprite barChartEmoji;   // 📊
    public Sprite fireEmoji;       // 🔥
    public Sprite sparkleEmoji;    // ✨
    public Sprite partyEmoji;      // 🎉
    public Sprite clipboardEmoji;  // 📋
    public Sprite bulbEmoji;       // 💡
    public Sprite arrowEmoji;      // >
    public Sprite paintEmoji;      // 🎨
    
    [Header("감정 이모지")]
    public Sprite happyEmoji;      // 😊
    public Sprite sadEmoji;        // 😢
    public Sprite angryEmoji;      // 😡
    public Sprite tiredEmoji;      // 😴
    public Sprite thinkingEmoji;   // 🤔
    
    private static EmojiHelper instance;
    public static EmojiHelper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EmojiHelper>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EmojiHelper");
                    instance = go.AddComponent<EmojiHelper>();
                }
            }
            return instance;
        }
    }
    
    /// <summary>
    /// 텍스트에 이모지 이미지를 추가하는 메서드
    /// </summary>
    public void AddEmojiToText(TextMeshProUGUI textComponent, string text, EmojiType emojiType)
    {
        // 기본 텍스트 설정
        textComponent.text = text;
        
        // 이모지 이미지 생성
        GameObject emojiObj = CreateEmojiImage(emojiType);
        if (emojiObj != null)
        {
            // 텍스트 옆에 이모지 배치
            emojiObj.transform.SetParent(textComponent.transform.parent);
            PositionEmojiNextToText(emojiObj, textComponent);
        }
    }
    
    /// <summary>
    /// 이모지 이미지 오브젝트 생성
    /// </summary>
    private GameObject CreateEmojiImage(EmojiType emojiType)
    {
        Sprite emojiSprite = GetEmojiSprite(emojiType);
        if (emojiSprite == null) return null;
        
        GameObject emojiObj = new GameObject($"Emoji_{emojiType}");
        Image emojiImage = emojiObj.AddComponent<Image>();
        emojiImage.sprite = emojiSprite;
        emojiImage.preserveAspect = true;
        
        // 적절한 크기 설정
        RectTransform rectTransform = emojiObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(20, 20);
        
        return emojiObj;
    }
    
    /// <summary>
    /// 이모지를 텍스트 옆에 배치
    /// </summary>
    private void PositionEmojiNextToText(GameObject emojiObj, TextMeshProUGUI textComponent)
    {
        RectTransform emojiRect = emojiObj.GetComponent<RectTransform>();
        RectTransform textRect = textComponent.GetComponent<RectTransform>();
        
        emojiRect.anchorMin = new Vector2(1, 0.5f);
        emojiRect.anchorMax = new Vector2(1, 0.5f);
        emojiRect.pivot = new Vector2(0, 0.5f);
        emojiRect.anchoredPosition = new Vector2(5, 0); // 텍스트 오른쪽에 5픽셀 간격
    }
    
    /// <summary>
    /// 이모지 타입에 따른 스프라이트 반환
    /// </summary>
    private Sprite GetEmojiSprite(EmojiType emojiType)
    {
        switch (emojiType)
        {
            case EmojiType.Chart: return chartEmoji;
            case EmojiType.Star: return starEmoji;
            case EmojiType.Muscle: return muscleEmoji;
            case EmojiType.Target: return targetEmoji;
            case EmojiType.BarChart: return barChartEmoji;
            case EmojiType.Fire: return fireEmoji;
            case EmojiType.Sparkle: return sparkleEmoji;
            case EmojiType.Party: return partyEmoji;
            case EmojiType.Clipboard: return clipboardEmoji;
            case EmojiType.Bulb: return bulbEmoji;
            case EmojiType.Arrow: return arrowEmoji;
            case EmojiType.Paint: return paintEmoji;
            case EmojiType.Happy: return happyEmoji;
            case EmojiType.Sad: return sadEmoji;
            case EmojiType.Angry: return angryEmoji;
            case EmojiType.Tired: return tiredEmoji;
            case EmojiType.Thinking: return thinkingEmoji;
            default: return null;
        }
    }
    
    /// <summary>
    /// 텍스트에서 이모지 문자를 제거하고 이미지로 대체
    /// </summary>
    public string ReplaceEmojiWithImage(string text, out List<EmojiType> emojisFound)
    {
        emojisFound = new List<EmojiType>();
        string cleanText = text;
        
        // 이모지 매핑
        Dictionary<string, EmojiType> emojiMap = new Dictionary<string, EmojiType>
        {
            {"📈", EmojiType.Chart},
            {"🌟", EmojiType.Star},
            {"💪", EmojiType.Muscle},
            {"🎯", EmojiType.Target},
            {"📊", EmojiType.BarChart},
            {"🔥", EmojiType.Fire},
            {"✨", EmojiType.Sparkle},
            {"🎉", EmojiType.Party},
            {"📋", EmojiType.Clipboard},
            {"💡", EmojiType.Bulb},
            {">", EmojiType.Arrow},
            {"🎨", EmojiType.Paint},
            {"😊", EmojiType.Happy},
            {"😢", EmojiType.Sad},
            {"😡", EmojiType.Angry},
            {"😴", EmojiType.Tired},
            {"🤔", EmojiType.Thinking}
        };
        
        foreach (var emoji in emojiMap)
        {
            if (cleanText.Contains(emoji.Key))
            {
                cleanText = cleanText.Replace(emoji.Key, "");
                emojisFound.Add(emoji.Value);
            }
        }
        
        return cleanText;
    }
}

/// <summary>
/// 이모지 타입 열거형
/// </summary>
public enum EmojiType
{
    Chart,      // 📈
    Star,       // 🌟
    Muscle,     // 💪
    Target,     // 🎯
    BarChart,   // 📊
    Fire,       // 🔥
    Sparkle,    // ✨
    Party,      // 🎉
    Clipboard,  // 📋
    Bulb,       // 💡
    Arrow,      // >
    Paint,      // 🎨
    Happy,      // 😊
    Sad,        // 😢
    Angry,      // 😡
    Tired,      // 😴
    Thinking    // 🤔
}
using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 빠른 이모지 수정기
/// 기존 StudentGrowthUI에 바로 추가할 수 있는 간단한 버전
/// </summary>
public class QuickEmojiManager : MonoBehaviour
{
    [Header("빠른 이모지 수정")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool usePrettyMode = true; // true = 예쁜 모드, false = 안전 모드
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixAllEmojisNow();
        }
    }
    
    [ContextMenu("모든 이모지 즉시 수정")]
    public void FixAllEmojisNow()
    {
        Dictionary<string, string> emojiReplacements = GetEmojiReplacements();
        
        // 현재 씬의 모든 TextMeshPro 컴포넌트 찾기
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        int fixedCount = 0;
        
        Debug.Log($"[QuickEmojiManager] {allTexts.Length}개의 TextMeshPro 컴포넌트를 검사 중...");
        
        foreach (var textComponent in allTexts)
        {
            if (string.IsNullOrEmpty(textComponent.text)) continue;
            
            string originalText = textComponent.text;
            string newText = originalText;
            
            // 모든 가능한 이모지 문자 확인
            Debug.Log($"검사 중: {textComponent.gameObject.name} - '{originalText}'");
            
            // 이모지 교체
            foreach (var replacement in emojiReplacements)
            {
                if (newText.Contains(replacement.Key))
                {
                    newText = newText.Replace(replacement.Key, replacement.Value);
                    Debug.Log($"  발견: '{replacement.Key}' → '{replacement.Value}'");
                }
            }
            
            // 추가 문제 문자들 수정
            newText = FixProblematicCharacters(newText);
            
            // 텍스트가 변경된 경우
            if (originalText != newText)
            {
                textComponent.text = newText;
                fixedCount++;
                Debug.Log($"✅ 이모지 수정: {textComponent.gameObject.name} - '{originalText}' → '{newText}'");
            }
        }
        
        string mode = usePrettyMode ? "예쁜 모드" : "안전 모드";
        Debug.Log($"[QuickEmojiManager] {mode}에서 {fixedCount}개 텍스트의 이모지를 수정했습니다!");
        
        if (fixedCount == 0)
        {
            Debug.Log("[QuickEmojiManager] 수정할 이모지가 없습니다. 모든 텍스트가 이미 안전합니다!");
        }
    }
    
    /// <summary>
    /// 문제가 되는 특수 문자들 수정
    /// </summary>
    private string FixProblematicCharacters(string text)
    {
        // 100% 안전한 ASCII 문자로 매칭
        Dictionary<string, string> problematicChars = new Dictionary<string, string>
        {
            // 별 관련 - 모든 별을 안전한 * 로
            {"\u2726", "*"},   // ✦ → *
            {"\u2605", "*"},   // ★ → *
            {"\u2B50", "*"},   // ⭐ → *
            {"\u1F31F", "*"},  // 🌟 → *
            {"\u2728", "*"},   // ✨ → *
            
            // 왕관/트로피 관련
            {"\u2654", "1st"}, // ♔ → 1st
            {"\u1F451", "1st"}, // 👑 → 1st
            {"\u1F3C6", "1st"}, // 🏆 → 1st
            
            // 박수/응원 관련
            {"\u1F44F", ":)"}, // 👏 → :)
            {"\u1F389", "!"}, // 🎉 → !
            
            // 차트/그래프 관련
            {"\u1F4C8", "^"},  // 📈 → ^
            {"\u1F4CA", "|"},  // 📊 → |
            {"\u25D3", "|"},   // ◓ → |
            {"\u25B2", "^"},   // ▲ → ^
            
            // 화살표 관련
            {"\u27A1\uFE0F", "->"}, // > → ->
            {"\u27A1", "->"},  // > → ->
            {"\u2192", "->"},  // → → ->
            
            // 기타
            {"\u1F3A8", "#"},  // 🎨 → #
            {"\u2665", "<3"},  // ♥ → <3
            {"\u2764", "<3"},  // ❤ → <3
            
            // 특수 블록 문자들 (안전한 ASCII로)
            {"\u2593", "="},   // ▓ → =
            {"\u2592", "-"},   // ▒ → -
            {"\u2591", "."},   // ░ → .
            {"\u25A4", "#"},   // ▤ → #
            {"\u25C9", "O"},   // ◉ → O
            {"\u25C6", "#"},   // ◆ → #
        };
        
        string result = text;
        foreach (var fix in problematicChars)
        {
            if (result.Contains(fix.Key))
            {
                result = result.Replace(fix.Key, fix.Value);
                Debug.Log($"  특수문자 수정: '{fix.Key}' → '{fix.Value}'");
            }
        }
        
        return result;
    }
    
    private Dictionary<string, string> GetEmojiReplacements()
    {
        if (usePrettyMode)
        {
            // 100% 안전한 ASCII 모드 (예쁜 버전)
            return new Dictionary<string, string>
            {
                {"📈", "^"},    // 차트 → 상승
                {"📊", "|||"},  // 막대 → 세로줄
                {"🌟", "*"},    // 별 → 별표
                {"⭐", "*"},    // 별 → 별표
                {"💪", "UP"},   // 근육 → UP
                {"🎯", "O"},    // 타겟 → 원
                {"🔥", "^"},    // 불꽃 → 삼각형
                {"✨", "*"},    // 반짝임 → 별
                {"🎉", "!"},    // 파티 → 느낌표
                {"📋", "[]"},   // 클립보드 → 대괄호
                {"💡", "O"},    // 전구 → 원
                {">", "->"},   // 화살표 → 화살표
                {"🎨", "#"},    // 페인트 → 해시
                {"🚀", "^"},    // 로켓 → 위쪽
                {"🏆", "1st"},  // 트로피 → 1등
                {"❤️", "<3"},   // 하트 → 하트
                {"💯", "100%"}, // 100점
                {"👏", ":)"},   // 박수 → 웃음
                
                // 감정 - 완전 안전한 ASCII
                {"😊", ":)"},
                {"😢", ":("},
                {"😡", ">:("},
                {"😴", "Zzz"},
                {"🤔", "?"}
            };
        }
        else
        {
            // 안전 모드 - 100% 기본 ASCII만 사용
            return new Dictionary<string, string>
            {
                {"📈", "UP"},    // 상승
                {"📊", "||"},    // 막대
                {"🌟", "*"},     // 별
                {"⭐", "*"},     // 별
                {"💪", "Strong"}, // 강력
                {"🎯", "O"},     // 타겟
                {"🔥", "HOT"},   // 뜨거움
                {"✨", "*"},     // 별빛
                {"🎉", "!"},     // 축하
                {"📋", "[]"},    // 리스트
                {"💡", "IDEA"},  // 아이디어
                {">", "->"},    // 화살표
                {"🎨", "ART"},   // 아트
                {"🚀", "GO"},    // 출발
                {"🏆", "WIN"},   // 승리
                {"❤️", "LOVE"},  // 사랑
                {"💯", "100%"},  // 100%
                {"👏", "GOOD"},  // 좋음
                
                // 감정 - ASCII 버전
                {"😊", ":)"},
                {"😢", ":("},
                {"😡", ">:("},
                {"😴", "ZZZ"},
                {"🤔", "?"}
            };
        }
    }
    
    [ContextMenu("예쁜/안전 모드 전환")]
    public void ToggleMode()
    {
        usePrettyMode = !usePrettyMode;
        FixAllEmojisNow();
        
        string newMode = usePrettyMode ? "예쁜 모드" : "안전 모드";
        Debug.Log($"[QuickEmojiManager] {newMode}로 전환되었습니다!");
    }
}
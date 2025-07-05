using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Unity 호환 이모지 텍스트 헬퍼
/// NotoColorEmoji 대신 Unity 호환 이모지 사용
/// </summary>
public static class EmojiTextHelper
{
    // Unity에서 99% 안전한 예쁜 이모지 매핑 (최고 품질)
    private static Dictionary<string, string> prettyEmojiMap = new Dictionary<string, string>
    {
        // 차트/그래프 관련 - 매우 예쁜 ASCII 아트
        {"📈", "📊"},   // Chart를 더 안전한 bar chart로
        {"📊", "▓▒░"},  // 3D 막대 효과
        {"📋", "▤"},    // 클립보드
        
        // 별/성취 관련 - 반짝이는 효과
        {"🌟", "✦"},   // 반짝이는 별
        {"⭐", "★"},   // 일반 별
        {"✨", "◇"},   // 다이아몬드 반짝임
        
        // 감정/상태 관련 - 귀여운 표현
        {"💪", "💪"},  // 근육은 그대로 (안전함)
        {"🔥", "▲"},   // 불꽃을 삼각형으로
        {"🚀", "^"},   // 로켓을 상승 화살표로
        {"🎯", "◉"},   // 타겟
        {"🏆", "♔"},   // 트로피를 왕관으로
        
        // 화살표/방향 관련
        {"➡️", ">"},   // 오른쪽 화살표
        {"⬆️", "^"},   // 위쪽 화살표
        {"⬇️", "v"},   // 아래쪽 화살표
        
        // 하트/사랑 관련
        {"❤️", "♥"},   // 빨간 하트
        {"💖", "♡"},   // 분홍 하트
        {"💝", "♥"},   // 선물 하트
        
        // 기타 유용한 기호들
        {"🎨", "◆"},   // 페인트 팔레트
        {"🎉", "※"},   // 파티
        {"💡", "●"},   // 전구
        {"💯", "100%"}, // 100점
        
        // UI 관련
        {"📱", "▣"},   // 핸드폰
        {"💻", "▢"},   // 컴퓨터
        {"🖥️", "▦"},   // 모니터
        
        // 감정 이모지 - 예쁜 버전
        {"😊", "😊"},   // 행복 - 그대로 (안전함)
        {"😢", "😢"},   // 슬픔 - 그대로 (안전함)
        {"😡", "😡"},   // 화남 - 그대로 (안전함) 
        {"😴", "😴"},   // 졸림 - 그대로 (안전함)
        {"🤔", "🤔"},   // 생각 - 그대로 (안전함)
        {"😍", "😍"},   // 사랑
        {"😭", "😭"},   // 울음
        {"😂", "😂"},   // 웃음
        {"😰", "😰"},   // 걱정
        {"😌", "😌"},   // 평온
        {"🥺", "🥺"},   // 애원
        {"😤", "😤"},   // 분노
        {"🙄", "🙄"},   // 짜증
        {"😎", "😎"},   // 멋짐
        {"🤗", "🤗"},   // 포옹
    };
    
    // 완전히 안전한 ASCII 대체 문자들
    private static Dictionary<string, string> safeReplacementMap = new Dictionary<string, string>
    {
        {"📈", "^"},   // 상승 화살표
        {"🌟", "★"},   // 별
        {"💪", "💪"},  // 그대로 사용 (대부분 지원)
        {"🎯", "◉"},   // 타겟
        {"📊", "▊"},   // 막대 그래프
        {"🔥", "▲"},   // 삼각형
        {"✨", "✦"},   // 별빛
        {"🎉", "※"},   // 특수 문자
        {"📋", "▣"},   // 체크박스
        {"💡", "●"},   // 원
        {"➡️", ">"},   // 화살표
        {"🎨", "◆"},   // 다이아몬드
        
        // 감정 이모지 ASCII 대체
        {"😊", ":)"},  // 웃는 얼굴
        {"😢", ":("},  // 슬픈 얼굴
        {"😡", ">:("},  // 화난 얼굴
        {"😴", "ZZZ"}, // 졸린 얼굴
        {"🤔", "?"},   // 생각하는 얼굴
        
        {"💯", "100%"},
        {"⭐", "★"},
        {"🚀", "↑"},
        {"🏆", "♔"},
        {"❤️", "♥"},
    };
    
    /// <summary>
    /// 텍스트에서 이모지를 Unity 호환 버전으로 변환
    /// </summary>
    public static string ConvertEmojisForUnity(string text, bool useSafeMode = false)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        string result = text;
        Dictionary<string, string> mapToUse = useSafeMode ? safeReplacementMap : prettyEmojiMap;
        
        foreach (var kvp in mapToUse)
        {
            if (result.Contains(kvp.Key))
            {
                result = result.Replace(kvp.Key, kvp.Value);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// TextMeshPro 컴포넌트에 안전한 이모지 텍스트 설정
    /// </summary>
    public static void SetEmojiText(TextMeshProUGUI textComponent, string text, bool useSafeMode = false)
    {
        if (textComponent == null) return;
        
        string convertedText = ConvertEmojisForUnity(text, useSafeMode);
        textComponent.text = convertedText;
    }
    
    /// <summary>
    /// 모든 TextMeshPro 컴포넌트에서 이모지 문제 해결
    /// </summary>
    public static void FixAllEmojiTexts(GameObject rootObject, bool useSafeMode = false)
    {
        TextMeshProUGUI[] allTexts = rootObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        
        foreach (var text in allTexts)
        {
            if (!string.IsNullOrEmpty(text.text))
            {
                text.text = ConvertEmojisForUnity(text.text, useSafeMode);
            }
        }
        
        Debug.Log($"Fixed {allTexts.Length} TextMeshPro components for emoji compatibility");
    }
    
    /// <summary>
    /// 안전한 이모지 문자열 생성
    /// </summary>
    public static class SafeEmojis
    {
        public const string Chart = "^";
        public const string Star = "★";
        public const string Muscle = "💪";
        public const string Target = "◉";
        public const string BarChart = "▊";
        public const string Fire = "▲";
        public const string Sparkle = "✦";
        public const string Party = "※";
        public const string Clipboard = "▣";
        public const string Bulb = "●";
        public const string Arrow = "→";
        public const string Paint = "◆";
        
        public const string Happy = ":)";
        public const string Sad = ":(";
        public const string Angry = ">:(";
        public const string Tired = "ZZZ";
        public const string Thinking = "?";
        
        public const string Perfect = "100%";
        public const string StarAlt = "★";
        public const string Rocket = "↑";
        public const string Trophy = "♔";
        public const string Heart = "♥";
    }
}

/// <summary>
/// ✨ 최고급 자동 이모지 텍스트 수정기 ✨
/// 씬에 추가하면 자동으로 모든 TextMeshPro 이모지 문제 해결
/// 실시간 감지, 디버그 모드, 상세 로그 지원
/// </summary>
public class AutoEmojiTextFixer : MonoBehaviour
{
    [Header("◆ 이모지 수정 설정")]
    [SerializeField] private bool useSafeMode = false; // 기본값을 예쁜 모드로
    [SerializeField] private bool fixOnStart = true;
    [SerializeField] private bool fixOnEnable = true;
    [SerializeField] private bool enableRealtimeCheck = true; // 실시간 체크
    [SerializeField] private float checkInterval = 2f; // 2초마다 체크
    
    [Header("◆ 디버그 설정")]
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private bool showDetailedLogs = true;
    [SerializeField] private bool highlightFixedTexts = false; // 수정된 텍스트 하이라이트
    
    [Header("◆ 범위 설정")]
    [SerializeField] private bool includeInactiveObjects = true;
    [SerializeField] private bool includeChildrenOnly = false; // 자식 오브젝트만 포함
    
    // 실시간 체크용
    private Coroutine realtimeCheckCoroutine;
    private int lastFixedCount = 0;
    
    private void Start()
    {
        if (fixOnStart)
        {
            FixAllEmojis();
        }
        
        if (enableRealtimeCheck)
        {
            StartRealtimeCheck();
        }
    }
    
    private void OnEnable()
    {
        if (fixOnEnable && !fixOnStart)
        {
            FixAllEmojis();
        }
    }
    
    private void OnDisable()
    {
        StopRealtimeCheck();
    }
    
    /// <summary>
    /// 실시간 체크 시작
    /// </summary>
    private void StartRealtimeCheck()
    {
        if (realtimeCheckCoroutine != null)
        {
            StopCoroutine(realtimeCheckCoroutine);
        }
        
        realtimeCheckCoroutine = StartCoroutine(RealtimeCheckCoroutine());
        
        if (enableDebugMode)
        {
            Debug.Log($"[AutoEmojiTextFixer] 실시간 이모지 체크 시작 (간격: {checkInterval}초)");
        }
    }
    
    /// <summary>
    /// 실시간 체크 중지
    /// </summary>
    private void StopRealtimeCheck()
    {
        if (realtimeCheckCoroutine != null)
        {
            StopCoroutine(realtimeCheckCoroutine);
            realtimeCheckCoroutine = null;
        }
    }
    
    /// <summary>
    /// 실시간 체크 코루틴
    /// </summary>
    private System.Collections.IEnumerator RealtimeCheckCoroutine()
    {
        while (enableRealtimeCheck)
        {
            yield return new WaitForSeconds(checkInterval);
            
            // 조용히 체크 (로그 없이)
            bool originalDebugMode = enableDebugMode;
            bool originalDetailedLogs = showDetailedLogs;
            
            enableDebugMode = false;
            showDetailedLogs = false;
            
            int fixedCount = FixAllEmojisInternal();
            
            enableDebugMode = originalDebugMode;
            showDetailedLogs = originalDetailedLogs;
            
            // 새로운 이모지가 발견된 경우에만 로그
            if (fixedCount > 0 && enableDebugMode)
            {
                Debug.Log($"[AutoEmojiTextFixer] 실시간 체크: {fixedCount}개 새 이모지 수정됨");
            }
        }
    }
    
    /// <summary>
    /// 모든 이모지 수정 (메뉴용)
    /// </summary>
    [ContextMenu("★ 모든 이모지 수정하기")]
    public void FixAllEmojis()
    {
        int fixedCount = FixAllEmojisInternal();
        
        if (enableDebugMode)
        {
            Debug.Log($"[AutoEmojiTextFixer] ✨ 수동 수정 완료: {fixedCount}개 텍스트 처리됨");
        }
    }
    
    /// <summary>
    /// 안전 모드 토글
    /// </summary>
    [ContextMenu("◆ 안전/예쁜 모드 토글")]
    public void ToggleSafeMode()
    {
        useSafeMode = !useSafeMode;
        FixAllEmojis();
        
        string mode = useSafeMode ? "안전 모드" : "예쁜 모드";
        Debug.Log($"[AutoEmojiTextFixer] {mode}로 전환되었습니다!");
    }
    
    /// <summary>
    /// 실제 이모지 수정 처리
    /// </summary>
    private int FixAllEmojisInternal()
    {
        GameObject targetObject = includeChildrenOnly ? gameObject : SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault();
        
        if (targetObject == null)
        {
            targetObject = gameObject;
        }
        
        TextMeshProUGUI[] allTexts = targetObject.GetComponentsInChildren<TextMeshProUGUI>(includeInactiveObjects);
        int fixedCount = 0;
        List<string> fixedTexts = new List<string>();
        
        foreach (var textComponent in allTexts)
        {
            if (!string.IsNullOrEmpty(textComponent.text))
            {
                string originalText = textComponent.text;
                string convertedText = EmojiTextHelper.ConvertEmojisForUnity(originalText, useSafeMode);
                
                if (originalText != convertedText)
                {
                    textComponent.text = convertedText;
                    fixedCount++;
                    
                    if (showDetailedLogs)
                    {
                        fixedTexts.Add($"  • {textComponent.gameObject.name}: '{originalText}' → '{convertedText}'");
                    }
                    
                    // 하이라이트 효과
                    if (highlightFixedTexts)
                    {
                        StartCoroutine(HighlightText(textComponent));
                    }
                }
            }
        }
        
        // 상세 로그 출력
        if (enableDebugMode && fixedCount > 0)
        {
            string modeText = useSafeMode ? "안전 모드" : "예쁜 모드";
            Debug.Log($"[AutoEmojiTextFixer] {modeText}에서 {fixedCount}개 텍스트 수정됨!");
            
            if (showDetailedLogs && fixedTexts.Count > 0)
            {
                Debug.Log($"수정된 텍스트 목록:\n{string.Join("\n", fixedTexts)}");
            }
        }
        
        lastFixedCount = fixedCount;
        return fixedCount;
    }
    
    /// <summary>
    /// 텍스트 하이라이트 효과
    /// </summary>
    private System.Collections.IEnumerator HighlightText(TextMeshProUGUI textComponent)
    {
        Color originalColor = textComponent.color;
        Color highlightColor = Color.yellow;
        
        // 노란색으로 변경
        textComponent.color = highlightColor;
        yield return new WaitForSeconds(0.5f);
        
        // 원래 색으로 복원
        textComponent.color = originalColor;
    }
    
    /// <summary>
    /// 설정 리셋
    /// </summary>
    [ContextMenu("◇ 설정 리셋")]
    public void ResetSettings()
    {
        useSafeMode = false;
        fixOnStart = true;
        fixOnEnable = true;
        enableRealtimeCheck = true;
        checkInterval = 2f;
        enableDebugMode = true;
        showDetailedLogs = true;
        highlightFixedTexts = false;
        includeInactiveObjects = true;
        includeChildrenOnly = false;
        
        Debug.Log("[AutoEmojiTextFixer] 설정이 기본값으로 리셋되었습니다!");
    }
}
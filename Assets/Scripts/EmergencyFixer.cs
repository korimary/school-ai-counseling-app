using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 긴급 문제 해결을 위한 단순하고 직접적인 스크립트
/// SystemFixManager가 작동하지 않을 때 사용
/// </summary>
public class EmergencyFixer : MonoBehaviour
{
    [Header("긴급 수정 설정")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool showDebugLogs = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            PerformEmergencyFix();
        }
    }
    
    [ContextMenu("🚨 긴급 수정 실행")]
    public void PerformEmergencyFix()
    {
        LogMessage("=== 🚨 긴급 수정 시작 ===");
        
        // 1. 교사 모드 강제 설정
        FixTeacherMode();
        
        // 2. 누락된 스크립트 수정
        FixMissingScripts();
        
        // 3. 유니코드 문자 수정
        FixUnicodeCharacters();
        
        // 4. 폰트 강제 적용
        ForceApplyKoreanFont();
        
        LogMessage("=== 🚨 긴급 수정 완료 ===");
    }
    
    private void FixTeacherMode()
    {
        LogMessage("교사 모드 강제 설정 중...");
        try
        {
            UserManager.SetTeacherMode(true);
            LogMessage("✅ 교사 모드 설정 완료");
        }
        catch (System.Exception ex)
        {
            LogMessage($"❌ 교사 모드 설정 실패: {ex.Message}");
        }
    }
    
    private void FixMissingScripts()
    {
        LogMessage("누락된 스크립트 수정 중...");
        
        // EmotionBarItem 수정
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        int fixedItems = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("EmotionBarItem"))
            {
                EmotionBarItem component = obj.GetComponent<EmotionBarItem>();
                if (component == null)
                {
                    obj.AddComponent<EmotionBarItem>();
                    fixedItems++;
                    LogMessage($"✅ EmotionBarItem 추가: {obj.name}");
                }
            }
            else if (obj.name.Contains("StudentDashboardItem"))
            {
                StudentDashboardItem component = obj.GetComponent<StudentDashboardItem>();
                if (component == null)
                {
                    obj.AddComponent<StudentDashboardItem>();
                    fixedItems++;
                    LogMessage($"✅ StudentDashboardItem 추가: {obj.name}");
                }
            }
        }
        
        LogMessage($"✅ {fixedItems}개의 누락된 스크립트 수정 완료");
    }
    
    private void FixUnicodeCharacters()
    {
        LogMessage("유니코드 문자 수정 중...");
        
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        int fixedCount = 0;
        
        foreach (var text in allTexts)
        {
            if (text == null || string.IsNullOrEmpty(text.text)) continue;
            
            string originalText = text.text;
            string newText = originalText;
            
            // 문제가 있는 유니코드 문자들 교체 (기본 문자로 변환)
            newText = newText.Replace("←", "<");     // 왼쪽 화살표
            newText = newText.Replace("→", ">");     // 오른쪽 화살표
            newText = newText.Replace("↗", "^");     // 우상향 화살표
            newText = newText.Replace("↘", "v");     // 우하향 화살표
            newText = newText.Replace("↑", "^");     // 위쪽 화살표
            newText = newText.Replace("↓", "v");     // 아래쪽 화살표
            newText = newText.Replace("➡", ">");     // 오른쪽 화살표 (굵은)
            newText = newText.Replace("⬅", "<");     // 왼쪽 화살표 (굵은)
            newText = newText.Replace("⬆", "^");     // 위쪽 화살표 (굵은)
            newText = newText.Replace("⬇", "v");     // 아래쪽 화살표 (굵은)
            newText = newText.Replace("️", "");      // \uFE0F (Variation Selector)
            newText = newText.Replace("⚡", "!");     // 번개 이모지
            newText = newText.Replace("📊", "[차트]"); // 차트 이모지
            newText = newText.Replace("📈", "^");     // 상승 차트
            newText = newText.Replace("📉", "v");     // 하락 차트
            
            if (newText != originalText)
            {
                text.text = newText;
                fixedCount++;
                LogMessage($"유니코드 수정: {text.gameObject.name}");
            }
        }
        
        LogMessage($"✅ {fixedCount}개의 유니코드 문자 수정 완료");
    }
    
    private void ForceApplyKoreanFont()
    {
        LogMessage("한국어 폰트 강제 적용 중...");
        
        // 사용 가능한 폰트 찾기
        TMP_FontAsset koreanFont = null;
        
        // 다양한 경로로 폰트 검색
        string[] fontPaths = {
            "NotoSansKR-Bold SDF",
            "NotoSansKR-Regular SDF", 
            "Noto Sans KR SDF",
            "NotoSans-Korean SDF"
        };
        
        foreach (string fontPath in fontPaths)
        {
            koreanFont = Resources.Load<TMP_FontAsset>(fontPath);
            if (koreanFont != null)
            {
                LogMessage($"폰트 발견: {fontPath}");
                break;
            }
        }
        
        // Assets에서 직접 검색
        if (koreanFont == null)
        {
            TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var font in allFonts)
            {
                if (font.name.ToLower().Contains("noto") && 
                    (font.name.ToLower().Contains("kr") || font.name.ToLower().Contains("korean")))
                {
                    koreanFont = font;
                    LogMessage($"Assets에서 폰트 발견: {font.name}");
                    break;
                }
            }
        }
        
        if (koreanFont != null)
        {
            // 모든 텍스트에 폰트 적용
            TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
            int changedCount = 0;
            
            foreach (var text in allTexts)
            {
                if (text.font != koreanFont)
                {
                    text.font = koreanFont;
                    changedCount++;
                }
            }
            
            LogMessage($"✅ {changedCount}개의 텍스트에 {koreanFont.name} 폰트 적용 완료");
        }
        else
        {
            LogMessage("❌ 한국어 폰트를 찾을 수 없습니다");
        }
    }
    
    [ContextMenu("현재 상태 진단")]
    public void DiagnoseCurrentState()
    {
        LogMessage("=== 현재 상태 진단 ===");
        
        // 교사 모드 확인
        bool isTeacher = UserManager.IsTeacherMode();
        LogMessage($"교사 모드: {(isTeacher ? "✅ 활성" : "❌ 비활성")}");
        
        // 누락된 스크립트 확인
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        int missingEmotionBars = 0;
        int missingStudentItems = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("EmotionBarItem") && obj.GetComponent<EmotionBarItem>() == null)
                missingEmotionBars++;
            if (obj.name.Contains("StudentDashboardItem") && obj.GetComponent<StudentDashboardItem>() == null)
                missingStudentItems++;
        }
        
        LogMessage($"누락된 EmotionBarItem: {missingEmotionBars}개");
        LogMessage($"누락된 StudentDashboardItem: {missingStudentItems}개");
        
        // 폰트 상태 확인
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        var fontUsage = new System.Collections.Generic.Dictionary<string, int>();
        
        foreach (var text in allTexts)
        {
            string fontName = text.font?.name ?? "없음";
            if (fontUsage.ContainsKey(fontName))
                fontUsage[fontName]++;
            else
                fontUsage[fontName] = 1;
        }
        
        LogMessage("=== 폰트 사용 현황 ===");
        foreach (var kvp in fontUsage)
        {
            LogMessage($"{kvp.Key}: {kvp.Value}개");
        }
        
        LogMessage("=== 진단 완료 ===");
    }
    
    private void LogMessage(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[EmergencyFixer] {message}");
        }
    }
}
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 시스템 전반의 문제들을 자동으로 해결하는 전역 매니저 (Singleton)
/// 모든 씬에서 자동으로 폰트, 스크립트, 유니코드 문제를 해결합니다.
/// </summary>
public class SystemFixManager : MonoBehaviour
{
    private static SystemFixManager _instance;
    public static SystemFixManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SystemFixManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SystemFixManager");
                    _instance = go.AddComponent<SystemFixManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("자동 수정 설정")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool autoFixOnSceneLoad = true;
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("폰트 설정")]
    [SerializeField] private TMP_FontAsset fallbackFont;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void Initialize()
    {
        LogDebug("=== SystemFixManager 전역 초기화 ===");
        
        if (autoFixOnStart)
        {
            // 즉시 실행과 코루틴 실행 모두 시도
            ForceFixAllProblemsNow();
            StartCoroutine(FixAllIssues());
        }
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (autoFixOnSceneLoad)
        {
            LogDebug($"=== 씬 로드 감지: {scene.name} ===");
            StartCoroutine(DelayedSceneFix());
        }
    }
    
    private System.Collections.IEnumerator DelayedSceneFix()
    {
        // 씬 로드 완료 대기
        yield return new WaitForEndOfFrame();
        
        // 즉시 모든 문제 해결
        LogDebug("=== 씬별 자동 수정 시작 ===");
        FixTeacherMode();
        FixFontIssues();
        FixMissingScripts();
        FixUnicodeCharacters();
        LogDebug("=== 씬별 자동 수정 완료 ===");
    }
    
    private System.Collections.IEnumerator FixAllIssues()
    {
        LogDebug("=== 시스템 수정 시작 ===");
        
        // 1. 교사 모드 설정
        FixTeacherMode();
        yield return new WaitForEndOfFrame();
        
        // 2. 폰트 문제 해결
        FixFontIssues();
        yield return new WaitForEndOfFrame();
        
        // 3. 누락된 스크립트 수정
        FixMissingScripts();
        yield return new WaitForEndOfFrame();
        
        // 4. 유니코드 문자 수정
        FixUnicodeCharacters();
        yield return new WaitForEndOfFrame();
        
        LogDebug("=== 시스템 수정 완료 ===");
    }
    
    /// <summary>
    /// 교사 모드 자동 설정
    /// </summary>
    [ContextMenu("1. 교사 모드 설정")]
    public void FixTeacherMode()
    {
        LogDebug("교사 모드 설정 중...");
        
        // UserManager를 통해 교사 모드 설정
        UserManager.SetTeacherMode(true);
        
        // 테스트용 클래스 코드 설정
        if (string.IsNullOrEmpty(ClassCodeManager.GetCurrentClassCode()))
        {
            var testSchoolData = new SchoolData();
            testSchoolData.teacherInfo = new TeacherInfo
            {
                teacherName = "테스트 선생님",
                className = "3학년 1반",
                schoolName = "테스트 초등학교",
                classCode = "TEST-1234"
            };
            
            // 테스트 학생들 추가
            testSchoolData.students = new System.Collections.Generic.List<StudentInfo>
            {
                new StudentInfo(1, "테스트학생1"),
                new StudentInfo(2, "테스트학생2")
            };
            
            // SchoolData는 원본 구조 유지 (감정 데이터는 별도 관리)
            StudentDataManager.SaveSchoolData(testSchoolData);
            ClassCodeManager.SetCurrentClassCode("TEST-1234", 
                ClassCodeManager.CreateClassCodeDataFromSchoolData(testSchoolData, "TEST-1234"));
                
            // EmotionManager를 통해 별도로 테스트 감정 데이터 생성 (기존 구조 유지)
            CreateTestEmotionData();
        }
        
        LogDebug("✅ 교사 모드 설정 완료");
    }
    
    /// <summary>
    /// 폰트 문제 자동 해결
    /// </summary>
    [ContextMenu("2. 폰트 문제 해결")]
    public void FixFontIssues()
    {
        LogDebug("폰트 문제 해결 중...");
        
        // 사용 가능한 한국어 폰트 찾기
        TMP_FontAsset koreanFont = FindBestKoreanFont();
        
        if (koreanFont != null)
        {
            FontManager.DefaultKoreanFont = koreanFont;
            LogDebug($"✅ 한국어 폰트 설정: {koreanFont.name}");
            
            // 모든 텍스트에 적용
            ApplyFontToAllTexts(koreanFont);
        }
        else
        {
            LogDebug("⚠️ 한국어 폰트를 찾을 수 없습니다. 기본 폰트 사용");
        }
    }
    
    /// <summary>
    /// 누락된 스크립트 수정
    /// </summary>
    [ContextMenu("3. 누락된 스크립트 수정")]
    public void FixMissingScripts()
    {
        LogDebug("누락된 스크립트 수정 중...");
        
        // EmotionBarItem 수정
        FixEmotionBarItems();
        
        // StudentDashboardItem 수정
        FixStudentDashboardItems();
        
        LogDebug("✅ 누락된 스크립트 수정 완료");
    }
    
    /// <summary>
    /// 유니코드 문자 문제 해결
    /// </summary>
    [ContextMenu("4. 유니코드 문자 수정")]
    public void FixUnicodeCharacters()
    {
        LogDebug("유니코드 문자 수정 중...");
        
        // 모든 텍스트에서 문제가 있는 유니코드 문자 교체
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
            newText = newText.Replace("📊", "[차트]"); // 차트 이모지
            newText = newText.Replace("📈", "^");     // 상승 차트
            newText = newText.Replace("📉", "v");     // 하락 차트
            
            if (newText != originalText)
            {
                text.text = newText;
                fixedCount++;
                LogDebug($"유니코드 문자 수정: {text.gameObject.name}");
            }
        }
        
        LogDebug($"✅ {fixedCount}개의 유니코드 문자 수정 완료");
    }
    
    /// <summary>
    /// 가장 적합한 한국어 폰트 찾기
    /// </summary>
    private TMP_FontAsset FindBestKoreanFont()
    {
        // 우선순위에 따라 폰트 검색
        string[] fontNames = {
            "NotoSansKR-Regular SDF",
            "NotoSansKR-Bold SDF", 
            "Noto Sans KR SDF",
            "NotoSans-Korean SDF",
            "Malgun Gothic SDF",
            "Arial Unicode MS SDF"
        };
        
        foreach (string fontName in fontNames)
        {
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>(fontName);
            if (font != null)
            {
                LogDebug($"폰트 발견: {fontName}");
                return font;
            }
        }
        
        // Assets 폴더에서 직접 검색
        TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var font in allFonts)
        {
            if (font.name.ToLower().Contains("noto") || 
                font.name.ToLower().Contains("korean") ||
                font.name.ToLower().Contains("kr"))
            {
                LogDebug($"Assets에서 폰트 발견: {font.name}");
                return font;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 모든 텍스트에 폰트 적용
    /// </summary>
    private void ApplyFontToAllTexts(TMP_FontAsset font)
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        int changedCount = 0;
        
        foreach (var text in allTexts)
        {
            if (text.font != font)
            {
                text.font = font;
                changedCount++;
            }
        }
        
        LogDebug($"✅ {changedCount}개의 텍스트에 폰트 적용 완료");
    }
    
    /// <summary>
    /// EmotionBarItem 오브젝트들 수정
    /// </summary>
    private void FixEmotionBarItems()
    {
        // 모든 GameObject 검색 (활성/비활성 포함)
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("EmotionBarItem"))
            {
                // EmotionBarItem 컴포넌트가 없으면 추가
                EmotionBarItem component = obj.GetComponent<EmotionBarItem>();
                if (component == null)
                {
                    component = obj.AddComponent<EmotionBarItem>();
                    
                    // 필드 자동 연결
                    ConnectEmotionBarItemFields(component);
                    
                    LogDebug($"✅ EmotionBarItem 컴포넌트 추가: {obj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// StudentDashboardItem 오브젝트들 수정
    /// </summary>
    private void FixStudentDashboardItems()
    {
        // 모든 GameObject 검색 (활성/비활성 포함)
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("StudentDashboardItem"))
            {
                // StudentDashboardItem 컴포넌트가 없으면 추가
                StudentDashboardItem component = obj.GetComponent<StudentDashboardItem>();
                if (component == null)
                {
                    component = obj.AddComponent<StudentDashboardItem>();
                    
                    // 필드 자동 연결
                    ConnectStudentDashboardItemFields(component);
                    
                    LogDebug($"✅ StudentDashboardItem 컴포넌트 추가: {obj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// EmotionBarItem 필드 자동 연결
    /// </summary>
    private void ConnectEmotionBarItemFields(EmotionBarItem component)
    {
        Transform transform = component.transform;
        
        // 자식 오브젝트들에서 필요한 컴포넌트 찾기
        var emotionNameText = transform.Find("EmotionNameText")?.GetComponent<TextMeshProUGUI>();
        var countText = transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
        var improvementText = transform.Find("ImprovementText")?.GetComponent<TextMeshProUGUI>();
        var barFillImage = transform.Find("BarContainer/FillBar")?.GetComponent<Image>();
        var backgroundImage = transform.GetComponent<Image>();
        
        // 리플렉션으로 필드 설정
        if (emotionNameText != null)
            SetPrivateField(component, "emotionNameText", emotionNameText);
        if (countText != null)
            SetPrivateField(component, "countText", countText);
        if (improvementText != null)
            SetPrivateField(component, "improvementText", improvementText);
        if (barFillImage != null)
            SetPrivateField(component, "barFillImage", barFillImage);
        if (backgroundImage != null)
            SetPrivateField(component, "backgroundImage", backgroundImage);
    }
    
    /// <summary>
    /// StudentDashboardItem 필드 자동 연결
    /// </summary>
    private void ConnectStudentDashboardItemFields(StudentDashboardItem component)
    {
        Transform transform = component.transform;
        
        // 자식 오브젝트들에서 필요한 컴포넌트 찾기
        var studentNumberText = transform.Find("StudentNumberText")?.GetComponent<TextMeshProUGUI>();
        var sessionCountText = transform.Find("SessionContainer/SessionCountText")?.GetComponent<TextMeshProUGUI>();
        var improvementText = transform.Find("ImprovementContainer/ImprovementText")?.GetComponent<TextMeshProUGUI>();
        var recentEmotionText = transform.Find("EmotionContainer/RecentEmotionText")?.GetComponent<TextMeshProUGUI>();
        var detailButton = transform.Find("DetailButton")?.GetComponent<Button>();
        var statusIndicator = transform.Find("StatusIndicator")?.GetComponent<Image>();
        
        // 리플렉션으로 필드 설정
        if (studentNumberText != null)
            SetPrivateField(component, "studentNumberText", studentNumberText);
        if (sessionCountText != null)
            SetPrivateField(component, "sessionCountText", sessionCountText);
        if (improvementText != null)
            SetPrivateField(component, "improvementText", improvementText);
        if (recentEmotionText != null)
            SetPrivateField(component, "recentEmotionText", recentEmotionText);
        if (detailButton != null)
            SetPrivateField(component, "detailButton", detailButton);
        if (statusIndicator != null)
            SetPrivateField(component, "statusIndicator", statusIndicator);
    }
    
    /// <summary>
    /// 리플렉션을 통한 private 필드 설정
    /// </summary>
    private void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null) return;
        
        try
        {
            var field = target.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);
                
            if (field != null && value != null)
            {
                field.SetValue(target, value);
                LogDebug($"  - {fieldName} 연결됨");
            }
            else if (field == null)
            {
                LogDebug($"  - {fieldName} 필드를 찾을 수 없음");
            }
        }
        catch (System.Exception ex)
        {
            LogDebug($"  - {fieldName} 연결 실패: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 모든 문제 한 번에 해결
    /// </summary>
    [ContextMenu("모든 문제 해결")]
    public void FixAllProblems()
    {
        StartCoroutine(FixAllIssues());
    }
    
    /// <summary>
    /// 즉시 모든 문제 해결 (코루틴 없이)
    /// </summary>
    [ContextMenu("🚨 긴급 수정 (즉시 실행)")]
    public void ForceFixAllProblemsNow()
    {
        LogDebug("=== 🚨 긴급 즉시 수정 시작 ===");
        
        // 교사 모드 강제 설정
        try
        {
            UserManager.SetTeacherMode(true);
            LogDebug("✅ 교사 모드 강제 설정 완료");
        }
        catch (System.Exception ex)
        {
            LogDebug($"❌ 교사 모드 설정 실패: {ex.Message}");
        }
        
        // 폰트 즉시 수정
        try
        {
            FixFontIssuesImmediate();
            LogDebug("✅ 폰트 즉시 수정 완료");
        }
        catch (System.Exception ex)
        {
            LogDebug($"❌ 폰트 수정 실패: {ex.Message}");
        }
        
        // 스크립트 즉시 수정
        try
        {
            FixMissingScriptsImmediate();
            LogDebug("✅ 스크립트 즉시 수정 완료");
        }
        catch (System.Exception ex)
        {
            LogDebug($"❌ 스크립트 수정 실패: {ex.Message}");
        }
        
        // 유니코드 즉시 수정
        try
        {
            FixUnicodeCharactersImmediate();
            LogDebug("✅ 유니코드 즉시 수정 완료");
        }
        catch (System.Exception ex)
        {
            LogDebug($"❌ 유니코드 수정 실패: {ex.Message}");
        }
        
        LogDebug("=== 🚨 긴급 즉시 수정 완료 ===");
    }
    
    /// <summary>
    /// 전역 시스템 매니저 수동 초기화 (프로그래매틱 접근용)
    /// </summary>
    public static void InitializeGlobally()
    {
        if (Instance != null)
        {
            Instance.LogDebug("SystemFixManager 전역 초기화 완료");
        }
    }
    
    /// <summary>
    /// 런타임 시작 시 자동 초기화
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RuntimeInitialize()
    {
        InitializeGlobally();
        Debug.Log("[SystemFixManager] 런타임 자동 초기화 완료");
    }
    
    /// <summary>
    /// 현재 시스템 상태 진단
    /// </summary>
    [ContextMenu("시스템 진단")]
    public void DiagnoseSystem()
    {
        LogDebug("=== 시스템 진단 시작 ===");
        
        // 교사 모드 확인
        bool isTeacherMode = UserManager.IsTeacherMode();
        LogDebug($"교사 모드: {(isTeacherMode ? "✅ 설정됨" : "❌ 설정 안됨")}");
        
        // 클래스 코드 확인
        string classCode = ClassCodeManager.GetCurrentClassCode();
        LogDebug($"클래스 코드: {(string.IsNullOrEmpty(classCode) ? "❌ 없음" : $"✅ {classCode}")}");
        
        // 폰트 확인
        var koreanFont = FontManager.DefaultKoreanFont;
        LogDebug($"한국어 폰트: {(koreanFont != null ? $"✅ {koreanFont.name}" : "❌ 없음")}");
        
        // 누락된 스크립트 확인
        EmotionBarItem[] emotionBars = FindObjectsOfType<EmotionBarItem>(true);
        StudentDashboardItem[] studentItems = FindObjectsOfType<StudentDashboardItem>(true);
        
        LogDebug($"EmotionBarItem: {emotionBars.Length}개 발견");
        LogDebug($"StudentDashboardItem: {studentItems.Length}개 발견");
        
        LogDebug("=== 시스템 진단 완료 ===");
    }
    
    /// <summary>
    /// 폰트 문제 즉시 해결 (코루틴 없이)
    /// </summary>
    private void FixFontIssuesImmediate()
    {
        LogDebug("폰트 문제 즉시 해결 중...");
        
        // 사용 가능한 한국어 폰트 찾기
        TMP_FontAsset koreanFont = FindBestKoreanFont();
        
        if (koreanFont != null)
        {
            FontManager.DefaultKoreanFont = koreanFont;
            LogDebug($"✅ 한국어 폰트 설정: {koreanFont.name}");
            
            // 모든 텍스트에 적용
            ApplyFontToAllTexts(koreanFont);
        }
        else
        {
            LogDebug("⚠️ 한국어 폰트를 찾을 수 없습니다. 기본 폰트 사용");
        }
    }
    
    /// <summary>
    /// 누락된 스크립트 즉시 수정 (코루틴 없이)
    /// </summary>
    private void FixMissingScriptsImmediate()
    {
        LogDebug("누락된 스크립트 즉시 수정 중...");
        
        // EmotionBarItem 수정
        FixEmotionBarItems();
        
        // StudentDashboardItem 수정
        FixStudentDashboardItems();
        
        LogDebug("✅ 누락된 스크립트 즉시 수정 완료");
    }
    
    /// <summary>
    /// 유니코드 문자 즉시 수정 (코루틴 없이)
    /// </summary>
    private void FixUnicodeCharactersImmediate()
    {
        LogDebug("유니코드 문자 즉시 수정 중...");
        
        // 모든 텍스트에서 문제가 있는 유니코드 문자 교체
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
            newText = newText.Replace("📊", "[차트]"); // 차트 이모지
            newText = newText.Replace("📈", "^");     // 상승 차트
            newText = newText.Replace("📉", "v");     // 하락 차트
            
            if (newText != originalText)
            {
                text.text = newText;
                fixedCount++;
                LogDebug($"유니코드 문자 수정: {text.gameObject.name}");
            }
        }
        
        LogDebug($"✅ {fixedCount}개의 유니코드 문자 즉시 수정 완료");
    }
    
    /// <summary>
    /// 테스트용 감정 데이터 생성 (기존 구조를 건드리지 않는 안전한 방법)
    /// </summary>
    private void CreateTestEmotionData()
    {
        try
        {
            // EmotionManager가 있는지 안전하게 확인
            if (EmotionManager.Instance != null)
            {
                LogDebug("EmotionManager를 통한 테스트 데이터 생성은 추후 구현 예정");
                // 현재는 EmotionManager 구조를 완전히 파악하지 못했으므로 
                // 안전하게 건너뛰고 추후 필요시 구현
            }
        }
        catch (System.Exception ex)
        {
            LogDebug($"테스트 감정 데이터 생성 건너뜀: {ex.Message}");
        }
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SystemFix] {message}");
        }
    }
}
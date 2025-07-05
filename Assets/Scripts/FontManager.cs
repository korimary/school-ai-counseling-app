using UnityEngine;
using TMPro;

/// <summary>
/// 프로젝트 전체에서 일관된 폰트 사용을 위한 매니저
/// </summary>
public static class FontManager
{
    private static TMP_FontAsset _defaultKoreanFont;
    private static TMP_FontAsset _defaultEnglishFont;
    
    /// <summary>
    /// 기본 한국어 폰트 (Noto Sans KR)
    /// </summary>
    public static TMP_FontAsset DefaultKoreanFont
    {
        get
        {
            if (_defaultKoreanFont == null)
            {
                // 다양한 Noto Sans KR 폰트 이름들로 시도
                string[] fontPaths = {
                    "Noto Sans KR SDF",
                    "NotoSansKR-Regular SDF", 
                    "NotoSansKR-Bold SDF",
                    "NotoSans-Korean SDF",
                    "Noto Sans KR-Regular SDF",
                    "Fonts/Noto Sans KR SDF",
                    "Fonts/NotoSansKR-Regular SDF"
                };
                
                foreach (string fontPath in fontPaths)
                {
                    _defaultKoreanFont = Resources.Load<TMP_FontAsset>(fontPath);
                    if (_defaultKoreanFont != null)
                    {
                        Debug.Log($"한국어 폰트 로드 성공: {fontPath}");
                        break;
                    }
                }
                
                // 그래도 없으면 FindObjectsOfTypeAll로 직접 검색
                if (_defaultKoreanFont == null)
                {
                    TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                    foreach (var font in allFonts)
                    {
                        if (font.name.ToLower().Contains("noto") && 
                            (font.name.ToLower().Contains("kr") || font.name.ToLower().Contains("korean")))
                        {
                            _defaultKoreanFont = font;
                            Debug.Log($"Assets에서 한국어 폰트 발견: {font.name}");
                            break;
                        }
                    }
                }
                
                // 최후의 수단으로 TMP 기본 폰트 사용
                if (_defaultKoreanFont == null)
                {
                    _defaultKoreanFont = TMP_Settings.defaultFontAsset;
                    Debug.LogWarning("Noto Sans KR 폰트를 찾을 수 없어 기본 폰트를 사용합니다.");
                }
            }
            return _defaultKoreanFont;
        }
        set
        {
            _defaultKoreanFont = value;
        }
    }
    
    /// <summary>
    /// 기본 영어 폰트
    /// </summary>
    public static TMP_FontAsset DefaultEnglishFont
    {
        get
        {
            if (_defaultEnglishFont == null)
            {
                _defaultEnglishFont = DefaultKoreanFont; // 한국어 폰트를 영어에도 사용
            }
            return _defaultEnglishFont;
        }
        set
        {
            _defaultEnglishFont = value;
        }
    }
    
    /// <summary>
    /// 텍스트 컴포넌트에 기본 한국어 폰트 적용
    /// </summary>
    public static void ApplyDefaultKoreanFont(TextMeshProUGUI textComponent)
    {
        if (textComponent != null && DefaultKoreanFont != null)
        {
            textComponent.font = DefaultKoreanFont;
        }
    }
    
    /// <summary>
    /// 텍스트 컴포넌트에 기본 한국어 폰트 적용 (3D 버전)
    /// </summary>
    public static void ApplyDefaultKoreanFont(TextMeshPro textComponent)
    {
        if (textComponent != null && DefaultKoreanFont != null)
        {
            textComponent.font = DefaultKoreanFont;
        }
    }
    
    /// <summary>
    /// 특정 폰트를 프로젝트 기본 폰트로 설정
    /// </summary>
    public static void SetProjectDefaultFont(TMP_FontAsset font)
    {
        if (font != null)
        {
            DefaultKoreanFont = font;
            DefaultEnglishFont = font;
            
#if UNITY_EDITOR
            // Editor에서 TMP Settings도 업데이트
            var tmpSettings = TMP_Settings.instance;
            if (tmpSettings != null)
            {
                UnityEditor.SerializedObject serializedSettings = new UnityEditor.SerializedObject(tmpSettings);
                UnityEditor.SerializedProperty defaultFontAssetProperty = serializedSettings.FindProperty("m_defaultFontAsset");
                
                if (defaultFontAssetProperty != null)
                {
                    defaultFontAssetProperty.objectReferenceValue = font;
                    serializedSettings.ApplyModifiedProperties();
                    UnityEditor.EditorUtility.SetDirty(tmpSettings);
                }
            }
#endif
            
            Debug.Log($"프로젝트 기본 폰트가 {font.name}으로 설정되었습니다.");
        }
    }
    
    /// <summary>
    /// 사용 가능한 한국어 폰트 목록 가져오기
    /// </summary>
    public static TMP_FontAsset[] GetAvailableKoreanFonts()
    {
        TMP_FontAsset[] allFonts = Resources.LoadAll<TMP_FontAsset>("");
        System.Collections.Generic.List<TMP_FontAsset> koreanFonts = new System.Collections.Generic.List<TMP_FontAsset>();
        
        foreach (var font in allFonts)
        {
            if (font.name.ToLower().Contains("noto") || 
                font.name.ToLower().Contains("korean") || 
                font.name.ToLower().Contains("kr"))
            {
                koreanFonts.Add(font);
            }
        }
        
        return koreanFonts.ToArray();
    }
    
    /// <summary>
    /// 폰트 초기화 (게임 시작 시 호출)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // 기본 폰트 로드
        var koreanFont = DefaultKoreanFont;
        if (koreanFont != null)
        {
            Debug.Log($"기본 한국어 폰트 로드됨: {koreanFont.name}");
        }
    }
}
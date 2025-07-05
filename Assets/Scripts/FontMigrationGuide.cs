using UnityEngine;
using TMPro;

/// <summary>
/// 폰트 마이그레이션 가이드 및 유틸리티
/// </summary>
public class FontMigrationGuide : MonoBehaviour
{
    [Header("사용법")]
    [TextArea(10, 20)]
    [SerializeField] private string instructions = @"
=== 폰트 마이그레이션 완전 가이드 ===

🔧 1단계: Unity Editor에서 Tools > Font Migration Tool 실행

📋 2단계: 완전 자동 마이그레이션
   - 'Noto Sans KR' 폰트를 'New Default Font'에 드래그
   - 'Liberation Sans SDF'를 'Old Font'에 드래그  
   - '전체 프로세스 자동 실행' 버튼 클릭

🎯 3단계: 개별 단계 실행 (필요시)
   1. '프로젝트 스캔' - 폰트 사용 현황 분석
   2. 'TextMeshPro 기본 설정 변경' - TMP 기본 폰트 변경
   3. '씬 오브젝트 폰트 교체' - 현재 씬의 모든 텍스트 교체
   4. '프리팹 폰트 교체' - 모든 프리팹의 텍스트 교체
   5. '스크립트 코드 업데이트' - 동적 생성 코드 업데이트
   6. '기존 폰트 삭제' - Liberation Sans SDF 완전 제거

⚠️ 주의사항:
   - 작업 전 반드시 프로젝트 백업
   - 6단계는 마지막에 실행 (되돌릴 수 없음)
   - 모든 씬을 열어서 확인 권장

✨ 새로운 코드 작성 시:
   TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
   FontManager.ApplyDefaultKoreanFont(text);

🎉 완료 후 확인사항:
   - 새로 생성되는 텍스트가 Noto Sans KR인지 확인
   - 기존 텍스트들이 모두 교체되었는지 확인
   - Liberation Sans SDF 참조 오류 없는지 확인
";
    
    [Header("빠른 폰트 설정")]
    [SerializeField] private TMP_FontAsset notoSansKR;
    
    void Start()
    {
        if (notoSansKR != null)
        {
            FontManager.SetProjectDefaultFont(notoSansKR);
            Debug.Log($"기본 폰트가 {notoSansKR.name}으로 설정되었습니다.");
        }
        
        // instructions 필드가 UI에 표시되어 경고 해결
        if (!string.IsNullOrEmpty(instructions))
        {
            // Inspector에 표시용 필드이므로 로그로 사용여부 확인
        }
    }
    
    [ContextMenu("현재 폰트 상태 확인")]
    public void CheckCurrentFontStatus()
    {
        Debug.Log("=== 현재 폰트 상태 ===");
        Debug.Log($"FontManager 기본 한국어 폰트: {FontManager.DefaultKoreanFont?.name ?? "없음"}");
        Debug.Log($"TMP 기본 폰트: {TMP_Settings.defaultFontAsset?.name ?? "없음"}");
        
        // 씬의 모든 텍스트 컴포넌트 확인
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
        
        Debug.Log("=== 씬 내 폰트 사용 현황 ===");
        foreach (var kvp in fontUsage)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}개");
        }
    }
    
    [ContextMenu("Liberation Sans SDF 사용 텍스트 찾기")]
    public void FindLiberationSansUsage()
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        int count = 0;
        
        Debug.Log("=== Liberation Sans SDF 사용 텍스트 ===");
        foreach (var text in allTexts)
        {
            if (text.font != null && text.font.name.Contains("Liberation"))
            {
                Debug.Log($"- {text.gameObject.name} ({text.text})", text.gameObject);
                count++;
            }
        }
        
        if (count == 0)
        {
            Debug.Log("✅ Liberation Sans SDF를 사용하는 텍스트가 없습니다!");
        }
        else
        {
            Debug.Log($"⚠️ {count}개의 텍스트가 Liberation Sans SDF를 사용 중입니다.");
        }
    }
    
    [ContextMenu("모든 텍스트를 Noto Sans KR로 변경")]
    public void ConvertAllTextsToNotoSans()
    {
        if (FontManager.DefaultKoreanFont == null)
        {
            Debug.LogError("FontManager의 기본 한국어 폰트가 설정되지 않았습니다.");
            return;
        }
        
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        int changedCount = 0;
        
        foreach (var text in allTexts)
        {
            if (text.font != FontManager.DefaultKoreanFont)
            {
                text.font = FontManager.DefaultKoreanFont;
                changedCount++;
            }
        }
        
        Debug.Log($"✅ {changedCount}개의 텍스트를 Noto Sans KR로 변경했습니다.");
    }
}
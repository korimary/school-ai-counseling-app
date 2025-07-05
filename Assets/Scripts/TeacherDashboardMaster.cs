using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// TeacherDashboard의 전체 UI를 동적으로 생성하고 관리하는 마스터 클래스
/// Unity Play 모드에서 실행하면 완전한 UI가 생성되며, 
/// 이를 복사하여 Scene에 붙여넣기로 사용할 수 있습니다.
/// </summary>
public class TeacherDashboardMaster : MonoBehaviour
{
    [Header("생성 설정")]
    [SerializeField] private bool autoCreateOnStart = true;
    [SerializeField] private bool createCanvas = true;
    
    [Header("UI 컴포넌트들")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private TeacherDashboardUIGenerator uiGenerator;
    [SerializeField] private UIPrefabGenerator prefabGenerator;
    [SerializeField] private PopupManager popupManager;
    
    [Header("생성 순서 제어")]
    [SerializeField] private float delayBetweenSteps = 0.1f;
    
    void Start()
    {
        // UI가 이미 완성되었으므로 동적 생성을 건너뜁니다
        Debug.Log("[TeacherDashboardMaster] UI가 이미 완성되어 동적 생성을 건너뜁니다.");
        
        // 🚨 긴급 수정 실행 - EmergencyFixer 사용
        try
        {
            // EmergencyFixer 컴포넌트 추가 및 실행
            EmergencyFixer emergencyFixer = GetComponent<EmergencyFixer>();
            if (emergencyFixer == null)
            {
                emergencyFixer = gameObject.AddComponent<EmergencyFixer>();
            }
            
            Debug.Log("[TeacherDashboardMaster] EmergencyFixer 실행 중...");
            emergencyFixer.PerformEmergencyFix();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TeacherDashboardMaster] EmergencyFixer 실행 실패: {ex.Message}");
        }
        
        // SystemFixManager 백업 시도
        try
        {
            SystemFixManager fixManager = SystemFixManager.Instance;
            if (fixManager != null)
            {
                Debug.Log("[TeacherDashboardMaster] SystemFixManager 백업 실행 중...");
                fixManager.ForceFixAllProblemsNow(); // 즉시 실행
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TeacherDashboardMaster] SystemFixManager 백업 실행 실패: {ex.Message}");
        }
        
        // 더 이상 동적 UI 생성이 필요하지 않습니다
        if (autoCreateOnStart)
        {
            Debug.Log("[TeacherDashboardMaster] autoCreateOnStart가 활성화되어 있지만 UI가 이미 완성되었습니다.");
            // StartCoroutine(CreateCompleteUI()); // 비활성화됨
        }
    }
    
    /// <summary>
    /// 완전한 Teacher Dashboard UI를 단계별로 생성
    /// </summary>
    public IEnumerator CreateCompleteUI()
    {
        Debug.Log("=== Teacher Dashboard UI 생성 시작 ===");
        
        // 1단계: Canvas 생성
        yield return StartCoroutine(CreateCanvasStep());
        
        // 2단계: PopupManager 생성
        yield return StartCoroutine(CreatePopupManagerStep());
        
        // 3단계: UI Generator 생성 및 실행
        yield return StartCoroutine(CreateUIGeneratorStep());
        
        // 4단계: Prefab Generator 생성 및 실행
        yield return StartCoroutine(CreatePrefabGeneratorStep());
        
        // 5단계: 최종 연결 및 검증
        yield return StartCoroutine(FinalizeStep());
        
        Debug.Log("=== Teacher Dashboard UI 생성 완료 ===");
        Debug.Log("이제 Play 모드에서 생성된 UI를 확인하고 복사할 수 있습니다!");
    }
    
    private IEnumerator CreateCanvasStep()
    {
        Debug.Log("1단계: Canvas 생성");
        
        if (createCanvas && mainCanvas == null)
        {
            // Canvas 생성
            GameObject canvasObj = new GameObject("TeacherDashboardCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;
            
            // CanvasScaler 추가
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // GraphicRaycaster 추가
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("Canvas 생성 완료");
        }
        else if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
            Debug.Log("기존 Canvas 사용");
        }
        
        yield return new WaitForSeconds(delayBetweenSteps);
    }
    
    private IEnumerator CreatePopupManagerStep()
    {
        Debug.Log("2단계: PopupManager 생성");
        
        if (popupManager == null)
        {
            GameObject popupManagerObj = new GameObject("PopupManager");
            popupManager = popupManagerObj.AddComponent<PopupManager>();
            
            // PopupManager를 Canvas의 최상위에 배치
            popupManagerObj.transform.SetParent(mainCanvas.transform, false);
            
            Debug.Log("PopupManager 생성 완료");
        }
        
        yield return new WaitForSeconds(delayBetweenSteps);
    }
    
    private IEnumerator CreateUIGeneratorStep()
    {
        Debug.Log("3단계: UI Generator 생성 및 실행");
        
        if (uiGenerator == null)
        {
            GameObject generatorObj = new GameObject("UIGenerator");
            uiGenerator = generatorObj.AddComponent<TeacherDashboardUIGenerator>();
            
            // Canvas 할당
            uiGenerator.targetCanvas = mainCanvas;
            
            Debug.Log("UI Generator 생성 완료 - UI 생성 대기");
        }
        
        // UI Generator가 UI를 생성할 시간을 기다림
        yield return new WaitForSeconds(1f);
    }
    
    private IEnumerator CreatePrefabGeneratorStep()
    {
        Debug.Log("4단계: Prefab Generator 생성 및 실행");
        
        if (prefabGenerator == null)
        {
            GameObject prefabGeneratorObj = new GameObject("PrefabGenerator");
            prefabGenerator = prefabGeneratorObj.AddComponent<UIPrefabGenerator>();
            
            Debug.Log("Prefab Generator 생성 완료 - 프리팹 생성 대기");
        }
        
        // Prefab Generator가 프리팹을 생성할 시간을 기다림
        yield return new WaitForSeconds(1f);
    }
    
    private IEnumerator FinalizeStep()
    {
        Debug.Log("5단계: 최종 연결 및 검증");
        
        // TeacherDashboardUI 컴포넌트 찾기
        TeacherDashboardUI dashboardUI = FindObjectOfType<TeacherDashboardUI>();
        if (dashboardUI != null)
        {
            Debug.Log("✓ TeacherDashboardUI 컴포넌트 확인됨");
            
            // 필수 컴포넌트들 검증
            ValidateUIComponents(dashboardUI);
        }
        else
        {
            Debug.LogError("✗ TeacherDashboardUI 컴포넌트를 찾을 수 없습니다!");
        }
        
        // EventSystem 확인
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.Log("EventSystem이 없어서 생성합니다.");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        yield return new WaitForSeconds(delayBetweenSteps);
        
        Debug.Log("==========================================");
        Debug.Log("🎉 Teacher Dashboard UI 생성이 완료되었습니다!");
        Debug.Log("📋 다음 단계:");
        Debug.Log("1. Play 모드에서 UI가 올바르게 표시되는지 확인");
        Debug.Log("2. Hierarchy에서 'TeacherDashboardUI' 오브젝트를 선택");
        Debug.Log("3. 우클릭 → Copy");
        Debug.Log("4. Play 모드 종료");
        Debug.Log("5. Scene에서 우클릭 → Paste");
        Debug.Log("6. Inspector에서 각 필드들이 올바르게 연결되었는지 확인");
        Debug.Log("==========================================");
    }
    
    private void ValidateUIComponents(TeacherDashboardUI dashboardUI)
    {
        Debug.Log("--- UI 컴포넌트 검증 ---");
        
        // 리플렉션을 사용하여 모든 SerializeField 검증
        var fields = dashboardUI.GetType().GetFields(
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
        
        int connectedFields = 0;
        int totalFields = 0;
        
        foreach (var field in fields)
        {
            var serializeFieldAttr = System.Attribute.GetCustomAttribute(field, typeof(SerializeField));
            var headerAttr = System.Attribute.GetCustomAttribute(field, typeof(HeaderAttribute));
            
            if (serializeFieldAttr != null || field.IsPublic)
            {
                totalFields++;
                object value = field.GetValue(dashboardUI);
                
                if (value != null)
                {
                    connectedFields++;
                    Debug.Log($"✓ {field.Name}: {value.GetType().Name}");
                }
                else
                {
                    Debug.LogWarning($"✗ {field.Name}: null");
                }
            }
        }
        
        Debug.Log($"연결된 필드: {connectedFields}/{totalFields}");
        
        if (connectedFields == totalFields)
        {
            Debug.Log("🎉 모든 UI 컴포넌트가 올바르게 연결되었습니다!");
        }
        else
        {
            Debug.LogWarning($"⚠️ {totalFields - connectedFields}개의 필드가 연결되지 않았습니다.");
        }
    }
    
    /// <summary>
    /// 수동으로 UI 생성을 시작하는 메서드 (Inspector 버튼용)
    /// </summary>
    [ContextMenu("Create Teacher Dashboard UI")]
    public void CreateUIManually()
    {
        StartCoroutine(CreateCompleteUI());
    }
    
    /// <summary>
    /// 생성된 UI를 모두 삭제하는 메서드 (테스트용)
    /// </summary>
    [ContextMenu("Clear All Generated UI")]
    public void ClearGeneratedUI()
    {
        TeacherDashboardUI[] dashboards = FindObjectsOfType<TeacherDashboardUI>();
        foreach (var dashboard in dashboards)
        {
            if (Application.isPlaying)
                Destroy(dashboard.gameObject);
            else
                DestroyImmediate(dashboard.gameObject);
        }
        
        if (uiGenerator != null)
        {
            if (Application.isPlaying)
                Destroy(uiGenerator.gameObject);
            else
                DestroyImmediate(uiGenerator.gameObject);
        }
        
        if (prefabGenerator != null)
        {
            if (Application.isPlaying)
                Destroy(prefabGenerator.gameObject);
            else
                DestroyImmediate(prefabGenerator.gameObject);
        }
        
        Debug.Log("생성된 UI가 모두 삭제되었습니다.");
    }
    
    void Update()
    {
        // 키보드 단축키로 UI 생성/삭제
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CreateUIManually();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ClearGeneratedUI();
        }
    }
}
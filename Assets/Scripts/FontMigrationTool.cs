using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

#if UNITY_EDITOR
public class FontMigrationTool : EditorWindow
{
    private TMP_FontAsset newDefaultFont;
    private TMP_FontAsset oldFont;
    private List<GameObject> foundObjects = new List<GameObject>();
    private List<string> foundPrefabs = new List<string>();
    private Vector2 scrollPosition;
    private bool showFoundObjects = false;
    
    [MenuItem("Tools/Font Migration Tool")]
    public static void ShowWindow()
    {
        GetWindow<FontMigrationTool>("Font Migration Tool");
    }
    
    void OnGUI()
    {
        GUILayout.Label("폰트 마이그레이션 도구", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // 폰트 선택
        GUILayout.Label("교체할 폰트들:");
        oldFont = (TMP_FontAsset)EditorGUILayout.ObjectField("기존 폰트 (Liberation Sans SDF)", oldFont, typeof(TMP_FontAsset), false);
        newDefaultFont = (TMP_FontAsset)EditorGUILayout.ObjectField("새 기본 폰트 (Noto Sans KR)", newDefaultFont, typeof(TMP_FontAsset), false);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("1. 프로젝트 스캔 (폰트 사용 현황 분석)", GUILayout.Height(30)))
        {
            ScanProject();
        }
        
        if (foundObjects.Count > 0 || foundPrefabs.Count > 0)
        {
            GUILayout.Space(5);
            showFoundObjects = EditorGUILayout.Foldout(showFoundObjects, $"발견된 오브젝트: {foundObjects.Count}개, 프리팹: {foundPrefabs.Count}개");
            
            if (showFoundObjects)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                
                GUILayout.Label("씬 오브젝트:", EditorStyles.boldLabel);
                foreach (var obj in foundObjects)
                {
                    if (obj != null)
                    {
                        EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                    }
                }
                
                GUILayout.Label("프리팹:", EditorStyles.boldLabel);
                foreach (var prefabPath in foundPrefabs)
                {
                    GUILayout.Label(prefabPath);
                }
                
                GUILayout.EndScrollView();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("2. TextMeshPro 기본 설정 변경", GUILayout.Height(30)))
            {
                ChangeDefaultTMPSettings();
            }
            
            if (GUILayout.Button("3. 씬 오브젝트 폰트 교체", GUILayout.Height(30)))
            {
                ReplaceSceneFonts();
            }
            
            if (GUILayout.Button("4. 프리팹 폰트 교체", GUILayout.Height(30)))
            {
                ReplacePrefabFonts();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("5. 스크립트 코드 업데이트", GUILayout.Height(30)))
            {
                UpdateScriptCodes();
            }
            
            GUILayout.Space(10);
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("6. 기존 폰트 삭제 (주의!)", GUILayout.Height(30)))
            {
                DeleteOldFont();
            }
            GUI.backgroundColor = Color.white;
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("전체 프로세스 자동 실행", GUILayout.Height(40)))
        {
            RunFullMigration();
        }
    }
    
    private void ScanProject()
    {
        foundObjects.Clear();
        foundPrefabs.Clear();
        
        if (oldFont == null)
        {
            Debug.LogError("교체할 기존 폰트를 선택해주세요.");
            return;
        }
        
        // 씬 오브젝트 스캔
        TextMeshProUGUI[] sceneTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        TextMeshPro[] scene3DTexts = FindObjectsOfType<TextMeshPro>(true);
        
        foreach (var text in sceneTexts)
        {
            if (text.font == oldFont)
            {
                foundObjects.Add(text.gameObject);
            }
        }
        
        foreach (var text in scene3DTexts)
        {
            if (text.font == oldFont)
            {
                foundObjects.Add(text.gameObject);
            }
        }
        
        // 프리팹 스캔
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                TextMeshProUGUI[] prefabTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                TextMeshPro[] prefab3DTexts = prefab.GetComponentsInChildren<TextMeshPro>(true);
                
                bool hasOldFont = false;
                
                foreach (var text in prefabTexts)
                {
                    if (text.font == oldFont)
                    {
                        hasOldFont = true;
                        break;
                    }
                }
                
                if (!hasOldFont)
                {
                    foreach (var text in prefab3DTexts)
                    {
                        if (text.font == oldFont)
                        {
                            hasOldFont = true;
                            break;
                        }
                    }
                }
                
                if (hasOldFont)
                {
                    foundPrefabs.Add(path);
                }
            }
        }
        
        Debug.Log($"스캔 완료: 씬 오브젝트 {foundObjects.Count}개, 프리팹 {foundPrefabs.Count}개에서 기존 폰트 발견");
    }
    
    private void ChangeDefaultTMPSettings()
    {
        if (newDefaultFont == null)
        {
            Debug.LogError("새 기본 폰트를 선택해주세요.");
            return;
        }
        
        // TMP Settings 찾기
        TMP_Settings tmpSettings = TMP_Settings.instance;
        if (tmpSettings == null)
        {
            Debug.LogError("TMP_Settings를 찾을 수 없습니다.");
            return;
        }
        
        // 기본 폰트 변경
        SerializedObject serializedSettings = new SerializedObject(tmpSettings);
        SerializedProperty defaultFontAssetProperty = serializedSettings.FindProperty("m_defaultFontAsset");
        
        if (defaultFontAssetProperty != null)
        {
            defaultFontAssetProperty.objectReferenceValue = newDefaultFont;
            serializedSettings.ApplyModifiedProperties();
            
            Debug.Log($"TextMeshPro 기본 폰트가 {newDefaultFont.name}으로 변경되었습니다.");
            
            // TMP Settings 저장
            EditorUtility.SetDirty(tmpSettings);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.LogError("TMP Settings의 기본 폰트 속성을 찾을 수 없습니다.");
        }
    }
    
    private void ReplaceSceneFonts()
    {
        if (newDefaultFont == null || oldFont == null)
        {
            Debug.LogError("폰트를 모두 선택해주세요.");
            return;
        }
        
        int replacedCount = 0;
        
        // 씬의 모든 TextMeshPro 컴포넌트 교체
        TextMeshProUGUI[] sceneTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        TextMeshPro[] scene3DTexts = FindObjectsOfType<TextMeshPro>(true);
        
        foreach (var text in sceneTexts)
        {
            if (text.font == oldFont)
            {
                Undo.RecordObject(text, "Replace Font");
                text.font = newDefaultFont;
                EditorUtility.SetDirty(text);
                replacedCount++;
            }
        }
        
        foreach (var text in scene3DTexts)
        {
            if (text.font == oldFont)
            {
                Undo.RecordObject(text, "Replace Font");
                text.font = newDefaultFont;
                EditorUtility.SetDirty(text);
                replacedCount++;
            }
        }
        
        Debug.Log($"씬에서 {replacedCount}개의 텍스트 폰트를 교체했습니다.");
    }
    
    private void ReplacePrefabFonts()
    {
        if (newDefaultFont == null || oldFont == null)
        {
            Debug.LogError("폰트를 모두 선택해주세요.");
            return;
        }
        
        int replacedPrefabs = 0;
        int replacedComponents = 0;
        
        foreach (string prefabPath in foundPrefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;
            
            bool modified = false;
            
            // 프리팹의 모든 TextMeshPro 컴포넌트 교체
            TextMeshProUGUI[] prefabTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            TextMeshPro[] prefab3DTexts = prefab.GetComponentsInChildren<TextMeshPro>(true);
            
            foreach (var text in prefabTexts)
            {
                if (text.font == oldFont)
                {
                    text.font = newDefaultFont;
                    modified = true;
                    replacedComponents++;
                }
            }
            
            foreach (var text in prefab3DTexts)
            {
                if (text.font == oldFont)
                {
                    text.font = newDefaultFont;
                    modified = true;
                    replacedComponents++;
                }
            }
            
            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                replacedPrefabs++;
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"{replacedPrefabs}개 프리팹에서 {replacedComponents}개의 텍스트 폰트를 교체했습니다.");
    }
    
    private void UpdateScriptCodes()
    {
        Debug.Log("스크립트 코드 업데이트를 시작합니다...");
        
        // 동적 UI 생성 스크립트들 업데이트
        UpdateUIGeneratorScripts();
        
        Debug.Log("스크립트 코드 업데이트가 완료되었습니다.");
    }
    
    private void UpdateUIGeneratorScripts()
    {
        if (newDefaultFont == null) return;
        
        string fontAssetName = newDefaultFont.name;
        
        // TeacherDashboardUIGenerator.cs 업데이트
        string generatorPath = "/mnt/c/Users/blitz/School Ai Counseling App/Assets/Scripts/TeacherDashboardUIGenerator.cs";
        if (File.Exists(generatorPath))
        {
            string content = File.ReadAllText(generatorPath);
            
            // 폰트 설정 코드 추가
            if (!content.Contains("// 폰트 설정"))
            {
                string fontSetupCode = @"
        // 폰트 설정
        if (textComponent != null && newDefaultFont != null)
        {
            textComponent.font = Resources.Load<TMP_FontAsset>(""" + fontAssetName + @""");
        }";
                
                // CreateInfoText 메소드에 폰트 설정 추가
                content = content.Replace(
                    "textComponent.fontSize = fontSize;",
                    "textComponent.fontSize = fontSize;" + fontSetupCode
                );
                
                File.WriteAllText(generatorPath, content);
                Debug.Log("TeacherDashboardUIGenerator.cs 업데이트 완료");
            }
        }
    }
    
    private void DeleteOldFont()
    {
        if (oldFont == null)
        {
            Debug.LogError("삭제할 폰트를 선택해주세요.");
            return;
        }
        
        if (EditorUtility.DisplayDialog("폰트 삭제 확인", 
            $"{oldFont.name} 폰트를 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.", 
            "삭제", "취소"))
        {
            string fontPath = AssetDatabase.GetAssetPath(oldFont);
            AssetDatabase.DeleteAsset(fontPath);
            AssetDatabase.Refresh();
            
            Debug.Log($"{oldFont.name} 폰트가 삭제되었습니다.");
            oldFont = null;
        }
    }
    
    private void RunFullMigration()
    {
        if (newDefaultFont == null)
        {
            Debug.LogError("새 기본 폰트를 선택해주세요.");
            return;
        }
        
        if (EditorUtility.DisplayDialog("전체 마이그레이션", 
            "전체 폰트 마이그레이션을 실행하시겠습니까?\n이 작업은 시간이 걸릴 수 있습니다.", 
            "실행", "취소"))
        {
            try
            {
                EditorUtility.DisplayProgressBar("폰트 마이그레이션", "프로젝트 스캔 중...", 0.1f);
                ScanProject();
                
                EditorUtility.DisplayProgressBar("폰트 마이그레이션", "기본 설정 변경 중...", 0.3f);
                ChangeDefaultTMPSettings();
                
                EditorUtility.DisplayProgressBar("폰트 마이그레이션", "씬 오브젝트 교체 중...", 0.5f);
                ReplaceSceneFonts();
                
                EditorUtility.DisplayProgressBar("폰트 마이그레이션", "프리팹 교체 중...", 0.7f);
                ReplacePrefabFonts();
                
                EditorUtility.DisplayProgressBar("폰트 마이그레이션", "스크립트 업데이트 중...", 0.9f);
                UpdateScriptCodes();
                
                EditorUtility.ClearProgressBar();
                
                Debug.Log("전체 폰트 마이그레이션이 완료되었습니다!");
                EditorUtility.DisplayDialog("완료", "폰트 마이그레이션이 성공적으로 완료되었습니다.", "확인");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"마이그레이션 중 오류 발생: {e.Message}");
            }
        }
    }
}
#endif
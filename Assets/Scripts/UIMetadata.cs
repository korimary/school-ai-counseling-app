using UnityEngine;

/// <summary>
/// UI 생성 정보를 저장하는 메타데이터 컴포넌트
/// </summary>
public class UIMetadata : MonoBehaviour
{
    [Header("UI 메타데이터")]
    public string creationTime;
    public string version;
    public string description;
    
    [Header("생성 정보")]
    [TextArea(3, 5)]
    public string creationNotes = "복사 최적화된 동적 UI 생성으로 만들어진 UI입니다.";
    
    private void Start()
    {
        // 메타데이터 로그 출력
        Debug.Log($"UI 메타데이터 - 생성시간: {creationTime}, 버전: {version}, 설명: {description}");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupManager : MonoBehaviour
{
    private static PopupManager instance;
    public static PopupManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PopupManager>();
            }
            return instance;
        }
    }
    
    [Header("메시지 팝업")]
    [SerializeField] private GameObject messagePopupPrefab;
    [SerializeField] private Transform popupContainer;
    
    [Header("에러 팝업")]
    [SerializeField] private GameObject errorPopupPrefab;
    
    [Header("확인 다이얼로그")]
    [SerializeField] private GameObject confirmPopupPrefab;
    
    [Header("로딩 인디케이터")]
    [SerializeField] private GameObject loadingPopupPrefab;
    
    private Queue<PopupData> popupQueue = new Queue<PopupData>();
    private GameObject currentPopup;
    private bool isShowingPopup = false;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // 일반 메시지 팝업
    public void ShowMessage(string message, float duration = 2f)
    {
        var popupData = new PopupData
        {
            type = PopupType.Message,
            message = message,
            duration = duration
        };
        
        EnqueuePopup(popupData);
    }
    
    // 에러 팝업
    public void ShowError(string errorMessage)
    {
        var popupData = new PopupData
        {
            type = PopupType.Error,
            title = "오류",
            message = errorMessage,
            duration = 0f // 수동으로 닫아야 함
        };
        
        EnqueuePopup(popupData);
    }
    
    // 확인 다이얼로그
    public void ShowConfirmDialog(string message, System.Action onConfirm, System.Action onCancel = null)
    {
        var popupData = new PopupData
        {
            type = PopupType.Confirm,
            title = "확인",
            message = message,
            onConfirm = onConfirm,
            onCancel = onCancel,
            duration = 0f
        };
        
        EnqueuePopup(popupData);
    }
    
    // 로딩 팝업
    public GameObject ShowLoading(string message = "로딩 중...")
    {
        if (loadingPopupPrefab == null) return null;
        
        GameObject loadingPopup = Instantiate(loadingPopupPrefab, popupContainer ?? transform);
        
        var messageText = loadingPopup.GetComponentInChildren<TextMeshProUGUI>();
        if (messageText != null)
            messageText.text = message;
            
        return loadingPopup;
    }
    
    public void HideLoading(GameObject loadingPopup)
    {
        if (loadingPopup != null)
            Destroy(loadingPopup);
    }
    
    private void EnqueuePopup(PopupData data)
    {
        popupQueue.Enqueue(data);
        
        if (!isShowingPopup)
        {
            StartCoroutine(ProcessPopupQueue());
        }
    }
    
    private IEnumerator ProcessPopupQueue()
    {
        while (popupQueue.Count > 0)
        {
            isShowingPopup = true;
            var popupData = popupQueue.Dequeue();
            
            yield return ShowPopup(popupData);
            
            // 팝업 간 간격
            yield return new WaitForSeconds(0.2f);
        }
        
        isShowingPopup = false;
    }
    
    private IEnumerator ShowPopup(PopupData data)
    {
        GameObject prefab = null;
        
        switch (data.type)
        {
            case PopupType.Message:
                prefab = messagePopupPrefab;
                break;
            case PopupType.Error:
                prefab = errorPopupPrefab;
                break;
            case PopupType.Confirm:
                prefab = confirmPopupPrefab;
                break;
        }
        
        if (prefab == null)
        {
            Debug.LogError($"팝업 프리팹이 없습니다: {data.type}");
            yield break;
        }
        
        currentPopup = Instantiate(prefab, popupContainer ?? transform);
        
        // 팝업 설정
        SetupPopup(currentPopup, data);
        
        // 애니메이션
        yield return AnimatePopupIn(currentPopup);
        
        // 자동 닫기 대기
        if (data.duration > 0)
        {
            yield return new WaitForSeconds(data.duration);
            yield return CloseCurrentPopup();
        }
    }
    
    private void SetupPopup(GameObject popup, PopupData data)
    {
        // 제목 설정
        var titleText = popup.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null && !string.IsNullOrEmpty(data.title))
        {
            titleText.text = data.title;
        }
        
        // 메시지 설정
        var messageText = popup.transform.Find("Message")?.GetComponent<TextMeshProUGUI>();
        if (messageText != null)
        {
            messageText.text = data.message;
        }
        
        // 버튼 설정
        switch (data.type)
        {
            case PopupType.Message:
                // 메시지 팝업은 버튼 없음 (자동으로 닫힘)
                break;
                
            case PopupType.Error:
                var closeButton = popup.transform.Find("CloseButton")?.GetComponent<Button>();
                if (closeButton != null)
                {
                    closeButton.onClick.AddListener(() => StartCoroutine(CloseCurrentPopup()));
                }
                break;
                
            case PopupType.Confirm:
                var confirmButton = popup.transform.Find("ConfirmButton")?.GetComponent<Button>();
                var cancelButton = popup.transform.Find("CancelButton")?.GetComponent<Button>();
                
                if (confirmButton != null)
                {
                    confirmButton.onClick.AddListener(() =>
                    {
                        data.onConfirm?.Invoke();
                        StartCoroutine(CloseCurrentPopup());
                    });
                }
                
                if (cancelButton != null)
                {
                    cancelButton.onClick.AddListener(() =>
                    {
                        data.onCancel?.Invoke();
                        StartCoroutine(CloseCurrentPopup());
                    });
                }
                break;
        }
    }
    
    private IEnumerator AnimatePopupIn(GameObject popup)
    {
        var canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = popup.AddComponent<CanvasGroup>();
            
        var rectTransform = popup.GetComponent<RectTransform>();
        
        // 초기 상태
        canvasGroup.alpha = 0f;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * 0.8f;
        }
        
        // 애니메이션
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);
            }
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }
    }
    
    private IEnumerator CloseCurrentPopup()
    {
        if (currentPopup == null) yield break;
        
        var canvasGroup = currentPopup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = currentPopup.AddComponent<CanvasGroup>();
            
        // 페이드 아웃
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        
        Destroy(currentPopup);
        currentPopup = null;
    }
    
    // 외부에서 현재 팝업 강제로 닫기
    public void ForceCloseCurrentPopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }
    }
    
    private class PopupData
    {
        public PopupType type;
        public string title;
        public string message;
        public float duration;
        public System.Action onConfirm;
        public System.Action onCancel;
    }
    
    private enum PopupType
    {
        Message,
        Error,
        Confirm
    }
}

// 간단한 메시지 팝업 컴포넌트
public class SimpleMessagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        StartCoroutine(AutoClose());
    }
    
    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
    
    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(displayDuration);
        
        // 페이드 아웃
        float fadeTime = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ErrorHandler : MonoBehaviour
{
    [Header("에러 UI 설정")]
    [SerializeField] private GameObject errorPopupPrefab;
    [SerializeField] private Transform errorPopupParent;
    [SerializeField] private float popupDuration = 3f;
    [SerializeField] private bool enableDebugMode = true;

    [Header("네트워크 에러 설정")]
    [SerializeField] private int maxRetryAttempts = 3;
    [SerializeField] private float retryDelay = 2f;
    [SerializeField] private bool showNetworkErrors = true;

    private static ErrorHandler _instance;
    public static ErrorHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ErrorHandler>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ErrorHandler");
                    _instance = go.AddComponent<ErrorHandler>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // 이벤트 시스템
    public static event System.Action<ErrorInfo> OnErrorOccurred;
    public static event System.Action<ErrorInfo> OnErrorResolved;

    // 에러 로그
    private List<ErrorInfo> errorLog = new List<ErrorInfo>();
    private Queue<ErrorInfo> pendingErrors = new Queue<ErrorInfo>();
    private bool isShowingError = false;

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

    private void Initialize()
    {
        // Unity 에러 로그 캐치
        Application.logMessageReceived += OnLogMessageReceived;
        
        Debug.Log("ErrorHandler 초기화 완료");
    }

    private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            ErrorInfo errorInfo = new ErrorInfo
            {
                id = Guid.NewGuid().ToString(),
                message = logString,
                stackTrace = stackTrace,
                errorType = ErrorType.System,
                severity = ErrorSeverity.High,
                timestamp = DateTime.Now,
                isResolved = false
            };

            LogError(errorInfo);
        }
    }

    /// <summary>
    /// 일반 에러 로깅
    /// </summary>
    public void LogError(string message, ErrorType errorType = ErrorType.General, ErrorSeverity severity = ErrorSeverity.Medium)
    {
        ErrorInfo errorInfo = new ErrorInfo
        {
            id = Guid.NewGuid().ToString(),
            message = message,
            errorType = errorType,
            severity = severity,
            timestamp = DateTime.Now,
            isResolved = false
        };

        LogError(errorInfo);
    }

    /// <summary>
    /// 상세 에러 로깅
    /// </summary>
    public void LogError(ErrorInfo errorInfo)
    {
        errorLog.Add(errorInfo);
        
        if (enableDebugMode)
        {
            Debug.LogError($"[ErrorHandler] {errorInfo.errorType}: {errorInfo.message}");
        }

        OnErrorOccurred?.Invoke(errorInfo);

        // 에러 팝업 표시 결정
        if (ShouldShowErrorPopup(errorInfo))
        {
            if (isShowingError)
            {
                pendingErrors.Enqueue(errorInfo);
            }
            else
            {
                ShowErrorPopup(errorInfo);
            }
        }

        // 자동 복구 시도
        TryAutoRecover(errorInfo);
    }

    /// <summary>
    /// 네트워크 에러 처리
    /// </summary>
    public void HandleNetworkError(string operation, string errorMessage, System.Action retryAction = null)
    {
        ErrorInfo errorInfo = new ErrorInfo
        {
            id = Guid.NewGuid().ToString(),
            message = $"네트워크 오류: {operation} - {errorMessage}",
            errorType = ErrorType.Network,
            severity = ErrorSeverity.Medium,
            timestamp = DateTime.Now,
            isResolved = false,
            retryAction = retryAction,
            retryCount = 0
        };

        LogError(errorInfo);

        // 자동 재시도
        if (retryAction != null && errorInfo.retryCount < maxRetryAttempts)
        {
            StartCoroutine(RetryOperation(errorInfo));
        }
    }

    /// <summary>
    /// 사용자 입력 에러 처리
    /// </summary>
    public void HandleUserInputError(string fieldName, string errorMessage, GameObject inputField = null)
    {
        ErrorInfo errorInfo = new ErrorInfo
        {
            id = Guid.NewGuid().ToString(),
            message = $"입력 오류: {fieldName} - {errorMessage}",
            errorType = ErrorType.UserInput,
            severity = ErrorSeverity.Low,
            timestamp = DateTime.Now,
            isResolved = false,
            relatedGameObject = inputField
        };

        LogError(errorInfo);

        // 입력 필드 하이라이트
        if (inputField != null)
        {
            HighlightErrorField(inputField);
        }
    }

    /// <summary>
    /// API 에러 처리
    /// </summary>
    public void HandleAPIError(string apiName, int statusCode, string errorMessage, System.Action retryAction = null)
    {
        ErrorSeverity severity = statusCode >= 500 ? ErrorSeverity.High : ErrorSeverity.Medium;
        
        ErrorInfo errorInfo = new ErrorInfo
        {
            id = Guid.NewGuid().ToString(),
            message = $"API 오류: {apiName} (코드: {statusCode}) - {errorMessage}",
            errorType = ErrorType.API,
            severity = severity,
            timestamp = DateTime.Now,
            isResolved = false,
            retryAction = retryAction,
            retryCount = 0
        };

        LogError(errorInfo);

        // 5xx 에러는 자동 재시도
        if (statusCode >= 500 && retryAction != null && errorInfo.retryCount < maxRetryAttempts)
        {
            StartCoroutine(RetryOperation(errorInfo));
        }
    }

    /// <summary>
    /// 데이터 저장 에러 처리
    /// </summary>
    public void HandleDataError(string operation, string errorMessage, System.Action recoveryAction = null)
    {
        ErrorInfo errorInfo = new ErrorInfo
        {
            id = Guid.NewGuid().ToString(),
            message = $"데이터 오류: {operation} - {errorMessage}",
            errorType = ErrorType.Data,
            severity = ErrorSeverity.High,
            timestamp = DateTime.Now,
            isResolved = false,
            retryAction = recoveryAction
        };

        LogError(errorInfo);

        // 데이터 복구 시도
        if (recoveryAction != null)
        {
            recoveryAction.Invoke();
        }
    }

    private bool ShouldShowErrorPopup(ErrorInfo errorInfo)
    {
        // 시스템 에러와 높은 심각도 에러만 팝업 표시
        if (errorInfo.errorType == ErrorType.System && errorInfo.severity == ErrorSeverity.High)
            return true;
            
        // 네트워크 에러는 설정에 따라
        if (errorInfo.errorType == ErrorType.Network && showNetworkErrors)
            return true;
            
        // API 에러 중 중요한 것들
        if (errorInfo.errorType == ErrorType.API && errorInfo.severity >= ErrorSeverity.Medium)
            return true;

        return false;
    }

    private void ShowErrorPopup(ErrorInfo errorInfo)
    {
        if (errorPopupPrefab == null) return;

        isShowingError = true;

        GameObject popup = Instantiate(errorPopupPrefab, errorPopupParent);
        ErrorPopup popupComponent = popup.GetComponent<ErrorPopup>();

        if (popupComponent != null)
        {
            popupComponent.Setup(errorInfo, () => {
                isShowingError = false;
                CheckPendingErrors();
            });
        }
        else
        {
            // 기본 팝업 설정
            SetupBasicErrorPopup(popup, errorInfo);
        }

        // 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayError();
        }
    }

    private void SetupBasicErrorPopup(GameObject popup, ErrorInfo errorInfo)
    {
        TextMeshProUGUI messageText = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (messageText != null)
        {
            messageText.text = GetUserFriendlyMessage(errorInfo);
        }

        Button closeButton = popup.GetComponentInChildren<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => {
                Destroy(popup);
                isShowingError = false;
                CheckPendingErrors();
            });
        }

        // 자동 닫기
        StartCoroutine(AutoClosePopup(popup, popupDuration));
    }

    private IEnumerator AutoClosePopup(GameObject popup, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (popup != null)
        {
            if (UIAnimationManager.Instance != null)
            {
                UIAnimationManager.Instance.FadeOutPanel(popup, () => {
                    if (popup != null) Destroy(popup);
                });
            }
            else
            {
                Destroy(popup);
            }
        }
        
        isShowingError = false;
        CheckPendingErrors();
    }

    private void CheckPendingErrors()
    {
        if (pendingErrors.Count > 0 && !isShowingError)
        {
            ErrorInfo nextError = pendingErrors.Dequeue();
            ShowErrorPopup(nextError);
        }
    }

    private string GetUserFriendlyMessage(ErrorInfo errorInfo)
    {
        switch (errorInfo.errorType)
        {
            case ErrorType.Network:
                return "인터넷 연결을 확인해주세요. 잠시 후 다시 시도됩니다.";
            case ErrorType.API:
                return "서버와 통신 중 문제가 발생했습니다. 잠시 후 다시 시도해주세요.";
            case ErrorType.UserInput:
                return "입력 정보를 확인해주세요.";
            case ErrorType.Data:
                return "데이터 처리 중 문제가 발생했습니다.";
            case ErrorType.System:
                return "시스템 오류가 발생했습니다. 앱을 다시 시작해주세요.";
            default:
                return "예상치 못한 문제가 발생했습니다.";
        }
    }

    private IEnumerator RetryOperation(ErrorInfo errorInfo)
    {
        yield return new WaitForSeconds(retryDelay);
        
        errorInfo.retryCount++;
        
        if (enableDebugMode)
        {
            Debug.Log($"재시도 {errorInfo.retryCount}/{maxRetryAttempts}: {errorInfo.message}");
        }

        try
        {
            errorInfo.retryAction?.Invoke();
        }
        catch (Exception e)
        {
            if (errorInfo.retryCount < maxRetryAttempts)
            {
                StartCoroutine(RetryOperation(errorInfo));
            }
            else
            {
                LogError($"재시도 실패: {e.Message}", ErrorType.System, ErrorSeverity.High);
            }
        }
    }

    private void TryAutoRecover(ErrorInfo errorInfo)
    {
        switch (errorInfo.errorType)
        {
            case ErrorType.Data:
                // 데이터 복구 시도
                TryDataRecovery(errorInfo);
                break;
            case ErrorType.Network:
                // 네트워크 재연결 시도
                StartCoroutine(CheckNetworkRecovery(errorInfo));
                break;
        }
    }

    private void TryDataRecovery(ErrorInfo errorInfo)
    {
        // 로컬 백업에서 데이터 복구 시도
        if (EmotionManager.Instance != null)
        {
            EmotionManager.Instance.LoadLocalData();
        }

        if (enableDebugMode)
        {
            Debug.Log("데이터 복구 시도 완료");
        }
    }

    private IEnumerator CheckNetworkRecovery(ErrorInfo errorInfo)
    {
        yield return new WaitForSeconds(5f);

        // 간단한 네트워크 체크
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            errorInfo.isResolved = true;
            OnErrorResolved?.Invoke(errorInfo);
            
            if (enableDebugMode)
            {
                Debug.Log("네트워크 연결 복구됨");
            }
        }
    }

    private void HighlightErrorField(GameObject inputField)
    {
        Image image = inputField.GetComponent<Image>();
        if (image != null)
        {
            StartCoroutine(HighlightCoroutine(image));
        }
    }

    private IEnumerator HighlightCoroutine(Image image)
    {
        Color originalColor = image.color;
        Color errorColor = Color.red;
        
        // 빨간색으로 깜빡이기
        for (int i = 0; i < 3; i++)
        {
            image.color = errorColor;
            yield return new WaitForSeconds(0.2f);
            image.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// 에러 로그 가져오기
    /// </summary>
    public List<ErrorInfo> GetErrorLog()
    {
        return new List<ErrorInfo>(errorLog);
    }

    /// <summary>
    /// 특정 타입의 에러 개수 가져오기
    /// </summary>
    public int GetErrorCount(ErrorType errorType)
    {
        return errorLog.FindAll(e => e.errorType == errorType && !e.isResolved).Count;
    }

    /// <summary>
    /// 에러 로그 클리어
    /// </summary>
    public void ClearErrorLog()
    {
        errorLog.Clear();
        pendingErrors.Clear();
        
        if (enableDebugMode)
        {
            Debug.Log("에러 로그가 클리어되었습니다.");
        }
    }

    /// <summary>
    /// 에러 통계 가져오기
    /// </summary>
    public ErrorStatistics GetErrorStatistics()
    {
        return new ErrorStatistics(errorLog);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
    }
}

/// <summary>
/// 에러 정보 클래스
/// </summary>
[Serializable]
public class ErrorInfo
{
    public string id;
    public string message;
    public string stackTrace;
    public ErrorType errorType;
    public ErrorSeverity severity;
    public DateTime timestamp;
    public bool isResolved;
    public int retryCount;
    public System.Action retryAction;
    public GameObject relatedGameObject;

    public override string ToString()
    {
        return $"[{timestamp:HH:mm:ss}] {errorType} ({severity}): {message}";
    }
}

/// <summary>
/// 에러 타입
/// </summary>
public enum ErrorType
{
    General,    // 일반
    Network,    // 네트워크
    API,        // API
    UserInput,  // 사용자 입력
    Data,       // 데이터
    System      // 시스템
}

/// <summary>
/// 에러 심각도
/// </summary>
public enum ErrorSeverity
{
    Low,        // 낮음
    Medium,     // 보통
    High,       // 높음
    Critical    // 심각
}

/// <summary>
/// 에러 통계
/// </summary>
[Serializable]
public class ErrorStatistics
{
    public int totalErrors;
    public int resolvedErrors;
    public Dictionary<ErrorType, int> errorsByType;
    public Dictionary<ErrorSeverity, int> errorsBySeverity;
    public DateTime oldestError;
    public DateTime newestError;

    public ErrorStatistics(List<ErrorInfo> errorLog)
    {
        totalErrors = errorLog.Count;
        resolvedErrors = errorLog.FindAll(e => e.isResolved).Count;
        
        errorsByType = new Dictionary<ErrorType, int>();
        errorsBySeverity = new Dictionary<ErrorSeverity, int>();
        
        foreach (var error in errorLog)
        {
            if (!errorsByType.ContainsKey(error.errorType))
                errorsByType[error.errorType] = 0;
            errorsByType[error.errorType]++;
            
            if (!errorsBySeverity.ContainsKey(error.severity))
                errorsBySeverity[error.severity] = 0;
            errorsBySeverity[error.severity]++;
        }
        
        if (errorLog.Count > 0)
        {
            oldestError = errorLog[0].timestamp;
            newestError = errorLog[errorLog.Count - 1].timestamp;
        }
    }
}

/// <summary>
/// 에러 팝업 컴포넌트
/// </summary>
public class ErrorPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button retryButton;

    private ErrorInfo errorInfo;
    private System.Action onClose;

    public void Setup(ErrorInfo errorInfo, System.Action onClose)
    {
        this.errorInfo = errorInfo;
        this.onClose = onClose;

        if (titleText != null)
        {
            titleText.text = GetErrorTitle(errorInfo.errorType);
        }

        if (messageText != null)
        {
            messageText.text = GetUserFriendlyMessage(errorInfo);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }

        if (retryButton != null)
        {
            if (errorInfo.retryAction != null)
            {
                retryButton.gameObject.SetActive(true);
                retryButton.onClick.AddListener(RetryAction);
            }
            else
            {
                retryButton.gameObject.SetActive(false);
            }
        }
    }

    private string GetErrorTitle(ErrorType errorType)
    {
        switch (errorType)
        {
            case ErrorType.Network: return "연결 오류";
            case ErrorType.API: return "서버 오류";
            case ErrorType.UserInput: return "입력 오류";
            case ErrorType.Data: return "데이터 오류";
            case ErrorType.System: return "시스템 오류";
            default: return "오류";
        }
    }

    private string GetUserFriendlyMessage(ErrorInfo errorInfo)
    {
        // ErrorHandler의 GetUserFriendlyMessage와 동일한 로직
        switch (errorInfo.errorType)
        {
            case ErrorType.Network:
                return "인터넷 연결을 확인해주세요.";
            case ErrorType.API:
                return "서버와 통신 중 문제가 발생했습니다.";
            case ErrorType.UserInput:
                return "입력 정보를 확인해주세요.";
            case ErrorType.Data:
                return "데이터 처리 중 문제가 발생했습니다.";
            case ErrorType.System:
                return "시스템 오류가 발생했습니다.";
            default:
                return "예상치 못한 문제가 발생했습니다.";
        }
    }

    private void ClosePopup()
    {
        onClose?.Invoke();
        Destroy(gameObject);
    }

    private void RetryAction()
    {
        errorInfo.retryAction?.Invoke();
        ClosePopup();
    }
}
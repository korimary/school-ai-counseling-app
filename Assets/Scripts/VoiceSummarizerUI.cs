using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VoiceSummarizerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button recordButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI transcriptionText;
    public TextMeshProUGUI summaryText;
    public ScrollRect transcriptionScrollRect;
    public ScrollRect summaryScrollRect;
    public TMP_InputField apiKeyInput;
    public Button saveButton;
    public Button viewHistoryButton;
    public TextMeshProUGUI instructionText;
    public Button settingsButton;
    public Button recordsButton;
    public Button dashboardButton;

    [Header("Components")]
    public AudioRecorder audioRecorder;
    public OpenAIClient openAIClient;
    public DataSaver dataSaver;

    private bool isProcessing = false;
    private string currentTranscription = "";
    private string currentSummary = "";
    private SchoolData schoolData;
    private int detectedStudentNumber = -1;

    void Start()
    {
        // 학교 데이터 로드
        schoolData = StudentDataManager.LoadSchoolData();
        
        SetupUI();
        SubscribeToEvents();
        LoadApiKey();
        UpdateInstructionText();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SetupUI()
    {
        recordButton.onClick.AddListener(OnRecordButtonClicked);
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        if (viewHistoryButton != null)
            viewHistoryButton.onClick.AddListener(OnViewHistoryClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        if (recordsButton != null)
            recordsButton.onClick.AddListener(OnRecordsClicked);
        if (dashboardButton != null)
            dashboardButton.onClick.AddListener(OnDashboardClicked);
        apiKeyInput.onEndEdit.AddListener(OnApiKeyChanged);
        
        // ScrollRect Content 사이즈 자동 조정 설정
        SetupScrollRects();
        
        UpdateUI();
        saveButton.interactable = false;
    }

    private void SubscribeToEvents()
    {
        // Audio Recorder Events
        audioRecorder.OnRecordingComplete += OnRecordingComplete;
        audioRecorder.OnError += OnError;

        // OpenAI Client Events
        openAIClient.OnTranscriptionComplete += OnTranscriptionComplete;
        openAIClient.OnSummaryComplete += OnSummaryComplete;
        openAIClient.OnError += OnError;

        // Data Saver Events
        dataSaver.OnSaveComplete += OnSaveComplete;
        dataSaver.OnError += OnError;
    }

    private void UnsubscribeFromEvents()
    {
        if (audioRecorder != null)
        {
            audioRecorder.OnRecordingComplete -= OnRecordingComplete;
            audioRecorder.OnError -= OnError;
        }

        if (openAIClient != null)
        {
            openAIClient.OnTranscriptionComplete -= OnTranscriptionComplete;
            openAIClient.OnSummaryComplete -= OnSummaryComplete;
            openAIClient.OnError -= OnError;
        }

        if (dataSaver != null)
        {
            dataSaver.OnSaveComplete -= OnSaveComplete;
            dataSaver.OnError -= OnError;
        }
    }

    private void OnRecordButtonClicked()
    {
        Debug.Log("[VoiceSummarizerUI] 녹음 버튼 클릭됨!");
        
        if (audioRecorder.IsRecording)
        {
            Debug.Log("[VoiceSummarizerUI] 녹음 중지 요청");
            audioRecorder.StopRecording();
        }
        else
        {
            if (string.IsNullOrEmpty(apiKeyInput.text))
            {
                Debug.LogWarning("[VoiceSummarizerUI] API 키가 입력되지 않음");
                statusText.text = "OpenAI API 키를 먼저 입력해주세요";
                statusText.color = Color.red;
                return;
            }

            Debug.Log("[VoiceSummarizerUI] 녹음 시작 요청");
            audioRecorder.StartRecording();
        }
        
        UpdateUI();
    }

    private void OnSaveButtonClicked()
    {
        Debug.Log("Save button clicked!");
        
        if (!string.IsNullOrEmpty(currentTranscription) && !string.IsNullOrEmpty(currentSummary))
        {
            Debug.Log($"Saving - Transcription length: {currentTranscription.Length}, Summary length: {currentSummary.Length}");
            
            // 학생 정보 포함하여 저장
            string studentName = "";
            if (detectedStudentNumber > 0 && schoolData != null)
            {
                var student = schoolData.students.Find(s => s.number == detectedStudentNumber);
                if (student != null)
                {
                    studentName = student.name;
                }
            }
            
            dataSaver.SaveSummary(currentTranscription, currentSummary, detectedStudentNumber, studentName);
            statusText.text = "Saving...";
            statusText.color = Color.blue;
        }
        else
        {
            Debug.LogError("Cannot save - transcription or summary is empty!");
            statusText.text = "Nothing to save!";
            statusText.color = Color.red;
        }
    }

    private void OnApiKeyChanged(string apiKey)
    {
        openAIClient.SetApiKey(apiKey);
        SaveApiKey(apiKey);
    }

    private void OnRecordingComplete(byte[] audioData)
    {
        isProcessing = true;
        statusText.text = "Converting speech to text...";
        statusText.color = Color.blue;
        
        openAIClient.TranscribeAudio(audioData);
        UpdateUI();
    }

    private void OnTranscriptionComplete(string transcription)
    {
        currentTranscription = transcription;
        transcriptionText.text = transcription;
        
        // 학생 번호 감지
        DetectStudentNumber(transcription);
        
        // TextMeshProUGUI 크기 강제 업데이트
        StartCoroutine(ForceUpdateTextAndScroll(transcriptionText, transcriptionScrollRect, false));
        
        statusText.text = "Generating summary...";
        
        // 학생 번호가 감지되면 프롬프트에 포함
        string enhancedTranscription = transcription;
        if (detectedStudentNumber > 0)
        {
            enhancedTranscription = $"[학생 번호: {detectedStudentNumber}번]\n{transcription}";
        }
        
        openAIClient.SummarizeText(enhancedTranscription);
    }

    private void OnSummaryComplete(string summary)
    {
        Debug.Log($"[UI] OnSummaryComplete 호출됨 - 길이: {summary.Length}자");
        Debug.Log($"[UI] 요약 내용 미리보기: {summary.Substring(0, Mathf.Min(100, summary.Length))}...");
        
        // 특수문자 필터링 (폰트 없는 문자들 제거)
        string filteredSummary = summary
            .Replace("【", "[")
            .Replace("】", "]")
            .Replace("〔", "(")
            .Replace("〕", ")")
            .Replace("〈", "<")
            .Replace("〉", ">")
            .Replace("｢", "「")
            .Replace("｣", "」");
            
        // 학생 번호가 감지되었으면 요약 상단에 표시
        if (detectedStudentNumber > 0 && schoolData != null)
        {
            var student = schoolData.students.Find(s => s.number == detectedStudentNumber);
            if (student != null)
            {
                filteredSummary = $"[{student.number}번 {student.name} 학생 상담 기록]\n\n{filteredSummary}";
            }
        }
            
        Debug.Log($"[UI] 필터링 후 길이: {filteredSummary.Length}자");
        
        currentSummary = filteredSummary;
        
        // UI 업데이트가 메인 스레드에서 실행되도록 강제
        if (summaryText != null)
        {
            summaryText.text = filteredSummary;
            Debug.Log($"[UI] summaryText.text 업데이트 완료: {summaryText.text.Length}자");
        }
        else
        {
            Debug.LogError("[UI] summaryText가 null입니다!");
        }
        
        // TextMeshProUGUI 크기 강제 업데이트
        StartCoroutine(ForceUpdateTextAndScroll(summaryText, summaryScrollRect, true));
        
        isProcessing = false;
        statusText.text = "Complete! Ready to save.";
        statusText.color = Color.green;
        
        saveButton.interactable = true;
        UpdateUI();
        
        Debug.Log("[UI] OnSummaryComplete 함수 완료");
    }

    private void OnSaveComplete()
    {
        statusText.text = "Saved successfully!";
        statusText.color = Color.green;
        
        // Clear the current data
        currentTranscription = "";
        currentSummary = "";
        transcriptionText.text = "";
        summaryText.text = "";
        saveButton.interactable = false;
    }

    private void OnError(string error)
    {
        statusText.text = $"Error: {error}";
        statusText.color = Color.red;
        isProcessing = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update record button
        if (audioRecorder.IsRecording)
        {
            var buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = "Stop Recording";
            
            var buttonImage = recordButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = Color.red;
            
            statusText.text = "Recording... Click to stop";
            statusText.color = Color.green;
        }
        else if (isProcessing)
        {
            var buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = "Processing...";
            
            var buttonImage = recordButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = Color.gray;
            
            recordButton.interactable = false;
        }
        else
        {
            var buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = "Start Recording";
            
            var buttonImage = recordButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = Color.white;
            
            recordButton.interactable = true;
            
            if (string.IsNullOrEmpty(statusText.text) || statusText.text.Contains("Click"))
            {
                statusText.text = "Click to start recording";
                statusText.color = Color.white;
            }
        }
    }

    private void LoadApiKey()
    {
        string savedApiKey = PlayerPrefs.GetString("OpenAI_API_Key", "");
        if (!string.IsNullOrEmpty(savedApiKey))
        {
            apiKeyInput.text = savedApiKey;
            openAIClient.SetApiKey(savedApiKey);
        }
    }

    private void SaveApiKey(string apiKey)
    {
        PlayerPrefs.SetString("OpenAI_API_Key", apiKey);
        PlayerPrefs.Save();
    }

    private void SetupScrollRects()
    {
        // Transcription ScrollRect 설정
        if (transcriptionScrollRect != null)
        {
            SetupScrollRectContent(transcriptionScrollRect);
        }
        
        // Summary ScrollRect 설정
        if (summaryScrollRect != null)
        {
            SetupScrollRectContent(summaryScrollRect);
        }
    }
    
    private void SetupScrollRectContent(ScrollRect scrollRect)
    {
        if (scrollRect == null || scrollRect.content == null) return;
        
        // ScrollRect 기본 설정 강제 적용
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = 20f;
        
        // Viewport와 Content 설정
        if (scrollRect.viewport == null)
        {
            scrollRect.viewport = scrollRect.GetComponent<RectTransform>();
        }
        
        // Content Size Fitter 비활성화 (제거 대신)
        ContentSizeFitter contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter != null)
        {
            contentSizeFitter.enabled = false;
            Debug.Log($"[UI] ContentSizeFitter 비활성화: {scrollRect.name}");
        }
        
        // Vertical Layout Group 비활성화 (제거 대신)
        VerticalLayoutGroup layoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
            Debug.Log($"[UI] VerticalLayoutGroup 비활성화: {scrollRect.name}");
        }
        
        // Content RectTransform 설정
        RectTransform contentRect = scrollRect.content;
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        
        Debug.Log($"[UI] ScrollRect 기본 설정 완료: {scrollRect.name}");
    }

    private System.Collections.IEnumerator ForceUpdateTextAndScroll(TextMeshProUGUI textComponent, ScrollRect scrollRect, bool toTop)
    {
        if (textComponent == null || scrollRect == null) yield break;
        
        Debug.Log($"[UI] TextMeshProUGUI 강제 업데이트 시작: {textComponent.name}");
        
        // TextMeshProUGUI 강제 업데이트
        textComponent.ForceMeshUpdate();
        
        // 여러 프레임 대기하여 레이아웃 완전히 재계산
        yield return null; // 1프레임
        yield return null; // 2프레임
        yield return null; // 3프레임
        
        // Canvas 강제 업데이트
        Canvas.ForceUpdateCanvases();
        
        // Content의 RectTransform 크기 강제 업데이트
        if (scrollRect.content != null)
        {
            // TextMeshProUGUI의 실제 렌더링 크기 계산
            textComponent.ForceMeshUpdate();
            Vector2 textSize = textComponent.GetRenderedValues(false);
            float textHeight = textSize.y;
            
            // ScrollRect viewport 크기 확인
            RectTransform viewportRect = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();
            float viewportHeight = viewportRect.rect.height;
            
            Debug.Log($"[UI] TextMeshProUGUI 렌더링 높이: {textHeight}");
            Debug.Log($"[UI] ScrollRect Viewport 높이: {viewportHeight}");
            
            // Content 크기 직접 설정 (텍스트 높이 + 여백)
            RectTransform contentRect = scrollRect.content;
            Vector2 sizeDelta = contentRect.sizeDelta;
            
            // 텍스트가 viewport보다 클 때만 스크롤 가능하도록 설정
            float newHeight = Mathf.Max(textHeight + 40f, viewportHeight + 1f);
            sizeDelta.y = newHeight;
            contentRect.sizeDelta = sizeDelta;
            
            Debug.Log($"[UI] Content 최종 크기 설정: {newHeight} (스크롤 가능: {newHeight > viewportHeight})");
            
            // TextMeshProUGUI 위치 조정 (Content 상단에 고정)
            RectTransform textRect = textComponent.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 1);
            textRect.anchoredPosition = new Vector2(0, 0);
            textRect.sizeDelta = new Vector2(0, textHeight);
        }
        
        // 레이아웃 강제 재구축
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        
        // 한 프레임 더 대기
        yield return null;
        
        // 스크롤 위치 설정
        scrollRect.verticalNormalizedPosition = toTop ? 1f : 0f;
        
        Debug.Log($"[UI] TextMeshProUGUI 스크롤 업데이트 완료: {(toTop ? "상단" : "하단")}");
    }

    private System.Collections.IEnumerator UpdateScrollPosition(ScrollRect scrollRect, bool toTop)
    {
        if (scrollRect == null) yield break;
        
        // UI 업데이트를 위해 두 프레임 대기 (레이아웃 재계산 시간 확보)
        yield return null;
        yield return null;
        
        // Canvas 업데이트
        Canvas.ForceUpdateCanvases();
        
        // 레이아웃 강제 업데이트
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        
        // 스크롤 위치 설정
        scrollRect.verticalNormalizedPosition = toTop ? 1f : 0f;
        
        Debug.Log($"[UI] 스크롤 위치 업데이트 완료: {(toTop ? "상단" : "하단")}");
    }

    private void OnViewHistoryClicked()
    {
        Debug.Log("View history clicked!");
        
        // 저장된 모든 기록 불러오기
        SummaryData[] allSummaries = dataSaver.LoadAllSummaries();
        
        if (allSummaries.Length == 0)
        {
            statusText.text = "저장된 기록이 없습니다.";
            statusText.color = Color.yellow;
            return;
        }
        
        // 가장 최근 기록부터 표시
        string historyText = $"=== 저장된 상담 기록 ({allSummaries.Length}개) ===\n\n";
        
        for (int i = allSummaries.Length - 1; i >= 0; i--)
        {
            var data = allSummaries[i];
            historyText += $"[{data.timestamp}]\n";
            historyText += $"요약:\n{data.summary}\n";
            historyText += $"-------------------\n\n";
        }
        
        // 요약 텍스트 영역에 표시
        summaryText.text = historyText;
        
        // TextMeshProUGUI 크기 강제 업데이트
        StartCoroutine(ForceUpdateTextAndScroll(summaryText, summaryScrollRect, true));
        
        statusText.text = $"{allSummaries.Length}개의 기록을 불러왔습니다.";
        statusText.color = Color.green;
        
        // 저장 경로도 표시
        Debug.Log($"저장 경로: {dataSaver.GetSaveDirectory()}");
    }
    
    private void UpdateInstructionText()
    {
        if (instructionText != null && schoolData != null)
        {
            instructionText.text = $"{schoolData.teacherInfo.schoolName} {schoolData.teacherInfo.grade} {schoolData.teacherInfo.className}\n" +
                                  $"담임: {schoolData.teacherInfo.teacherName} 선생님\n\n" +
                                  "학생 번호를 말하고 상담을 시작하세요";
        }
    }
    
    private void DetectStudentNumber(string text)
    {
        detectedStudentNumber = -1;
        
        if (schoolData == null || schoolData.students == null) return;
        
        // 텍스트에서 숫자 번호 찾기
        string[] words = text.Split(' ', ',', '.', '\n');
        foreach (string word in words)
        {
            // "1번", "1", "일번" 등의 패턴 확인
            if (word.Contains("번"))
            {
                string numberPart = word.Replace("번", "");
                if (int.TryParse(numberPart, out int number))
                {
                    if (number > 0 && number <= schoolData.students.Count)
                    {
                        detectedStudentNumber = number;
                        break;
                    }
                }
            }
            // 숫자만 있는 경우도 체크
            else if (int.TryParse(word, out int number))
            {
                if (number > 0 && number <= schoolData.students.Count)
                {
                    detectedStudentNumber = number;
                    break;
                }
            }
        }
        
        if (detectedStudentNumber > 0)
        {
            var student = schoolData.students.Find(s => s.number == detectedStudentNumber);
            if (student != null)
            {
                statusText.text = $"{student.number}번 {student.name} 학생 감지됨";
                statusText.color = Color.cyan;
            }
        }
    }
    
    private void OnSettingsClicked()
    {
        // 설정 씬으로 이동 (추후 구현)
        Debug.Log("Settings clicked - To be implemented");
    }
    
    private void OnRecordsClicked()
    {
        // 기록 조회 씬으로 이동
        SceneManager.LoadScene("RecordsViewScene");
    }
    
    private void OnDashboardClicked()
    {
        // 교사 대시보드 씬으로 이동
        if (UserManager.IsTeacherMode())
        {
            SceneManager.LoadScene("TeacherDashboardScene");
        }
        else
        {
            Debug.LogWarning("교사 모드가 아닙니다.");
        }
    }
}
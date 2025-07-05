using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class TranscriptionResponse
{
    public string text;
}

[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class ChatRequest
{
    public string model;
    public ChatMessage[] messages;
    public float temperature;
    public int max_tokens;
}

[System.Serializable]
public class ChatChoice
{
    public ChatMessage message;
}

[System.Serializable]
public class ChatResponse
{
    public ChatChoice[] choices;
}

public class OpenAIClient : MonoBehaviour
{
    [Header("OpenAI Settings")]
    [SerializeField] private string apiKey = ""; // 런타임에서 설정됨
    
    private const string WHISPER_URL = "https://api.openai.com/v1/audio/transcriptions";
    private const string CHAT_URL = "https://api.openai.com/v1/chat/completions";

    public event System.Action<string> OnTranscriptionComplete;
    public event System.Action<string> OnSummaryComplete;
    public event System.Action<string> OnError;

    public void TranscribeAudio(byte[] audioData)
    {
        StartCoroutine(TranscribeAudioCoroutine(audioData));
    }

    public void SummarizeText(string text)
    {
        StartCoroutine(SummarizeTextCoroutine(text));
    }

    private IEnumerator TranscribeAudioCoroutine(byte[] audioData)
    {
        if (!ValidateApiKey()) yield break;
        
        float startTime = Time.time;
        Debug.Log($"[Transcription] 시작 - 파일 크기: {audioData.Length / 1024}KB");
        
        // Create form data
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
        form.AddField("model", "whisper-1");
        form.AddField("language", "ko"); // 한국어로 고정

        using (UnityWebRequest request = UnityWebRequest.Post(WHISPER_URL, form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();
            
            Debug.Log($"[Transcription] 완료 - 소요시간: {Time.time - startTime:F1}초");

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    TranscriptionResponse response = JsonUtility.FromJson<TranscriptionResponse>(request.downloadHandler.text);
                    OnTranscriptionComplete?.Invoke(response.text);
                }
                catch (Exception e)
                {
                    OnError?.Invoke($"Failed to parse transcription: {e.Message}");
                }
            }
            else
            {
                OnError?.Invoke($"Transcription failed: {request.error}");
            }
        }
    }

    private IEnumerator SummarizeTextCoroutine(string text)
    {
        if (!ValidateApiKey()) yield break;
        
        float startTime = Time.time;
        Debug.Log($"[Summary] 시작 - 입력 텍스트 길이: {text.Length}자");
        Debug.Log($"[Summary] 입력 텍스트 미리보기: {text.Substring(0, Mathf.Min(100, text.Length))}...");
        
        ChatRequest chatRequest = new ChatRequest
        {
            model = "gpt-4o-mini", // 더 빠른 모델로 변경
            messages = new ChatMessage[]
            {
                new ChatMessage
                {
                    role = "system",
                    content = @"다음 상담 대화를 교사의 상담 기록으로 정리해주세요.

**중요: 특수문자 【】〔〕〈〉등은 사용하지 말고, 일반 괄호 ()나 - 등만 사용하세요**

[상담 기록 작성 가이드]
1. 상담 개요: 일시, 장소, 상담 이유를 간단히
2. 상담 내용: 실제 대화 내용을 시간 순서대로 정리
3. 교육적 지도 사항: 교사가 제공한 조언과 지도 내용 강조
4. 후속 조치: 앞으로의 계획이나 추가 상담 필요성
5. 교사 소견: 학생에 대한 관찰과 교육적 평가

**핵심 포인트: 교사가 해당 사안을 교육적으로 의미있게 지도했음을 기록에 명확히 드러내주세요**

분량: 입력 텍스트 길이에 비례하여 적절히 조절

---
[학부모 안내문]
오늘 자녀와 상담을 진행했습니다.
(주요 내용과 교육적 지도 사항을 2-3문장으로 요약)
가정에서도 관심과 격려 부탁드립니다."
                },
                new ChatMessage
                {
                    role = "user",
                    content = $"다음 상담 내용을 요약해주세요:\n\n{text}"
                }
            },
            temperature = 0.3f,
            max_tokens = 3000 // 더 충분한 길이로 증가
        };

        string json = JsonUtility.ToJson(chatRequest);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(CHAT_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();
            
            Debug.Log($"[Summary] 완료 - 소요시간: {Time.time - startTime:F1}초");
            Debug.Log($"[Summary] 응답 상태: {request.result}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log($"[Summary] 응답 내용: {request.downloadHandler.text}");
                    ChatResponse response = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                    if (response.choices != null && response.choices.Length > 0)
                    {
                        string summary = response.choices[0].message.content;
                        Debug.Log($"[Summary] 성공 - 요약 길이: {summary.Length}자");
                        OnSummaryComplete?.Invoke(summary);
                    }
                    else
                    {
                        Debug.LogError("[Summary] 응답에 choices가 없음");
                        OnError?.Invoke("No summary generated");
                    }
                }
                catch (Exception e)
                {
                    OnError?.Invoke($"Failed to parse summary: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[Summary] HTTP 에러: {request.error}");
                Debug.LogError($"[Summary] 응답 코드: {request.responseCode}");
                Debug.LogError($"[Summary] 응답 내용: {request.downloadHandler.text}");
                OnError?.Invoke($"Summary failed: {request.error}");
            }
        }
    }

    // Method to set API key at runtime (for security)
    public void SetApiKey(string key)
    {
        apiKey = key;
    }
    
    // Method to check if API key is set
    public bool IsApiKeySet()
    {
        return !string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_OPENAI_API_KEY";
    }
    
    // Method to validate API key before making requests
    private bool ValidateApiKey()
    {
        if (!IsApiKeySet())
        {
            OnError?.Invoke("API 키가 설정되지 않았습니다. 설정에서 OpenAI API 키를 입력해주세요.");
            return false;
        }
        return true;
    }
}
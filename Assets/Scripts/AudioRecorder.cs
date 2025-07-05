using System;
using System.IO;
using UnityEngine;

public class AudioRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    public int recordingLength = 600; // 10분으로 증가
    public int sampleRate = 8000; // 전화 품질 (가장 낮은 품질)
    public bool useMono = true; // 모노로 변경 (스테레오의 절반 크기)
    
    private AudioClip recordedClip;
    private bool isRecording = false;
    
    public bool IsRecording => isRecording;
    public event System.Action<byte[]> OnRecordingComplete;
    public event System.Action<string> OnError;

    public void StartRecording()
    {
        Debug.Log($"[AudioRecorder] StartRecording 호출됨");
        Debug.Log($"[AudioRecorder] 감지된 마이크 개수: {Microphone.devices.Length}");
        
        // 모든 마이크 디바이스 출력
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log($"[AudioRecorder] 마이크 {i}: {Microphone.devices[i]}");
        }
        
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[AudioRecorder] 마이크를 찾을 수 없습니다!");
            OnError?.Invoke("마이크를 찾을 수 없습니다! 마이크가 연결되어 있는지 확인해주세요.");
            return;
        }

        try
        {
            // 마이크 권한 요청 (Unity 2018.2 이상)
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Debug.Log("[AudioRecorder] 마이크 권한 요청 중...");
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }
            
            Debug.Log($"[AudioRecorder] 녹음 시작 시도 - 길이: {recordingLength}초, 샘플레이트: {sampleRate}Hz");
            
            // 모노 녹음으로 크기 50% 감소
            int channels = useMono ? 1 : 2;
            recordedClip = Microphone.Start(null, false, recordingLength, sampleRate);
            
            if (recordedClip == null)
            {
                Debug.LogError("[AudioRecorder] 녹음 클립 생성 실패!");
                OnError?.Invoke("녹음을 시작할 수 없습니다!");
                return;
            }
            
            isRecording = true;
            Debug.Log($"[AudioRecorder] 녹음 시작 성공! - Sample Rate: {sampleRate}Hz, Mono: {useMono}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AudioRecorder] 녹음 시작 실패: {e.Message}");
            OnError?.Invoke($"녹음 시작 실패: {e.Message}");
        }
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(null);
        isRecording = false;
        
        // 실제 녹음된 부분만 추출
        int actualPosition = Microphone.GetPosition(null);
        if (actualPosition == 0) 
        {
            // 수동으로 중지한 경우 - 전체 길이 사용하지 말고 실제 녹음 시간만 사용
            actualPosition = recordedClip.samples;
        }
        
        Debug.Log($"Microphone position: {actualPosition}, Total samples: {recordedClip.samples}");
        
        AudioClip trimmedClip = AudioClip.Create("trimmed", actualPosition, recordedClip.channels, recordedClip.frequency, false);
        float[] trimmedSamples = new float[actualPosition];
        recordedClip.GetData(trimmedSamples, 0);
        trimmedClip.SetData(trimmedSamples, 0);
        
        // Convert to WAV bytes
        byte[] audioData = ConvertToWAV(trimmedClip);
        
        // 실제 녹음 시간 계산
        float recordingTime = actualPosition / (float)sampleRate;
        Debug.Log($"Recording stopped - Duration: {recordingTime:F1}s, Size: {audioData.Length / 1024}KB, Quality: {sampleRate}Hz {(useMono ? "Mono" : "Stereo")}, Actual samples: {actualPosition}");
        
        OnRecordingComplete?.Invoke(audioData);
    }

    private byte[] ConvertToWAV(AudioClip clip)
    {
        // 전체 샘플 가져오기 (다운샘플링 제거)
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        // 16-bit PCM으로 변환 (OpenAI가 요구하는 형식)
        byte[] pcmData = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short value = (short)(samples[i] * 32767);
            byte[] bytes = BitConverter.GetBytes(value);
            pcmData[i * 2] = bytes[0];
            pcmData[i * 2 + 1] = bytes[1];
        }

        // Create WAV header (16비트 모노)
        byte[] header = CreateWAVHeader(clip.frequency, 1, pcmData.Length, 16);
        
        // Combine header and data
        byte[] wavFile = new byte[header.Length + pcmData.Length];
        Array.Copy(header, 0, wavFile, 0, header.Length);
        Array.Copy(pcmData, 0, wavFile, header.Length, pcmData.Length);

        return wavFile;
    }

    private byte[] CreateWAVHeader(int sampleRate, int channels, int dataLength, int bitsPerSample)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            int bytesPerSample = bitsPerSample / 8;
            
            // RIFF header
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + dataLength);
            writer.Write("WAVE".ToCharArray());
            
            // Format chunk
            writer.Write("fmt ".ToCharArray());
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bytesPerSample);
            writer.Write((short)(channels * bytesPerSample));
            writer.Write((short)bitsPerSample);
            
            // Data chunk
            writer.Write("data".ToCharArray());
            writer.Write(dataLength);
            
            return stream.ToArray();
        }
    }
}
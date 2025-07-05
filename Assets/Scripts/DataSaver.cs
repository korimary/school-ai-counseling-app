using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SummaryData
{
    public string timestamp;
    public string transcription;
    public string summary;
    public int audioLength;
    public int studentNumber;
    public string studentName;

    public SummaryData(string transcription, string summary, int studentNumber = -1, string studentName = "")
    {
        this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.transcription = transcription;
        this.summary = summary;
        this.audioLength = transcription.Length; // Simple estimate
        this.studentNumber = studentNumber;
        this.studentName = studentName;
    }
}

public class DataSaver : MonoBehaviour
{
    [Header("Save Settings")]
    public bool saveToFile = true;
    public bool saveToPlayerPrefs = true;
    
    private string saveDirectory;
    private const string SAVE_KEY_PREFIX = "VoiceSummary_";

    public event System.Action OnSaveComplete;
    public event System.Action<string> OnError;

    void Start()
    {
        // Create save directory
        saveDirectory = Path.Combine(Application.persistentDataPath, "VoiceSummaries");
        
        if (saveToFile && !Directory.Exists(saveDirectory))
        {
            try
            {
                Directory.CreateDirectory(saveDirectory);
                Debug.Log($"Save directory created: {saveDirectory}");
            }
            catch (Exception e)
            {
                OnError?.Invoke($"Failed to create save directory: {e.Message}");
            }
        }
    }

    public void SaveSummary(string transcription, string summary, int studentNumber = -1, string studentName = "")
    {
        if (string.IsNullOrEmpty(transcription) || string.IsNullOrEmpty(summary))
        {
            OnError?.Invoke("Cannot save empty transcription or summary");
            return;
        }

        SummaryData data = new SummaryData(transcription, summary, studentNumber, studentName);
        
        bool success = true;

        // Save to file
        if (saveToFile)
        {
            success &= SaveToFile(data);
        }

        // Save to PlayerPrefs
        if (saveToPlayerPrefs)
        {
            success &= SaveToPlayerPrefs(data);
        }

        if (success)
        {
            OnSaveComplete?.Invoke();
            Debug.Log("Summary saved successfully");
        }
    }

    private bool SaveToFile(SummaryData data)
    {
        try
        {
            string filename;
            if (data.studentNumber > 0)
            {
                filename = $"Student{data.studentNumber:D2}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            }
            else
            {
                filename = $"Summary_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            }
            string filepath = Path.Combine(saveDirectory, filename);
            
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filepath, json);
            
            Debug.Log($"Saved to file: {filepath}");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Failed to save to file: {e.Message}");
            return false;
        }
    }

    private bool SaveToPlayerPrefs(SummaryData data)
    {
        try
        {
            string key = SAVE_KEY_PREFIX + DateTime.Now.Ticks;
            string json = JsonUtility.ToJson(data);
            
            PlayerPrefs.SetString(key, json);
            
            // Also save a list of all keys for retrieval
            string allKeysString = PlayerPrefs.GetString("VoiceSummary_AllKeys", "");
            allKeysString += key + ";";
            PlayerPrefs.SetString("VoiceSummary_AllKeys", allKeysString);
            
            PlayerPrefs.Save();
            
            Debug.Log($"Saved to PlayerPrefs with key: {key}");
            return true;
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Failed to save to PlayerPrefs: {e.Message}");
            return false;
        }
    }

    // Method to retrieve all saved summaries
    public SummaryData[] LoadAllSummaries(int studentNumber = -1)
    {
        var summaries = new System.Collections.Generic.List<SummaryData>();

        // Load from PlayerPrefs
        if (saveToPlayerPrefs)
        {
            LoadFromPlayerPrefs(summaries);
        }

        // Load from files
        if (saveToFile)
        {
            LoadFromFiles(summaries);
        }

        // Filter by student number if specified
        if (studentNumber > 0)
        {
            var filteredSummaries = new System.Collections.Generic.List<SummaryData>();
            foreach (var summary in summaries)
            {
                if (summary.studentNumber == studentNumber)
                {
                    filteredSummaries.Add(summary);
                }
            }
            return filteredSummaries.ToArray();
        }

        return summaries.ToArray();
    }

    private void LoadFromPlayerPrefs(System.Collections.Generic.List<SummaryData> summaries)
    {
        try
        {
            string allKeysString = PlayerPrefs.GetString("VoiceSummary_AllKeys", "");
            if (string.IsNullOrEmpty(allKeysString)) return;

            string[] keys = allKeysString.Split(';');
            
            foreach (string key in keys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                
                string json = PlayerPrefs.GetString(key, "");
                if (!string.IsNullOrEmpty(json))
                {
                    SummaryData data = JsonUtility.FromJson<SummaryData>(json);
                    if (data != null)
                    {
                        summaries.Add(data);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load from PlayerPrefs: {e.Message}");
        }
    }

    private void LoadFromFiles(System.Collections.Generic.List<SummaryData> summaries)
    {
        try
        {
            if (!Directory.Exists(saveDirectory)) return;

            string[] files = Directory.GetFiles(saveDirectory, "*.json");
            
            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                SummaryData data = JsonUtility.FromJson<SummaryData>(json);
                if (data != null)
                {
                    summaries.Add(data);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load from files: {e.Message}");
        }
    }

    // Method to clear all saved data
    public void ClearAllData()
    {
        try
        {
            // Clear PlayerPrefs
            if (saveToPlayerPrefs)
            {
                string allKeysString = PlayerPrefs.GetString("VoiceSummary_AllKeys", "");
                string[] keys = allKeysString.Split(';');
                
                foreach (string key in keys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        PlayerPrefs.DeleteKey(key);
                    }
                }
                
                PlayerPrefs.DeleteKey("VoiceSummary_AllKeys");
                PlayerPrefs.Save();
            }

            // Clear files
            if (saveToFile && Directory.Exists(saveDirectory))
            {
                string[] files = Directory.GetFiles(saveDirectory);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }

            Debug.Log("All data cleared successfully");
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Failed to clear data: {e.Message}");
        }
    }

    // Get save directory path for debugging
    public string GetSaveDirectory()
    {
        return saveDirectory;
    }

    // Get total number of saved summaries
    public int GetSavedSummaryCount()
    {
        return LoadAllSummaries().Length;
    }
}
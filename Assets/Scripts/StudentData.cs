using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StudentInfo
{
    public int number;
    public string name;
    
    public StudentInfo(int number, string name)
    {
        this.number = number;
        this.name = name;
    }
}

[Serializable]
public class TeacherInfo
{
    public string schoolName;
    public string grade;
    public string className;
    public string teacherName;
    public int studentCount;
    public string classCode;
    
    public TeacherInfo()
    {
        schoolName = "";
        grade = "";
        className = "";
        teacherName = "";
        studentCount = 0;
        classCode = "";
    }
}

[Serializable]
public class SchoolData
{
    public TeacherInfo teacherInfo;
    public List<StudentInfo> students;
    
    public SchoolData()
    {
        teacherInfo = new TeacherInfo();
        students = new List<StudentInfo>();
    }
}

public static class StudentDataManager
{
    private const string SCHOOL_DATA_KEY = "SchoolData";
    
    public static void SaveSchoolData(SchoolData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SCHOOL_DATA_KEY, json);
        PlayerPrefs.Save();
    }
    
    public static SchoolData LoadSchoolData()
    {
        if (PlayerPrefs.HasKey(SCHOOL_DATA_KEY))
        {
            string json = PlayerPrefs.GetString(SCHOOL_DATA_KEY);
            return JsonUtility.FromJson<SchoolData>(json);
        }
        return null;
    }
    
    public static bool HasSchoolData()
    {
        return PlayerPrefs.HasKey(SCHOOL_DATA_KEY);
    }
    
    public static void ClearSchoolData()
    {
        PlayerPrefs.DeleteKey(SCHOOL_DATA_KEY);
        PlayerPrefs.Save();
    }
}
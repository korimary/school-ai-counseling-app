using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClassCodeData
{
    public string code;
    public string className;
    public string teacherName;
    public string schoolName;
    public string grade;
    public DateTime createdAt;
    public List<StudentInfo> students;
    
    public ClassCodeData()
    {
        students = new List<StudentInfo>();
        createdAt = DateTime.Now;
    }
}

public static class ClassCodeManager
{
    private const string CLASS_CODE_KEY = "ClassCode";
    private const string CLASS_CODE_DATA_KEY = "ClassCodeData";
    
    // 클래스 코드 생성 (학급명-1234 형식)
    public static string GenerateClassCode(string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogError("클래스명이 비어있습니다.");
            return null;
        }
        
        // 4자리 랜덤 숫자 생성
        System.Random random = new System.Random();
        int randomNumber = random.Next(1000, 9999);
        
        // 클래스 코드 형식: 학급명-1234
        string classCode = $"{className}-{randomNumber}";
        
        // 중복 확인 (이미 존재하는 코드면 다시 생성)
        int attempts = 0;
        while (ClassCodeExists(classCode) && attempts < 100)
        {
            randomNumber = random.Next(1000, 9999);
            classCode = $"{className}-{randomNumber}";
            attempts++;
        }
        
        if (attempts >= 100)
        {
            Debug.LogError("클래스 코드 생성에 실패했습니다. 너무 많은 시도.");
            return null;
        }
        
        Debug.Log($"클래스 코드 생성됨: {classCode}");
        return classCode;
    }
    
    // 클래스 코드 유효성 검사
    public static bool ValidateClassCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;
        
        // 기본 형식 확인: 학급명-1234
        if (!code.Contains("-"))
            return false;
        
        string[] parts = code.Split('-');
        if (parts.Length != 2)
            return false;
        
        // 숫자 부분 확인
        if (!int.TryParse(parts[1], out int number))
            return false;
        
        // 4자리 숫자인지 확인
        if (number < 1000 || number > 9999)
            return false;
        
        return true;
    }
    
    // 현재 클래스 코드 가져오기
    public static string GetCurrentClassCode()
    {
        return PlayerPrefs.GetString(CLASS_CODE_KEY, "");
    }

    // 클래스 코드 존재 여부 확인
    public static bool ClassCodeExists(string code)
    {
        if (!ValidateClassCode(code))
            return false;
        
        string currentCode = GetCurrentClassCode();
        return !string.IsNullOrEmpty(currentCode) && currentCode == code;
    }
    
    // 현재 클래스 코드 설정
    public static void SetCurrentClassCode(string code, ClassCodeData data)
    {
        if (!ValidateClassCode(code))
        {
            Debug.LogError($"유효하지 않은 클래스 코드: {code}");
            return;
        }
        
        PlayerPrefs.SetString(CLASS_CODE_KEY, code);
        
        // 클래스 데이터 저장
        if (data != null)
        {
            data.code = code;
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(CLASS_CODE_DATA_KEY, json);
        }
        
        PlayerPrefs.Save();
        Debug.Log($"클래스 코드 설정됨: {code}");
    }
    
    // 클래스 코드 데이터 가져오기
    public static ClassCodeData GetClassCodeData()
    {
        if (!PlayerPrefs.HasKey(CLASS_CODE_DATA_KEY))
            return null;
        
        string json = PlayerPrefs.GetString(CLASS_CODE_DATA_KEY);
        try
        {
            return JsonUtility.FromJson<ClassCodeData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"클래스 코드 데이터 로드 실패: {e.Message}");
            return null;
        }
    }
    
    // 학교 데이터를 클래스 코드 데이터로 변환
    public static ClassCodeData CreateClassCodeDataFromSchoolData(SchoolData schoolData, string classCode)
    {
        if (schoolData == null)
            return null;
        
        ClassCodeData data = new ClassCodeData();
        data.code = classCode;
        data.className = schoolData.teacherInfo.className;
        data.teacherName = schoolData.teacherInfo.teacherName;
        data.schoolName = schoolData.teacherInfo.schoolName;
        data.grade = schoolData.teacherInfo.grade;
        data.students = new List<StudentInfo>(schoolData.students);
        
        return data;
    }
    
    // 클래스 코드로 학생 정보 가져오기
    public static StudentInfo GetStudentInfo(string classCode, int studentNumber)
    {
        if (!ClassCodeExists(classCode))
        {
            Debug.LogWarning($"존재하지 않는 클래스 코드: {classCode}");
            return null;
        }
        
        ClassCodeData data = GetClassCodeData();
        if (data == null || data.students == null)
        {
            Debug.LogWarning("클래스 데이터가 없습니다.");
            return null;
        }
        
        foreach (var student in data.students)
        {
            if (student.number == studentNumber)
            {
                return student;
            }
        }
        
        Debug.LogWarning($"클래스 {classCode}에서 {studentNumber}번 학생을 찾을 수 없습니다.");
        return null;
    }
    
    // 클래스 코드로 학생 로그인 검증
    public static bool ValidateStudentLogin(string classCode, int studentNumber, out StudentInfo studentInfo)
    {
        studentInfo = null;
        
        if (!ValidateClassCode(classCode))
        {
            Debug.LogWarning($"유효하지 않은 클래스 코드 형식: {classCode}");
            return false;
        }
        
        if (!ClassCodeExists(classCode))
        {
            Debug.LogWarning($"존재하지 않는 클래스 코드: {classCode}");
            return false;
        }
        
        studentInfo = GetStudentInfo(classCode, studentNumber);
        return studentInfo != null;
    }
    
    // 클래스 코드에 학생 추가
    public static bool AddStudentToClass(string classCode, StudentInfo student)
    {
        if (!ClassCodeExists(classCode))
        {
            Debug.LogWarning($"존재하지 않는 클래스 코드: {classCode}");
            return false;
        }
        
        ClassCodeData data = GetClassCodeData();
        if (data == null)
        {
            Debug.LogError("클래스 데이터가 없습니다.");
            return false;
        }
        
        // 중복 학생 번호 확인
        foreach (var existingStudent in data.students)
        {
            if (existingStudent.number == student.number)
            {
                Debug.LogWarning($"이미 존재하는 학생 번호: {student.number}");
                return false;
            }
        }
        
        data.students.Add(student);
        
        // 데이터 저장
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(CLASS_CODE_DATA_KEY, json);
        PlayerPrefs.Save();
        
        Debug.Log($"학생 추가됨: {student.number}번 {student.name}");
        return true;
    }
    
    // 클래스 코드에서 학생 제거
    public static bool RemoveStudentFromClass(string classCode, int studentNumber)
    {
        if (!ClassCodeExists(classCode))
        {
            Debug.LogWarning($"존재하지 않는 클래스 코드: {classCode}");
            return false;
        }
        
        ClassCodeData data = GetClassCodeData();
        if (data == null)
        {
            Debug.LogError("클래스 데이터가 없습니다.");
            return false;
        }
        
        // 학생 찾기 및 제거
        for (int i = 0; i < data.students.Count; i++)
        {
            if (data.students[i].number == studentNumber)
            {
                string studentName = data.students[i].name;
                data.students.RemoveAt(i);
                
                // 데이터 저장
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(CLASS_CODE_DATA_KEY, json);
                PlayerPrefs.Save();
                
                Debug.Log($"학생 제거됨: {studentNumber}번 {studentName}");
                return true;
            }
        }
        
        Debug.LogWarning($"제거할 학생을 찾을 수 없습니다: {studentNumber}번");
        return false;
    }
    
    // 클래스 코드 삭제
    public static void ClearClassCode()
    {
        PlayerPrefs.DeleteKey(CLASS_CODE_KEY);
        PlayerPrefs.DeleteKey(CLASS_CODE_DATA_KEY);
        PlayerPrefs.Save();
        Debug.Log("클래스 코드가 삭제되었습니다.");
    }
    
    // 클래스 코드 정보 문자열로 반환 (디버깅용)
    public static string GetClassCodeInfoString()
    {
        string currentCode = GetCurrentClassCode();
        if (string.IsNullOrEmpty(currentCode))
        {
            return "클래스 코드 없음";
        }
        
        ClassCodeData data = GetClassCodeData();
        if (data == null)
        {
            return $"클래스 코드: {currentCode} (데이터 없음)";
        }
        
        return $"클래스 코드: {currentCode} ({data.schoolName} {data.grade} {data.className}, 학생 {data.students.Count}명)";
    }
}
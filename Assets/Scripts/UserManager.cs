using System;
using UnityEngine;

public static class UserManager
{
    public enum UserMode
    {
        None,
        Teacher,
        Student
    }

    private const string USER_MODE_KEY = "UserMode";
    private const string STUDENT_ID_KEY = "StudentID";
    private const string STUDENT_NAME_KEY = "StudentName";

    // 현재 사용자 모드
    private static UserMode currentMode = UserMode.None;

    // 학생 정보 (학생 모드일 때만 사용)
    private static int studentID = -1;
    private static string studentName = "";

    // 초기화
    static UserManager()
    {
        LoadUserData();
    }

    // 사용자 모드 설정
    public static void SetUserMode(UserMode mode)
    {
        currentMode = mode;
        PlayerPrefs.SetString(USER_MODE_KEY, mode.ToString());
        PlayerPrefs.Save();
        
        Debug.Log($"사용자 모드 설정됨: {mode}");
        
        // 교사 모드로 변경 시 학생 정보 초기화
        if (mode == UserMode.Teacher)
        {
            ClearStudentInfo();
        }
    }

    // 현재 사용자 모드 가져오기
    public static UserMode GetUserMode()
    {
        return currentMode;
    }

    // 교사 모드인지 확인
    public static bool IsTeacherMode()
    {
        return currentMode == UserMode.Teacher;
    }

    // 학생 모드인지 확인
    public static bool IsStudentMode()
    {
        return currentMode == UserMode.Student;
    }

    // 역할이 선택되어 있는지 확인
    public static bool HasSelectedRole()
    {
        return currentMode != UserMode.None;
    }

    // 학생 정보 설정 (학생 모드에서만 사용)
    public static void SetStudentInfo(int id, string name)
    {
        if (currentMode != UserMode.Student)
        {
            Debug.LogWarning("학생 모드가 아닌 상태에서 학생 정보를 설정하려고 했습니다.");
            return;
        }

        studentID = id;
        studentName = name;
        
        PlayerPrefs.SetInt(STUDENT_ID_KEY, id);
        PlayerPrefs.SetString(STUDENT_NAME_KEY, name);
        PlayerPrefs.Save();
        
        Debug.Log($"학생 정보 설정됨: {id}번 {name}");
    }

    // 현재 학생 ID 가져오기
    public static int GetStudentID()
    {
        return studentID;
    }

    // 현재 학생 이름 가져오기
    public static string GetStudentName()
    {
        return studentName;
    }

    // 학생 정보가 설정되어 있는지 확인
    public static bool HasStudentInfo()
    {
        return currentMode == UserMode.Student && studentID > 0 && !string.IsNullOrEmpty(studentName);
    }

    // 학생 정보 초기화
    private static void ClearStudentInfo()
    {
        studentID = -1;
        studentName = "";
        
        PlayerPrefs.DeleteKey(STUDENT_ID_KEY);
        PlayerPrefs.DeleteKey(STUDENT_NAME_KEY);
        PlayerPrefs.Save();
    }

    // 저장된 사용자 데이터 불러오기
    private static void LoadUserData()
    {
        // 사용자 모드 불러오기
        string savedMode = PlayerPrefs.GetString(USER_MODE_KEY, UserMode.None.ToString());
        if (System.Enum.TryParse<UserMode>(savedMode, out UserMode mode))
        {
            currentMode = mode;
        }

        // 학생 정보 불러오기 (학생 모드일 때만)
        if (currentMode == UserMode.Student)
        {
            studentID = PlayerPrefs.GetInt(STUDENT_ID_KEY, -1);
            studentName = PlayerPrefs.GetString(STUDENT_NAME_KEY, "");
        }
        
        Debug.Log($"사용자 데이터 로드됨 - 모드: {currentMode}, 학생ID: {studentID}, 학생이름: {studentName}");
    }

    // 모든 사용자 데이터 초기화
    public static void ResetUserData()
    {
        currentMode = UserMode.None;
        studentID = -1;
        studentName = "";
        
        PlayerPrefs.DeleteKey(USER_MODE_KEY);
        PlayerPrefs.DeleteKey(STUDENT_ID_KEY);
        PlayerPrefs.DeleteKey(STUDENT_NAME_KEY);
        PlayerPrefs.Save();
        
        Debug.Log("모든 사용자 데이터가 초기화되었습니다.");
    }

    // 현재 사용자 정보를 문자열로 반환 (디버깅용)
    public static string GetUserInfoString()
    {
        if (currentMode == UserMode.Teacher)
        {
            return "교사 모드";
        }
        else if (currentMode == UserMode.Student && HasStudentInfo())
        {
            return $"학생 모드 - {studentID}번 {studentName}";
        }
        else if (currentMode == UserMode.Student)
        {
            return "학생 모드 - 정보 없음";
        }
        else
        {
            return "모드 선택 안됨";
        }
    }
}
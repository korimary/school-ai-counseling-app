using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{
    void Start()
    {
        // 역할 선택이 안 되어 있으면 메인 메뉴로
        if (!UserManager.HasSelectedRole())
        {
            SceneManager.LoadScene("MainMenuScene");
            return;
        }

        // 교사 모드인 경우 기존 로직 유지
        if (UserManager.IsTeacherMode())
        {
            // 학교 데이터가 있는지 확인
            if (StudentDataManager.HasSchoolData())
            {
                // 데이터가 있으면 메인 씬으로
                SceneManager.LoadScene("SampleScene");
            }
            else
            {
                // 데이터가 없으면 초기 설정 씬으로
                SceneManager.LoadScene("InitialSetupScene");
            }
        }
        // 학생 모드인 경우 학생 로그인 씬으로
        else if (UserManager.IsStudentMode())
        {
            SceneManager.LoadScene("StudentLoginScene");
        }
    }
}
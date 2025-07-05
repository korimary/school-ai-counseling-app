# 🏫 학교 AI 상담 앱 (School AI Counseling App)

학생들의 감정을 추적하고 상담 기록을 AI로 요약하는 Unity 기반 교육 앱입니다.

## 📱 주요 기능

### 👩‍🏫 교사 모드
- 🎙️ 음성 녹음으로 상담 내용 기록
- 🤖 AI가 자동으로 상담 내용 요약
- 📊 학생별 상담 기록 관리
- 📈 학급 전체 감정 통계 확인

### 👦 학생 모드
- 😊 하루 시작/끝 감정 체크인
- 📝 감정 일기 작성
- 🏆 성장 배지 획득
- 📊 나의 감정 변화 그래프 확인

## 🛠️ 기술 스택

- **개발 환경**: Unity 2022.3.62f1
- **프로그래밍 언어**: C#
- **AI API**: OpenAI (Whisper, GPT-4)
- **데이터 저장**: JSON + PlayerPrefs
- **UI**: Unity UI + TextMeshPro

## 🚀 시작하기

### 필요한 것들
1. Unity Hub 설치
2. Unity 2022.3.62f1 버전
3. OpenAI API 키

### 설치 방법
1. 이 저장소를 클론합니다
   ```bash
   git clone https://github.com/[your-username]/school-ai-counseling-app.git
   ```
2. Unity Hub에서 프로젝트 열기
3. `SampleScene` 씬 로드
4. Play 버튼을 눌러 실행

### API 키 설정
1. 앱 실행 후 설정 메뉴에서 OpenAI API 키 입력
2. 또는 `VoiceSummarizerUI.cs`에서 직접 설정

## 📂 프로젝트 구조

```
Assets/
├── Scenes/          # Unity 씬 파일들
├── Scripts/         # C# 스크립트들
│   ├── UI/         # UI 관련 스크립트
│   ├── Data/       # 데이터 모델
│   └── Managers/   # 매니저 클래스들
├── Prefabs/        # 재사용 가능한 오브젝트
└── Resources/      # 리소스 파일들
```

## 🎯 주요 스크립트

- `VoiceSummarizerUI.cs` - 음성 녹음 및 AI 요약
- `StudentEmotionUI.cs` - 학생 감정 체크인/아웃
- `TeacherDashboardUI.cs` - 교사 대시보드
- `EmotionManager.cs` - 감정 데이터 관리
- `UserManager.cs` - 사용자 모드 관리

## 📸 스크린샷

(나중에 스크린샷 추가 예정)

## 🤝 기여하기

버그 리포트나 기능 제안은 Issues에 남겨주세요!

## 📝 라이선스

이 프로젝트는 MIT 라이선스를 따릅니다.

## 👨‍💻 개발자

- 개발자: [Your Name]
- 이메일: [your-email@gmail.com]

## 🙏 감사의 말

이 프로젝트는 학생들의 정서적 건강을 지원하고자 만들어졌습니다.

---

⭐ 이 프로젝트가 도움이 되었다면 Star를 눌러주세요!
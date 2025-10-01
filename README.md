# Lc_auto

Windows 전용 Lightroom Classic 자동화 런처 애플리케이션

## 개요

Lc_auto는 사진 스튜디오 워크플로우를 위한 작고 가벼운 런처 앱입니다. 항상 최상위에 표시되며, 다음 기능을 제공합니다:

- **촬영 시작 자동화** (버튼 a): 고객 정보 입력 후 Lightroom Classic에서 촬영 세션 시작
- **내보내기 자동화** (버튼 b): 고객 정보 입력 후 Lightroom Classic에서 사진 내보내기
- **배경지 설치 동영상 재생** (버튼 c): 고객을 위한 안내 동영상 재생
- **사진 저장 폴더 열기** (버튼 d): Windows 탐색기로 출력 폴더 열기
- **예약 알림 팝업**: 설정된 시간에 자동으로 알림 표시

## 주요 특징

- **항상 최상위 유지**: 앱은 기본적으로 다른 창 위에 표시되며, 자동화 실행 시 Lightroom에 포커스를 양보한 후 자동 복귀
- **설정 기반 동작**: `config/config.json` 파일로 모든 동작 제어
- **핫 리로드**: 설정 파일 변경 시 앱 재시작 없이 즉시 반영
- **단일 실행 파일**: 설치 불필요, exe 파일만으로 실행 가능
- **관리자 권한 불필요**: 일반 사용자 권한으로 실행

## 시스템 요구사항

- **OS**: Windows 10 이상
- **아키텍처**: x64
- **필수 소프트웨어**: Lightroom Classic (자동화 기능 사용 시)
- **.NET**: .NET 8 Runtime (self-contained 빌드 시 불필요)

## 설치

1. 최신 릴리스에서 `Lc_auto-win-x64.zip` 다운로드
2. 원하는 위치에 압축 해제
3. `Lc_auto.exe` 실행

## 설정

앱 실행 파일과 동일한 디렉토리의 `config/config.json` 파일을 편집합니다.

### 설정 예제

```json
{
  "media": {
    "videoPath": "C:\\Media\\background_installation.mp4",
    "playerPath": ""
  },
  "automation": {
    "p1": {
      "featureId": "Lightroom.StartPhotoSession",
      "description": "촬영 시작 자동화"
    },
    "p2": {
      "featureId": "Lightroom.ExportPhotos",
      "description": "내보내기 자동화"
    },
    "timeoutSec": 30
  },
  "paths": {
    "targetFolder": "C:\\Photos\\Output"
  },
  "alerts": [
    { "time": "09:00", "message": "오전 촬영 시작", "repeat": "daily" },
    { "time": "14:00", "message": "오후 촬영 시작", "repeat": "daily" }
  ],
  "ui": {
    "alwaysOnTop": true,
    "theme": "auto"
  }
}
```

### 설정 항목 설명

- **media.videoPath**: 재생할 동영상 파일 경로 (버튼 c)
- **media.playerPath**: 사용할 동영상 플레이어 경로 (빈 문자열이면 OS 기본 플레이어 사용)
- **automation.p1/p2**: 자동화 기능 ID (Lightroom Classic UI 요소와 매핑)
- **automation.timeoutSec**: 자동화 시도당 타임아웃 (초 단위, 최대 2회 재시도)
- **paths.targetFolder**: 버튼 d로 열 폴더 경로
- **alerts**: 예약 알림 배열 (시간 HH:mm 형식, 메시지, 반복 설정)
- **ui.alwaysOnTop**: 항상 최상위 표시 여부 (항상 true 권장)
- **ui.theme**: UI 테마 ("auto", "light", "dark")

## 사용 방법

### 촬영 시작 (버튼 a)
1. 버튼 a 클릭
2. 팝업 폼에 "예약자 성함"과 "휴대폰 뒤4자리" 입력
3. "촬영 시작" 버튼 클릭
4. Lightroom Classic에서 자동화 실행 (입력값은 자동으로 붙여넣기됨)

### 내보내기 (버튼 b)
1. 버튼 b 클릭
2. 팝업 폼에 "예약자 성함"과 "휴대폰 뒤4자리" 입력
3. "내보내기 시작" 버튼 클릭
4. Lightroom Classic에서 내보내기 자동화 실행

### 동영상 재생 (버튼 c)
- 버튼 c 클릭하여 배경지 설치 안내 동영상 재생

### 폴더 열기 (버튼 d)
- 버튼 d 클릭하여 사진 저장 폴더를 Windows 탐색기로 열기

## 로그

로그 파일은 앱 실행 파일과 동일한 디렉토리의 `log/` 폴더에 저장됩니다.

- **보존 기간**: 7일
- **파일 크기 제한**: 50MB (자동 롤링)
- **로그 레벨**: INFO (정상 동작), WARN (복구 가능한 문제), ERROR (실패)

## 문제 해결

### 자동화가 실행되지 않음
- Lightroom Classic이 실행 중인지 확인
- `config.json`의 `featureId`가 올바르게 설정되었는지 확인
- 로그 파일에서 오류 메시지 확인

### 동영상이 재생되지 않음
- `media.videoPath`가 유효한 파일 경로인지 확인
- `media.playerPath`가 설정된 경우 플레이어가 존재하는지 확인
- OS 기본 동영상 플레이어가 설정되어 있는지 확인

### 폴더가 열리지 않음
- `paths.targetFolder`가 존재하는 경로인지 확인
- 해당 폴더에 대한 읽기 권한이 있는지 확인

### 앱이 최상위에 표시되지 않음
- `config.json`의 `ui.alwaysOnTop`이 `true`로 설정되었는지 확인
- 로그에서 TopMost 복원 실패 메시지 확인

## 개발

자세한 개발 문서는 `docs/` 폴더를 참조하세요:

- **PRD.md**: 제품 요구사항 정의
- **Specifications.md**: 기술 사양
- **Architecture.md**: 아키텍처 설계
- **UserStories.md**: 사용자 시나리오
- **CLAUDE.md**: AI 개발 가이드

### 빌드 방법

```bash
# 복원
dotnet restore

# 빌드
dotnet build

# 단일 파일로 게시
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### 기술 스택

- **.NET 8** (C# WPF)
- **FlaUI** (UI 자동화)
- **ModernWpf** (UI 테마)
- **Serilog** (로깅)

## 라이선스

(라이선스 정보 추가 예정)

## 기여

이슈 및 개선 제안은 GitHub Issues를 통해 제출해 주세요.

## 변경 이력

최신 변경 사항은 [Releases](https://github.com/your-repo/releases) 페이지를 참조하세요.

# 인포메이션

| 속성 | 설명 | 예시 |
|------|------|------|
| **AutomationId** | 고유 식별자 (가장 안정적) | `"FileMenu"`, `"ExportButton"` |
| **Name** | 표시되는 텍스트 | `"File"`, `"Export"`, `"확인"` |
| **ClassName** | 윈도우 클래스 이름 | `"Button"`, `"MenuItem"` |
| **ControlType** | 컨트롤 유형 | `"Button"`, `"Edit"`, `"MenuItem"` |

---


AutomationId:
Name:
ClassName:
ControlType:


---

# 촬영 매니저
---
#### 1.왼쪽 상단 '파일(F)' 클릭
AutomationId:Not Supported
Name:파일(F)
ClassName:Not Supported
ControlType:MenuItem

#### 2.'메뉴' 드롭다운 확인
AutomationId:Not Supported
Name:파일(F)
ClassName:#32768
ControlType:Menu

#### 3.'연결전송된 촬영' 클릭
AutomationId:Not Supported
Name:연결전송된 촬영
ClassName:Not Supported
ControlType:MenuItem

#### 4.'연결전송된 촬영' 드롭다운 확인
AutomationId:Not Supported
Name:파일(F)
ClassName:#32768
ControlType:Menu

#### 5.'연결전송된 촬영 시작' 클릭
AutomationId:40506
Name:연결전송된 촬영 시작...
ClassName:Not Supported
ControlType:MenuItem

#### 6.'연결전송된 촬영 설정' 윈도우 팝업 확인
AutomationId:Not Supported
Name:연결전송된 촬영 설정
ClassName:Afx:0000000140000000:0
ControlType:Window

#### 7. '세션 이름' 클릭
AutomationId:65535
Name:세션 이름:
ClassName:Edit
ControlType:Edit

#### 8. '모두 선택' -> '삭제' -> 전 이름+휴대전화뒤4자리 조합값 붙여넣기

#### 9. 숏별로 사진 나누기 체크박스 상태 확인
AutomationId:Edit
Name:숏별로 사진 나누기
ClassName:Button
ControlType:CheckBox
ToggleState: On / Off 

#### 10. 상태 On -> Off, Off -> Pass

#### 11. '사용자 정의이름' 템플릿 콤보박스 클릭
AutomationId:2134
Name:템플릿:
ClassName:ComboBox
ControlType:ComboBox

#### 12. '사용자 정의이름' 템플릿 콤보박스 드롭다운 확인
AutomationId:Not Supported
Name:컨텍스트
ClassName:#32768
ControlType:Menu

#### 13. '사용자 정의 이름 - 원본 파일 번호' 선택
AutomationId:3
Name:사용자 정의 이름 - 원본 파일 번호
ClassName:Not Supported
ControlType:MenuItem

#### 14. '사용자 정의 텍스트' 입력창 클릭
AutomationId:2136
Name:사용자 정의 텍스트:
ClassName:Edit
ControlType:Edit

#### 15. '원본' 텍스트 입력

---


#### 16. 경로 '선택' 버튼 클릭
AutomationId:65535
Name:선택...
ClassName:Button
ControlType:Button

#### 17. '폴더 선택' 윈도우 확인
AutomationId:Not Supported
Name:폴더 선택
ClassName:#32770
ControlType:Window

--

#### 18. 주소창 클릭
AutomationId:1001
Name:
ClassName:ToolbarWindow32
ControlType:ToolBar

#### 19. 'C:\dabi_shoot' 텍스트 붙여넣기

#### 20. 최종 '확인' 버튼 클릭릭
AutomationId:1
Name:확인
ClassName:Button
ControlType:Button

---

아래는 실패 케이스 2개, 2초동안 체크하기, 보이면 오류
Cas01 : There is no cable
Case02 : Connection fail

Case01 :
AutomationId:-1985744256
Name:카메라를 감지하는 중...
ClassName:Static
ControlType:Text

Case 02 :


ControlType : Pane, Name : 연결전송된 촬영
위 항목에 종속되어 있는 아래 항목 찾기

AutomationId:65535
Name:카메라가 검색되지 않음
ClassName:ComboBox
ControlType:ComboBox
LegacyIAccessible.Value = '카메라가 검색되지 않음'


---

## 셔터 세팅

AutomationId:1412051328
Name:셔터:
ClassName:Static
ControlType:Text

### 콤보박스 클릭 (위 객체와 상대 위치 기반 탐색 설정)
AutomationId:65535
Name:[참조 및 활용하지 말 것 (값이 변동하기 떄문)]
ClassName:ComboBox
ControlType:ComboBox


### 메뉴박스 확인
AutomationId:Not Supported
Name:컨텍스트
ClassName:#32768
ControlType:Menu


### 설정된 값 클릭
AutomationId:[아래 *순번자료*에 따름]
Name:[아래 config.yaml-'셔터'항목에 따름]
ClassName:Not Supported
ControlType:MenuItem

//*순번자료*
Name:4 -> AutomationId:23
Name:5 -> AutomationId:24
Name:6 -> AutomationId:25
Name:8 -> AutomationId:26
Name:10 -> AutomationId:27
Name:13 -> AutomationId:28
Name:15 -> AutomationId:29
Name:20 -> AutomationId:30
//
config.yaml 베이스 문서 : docs\configfile_format.md
//




## 조리개 세팅

AutomationId:1412053888
Name:조리개:
ClassName:Static
ControlType:Text
### 콤보박스 클릭 (위 객체와 상대 위치 기반 탐색 설정)
AutomationId:65535
Name:[참조 및 활용하지 말 것 (값이 변동하기 떄문)]
ClassName:ComboBox
ControlType:ComboBox


### 메뉴박스 확인
AutomationId:Not Supported
Name:컨텍스트
ClassName:#32768
ControlType:Menu


### 설정된 값 클릭
AutomationId:[아래 *순번자료*에 따름]
Name:[아래 config.yaml-'조리개' 항목에 따름]
ClassName:Not Supported
ControlType:MenuItem

//*순번자료*
Name:7.1 -> AutomationId:6
Name:8 -> AutomationId:7
Name:9 -> AutomationId:8
Name:10 -> AutomationId:9
Name:11 -> AutomationId:10
Name:13 -> AutomationId:11
Name:14 -> AutomationId:12
Name:26 -> AutomationId:13
//
config.yaml 베이스 문서 : docs\configfile_format.md
//


## ISO 세팅

AutomationId:1412049280
Name:ISO:
ClassName:Static
ControlType:Text
### 콤보박스 클릭 (위 객체와 상대 위치 기반 탐색 설정)
AutomationId:65535
Name:[참조 및 활용하지 말 것 (값이 변동하기 떄문)]
ClassName:ComboBox
ControlType:ComboBox


### 메뉴박스 확인
AutomationId:Not Supported
Name:컨텍스트
ClassName:#32768
ControlType:Menu


### 설정된 값 클릭
AutomationId:[아래 *순번자료*에 따름]
Name:[아래 config.yaml-'ISO' 항목에 따름]
ClassName:Not Supported
ControlType:MenuItem

//*순번자료*
Name:100 -> AutomationId:1
Name:200 -> AutomationId:2
Name:400 -> AutomationId:3
Name:800 -> AutomationId:4
Name:1600 -> AutomationId:5
//
config.yaml 베이스 문서 : docs\configfile_format.md
//


## WB 세팅

AutomationId:1412048768
Name:WB:
ClassName:Static
ControlType:Text
### 콤보박스 클릭 (위 객체와 상대 위치 기반 탐색 설정)
AutomationId:65535
Name:[참조 및 활용하지 말 것 (값이 변동하기 떄문)]
ClassName:ComboBox
ControlType:ComboBox


### 메뉴박스 확인
AutomationId:Not Supported
Name:컨텍스트
ClassName:#32768
ControlType:Menu


### 설정된 값 클릭
AutomationId:[아래 *순번자료*에 따름]
Name:[아래 config.yaml-'WB' 항목에 따름]
ClassName:Not Supported
ControlType:MenuItem

//*순번자료*
Name:자동 -> AutomationId:1
Name:일광 -> AutomationId:2
Name:그늘 -> AutomationId:3
Name:흐림 -> AutomationId:4
Name:텅스텐 -> AutomationId:5
Name:형광 -> AutomationId:6
Name:플래시 -> AutomationId:7
Name:수동 -> AutomationId:8
//
config.yaml 베이스 문서 : docs\configfile_format.md
//


--------------------------------------------------


# 내보내기 매니저

## 구조 설명

아래는 데이터 세트입니다.
데이터 세트 윗 줄에 동작 설명합니다.
[] 형식은 명령에 일치하게 코딩하세요.

"
AutomationId:
Name:
ClassName:
ControlType:
"

토글 상태 확인 : 'ToggleState' On or Off 확인


## ---<내보내기 클릭>---

#전체선택 (ctrl+A) 누르기 위한 준비
AutomationId:100
Name:(D2D Bridge View)
ClassName:AfxWnd140u
ControlType:Pane

[키보드 컨트롤+A 누르는 코드 이 스텝에 추가]

#파일 클릭
AutomationId:Not Supported
Name:파일(F)
ClassName:Not Supported
ControlType:MenuItem

#내보내기 클릭
AutomationId:40513
Name:내보내기(E)...
ClassName:Not Supported
ControlType:MenuItem

## ---<내보내기 상세 설정>---

### ---<내보내기 위치>---

#아래 객체로 새로운 윈도우 로드됐는지 확인
AutomationId:-1574358304
Name:내보내기 위치
ClassName:Static
ControlType:Static

#아래 객체 보이는지 확인하여 보이면 현재 스텝 바이패스
AutomationId:1817901184
Name:내보낼 위치:
ClassName:Static
ControlType:Text

#안보이면 아래 객체 클릭
AutomationId:-1574358304
Name:내보내기 위치
ClassName:Static
ControlType:Text

#내보낼 위치 메뉴 클릭
AutomationId:65535
Name:내보낼 위치:
ClassName:ComboBox
ControlType:ComboBox

#내보낼 위치 메뉴 선택
AutomationId:1
Name:특정 폴더
ClassName:Not Supported
ControlType:MenuItem

#내보낼 위치 설정
AutomationId:65535
Name:선택...
ClassName:Button
ControlType:Button

#새로운 윈도우 뜨면 아래 객체 클릭
AutomationId:1001
Name:주소: (포함한 문자 찾기)
ClassName:ToolbarWindow32
ControlType:ToolBar

[입력칸 전체선택 후 delete 하고 'C:\사진저장폴더' 텍스트 붙여넣는 코드 이 스텝에 추가]

#'폴더 선택' 클릭
AutomationId:1
Name:폴더 선택
ClassName:Button
ControlType:Button

#아래 객체 토글 상태 확인하여 On 만들기 
AutomationId:100
Name:하위 폴더에 넣기:
ClassName:Button
ControlType:CheckBox

#입력창 클릭 (위 객체로 '상대 위치 기반 탐색'하여 찾기)
AutomationId:65535
Name:
ClassName:Edit
ControlType:Edit

[입력칸 전체선택 후 delete 하고 '내보내기 시작' 버튼 누르기 전 입력되어 조합된 텍스트 붙여넣는 코드 이 스텝에 추가 (형식 : 홍길동1234)]
### ---<파일 이름 지정>---

#아래 객체로 '파일 이름 지정' 설정창이 활성화 돼있는지 확인
#아래 객체가 보이면 이 스텝 바이패스
AutomationId:100
Name:바꿀 이름:
ClassName:Button
ControlType:CheckBox

#안보이면 아래 텍스트 한 번 클릭하기
AutomationId:-1744491968
Name:파일 이름 지정
ClassName:Static
ControlType:Text

#토글 상태 확인하여 On 만들기
AutomationId:100
Name:바꿀 이름:
ClassName:Button
ControlType:CheckBox

#콤보 박스 클릭 (위 객체와 상대 위치 기반 탐색 설정)
AutomationId:2134
Name:
ClassName:ComboBox
ControlType:ComboBox

#선택
AutomationId:3
Name:사용자 정의 이름 - 원본 파일 번호
ClassName:Not Supported
ControlType:MenuItem

#입력칸 클릭
AutomationId:2136
Name:사용자 정의 텍스트:
ClassName:Edit
ControlType:Edit

[입력칸 전체선택 후 delete 하고 '필터' 텍스트 입력 코드를 이 스텝에 추가]
### ---<파일 설정>---

#아래 객체 보이는지 확인하여 보이면 현재 스텝 바이패스
AutomationId:-1076339584
Name:이미지 형식:
ClassName:Static
ControlType:Text

[보이지 않으면 아래 객체 클릭하는 코드 이 스텝에 추가]

AutomationId:-1799002016
Name:파일 설정
ClassName:Static
ControlType:Text

#콤보 박스 클릭
AutomationId:65535
Name:이미지 형식:
ClassName:ComboBox
ControlType:ComboBox

#선택
AutomationId:1
Name:JPEG
ClassName:Not Supported
ControlType:MenuItem

#아래 객체 상대 위치 기반 탐색
AutomationId:100
Name:품질:
ClassName:msctls_trackbar32
ControlType:Slider

#입력칸 클릭
AutomationId:65535
Name:
ClassName:Edit
ControlType:Edit

[전체선택 후 delete 하고'90' 텍스트 입력 코드 이 스텝에 추가]

#아래 토글상태 확인 후 Off
AutomationId:100
Name:다음으로 파일 크기 제한:
ClassName:Button
ControlType:CheckBox
### ---<이미지 크기 조정>---

#아래 객체 보이는지 확인하여 보이면 현재 스텝 바이패스
AutomationId:100
Name:크기 조정하여 맞추기:
ClassName:Button
ControlType:CheckBox

[보이지 않으면 아래 객체 클릭하는 코드 이 스텝에 추가]
AutomationId:-1853753216
Name:이미지 크기 조정
ClassName:Static
ControlType:Text

#토클 상태 확인하여 체크박스 Off
AutomationId:100
Name:크기 조정하여 맞추기:
ClassName:Button
ControlType:CheckBox

#위 객체 상대 위치 기반 탐색 설정하여 아래 콤보박스 클릭
AutomationId:65535
Name:
ClassName:ComboBox
ControlType:ComboBox

#선택
AutomationId:1
Name:너비 및 높이
ClassName:Not Supported
ControlType:MenuItem

#토글 상태 확인하여 체크박스 Off
AutomationId:100
Name:확대 안 함
ClassName:Button
ControlType:CheckBox


[아래 객체에 텍스트 '2456' 입력하는 코드 이 스텝에 추가]
AutomationId:65535
Name:%
ClassName:Edit
ControlType:Edit

[아래 객체에 텍스트 '4000' 입력하는 코드 이 스텝에 추가]
AutomationId:65535
Name:높이:
ClassName:Edit
ControlType:Edit

#콤보박스 클릭
AutomationId:65535
Name:메가픽셀
ClassName:ComboBox
ControlType:ComboBox

#선택
AutomationId:1
Name:픽셀
ClassName:Not Supported
ControlType:MenuItem

AutomationId:65535
Name:해상도:
ClassName:Edit
ControlType:Edit

#콤보박스 클릭 (위 객체 상대 위치 기반 탐색 설정)
AutomationId:65535
Name:
ClassName:ComboBox
ControlType:ComboBox

#선택
AutomationId:1
Name:인치당 픽셀
ClassName:Not Supported
ControlType:MenuItem

#클릭
AutomationId:65535
Name:해상도:
ClassName:Edit
ControlType:Edit

[전체선택 후 delete 하고'160' 텍스트 입력 코드 이 스텝에 추가]
### ---<출력 선명하게 하기>---

#아래 객체 보이는지 확인하여 보이면 현재 스텝 바이패스
AutomationId:100
Name:선명하게 하기:
ClassName:Button
ControlType:CheckBox

[보이지 않으면 아래 객체 클릭하는 코드 이 스텝에 추가]

AutomationId:-1853757824
Name:출력 선명하게 하기
ClassName:Static
ControlType:Text

#토글상태 확인하여 체크박스 On
AutomationId:100
Name:선명하게 하기:
ClassName:Button
ControlType:CheckBox

#콤보박스 클릭
AutomationId:100
Name:선명하게 하기:
ClassName:Button
ControlType:CheckBox

#선택 (위 객체와 상대 위치 기반 탐색 설정)
AutomationId:65535
Name:
ClassName:ComboBox
ControlType:ComboBox

#콤보박스 클릭
AutomationId:1
Name:화면
ClassName:Not Supported
ControlType:MenuItem

AutomationId:-1788895104
Name:양:
ClassName:Static
ControlType:Text

#콤보박스 클릭 (위 객체와 상대 위치 기반 탐색 설정)
AutomationId:65535
Name:양:
ClassName:ComboBox
ControlType:ComboBox

#선택 (AutomationId:-1788895104 객체와 상대 위치 기반 탐색 설정)
AutomationId:3
Name:고
ClassName:Not Supported
ControlType:MenuItem

#내보내기 클릭
AutomationId:1
Name:내보내기
ClassName:Button
ControlType:Button
## ---<덮어쓰기 경고 대응>

[내보내기 클릭 후 2초까지 아래 객체 화면이 보이면 '덮어쓰기' 버튼 클릭하는 코드 이 스텝에 추가]

AutomationId:-2012510512
Name:다음 파일이 이미 존재합니다.
ClassName:Static
ControlType:Text

#'덮어쓰기' 버튼
AutomationId:65535
Name:덮어쓰기
ClassName:Button
ControlType:Button

[내보내기 클릭 후 2초까지 아래 객체 화면이 안보이면 프로세스 종료]
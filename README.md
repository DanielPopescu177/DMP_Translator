# DMP Translator 사용 방법

## 1. 필요한 것
- Visual Studio 2022 또는 Visual Studio Code
- .NET 6.0 SDK (https://dotnet.microsoft.com/download/dotnet/6.0)
- Duel Masters Play's (MelonLoader 설치됨)

## 2. 설정 방법

### 2-1. 게임 경로 확인
1. Duel Masters Play's가 설치된 폴더를 찾으세요
   예: C:\Program Files\DuelMastersPlays
   또는 C:\Steam\steamapps\common\DuelMastersPlays

2. 폴더 안에 이런 파일들이 있어야 합니다:
   - DuelMastersPlays.exe
   - MelonLoader 폴더
   - Mods 폴더

### 2-2. 프로젝트 파일 수정
1. DMP_Translator.csproj 파일을 메모장으로 엽니다
2. 이 부분을 찾아서 게임 경로로 수정:
   ```xml
   <GamePath>C:\Program Files\DuelMastersPlays</GamePath>
   ```

3. build.bat 파일도 열어서 같은 경로로 수정:
   ```batch
   set GAME_PATH=C:\Program Files\DuelMastersPlays
   ```

## 3. 빌드하기

### 방법 1: build.bat 사용 (간단)
1. build.bat 파일을 더블클릭
2. 빌드가 완료되면 자동으로 게임 폴더에 복사됨

### 방법 2: 명령어 사용
1. 폴더에서 마우스 우클릭 > "터미널에서 열기"
2. 다음 명령어 입력:
   ```
   dotnet build -c Release
   ```
3. 생성된 DLL을 게임 Mods 폴더에 복사:
   ```
   copy bin\Release\net6.0\DMP_Translator.dll "C:\게임경로\Mods\"
   ```

## 4. 사용하기

1. 게임을 실행합니다

2. 로그 확인:
   - 게임 폴더에 MelonLoader/Latest.log 파일이 생성됨
   - "DMP 번역기 로드됨!" 메시지 확인

3. 게임에서 키 사용:
   - F8 키: 화면의 모든 텍스트 출력
   - F9 키: 카드 오브젝트만 출력

4. 로그 파일 확인:
   - 게임 폴더의 MelonLoader/Latest.log 열기
   - 어떤 텍스트가 있는지 확인

## 5. 문제 해결

### "빌드 실패" 에러
- .NET 6.0 SDK가 설치되어 있는지 확인
- 명령 프롬프트에서 `dotnet --version` 입력해서 확인

### "DLL 복사 실패" 에러
- 게임 경로가 맞는지 확인
- Mods 폴더가 있는지 확인
- 게임을 닫고 다시 시도

### "모드가 로드되지 않음"
1. Latest.log 파일 확인
2. MelonLoader가 제대로 설치되었는지 확인
3. IL2CPP 게임인지 확인 (GameAssembly.dll이 있어야 함)

## 6. 다음 단계

로그를 확인한 후:
1. 카드 텍스트가 어떤 컴포넌트인지 파악
2. 해당 정보를 알려주면 번역 기능 추가 가능

## 7. 로그 예시

정상 작동 시:
```
[DMP Translator] DMP 번역기 로드됨!
[DMP Translator] F8 키: 화면의 모든 텍스트 정보 출력
[DMP Translator] F9 키: 카드 오브젝트만 찾아서 출력
```

F9 누른 후:
```
[DMP Translator] ===== 카드 오브젝트 찾기 시작 =====
[DMP Translator] [카드 1] CardObject_001
[DMP Translator]   컴포넌트: Text
[DMP Translator]     텍스트: 火文明
[DMP Translator]   자식 텍스트: このクリーチャーが攻撃する時...
```

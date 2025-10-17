using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Linq;

[assembly: MelonInfo(typeof(DMPTranslator.TranslatorMod), "DMP Translator", "1.0.0", "VB")]

namespace DMPTranslator
{
    public class TranslatorMod : MelonMod
    {
        private static Dictionary<string, string> translationCache = new Dictionary<string, string>();
        private static GoogleTranslator translator;
        private static PapagoTranslator papagoTranslator;
        private static DeepLTranslator deepLTranslator;
        private static bool autoTranslateEnabled = false;
        private static string translationEngine = "google"; // google, papago, deepl
        private static string sourceLang = "ja";
        private static string targetLang = "ko";
        
        // UI 설정
        private static float characterSpacing = 2f;
        private static float lineSpacing = 0f;
        private static bool enableAutoSizing = false;
        private static string customFontPath = ""; // 커스텀 폰트 경로
        
        private static string cacheFilePath;
        private static string userDataPath;
        private static Il2CppSystem.Type tmpType = null;
        private static Il2CppSystem.Type tmpFontAssetType = null;
        private static bool tmpTypeSearched = false;
        private static UnityEngine.Object koreanTMPFont = null;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("==========================================");
            MelonLogger.Msg("DMP 번역기 로드됨! / DMP Translator Loaded!");
            MelonLogger.Msg("");
            MelonLogger.Msg("키 안내: / Key Bindings:");
            MelonLogger.Msg("  F8: 화면의 모든 텍스트 정보 출력 / Print all text info on screen");
            MelonLogger.Msg("  F9: 번역 캐시 갱신 (재번역) / Refresh translation cache (re-translate)");
            MelonLogger.Msg("  F10: 자동 번역 ON/OFF / Auto-translate ON/OFF");
            MelonLogger.Msg("  F11: 현재 화면 수동 번역 / Manual translate current screen");
            MelonLogger.Msg("==========================================");

            translator = new GoogleTranslator();
            
            string gamePath = Application.dataPath;
            userDataPath = Path.Combine(Path.GetDirectoryName(gamePath), "UserData", "DMP_Translator");
            Directory.CreateDirectory(userDataPath);
            
            cacheFilePath = Path.Combine(userDataPath, "translation_cache.txt");
            string configPath = Path.Combine(userDataPath, "translator_config.txt");

            LoadConfig(configPath);
            LoadCache();
            
            MelonLogger.Msg($"번역 캐시: {translationCache.Count}개 로드됨 / Translation cache: {translationCache.Count} loaded");
            MelonLogger.Msg($"번역 엔진 / Engine: {translationEngine.ToUpper()}");
            MelonLogger.Msg($"번역 방향 / Direction: {sourceLang.ToUpper()} → {targetLang.ToUpper()}");
            MelonLogger.Msg("");
            MelonLogger.Msg("UI 설정 / UI Settings:");
            MelonLogger.Msg($"  글자 간격 (TMP) / Character Spacing (TMP): {characterSpacing}");
            MelonLogger.Msg($"  줄 간격 (모든 텍스트) / Line Spacing (All Text): {lineSpacing}");
            MelonLogger.Msg($"  자동 크기 조절 (TMP) / Auto Sizing (TMP): {(enableAutoSizing ? "ON" : "OFF")}");
            if (!string.IsNullOrEmpty(customFontPath))
            {
                MelonLogger.Msg($"  커스텀 폰트 / Custom Font: {customFontPath}");
            }
            
            // TMP 타입 찾기
            FindTMPType();
            
            // 한글 폰트 로드
            LoadKoreanFont();
        }

        private void LoadConfig(string configPath)
        {
            if (File.Exists(configPath))
            {
                try
                {
                    var lines = File.ReadAllLines(configPath);
                    string papagoClientId = "";
                    string papagoClientSecret = "";
                    string deepLApiKey = "";
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("TRANSLATION_ENGINE="))
                        {
                            translationEngine = line.Substring("TRANSLATION_ENGINE=".Length).Trim().ToLower();
                        }
                        else if (line.StartsWith("SOURCE_LANG="))
                        {
                            sourceLang = line.Substring("SOURCE_LANG=".Length).Trim().ToLower();
                        }
                        else if (line.StartsWith("TARGET_LANG="))
                        {
                            targetLang = line.Substring("TARGET_LANG=".Length).Trim().ToLower();
                        }
                        else if (line.StartsWith("PAPAGO_CLIENT_ID="))
                        {
                            papagoClientId = line.Substring("PAPAGO_CLIENT_ID=".Length).Trim();
                        }
                        else if (line.StartsWith("PAPAGO_CLIENT_SECRET="))
                        {
                            papagoClientSecret = line.Substring("PAPAGO_CLIENT_SECRET=".Length).Trim();
                        }
                        else if (line.StartsWith("DEEPL_API_KEY="))
                        {
                            deepLApiKey = line.Substring("DEEPL_API_KEY=".Length).Trim();
                        }
                        else if (line.StartsWith("CHARACTER_SPACING="))
                        {
                            if (float.TryParse(line.Substring("CHARACTER_SPACING=".Length).Trim(), out float cs))
                            {
                                characterSpacing = cs;
                            }
                        }
                        else if (line.StartsWith("LINE_SPACING="))
                        {
                            if (float.TryParse(line.Substring("LINE_SPACING=".Length).Trim(), out float ls))
                            {
                                lineSpacing = ls;
                            }
                        }
                        else if (line.StartsWith("ENABLE_AUTO_SIZING="))
                        {
                            enableAutoSizing = line.Substring("ENABLE_AUTO_SIZING=".Length).Trim().ToLower() == "true";
                        }
                        else if (line.StartsWith("CUSTOM_FONT_PATH="))
                        {
                            customFontPath = line.Substring("CUSTOM_FONT_PATH=".Length).Trim();
                        }
                    }

                    // Papago 초기화
                    if (!string.IsNullOrEmpty(papagoClientId) && !string.IsNullOrEmpty(papagoClientSecret))
                    {
                        papagoTranslator = new PapagoTranslator(papagoClientId, papagoClientSecret);
                        MelonLogger.Msg("Papago API 설정 완료");
                    }
                    
                    // DeepL 초기화
                    if (!string.IsNullOrEmpty(deepLApiKey))
                    {
                        deepLTranslator = new DeepLTranslator(deepLApiKey);
                        MelonLogger.Msg("DeepL API 설정 완료");
                    }
                }
                catch (System.Exception e)
                {
                    MelonLogger.Error($"설정 파일 로드 실패: {e.Message}");
                }
            }
            else
            {
                File.WriteAllText(configPath, @"# DMP Translator 설정 파일 / Configuration File

# 번역 엔진 선택 (google, papago, deepl)
# Translation engine selection (google, papago, deepl)
# google: 무료, API 키 불필요 / Free, no API key required
# papago: 네이버 클라우드 API 키 필요 (정확도 높음) / Naver Cloud API key required (high accuracy)
# deepl: DeepL API 키 필요 (품질 최고, 월 50만자 무료) / DeepL API key required (best quality, 500k chars/month free)
TRANSLATION_ENGINE=google

# 번역 언어 설정
# Translation language settings
# 지원 언어 / Supported languages: ja(일본어/Japanese), en(영어/English), ko(한국어/Korean), zh-CN(중국어/Chinese), etc.
SOURCE_LANG=ja
TARGET_LANG=ko

# === Papago API 설정 / Papago API Settings ===
# API 키 발급 / Get API key: https://www.ncloud.com/
# 1. 회원가입/로그인 / Sign up/Login
# 2. Console > AI·NAVER API > Application 등록 / Register Application
# 3. Papago NMT 선택 / Select Papago NMT
# 4. Client ID와 Client Secret 복사 / Copy Client ID and Client Secret
PAPAGO_CLIENT_ID=
PAPAGO_CLIENT_SECRET=

# === DeepL API 설정 / DeepL API Settings ===
# API 키 발급 / Get API key: https://www.deepl.com/pro-api
# 무료 플랜 / Free plan: 월 50만자 무료 / 500k chars/month free
# 주의 / Note: DeepL은 언어 코드를 대문자로 사용 (JA, KO, EN 등) / DeepL uses uppercase language codes (JA, KO, EN, etc.)
DEEPL_API_KEY=

# === UI 설정 / UI Settings ===
# ⚠️ 주의 / WARNING: 
# CHARACTER_SPACING과 ENABLE_AUTO_SIZING은 TextMeshPro(TMP)에만 적용됩니다
# CHARACTER_SPACING and ENABLE_AUTO_SIZING only apply to TextMeshPro(TMP)
# LINE_SPACING은 일반 UI Text와 TMP 모두 적용됩니다
# LINE_SPACING applies to both regular UI Text and TMP

# 글자 간격 조정 (0~10, 기본: 2) - TMP 전용
# Character spacing adjustment (0~10, default: 2) - TMP only
# 글자가 겹치면 숫자를 높이세요 (예: 3, 4, 5)
# Increase the value if characters overlap (e.g., 3, 4, 5)
# 일반 UI Text에는 적용되지 않습니다
# Not applicable to regular UI Text
CHARACTER_SPACING=2

# 줄 간격 조정 (-10~10, 기본: 0) - 모든 텍스트 적용
# Line spacing adjustment (-10~10, default: 0) - Applies to all text
# 줄 사이 간격을 조절합니다
# Adjusts spacing between lines
# 양수: 줄 간격 넓게, 음수: 줄 간격 좁게
# Positive: wider spacing, Negative: narrower spacing
LINE_SPACING=0

# 자동 크기 조절 (true/false, 기본: false) - TMP 전용
# Auto size adjustment (true/false, default: false) - TMP only
# true로 설정하면 텍스트가 박스에 맞게 자동으로 크기 조절됨
# When set to true, text automatically resizes to fit the box
# 카드 텍스트가 잘릴 때 유용합니다
# Useful when card text gets cut off
# 일반 UI Text에는 적용되지 않습니다
# Not applicable to regular UI Text
ENABLE_AUTO_SIZING=false

# 커스텀 폰트 경로 (선택사항)
# Custom font path (optional)
# 빈 칸으로 두면 기본 폰트(notosanscjkjp_bold) 사용
# Leave blank to use default font (notosanscjkjp_bold)
# 다른 폰트를 사용하려면 AssetBundle 파일 경로를 입력하세요
# Enter AssetBundle file path to use a different font
# 예시 / Example: CUSTOM_FONT_PATH=UserData/DMP_Translator/my_custom_font
CUSTOM_FONT_PATH=

# === 사용 예시 / Usage Examples ===
#
# Google로 일본어→한국어 (기본, 무료):
# Japanese→Korean with Google (default, free):
# TRANSLATION_ENGINE=google
# SOURCE_LANG=ja
# TARGET_LANG=ko
#
# Papago로 일본어→영어:
# Japanese→English with Papago:
# TRANSLATION_ENGINE=papago
# SOURCE_LANG=ja
# TARGET_LANG=en
# PAPAGO_CLIENT_ID=your_client_id
# PAPAGO_CLIENT_SECRET=your_client_secret
#
# DeepL로 일본어→한국어:
# Japanese→Korean with DeepL:
# TRANSLATION_ENGINE=deepl
# SOURCE_LANG=JA
# TARGET_LANG=KO
# DEEPL_API_KEY=your_api_key
#
# 글자 겹침 해결:
# Fix overlapping characters:
# CHARACTER_SPACING=3 또는 4로 증가 / Increase to 3 or 4
#
# 자동 크기 조절 활성화:
# Enable auto size adjustment:
# ENABLE_AUTO_SIZING=true
");
                MelonLogger.Msg($"설정 파일 생성됨: {configPath}");
            }
        }

        private void LoadCache()
        {
            if (File.Exists(cacheFilePath))
            {
                try
                {
                    var lines = File.ReadAllLines(cacheFilePath);
                    foreach (var line in lines)
                    {
                        // 검색어와 번역을 구분하는 구분자: =
                        var separatorIndex = line.IndexOf("=");
                        if (separatorIndex > 0)
                        {
                            var original = line.Substring(0, separatorIndex);
                            var translated = line.Substring(separatorIndex + 1);
                            
                            // \\n을 실제 줄바꿈로 복원
                            original = original.Replace("\\n", "\n");
                            translated = translated.Replace("\\n", "\n");
                            
                            translationCache[original] = translated;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    MelonLogger.Error($"캐시 로드 실패: {e.Message}");
                }
            }
        }

        private void SaveCache()
        {
            try
            {
                var lines = new List<string>();
                foreach (var kvp in translationCache)
                {
                    // 줄바꿈을 \n으로 변환
                    var key = kvp.Key.Replace("\n", "\\n").Replace("\r", "");
                    var value = kvp.Value.Replace("\n", "\\n").Replace("\r", "");
                    lines.Add($"{key}={value}");
                }
                File.WriteAllLines(cacheFilePath, lines);
                MelonLogger.Msg($"번역 캐시 저장 완료: {translationCache.Count}개");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"캐시 저장 실패: {e.Message}");
            }
        }

        private Il2CppSystem.Type FindTMPType()
        {
            if (tmpTypeSearched)
                return tmpType;

            tmpTypeSearched = true;

            try
            {
                var assembly = Il2CppSystem.Reflection.Assembly.Load("Unity.TextMeshPro");
                if (assembly != null)
                {
                    tmpType = assembly.GetType("TMPro.TextMeshProUGUI");
                    tmpFontAssetType = assembly.GetType("TMPro.TMP_FontAsset");
                    
                    if (tmpType != null)
                    {
                        MelonLogger.Msg("TextMeshProUGUI 타입 발견! / TextMeshProUGUI type found!");
                    }
                    if (tmpFontAssetType != null)
                    {
                        MelonLogger.Msg("TMP_FontAsset 타입 발견! / TMP_FontAsset type found!");
                    }
                    
                    return tmpType;
                }
                
                MelonLogger.Warning("TextMeshPro 타입을 찾을 수 없습니다 / Cannot find TextMeshPro type");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"TMP 타입 검색 중 오류: {e.Message}");
            }

            return null;
        }

        private void LoadKoreanFont()
        {
            MelonLogger.Msg("\n===== 한글 폰트 로딩 / Loading Korean Font =====");
            
            // 커스텀 폰트 경로 확인
            string fontBundlePath;
            if (!string.IsNullOrEmpty(customFontPath))
            {
                fontBundlePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), customFontPath);
                MelonLogger.Msg($"커스텀 폰트 사용 / Using custom font: {customFontPath}");
            }
            else
            {
                fontBundlePath = Path.Combine(userDataPath, "notosanscjkjp_bold");
                MelonLogger.Msg("기본 폰트 사용 / Using default font");
            }
            
            if (!File.Exists(fontBundlePath))
            {
                MelonLogger.Warning($"한글 폰트 번들이 없습니다 / Korean font bundle not found: {fontBundlePath}");
                MelonLogger.Msg("");
                MelonLogger.Msg("📌 한글 폰트 설치 / Korean Font Installation:");
                MelonLogger.Msg($"   'notosanscjkjp_bold' 파일을 이 경로에 넣으세요 / Put 'notosanscjkjp_bold' file in this path:");
                MelonLogger.Msg($"   {fontBundlePath}");
                MelonLogger.Msg("");
                MelonLogger.Msg("⚠️ 폰트 없이도 번역은 작동합니다 (□□로 표시, 로그 확인) / Translation works without font (displays as □□, check logs)");
                MelonLogger.Msg("");
                return;
            }

            try
            {
                MelonLogger.Msg($"AssetBundle 로딩 중 / Loading AssetBundle: {fontBundlePath}");
                var bundle = AssetBundle.LoadFromFile(fontBundlePath);
                
                if (bundle == null)
                {
                    MelonLogger.Error("AssetBundle 로드 실패! / AssetBundle load failed!");
                    return;
                }
                
                MelonLogger.Msg("✅ AssetBundle 로드 성공! / AssetBundle loaded successfully!");
                
                // 번들 내 모든 에셋 이름 출력
                var assetNames = bundle.GetAllAssetNames();
                MelonLogger.Msg($"번들 내 에셋 {assetNames.Length}개: / {assetNames.Length} assets in bundle:");
                foreach (var name in assetNames)
                {
                    MelonLogger.Msg($"  - {name}");
                }
                
                // 에셋 이름으로 직접 로드
                if (assetNames.Length > 0)
                {
                    string assetName = assetNames[0]; // "assets/assetbundles/notosanscjkjp-bold sdf.asset"
                    MelonLogger.Msg($"폰트 에셋 로딩 / Loading font asset: {assetName}");
                    
                    koreanTMPFont = bundle.LoadAsset<UnityEngine.Object>(assetName);
                    
                    if (koreanTMPFont != null)
                    {
                        koreanTMPFont.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        MelonLogger.Msg($"✅ TMP 폰트 에셋 로드 성공! / TMP font asset loaded successfully!");
                        MelonLogger.Msg($"   이름 / Name: {koreanTMPFont.name}");
                        MelonLogger.Msg($"   타입 / Type: {koreanTMPFont.GetType().Name}");
                        
                        // 폰트 초기화 시도
                        try
                        {
                            if (tmpFontAssetType != null)
                            {
                                // ReadFontAssetDefinition 메서드 호출
                                var readMethod = tmpFontAssetType.GetMethod("ReadFontAssetDefinition");
                                if (readMethod != null)
                                {
                                    readMethod.Invoke(koreanTMPFont, null);
                                    MelonLogger.Msg("✅ 폰트 초기화 완료! / Font initialization complete!");
                                }
                                else
                                {
                                    MelonLogger.Warning("ReadFontAssetDefinition 메서드를 찾을 수 없습니다 / Cannot find ReadFontAssetDefinition method");
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            MelonLogger.Warning($"폰트 초기화 실패 / Font initialization failed: {e.Message}");
                        }
                    }
                    else
                    {
                        MelonLogger.Error("폰트 에셋 로드 실패 (null) / Font asset load failed (null)");
                    }
                }
                
                if (koreanTMPFont != null)
                {
                    MelonLogger.Msg("✅ 한글 폰트 준비 완료! / Korean font ready!");
                }
                else
                {
                    MelonLogger.Warning("⚠️ TMP 폰트를 찾지 못했습니다 / TMP font not found");
                }
                
                MelonLogger.Msg("===== 폰트 로딩 완료 / Font Loading Complete =====\n");
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"폰트 로드 실패: {e.Message}");
                MelonLogger.Error($"스택: {e.StackTrace}");
            }
        }

        public override void OnUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F8))
            {
                PrintAllTexts();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                MelonLogger.Msg("번역 캐시 갱신 중... / Refreshing translation cache...");
                translationCache.Clear();
                LoadCache(); // 파일에서 다시 로드
                MelonLogger.Msg($"캐시 갱신 완료: {translationCache.Count}개 로드됨 / Cache refreshed: {translationCache.Count} loaded");
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.F10))
            {
                autoTranslateEnabled = !autoTranslateEnabled;
                MelonLogger.Msg($"자동 번역 / Auto-translate: {(autoTranslateEnabled ? "ON" : "OFF")}");
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.F11))
            {
                MelonLogger.Msg("\n화면 번역 시작... / Starting screen translation...");
                TranslateAllTexts();
            }

            if (autoTranslateEnabled && Time.frameCount % 30 == 0) // 120 → 30 (빠르게)
            {
                TranslateAllTexts();
            }
        }

        private void PrintAllTexts()
        {
            MelonLogger.Msg("\n===== 모든 Text 출력 시작 =====");

            var texts = UnityEngine.Object.FindObjectsOfType<Text>();
            MelonLogger.Msg($"Unity Text: {texts.Length}개");
            int uiTextCount = 0;
            foreach (var text in texts)
            {
                if (text != null && text.gameObject.activeInHierarchy && !string.IsNullOrEmpty(text.text))
                {
                    MelonLogger.Msg($"[UI Text][{text.gameObject.name}] {text.text}");
                    uiTextCount++;
                }
            }
            MelonLogger.Msg($"실제 출력된 UI Text: {uiTextCount}개");

            if (tmpType != null)
            {
                try
                {
                    var tmps = UnityEngine.Object.FindObjectsOfType(tmpType);
                    MelonLogger.Msg($"\nTextMeshPro 오브젝트: {tmps.Length}개");
                    
                    var textProp = tmpType.GetProperty("text");
                    int tmpTextCount = 0;
                    
                    foreach (var tmp in tmps)
                    {
                        try
                        {
                            var comp = tmp.TryCast<Component>();
                            if (comp != null && comp.gameObject.activeInHierarchy)
                            {
                                var textValue = textProp?.GetValue(tmp)?.ToString();
                                if (!string.IsNullOrEmpty(textValue))
                                {
                                    MelonLogger.Msg($"[TMP][{comp.gameObject.name}] {textValue}");
                                    tmpTextCount++;
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            MelonLogger.Warning($"TMP 텍스트 읽기 실패: {e.Message}");
                        }
                    }
                    MelonLogger.Msg($"실제 출력된 TMP Text: {tmpTextCount}개");
                }
                catch (System.Exception e)
                {
                    MelonLogger.Error($"TextMeshPro 처리 중 오류: {e.Message}");
                }
            }

            MelonLogger.Msg("===== 출력 완료 =====\n");
        }

        private static bool isTranslating = false;
        private static int translationDelay = 0;
        private static int currentBatchIndex = 0; // 현재 배치 인덱스

        private async void TranslateAllTexts()
        {
            // 이미 번역 중이면 건너뛸기
            if (isTranslating)
            {
                return;
            }

            isTranslating = true;
            int translated = 0;
            int tmpTranslated = 0;

            try
            {
                var texts = UnityEngine.Object.FindObjectsOfType<Text>();
                
                // UI Text는 일반적으로 개수가 적으므로 전체 번역
                foreach (var text in texts)
            {
                if (text != null && text.gameObject.activeInHierarchy && !string.IsNullOrEmpty(text.text))
                {
                    if (IsJapanese(text.text))
                    {
                        string original = text.text;
                        string result = "";
                        
                        if (translationCache.TryGetValue(original, out string cached))
                        {
                            result = cached;
                        }
                        else
                        {
                            result = await TranslateText(original);
                            if (result != original)
                            {
                                translationCache[original] = result;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(result) && result != original)
                        {
                            text.text = result;
                            
                            // UI Text에 줄 간격 적용 (lineSpacing만 지원)
                            try
                            {
                                text.lineSpacing = lineSpacing;
                            }
                            catch { }
                            
                            translated++;
                            MelonLogger.Msg($"[UI Text] {original} → {result}");
                            if (lineSpacing != 0)
                            {
                            MelonLogger.Msg($"  └ 줄 간격 / Line spacing: {lineSpacing}");
                            }
                        }
                    }
                }
            }

            if (tmpType != null)
            {
                try
                {
                    var tmps = UnityEngine.Object.FindObjectsOfType(tmpType);
                    
                    // TMP 텍스트 배치 처리 (150개씩 나눠서 번역)
                    const int BATCH_SIZE = 150;
                    int totalCount = tmps.Length;
                    
                    if (totalCount == 0)
                    {
                        // TMP 텍스트가 없으면 배치 인덱스 초기화
                        currentBatchIndex = 0;
                    }
                    else
                    {
                        // 현재 배치 범위 계산
                        int startIndex = currentBatchIndex * BATCH_SIZE;
                        int endIndex = Math.Min(startIndex + BATCH_SIZE, totalCount);
                        
                        if (startIndex >= totalCount)
                        {
                            // 모든 배치 완료, 처음으로 돌아가기
                            currentBatchIndex = 0;
                            startIndex = 0;
                            endIndex = Math.Min(BATCH_SIZE, totalCount);
                        }
                        
                        MelonLogger.Msg($"TMP 번역: {startIndex+1}~{endIndex}/{totalCount}");
                        
                        var textProp = tmpType.GetProperty("text");
                        var fontProp = tmpType.GetProperty("font");
                        
                        // 현재 배치만 처리
                        for (int i = startIndex; i < endIndex; i++)
                        {
                            try
                            {
                                // 인덱스 범위 체크
                                if (i >= tmps.Length)
                                    break;
                                    
                                var tmp = tmps[i];
                                if (tmp == null)
                                    continue;
                                    
                                var comp = tmp.TryCast<Component>();
                            
                            // 오브젝트 유효성 체크
                            if (comp == null || !comp.gameObject)
                                continue;
                            if (!comp.gameObject.activeInHierarchy)
                                continue;
                                
                            if (textProp != null)
                            {
                                var textValue = textProp.GetValue(tmp)?.ToString();
                                
                                if (!string.IsNullOrEmpty(textValue) && IsJapanese(textValue))
                                {
                                    string original = textValue;
                                    string result = "";
                                    
                                    if (translationCache.TryGetValue(original, out string cached))
                                    {
                                        result = cached;
                                    }
                                    else
                                    {
                                        result = await TranslateText(original);
                                        if (result != original)
                                        {
                                            translationCache[original] = result;
                                        }
                                    }
                                    
                                    if (!string.IsNullOrEmpty(result) && result != original)
                                    {
                                        // 1. 먼저 텍스트 변경
                                        try
                                        {
                                            // 모든 단계마다 null 체크
                                            if (tmp == null || comp == null || comp.gameObject == null)
                                                continue;
                                            if (!comp.gameObject.activeInHierarchy)
                                                continue;
                                                
                                            textProp.SetValue(tmp, result);
                                        }
                                        catch
                                        {
                                            // 텍스트 변경 실패, 건너뛰기
                                            continue;
                                        }
                                        
                                        // 2. 폰트 적용 (텍스트 변경 후에!)
                                        if (koreanTMPFont != null && fontProp != null)
                                        {
                                            try
                                            {
                                                // 다시 한번 null 체크
                                                if (tmp == null || comp == null || comp.gameObject == null)
                                                    continue;
                                                if (!comp.gameObject.activeInHierarchy)
                                                    continue;
                                                    
                                                var setValueMethod = fontProp.GetSetMethod();
                                                if (setValueMethod != null)
                                                {
                                                    setValueMethod.Invoke(tmp, new Il2CppSystem.Object[] { koreanTMPFont });
                                                }
                                            }
                                            catch
                                            {
                                                // 폰트 적용 실패, 계속 진행
                                            }
                                        }
                                        
                                        // TMP UI 설정 적용
                                        List<string> appliedSettings = new List<string>();
                                        try
                                        {
                                            // Character Spacing 조정 (글자 겹침 방지)
                                            var characterSpacingProp = tmpType.GetProperty("characterSpacing");
                                            if (characterSpacingProp != null && characterSpacingProp.CanWrite)
                                            {
                                                characterSpacingProp.SetValue(tmp, characterSpacing);
                                                if (characterSpacing != 0)
                                                {
                                                    appliedSettings.Add($"글자간격 / Char spacing: {characterSpacing}");
                                                }
                                            }
                                            
                                            // Line Spacing 조정 (줄 간격)
                                            var lineSpacingProp = tmpType.GetProperty("lineSpacing");
                                            if (lineSpacingProp != null && lineSpacingProp.CanWrite)
                                            {
                                                lineSpacingProp.SetValue(tmp, lineSpacing);
                                                if (lineSpacing != 0)
                                                {
                                                    appliedSettings.Add($"줄간격 / Line spacing: {lineSpacing}");
                                                }
                                            }
                                            
                                            // Auto Sizing 설정
                                            if (enableAutoSizing)
                                            {
                                                var enableAutoSizingProp = tmpType.GetProperty("enableAutoSizing");
                                                if (enableAutoSizingProp != null && enableAutoSizingProp.CanWrite)
                                                {
                                                    enableAutoSizingProp.SetValue(tmp, true);
                                                    appliedSettings.Add("자동크기조절 / Auto sizing: ON");
                                                }
                                                
                                                // Auto Sizing 범위 설정 (선택사항)
                                                var fontSizeMinProp = tmpType.GetProperty("fontSizeMin");
                                                var fontSizeMaxProp = tmpType.GetProperty("fontSizeMax");
                                                if (fontSizeMinProp != null && fontSizeMinProp.CanWrite)
                                                {
                                                    fontSizeMinProp.SetValue(tmp, 10f); // 최소 크기
                                                }
                                                if (fontSizeMaxProp != null && fontSizeMaxProp.CanWrite)
                                                {
                                                    fontSizeMaxProp.SetValue(tmp, 72f); // 최대 크기
                                                }
                                            }
                                        }
                                        catch (System.Exception e)
                                        {
                                            MelonLogger.Warning($"TMP UI 설정 적용 실패: {e.Message}");
                                        }
                                        
                                        // TMP 강제 업데이트 (글자 깨짐 방지)
                                        try
                                        {
                                            var updateMethod = tmpType.GetMethod("ForceMeshUpdate", new Il2CppSystem.Type[] { });
                                            if (updateMethod != null)
                                            {
                                                updateMethod.Invoke(tmp, null);
                                            }
                                        }
                                        catch { }
                                        
                                        translated++;
                                        tmpTranslated++;
                                        MelonLogger.Msg($"[TMP] {original} → {result}");
                                        if (appliedSettings.Count > 0)
                                        {
                                        MelonLogger.Msg($"  └ 설정 적용 / Settings applied: {string.Join(", ", appliedSettings)}");
                                        }
                                    }
                                }
                            }
                            }
                            catch (System.Exception e)
                            {
                                // 오브젝트 접근 중 에러 - 무시하고 계속
                                continue;
                            }
                        }
                        
                        // 다음 배치로 이동
                        currentBatchIndex++;
                    }
                }
                catch (System.Exception e)
                {
                    MelonLogger.Error($"TextMeshPro 번역 중 오류: {e.Message}");
                }
            }

                if (translated > 0)
                {
                    MelonLogger.Msg($"\n===== 번역 완료 / Translation Complete: {translated}개 / {translated} texts =====");
                    
                    if (tmpTranslated > 0)
                    {
                        if (koreanTMPFont != null)
                        {
                            MelonLogger.Msg("✅ TMP 한글 폰트 적용됨 / TMP Korean font applied");
                        }
                        else
                        {
                            MelonLogger.Warning("⚠️ TMP 한글 폰트 없음 (□□로 표시) / TMP Korean font not available (displays as □□)");
                        }
                    }
                }
                else
                {
                    MelonLogger.Msg("번역할 일본어 텍스트가 없습니다 / No Japanese text to translate");
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"번역 중 오류: {e.Message}");
            }
            finally
            {
                isTranslating = false;
            }
        }

        private async Task<string> TranslateText(string text)
        {
            try
            {
                switch (translationEngine)
                {
                    case "papago":
                        if (papagoTranslator != null)
                        {
                            return await papagoTranslator.TranslateAsync(text, sourceLang, targetLang);
                        }
                        MelonLogger.Warning("Papago API 키가 설정되지 않음. Google로 대체");
                        break;
                        
                    case "deepl":
                        if (deepLTranslator != null)
                        {
                            return await deepLTranslator.TranslateAsync(text, sourceLang.ToUpper(), targetLang.ToUpper());
                        }
                        MelonLogger.Warning("DeepL API 키가 설정되지 않음. Google로 대체");
                        break;
                }
                
                // 기본: Google Translate
                return await translator.TranslateAsync(text, sourceLang, targetLang);
            }
            catch (System.Exception e)
            {
                MelonLogger.Error($"번역 오류: {e.Message}");
                return text;
            }
        }

        private bool IsJapanese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            
            foreach (char c in text)
            {
                if ((c >= 0x3040 && c <= 0x309F) ||
                    (c >= 0x30A0 && c <= 0x30FF) ||
                    (c >= 0x4E00 && c <= 0x9FFF))
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnApplicationQuit()
        {
            SaveCache();
        }
    }
}

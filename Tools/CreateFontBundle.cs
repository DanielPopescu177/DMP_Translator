using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateFontBundle
{
    [MenuItem("Assets/Build Font AssetBundle")]
    static void BuildFontBundle()
    {
        // 1. 먼저 AssetBundles 폴더 정리
        string assetBundleDirectory = "Assets/AssetBundles";
        if (Directory.Exists(assetBundleDirectory))
        {
            Debug.Log("기존 AssetBundles 폴더 정리 중... / Cleaning existing AssetBundles folder...");
            try
            {
                // .manifest 파일들만 삭제
                string[] manifestFiles = Directory.GetFiles(assetBundleDirectory, "*.manifest", SearchOption.AllDirectories);
                foreach (string file in manifestFiles)
                {
                    File.Delete(file);
                    Debug.Log($"삭제됨 / Deleted: {file}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"정리 중 오류 / Error during cleanup: {e.Message}");
            }
        }
        else
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // AssetDatabase 새로고침
        AssetDatabase.Refresh();

        // 선택한 폰트 에셋 확인
        string[] selection = Selection.assetGUIDs;
        if (selection.Length == 0)
        {
            Debug.LogError("폰트 에셋을 먼저 선택하세요! / Please select a font asset first!");
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(selection[0]);
        Object selectedAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        
        // 선택한 에셋이 TMP 폰트인지 확인
        if (selectedAsset == null)
        {
            Debug.LogError("에셋을 로드할 수 없습니다! / Cannot load asset!");
            return;
        }

        Debug.Log($"선택된 에셋 / Selected asset: {assetPath}");
        Debug.Log($"에셋 타입 / Asset type: {selectedAsset.GetType().Name}");

        // TMP 폰트 에셋인지 확인
        string assetTypeName = selectedAsset.GetType().Name;
        if (assetTypeName != "TMP_FontAsset")
        {
            Debug.LogWarning($"⚠️ 선택한 에셋이 TMP_FontAsset이 아닙니다! / Selected asset is not a TMP_FontAsset!");
            Debug.LogWarning($"현재 타입 / Current type: {assetTypeName}");
            Debug.LogWarning("TextMeshPro 폰트 에셋을 선택해주세요! / Please select a TextMeshPro font asset!");
        }

        // 폰트 이름 추출 (확장자 제거)
        string fontName = Path.GetFileNameWithoutExtension(assetPath);
        fontName = fontName.ToLower().Replace(" ", "").Replace("-", "");
        
        Debug.Log($"번들 이름 / Bundle name: {fontName}");

        // 에셋 번들 이름 설정
        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null)
        {
            Debug.LogError("AssetImporter를 가져올 수 없습니다! / Cannot get AssetImporter!");
            return;
        }

        importer.SetAssetBundleNameAndVariant(fontName, "");
        AssetDatabase.SaveAssets();
        
        Debug.Log("AssetBundle 빌드 시작... / Starting AssetBundle build...");

        try
        {
            // AssetBundle 빌드 (ForceRebuildAssetBundle 옵션 사용)
            BuildPipeline.BuildAssetBundles(
                assetBundleDirectory,
                BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.StrictMode,
                BuildTarget.StandaloneWindows64
            );

            Debug.Log($"✅ AssetBundle 생성 완료! / AssetBundle created successfully!");
            Debug.Log($"📁 위치 / Location: {assetBundleDirectory}/{fontName}");
            Debug.Log("");
            Debug.Log("=== 다음 단계 / Next Steps ===");
            Debug.Log($"1. '{assetBundleDirectory}/{fontName}' 파일을 복사 / Copy the '{assetBundleDirectory}/{fontName}' file");
            Debug.Log($"2. 게임 폴더의 UserData/DMP_Translator/ 에 붙여넣기 / Paste to game's UserData/DMP_Translator/");
            Debug.Log($"3. translator_config.txt에서 CUSTOM_FONT_PATH 설정 / Set CUSTOM_FONT_PATH in translator_config.txt:");
            Debug.Log($"   CUSTOM_FONT_PATH=UserData/DMP_Translator/{fontName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AssetBundle 빌드 실패 / AssetBundle build failed: {e.Message}");
        }
    }

    [MenuItem("Assets/Clean AssetBundles Folder")]
    static void CleanAssetBundlesFolder()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        
        if (!Directory.Exists(assetBundleDirectory))
        {
            Debug.Log("AssetBundles 폴더가 없습니다. / AssetBundles folder does not exist.");
            return;
        }

        Debug.Log("AssetBundles 폴더 전체 정리 중... / Cleaning entire AssetBundles folder...");
        
        try
        {
            // 모든 파일 삭제
            string[] files = Directory.GetFiles(assetBundleDirectory);
            foreach (string file in files)
            {
                File.Delete(file);
                Debug.Log($"삭제됨 / Deleted: {file}");
            }

            // .meta 파일도 정리
            string[] metaFiles = Directory.GetFiles(assetBundleDirectory, "*.meta");
            foreach (string file in metaFiles)
            {
                File.Delete(file);
            }

            AssetDatabase.Refresh();
            Debug.Log("✅ 정리 완료! / Cleanup complete!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"정리 실패 / Cleanup failed: {e.Message}");
        }
    }

    [MenuItem("Assets/Build Multiple Font AssetBundles")]
    static void BuildMultipleFontBundles()
    {
        // 1. 먼저 정리
        string assetBundleDirectory = "Assets/AssetBundles";
        if (Directory.Exists(assetBundleDirectory))
        {
            string[] manifestFiles = Directory.GetFiles(assetBundleDirectory, "*.manifest", SearchOption.AllDirectories);
            foreach (string file in manifestFiles)
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        AssetDatabase.Refresh();

        // 선택한 모든 폰트 에셋 처리
        string[] selection = Selection.assetGUIDs;
        if (selection.Length == 0)
        {
            Debug.LogError("폰트 에셋을 먼저 선택하세요! / Please select font assets first!");
            return;
        }

        int successCount = 0;
        foreach (string guid in selection)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            if (asset == null) continue;

            string assetTypeName = asset.GetType().Name;
            if (assetTypeName != "TMP_FontAsset")
            {
                Debug.LogWarning($"⚠️ 건너뛰기 / Skipping: {assetPath} (Not a TMP_FontAsset)");
                continue;
            }

            // 폰트 이름 자동 생성
            string fontName = Path.GetFileNameWithoutExtension(assetPath);
            fontName = fontName.ToLower().Replace(" ", "").Replace("-", "");

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            importer.SetAssetBundleNameAndVariant(fontName, "");

            Debug.Log($"✅ 번들 설정 완료 / Bundle configured: {fontName}");
            successCount++;
        }

        if (successCount == 0)
        {
            Debug.LogError("TMP_FontAsset이 선택되지 않았습니다! / No TMP_FontAsset selected!");
            return;
        }

        AssetDatabase.SaveAssets();

        Debug.Log($"총 {successCount}개 폰트 빌드 시작... / Building {successCount} fonts...");

        try
        {
            BuildPipeline.BuildAssetBundles(
                assetBundleDirectory,
                BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.StrictMode,
                BuildTarget.StandaloneWindows64
            );

            Debug.Log($"✅ {successCount}개 AssetBundle 생성 완료! / {successCount} AssetBundles created successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AssetBundle 빌드 실패 / AssetBundle build failed: {e.Message}");
        }
    }
}

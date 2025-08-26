#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KYS;
using TMPro;

namespace KYS.Editor
{
    /// <summary>
    /// 프리팹에서 로컬라이제이션 키를 추출하는 에디터 도구
    /// </summary>
    public class PrefabLocalizationKeyExtractor : EditorWindow
    {
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private bool showOnlyUIPrefabs = true;
        private List<string> extractedKeys = new List<string>();
        private Dictionary<string, List<string>> prefabKeysMap = new Dictionary<string, List<string>>();

        [MenuItem("KYS/프리팹 키 추출기")]
        public static void ShowWindow()
        {
            GetWindow<PrefabLocalizationKeyExtractor>("프리팹 키 추출기");
        }

        private void OnGUI()
        {
            GUILayout.Label("프리팹에서 로컬라이제이션 키 추출", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            // 검색 필터
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("검색 필터:", GUILayout.Width(80));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            EditorGUILayout.EndHorizontal();
            
            // UI 프리팹만 표시 옵션
            showOnlyUIPrefabs = EditorGUILayout.Toggle("UI 프리팹만 표시", showOnlyUIPrefabs);
            
            EditorGUILayout.Space();
            
            // 버튼들
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("선택된 프리팹에서 키 추출"))
            {
                ExtractKeysFromSelectedPrefab();
            }
            
            if (GUILayout.Button("모든 UI 프리팹에서 키 추출"))
            {
                ExtractKeysFromAllUIPrefabs();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("특정 폴더의 프리팹에서 키 추출"))
            {
                ExtractKeysFromFolder();
            }
            
                         if (GUILayout.Button("결과를 CSV로 내보내기"))
             {
                 ExportKeysToCSV();
             }
             
             EditorGUILayout.EndHorizontal();
             
                         EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("AutoLocalizedText 자동 추가"))
            {
                AddAutoLocalizedTextToPrefabs();
            }
            
            if (GUILayout.Button("번역 있는 키만 AutoLocalization 활성화 (동적 데이터 제외)"))
            {
                EnableAutoLocalizationForTranslatedKeys();
            }
            
            if (GUILayout.Button("번역 없는 키 + 동적 데이터 키 AutoLocalization 비활성화"))
            {
                DisableAutoLocalizationForUntranslatedKeys();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("번역 상태 확인 및 보고"))
            {
                CheckTranslationStatus();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 결과 표시
            if (extractedKeys.Count > 0)
            {
                GUILayout.Label($"총 {extractedKeys.Count}개의 고유 키를 찾았습니다:", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var kvp in prefabKeysMap)
                {
                    if (string.IsNullOrEmpty(searchFilter) || 
                        kvp.Key.ToLower().Contains(searchFilter.ToLower()) ||
                        kvp.Value.Any(key => key.ToLower().Contains(searchFilter.ToLower())))
                    {
                        EditorGUILayout.BeginVertical("box");
                        
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label($"📁 {kvp.Key}", EditorStyles.boldLabel);
                        if (GUILayout.Button("선택", GUILayout.Width(60)))
                        {
                            SelectPrefab(kvp.Key);
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        foreach (string key in kvp.Value)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(20);
                            GUILayout.Label($"🔑 {key}");
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                }
                
                EditorGUILayout.EndScrollView();
            }
        }

        private void ExtractKeysFromSelectedPrefab()
        {
            if (Selection.activeObject == null)
            {
                EditorUtility.DisplayDialog("알림", "프리팹을 선택해주세요.", "확인");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!prefabPath.EndsWith(".prefab"))
            {
                EditorUtility.DisplayDialog("알림", "선택된 오브젝트가 프리팹이 아닙니다.", "확인");
                return;
            }

            ExtractKeysFromPrefabPath(prefabPath);
        }

        private void ExtractKeysFromAllUIPrefabs()
        {
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab");
            var uiPrefabPaths = new List<string>();

            foreach (string guid in prefabPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (showOnlyUIPrefabs)
                {
                    if (path.Contains("/UI/") || path.Contains("/Prefabs/UI/") || path.Contains("/UIPanel/") || path.Contains("/UIPopup/"))
                    {
                        uiPrefabPaths.Add(path);
                    }
                }
                else
                {
                    uiPrefabPaths.Add(path);
                }
            }

            Debug.Log($"프리팹 {uiPrefabPaths.Count}개에서 키 추출 시작...");
            
            prefabKeysMap.Clear();
            extractedKeys.Clear();
            
            foreach (string path in uiPrefabPaths)
            {
                var keys = ExtractKeysFromPrefabPath(path);
                if (keys.Count > 0)
                {
                    prefabKeysMap[Path.GetFileNameWithoutExtension(path)] = keys;
                    foreach (string key in keys)
                    {
                        if (!extractedKeys.Contains(key))
                        {
                            extractedKeys.Add(key);
                        }
                    }
                }
            }

            Debug.Log($"총 {extractedKeys.Count}개의 고유 키를 찾았습니다.");
            Repaint();
        }

        private void ExtractKeysFromFolder()
        {
            string folderPath = EditorUtility.OpenFolderPanel("프리팹 폴더 선택", "Assets", "");
            if (string.IsNullOrEmpty(folderPath))
                return;

            // Assets 경로로 변환
            if (folderPath.StartsWith(Application.dataPath))
            {
                folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length);
            }

            // 하위 폴더까지 포함하여 모든 프리팹 찾기
            var allPrefabPaths = new List<string>();
            
            // 1. 선택된 폴더의 모든 하위 폴더 찾기
            var subFolders = GetAllSubFolders(folderPath);
            var searchFolders = new List<string> { folderPath };
            searchFolders.AddRange(subFolders);
            
            Debug.Log($"검색할 폴더들: {string.Join(", ", searchFolders)}");
            
            // 2. 모든 폴더에서 프리팹 찾기
            foreach (string searchFolder in searchFolders)
            {
                string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new string[] { searchFolder });
                foreach (string guid in prefabPaths)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!allPrefabPaths.Contains(path))
                    {
                        allPrefabPaths.Add(path);
                    }
                }
            }
            
            Debug.Log($"선택된 폴더와 하위 폴더에서 총 {allPrefabPaths.Count}개의 프리팹을 찾았습니다.");
            
            prefabKeysMap.Clear();
            extractedKeys.Clear();
            
            foreach (string path in allPrefabPaths)
            {
                var keys = ExtractKeysFromPrefabPath(path);
                if (keys.Count > 0)
                {
                    prefabKeysMap[Path.GetFileNameWithoutExtension(path)] = keys;
                    foreach (string key in keys)
                    {
                        if (!extractedKeys.Contains(key))
                        {
                            extractedKeys.Add(key);
                        }
                    }
                }
            }

            Debug.Log($"총 {extractedKeys.Count}개의 고유 키를 찾았습니다.");
            Repaint();
        }

        /// <summary>
        /// 지정된 폴더의 모든 하위 폴더를 재귀적으로 찾습니다.
        /// </summary>
        private List<string> GetAllSubFolders(string rootFolder)
        {
            var subFolders = new List<string>();
            
            try
            {
                // AssetDatabase.GetSubFolders는 Unity 2019.4부터 사용 가능
                string[] folders = AssetDatabase.GetSubFolders(rootFolder);
                
                foreach (string folder in folders)
                {
                    subFolders.Add(folder);
                    // 재귀적으로 하위 폴더의 하위 폴더도 찾기
                    subFolders.AddRange(GetAllSubFolders(folder));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"하위 폴더 검색 중 오류 발생: {e.Message}");
            }
            
            return subFolders;
        }

        private List<string> ExtractKeysFromPrefabPath(string prefabPath)
        {
            var keys = new List<string>();
            
            // 프리팹 로드
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"프리팹을 로드할 수 없습니다: {prefabPath}");
                return keys;
            }

            // 프리팹에서 AutoLocalizedText 컴포넌트 찾기
            var autoTexts = prefab.GetComponentsInChildren<AutoLocalizedText>(true);
            
            Debug.Log($"프리팹 '{prefab.name}'에서 {autoTexts.Length}개의 AutoLocalizedText 컴포넌트를 찾았습니다.");
            
            // AutoLocalizedText 컴포넌트에서 키 추출
            foreach (var autoText in autoTexts)
            {
                string key = ExtractKeyFromAutoLocalizedText(autoText);
                if (!string.IsNullOrEmpty(key) && !keys.Contains(key))
                {
                    keys.Add(key);
                    Debug.Log($"AutoLocalizedText 키 추출: {autoText.gameObject.name} -> {key}");
                }
                else
                {
                    Debug.LogWarning($"AutoLocalizedText 키 추출 실패: {autoText.gameObject.name} (키: {key})");
                }
            }

            // AutoLocalizedText가 없는 TextMeshProUGUI 컴포넌트도 찾아서 키 생성
            var textComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            var textComponentsWithoutAutoLocalized = textComponents.Where(t => t.GetComponent<AutoLocalizedText>() == null).ToArray();
            
            if (textComponentsWithoutAutoLocalized.Length > 0)
            {
                Debug.Log($"AutoLocalizedText가 없는 TextMeshProUGUI 컴포넌트 {textComponentsWithoutAutoLocalized.Length}개 발견");
                
                foreach (var textComponent in textComponentsWithoutAutoLocalized)
                {
                    string key = GenerateKeyFromTextComponent(textComponent);
                    if (!string.IsNullOrEmpty(key) && !keys.Contains(key))
                    {
                        keys.Add(key);
                        Debug.Log($"TextMeshProUGUI 키 생성: {textComponent.gameObject.name} -> {key}");
                    }
                }
            }

            return keys;
        }

        private string ExtractKeyFromAutoLocalizedText(AutoLocalizedText autoText)
        {
            // 1. 먼저 GetLocalizationKey() 시도
            string key = autoText.GetLocalizationKey();
            if (!string.IsNullOrEmpty(key))
            {
                return key;
            }

            // 2. GameObject 이름에서 키 생성
            string gameObjectName = autoText.gameObject.name;
            
            // TextMeshProUGUI 컴포넌트가 있는지 확인
            var tmpText = autoText.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                // UI 이름 기반 키 생성 (Text 접미사 제거)
                key = gameObjectName.ToLower()
                    .Replace("text", "")
                    .Replace("_", "")
                    .Replace("-", "")
                    .Replace(" ", "")
                    .Trim();
                
                // 빈 문자열이면 원본 이름 사용
                if (string.IsNullOrEmpty(key))
                {
                    key = gameObjectName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
                }
                
                return key;
            }

            // 3. 기본적으로 GameObject 이름 사용
            return gameObjectName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
        }

        private string GenerateKeyFromTextComponent(TextMeshProUGUI textComponent)
        {
            string gameObjectName = textComponent.gameObject.name;
            
            // UI 이름 기반 키 생성 (Text 접미사 제거)
            string key = gameObjectName.ToLower()
                .Replace("text", "")
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "")
                .Trim();
            
            // 빈 문자열이면 원본 이름 사용
            if (string.IsNullOrEmpty(key))
            {
                key = gameObjectName.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
            }
            
            return key;
        }

        private void SelectPrefab(string prefabName)
        {
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }

        private void ExportKeysToCSV()
        {
            if (extractedKeys.Count == 0)
            {
                EditorUtility.DisplayDialog("알림", "내보낼 키가 없습니다.", "확인");
                return;
            }

            // 기본 CSV 파일 경로
            string defaultCsvPath = "Assets/000WorkSpaces/KYS/Scripts/UI/Localization/LanguageData.csv";
            
            // 사용자에게 선택 옵션 제공
            int choice = EditorUtility.DisplayDialogComplex(
                "CSV 내보내기 옵션",
                "어떤 방식으로 키를 내보내시겠습니까?",
                "기존 파일에 추가",
                "새 파일 생성",
                "취소"
            );

            if (choice == 2) // 취소
                return;

            string csvPath;
            if (choice == 0) // 기존 파일에 추가
            {
                csvPath = defaultCsvPath;
                if (!File.Exists(csvPath))
                {
                    EditorUtility.DisplayDialog("오류", "기본 CSV 파일을 찾을 수 없습니다.", "확인");
                    return;
                }
            }
            else // 새 파일 생성
            {
                csvPath = EditorUtility.SaveFilePanel("CSV 파일 저장", "Assets", "LocalizationKeys", "csv");
                if (string.IsNullOrEmpty(csvPath))
                    return;
            }

            try
            {
                if (choice == 0) // 기존 파일에 추가
                {
                    AddKeysToExistingCSV(csvPath);
                }
                else // 새 파일 생성
                {
                    CreateNewCSV(csvPath);
                }

                AssetDatabase.Refresh();
                Debug.Log($"CSV 파일이 업데이트되었습니다: {csvPath}");
                Debug.Log($"총 {extractedKeys.Count}개의 키가 처리되었습니다.");
                
                EditorUtility.DisplayDialog("완료", $"CSV 파일이 업데이트되었습니다.\n경로: {csvPath}\n키 개수: {extractedKeys.Count}개", "확인");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CSV 파일 처리 중 오류 발생: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"CSV 파일 처리 중 오류가 발생했습니다: {e.Message}", "확인");
            }
        }

        private void AddKeysToExistingCSV(string csvPath)
        {
            // 기존 CSV 파일 읽기
            string[] existingLines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
            var existingKeys = new HashSet<string>();
            var newLines = new List<string>();

            // 기존 키들 수집
            for (int i = 1; i < existingLines.Length; i++) // 헤더 제외
            {
                string line = existingLines[i].Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    string[] fields = ParseCSVLine(line);
                    if (fields.Length > 0 && !string.IsNullOrEmpty(fields[0]))
                    {
                        existingKeys.Add(fields[0].Trim());
                    }
                }
            }

            // 기존 라인들 추가
            newLines.AddRange(existingLines);

            // 새로운 키들 추가
            int addedCount = 0;
            foreach (string key in extractedKeys)
            {
                if (!existingKeys.Contains(key))
                {
                    // 헤더 형식에 맞춰 빈 번역 필드 추가
                    string[] headers = ParseCSVLine(existingLines[0]);
                    int languageCount = headers.Length - 1; // Key 열 제외
                    
                    var keyFields = new List<string> { key };
                    for (int i = 0; i < languageCount; i++)
                    {
                        keyFields.Add(""); // 각 언어별 빈 번역
                    }
                    string newLine = string.Join(",", keyFields);
                    newLines.Add(newLine);
                    addedCount++;
                    Debug.Log($"새 키 추가: {key}");
                }
                else
                {
                    Debug.Log($"기존 키 건너뛰기: {key}");
                }
            }

            // 파일 다시 작성
            File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
            Debug.Log($"기존 CSV에 {addedCount}개의 새 키가 추가되었습니다.");
        }

        private void CreateNewCSV(string csvPath)
        {
            var csvLines = new List<string>();
            csvLines.Add("Key,Korean,English"); // 헤더
            
            foreach (string key in extractedKeys)
            {
                csvLines.Add($"{key},,"); // 키만 추가, 번역은 빈칸
            }

            File.WriteAllLines(csvPath, csvLines.ToArray(), System.Text.Encoding.UTF8);
        }

                 private string[] ParseCSVLine(string line)
         {
             var fields = new List<string>();
             bool inQuotes = false;
             string currentField = "";

             for (int i = 0; i < line.Length; i++)
             {
                 char c = line[i];
                 
                 if (c == '"')
                 {
                     inQuotes = !inQuotes;
                 }
                 else if (c == ',' && !inQuotes)
                 {
                     fields.Add(currentField);
                     currentField = "";
                 }
                 else
                 {
                     currentField += c;
                 }
             }
             
             fields.Add(currentField);
             return fields.ToArray();
         }

         private void AddAutoLocalizedTextToPrefabs()
         {
             if (prefabKeysMap.Count == 0)
             {
                 EditorUtility.DisplayDialog("알림", "먼저 프리팹에서 키를 추출해주세요.", "확인");
                 return;
             }

             // 사용자에게 선택 옵션 제공
             int choice = EditorUtility.DisplayDialogComplex(
                 "AutoLocalizedText 추가 옵션",
                 "어떤 방식으로 AutoLocalizedText를 추가하시겠습니까?",
                 "선택된 프리팹에만 추가",
                 "모든 추출된 프리팹에 추가",
                 "취소"
             );

             if (choice == 2) // 취소
                 return;

             try
             {
                 int totalProcessed = 0;
                 int totalAdded = 0;

                 if (choice == 0) // 선택된 프리팹에만 추가
                 {
                     if (Selection.activeObject == null)
                     {
                         EditorUtility.DisplayDialog("알림", "프리팹을 선택해주세요.", "확인");
                         return;
                     }

                     string prefabPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                     if (!prefabPath.EndsWith(".prefab"))
                     {
                         EditorUtility.DisplayDialog("알림", "선택된 오브젝트가 프리팹이 아닙니다.", "확인");
                         return;
                     }

                     var result = AddAutoLocalizedTextToPrefab(prefabPath);
                     totalProcessed = 1;
                     totalAdded = result;
                 }
                 else // 모든 추출된 프리팹에 추가
                 {
                     foreach (var kvp in prefabKeysMap)
                     {
                         string prefabName = kvp.Key;
                         string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
                         
                         if (guids.Length > 0)
                         {
                             string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                             int added = AddAutoLocalizedTextToPrefab(prefabPath);
                             totalAdded += added;
                         }
                         totalProcessed++;
                     }
                 }

                 AssetDatabase.Refresh();
                 EditorUtility.DisplayDialog("완료", 
                     $"AutoLocalizedText 추가 완료!\n" +
                     $"처리된 프리팹: {totalProcessed}개\n" +
                     $"추가된 컴포넌트: {totalAdded}개", "확인");
             }
             catch (System.Exception e)
             {
                 Debug.LogError($"AutoLocalizedText 추가 중 오류 발생: {e.Message}");
                 EditorUtility.DisplayDialog("오류", $"AutoLocalizedText 추가 중 오류가 발생했습니다: {e.Message}", "확인");
             }
         }

         private int AddAutoLocalizedTextToPrefab(string prefabPath)
         {
             // 프리팹 로드
             GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
             if (prefab == null)
             {
                 Debug.LogError($"프리팹을 로드할 수 없습니다: {prefabPath}");
                 return 0;
             }

             // 프리팹 인스턴스 생성
             GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
             if (prefabInstance == null)
             {
                 Debug.LogError($"프리팹 인스턴스를 생성할 수 없습니다: {prefabPath}");
                 return 0;
             }

             int addedCount = 0;
             bool hasChanges = false;

             try
             {
                 // 모든 TextMeshProUGUI 컴포넌트 찾기
                 var textComponents = prefabInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
                 
                 foreach (var textComponent in textComponents)
                 {
                     // 이미 AutoLocalizedText가 있는지 확인
                     var existingAutoLocalized = textComponent.GetComponent<AutoLocalizedText>();
                     if (existingAutoLocalized != null)
                     {
                         Debug.Log($"이미 AutoLocalizedText가 있습니다: {textComponent.gameObject.name}");
                         continue;
                     }

                     // AutoLocalizedText 컴포넌트 추가
                     var autoLocalized = textComponent.gameObject.AddComponent<AutoLocalizedText>();
                     
                     // 초기 설정: enableAutoLocalization = false
                     var serializedObject = new UnityEditor.SerializedObject(autoLocalized);
                     var enableProperty = serializedObject.FindProperty("enableAutoLocalization");
                     if (enableProperty != null)
                     {
                         enableProperty.boolValue = false;
                         serializedObject.ApplyModifiedProperties();
                     }

                     addedCount++;
                     hasChanges = true;
                     Debug.Log($"AutoLocalizedText 추가됨: {textComponent.gameObject.name}");
                 }

                 // 변경사항이 있으면 프리팹에 적용
                 if (hasChanges)
                 {
                     PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                     Debug.Log($"프리팹 업데이트 완료: {prefabPath}");
                 }
             }
             finally
             {
                 // 인스턴스 정리
                 if (prefabInstance != null)
                 {
                     DestroyImmediate(prefabInstance);
                 }
             }

             return addedCount;
         }

         /// <summary>
         /// 번역이 있는 키만 AutoLocalization을 활성화하는 메서드
         /// </summary>
         private void EnableAutoLocalizationForTranslatedKeys()
         {
             if (prefabKeysMap.Count == 0)
             {
                 EditorUtility.DisplayDialog("알림", "먼저 프리팹에서 키를 추출해주세요.", "확인");
                 return;
             }

             // LocalizationManager 초기화 확인
             if (LocalizationManager.Instance == null)
             {
                 EditorUtility.DisplayDialog("알림", "LocalizationManager가 초기화되지 않았습니다. 게임을 실행한 후 다시 시도해주세요.", "확인");
                 return;
             }

             // 사용자에게 선택 옵션 제공
             int choice = EditorUtility.DisplayDialogComplex(
                 "AutoLocalization 활성화 옵션",
                 "어떤 방식으로 AutoLocalization을 활성화하시겠습니까?",
                 "선택된 프리팹에만 적용",
                 "모든 추출된 프리팹에 적용",
                 "취소"
             );

             if (choice == 2) // 취소
                 return;

             try
             {
                 int totalProcessed = 0;
                 int totalEnabled = 0;

                 if (choice == 0) // 선택된 프리팹에만 적용
                 {
                     if (Selection.activeObject == null)
                     {
                         EditorUtility.DisplayDialog("알림", "프리팹을 선택해주세요.", "확인");
                         return;
                     }

                     string prefabPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                     if (!prefabPath.EndsWith(".prefab"))
                     {
                         EditorUtility.DisplayDialog("알림", "선택된 오브젝트가 프리팹이 아닙니다.", "확인");
                         return;
                     }

                     var result = EnableAutoLocalizationInPrefab(prefabPath);
                     totalProcessed = 1;
                     totalEnabled = result;
                 }
                 else // 모든 추출된 프리팹에 적용
                 {
                     foreach (var kvp in prefabKeysMap)
                     {
                         string prefabName = kvp.Key;
                         string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
                         
                         if (guids.Length > 0)
                         {
                             string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                             int enabled = EnableAutoLocalizationInPrefab(prefabPath);
                             totalEnabled += enabled;
                         }
                         totalProcessed++;
                     }
                 }

                 AssetDatabase.Refresh();
                 EditorUtility.DisplayDialog("완료", 
                     $"AutoLocalization 활성화 완료!\n" +
                     $"처리된 프리팹: {totalProcessed}개\n" +
                     $"활성화된 컴포넌트: {totalEnabled}개", "확인");
             }
             catch (System.Exception e)
             {
                 Debug.LogError($"AutoLocalization 활성화 중 오류 발생: {e.Message}");
                 EditorUtility.DisplayDialog("오류", $"AutoLocalization 활성화 중 오류가 발생했습니다: {e.Message}", "확인");
             }
         }

         /// <summary>
         /// 특정 프리팹에서 번역이 있는 키의 AutoLocalization을 활성화
         /// </summary>
         private int EnableAutoLocalizationInPrefab(string prefabPath)
         {
             // 프리팹 로드
             GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
             if (prefab == null)
             {
                 Debug.LogError($"프리팹을 로드할 수 없습니다: {prefabPath}");
                 return 0;
             }

             // 프리팹 인스턴스 생성
             GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
             if (prefabInstance == null)
             {
                 Debug.LogError($"프리팹 인스턴스를 생성할 수 없습니다: {prefabPath}");
                 return 0;
             }

             int enabledCount = 0;
             bool hasChanges = false;

             try
             {
                 // 모든 AutoLocalizedText 컴포넌트 찾기
                 var autoLocalizedComponents = prefabInstance.GetComponentsInChildren<AutoLocalizedText>(true);
                 
                 foreach (var autoLocalized in autoLocalizedComponents)
                 {
                     // 현재 키 가져오기
                     string currentKey = autoLocalized.GetLocalizationKey();
                     
                     if (string.IsNullOrEmpty(currentKey))
                     {
                         Debug.LogWarning($"AutoLocalizedText에 키가 없습니다: {autoLocalized.gameObject.name}");
                         continue;
                     }

                     // 번역이 있는지 확인 (한국어 기준)
                     bool hasTranslation = LocalizationManager.Instance.HasTranslation(currentKey, SystemLanguage.Korean);
                     
                     // 동적 데이터 키인지 확인
                     bool isDynamicKey = IsDynamicDataKey(currentKey);
                     
                     if (hasTranslation && !isDynamicKey)
                     {
                         // enableAutoLocalization을 true로 설정
                         var serializedObject = new UnityEditor.SerializedObject(autoLocalized);
                         var enableProperty = serializedObject.FindProperty("enableAutoLocalization");
                         
                         if (enableProperty != null && !enableProperty.boolValue)
                         {
                             enableProperty.boolValue = true;
                             serializedObject.ApplyModifiedProperties();
                             
                             enabledCount++;
                             hasChanges = true;
                             Debug.Log($"AutoLocalization 활성화됨: {autoLocalized.gameObject.name} (키: {currentKey})");
                         }
                     }
                     else if (isDynamicKey)
                     {
                         Debug.Log($"동적 데이터 키로 인해 AutoLocalization 비활성화 유지: {autoLocalized.gameObject.name} (키: {currentKey})");
                     }
                     else
                     {
                         Debug.Log($"번역이 없어서 AutoLocalization 비활성화 유지: {autoLocalized.gameObject.name} (키: {currentKey})");
                     }
                 }

                 // 변경사항이 있으면 프리팹에 적용
                 if (hasChanges)
                 {
                     PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                     Debug.Log($"프리팹 업데이트 완료: {prefabPath}");
                 }
             }
             finally
             {
                 // 인스턴스 정리
                 if (prefabInstance != null)
                 {
                     DestroyImmediate(prefabInstance);
                 }
             }

                          return enabledCount;
         }

         /// <summary>
         /// 번역이 없는 키와 동적 데이터 키의 AutoLocalization을 비활성화하는 메서드
         /// </summary>
         private void DisableAutoLocalizationForUntranslatedKeys()
         {
             if (prefabKeysMap.Count == 0)
             {
                 EditorUtility.DisplayDialog("알림", "먼저 프리팹에서 키를 추출해주세요.", "확인");
                 return;
             }

             // LocalizationManager 초기화 확인
             if (LocalizationManager.Instance == null)
             {
                 EditorUtility.DisplayDialog("알림", "LocalizationManager가 초기화되지 않았습니다. 게임을 실행한 후 다시 시도해주세요.", "확인");
                 return;
             }

             // 사용자에게 선택 옵션 제공
             int choice = EditorUtility.DisplayDialogComplex(
                 "AutoLocalization 비활성화 옵션",
                 "번역이 없는 키와 동적 데이터 키({0}, {1} 등)의 AutoLocalization을 비활성화하시겠습니까?",
                 "선택된 프리팹에만 적용",
                 "모든 추출된 프리팹에 적용",
                 "취소"
             );

             if (choice == 2) // 취소
                 return;

             try
             {
                 int totalProcessed = 0;
                 int totalDisabled = 0;

                 if (choice == 0) // 선택된 프리팹에만 적용
                 {
                     if (Selection.activeObject == null)
                     {
                         EditorUtility.DisplayDialog("알림", "프리팹을 선택해주세요.", "확인");
                         return;
                     }

                     string prefabPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                     if (!prefabPath.EndsWith(".prefab"))
                     {
                         EditorUtility.DisplayDialog("알림", "선택된 오브젝트가 프리팹이 아닙니다.", "확인");
                         return;
                     }

                     var result = DisableAutoLocalizationInPrefab(prefabPath);
                     totalProcessed = 1;
                     totalDisabled = result;
                 }
                 else // 모든 추출된 프리팹에 적용
                 {
                     foreach (var kvp in prefabKeysMap)
                     {
                         string prefabName = kvp.Key;
                         string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
                         
                         if (guids.Length > 0)
                         {
                             string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                             int disabled = DisableAutoLocalizationInPrefab(prefabPath);
                             totalDisabled += disabled;
                         }
                         totalProcessed++;
                     }
                 }

                 AssetDatabase.Refresh();
                 EditorUtility.DisplayDialog("완료", 
                     $"AutoLocalization 비활성화 완료!\n" +
                     $"처리된 프리팹: {totalProcessed}개\n" +
                     $"비활성화된 컴포넌트: {totalDisabled}개", "확인");
             }
             catch (System.Exception e)
             {
                 Debug.LogError($"AutoLocalization 비활성화 중 오류 발생: {e.Message}");
                 EditorUtility.DisplayDialog("오류", $"AutoLocalization 비활성화 중 오류가 발생했습니다: {e.Message}", "확인");
             }
         }

         /// <summary>
         /// 특정 프리팹에서 번역이 없는 키의 AutoLocalization을 비활성화
         /// </summary>
         private int DisableAutoLocalizationInPrefab(string prefabPath)
         {
             // 프리팹 로드
             GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
             if (prefab == null)
             {
                 Debug.LogError($"프리팹을 로드할 수 없습니다: {prefabPath}");
                 return 0;
             }

             // 프리팹 인스턴스 생성
             GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
             if (prefabInstance == null)
             {
                 Debug.LogError($"프리팹 인스턴스를 생성할 수 없습니다: {prefabPath}");
                 return 0;
             }

             int disabledCount = 0;
             bool hasChanges = false;

             try
             {
                 // 모든 AutoLocalizedText 컴포넌트 찾기
                 var autoLocalizedComponents = prefabInstance.GetComponentsInChildren<AutoLocalizedText>(true);
                 
                 foreach (var autoLocalized in autoLocalizedComponents)
                 {
                     // 현재 키 가져오기
                     string currentKey = autoLocalized.GetLocalizationKey();
                     
                     if (string.IsNullOrEmpty(currentKey))
                     {
                         Debug.LogWarning($"AutoLocalizedText에 키가 없습니다: {autoLocalized.gameObject.name}");
                         continue;
                     }

                     // 번역이 없는지 확인 (한국어 기준)
                     bool hasTranslation = LocalizationManager.Instance.HasTranslation(currentKey, SystemLanguage.Korean);
                     
                     // 동적 데이터 키인지 확인
                     bool isDynamicKey = IsDynamicDataKey(currentKey);
                     
                     if (!hasTranslation || isDynamicKey)
                     {
                         // enableAutoLocalization을 false로 설정
                         var serializedObject = new UnityEditor.SerializedObject(autoLocalized);
                         var enableProperty = serializedObject.FindProperty("enableAutoLocalization");
                         
                         if (enableProperty != null && enableProperty.boolValue)
                         {
                             enableProperty.boolValue = false;
                             serializedObject.ApplyModifiedProperties();
                             
                             disabledCount++;
                             hasChanges = true;
                             
                             if (isDynamicKey)
                             {
                                 Debug.Log($"동적 데이터 키로 인해 AutoLocalization 비활성화됨: {autoLocalized.gameObject.name} (키: {currentKey})");
                             }
                             else
                             {
                                 Debug.Log($"번역이 없어서 AutoLocalization 비활성화됨: {autoLocalized.gameObject.name} (키: {currentKey})");
                             }
                         }
                     }
                     else
                     {
                         Debug.Log($"번역이 있어서 AutoLocalization 활성화 유지: {autoLocalized.gameObject.name} (키: {currentKey})");
                     }
                 }

                 // 변경사항이 있으면 프리팹에 적용
                 if (hasChanges)
                 {
                     PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                     Debug.Log($"프리팹 업데이트 완료: {prefabPath}");
                 }
             }
             finally
             {
                 // 인스턴스 정리
                 if (prefabInstance != null)
                 {
                     DestroyImmediate(prefabInstance);
                 }
             }

             return disabledCount;
         }

         /// <summary>
         /// 동적 데이터 키인지 확인하는 메서드
         /// </summary>
         private bool IsDynamicDataKey(string key)
         {
             // 플레이스홀더가 포함된 키들
             string[] dynamicPatterns = {
                 "upgrademoneynum", "beforeupgradenum", "afterupgradenum", 
                 "moneynum", "workerlevenum", "upgrademoneynum1", "upgrademoneynum2",
                 "beforeupgradenum1", "beforeupgradenum2", "afterupgradenum1", "afterupgradenum2"
             };
             
             foreach (string pattern in dynamicPatterns)
             {
                 if (key.Contains(pattern))
                     return true;
             }
             
             return false;
         }

         /// <summary>
         /// 번역 상태를 확인하고 보고하는 메서드
         /// </summary>
         private void CheckTranslationStatus()
         {
             if (prefabKeysMap.Count == 0)
             {
                 EditorUtility.DisplayDialog("알림", "먼저 프리팹에서 키를 추출해주세요.", "확인");
                 return;
             }

             // LocalizationManager 초기화 확인
             if (LocalizationManager.Instance == null)
             {
                 EditorUtility.DisplayDialog("알림", "LocalizationManager가 초기화되지 않았습니다. 게임을 실행한 후 다시 시도해주세요.", "확인");
                 return;
             }

             var translatedKeys = new List<string>();
             var untranslatedKeys = new List<string>();
             var autoLocalizedEnabled = new List<string>();
             var autoLocalizedDisabled = new List<string>();

             // 모든 추출된 키에 대해 번역 상태 확인
             foreach (var key in extractedKeys)
             {
                 bool hasTranslation = LocalizationManager.Instance.HasTranslation(key, SystemLanguage.Korean);
                 
                 if (hasTranslation)
                 {
                     translatedKeys.Add(key);
                 }
                 else
                 {
                     untranslatedKeys.Add(key);
                 }
             }

             // 프리팹에서 AutoLocalizedText 상태 확인
             foreach (var kvp in prefabKeysMap)
             {
                 string prefabName = kvp.Key;
                 string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
                 
                 if (guids.Length > 0)
                 {
                     string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                     CheckAutoLocalizedTextStatusInPrefab(prefabPath, autoLocalizedEnabled, autoLocalizedDisabled);
                 }
             }

             // 결과 보고
             string report = $"📊 번역 상태 보고서\n\n";
             report += $"총 추출된 키: {extractedKeys.Count}개\n";
             report += $"번역된 키: {translatedKeys.Count}개\n";
             report += $"번역되지 않은 키: {untranslatedKeys.Count}개\n";
             report += $"번역 완성도: {(float)translatedKeys.Count / extractedKeys.Count * 100:F1}%\n\n";
             
             report += $"AutoLocalization 활성화된 키: {autoLocalizedEnabled.Count}개\n";
             report += $"AutoLocalization 비활성화된 키: {autoLocalizedDisabled.Count}개\n\n";

             if (untranslatedKeys.Count > 0)
             {
                 report += $"⚠️ 번역되지 않은 키들:\n";
                 foreach (var key in untranslatedKeys.Take(10)) // 최대 10개만 표시
                 {
                     report += $"  - {key}\n";
                 }
                 if (untranslatedKeys.Count > 10)
                 {
                     report += $"  ... 외 {untranslatedKeys.Count - 10}개\n";
                 }
                 report += "\n";
             }

             if (autoLocalizedDisabled.Count > 0)
             {
                 report += $"🔧 AutoLocalization 비활성화된 키들:\n";
                 foreach (var key in autoLocalizedDisabled.Take(10)) // 최대 10개만 표시
                 {
                     report += $"  - {key}\n";
                 }
                 if (autoLocalizedDisabled.Count > 10)
                 {
                     report += $"  ... 외 {autoLocalizedDisabled.Count - 10}개\n";
                 }
             }

             EditorUtility.DisplayDialog("번역 상태 보고서", report, "확인");
             
             // 콘솔에도 출력
             Debug.Log($"[PrefabLocalizationKeyExtractor] 번역 상태 보고서:\n{report}");
         }

         /// <summary>
         /// 특정 프리팹에서 AutoLocalizedText 상태를 확인
         /// </summary>
         private void CheckAutoLocalizedTextStatusInPrefab(string prefabPath, List<string> enabledKeys, List<string> disabledKeys)
         {
             // 프리팹 로드
             GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
             if (prefab == null) return;

             // 프리팹 인스턴스 생성
             GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
             if (prefabInstance == null) return;

             try
             {
                 // 모든 AutoLocalizedText 컴포넌트 찾기
                 var autoLocalizedComponents = prefabInstance.GetComponentsInChildren<AutoLocalizedText>(true);
                 
                 foreach (var autoLocalized in autoLocalizedComponents)
                 {
                     string currentKey = autoLocalized.GetLocalizationKey();
                     
                     if (!string.IsNullOrEmpty(currentKey))
                     {
                         var serializedObject = new UnityEditor.SerializedObject(autoLocalized);
                         var enableProperty = serializedObject.FindProperty("enableAutoLocalization");
                         
                         if (enableProperty != null)
                         {
                             if (enableProperty.boolValue)
                             {
                                 if (!enabledKeys.Contains(currentKey))
                                     enabledKeys.Add(currentKey);
                             }
                             else
                             {
                                 if (!disabledKeys.Contains(currentKey))
                                     disabledKeys.Add(currentKey);
                             }
                         }
                     }
                 }
             }
             finally
             {
                 // 인스턴스 정리
                 if (prefabInstance != null)
                 {
                     DestroyImmediate(prefabInstance);
                 }
             }
         }
     }
 }
 #endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KYS;

namespace KYS.Editor
{
    /// <summary>
    /// 로컬라이제이션 키를 관리하는 에디터 도구
    /// </summary>
    public class LocalizationKeyManager : EditorWindow
    {
        private Vector2 scrollPosition;
        private string csvPath = "Assets/000WorkSpaces/KYS/Csvs/LanguageData.csv";
        private List<string> allKeys = new List<string>();
        private List<string> missingKeys = new List<string>();
        private List<string> duplicateKeys = new List<string>();
        private Dictionary<string, bool> keyStatus = new Dictionary<string, bool>();

        [MenuItem("KYS/로컬라이제이션 키 관리자")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationKeyManager>("로컬라이제이션 키 관리자");
        }

        private void OnEnable()
        {
            LoadCSVData();
        }

        private void OnGUI()
        {
            GUILayout.Label("로컬라이제이션 키 관리자", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            // CSV 파일 경로
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("CSV 파일:", GUILayout.Width(80));
            csvPath = EditorGUILayout.TextField(csvPath);
            if (GUILayout.Button("찾기", GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFilePanel("CSV 파일 선택", "Assets", "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    csvPath = path;
                    LoadCSVData();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 버튼들
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("CSV 데이터 새로고침"))
            {
                LoadCSVData();
            }
            
            if (GUILayout.Button("중복 키 검사"))
            {
                CheckDuplicateKeys();
            }
            
            if (GUILayout.Button("누락된 키 검사"))
            {
                CheckMissingKeys();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("프리팹에서 누락된 키 자동 추가"))
            {
                AutoAddMissingKeysFromPrefabs();
            }
            
            if (GUILayout.Button("중복 키 제거"))
            {
                RemoveDuplicateKeys();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("CSV 백업 생성"))
            {
                CreateCSVBackup();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 통계 정보
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("📊 통계 정보", EditorStyles.boldLabel);
            GUILayout.Label($"총 키 개수: {allKeys.Count}");
            GUILayout.Label($"중복 키 개수: {duplicateKeys.Count}");
            GUILayout.Label($"누락된 키 개수: {missingKeys.Count}");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 결과 표시
            if (allKeys.Count > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                // 중복 키 표시
                if (duplicateKeys.Count > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    GUILayout.Label("⚠️ 중복된 키들", EditorStyles.boldLabel);
                    foreach (string key in duplicateKeys)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label($"🔴 {key}");
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(10);
                }
                
                // 누락된 키 표시
                if (missingKeys.Count > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    GUILayout.Label("❌ 누락된 키들", EditorStyles.boldLabel);
                    foreach (string key in missingKeys)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label($"🟡 {key}");
                        if (GUILayout.Button("추가", GUILayout.Width(50)))
                        {
                            AddKeyToCSV(key);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(10);
                }
                
                // 모든 키 표시
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("📋 모든 키들", EditorStyles.boldLabel);
                foreach (string key in allKeys)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    bool hasTranslation = keyStatus.ContainsKey(key) && keyStatus[key];
                    GUILayout.Label($"{(hasTranslation ? "🟢" : "🔴")} {key}");
                    if (GUILayout.Button("삭제", GUILayout.Width(50)))
                    {
                        RemoveKeyFromCSV(key);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndScrollView();
            }
        }

        private void LoadCSVData()
        {
            if (!File.Exists(csvPath))
            {
                Debug.LogError($"CSV 파일을 찾을 수 없습니다: {csvPath}");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                if (lines.Length < 2)
                {
                    Debug.LogWarning("CSV 파일이 비어있거나 헤더만 있습니다.");
                    return;
                }

                allKeys.Clear();
                keyStatus.Clear();

                // 헤더 건너뛰기
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] fields = ParseCSVLine(line);
                    if (fields.Length > 0)
                    {
                        string key = fields[0];
                        if (!string.IsNullOrEmpty(key))
                        {
                            allKeys.Add(key);
                            
                            // 번역 존재 여부 확인 (한국어 또는 영어 번역이 있으면 true)
                            bool hasTranslation = fields.Length > 1 && !string.IsNullOrEmpty(fields[1]) ||
                                                 fields.Length > 2 && !string.IsNullOrEmpty(fields[2]);
                            keyStatus[key] = hasTranslation;
                        }
                    }
                }

                Debug.Log($"CSV에서 {allKeys.Count}개의 키를 로드했습니다.");
                CheckDuplicateKeys();
                CheckMissingKeys();
                Repaint();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CSV 파일 로드 중 오류 발생: {e.Message}");
            }
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

        private void CheckDuplicateKeys()
        {
            duplicateKeys.Clear();
            var keyCount = new Dictionary<string, int>();

            foreach (string key in allKeys)
            {
                if (keyCount.ContainsKey(key))
                {
                    keyCount[key]++;
                    if (keyCount[key] == 2) // 첫 번째 중복 발견
                    {
                        duplicateKeys.Add(key);
                    }
                }
                else
                {
                    keyCount[key] = 1;
                }
            }

            Debug.Log($"중복 키 검사 완료: {duplicateKeys.Count}개의 중복 키 발견");
        }

        private void CheckMissingKeys()
        {
            missingKeys.Clear();
            
            // 프리팹에서 키 추출
            var prefabKeys = ExtractKeysFromAllPrefabs();
            
            foreach (string prefabKey in prefabKeys)
            {
                if (!allKeys.Contains(prefabKey))
                {
                    missingKeys.Add(prefabKey);
                }
            }

            Debug.Log($"누락된 키 검사 완료: {missingKeys.Count}개의 누락된 키 발견");
        }

        private List<string> ExtractKeysFromAllPrefabs()
        {
            var keys = new List<string>();
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab");
            
            foreach (string guid in prefabPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/UI/") || path.Contains("/Prefabs/UI/"))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        var autoTexts = prefab.GetComponentsInChildren<AutoLocalizedText>(true);
                        foreach (var autoText in autoTexts)
                        {
                            string key = autoText.GetLocalizationKey();
                            if (!string.IsNullOrEmpty(key) && !keys.Contains(key))
                            {
                                keys.Add(key);
                            }
                        }
                    }
                }
            }

            return keys;
        }

        private void AutoAddMissingKeysFromPrefabs()
        {
            CheckMissingKeys();
            
            if (missingKeys.Count == 0)
            {
                EditorUtility.DisplayDialog("알림", "추가할 키가 없습니다.", "확인");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                var newLines = new List<string>(lines);

                foreach (string key in missingKeys)
                {
                    string[] headers = ParseCSVLine(lines[0]);
                    int languageCount = headers.Length - 1; // Key 열 제외
                    
                    var keyFields = new List<string> { key };
                    for (int i = 0; i < languageCount; i++)
                    {
                        keyFields.Add(""); // 각 언어별 빈 번역
                    }
                    string newLine = string.Join(",", keyFields);
                    newLines.Add(newLine);
                }

                File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                AssetDatabase.Refresh();
                
                Debug.Log($"{missingKeys.Count}개의 키가 CSV에 추가되었습니다.");
                EditorUtility.DisplayDialog("완료", $"{missingKeys.Count}개의 키가 CSV에 추가되었습니다.", "확인");
                
                LoadCSVData();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"키 추가 중 오류 발생: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"키 추가 중 오류가 발생했습니다: {e.Message}", "확인");
            }
        }

        private void AddKeyToCSV(string key)
        {
            try
            {
                string[] lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                string[] headers = ParseCSVLine(lines[0]);
                int languageCount = headers.Length - 1;
                
                var keyFields = new List<string> { key };
                for (int i = 0; i < languageCount; i++)
                {
                    keyFields.Add("");
                }
                string newLine = string.Join(",", keyFields);
                
                var newLines = new List<string>(lines) { newLine };
                File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                AssetDatabase.Refresh();
                
                Debug.Log($"키 '{key}'가 CSV에 추가되었습니다.");
                LoadCSVData();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"키 추가 중 오류 발생: {e.Message}");
            }
        }

        private void RemoveKeyFromCSV(string key)
        {
            if (EditorUtility.DisplayDialog("확인", $"키 '{key}'를 삭제하시겠습니까?", "삭제", "취소"))
            {
                try
                {
                    string[] lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                    var newLines = new List<string>();
                    
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] fields = ParseCSVLine(lines[i]);
                        if (fields.Length > 0 && fields[0] != key)
                        {
                            newLines.Add(lines[i]);
                        }
                    }
                    
                    File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                    AssetDatabase.Refresh();
                    
                    Debug.Log($"키 '{key}'가 CSV에서 삭제되었습니다.");
                    LoadCSVData();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"키 삭제 중 오류 발생: {e.Message}");
                }
            }
        }

        private void RemoveDuplicateKeys()
        {
            if (duplicateKeys.Count == 0)
            {
                EditorUtility.DisplayDialog("알림", "중복된 키가 없습니다.", "확인");
                return;
            }

            if (!EditorUtility.DisplayDialog("확인", 
                $"중복된 키 {duplicateKeys.Count}개를 제거하시겠습니까?\n\n" +
                "이 작업은 중복된 키 중 첫 번째 키만 남기고 나머지를 삭제합니다.\n" +
                "작업 전에 자동으로 백업이 생성됩니다.", 
                "제거", "취소"))
            {
                return;
            }

            try
            {
                // 먼저 백업 생성
                CreateCSVBackup();
                
                string[] lines = File.ReadAllLines(csvPath, System.Text.Encoding.UTF8);
                if (lines.Length < 2)
                {
                    EditorUtility.DisplayDialog("오류", "CSV 파일에 데이터가 없습니다.", "확인");
                    return;
                }

                // 헤더와 데이터 분리
                string headerLine = lines[0];
                var dataLines = new List<string>(lines.Skip(1));

                // 중복 키 찾기 및 제거
                var processedKeys = new HashSet<string>();
                var newDataLines = new List<string>();
                var removedKeys = new List<string>();

                foreach (string line in dataLines)
                {
                    if (string.IsNullOrEmpty(line.Trim())) continue;

                    string[] fields = ParseCSVLine(line);
                    if (fields.Length > 0)
                    {
                        string key = fields[0].Trim();
                        if (!string.IsNullOrEmpty(key))
                        {
                            if (!processedKeys.Contains(key))
                            {
                                newDataLines.Add(line);
                                processedKeys.Add(key);
                            }
                            else
                            {
                                removedKeys.Add(key);
                                Debug.Log($"중복 키 제거: {key}");
                            }
                        }
                    }
                }

                // 새로운 CSV 파일 작성
                var newLines = new List<string> { headerLine };
                newLines.AddRange(newDataLines);

                File.WriteAllLines(csvPath, newLines.ToArray(), System.Text.Encoding.UTF8);
                AssetDatabase.Refresh();

                Debug.Log($"중복 키 제거 완료. {removedKeys.Count}개의 중복 라인 제거됨");
                EditorUtility.DisplayDialog("완료", 
                    $"중복 키 제거가 완료되었습니다.\n\n" +
                    $"제거된 중복 키: {removedKeys.Count}개\n" +
                    $"남은 키: {newDataLines.Count}개", 
                    "확인");

                // 데이터 다시 로드
                LoadCSVData();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"중복 키 제거 중 오류 발생: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"중복 키 제거 중 오류가 발생했습니다: {e.Message}", "확인");
            }
        }

        private void CreateCSVBackup()
        {
            try
            {
                string backupPath = csvPath.Replace(".csv", $"_backup_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
                File.Copy(csvPath, backupPath);
                AssetDatabase.Refresh();
                
                Debug.Log($"CSV 백업이 생성되었습니다: {backupPath}");
                EditorUtility.DisplayDialog("완료", $"CSV 백업이 생성되었습니다.\n경로: {backupPath}", "확인");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"백업 생성 중 오류 발생: {e.Message}");
                EditorUtility.DisplayDialog("오류", $"백업 생성 중 오류가 발생했습니다: {e.Message}", "확인");
            }
        }
    }
}
#endif

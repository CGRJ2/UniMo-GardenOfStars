#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KYS;

namespace KYS.Editor
{
    /// <summary>
    /// UI 인스펙터에서 로컬라이제이션 키를 확인하고 관리하는 헬퍼
    /// </summary>
    [CustomEditor(typeof(BaseUI), true)]
    public class UIInspectorHelper : UnityEditor.Editor
    {
        private bool showLocalizationInfo = false;
        private bool showUIElementInfo = false;
        private Vector2 scrollPosition;

        public override void OnInspectorGUI()
        {
            // 기본 인스펙터 그리기
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            // 구분선
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // KYS UI 헬퍼 섹션
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("🔧 KYS UI 헬퍼", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            // 로컬라이제이션 정보
            showLocalizationInfo = EditorGUILayout.Foldout(showLocalizationInfo, "📝 로컬라이제이션 정보");
            if (showLocalizationInfo)
            {
                DrawLocalizationInfo();
            }
            
            EditorGUILayout.Space();
            
            // UI 요소 정보
            showUIElementInfo = EditorGUILayout.Foldout(showUIElementInfo, "🎯 UI 요소 정보");
            if (showUIElementInfo)
            {
                DrawUIElementInfo();
            }
            
            EditorGUILayout.Space();
            
            // 액션 버튼들
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🔍 UI 요소 새로고침"))
            {
                RefreshUIElements();
            }
            
            if (GUILayout.Button("📋 로컬라이제이션 키 출력"))
            {
                PrintLocalizationKeys();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("➕ 누락된 키 CSV에 추가"))
            {
                AddMissingKeysToCSV();
            }
            
            if (GUILayout.Button("🔄 AutoLocalization 설정"))
            {
                SetupAutoLocalization();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawLocalizationInfo()
        {
            BaseUI baseUI = (BaseUI)target;
            
            EditorGUILayout.BeginVertical("box");
            
            // AutoLocalization 키들
            string[] autoKeys = baseUI.GetAutoLocalizeKeys();
            GUILayout.Label($"AutoLocalization 키들 ({autoKeys.Length}개):", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
            
            foreach (string key in autoKeys)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                
                bool hasTranslation = LocalizationManager.Instance != null && 
                                    LocalizationManager.Instance.HasTranslation(key);
                
                GUILayout.Label($"{(hasTranslation ? "🟢" : "🔴")} {key}");
                
                if (GUILayout.Button("CSV에 추가", GUILayout.Width(80)))
                {
                    AddKeyToCSV(key);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawUIElementInfo()
        {
            BaseUI baseUI = (BaseUI)target;
            
            EditorGUILayout.BeginVertical("box");
            
            // UI 요소들
            string[] uiNames = baseUI.GetAllUINames();
            GUILayout.Label($"UI 요소들 ({uiNames.Length}개):", EditorStyles.boldLabel);
            
            foreach (string uiName in uiNames)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                
                GameObject uiObject = baseUI.GetUI(uiName);
                bool hasAutoLocalizedText = uiObject != null && 
                                          uiObject.GetComponent<AutoLocalizedText>() != null;
                
                GUILayout.Label($"{(hasAutoLocalizedText ? "🔤" : "📄")} {uiName}");
                
                if (uiObject != null)
                {
                    if (GUILayout.Button("선택", GUILayout.Width(50)))
                    {
                        Selection.activeGameObject = uiObject;
                        EditorGUIUtility.PingObject(uiObject);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void RefreshUIElements()
        {
            BaseUI baseUI = (BaseUI)target;
            baseUI.RefreshCache();
            Debug.Log($"[UIInspectorHelper] {baseUI.name}의 UI 요소 캐시를 새로고침했습니다.");
        }

        private void PrintLocalizationKeys()
        {
            BaseUI baseUI = (BaseUI)target;
            
            Debug.Log($"[UIInspectorHelper] === {baseUI.name} 로컬라이제이션 키 정보 ===");
            
            // AutoLocalization 키들
            string[] autoKeys = baseUI.GetAutoLocalizeKeys();
            Debug.Log($"[UIInspectorHelper] AutoLocalization 키들 ({autoKeys.Length}개):");
            foreach (string key in autoKeys)
            {
                bool hasTranslation = LocalizationManager.Instance != null && 
                                    LocalizationManager.Instance.HasTranslation(key);
                Debug.Log($"  - {key} (번역 존재: {hasTranslation})");
            }
            
            // UI 요소에서 AutoLocalizedText 키들
            string[] uiNames = baseUI.GetAllUINames();
            var autoLocalizedKeys = new System.Collections.Generic.List<string>();
            
            foreach (string uiName in uiNames)
            {
                GameObject uiObject = baseUI.GetUI(uiName);
                if (uiObject != null)
                {
                    var autoText = uiObject.GetComponent<AutoLocalizedText>();
                    if (autoText != null)
                    {
                        string key = autoText.GetLocalizationKey();
                        if (!string.IsNullOrEmpty(key))
                        {
                            autoLocalizedKeys.Add(key);
                        }
                    }
                }
            }
            
            Debug.Log($"[UIInspectorHelper] AutoLocalizedText 키들 ({autoLocalizedKeys.Count}개):");
            foreach (string key in autoLocalizedKeys)
            {
                bool hasTranslation = LocalizationManager.Instance != null && 
                                    LocalizationManager.Instance.HasTranslation(key);
                Debug.Log($"  - {key} (번역 존재: {hasTranslation})");
            }
        }

        private void AddMissingKeysToCSV()
        {
            BaseUI baseUI = (BaseUI)target;
            
            if (LocalizationManager.Instance == null)
            {
                Debug.LogError("[UIInspectorHelper] LocalizationManager가 초기화되지 않았습니다.");
                return;
            }
            
            // 누락된 키 찾기
            var missingKeys = new System.Collections.Generic.List<string>();
            
            // AutoLocalization 키들 확인
            string[] autoKeys = baseUI.GetAutoLocalizeKeys();
            foreach (string key in autoKeys)
            {
                if (!LocalizationManager.Instance.HasTranslation(key))
                {
                    missingKeys.Add(key);
                }
            }
            
            // AutoLocalizedText 키들 확인
            string[] uiNames = baseUI.GetAllUINames();
            foreach (string uiName in uiNames)
            {
                GameObject uiObject = baseUI.GetUI(uiName);
                if (uiObject != null)
                {
                    var autoText = uiObject.GetComponent<AutoLocalizedText>();
                    if (autoText != null)
                    {
                        string key = autoText.GetLocalizationKey();
                        if (!string.IsNullOrEmpty(key) && !LocalizationManager.Instance.HasTranslation(key))
                        {
                            missingKeys.Add(key);
                        }
                    }
                }
            }
            
            if (missingKeys.Count == 0)
            {
                Debug.Log("[UIInspectorHelper] 누락된 키가 없습니다.");
                return;
            }
            
            // CSV에 추가
            LocalizationManager.Instance.AutoAddMissingKeysForPopup(baseUI);
            
            Debug.Log($"[UIInspectorHelper] {missingKeys.Count}개의 누락된 키를 CSV에 추가했습니다.");
        }

        private void AddKeyToCSV(string key)
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.UpdateTranslationInCSV(key, SystemLanguage.Korean, "");
                Debug.Log($"[UIInspectorHelper] 키 '{key}'를 CSV에 추가했습니다.");
            }
        }

        private void SetupAutoLocalization()
        {
            BaseUI baseUI = (BaseUI)target;
            
            // AutoLocalizedText가 없는 UI 요소들에 추가
            string[] uiNames = baseUI.GetAllUINames();
            int addedCount = 0;
            
            foreach (string uiName in uiNames)
            {
                GameObject uiObject = baseUI.GetUI(uiName);
                if (uiObject != null)
                {
                    var textComponent = uiObject.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        var autoText = uiObject.GetComponent<AutoLocalizedText>();
                        if (autoText == null)
                        {
                            autoText = Undo.AddComponent<AutoLocalizedText>(uiObject);
                            addedCount++;
                        }
                    }
                }
            }
            
            Debug.Log($"[UIInspectorHelper] {addedCount}개의 UI 요소에 AutoLocalizedText를 추가했습니다.");
        }
    }

    /// <summary>
    /// AutoLocalizedText 컴포넌트용 커스텀 인스펙터
    /// </summary>
    [CustomEditor(typeof(AutoLocalizedText))]
    public class AutoLocalizedTextInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 기본 인스펙터 그리기
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            // 구분선
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // AutoLocalizedText 헬퍼
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("🔤 AutoLocalizedText 헬퍼", EditorStyles.boldLabel);
            
            AutoLocalizedText autoText = (AutoLocalizedText)target;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("현재 키:", GUILayout.Width(80));
            string currentKey = autoText.GetLocalizationKey();
            GUILayout.Label(currentKey, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("현재 텍스트:", GUILayout.Width(80));
            string currentText = autoText.GetCurrentText();
            GUILayout.Label(currentText);
            EditorGUILayout.EndHorizontal();
            
            bool hasTranslation = LocalizationManager.Instance != null && 
                                LocalizationManager.Instance.HasTranslation(currentKey);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("번역 존재:", GUILayout.Width(80));
            GUILayout.Label(hasTranslation ? "🟢 예" : "🔴 아니오");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🔄 텍스트 새로고침"))
            {
                autoText.SetAutoLocalizationEnabled(true);
            }
            
            if (GUILayout.Button("📋 키를 CSV에 추가"))
            {
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.UpdateTranslationInCSV(currentKey, SystemLanguage.Korean, "");
                    Debug.Log($"[AutoLocalizedTextInspector] 키 '{currentKey}'를 CSV에 추가했습니다.");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
    }
}
#endif

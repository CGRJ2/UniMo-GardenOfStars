using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace KYS
{
    /// <summary>
    /// 선택지 패널 - 대화 중 선택 옵션 표시
    /// </summary>
    public class ChoicePanel : BaseUI
    {
        [Header("UI Element Names")]
        [SerializeField] private string choiceContentName = "ChoiceContent";
        [SerializeField] private string choiceButtonPrefabName = "ChoiceButton";
        
        // UI 요소 참조
        private Transform choiceContent => GetUI<Transform>(choiceContentName);
        
        [Header("Choice Settings")]
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private int maxChoices = 4;
        
        // 선택지 관리
        private Action<int> onChoiceSelected;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 선택지 버튼 프리팹 자동 찾기
            if (choiceButtonPrefab == null)
            {
                var prefabButton = GetUI<Button>(choiceButtonPrefabName);
                if (prefabButton != null)
                {
                    choiceButtonPrefab = prefabButton.gameObject;
                    prefabButton.gameObject.SetActive(false); // 템플릿은 숨김
                }
            }
        }
        
        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "choice_select_prompt",
                "choice_option_1",
                "choice_option_2", 
                "choice_option_3",
                "choice_option_4"
            };
        }
        
        public override void Initialize()
        {
            base.Initialize();
            ClearChoices();
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            ClearChoices();
            onChoiceSelected = null;
        }
        
        /// <summary>
        /// 선택지 설정
        /// </summary>
        public void SetupChoices(string[] choices, Action<int> onSelected)
        {
            if (choices == null || choices.Length == 0)
            {
                Debug.LogWarning("[ChoicePanel] 선택지가 비어있습니다.");
                return;
            }
            
            onChoiceSelected = onSelected;
            ClearChoices();
            
            for (int i = 0; i < Mathf.Min(choices.Length, maxChoices); i++)
            {
                CreateChoiceButton(choices[i], i);
            }
        }
        
        /// <summary>
        /// 선택지 버튼 생성
        /// </summary>
        private void CreateChoiceButton(string choiceText, int index)
        {
            if (choiceButtonPrefab == null || choiceContent == null)
            {
                Debug.LogError("[ChoicePanel] choiceButtonPrefab 또는 choiceContent가 설정되지 않았습니다.");
                return;
            }
            
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContent);
            buttonObj.SetActive(true);
            
            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("[ChoicePanel] 버튼 프리팹에 Button 컴포넌트가 없습니다.");
                return;
            }
            
            // 버튼 텍스트 설정
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choiceText;
            }
            
            // 버튼 이벤트 설정
            int choiceIndex = index;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnChoiceButtonClicked(choiceIndex));
        }
        
        /// <summary>
        /// 선택지 클리어
        /// </summary>
        public void ClearChoices()
        {
            if (choiceContent != null)
            {
                foreach (Transform child in choiceContent)
                {
                    if (child.gameObject != choiceButtonPrefab)
                        Destroy(child.gameObject);
                }
            }
        }
        
        /// <summary>
        /// 선택지 버튼 클릭 처리
        /// </summary>
        private void OnChoiceButtonClicked(int choiceIndex)
        {
            PlayClickSound();
            onChoiceSelected?.Invoke(choiceIndex);
        }
        
        #region 디버그 및 에디터 메서드
        [ContextMenu("테스트 - 샘플 선택지 생성")]
        public void CreateSampleChoices()
        {
            string[] sampleChoices = new string[]
            {
                "첫 번째 선택지",
                "두 번째 선택지",
                "세 번째 선택지"
            };
            
            SetupChoices(sampleChoices, (index) => 
            {
                Debug.Log($"[ChoicePanel] 선택됨: {index} - {sampleChoices[index]}");
            });
        }
        
        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[ChoicePanel] 선택지 프리팹: {(choiceButtonPrefab != null ? "설정됨" : "없음")}");
            Debug.Log($"[ChoicePanel] 선택지 컨텐츠: {(choiceContent != null ? "설정됨" : "없음")}");
        }
        #endregion
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace KYS
{
    /// <summary>
    /// 선택지 패널 - 대화 중 선택 옵션 표시 (풀링 시스템 사용)
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
        [SerializeField] private int initialPoolSize = 8; // 초기 풀 크기
        
        // 선택지 관리
        private Action<int> onChoiceSelected;
        private List<GameObject> choiceButtonPool = new List<GameObject>(); // 선택지 버튼 풀
        private List<GameObject> activeChoiceButtons = new List<GameObject>(); // 활성화된 선택지 버튼들
        
        protected override void Awake()
        {
            base.Awake();
            InitializeChoiceButtonPool();
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
        /// 선택지 버튼 풀 초기화
        /// </summary>
        private void InitializeChoiceButtonPool()
        {
            if (choiceButtonPrefab == null)
            {
                var prefabButton = GetUI<Button>(choiceButtonPrefabName);
                if (prefabButton != null)
                {
                    choiceButtonPrefab = prefabButton.gameObject;
                    prefabButton.gameObject.SetActive(false); // 템플릿은 숨김
                }
            }
            
            if (choiceButtonPrefab != null && choiceContent != null)
            {
                // 초기 풀 생성
                for (int i = 0; i < initialPoolSize; i++)
                {
                    GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContent);
                    buttonObj.SetActive(false);
                    choiceButtonPool.Add(buttonObj);
                }
            }
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
            
            int choiceCount = Mathf.Min(choices.Length, maxChoices);
            
            // 필요한 만큼 풀에서 버튼 가져오기
            for (int i = 0; i < choiceCount; i++)
            {
                GameObject buttonObj = GetChoiceButtonFromPool();
                if (buttonObj != null)
                {
                    SetupChoiceButton(buttonObj, choices[i], i);
                    activeChoiceButtons.Add(buttonObj);
                }
            }
        }
        
        /// <summary>
        /// 풀에서 선택지 버튼 가져오기
        /// </summary>
        private GameObject GetChoiceButtonFromPool()
        {
            // 비활성화된 버튼 찾기
            foreach (GameObject buttonObj in choiceButtonPool)
            {
                if (!buttonObj.activeInHierarchy)
                {
                    return buttonObj;
                }
            }
            
            // 풀이 부족하면 새로 생성
            if (choiceButtonPrefab != null && choiceContent != null)
            {
                GameObject newButton = Instantiate(choiceButtonPrefab, choiceContent);
                choiceButtonPool.Add(newButton);
                return newButton;
            }
            
            return null;
        }
        
        /// <summary>
        /// 선택지 버튼 설정
        /// </summary>
        private void SetupChoiceButton(GameObject buttonObj, string choiceText, int index)
        {
            if (buttonObj == null) return;
            
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
        /// 선택지 숨기기 (Destroy 대신 SetActive(false) 사용)
        /// </summary>
        public void ClearChoices()
        {
            // 활성화된 선택지 버튼들을 비활성화
            foreach (GameObject buttonObj in activeChoiceButtons)
            {
                if (buttonObj != null)
                {
                    buttonObj.SetActive(false);
                }
            }
            
            activeChoiceButtons.Clear();
        }
        
        /// <summary>
        /// 선택지 완전 제거 (메모리 정리 시에만 사용)
        /// </summary>
        public void DestroyAllChoices()
        {
            ClearChoices();
            
            // 풀의 모든 버튼 제거
            foreach (GameObject buttonObj in choiceButtonPool)
            {
                if (buttonObj != null)
                {
                    Destroy(buttonObj);
                }
            }
            
            choiceButtonPool.Clear();
        }
        
        /// <summary>
        /// 선택지 버튼 클릭 처리
        /// </summary>
        private void OnChoiceButtonClicked(int choiceIndex)
        {
            PlayClickSound();
            onChoiceSelected?.Invoke(choiceIndex);
        }
        
        /// <summary>
        /// 선택지 개수 반환
        /// </summary>
        public int GetActiveChoiceCount()
        {
            return activeChoiceButtons.Count;
        }
        
        /// <summary>
        /// 선택지 활성화 상태 확인
        /// </summary>
        public bool HasActiveChoices()
        {
            return activeChoiceButtons.Count > 0;
        }
        
        /// <summary>
        /// 특정 선택지 버튼 활성화/비활성화
        /// </summary>
        public void SetChoiceButtonActive(int choiceIndex, bool active)
        {
            if (choiceIndex >= 0 && choiceIndex < activeChoiceButtons.Count)
            {
                GameObject buttonObj = activeChoiceButtons[choiceIndex];
                if (buttonObj != null)
                {
                    buttonObj.SetActive(active);
                }
            }
        }
        
        /// <summary>
        /// 선택지 버튼 상호작용 활성화/비활성화
        /// </summary>
        public void SetChoiceButtonsInteractable(bool interactable)
        {
            foreach (GameObject buttonObj in activeChoiceButtons)
            {
                if (buttonObj != null)
                {
                    Button button = buttonObj.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = interactable;
                    }
                }
            }
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
        
        [ContextMenu("테스트 - 선택지 숨기기")]
        public void TestHideChoices()
        {
            ClearChoices();
            Debug.Log("[ChoicePanel] 선택지가 숨겨졌습니다.");
        }
        
        [ContextMenu("테스트 - 선택지 다시 표시")]
        public void TestShowChoices()
        {
            if (activeChoiceButtons.Count > 0)
            {
                foreach (GameObject buttonObj in activeChoiceButtons)
                {
                    if (buttonObj != null)
                    {
                        buttonObj.SetActive(true);
                    }
                }
                Debug.Log("[ChoicePanel] 선택지가 다시 표시되었습니다.");
            }
            else
            {
                Debug.Log("[ChoicePanel] 표시할 활성 선택지가 없습니다.");
            }
        }
        
        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[ChoicePanel] 선택지 프리팹: {(choiceButtonPrefab != null ? "설정됨" : "없음")}");
            Debug.Log($"[ChoicePanel] 선택지 컨텐츠: {(choiceContent != null ? "설정됨" : "없음")}");
            Debug.Log($"[ChoicePanel] 풀 크기: {choiceButtonPool.Count}");
            Debug.Log($"[ChoicePanel] 활성 선택지: {activeChoiceButtons.Count}");
        }
        
        [ContextMenu("풀 상태 확인")]
        public void CheckPoolStatus()
        {
            int activeInPool = 0;
            int inactiveInPool = 0;
            
            foreach (GameObject buttonObj in choiceButtonPool)
            {
                if (buttonObj != null)
                {
                    if (buttonObj.activeInHierarchy)
                        activeInPool++;
                    else
                        inactiveInPool++;
                }
            }
            
            Debug.Log($"[ChoicePanel] 풀 상태 - 활성: {activeInPool}, 비활성: {inactiveInPool}");
        }
        #endregion
    }
}

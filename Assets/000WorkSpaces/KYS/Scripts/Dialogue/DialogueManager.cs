using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using GameQuest;
using UnityEngine.SceneManagement;

namespace KYS
{
    /// <summary>
    /// 대화 시스템 매니저 - StoryPanel과 연동하여 노드 기반 대화 관리
    /// </summary>
    public class DialogueManager : Singleton<DialogueManager>
    {
        [Header("Dialogue Settings")]
        [SerializeField] private bool enableDebugLogs = true;

        // 현재 진행 중인 대화 정보
        private string currentNpcId;
        private string currentStageId;
        private string currentNodeId;
        private DialogueData currentDialogueData;
        private List<string> dialogueHistory = new List<string>();

        // 이벤트
        public System.Action<DialogueData> OnDialogueStarted;
        public System.Action<DialogueData> OnDialogueCompleted;
        public System.Action<DialogueData, int> OnChoiceSelected;
        public System.Action<string> OnDialogueNodeChanged;

        // 프로퍼티
        public string CurrentNpcId => currentNpcId;
        public string CurrentStageId => currentStageId;
        public string CurrentNodeId => currentNodeId;
        public DialogueData CurrentDialogueData => currentDialogueData;
        public bool IsDialogueActive => currentDialogueData != null;
        public List<string> DialogueHistory => new List<string>(dialogueHistory);

        private void Awake()
        {
            if (enableDebugLogs)
            { 
                ////Debug.Log("[DialogueManager] 초기화 완료"); 
            }

            SceneManager.sceneLoaded += (scene, mode) => OnDialogueCompleted = null;
        }



        /// <summary>
        /// StoryPanel을 동적으로 생성하고 대화 시작
        /// </summary>
        public bool StartDialogueWithPanel(string npcId, string stageId, string startNodeId = null)
        {
            if (enableDebugLogs)
                ////Debug.Log($"[DialogueManager] StoryPanel과 함께 대화 시작: {npcId}, {stageId}, {startNodeId}");

            // StoryPanel 생성 및 초기화 완료 대기
            StartCoroutine(StartDialogueWithPanelCoroutine(npcId, stageId, startNodeId));

            return true; // 비동기 처리이므로 true 반환
        }

        /// <summary>
        /// StoryPanel 생성 및 초기화 완료 후 대화 시작하는 코루틴
        /// </summary>
        private IEnumerator StartDialogueWithPanelCoroutine(string npcId, string stageId, string startNodeId)
        {
            if (enableDebugLogs)
                ////Debug.Log($"[DialogueManager] StoryPanel 생성 시작: {npcId}");

            // StoryPanel 생성
            Manager.ui.ShowPanelAsync<StoryPanel>();

            // StoryPanel이 생성될 때까지 대기
            StoryPanel storyPanel = null;
            float timeout = 5f; // 5초 타임아웃
            float elapsed = 0f;

            while (storyPanel == null && elapsed < timeout)
            {
                storyPanel = FindObjectOfType<StoryPanel>();
                if (storyPanel == null)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }
            }

            if (storyPanel == null)
            {
                Debug.LogError("[DialogueManager] StoryPanel 생성 타임아웃!");
                yield break;
            }

            if (enableDebugLogs)
            {
                //Debug.Log($"[DialogueManager] StoryPanel 발견: {storyPanel.name}");
            }

            // StoryPanel이 이미 초기화되었는지 확인
            bool isAlreadyInitialized = storyPanel.IsInitialized;
            
            if (isAlreadyInitialized)
            {
                if (enableDebugLogs)
                {
                    //Debug.Log("[DialogueManager] StoryPanel이 이미 초기화됨");
                }
            }
            else
            {
                if (enableDebugLogs)
                {
                    //Debug.Log("[DialogueManager] StoryPanel 초기화 대기 중...");
                }
            }

            // 초기화 완료 대기 (이미 초기화된 경우 스킵)
            if (!isAlreadyInitialized)
            {
                bool initializationCompleted = false;
                System.Action onInitializationCompleted = () => {
                    initializationCompleted = true;
                    if (enableDebugLogs)
                    {
                        //Debug.Log("[DialogueManager] StoryPanel 초기화 완료 확인됨");
                    }
                };

                storyPanel.OnInitializationCompleted += onInitializationCompleted;

                // 초기화 완료까지 대기
                elapsed = 0f;
                while (!initializationCompleted && elapsed < timeout)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }

                // 이벤트 구독 해제
                storyPanel.OnInitializationCompleted -= onInitializationCompleted;

                if (!initializationCompleted)
                {
                    Debug.LogError("[DialogueManager] StoryPanel 초기화 타임아웃!");
                    yield break;
                }
            }

            // 대화 시작
            if (enableDebugLogs)
            {
                //Debug.Log($"[DialogueManager] 대화 시작: {npcId}");
            }

            bool success = StartDialogue(npcId, stageId, startNodeId);
            if (success)
            {
                if (enableDebugLogs)
                {
                    //Debug.Log($"[DialogueManager] StoryPanel과 함께 대화 시작 성공: {npcId}");
                }

                // CurrentDialogueData 확인
                if (currentDialogueData != null)
                {
                    //Debug.Log($"[DialogueManager] CurrentDialogueData 설정됨: {currentDialogueData.Id}");
                }
                else
                {
                    Debug.LogError("[DialogueManager] CurrentDialogueData가 null입니다!");
                }
            }
            else
            {
                Debug.LogError($"[DialogueManager] StoryPanel과 함께 대화 시작 실패: {npcId}");
            }
        }



        /// <summary>
        /// NPC 대화 시작 (기존 메서드)
        /// </summary>
        public bool StartDialogue(string npcId, string stageId, string startNodeId = null)
        {
            if (IsDialogueActive)
            {
                Debug.LogWarning($"[DialogueManager] 이미 대화가 진행 중입니다. 현재: {currentNpcId}");
                return false;
            }

            currentNpcId = npcId;
            currentStageId = stageId;
            dialogueHistory.Clear();

            // 시작 노드 ID가 없으면 조건에 맞는 첫 번째 노드 찾기
            if (string.IsNullOrEmpty(startNodeId))
            {
                startNodeId = FindFirstAvailableNodeId(npcId, stageId);
            }

            if (string.IsNullOrEmpty(startNodeId))
            {
                Debug.LogError($"[DialogueManager] NPC {npcId}의 시작 노드를 찾을 수 없습니다.");
                return false;
            }

            return MoveToNode(startNodeId);
        }

        /// <summary>
        /// 특정 노드로 이동
        /// </summary>
        public bool MoveToNode(string nodeId)
        {
            //Debug.Log($"[DialogueManager] MoveToNode 호출됨 - 노드 ID: '{nodeId}'");
            
            if (string.IsNullOrEmpty(nodeId))
            {
                Debug.LogError("[DialogueManager] 노드 ID가 비어있습니다.");
                return false;
            }

            // 데이터 로드 상태 확인
            if (Manager.data?.Dialogue == null)
            {
                Debug.LogError("[DialogueManager] Dialogue 데이터가 아직 로드되지 않았습니다. DataManager 초기화를 기다려주세요.");
                return false;
            }

            Debug.Log($"[DialogueManager] Dialogue 데이터 로드됨 - 총 {Manager.data.Dialogue.Values.Count}개 노드");

            // CSV에서 노드 데이터 찾기
            if (!Manager.data.Dialogue.Values.TryGetValue(nodeId, out DialogueDataCsv csvData))
            {
                Debug.LogError($"[DialogueManager] 노드 '{nodeId}'를 찾을 수 없습니다. 사용 가능한 노드들:");
                foreach (var availableNode in Manager.data.Dialogue.Values.Values)
                {
                    Debug.Log($"  - {availableNode.Id} ({availableNode.NodeType})");
                }
                return false;
            }
            
            //Debug.Log($"[DialogueManager] 노드 '{nodeId}' 찾음 - 타입: {csvData.NodeType}");

            // Firebase에서 동적 데이터 가져오기 또는 생성
            DialogueData dialogueData = GetOrCreateDialogueData(nodeId);
            if (dialogueData == null)
            {
                Debug.LogError($"[DialogueManager] 노드 {nodeId}의 데이터를 생성할 수 없습니다.");
                return false;
            }

            currentNodeId = nodeId;
            currentDialogueData = dialogueData;
            dialogueHistory.Add(nodeId);

            if (enableDebugLogs)
                //Debug.Log($"[DialogueManager] 노드 이동: {nodeId}");

            OnDialogueNodeChanged?.Invoke(nodeId);
            OnDialogueStarted?.Invoke(dialogueData);

            // 가운데 이미지 처리
            ProcessCenterImage(dialogueData);

            return true;
        }

        /// <summary>
        /// 다음 노드로 진행
        /// </summary>
        public bool MoveToNextNode()
        {
            if (currentDialogueData == null)
            {
                Debug.LogError("[DialogueManager] 현재 대화 데이터가 없습니다.");
                return false;
            }

            string nextNodeId = currentDialogueData.NextNodeId;
            if (string.IsNullOrEmpty(nextNodeId))
            {
                // 대화 종료
                EndDialogue();
                return false;
            }

            return MoveToNode(nextNodeId);
        }

        /// <summary>
        /// 선택지 선택
        /// </summary>
        public bool SelectChoice(int choiceIndex)
        {
            Debug.Log($"[DialogueManager] SelectChoice 호출됨 - 인덱스: {choiceIndex}");
            
            if (currentDialogueData == null)
            {
                Debug.LogError("[DialogueManager] 현재 대화 데이터가 없습니다.");
                return false;
            }

            Debug.Log($"[DialogueManager] 현재 노드: {currentDialogueData.Id}");
            Debug.Log($"[DialogueManager] 노드 타입: {currentDialogueData.NodeType}");
            Debug.Log($"[DialogueManager] 선택지 존재 여부: {currentDialogueData.HasChoices}");
            
            if (!currentDialogueData.HasChoices)
            {
                Debug.LogError("[DialogueManager] 현재 노드에 선택지가 없습니다.");
                return false;
            }

            string[] choiceNextIds = currentDialogueData.GetFilteredChoiceNextIds();
            Debug.Log($"[DialogueManager] 필터링된 선택지 다음 노드 ID들: [{string.Join(", ", choiceNextIds)}]");
            
            // 필터링된 선택지 개수를 기준으로 인덱스 확인
            int availableChoices = choiceNextIds.Length;
            Debug.Log($"[DialogueManager] 사용 가능한 선택지 개수: {availableChoices}");
            
            if (choiceIndex < 0 || choiceIndex >= availableChoices)
            {
                Debug.LogError($"[DialogueManager] 잘못된 선택지 인덱스: {choiceIndex} (최대: {availableChoices - 1})");
                return false;
            }

            // 선택한 선택지 저장
            //currentDialogueData.ChoiceSelected.Value = choiceIndex;
            //currentDialogueData.IsCompleted.Value = true;

            OnChoiceSelected?.Invoke(currentDialogueData, choiceIndex);

            // 선택지에 따른 다음 노드로 이동
            string nextNodeId = choiceNextIds[choiceIndex];
            Debug.Log($"[DialogueManager] 선택된 다음 노드 ID: '{nextNodeId}'");
            
            if (string.IsNullOrEmpty(nextNodeId) || nextNodeId.ToLower() == "end")
            {
                Debug.LogWarning($"[DialogueManager] 다음 노드 ID가 비어있거나 'end': '{nextNodeId}' - 대화 종료");
                EndDialogue();
                return false;
            }

            Debug.Log($"[DialogueManager] MoveToNode 호출: {nextNodeId}");
            bool result = MoveToNode(nextNodeId);
            Debug.Log($"[DialogueManager] MoveToNode 결과: {result}");
            
            return result;
        }

        /// <summary>
        /// 대화 종료
        /// </summary>
        public void EndDialogue()
        {
            //Debug.Log($"[DialogueManager] 대화 종료: {currentNpcId}");

            if (currentDialogueData != null)
            {
                //currentDialogueData.IsCompleted.Value = true;
                OnDialogueCompleted?.Invoke(currentDialogueData);
            }

            if (enableDebugLogs)
                //Debug.Log($"[DialogueManager] 대화 종료: {currentNpcId}");

            currentNpcId = null;
            currentStageId = null;
            currentNodeId = null;
            currentDialogueData = null;
        }



        /// <summary>
        /// NPC의 첫 번째 노드 ID 찾기
        /// </summary>
        private string FindFirstNodeId(string npcId, string stageId)
        {
            var firstNode = Manager.data.Dialogue.Values.Values
                .FirstOrDefault(d => d.NpcId == npcId && d.StageId == stageId && d.NodeType == "start");

            return firstNode?.Id;
        }

        /// <summary>
        /// 조건을 만족하는 첫 번째 노드 ID 찾기
        /// </summary>
        private string FindFirstAvailableNodeId(string npcId, string stageId)
        {
            var startNodes = Manager.data.Dialogue.Values.Values
                .Where(d => d.NpcId == npcId && d.StageId == stageId && d.NodeType == "start")
                .OrderBy(d => d.Id)
                .ToList();

            foreach (var node in startNodes)
            {
                if (CheckNodeCondition(node))
                {
                    return node.Id;
                }
            }

            return null;
        }

        /// <summary>
        /// 노드 조건 체크
        /// </summary>
        private bool CheckNodeCondition(DialogueDataCsv node)
        {
            if (string.IsNullOrEmpty(node.ConditionType) || string.IsNullOrEmpty(node.ConditionValue))
            {
                return true; // 조건이 없으면 항상 통과
            }

            switch (node.ConditionType.ToLower())
            {
                case "quest_completed":
                    return CheckQuestCompleted(node.ConditionValue);
                    
                case "quest_progress":
                    return CheckQuestProgress(node.ConditionValue);
                    
                case "level":
                    return CheckLevel(node.ConditionValue);
                    
                case "item_owned":
                    return CheckItemOwned(node.ConditionValue);
                    
                case "flag_set":
                    return CheckFlagSet(node.ConditionValue);
                    
                default:
                    Debug.LogWarning($"[DialogueManager] 알 수 없는 조건 타입: {node.ConditionType}");
                    return true;
            }
        }

        /// <summary>
        /// 퀘스트 완료 체크 (CYE QuestManager 연동)
        /// </summary>
        private bool CheckQuestCompleted(string questId)
        {
            if (QuestManager.Instance == null)
            {
                Debug.LogWarning("[DialogueManager] QuestManager가 없습니다.");
                return false;
            }

            // 250908 CYE -> 퀘스트 아이디 타입 변경으로 인한 주석처리(int -> string)
            // // 퀘스트 ID를 int로 변환
            // if (!int.TryParse(questId, out int questIdInt))
            // {
            //     Debug.LogWarning($"[DialogueManager] 잘못된 퀘스트 ID 형식: {questId}");
            //     return false;
            // }

            // 현재 퀘스트 목록에서 해당 퀘스트 찾기
            var quest = Manager.firebase.UserData.CurStageData.Npc.CurQuestData;
            if (quest == null)
            {
                Debug.LogWarning($"[DialogueManager] 퀘스트를 찾을 수 없습니다: {questId}");
                return false;
            }

            bool isCompleted = quest.State == QuestState.Completed;
            //Debug.Log($"[DialogueManager] 퀘스트 완료 체크: {questId} = {isCompleted}");
            return isCompleted;
        }

        /// <summary>
        /// 퀘스트 진행도 체크 (CYE QuestManager 연동)
        /// </summary>
        private bool CheckQuestProgress(string conditionValue)
        {
            // 형식: "questId:progress" (예: "1:50")
            var parts = conditionValue.Split(':');
            if (parts.Length != 2) return false;

            // if (!int.TryParse(parts[0], out int questId) || !int.TryParse(parts[1], out int requiredProgress))
            if (!int.TryParse(parts[1], out int requiredProgress))
            {
                Debug.LogWarning($"[DialogueManager] 잘못된 진행도 조건 형식: {conditionValue}");
                return false;
            }

            if (QuestManager.Instance == null)
            {
                Debug.LogWarning("[DialogueManager] QuestManager가 없습니다.");
                return false;
            }

            // 현재 퀘스트 목록에서 해당 퀘스트 찾기
            var quest = Manager.firebase.UserData.CurStageData.Npc.CurQuestData;
            if (quest == null)
            {
                Debug.LogWarning($"[DialogueManager] 퀘스트를 찾을 수 없습니다: {parts[0]}");
                return false;
            }

            // 어떤 진행도인지 잘 모르겠어서 일단 1로 통일해두었습니다 :최재민
            // 퀘스트 진행도 계산 (완료된 진행도 항목 수 / 전체 진행도 항목 수 * 100)
            // int completedCount = quest._progresses.Count(p => p.IsContentClear);
            // int totalCount = quest._progresses.Count;

            int completedCount = 1;
            int totalCount = 1;

            int currentProgress = totalCount > 0 ? (completedCount * 100) / totalCount : 0;

            bool meetsRequirement = currentProgress >= requiredProgress;
            //Debug.Log($"[DialogueManager] 퀘스트 진행도 체크: {parts[0]} = {currentProgress}% >= {requiredProgress}% = {meetsRequirement}");
            return meetsRequirement;
        }

        /// <summary>
        /// 레벨 체크
        /// </summary>
        private bool CheckLevel(string conditionValue)
        {
            // 형식: "levelType:level" (예: "player:10")
            var parts = conditionValue.Split(':');
            if (parts.Length != 2) return false;

            string levelType = parts[0];
            int requiredLevel = int.Parse(parts[1]);

            // TODO: 실제 레벨 시스템과 연동
            // 예시: PlayerManager.Instance.GetLevel(levelType) >= requiredLevel
            //Debug.Log($"[DialogueManager] 레벨 체크: {levelType} >= {requiredLevel}");
            return true; // 임시로 항상 true
        }

        /// <summary>
        /// 아이템 보유 체크
        /// </summary>
        private bool CheckItemOwned(string conditionValue)
        {
            // 형식: "itemId:count" (예: "item_001:1")
            var parts = conditionValue.Split(':');
            if (parts.Length != 2) return false;

            string itemId = parts[0];
            int requiredCount = int.Parse(parts[1]);

            // TODO: 실제 인벤토리 시스템과 연동
            // 예시: InventoryManager.Instance.GetItemCount(itemId) >= requiredCount
            //Debug.Log($"[DialogueManager] 아이템 보유 체크: {itemId} >= {requiredCount}");
            return true; // 임시로 항상 true
        }

        /// <summary>
        /// 플래그 설정 체크
        /// </summary>
        private bool CheckFlagSet(string conditionValue)
        {
            // 형식: "flagKey:value" (예: "tutorial_completed:true")
            var parts = conditionValue.Split(':');
            if (parts.Length != 2) return false;

            string flagKey = parts[0];
            string expectedValue = parts[1];

            // TODO: 실제 플래그 시스템과 연동
            // 예시: FlagManager.Instance.GetFlag(flagKey) == expectedValue
            //Debug.Log($"[DialogueManager] 플래그 체크: {flagKey} == {expectedValue}");
            return true; // 임시로 항상 true
        }

        /// <summary>
        /// DialogueData 가져오기 또는 생성 (메모리에서만 관리)
        /// </summary>
        private DialogueData GetOrCreateDialogueData(string nodeId)
        {
            // 메모리에서 기존 데이터 찾기
            var existingData = currentDialogueData?.Id == nodeId ? currentDialogueData : null;

            if (existingData != null)
            {
                return existingData;
            }

            // 새 데이터 생성 (Firebase 저장 없이 메모리에서만 관리)
            var newData = new DialogueData(nodeId, null);
            return newData;
        }

        /// <summary>
        /// NPC의 모든 대화 노드 가져오기
        /// </summary>
        public List<DialogueDataCsv> GetNpcDialogueNodes(string npcId, string stageId = null)
        {
            return Manager.data.Dialogue.Values.Values
                .Where(d => d.NpcId == npcId && (stageId == null || d.StageId == stageId))
                .OrderBy(d => d.Id)
                .ToList();
        }

        /// <summary>
        /// 특정 노드의 완료 상태 확인 (메모리에서만 관리)
        /// </summary>
        public bool IsNodeCompleted(string nodeId)
        {
            // 메모리에서만 관리하므로 항상 false 반환 (또는 별도 로직 구현)
            return false;
        }

        /// <summary>
        /// NPC 대화 진행도 확인
        /// </summary>
        public float GetNpcDialogueProgress(string npcId, string stageId = null)
        {
            var npcNodes = GetNpcDialogueNodes(npcId, stageId);
            if (npcNodes.Count == 0) return 0f;

            int completedCount = npcNodes.Count(n => IsNodeCompleted(n.Id));
            return (float)completedCount / npcNodes.Count;
        }


        /// <summary>
        /// 컴퍼스 팝업 표시 (새로운 메서드)
        /// </summary>
        public void ShowCompassPopUp(string nodeID, CompassMessagePopup.CompassPositionType positionType, bool autoClose = true, int deley = 3000)
        {
            try
            {
                //Debug.Log("[DialogueManager] 컴퍼스 팝업 표시 시작");

                // 1. 컴퍼스 팝업 열기
                Manager.ui.ShowPopUpAsync<CompassMessagePopup>(popup =>
                {
                    popup.SetCompassPosition(positionType);
                    popup.SetCompassNode(nodeID);

                    if (autoClose)
                    {
                        //Debug.Log($"[DialogueManager] 컴퍼스 팝업이 열렸습니다. {deley / 1000}초 후 자동 종료됩니다...");

                        // 2. 지정된 시간 대기 후 자동 종료
                        StartCoroutine(WaitDelay(deley, popup));
                    }
                    else
                    {
                        //Debug.Log("[DialogueManager] 컴퍼스 팝업이 수동 모드로 열렸습니다. 사용자가 직접 닫아야 합니다.");
                    }
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueManager] 컴퍼스 팝업 표시 실패: {e.Message}");
            }
        }


        private IEnumerator WaitDelay(float delay, CompassMessagePopup popup)
        {
            yield return new WaitForSeconds(delay / 1000);

            // 3. 플레이어 행동 완료 시뮬레이션
            popup.CompleteCompassAction();
            //Debug.Log("[DialogueManager] 컴퍼스 행동 완료 시뮬레이션");
        }

        /// <summary>
        /// 가운데 이미지 처리
        /// </summary>
        private void ProcessCenterImage(DialogueData dialogueData)
        {
            // StoryPanel 찾기
            StoryPanel storyPanel = FindObjectOfType<StoryPanel>();
            if (storyPanel == null)
            {
                Debug.LogWarning("[DialogueManager] StoryPanel을 찾을 수 없어 가운데 이미지를 표시할 수 없습니다.");
                return;
            }

            // 가운데 이미지 표시 (빈 값이어도 ShowCenterImage 호출하여 기존 이미지 숨김)
            storyPanel.ShowCenterImage(
                dialogueData.CenterImage,
                dialogueData.CenterImageDuration,
                dialogueData.CenterImageFadeInTime,
                dialogueData.CenterImageFadeOutTime,
                dialogueData.HideCharacterImages,
                dialogueData.CenterImageInfinite
            );

            if (enableDebugLogs)
            {
                string durationText = dialogueData.CenterImageInfinite ? "무한" : $"{dialogueData.CenterImageDuration}초";
                /*
                Debug.Log($"[DialogueManager] 가운데 이미지 표시: {dialogueData.CenterImage}, " +
                         $"지속시간: {durationText}, " +
                         $"페이드인: {dialogueData.CenterImageFadeInTime}초, " +
                         $"페이드아웃: {dialogueData.CenterImageFadeOutTime}초, " +
                         $"캐릭터 숨김: {dialogueData.HideCharacterImages}");
                */
            }
        }

        /// <summary>
        /// 가운데 이미지 수동 숨김
        /// </summary>
        public void HideCenterImage()
        {
            StoryPanel storyPanel = FindObjectOfType<StoryPanel>();
            if (storyPanel != null)
            {
                storyPanel.ForceHideCenterImage();
                if (enableDebugLogs)
                {
                    //Debug.Log("[DialogueManager] 가운데 이미지 수동 숨김");
                }
            }
        }

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보 출력")]
        public void PrintDebugInfo()
        {
            //Debug.Log($"[DialogueManager] 현재 NPC: {currentNpcId}");
            //Debug.Log($"[DialogueManager] 현재 스테이지: {currentStageId}");
            //Debug.Log($"[DialogueManager] 현재 노드: {currentNodeId}");
            //Debug.Log($"[DialogueManager] 대화 활성: {IsDialogueActive}");
            //Debug.Log($"[DialogueManager] 대화 히스토리: {string.Join(" -> ", dialogueHistory)}");
        }
    }
}

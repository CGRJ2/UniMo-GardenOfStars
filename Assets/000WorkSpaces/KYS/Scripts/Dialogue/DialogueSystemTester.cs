using UnityEngine;
using System.Linq;
using KYS;

namespace KYS
{
    /// <summary>
    /// 대화 시스템 테스트 클래스
    /// </summary>
    public class DialogueSystemTester : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private string testNpcId = "npc001";
        [SerializeField] private string testStageId = "stage_01";
        [SerializeField] private string testStartNodeId = "npc001_start";

        [Header("참조")]
        [SerializeField] private StoryPanel storyPanel;

        private void Start()
        {
            // StoryPanel 참조 자동 찾기
            if (storyPanel == null)
            {
                storyPanel = FindObjectOfType<StoryPanel>();
                if (storyPanel == null)
                {
                    Debug.LogWarning("[DialogueSystemTester] StoryPanel을 찾을 수 없습니다. Inspector에서 직접 설정해주세요.");
                }
            }

            // DialogueManager 이벤트 구독
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
                DialogueManager.Instance.OnDialogueCompleted += OnDialogueCompleted;
                DialogueManager.Instance.OnChoiceSelected += OnChoiceSelected;
                DialogueManager.Instance.OnDialogueNodeChanged += OnDialogueNodeChanged;
            }
            else
            {
                Debug.LogWarning("[DialogueSystemTester] DialogueManager 인스턴스를 찾을 수 없습니다.");
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueStarted -= OnDialogueStarted;
                DialogueManager.Instance.OnDialogueCompleted -= OnDialogueCompleted;
                DialogueManager.Instance.OnChoiceSelected -= OnChoiceSelected;
                DialogueManager.Instance.OnDialogueNodeChanged -= OnDialogueNodeChanged;
            }
        }

        #region 테스트 메서드

        [ContextMenu("테스트 - 기본 대화 시작")]
        public void TestStartBasicDialogue()
        {
            if (storyPanel != null)
            {
                storyPanel.StartCSVDialogue(testNpcId, testStageId, testStartNodeId);
            }
            else
            {
                Debug.LogError("[DialogueSystemTester] StoryPanel을 찾을 수 없습니다.");
            }
        }

        [ContextMenu("테스트 - 시스템 상태 확인")]
        public void TestSystemStatus()
        {
            Debug.Log("=== 대화 시스템 상태 확인 ===");
            Debug.Log($"DataManager 존재: {Manager.data != null}");
            Debug.Log($"Dialogue 데이터 존재: {Manager.data?.Dialogue != null}");
            Debug.Log($"Dialogue 데이터 개수: {Manager.data?.Dialogue?.Values?.Count ?? 0}");
            Debug.Log($"DialogueManager 존재: {DialogueManager.Instance != null}");
            Debug.Log($"StoryPanel 참조: {storyPanel != null}");
            
            if (Manager.data?.Dialogue != null)
            {
                Debug.Log("=== 로드된 모든 노드 목록 ===");
                foreach (var node in Manager.data.Dialogue.Values.Values)
                {
                    Debug.Log($"  - {node.Id} | {node.NodeType} | {node.Speaker} | {node.DialogueText_Korea}");
                }
                
                // 특정 노드 찾기 테스트
                if (Manager.data.Dialogue.Values.TryGetValue("npc001_start", out var testNode))
                {
                    Debug.Log($"테스트 노드 찾음: {testNode.Id} - {testNode.NodeType}");
                    Debug.Log($"  - Speaker: {testNode.Speaker}");
                    Debug.Log($"  - 한국어: {testNode.DialogueText_Korea}");
                    Debug.Log($"  - 영어: {testNode.DialogueText_English}");
                    Debug.Log($"  - NextNodeId: {testNode.NextNodeId}");
                }
                else
                {
                    Debug.LogError("테스트 노드 'npc001_start'를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError("Dialogue 데이터가 null입니다. DataManager 초기화를 확인하세요.");
            }
        }

        [ContextMenu("테스트 - DialogueManager 직접 테스트")]
        public void TestDialogueManagerDirectly()
        {
            if (DialogueManager.Instance != null)
            {
                Debug.Log("=== DialogueManager 직접 테스트 ===");
                
                // StartDialogue 메서드 직접 호출
                bool result = DialogueManager.Instance.StartDialogue("npc001", "stage_01", "npc001_start");
                Debug.Log($"StartDialogue 결과: {result}");
                
                if (result)
                {
                    Debug.Log("대화 시작 성공!");
                }
                else
                {
                    Debug.LogError("대화 시작 실패!");
                }
            }
            else
            {
                Debug.LogError("DialogueManager 인스턴스가 없습니다.");
            }
        }

        [ContextMenu("테스트 - 데이터 로드 대기 후 테스트")]
        public void TestWithDataLoadWait()
        {
            StartCoroutine(TestWithDataLoadWaitCoroutine());
        }

        [ContextMenu("테스트 - CSV 파싱 디버깅")]
        public void TestCSVParsingDebug()
        {
            Debug.Log("=== CSV 파싱 디버깅 ===");
            
            if (Manager.data?.Dialogue != null)
            {
                Debug.Log($"DataTableParser 상태: {Manager.data.Dialogue != null}");
                Debug.Log($"Values Dictionary 상태: {Manager.data.Dialogue.Values != null}");
                Debug.Log($"Values Count: {Manager.data.Dialogue.Values.Count}");
                
                if (Manager.data.Dialogue.Values.Count > 0)
                {
                    Debug.Log("=== 첫 번째 노드 상세 정보 ===");
                    var firstNode = Manager.data.Dialogue.Values.Values.First();
                    Debug.Log($"  - Id: '{firstNode.Id}'");
                    Debug.Log($"  - NpcId: '{firstNode.NpcId}'");
                    Debug.Log($"  - StageId: '{firstNode.StageId}'");
                    Debug.Log($"  - NodeType: '{firstNode.NodeType}'");
                    Debug.Log($"  - Speaker: '{firstNode.Speaker}'");
                    Debug.Log($"  - DialogueText: '{firstNode.DialogueText}'");
                    Debug.Log($"  - DialogueText_Korea: '{firstNode.DialogueText_Korea}'");
                    Debug.Log($"  - DialogueText_English: '{firstNode.DialogueText_English}'");
                    Debug.Log($"  - NextNodeId: '{firstNode.NextNodeId}'");
                }
            }
            else
            {
                Debug.LogError("Dialogue 데이터가 null입니다.");
            }
        }

        private System.Collections.IEnumerator TestWithDataLoadWaitCoroutine()
        {
            Debug.Log("=== 데이터 로드 대기 테스트 ===");
            
            // 데이터 로드 대기
            float waitTime = 0f;
            while (Manager.data?.Dialogue == null && waitTime < 10f)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }
            
            if (Manager.data?.Dialogue != null)
            {
                Debug.Log($"데이터 로드 완료! 대기 시간: {waitTime:F1}초");
                Debug.Log($"로드된 노드 수: {Manager.data.Dialogue.Values.Count}");
                
                // 대화 시작 테스트
                if (storyPanel != null)
                {
                    storyPanel.StartCSVDialogue("npc001", "stage_01", "npc001_start");
                }
            }
            else
            {
                Debug.LogError("데이터 로드 실패 또는 타임아웃!");
            }
        }

        [ContextMenu("테스트 - NPC002 대화 시작")]
        public void TestStartNpc002Dialogue()
        {
            if (storyPanel != null)
            {
                storyPanel.StartCSVDialogue("npc002", "stage_01", "npc002_start");
            }
        }

        [ContextMenu("테스트 - 대화 진행도 확인")]
        public void TestCheckDialogueProgress()
        {
            if (DialogueManager.Instance != null)
            {
                float progress = DialogueManager.Instance.GetNpcDialogueProgress(testNpcId, testStageId);
                Debug.Log($"[DialogueSystemTester] NPC {testNpcId} 대화 진행도: {progress:P}");
            }
        }

        [ContextMenu("테스트 - 노드 완료 상태 확인")]
        public void TestCheckNodeCompletion()
        {
            if (DialogueManager.Instance != null)
            {
                bool isCompleted = DialogueManager.Instance.IsNodeCompleted(testStartNodeId);
                Debug.Log($"[DialogueSystemTester] 노드 {testStartNodeId} 완료 상태: {isCompleted}");
            }
        }

        [ContextMenu("테스트 - NPC 대화 노드 목록")]
        public void TestGetNpcDialogueNodes()
        {
            if (DialogueManager.Instance != null)
            {
                var nodes = DialogueManager.Instance.GetNpcDialogueNodes(testNpcId, testStageId);
                Debug.Log($"[DialogueSystemTester] NPC {testNpcId} 대화 노드 수: {nodes.Count}");
                foreach (var node in nodes)
                {
                    Debug.Log($"  - {node.Id}: {node.NodeType} - {node.DialogueText}");
                }
            }
        }

        [ContextMenu("테스트 - DialogueManager 디버그 정보")]
        public void TestDialogueManagerDebug()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.PrintDebugInfo();
            }
        }

        [ContextMenu("테스트 - 한국어 대화")]
        public void TestKoreanDialogue()
        {
            if (storyPanel != null)
            {
                // 한국어로 강제 설정
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.SetLanguage(SystemLanguage.Korean);
                }
                storyPanel.StartCSVDialogue(testNpcId, testStageId, testStartNodeId);
            }
        }

        [ContextMenu("테스트 - 영어 대화")]
        public void TestEnglishDialogue()
        {
            if (storyPanel != null)
            {
                // 영어로 강제 설정
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.SetLanguage(SystemLanguage.English);
                }
                storyPanel.StartCSVDialogue(testNpcId, testStageId, testStartNodeId);
            }
        }

        [ContextMenu("테스트 - 다국어 텍스트 확인")]
        public void TestMultilingualText()
        {
            if (DialogueManager.Instance != null && Manager.data.Dialogue != null)
            {
                var testNode = Manager.data.Dialogue.Values.Values.FirstOrDefault(d => d.Id == testStartNodeId);
                if (testNode != null)
                {
                    Debug.Log($"[DialogueSystemTester] 노드 {testStartNodeId} 다국어 텍스트:");
                    Debug.Log($"  - 기본: {testNode.DialogueText}");
                    Debug.Log($"  - 한국어: {testNode.DialogueText_Korea}");
                    Debug.Log($"  - 영어: {testNode.DialogueText_English}");
                }
            }
        }

        [ContextMenu("테스트 - 조건부 대화 시작")]
        public void TestConditionalDialogue()
        {
            if (storyPanel != null)
            {
                // 조건부 대화 시작 (퀘스트 완료 조건)
                storyPanel.StartCSVDialogue("npc001", "stage_01", "npc001_quest_start");
            }
        }

        [ContextMenu("테스트 - 조건 체크 시스템")]
        public void TestConditionCheckSystem()
        {
            if (DialogueManager.Instance != null && Manager.data.Dialogue != null)
            {
                var questNode = Manager.data.Dialogue.Values.Values.FirstOrDefault(d => d.Id == "npc001_quest_start");
                if (questNode != null)
                {
                    Debug.Log($"[DialogueSystemTester] 조건부 노드 정보:");
                    Debug.Log($"  - 노드 ID: {questNode.Id}");
                    Debug.Log($"  - 조건 타입: {questNode.ConditionType}");
                    Debug.Log($"  - 조건 값: {questNode.ConditionValue}");
                    Debug.Log($"  - 효과 타입: {questNode.EffectType}");
                    Debug.Log($"  - 효과 값: {questNode.EffectValue}");
                }
            }
        }

        [ContextMenu("테스트 - CYE 퀘스트 시스템 연동")]
        public void TestCYEQuestSystemIntegration()
        {
            if (QuestManager.Instance != null)
            {
                Debug.Log($"[DialogueSystemTester] CYE 퀘스트 시스템 연동 테스트:");
                Debug.Log($"  - QuestManager 존재: {QuestManager.Instance != null}");
                Debug.Log($"  - 현재 퀘스트 목록 수: {QuestManager.Instance._currentQuestList?.Length ?? 0}");
                
                if (QuestManager.Instance._currentQuestList != null && QuestManager.Instance._currentQuestList.Length > 0)
                {
                    var currentQuest = QuestManager.Instance.CurrentQuest;
                    Debug.Log($"  - 현재 퀘스트 ID: {currentQuest._data.QuestId}");
                    Debug.Log($"  - 현재 퀘스트 상태: {currentQuest._data.State}");
                    Debug.Log($"  - 퀘스트 진행도 항목 수: {currentQuest._progresses.Count}");
                    
                    foreach (var progress in currentQuest._progresses)
                    {
                        Debug.Log($"    - {progress.ContentTargetId}: {progress.ProgressCount}/{progress.ContentTargetCount} ({progress.State})");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[DialogueSystemTester] QuestManager가 없습니다. CYE 씬에서 테스트해주세요.");
            }
        }

        [ContextMenu("테스트 - 퀘스트 진행도 대화")]
        public void TestQuestProgressDialogue()
        {
            if (storyPanel != null)
            {
                // 퀘스트 진행도 조건 대화 시작
                storyPanel.StartCSVDialogue("npc001", "stage_01", "npc001_quest_progress");
            }
        }

        [ContextMenu("테스트 - 대화 중 언어 변경")]
        public void TestLanguageChangeDuringDialogue()
        {
            if (storyPanel != null)
            {
                // 먼저 한국어로 대화 시작
                if (LocalizationManager.Instance != null)
                {
                    LocalizationManager.Instance.SetLanguage(SystemLanguage.Korean);
                }
                storyPanel.StartCSVDialogue(testNpcId, testStageId, testStartNodeId);
                
                // 2초 후 영어로 변경
                StartCoroutine(ChangeLanguageAfterDelay(SystemLanguage.English, 2f));
            }
        }

        private System.Collections.IEnumerator ChangeLanguageAfterDelay(SystemLanguage newLanguage, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (LocalizationManager.Instance != null)
            {
                Debug.Log($"[DialogueSystemTester] 언어를 {newLanguage}로 변경합니다.");
                LocalizationManager.Instance.SetLanguage(newLanguage);
            }
        }

        [ContextMenu("테스트 - 대화창 닫기")]
        private void ClosePanel()
        {
                       if (storyPanel != null)
            {
                Manager.ui.ClosePanel();
            }
        }



        #endregion

        #region 이벤트 핸들러

        private void OnDialogueStarted(DialogueData dialogueData)
        {
            Debug.Log($"[DialogueSystemTester] 대화 시작: {dialogueData.Id} - {dialogueData.DialogueText}");
        }

        private void OnDialogueCompleted(DialogueData dialogueData)
        {
            Debug.Log($"[DialogueSystemTester] 대화 완료: {dialogueData.Id}");
        }

        private void OnChoiceSelected(DialogueData dialogueData, int choiceIndex)
        {
            Debug.Log($"[DialogueSystemTester] 선택지 선택: {dialogueData.Id} - 선택지 {choiceIndex}");
        }

        private void OnDialogueNodeChanged(string nodeId)
        {
            Debug.Log($"[DialogueSystemTester] 노드 변경: {nodeId}");
        }

        #endregion

        #region UI 테스트

        [ContextMenu("테스트 - StoryPanel 정보 출력")]
        public void TestStoryPanelInfo()
        {
            if (storyPanel != null)
            {
                Debug.Log($"[DialogueSystemTester] StoryPanel 상태:");
                Debug.Log($"  - 활성화: {storyPanel.IsActive}");
                Debug.Log($"  - 스토리 모드: {storyPanel.IsStoryMode()}");
                Debug.Log($"  - 대화 모드: {storyPanel.IsDialogueMode()}");
                Debug.Log($"  - 선택지 모드: {storyPanel.IsChoiceMode()}");
            }
        }

        [ContextMenu("테스트 - 다른 StageId 대화 (stage_02)")]
        public void TestDifferentStageIdDialogue()
        {
            Debug.Log("[DialogueSystemTester] 다른 StageId 대화 테스트 시작");
            
            // stage_02의 npc003 대화 시작
            bool success = DialogueManager.Instance.StartDialogueWithPanel("npc003", "stage_02", "npc003_start");
            
            if (success)
            {
                Debug.Log("[DialogueSystemTester] stage_02 대화 시작 성공");
            }
            else
            {
                Debug.LogError("[DialogueSystemTester] stage_02 대화 시작 실패");
            }
        }

        [ContextMenu("테스트 - 직접 노드 이동 (다른 StageId)")]
        public void TestDirectNodeMoveToDifferentStage()
        {
            Debug.Log("[DialogueSystemTester] 다른 StageId 노드로 직접 이동 테스트");
            
            // stage_02의 노드로 직접 이동
            bool success = DialogueManager.Instance.MoveToNode("npc003_dialogue_002");
            
            if (success)
            {
                Debug.Log("[DialogueSystemTester] 다른 StageId 노드로 이동 성공");
            }
            else
            {
                Debug.LogError("[DialogueSystemTester] 다른 StageId 노드로 이동 실패");
            }
        }

        [ContextMenu("테스트 - 빈 StageId 대화 (npc004)")]
        public void TestEmptyStageIdDialogue()
        {
            Debug.Log("[DialogueSystemTester] 빈 StageId 대화 테스트 시작");
            
            // 빈 StageId로 대화 시작 ("" 사용)
            bool success = DialogueManager.Instance.StartDialogueWithPanel("npc004", "", "npc004_start");
            
            if (success)
            {
                Debug.Log("[DialogueSystemTester] 빈 StageId 대화 시작 성공");
            }
            else
            {
                Debug.LogError("[DialogueSystemTester] 빈 StageId 대화 시작 실패");
            }
        }

        [ContextMenu("테스트 - null StageId 대화 (npc004)")]
        public void TestNullStageIdDialogue()
        {
            Debug.Log("[DialogueSystemTester] null StageId 대화 테스트 시작");
            
            // null StageId로 대화 시작
            bool success = DialogueManager.Instance.StartDialogueWithPanel("npc004", null, "npc004_start");
            
            if (success)
            {
                Debug.Log("[DialogueSystemTester] null StageId 대화 시작 성공");
            }
            else
            {
                Debug.LogError("[DialogueSystemTester] null StageId 대화 시작 실패");
            }
        }

        #endregion
    }
}

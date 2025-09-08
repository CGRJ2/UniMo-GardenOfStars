using UnityEngine;
using KYS.DialogueSystem;

namespace KYS
{
    /// <summary>
    /// StoryPanel 테스트를 위한 헬퍼 클래스
    /// 개발 및 디버깅 시에만 사용
    /// </summary>
    public class StoryPanelTester : MonoBehaviour
    {
        [Header("테스트용 멀티 캐릭터 대화 데이터")]
        [SerializeField] private string[] testCharacterNames = { "NPC", "플레이어", "NPC", "플레이어" };
        [SerializeField] private string[] testMultiDialogue = 
        {
            "안녕하세요! 무엇을 도와드릴까요?",
            "안녕하세요! 마을에 대해 알고 싶어요.",
            "이 마을은 평화로운 곳입니다. 많은 모험가들이 찾아오죠.",
            "정말 멋진 마을이네요! 감사합니다."
        };
        [SerializeField] private bool[] testCharacterPositions = { false, true, false, true }; // false: 왼쪽, true: 오른쪽
        
        [Header("테스트용 노드 기반 대화 데이터")]
        [SerializeField] private DialogueGraph testDialogueGraph;
        
        [Header("참조")]
        [SerializeField] private StoryPanel storyPanel;
        
        [Header("테스트용 이미지")]
        [SerializeField] private Sprite[] testCharacterSprites; // 테스트용 캐릭터 스프라이트들
        [SerializeField] private Sprite[] testBackgroundSprites; // 테스트용 배경 스프라이트들
        [SerializeField] private Sprite[] testConstellationSprites; // 테스트용 별자리 스프라이트들

        private void Awake()
        {
            if (storyPanel == null)
            {
                storyPanel = GetComponent<StoryPanel>();
            }
        }

        #region ContextMenu 테스트 메서드들

        [ContextMenu("테스트 - 멀티 캐릭터 대화 시작")]
        public void StartTestMultiCharacterDialogue()
        {
            if (storyPanel != null)
            {
                storyPanel.StartMultiCharacterDialogue(testCharacterNames, testMultiDialogue, testCharacterPositions);
            }
            else
            {
                Debug.LogWarning("[StoryPanelTester] StoryPanel 참조가 설정되지 않았습니다.");
            }
        }
        
 
        [ContextMenu("테스트 - 중첩 선택지 대화 시작")]
        public void StartTestNestedChoiceDialogue()
        {
            if (storyPanel != null)
            {
                // 중첩 선택지 테스트용 대화 그래프 생성
                DialogueGraph nestedChoiceGraph = CreateNestedChoiceTestGraph();
                storyPanel.StartNodeBasedDialogue(nestedChoiceGraph);
            }
        }
        
        [ContextMenu("테스트 - 다중 대화 → 선택지 시퀀스")]
        public void StartTestMultiDialogueChoiceSequence()
        {
            if (storyPanel != null)
            {
                // 다중 대화 → 선택지 시퀀스 테스트용 대화 그래프 생성
                DialogueGraph multiDialogueChoiceGraph = CreateMultiDialogueChoiceSequenceGraph();
                storyPanel.StartNodeBasedDialogue(multiDialogueChoiceGraph);
            }
        }

        [ContextMenu("테스트 - 타이핑 효과")]
        public void TestTypingEffect()
        {
            if (storyPanel != null)
            {
                string testText = "이것은 타이핑 효과 테스트입니다. DoTween을 사용하여 구현되었습니다.";
                storyPanel.SetStoryTextWithTyping(testText, true);
            }
        }

        [ContextMenu("테스트 - 타이핑 효과 즉시 완료")]
        public void TestCompleteTyping()
        {
            if (storyPanel != null)
            {
                storyPanel.CompleteTyping();
            }
        }

        [ContextMenu("테스트 - 타이핑 효과 중지")]
        public void TestStopTyping()
        {
            if (storyPanel != null)
            {
                storyPanel.StopTyping();
            }
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            if (storyPanel != null)
            {
                Debug.Log("[StoryPanelTester] UI 요소 정보 출력 - StoryPanel 참조 확인됨");
            }
            else
            {
                Debug.LogWarning("[StoryPanelTester] StoryPanel 참조가 설정되지 않았습니다.");
            }
        }
        
        [ContextMenu("테스트 - 배경 이미지 설정")]
        public void TestBackgroundImage()
        {
            if (storyPanel != null && testBackgroundSprites.Length > 0)
            {
                // 랜덤하게 배경 이미지 선택
                Sprite randomBackground = testBackgroundSprites[Random.Range(0, testBackgroundSprites.Length)];
                storyPanel.SetBackgroundImage(randomBackground);
                Debug.Log($"[StoryPanelTester] 배경 이미지 설정: {randomBackground.name}");
            }
            else
            {
                Debug.LogWarning("[StoryPanelTester] StoryPanel 참조가 설정되지 않았거나 테스트용 배경 이미지가 없습니다.");
            }
        }
        
        [ContextMenu("테스트 - 배경 이미지 숨기기")]
        public void TestHideBackgroundImage()
        {
            if (storyPanel != null)
            {
                storyPanel.HideBackgroundImage();
                Debug.Log("[StoryPanelTester] 배경 이미지 숨김");
            }
        }
        
        [ContextMenu("테스트 - 캐릭터 이미지 설정")]
        public void TestCharacterImage()
        {
            if (storyPanel != null && testCharacterSprites.Length > 0)
            {
                // 랜덤하게 캐릭터 이미지 선택
                Sprite randomCharacter = testCharacterSprites[Random.Range(0, testCharacterSprites.Length)];
                storyPanel.SetCharacterImage(randomCharacter, Random.Range(0, 2) == 1);
                Debug.Log($"[StoryPanelTester] 캐릭터 이미지 설정: {randomCharacter.name}");
            }
            else
            {
                Debug.LogWarning("[StoryPanelTester] StoryPanel 참조가 설정되지 않았거나 테스트용 캐릭터 이미지가 없습니다.");
            }
        }
        
        [ContextMenu("테스트 - 모든 캐릭터 이미지 숨기기")]
        public void TestHideAllCharacterImages()
        {
            if (storyPanel != null)
            {
                storyPanel.HideAllCharacterImages();
                Debug.Log("[StoryPanelTester] 모든 캐릭터 이미지 숨김");
            }
        }
        
        [ContextMenu("테스트 - 이미지 투명도 조절")]
        public void TestImageTransparency()
        {
            if (storyPanel != null)
            {
                // 반투명 효과 적용
                Color transparentColor = new Color(1f, 1f, 1f, 0.5f);
                storyPanel.SetImageColor(transparentColor, StoryPanel.ImageType.All);
                Debug.Log("[StoryPanelTester] 모든 이미지 투명도 50%로 설정");
            }
        }
        
        [ContextMenu("테스트 - 이미지 색상 복원")]
        public void TestImageColorRestore()
        {
            if (storyPanel != null)
            {
                // 원래 색상으로 복원
                Color normalColor = Color.white;
                storyPanel.SetImageColor(normalColor, StoryPanel.ImageType.All);
                Debug.Log("[StoryPanelTester] 모든 이미지 색상 복원");
            }
        }
        
        [ContextMenu("테스트 - 별자리 이미지 설정")]
        public void TestConstellationImage()
        {
            if (storyPanel != null && testConstellationSprites.Length > 0)
            {
                // 랜덤하게 별자리 이미지 선택
                Sprite randomConstellation = testConstellationSprites[Random.Range(0, testConstellationSprites.Length)];
                storyPanel.SetConstellationImage(randomConstellation);
                Debug.Log($"[StoryPanelTester] 별자리 이미지 설정: {randomConstellation.name}");
            }
            else
            {
                Debug.LogWarning("[StoryPanelTester] StoryPanel 참조가 설정되지 않았거나 테스트용 별자리 이미지가 없습니다.");
            }
        }
        
        [ContextMenu("테스트 - 별자리 이미지 숨기기")]
        public void TestHideConstellationImage()
        {
            if (storyPanel != null)
            {
                storyPanel.HideConstellationImage();
                Debug.Log("[StoryPanelTester] 별자리 이미지 숨김");
            }
        }
        
        [ContextMenu("테스트 - 별자리 이미지 토글")]
        public void TestToggleConstellationImage()
        {
            if (storyPanel != null)
            {
                storyPanel.ToggleConstellationImage();
                Debug.Log("[StoryPanelTester] 별자리 이미지 토글");
            }
        }
        
        [ContextMenu("테스트 - 별자리 이미지 투명도 조절")]
        public void TestConstellationTransparency()
        {
            if (storyPanel != null)
            {
                // 반투명 효과 적용
                Color transparentColor = new Color(1f, 1f, 1f, 0.7f);
                storyPanel.SetImageColor(transparentColor, StoryPanel.ImageType.Constellation);
                Debug.Log("[StoryPanelTester] 별자리 이미지 투명도 70%로 설정");
            }
        }

        #endregion

        #region 테스트용 대화 그래프 생성 메서드들

        /// <summary>
        /// 다중 대화 → 선택지 시퀀스 테스트용 대화 그래프 생성
        /// </summary>
        private DialogueGraph CreateMultiDialogueChoiceSequenceGraph()
        {
            DialogueGraph graph = new DialogueGraph();
            graph.graphId = "multi_dialogue_choice_sequence";
            graph.startNodeId = "start";
            
            // 노드들 생성
            DialogueNode[] nodes = new DialogueNode[]
            {
                // 시작 노드
                CreateTestNode("start", "NPC", "안녕하세요! 오늘은 특별한 이야기를 들려드릴게요.", false, NodeType.Dialogue, 
                    new string[] { "choice_topic" }, new string[0]),
                
                // 주제 선택지
                CreateTestNode("choice_topic", "", "어떤 이야기를 듣고 싶으신가요?", false, NodeType.Choice,
                    new string[] { "multi_dialogue_story", "multi_dialogue_quest", "multi_dialogue_secret" },
                    new string[] { "모험 이야기", "퀘스트 정보", "비밀 정보" }),
                
                // 모험 이야기 다중 대화 시퀀스
                CreateTestNode("multi_dialogue_story", "NPC", "모험 이야기를 들려드리겠습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_story_2" }, new string[0]),
                CreateTestNode("multi_dialogue_story_2", "플레이어", "어떤 모험인가요?", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_story_3" }, new string[0]),
                CreateTestNode("multi_dialogue_story_3", "NPC", "전설의 보물을 찾는 모험입니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_story_4" }, new string[0]),
                CreateTestNode("multi_dialogue_story_4", "플레이어", "보물이 어디에 있나요?", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_story_5" }, new string[0]),
                CreateTestNode("multi_dialogue_story_5", "NPC", "고대 신전 깊숙한 곳에 숨겨져 있다고 합니다.", false, NodeType.Dialogue,
                    new string[] { "choice_after_story" }, new string[0]),
                
                // 퀘스트 정보 다중 대화 시퀀스
                CreateTestNode("multi_dialogue_quest", "NPC", "퀘스트 정보를 알려드리겠습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_quest_2" }, new string[0]),
                CreateTestNode("multi_dialogue_quest_2", "플레이어", "어떤 퀘스트가 있나요?", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_quest_3" }, new string[0]),
                CreateTestNode("multi_dialogue_quest_3", "NPC", "현재 3개의 특별한 퀘스트가 있습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_quest_4" }, new string[0]),
                CreateTestNode("multi_dialogue_quest_4", "플레이어", "어떤 퀘스트인가요?", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_quest_5" }, new string[0]),
                CreateTestNode("multi_dialogue_quest_5", "NPC", "드래곤 퇴치, 마법서 수집, 고대 유물 발굴입니다.", false, NodeType.Dialogue,
                    new string[] { "choice_after_quest" }, new string[0]),
                
                // 비밀 정보 다중 대화 시퀀스
                CreateTestNode("multi_dialogue_secret", "NPC", "비밀 정보를 알려드리겠습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_secret_2" }, new string[0]),
                CreateTestNode("multi_dialogue_secret_2", "플레이어", "비밀 정보라니...", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_secret_3" }, new string[0]),
                CreateTestNode("multi_dialogue_secret_3", "NPC", "이 마을에는 숨겨진 지하실이 있습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_secret_4" }, new string[0]),
                CreateTestNode("multi_dialogue_secret_4", "플레이어", "지하실에 뭐가 있나요?", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_secret_5" }, new string[0]),
                CreateTestNode("multi_dialogue_secret_5", "NPC", "고대 마법사의 연구실이 있다고 합니다.", false, NodeType.Dialogue,
                    new string[] { "choice_after_secret" }, new string[0]),
                
                // 다중 대화 후 선택지들
                CreateTestNode("choice_after_story", "", "모험에 대해 더 자세히 알고 싶은 것이 있나요?", false, NodeType.Choice,
                    new string[] { "story_detail_1", "story_detail_2", "choice_final" },
                    new string[] { "보물의 정체", "신전의 위치", "다른 주제로" }),
                
                CreateTestNode("choice_after_quest", "", "퀘스트에 대해 더 자세히 알고 싶은 것이 있나요?", false, NodeType.Choice,
                    new string[] { "quest_detail_1", "quest_detail_2", "choice_final" },
                    new string[] { "드래곤 퇴치", "마법서 수집", "다른 주제로" }),
                
                CreateTestNode("choice_after_secret", "", "비밀에 대해 더 자세히 알고 싶은 것이 있나요?", false, NodeType.Choice,
                    new string[] { "secret_detail_1", "secret_detail_2", "choice_final" },
                    new string[] { "지하실 입구", "마법사의 연구", "다른 주제로" }),
                
                // 상세 정보 노드들
                CreateTestNode("story_detail_1", "NPC", "보물은 전설의 검입니다. 모든 것을 베어낼 수 있다고 해요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                CreateTestNode("story_detail_2", "NPC", "신전은 마을 동쪽 숲 속에 있습니다. 하지만 찾기 어려워요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                
                CreateTestNode("quest_detail_1", "NPC", "드래곤은 마을 북쪽 산에 살고 있습니다. 레벨 50 이상이어야 해요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                CreateTestNode("quest_detail_2", "NPC", "마법서는 던전의 보물상자에서 나옵니다. 희귀한 마법이 담겨있어요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                
                CreateTestNode("secret_detail_1", "NPC", "지하실 입구는 마을장 집 뒤뜰에 있습니다. 비밀 문이 있어요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                CreateTestNode("secret_detail_2", "NPC", "마법사는 100년 전에 살았던 인물입니다. 강력한 마법을 연구했다고 해요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                
                // 최종 선택지
                CreateTestNode("choice_final", "", "더 궁금한 것이 있나요?", false, NodeType.Choice,
                    new string[] { "choice_topic", "end_dialogue" },
                    new string[] { "다른 주제로 돌아가기", "대화 종료" }),
                
                // 대화 종료
                CreateTestNode("end_dialogue", "NPC", "알겠습니다. 언제든 다시 오세요!", false, NodeType.Dialogue,
                    new string[0], new string[0])
            };
            
            graph.nodes = nodes;
            return graph;
        }
        
        /// <summary>
        /// 중첩 선택지 테스트용 대화 그래프 생성
        /// </summary>
        private DialogueGraph CreateNestedChoiceTestGraph()
        {
            DialogueGraph graph = new DialogueGraph();
            graph.graphId = "nested_choice_test";
            graph.startNodeId = "start";
            
            // 노드들 생성
            DialogueNode[] nodes = new DialogueNode[]
            {
                // 시작 노드
                CreateTestNode("start", "NPC", "안녕하세요! 무엇을 도와드릴까요?", false, NodeType.Dialogue, 
                    new string[] { "choice_main" }, new string[0]),
                
                // 메인 선택지 (4개 옵션)
                CreateTestNode("choice_main", "", "어떤 것을 원하시나요?", false, NodeType.Choice,
                    new string[] { "choice_info", "shop_branch", "quest_branch", "end_dialogue" },
                    new string[] { "정보를 얻고 싶어요", "아이템을 구매하고 싶어요", "퀘스트를 받고 싶어요", "아무것도 필요 없어요" }),
                
                // 정보 분기 선택지 (3개 옵션)
                CreateTestNode("choice_info", "", "어떤 정보를 원하시나요?", false, NodeType.Choice,
                    new string[] { "multi_dialogue_village", "multi_dialogue_dungeon", "multi_dialogue_npc" },
                    new string[] { "마을에 대해", "던전에 대해", "NPC에 대해" }),
                
                // 마을 정보 다중 대화 시퀀스
                CreateTestNode("multi_dialogue_village", "NPC", "마을에 대해 말씀드리겠습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_village_2" }, new string[0]),
                CreateTestNode("multi_dialogue_village_2", "플레이어", "정말요? 자세히 들려주세요.", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_village_3" }, new string[0]),
                CreateTestNode("multi_dialogue_village_3", "NPC", "이 마을은 평화로운 곳입니다. 많은 모험가들이 찾아오죠.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_village_4" }, new string[0]),
                CreateTestNode("multi_dialogue_village_4", "플레이어", "흥미롭네요! 더 자세한 정보가 있나요?", true, NodeType.Dialogue,
                    new string[] { "choice_after_village" }, new string[0]),
                
                // 던전 정보 다중 대화 시퀀스
                CreateTestNode("multi_dialogue_dungeon", "NPC", "던전에 대해 말씀드리겠습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_dungeon_2" }, new string[0]),
                CreateTestNode("multi_dialogue_dungeon_2", "플레이어", "던전이요? 어디에 있나요?", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_dungeon_3" }, new string[0]),
                CreateTestNode("multi_dialogue_dungeon_3", "NPC", "던전은 마을 북쪽에 있습니다. 위험하니 주의하세요.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_dungeon_4" }, new string[0]),
                CreateTestNode("multi_dialogue_dungeon_4", "플레이어", "어떤 몬스터가 있나요?", true, NodeType.Dialogue,
                    new string[] { "choice_after_dungeon" }, new string[0]),
                
                // NPC 정보 다중 대화 시퀀스
                CreateTestNode("multi_dialogue_npc", "NPC", "NPC에 대해 말씀드리겠습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_npc_2" }, new string[0]),
                CreateTestNode("multi_dialogue_npc_2", "플레이어", "어떤 NPC들이 있나요?", true, NodeType.Dialogue,
                    new string[] { "multi_dialogue_npc_3" }, new string[0]),
                CreateTestNode("multi_dialogue_npc_3", "NPC", "상점 주인, 퀘스트 마스터, 마을장, 여관 주인이 있습니다.", false, NodeType.Dialogue,
                    new string[] { "multi_dialogue_npc_4" }, new string[0]),
                CreateTestNode("multi_dialogue_npc_4", "플레이어", "각각 어떤 일을 하는 분들인가요?", true, NodeType.Dialogue,
                    new string[] { "choice_after_npc" }, new string[0]),
                
                // 다중 대화 후 선택지들
                CreateTestNode("choice_after_village", "", "마을에 대해 더 자세히 알고 싶은 것이 있나요?", false, NodeType.Choice,
                    new string[] { "village_detail_1", "village_detail_2", "choice_final" },
                    new string[] { "마을의 역사", "마을의 특산품", "다른 주제로" }),
                
                CreateTestNode("choice_after_dungeon", "", "던전에 대해 더 자세히 알고 싶은 것이 있나요?", false, NodeType.Choice,
                    new string[] { "dungeon_detail_1", "dungeon_detail_2", "choice_final" },
                    new string[] { "던전의 깊이", "던전의 보물", "다른 주제로" }),
                
                CreateTestNode("choice_after_npc", "", "NPC에 대해 더 자세히 알고 싶은 것이 있나요?", false, NodeType.Choice,
                    new string[] { "npc_detail_1", "npc_detail_2", "choice_final" },
                    new string[] { "상점 주인", "퀘스트 마스터", "다른 주제로" }),
                
                // 상세 정보 노드들
                CreateTestNode("village_detail_1", "NPC", "마을의 역사는 100년이 넘었습니다. 처음에는 작은 정착지였지만...", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                CreateTestNode("village_detail_2", "NPC", "마을의 특산품은 신비한 약초입니다. 마법의 힘이 깃들어 있다고 해요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                
                CreateTestNode("dungeon_detail_1", "NPC", "던전은 총 10층으로 구성되어 있습니다. 깊을수록 강한 몬스터가 나와요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                CreateTestNode("dungeon_detail_2", "NPC", "던전에는 전설의 무기가 숨겨져 있다고 합니다. 하지만 찾기 어려워요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                
                CreateTestNode("npc_detail_1", "NPC", "상점 주인은 20년간 이 마을에서 장사를 해왔습니다. 정직하고 신뢰할 수 있어요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                CreateTestNode("npc_detail_2", "NPC", "퀘스트 마스터는 전설의 모험가입니다. 수많은 퀘스트를 완료했다고 해요.", false, NodeType.Dialogue,
                    new string[] { "choice_final" }, new string[0]),
                
                // 최종 선택지
                CreateTestNode("choice_final", "", "더 궁금한 것이 있나요?", false, NodeType.Choice,
                    new string[] { "choice_main", "end_dialogue" },
                    new string[] { "다른 주제로 돌아가기", "대화 종료" }),
                
                // 상점 분기
                CreateTestNode("shop_branch", "NPC", "상점을 열어드리겠습니다. 무엇을 구매하고 싶으신가요?", false, NodeType.Dialogue,
                    new string[] { "end_dialogue" }, new string[0]),
                
                // 퀘스트 분기
                CreateTestNode("quest_branch", "NPC", "퀘스트를 받으시겠습니까? 어떤 퀘스트를 원하시나요?", false, NodeType.Dialogue,
                    new string[] { "end_dialogue" }, new string[0]),
                
                // 대화 종료
                CreateTestNode("end_dialogue", "NPC", "알겠습니다. 언제든 다시 오세요!", false, NodeType.Dialogue,
                    new string[0], new string[0])
            };
            
            graph.nodes = nodes;
            return graph;
        }
        
        /// <summary>
        /// 테스트용 노드 생성 헬퍼 메서드
        /// </summary>
        private DialogueNode CreateTestNode(string nodeId, string characterName, string dialogueText, bool isRightCharacter, 
            NodeType nodeType, string[] nextNodeIds, string[] choiceTexts)
        {
            DialogueNode node = new DialogueNode();
            node.nodeId = nodeId;
            node.characterName = characterName;
            node.dialogueText = dialogueText;
            node.isRightCharacter = isRightCharacter;
            node.nodeType = nodeType;
            node.nextNodeIds = nextNodeIds;
            node.choiceTexts = choiceTexts;
            return node;
        }
        
        #endregion
    }
}

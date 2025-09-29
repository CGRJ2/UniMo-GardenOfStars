using System;
using UnityEngine;

namespace KYS.DialogueSystem
{
    /// <summary>
    /// 대화 노드 데이터 구조
    /// </summary>
    [System.Serializable]
    public class DialogueNode
    {
        [Header("노드 기본 정보")]
        public string nodeId; // 노드 고유 ID
        public string characterName; // 캐릭터 이름
        public string dialogueText; // 대화 텍스트
        public bool isRightCharacter; // 오른쪽 캐릭터 여부 (false: 왼쪽, true: 오른쪽)
        
        [Header("노드 타입")]
        public NodeType nodeType; // 노드 타입
        
        [Header("다음 노드 정보")]
        public string[] nextNodeIds; // 다음 노드 ID 배열 (분기용)
        public string[] choiceTexts; // 선택지 텍스트 (선택지 노드용)
        
        [Header("조건 및 효과")]
        public DialogueCondition[] conditions; // 노드 표시 조건
        public DialogueEffect[] effects; // 노드 실행 시 효과
        
        [Header("UI 설정")]
        public bool useTypingEffect = true; // 타이핑 효과 사용 여부
        public float typingSpeed = 0.05f; // 타이핑 속도
        
        public DialogueNode()
        {
            nodeId = "";
            characterName = "";
            dialogueText = "";
            isRightCharacter = false;
            nodeType = NodeType.Dialogue;
            nextNodeIds = new string[0];
            choiceTexts = new string[0];
            conditions = new DialogueCondition[0];
            effects = new DialogueEffect[0];
        }
    }
    
    /// <summary>
    /// 노드 타입
    /// </summary>
    public enum NodeType
    {
        Dialogue,       // 일반 대화
        Choice,         // 선택지
        StartChoice,    // startchoice (choice와 동일하게 동작)
        Story,          // 스토리 (캐릭터 없음)
        Event,          // 이벤트 트리거
        End             // 대화 종료
    }
    
    /// <summary>
    /// 대화 조건
    /// </summary>
    [System.Serializable]
    public class DialogueCondition
    {
        public ConditionType conditionType;
        public string conditionKey;
        public string conditionValue;
        
        public enum ConditionType
        {
            QuestProgress,    // 퀘스트 진행도
            ItemOwned,        // 아이템 보유
            FlagSet,          // 플래그 설정
            Level,            // 레벨
            Custom            // 커스텀
        }
    }
    
    /// <summary>
    /// 대화 효과
    /// </summary>
    [System.Serializable]
    public class DialogueEffect
    {
        public EffectType effectType;
        public string effectKey;
        public string effectValue;
        
        public enum EffectType
        {
            SetFlag,          // 플래그 설정
            GiveItem,         // 아이템 지급
            StartQuest,       // 퀘스트 시작
            ChangeRelationship, // 호감도 변경
            PlaySound,        // 사운드 재생
            Custom            // 커스텀
        }
    }
}

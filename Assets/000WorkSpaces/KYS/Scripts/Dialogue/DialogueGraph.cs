using System;
using System.Collections.Generic;
using UnityEngine;

namespace KYS.DialogueSystem
{
    /// <summary>
    /// 대화 그래프 관리자
    /// </summary>
    [System.Serializable]
    public class DialogueGraph
    {
        [Header("대화 그래프 정보")]
        public string graphId; // 그래프 고유 ID
        public string startNodeId; // 시작 노드 ID
        
        [Header("노드 데이터")]
        public DialogueNode[] nodes; // 모든 노드 배열
        
        // 노드 검색을 위한 딕셔너리
        private Dictionary<string, DialogueNode> nodeDictionary;
        
        public DialogueGraph()
        {
            graphId = "";
            startNodeId = "";
            nodes = new DialogueNode[0];
            nodeDictionary = new Dictionary<string, DialogueNode>();
        }
        
        /// <summary>
        /// 그래프 초기화 (딕셔너리 생성)
        /// </summary>
        public void Initialize()
        {
            nodeDictionary.Clear();
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.nodeId))
                {
                    nodeDictionary[node.nodeId] = node;
                }
            }
        }
        
        /// <summary>
        /// 노드 ID로 노드 찾기
        /// </summary>
        public DialogueNode GetNode(string nodeId)
        {
            if (nodeDictionary.TryGetValue(nodeId, out DialogueNode node))
            {
                return node;
            }
            return null;
        }
        
        /// <summary>
        /// 시작 노드 가져오기
        /// </summary>
        public DialogueNode GetStartNode()
        {
            return GetNode(startNodeId);
        }
        
        /// <summary>
        /// 노드가 존재하는지 확인
        /// </summary>
        public bool HasNode(string nodeId)
        {
            return nodeDictionary.ContainsKey(nodeId);
        }
        
        /// <summary>
        /// 모든 노드 ID 가져오기
        /// </summary>
        public string[] GetAllNodeIds()
        {
            string[] ids = new string[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                ids[i] = nodes[i].nodeId;
            }
            return ids;
        }
        
        /// <summary>
        /// 노드 조건 검사
        /// </summary>
        public bool CheckNodeConditions(DialogueNode node)
        {
            if (node.conditions == null || node.conditions.Length == 0)
                return true; // 조건이 없으면 항상 true
                
            foreach (var condition in node.conditions)
            {
                if (!CheckSingleCondition(condition))
                    return false; // 하나라도 false면 false
            }
            return true;
        }
        
        /// <summary>
        /// 단일 조건 검사
        /// </summary>
        private bool CheckSingleCondition(DialogueCondition condition)
        {
            // TODO: 실제 게임 데이터와 연동하여 조건 검사 구현
            switch (condition.conditionType)
            {
                case DialogueCondition.ConditionType.QuestProgress:
                    // 퀘스트 진행도 검사
                    return CheckQuestProgress(condition.conditionKey, condition.conditionValue);
                    
                case DialogueCondition.ConditionType.ItemOwned:
                    // 아이템 보유 검사
                    return CheckItemOwned(condition.conditionKey, condition.conditionValue);
                    
                case DialogueCondition.ConditionType.FlagSet:
                    // 플래그 설정 검사
                    return CheckFlagSet(condition.conditionKey, condition.conditionValue);
                    
                case DialogueCondition.ConditionType.Level:
                    // 레벨 검사
                    return CheckLevel(condition.conditionKey, condition.conditionValue);
                    
                default:
                    return true; // 기본값
            }
        }
        
        /// <summary>
        /// 퀘스트 진행도 검사 (임시 구현)
        /// </summary>
        private bool CheckQuestProgress(string questId, string requiredProgress)
        {
            // TODO: 실제 퀘스트 시스템과 연동
            Debug.Log($"[DialogueGraph] 퀘스트 진행도 검사: {questId} >= {requiredProgress}");
            return true;
        }
        
        /// <summary>
        /// 아이템 보유 검사 (임시 구현)
        /// </summary>
        private bool CheckItemOwned(string itemId, string requiredCount)
        {
            // TODO: 실제 인벤토리 시스템과 연동
            Debug.Log($"[DialogueGraph] 아이템 보유 검사: {itemId} >= {requiredCount}");
            return true;
        }
        
        /// <summary>
        /// 플래그 설정 검사 (임시 구현)
        /// </summary>
        private bool CheckFlagSet(string flagKey, string expectedValue)
        {
            // TODO: 실제 플래그 시스템과 연동
            Debug.Log($"[DialogueGraph] 플래그 검사: {flagKey} == {expectedValue}");
            return true;
        }
        
        /// <summary>
        /// 레벨 검사 (임시 구현)
        /// </summary>
        private bool CheckLevel(string levelType, string requiredLevel)
        {
            // TODO: 실제 레벨 시스템과 연동
            Debug.Log($"[DialogueGraph] 레벨 검사: {levelType} >= {requiredLevel}");
            return true;
        }
        
        /// <summary>
        /// 노드 효과 실행
        /// </summary>
        public void ExecuteNodeEffects(DialogueNode node)
        {
            if (node.effects == null || node.effects.Length == 0)
                return;
                
            foreach (var effect in node.effects)
            {
                ExecuteSingleEffect(effect);
            }
        }
        
        /// <summary>
        /// 단일 효과 실행
        /// </summary>
        private void ExecuteSingleEffect(DialogueEffect effect)
        {
            // TODO: 실제 게임 시스템과 연동하여 효과 실행 구현
            switch (effect.effectType)
            {
                case DialogueEffect.EffectType.SetFlag:
                    SetFlag(effect.effectKey, effect.effectValue);
                    break;
                    
                case DialogueEffect.EffectType.GiveItem:
                    GiveItem(effect.effectKey, effect.effectValue);
                    break;
                    
                case DialogueEffect.EffectType.StartQuest:
                    StartQuest(effect.effectKey, effect.effectValue);
                    break;
                    
                case DialogueEffect.EffectType.ChangeRelationship:
                    ChangeRelationship(effect.effectKey, effect.effectValue);
                    break;
                    
                case DialogueEffect.EffectType.PlaySound:
                    PlaySound(effect.effectKey, effect.effectValue);
                    break;
                    
                default:
                    Debug.Log($"[DialogueGraph] 알 수 없는 효과 타입: {effect.effectType}");
                    break;
            }
        }
        
        /// <summary>
        /// 플래그 설정 (임시 구현)
        /// </summary>
        private void SetFlag(string flagKey, string flagValue)
        {
            // TODO: 실제 플래그 시스템과 연동
            Debug.Log($"[DialogueGraph] 플래그 설정: {flagKey} = {flagValue}");
        }
        
        /// <summary>
        /// 아이템 지급 (임시 구현)
        /// </summary>
        private void GiveItem(string itemId, string count)
        {
            // TODO: 실제 인벤토리 시스템과 연동
            Debug.Log($"[DialogueGraph] 아이템 지급: {itemId} x{count}");
        }
        
        /// <summary>
        /// 퀘스트 시작 (임시 구현)
        /// </summary>
        private void StartQuest(string questId, string questData)
        {
            // TODO: 실제 퀘스트 시스템과 연동
            Debug.Log($"[DialogueGraph] 퀘스트 시작: {questId}");
        }
        
        /// <summary>
        /// 호감도 변경 (임시 구현)
        /// </summary>
        private void ChangeRelationship(string characterId, string changeValue)
        {
            // TODO: 실제 호감도 시스템과 연동
            Debug.Log($"[DialogueGraph] 호감도 변경: {characterId} += {changeValue}");
        }
        
        /// <summary>
        /// 사운드 재생 (임시 구현)
        /// </summary>
        private void PlaySound(string soundId, string volume)
        {
            // TODO: 실제 오디오 시스템과 연동
            Debug.Log($"[DialogueGraph] 사운드 재생: {soundId}, 볼륨: {volume}");
        }
    }
}

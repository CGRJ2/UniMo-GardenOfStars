using GameNpc;
using GameQuest;
using System;
using System.Collections;
using UnityEngine;

public class QuestRequireTile : InteractableBase
{
    public Action RequirementCompleteAction;

    public QuestContentProgressData requirement; // <= 현재 퀘스트 조건 중 하나 할당해주기
    [SerializeField] Transform attachPoint;
    [SerializeField] float insertDelayTime = 0.1f;

    public void UpdateView()
    {

    }

    IEnumerator AutoInserting()
    {
        while (characterRD != null)
        {
            // 플레이어 손에 있는 재료가 퀘스트 조건에 포함되는지 체크
            IngrediantInstance instanceProd;
            if (characterRD.IngrediantStack.TryPeek(out instanceProd))
            {
                // 퀘스트 조건이 할당되지 않은 발판이라면
                if (requirement == null) { Debug.LogError("해당 퀘스트 발판에 퀘스트 조건 데이터가 할당되지 않음"); break; }

                // 손에 있는 재료가 퀘스트 조건이 아니면 || 퀘스트가 이미 완료된 상황이면
                if (instanceProd.Data.ID != requirement.ContentTargetId || requirement.State == QuestProgressState.Completed)
                {
                    break;  // 상호작용 취소
                }

                
                // 현재 진행도에 개수 추가
                if (requirement.Count < requirement.ContentTargetCount)
                {
                    GetComponentInParent<NpcController>()?.ReceiveProduct(requirement.ContentTargetId);
                    IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                    popedProd.MoveToTargetAndShrink(attachPoint);
                }
                else // 필요 재료 수량만큼 다 넣으면 조건 완료처리 후 정지
                {
                    break;
                }

                // 다음 투입까지 딜레이 시간 설정
                yield return new WaitForSeconds(insertDelayTime);

            }
            // 플레이어 손에 재료가 없으면 바로 return
            else
            {
                yield return null;
            }
        }
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);

        StartCoroutine(AutoInserting());
    }
}

using System.Collections;
using UnityEngine;

public class QuestProdsInsertArea : InteractableBase
{
    public ConstellationBuilding ownerInstance;
    [SerializeField] Transform attachPoint;
    [SerializeField] float insertDelayTime = 0.1f;
    // 데이터 구조 설계할 때 수정
    public void Init(ConstellationBuilding instance)
    {
        this.ownerInstance = instance;
        //Manager.buildings.workStatinLists.insertAreas.Add(this);
    }

    IEnumerator AutoInserting()
    {
        while (characterRD != null)
        {

            // 플레이어 손에 있는 재료가 퀘스트 조건에 포함되는지 체크
            IngrediantInstance instanceProd;
            if (characterRD.IngrediantStack.TryPeek(out instanceProd))
            {
                Temp_Requirement targetRequirement = null;
                foreach (Temp_Requirement requirement in ownerInstance.requirements)
                {
                    // 손에 있는 재료가 퀘스트 조건에 있는 재료이고 && 충족되지 않은 상황이면
                    if (instanceProd.Data.ID == requirement.prodId && !requirement.isClear)
                    {
                        targetRequirement = requirement;    // 타겟으로 설정
                        break;
                    }
                }


                if (targetRequirement != null)
                {
                    // 현재 진행도에 개수 추가
                    if (targetRequirement.curCount < targetRequirement.needCount)
                    {
                        IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                        targetRequirement.curCount += 1;
                        popedProd.MoveToTargetAndShrink(attachPoint);
                    }
                    else // 필요 재료 수량만큼 다 넣으면 조건 완료처리 후 정지
                    {
                        targetRequirement.isClear = true;
                        break;
                    }
                }

                // 다음 투입까지 딜레이 시간 설정
                yield return new WaitForSeconds(insertDelayTime);

                // 재료 투입 후 퀘스트 상태 업데이트
                // 
            }
            // 플레이어 손에 재료가 없으면 바로 return
            else
            {
                yield return null;
            }
        }
    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        //Debug.Log($"건물재료삽입영역({buildingInstance.name}): 즉발형 상호작용 실행");

        StartCoroutine(AutoInserting());
    }


    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //Debug.Log($"건물재료삽입영역({buildingInstance.name}): 팝업형 상호작용 비활성화");
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        //Manager.buildings?.workStatinLists.insertAreas?.Remove(this);
    }
}

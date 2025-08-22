using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Insert : InteractableBase
{
    BuildingInstance_Work buildingInstance;
    // 데이터 구조 설계할 때 수정
    public void Init(BuildingInstance_Work buildingInstance)
    {
        this.buildingInstance = buildingInstance;
    }

    public override void ImediateInteract()
    {
        base.ImediateInteract();
        Debug.Log($"건물재료삽입영역({buildingInstance.name}): 즉발형 상호작용 실행");

        IngrediantInstance instanceProd;
        // 플레이어 손에 아무것도 없으면 return
        if (!pc.ingrediantStack.TryPop(out instanceProd)) return;

        // 맨 위의 재료와 투입 가능 재료가 같은 종류일 때 넣어주기
        if (instanceProd.ingrediantSO == buildingInstance.insertableProdData)
        {
            instanceProd.AttachToTarget(buildingInstance.attachPoint, buildingInstance.prodsStack.Count);

            buildingInstance.prodsStack.Push(instanceProd);
        }

    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        Debug.Log($"건물재료삽입영역({buildingInstance.name}): 팝업형 상호작용 활성화");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();
        Debug.Log($"건물재료삽입영역({buildingInstance.name}): 팝업형 상호작용 비활성화");
    }
}

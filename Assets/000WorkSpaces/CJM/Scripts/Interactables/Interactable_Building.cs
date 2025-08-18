using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Building : InteractableBase
{
    BuildingInstance buildingInstance;

    public override void ImediateInteract()
    {
        base.ImediateInteract();
        Debug.Log($"건물인스턴스({buildingInstance.name}): 즉발형 상호작용 실행");
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        Debug.Log($"건물인스턴스({buildingInstance.name}): 팝업형 상호작용 활성화");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();
        Debug.Log($"건물인스턴스({buildingInstance.name}): 팝업형 상호작용 비활성화");
    }
}

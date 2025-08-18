using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Ingrediant : InteractableBase
{
    IngrediantInstance ingrediantInstance;

    private void Awake()
    {
        ingrediantInstance = GetComponent<IngrediantInstance>();
    }

    public override void ImediateInteract()
    {
        base.ImediateInteract();
        Debug.Log($"재료 인스턴스({ingrediantInstance.ingrediantSO.Name}): 즉발형 상호작용 실행");

        // 임시
        transform.SetParent(pc.transform); 
        ingrediantInstance.owner = pc.gameObject;
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        Debug.Log($"재료 인스턴스({ingrediantInstance.ingrediantSO.Name}): 팝업형 상호작용 활성화");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();
        Debug.Log($"재료 인스턴스({ingrediantInstance.ingrediantSO.Name}): 팝업형 상호작용 비활성화");
    }
}

using System.Collections;
using UnityEngine;

// 풀 오브젝트를 인스턴스에서 상속받기 때문에 상호작용 역할을 별도의 MonoBehavior 클래스로 적용함
public class Interactable_Ingrediant : InteractableBase
{
    IngrediantInstance ingrediantInstance;

    bool isInteracted = false;
    bool isAttached = false;


    public void SetInstance(IngrediantInstance instance)
    {
        ingrediantInstance = instance;
    }

    
    public override void ImediateInteract()
    {
        base.ImediateInteract();

        // 1회 상호작용 후 막기
        if (isInteracted) return;
        isInteracted = true;

        Debug.Log($"재료 인스턴스({ingrediantInstance?.ingrediantSO?.Name}): 즉발형 상호작용 실행");

        // 플레이어 위치로 설정
        ingrediantInstance.owner = pc.gameObject;
        ingrediantInstance.AttachToTarget(pc.prodsAttachPoint, pc.ingrediantStack.Count);
        pc.ingrediantStack.Push(ingrediantInstance);
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        
        // 1회 상호작용 후 막기
        if (isInteracted) return;
        isInteracted = true;

        //Debug.Log($"재료 인스턴스({ingrediantInstance.ingrediantSO.Name}): 팝업형 상호작용 활성화");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();

        // 1회 상호작용 후 막기
        if (isInteracted) return;
        isInteracted = true;

        //Debug.Log($"재료 인스턴스({ingrediantInstance.ingrediantSO.Name}): 팝업형 상호작용 비활성화");
    }
}

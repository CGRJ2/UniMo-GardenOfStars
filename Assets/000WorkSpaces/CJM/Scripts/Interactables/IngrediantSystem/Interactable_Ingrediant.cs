using System.Collections;
using UnityEngine;

// 풀 오브젝트를 인스턴스에서 상속받기 때문에 상호작용 역할을 별도의 MonoBehavior 클래스로 적용함
public class Interactable_Ingrediant : InteractableBase
{
    IngrediantInstance ingrediantInstance;

    public bool isInteracted = false;
    //bool isAttached = false;


    public void SetInstance(IngrediantInstance instance)
    {
        ingrediantInstance = instance;
    }

    public void PickUpProds()
    {
        // 일꾼일 경우에도 추가해야함

        // 들고 있는 재료와 다른 재료는 줍지 않게 만들기 ---------------------------------
        /*IngrediantInstance instanceProd;
        if (pc.ingrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd.Data != ingrediantInstance.Data) return;
        }*/
        //---------------------------------------------------------------

        ingrediantInstance.owner = pc.gameObject; 
        ingrediantInstance.AttachToTarget(pc.prodsAttachPoint, pc.ingrediantStack.Count);
        //Debug.Log($"{pc.ingrediantStack.Count}번째 위치로");
        pc.ingrediantStack.Push(ingrediantInstance);
    }
    
    public override void EnterInteract_Player()
    {
        base.EnterInteract_Player();

        // 1회 상호작용 후 막기
        if (isInteracted) return;
        isInteracted = true;

        PickUpProds();
    }


    public override void DeactiveInteract_Player()
    {
        base.DeactiveInteract_Player();

        // 1회 상호작용 후 막기
        if (isInteracted) return;
        isInteracted = true;

    }
}

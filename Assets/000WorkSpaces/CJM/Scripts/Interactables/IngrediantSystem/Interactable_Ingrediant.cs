using System.Collections;
using UnityEngine;

// Ǯ ������Ʈ�� �ν��Ͻ����� ��ӹޱ� ������ ��ȣ�ۿ� ������ ������ MonoBehavior Ŭ������ ������
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
        // �ϲ��� ��쿡�� �߰��ؾ���

        // ��� �ִ� ���� �ٸ� ���� ���� �ʰ� ����� ---------------------------------
        /*IngrediantInstance instanceProd;
        if (pc.ingrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd.Data != ingrediantInstance.Data) return;
        }*/
        //---------------------------------------------------------------

        ingrediantInstance.owner = pc.gameObject; 
        ingrediantInstance.AttachToTarget(pc.prodsAttachPoint, pc.ingrediantStack.Count);
        //Debug.Log($"{pc.ingrediantStack.Count}��° ��ġ��");
        pc.ingrediantStack.Push(ingrediantInstance);
    }
    
    public override void EnterInteract_Player()
    {
        base.EnterInteract_Player();

        // 1ȸ ��ȣ�ۿ� �� ����
        if (isInteracted) return;
        isInteracted = true;

        PickUpProds();
    }


    public override void DeactiveInteract_Player()
    {
        base.DeactiveInteract_Player();

        // 1ȸ ��ȣ�ۿ� �� ����
        if (isInteracted) return;
        isInteracted = true;

    }
}

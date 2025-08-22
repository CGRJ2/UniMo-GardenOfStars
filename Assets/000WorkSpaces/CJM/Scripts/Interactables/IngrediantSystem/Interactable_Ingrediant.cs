using System.Collections;
using UnityEngine;

// Ǯ ������Ʈ�� �ν��Ͻ����� ��ӹޱ� ������ ��ȣ�ۿ� ������ ������ MonoBehavior Ŭ������ ������
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

        // 1ȸ ��ȣ�ۿ� �� ����
        if (isInteracted) return;
        isInteracted = true;

        Debug.Log($"��� �ν��Ͻ�({ingrediantInstance?.ingrediantSO?.Name}): ����� ��ȣ�ۿ� ����");

        // �÷��̾� ��ġ�� ����
        ingrediantInstance.owner = pc.gameObject;
        ingrediantInstance.AttachToTarget(pc.prodsAttachPoint, pc.ingrediantStack.Count);
        pc.ingrediantStack.Push(ingrediantInstance);
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        
        // 1ȸ ��ȣ�ۿ� �� ����
        if (isInteracted) return;
        isInteracted = true;

        //Debug.Log($"��� �ν��Ͻ�({ingrediantInstance.ingrediantSO.Name}): �˾��� ��ȣ�ۿ� Ȱ��ȭ");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();

        // 1ȸ ��ȣ�ۿ� �� ����
        if (isInteracted) return;
        isInteracted = true;

        //Debug.Log($"��� �ν��Ͻ�({ingrediantInstance.ingrediantSO.Name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}

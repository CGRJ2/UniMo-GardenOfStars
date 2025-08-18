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
        Debug.Log($"��� �ν��Ͻ�({ingrediantInstance.ingrediantSO.Name}): ����� ��ȣ�ۿ� ����");

        // �ӽ�
        transform.SetParent(pc.transform); 
        ingrediantInstance.owner = pc.gameObject;
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        Debug.Log($"��� �ν��Ͻ�({ingrediantInstance.ingrediantSO.Name}): �˾��� ��ȣ�ۿ� Ȱ��ȭ");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();
        Debug.Log($"��� �ν��Ͻ�({ingrediantInstance.ingrediantSO.Name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Building : InteractableBase
{
    BuildingInstance buildingInstance;

    public override void ImediateInteract()
    {
        base.ImediateInteract();
        Debug.Log($"�ǹ��ν��Ͻ�({buildingInstance.name}): ����� ��ȣ�ۿ� ����");
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        Debug.Log($"�ǹ��ν��Ͻ�({buildingInstance.name}): �˾��� ��ȣ�ۿ� Ȱ��ȭ");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();
        Debug.Log($"�ǹ��ν��Ͻ�({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}

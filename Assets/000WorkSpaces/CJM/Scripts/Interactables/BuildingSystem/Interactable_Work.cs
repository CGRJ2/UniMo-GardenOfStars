using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Work : InteractableBase
{
    BuildingInstance buildingInstance;
    // CharacterBase curWorker;
    public void Init(BuildingInstance_Work buildingInstance)
    {
        this.buildingInstance = buildingInstance;
    }

    public override void ImediateInteract()
    {
        base.ImediateInteract();
        Debug.Log($"�ǹ��۾�����({buildingInstance.name}): ����� ��ȣ�ۿ� ����");
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        Debug.Log($"�ǹ��۾�����({buildingInstance.name}): �˾��� ��ȣ�ۿ� Ȱ��ȭ");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();
        Debug.Log($"�ǹ��۾�����({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}

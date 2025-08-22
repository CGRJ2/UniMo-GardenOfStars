using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Insert : InteractableBase
{
    BuildingInstance_Work buildingInstance;
    // ������ ���� ������ �� ����
    public void Init(BuildingInstance_Work buildingInstance)
    {
        this.buildingInstance = buildingInstance;
    }

    public override void ImediateInteract()
    {
        base.ImediateInteract();
        Debug.Log($"�ǹ������Կ���({buildingInstance.name}): ����� ��ȣ�ۿ� ����");

        IngrediantInstance instanceProd;
        // �÷��̾� �տ� �ƹ��͵� ������ return
        if (!pc.ingrediantStack.TryPop(out instanceProd)) return;

        // �� ���� ���� ���� ���� ��ᰡ ���� ������ �� �־��ֱ�
        if (instanceProd.ingrediantSO == buildingInstance.insertableProdData)
        {
            instanceProd.AttachToTarget(buildingInstance.attachPoint, buildingInstance.prodsStack.Count);

            buildingInstance.prodsStack.Push(instanceProd);
        }

    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        Debug.Log($"�ǹ������Կ���({buildingInstance.name}): �˾��� ��ȣ�ۿ� Ȱ��ȭ");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();
        Debug.Log($"�ǹ������Կ���({buildingInstance.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}

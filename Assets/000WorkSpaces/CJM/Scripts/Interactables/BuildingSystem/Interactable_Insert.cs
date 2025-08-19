using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Insert : InteractableBase
{
    BuildingInstance buildingInstance;
    [SerializeField] List<IngrediantSO> InsertableSOList;

    public override void ImediateInteract()
    {
        base.ImediateInteract();
        Debug.Log($"�ǹ������Կ���({buildingInstance.name}): ����� ��ȣ�ۿ� ����");
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

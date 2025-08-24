using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstance : InteractableBase
{
    [SerializeField] BuildingSO buildingData;           // CSV or Sheet�� ���� ����


    public override void ImediateInteract()
    {
        base.ImediateInteract();
        Debug.Log($"�ǹ��ν��Ͻ�({buildingData?.name}): ����� ��ȣ�ۿ� ����");
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        Debug.Log($"�ǹ��ν��Ͻ�({buildingData?.name}): �˾��� ��ȣ�ۿ� Ȱ��ȭ");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();
        Debug.Log($"�ǹ��ν��Ͻ�({buildingData?.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}

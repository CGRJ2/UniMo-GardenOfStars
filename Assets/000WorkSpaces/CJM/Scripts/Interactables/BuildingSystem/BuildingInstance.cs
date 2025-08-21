using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstance : InteractableBase
{
    [SerializeField] BuildingSO buildingData;           // CSV or Sheet�� ���� ����


    public override void EnterInteract_Player()
    {
        base.EnterInteract_Player();
        //Debug.Log($"�ǹ��ν��Ͻ�({buildingData?.name}): ����� ��ȣ�ۿ� ����");
    }

    public override void DeactiveInteract_Player()
    {
        base.DeactiveInteract_Player();
        //Debug.Log($"�ǹ��ν��Ͻ�({buildingData?.name}): �˾��� ��ȣ�ۿ� ��Ȱ��ȭ");
    }
}

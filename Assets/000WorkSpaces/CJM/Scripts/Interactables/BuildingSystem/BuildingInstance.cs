using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInstance : InteractableBase
{
    [SerializeField] protected BuildingData _OriginData;           // CSV or Sheet�� ���� ����
    [SerializeField] protected BuildingActivePopUI activatePopUI;
    
    protected void BIBaseInit()
    {
        Manager.buildings.ActivatedBIList.Add(this);

        if (activatePopUI != null)
        {
            activatePopUI.GetComponent<Canvas>().worldCamera = Camera.main;
            activatePopUI.gameObject.SetActive(false);
        }
    }

    public override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.ActivatedBIList.Remove(this);

        if (activatePopUI != null)
            activatePopUI.gameObject.SetActive(false);
    }

    // �ǹ� Ȱ��ȭ ���� ��ȣ�ۿ�
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        
        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (activatePopUI != null)
                activatePopUI.gameObject.SetActive(true);  // �⺻ ��ȣ�ۿ� �˾� Ȱ��ȭ (���� �Ѵٸ�)
        }
    }

    // �ǹ� Ȱ��ȭ ���� ��ȣ�ۿ�
    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);

        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            if (activatePopUI != null)
                activatePopUI.gameObject.SetActive(false); // �⺻ ��ȣ�ۿ� �˾� ��Ȱ��ȭ (���� �Ѵٸ�)
        }
    }

    
}



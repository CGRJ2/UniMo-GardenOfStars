using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected CharaterRuntimeData characterRD;
    public CharaterRuntimeData personalTaskOwner;

    // ��ȣ�ۿ� ���� ����
    public virtual void Enter(CharaterRuntimeData characterRuntimeData)
    {
        characterRD = characterRuntimeData;
    }

    public virtual void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        personalTaskOwner = characterRuntimeData;
    }

    // ��ȣ�ۿ� �������� ����
    public virtual void Exit(CharaterRuntimeData characterRuntimeData)
    {
        if (characterRD == characterRuntimeData)
            characterRD = null;
    }

    public virtual void Exit_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        personalTaskOwner = null;
    }

    public virtual void OnDisableAdditionalActions() { }

    public void OnDisable()
    {
        OnDisableAdditionalActions();
    }

    private void OnTriggerEnter(Collider other)
    {
        CharaterRuntimeData _CharacterRD = other.transform.parent.GetComponent<CharaterRuntimeData>();

        if (_CharacterRD != null)
        {
            // �Ϲ� ��ȣ�ۿ� Ȱ��ȭ
            Enter(_CharacterRD);

            // 1�� ��ȣ�ۿ�(���� ��ȣ�ۿ� �������� ��ü�� ������ ����)
            if (personalTaskOwner == null)
                Enter_PersonalTask(_CharacterRD);
        }

    }

    private void OnTriggerStay(Collider other)
    {
        CharaterRuntimeData _CharacterRD = other.transform.parent.GetComponent<CharaterRuntimeData>();

        if (characterRD == null)
        {
            // �÷��̾�
            if (_CharacterRD != null)
            {
                characterRD ??= other.GetComponent<CharaterRuntimeData>();
                // �Ϲ� ��ȣ�ۿ� Ȱ��ȭ
                Enter(_CharacterRD);

                // 1�� ��ȣ�ۿ�(���� ��ȣ�ۿ� �������� ��ü�� ������ ����)
                if (personalTaskOwner == null)
                    Enter_PersonalTask(_CharacterRD);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CharaterRuntimeData _CharacterRD = other.transform.parent.GetComponent<CharaterRuntimeData>();

        if (_CharacterRD != null)
        {
            // �Ϲ� ��ȣ�ۿ� ��Ȱ��ȭ
            Exit(_CharacterRD);

            // 1�� ��ȣ�ۿ� ��Ȱ��ȭ(��� ���� ĳ���Ͱ� ��ȣ�ۿ����̴� ĳ���Ϳ�����)
            if (personalTaskOwner == _CharacterRD)
                Exit_PersonalTask(_CharacterRD);
        }
    }
}


public interface IWorkStation
{
    public bool GetWorkableState();
    public bool GetReserveState();
    public void SetReserveState(bool reserve);
}
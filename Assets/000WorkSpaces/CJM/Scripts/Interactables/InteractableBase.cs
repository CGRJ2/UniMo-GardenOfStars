using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected CharaterRuntimeData characterRD;
    public CharaterRuntimeData personalTaskOwner;


    [Header("�⺻ ��ȣ�ۿ� �˾�(���ٸ� �������� ����)")]
    [SerializeField] Canvas interactPopUI;
    //bool isInteractedd;
    public void InitPopUI()
    {
        if (interactPopUI != null)
        {
            interactPopUI.worldCamera = Camera.main;
            interactPopUI.gameObject.SetActive(false);
        }
    }

    // ��ȣ�ۿ� ���� ����
    public virtual void Enter(CharaterRuntimeData characterRuntimeData)
    {
        characterRD = characterRuntimeData;
        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(true);  // �⺻ ��ȣ�ۿ� �˾� Ȱ��ȭ (���� �Ѵٸ�)
    }


    public virtual void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        personalTaskOwner = characterRuntimeData;
    }


    // ��ȣ�ۿ� �������� ����
    public virtual void Exit(CharaterRuntimeData characterRuntimeData)
    {
        characterRD = null;
        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false); // �⺻ ��ȣ�ۿ� �˾� ��Ȱ��ȭ (���� �Ѵٸ�)
    }

    public virtual void Exit_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        personalTaskOwner = null;
    }

    public virtual void OnDisableAdditionalActions()
    {
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        OnDisableAdditionalActions();
    }

    private void OnTriggerEnter(Collider other)
    {
        CharaterRuntimeData _CharacterRD = other.transform.root.GetComponent<CharaterRuntimeData>();

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
        CharaterRuntimeData _CharacterRD = other.transform.root.GetComponent<CharaterRuntimeData>();


        // �÷��̾� ��ȣ�ۿ�(�켱����)

        // �̱� ��ȣ�ۿ� �ڸ��� ��� ���
        if (characterRD == null)
        {
            // �÷��̾�
            if (other.transform.root.GetComponent<CharaterRuntimeData>() != null)
            {
                characterRD ??= other.GetComponent<CharaterRuntimeData>();
                //EnterInteractSingleOnly(characterRD);
            }
        }
        else
        {
            if (this is InsertArea insert)
            {


            }

            if (this is BuildingInstance)
            {
                // �ǹ��̶�� �÷��̾ �۾��������� ������������ ��ȣ�ۿ� ����
                //Player�� IsMoving�� false��� �۾� ����
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        CharaterRuntimeData _CharacterRD = other.transform.root.GetComponent<CharaterRuntimeData>();

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

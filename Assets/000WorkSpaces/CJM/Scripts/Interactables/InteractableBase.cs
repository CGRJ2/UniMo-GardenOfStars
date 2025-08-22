using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected PlayerController characterRuntimeData;  // ���� ��ȣ�ۿ� ��ü�� Player�θ� �س��µ�. �÷��̾�� �ϲ� ��� ��ü�� �� �� �ֵ��� ���� �ʿ�


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
    public virtual void EnterInteract(PlayerController characterRuntimeData)
    {
        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(true);  // �⺻ ��ȣ�ۿ� �˾� Ȱ��ȭ (���� �Ѵٸ�)
    }


    public virtual void EnterInteractSingleOnly(PlayerController characterRuntimeData)
    {

    }


    // ��ȣ�ۿ� �������� ����
    public virtual void ExitInteract(PlayerController characterRuntimeData)
    {
        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false); // �⺻ ��ȣ�ۿ� �˾� ��Ȱ��ȭ (���� �Ѵٸ�)
    }


    public virtual void OnDisableAdditionalActions()
    {
        ExitInteract(characterRuntimeData);
    }

    public void OnDisable()
    {
        OnDisableAdditionalActions();
    }

    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾� ��ȣ�ۿ�
        if (other.GetComponent<PlayerController>() != null)
        {
            // �Ϲ� ��ȣ�ۿ�
            EnterInteract(characterRuntimeData); // �÷��̾� ���� ��ȣ�ۿ� ����

            // �̱� ��ȣ�ۿ�(���� ��ȣ�ۿ� �������� ��ü�� ������ ����)
            if (this.characterRuntimeData != null) return;
            EnterInteractSingleOnly(characterRuntimeData);
        }

        // �ϲ� ��ȣ�ۿ�
    }

    private void OnTriggerStay(Collider other)
    {
        // �÷��̾� ��ȣ�ۿ�(�켱����)

        // �̱� ��ȣ�ۿ� �ڸ��� ��� ���
        if (characterRuntimeData == null)
        {
            // �÷��̾�
            if (other.GetComponent<PlayerController>() != null)
            { 
                characterRuntimeData ??= other.GetComponent<PlayerController>();
                EnterInteractSingleOnly(characterRuntimeData);
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

        // �ϲ� ��ȣ�ۿ�
    }

    private void OnTriggerExit(Collider other)
    {
        // �÷��̾� ��ȣ�ۿ�
        if (other.GetComponent<PlayerController>() != null)
        {
            PlayerController pc = other.GetComponent<PlayerController>();

            if (characterRuntimeData == pc) // �̱� ��ȣ�ۿ� ���̴� ��ü�� ���� ���� �÷��̾���
                characterRuntimeData = null;
        }

        // �ϲ� ��ȣ�ۿ�
        ExitInteract(characterRuntimeData); // �˾� ��Ȱ��ȭ
    }
}

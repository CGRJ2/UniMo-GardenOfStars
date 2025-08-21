using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected PlayerController pc;  // ���� ��ȣ�ۿ� ��ü�� Player�θ� �س��µ�. �÷��̾�� �ϲ� ��� ��ü�� �� �� �ֵ��� ���� �ʿ�

    [Header("�⺻ ��ȣ�ۿ� �˾�(���ٸ� �������� ����)")]
    [SerializeField] Canvas interactPopUI;
    bool isInteractedd;

    public void InitPopUI()
    {
        if (interactPopUI != null)
        {
            interactPopUI.worldCamera = Camera.main;
            interactPopUI.gameObject.SetActive(false);
        }
    }

    // ��ȣ�ۿ� ���� ����
    public virtual void EnterInteract_Player()
    {
        isInteractedd = true;

        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(true);  // �⺻ ��ȣ�ۿ� �˾� Ȱ��ȭ (���� �Ѵٸ�)
    }
    public virtual void EnterInteract_WorkerAI()
    {
        isInteractedd = true;
    }



    // ��ȣ�ۿ� �������� ����
    public virtual void DeactiveInteract_Player()
    {
        isInteractedd = false;

        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false); // �⺻ ��ȣ�ۿ� �˾� ��Ȱ��ȭ (���� �Ѵٸ�)
    }
    public virtual void DeactiveInteract_WorkerAI()
    {
        isInteractedd = false;
    }

    public virtual void OnDisableAdditionalActions()
    {
        if (isInteractedd)
        {
            DeactiveInteract_Player();
            DeactiveInteract_WorkerAI();
        }
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
            // ���� ��ȣ�ۿ� �������� ��ü�� ������ ���� (�÷��̾�� �Ѹ��̶� �����������)
            pc ??= other.GetComponent<PlayerController>();

            EnterInteract_Player(); // �÷��̾� ���� ��ȣ�ۿ� ����
        }

        // �ϲ� ��ȣ�ۿ�
    }

    private void OnTriggerStay(Collider other)
    {
        // �÷��̾� ��ȣ�ۿ�(�켱����)
        if (other.GetComponent<PlayerController>() != null)
        {
            // �ٸ� ��ü�� ��ȣ�ۿ� ���� ���� ��, ���ο� ��ü�� ��ȣ�ۿ� ���� �ȿ� ���� ������� ��Ȳ�� ���� ����ó��
            if (!isInteractedd)  // �ٸ� ��ü�� ��ȣ�ۿ��� ���� ��,
            {
                if (pc == null)
                {
                    pc ??= other.GetComponent<PlayerController>();
                }
            }
        }

        // �ϲ� ��ȣ�ۿ�
    }

    private void OnTriggerExit(Collider other)
    {
        // �÷��̾� ��ȣ�ۿ�
        if (other.GetComponent<PlayerController>() != null)
        {
            pc = null;

            DeactiveInteract_Player(); // �˾� ��Ȱ��ȭ
        }

        // �ϲ� ��ȣ�ۿ�
    }
}

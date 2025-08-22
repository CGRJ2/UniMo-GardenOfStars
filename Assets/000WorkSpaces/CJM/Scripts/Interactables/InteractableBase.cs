using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected PlayerController pc;
    [Header("�⺻ ��ȣ�ۿ� �˾�(���ٸ� �������� ����)")]
    [SerializeField] Canvas interactPopUI;
    bool isActive;

    public void InitPopUI()
    {
        if (interactPopUI != null)
        {
            interactPopUI.worldCamera = Camera.main;
            interactPopUI.gameObject.SetActive(false);
        }
    }

    public virtual void ImediateInteract()
    {
        // ��� ���� ��ȣ�ۿ�
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(true);  // �⺻ ��ȣ�ۿ� �˾� Ȱ��ȭ (���� �Ѵٸ�)
    }

    public virtual void ActivePopUpInteract()
    {
        // [�г��� Ȱ��ȭ] Ȥ�� [��ư�� Ȱ��ȭ]
    }

    public virtual void DeactivePopUpInteract()
    {
        // [�г��� Ȱ��ȭ] Ȥ�� [��ư�� Ȱ��ȭ] �ߴ� �� ��Ȱ��ȭ
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false); // �⺻ ��ȣ�ۿ� �˾� ��Ȱ��ȭ (���� �Ѵٸ�)
    }

    // �÷��̾ ���߿� ������ų�, 
    public virtual void OnDisableActions()
    {
        if (isActive)
        {
            DeactivePopUpInteract();
            isActive = false;
        }
    }

    public void OnDisable()
    {
        OnDisableActions();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            pc = other.GetComponent<PlayerController>();

            // �÷��̾ ���°� Ȯ�� �ϸ� Ȱ��ȭ
            isActive = true;

            ImediateInteract(); // ��� ��ȣ�ۿ� ����

            // �˾� Ȱ��ȭ <= ���߿� ������ ��� ��ȣ�ۿ뿡�� Ȱ��ȭ�Ǵ� ��ư�� ������ ��, �� ��ư�� onClick�� �Ҵ�
            // �ش� ��ư�� �� ��ü�� Canvas�� �����ϰ� �� ĵ������ Ȱ��ȭ �ϴ� ������ ����?
            ActivePopUpInteract(); 
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            // �÷��̾ ���� ������
            if (isActive)
            {
                pc ??= other.GetComponent<PlayerController>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            // �÷��̾ ���� ������ �����ٸ� ��Ȱ��ȭ
            isActive = false;
            pc = null;

            DeactivePopUpInteract(); // �˾� ��Ȱ��ȭ
        }
    }
}

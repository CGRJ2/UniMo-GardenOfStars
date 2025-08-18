using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected PlayerController pc;
    bool isActive;


    public virtual void ImediateInteract()
    {
        // ��� ���� ��ȣ�ۿ� 
    }

    public virtual void ActivePopUpInteract()
    {
        // [�г��� Ȱ��ȭ] Ȥ�� [��ư�� Ȱ��ȭ]
    }

    public virtual void DeactivePopUpInteract()
    {
        // [�г��� Ȱ��ȭ] Ȥ�� [��ư�� Ȱ��ȭ] �ߴ� �� ��Ȱ��ȭ
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
            ActivePopUpInteract(); // �˾� Ȱ��ȭ
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

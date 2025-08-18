using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected PlayerController pc;
    bool isActive;


    public virtual void ImediateInteract()
    {
        // 즉시 실행 상호작용 
    }

    public virtual void ActivePopUpInteract()
    {
        // [패널을 활성화] 혹은 [버튼을 활성화]
    }

    public virtual void DeactivePopUpInteract()
    {
        // [패널을 활성화] 혹은 [버튼을 활성화] 했던 것 비활성화
    }

    // 플레이어가 도중에 사라지거나, 
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

            // 플레이어가 들어온걸 확인 하면 활성화
            isActive = true;

            ImediateInteract(); // 즉발 상호작용 실행
            ActivePopUpInteract(); // 팝업 활성화
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            // 플레이어가 들어와 있으면
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
            // 플레이어가 영역 밖으로 나갔다면 비활성화
            isActive = false;
            pc = null;

            DeactivePopUpInteract(); // 팝업 비활성화
        }
    }
}

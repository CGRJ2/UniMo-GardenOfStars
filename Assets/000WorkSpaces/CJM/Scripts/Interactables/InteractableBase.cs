using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected PlayerController pc;
    [Header("기본 상호작용 팝업(없다면 공란으로 유지)")]
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
        // 즉시 실행 상호작용
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화 (존재 한다면)
    }

    public virtual void ActivePopUpInteract()
    {
        // [패널을 활성화] 혹은 [버튼을 활성화]
    }

    public virtual void DeactivePopUpInteract()
    {
        // [패널을 활성화] 혹은 [버튼을 활성화] 했던 것 비활성화
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false); // 기본 상호작용 팝업 비활성화 (존재 한다면)
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

            // 팝업 활성화 <= 나중에 별도로 즉발 상호작용에서 활성화되는 버튼을 눌렀을 때, 그 버튼의 onClick에 할당
            // 해당 버튼은 이 객체가 Canvas를 보유하고 그 캔버스를 활성화 하는 식으로 진행?
            ActivePopUpInteract(); 
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

using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected PlayerController pc;  // 현재 상호작용 주체를 Player로만 해놨는데. 플레이어와 일꾼 모두 주체가 될 수 있도록 수정 필요

    [Header("기본 상호작용 팝업(없다면 공란으로 유지)")]
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

    // 상호작용 범위 진입
    public virtual void EnterInteract_Player()
    {
        isInteractedd = true;

        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화 (존재 한다면)
    }
    public virtual void EnterInteract_WorkerAI()
    {
        isInteractedd = true;
    }



    // 상호작용 범위에서 나감
    public virtual void DeactiveInteract_Player()
    {
        isInteractedd = false;

        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false); // 기본 상호작용 팝업 비활성화 (존재 한다면)
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
        // 플레이어 상호작용
        if (other.GetComponent<PlayerController>() != null)
        {
            // 먼저 상호작용 진행중인 객체가 있으면 무시 (플레이어는 한명이라 상관없을듯함)
            pc ??= other.GetComponent<PlayerController>();

            EnterInteract_Player(); // 플레이어 진입 상호작용 실행
        }

        // 일꾼 상호작용
    }

    private void OnTriggerStay(Collider other)
    {
        // 플레이어 상호작용(우선순위)
        if (other.GetComponent<PlayerController>() != null)
        {
            // 다른 객체와 상호작용 진행 중일 때, 새로운 객체가 상호작용 범위 안에 들어와 대기중인 상황에 대한 예외처리
            if (!isInteractedd)  // 다른 객체의 상호작용이 끝날 때,
            {
                if (pc == null)
                {
                    pc ??= other.GetComponent<PlayerController>();
                }
            }
        }

        // 일꾼 상호작용
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어 상호작용
        if (other.GetComponent<PlayerController>() != null)
        {
            pc = null;

            DeactiveInteract_Player(); // 팝업 비활성화
        }

        // 일꾼 상호작용
    }
}

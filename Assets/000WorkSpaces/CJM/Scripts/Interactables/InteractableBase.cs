using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected PlayerController characterRuntimeData;  // 현재 상호작용 주체를 Player로만 해놨는데. 플레이어와 일꾼 모두 주체가 될 수 있도록 수정 필요


    [Header("기본 상호작용 팝업(없다면 공란으로 유지)")]
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

    // 상호작용 범위 진입
    public virtual void EnterInteract(PlayerController characterRuntimeData)
    {
        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화 (존재 한다면)
    }


    public virtual void EnterInteractSingleOnly(PlayerController characterRuntimeData)
    {

    }


    // 상호작용 범위에서 나감
    public virtual void ExitInteract(PlayerController characterRuntimeData)
    {
        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false); // 기본 상호작용 팝업 비활성화 (존재 한다면)
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
        // 플레이어 상호작용
        if (other.GetComponent<PlayerController>() != null)
        {
            // 일반 상호작용
            EnterInteract(characterRuntimeData); // 플레이어 진입 상호작용 실행

            // 싱글 상호작용(먼저 상호작용 진행중인 객체가 있으면 무시)
            if (this.characterRuntimeData != null) return;
            EnterInteractSingleOnly(characterRuntimeData);
        }

        // 일꾼 상호작용
    }

    private void OnTriggerStay(Collider other)
    {
        // 플레이어 상호작용(우선순위)

        // 싱글 상호작용 자리가 비는 경우
        if (characterRuntimeData == null)
        {
            // 플레이어
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
                // 건물이라면 플레이어가 작업영역에서 정지했을때만 상호작용 실행
                //Player의 IsMoving이 false라면 작업 진행
            }
        }

        // 일꾼 상호작용
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어 상호작용
        if (other.GetComponent<PlayerController>() != null)
        {
            PlayerController pc = other.GetComponent<PlayerController>();

            if (characterRuntimeData == pc) // 싱글 상호작용 중이던 객체가 지금 나간 플레이어라면
                characterRuntimeData = null;
        }

        // 일꾼 상호작용
        ExitInteract(characterRuntimeData); // 팝업 비활성화
    }
}

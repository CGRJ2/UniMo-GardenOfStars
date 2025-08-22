using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected CharaterRuntimeData characterRD;
    public CharaterRuntimeData personalTaskOwner;


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
    public virtual void Enter(CharaterRuntimeData characterRuntimeData)
    {
        characterRD = characterRuntimeData;
        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(true);  // 기본 상호작용 팝업 활성화 (존재 한다면)
    }


    public virtual void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        personalTaskOwner = characterRuntimeData;
    }


    // 상호작용 범위에서 나감
    public virtual void Exit(CharaterRuntimeData characterRuntimeData)
    {
        characterRD = null;
        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (interactPopUI != null)
            interactPopUI.gameObject.SetActive(false); // 기본 상호작용 팝업 비활성화 (존재 한다면)
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
            // 일반 상호작용 활성화
            Enter(_CharacterRD);

            // 1인 상호작용(먼저 상호작용 진행중인 객체가 있으면 무시)
            if (personalTaskOwner == null)
                Enter_PersonalTask(_CharacterRD);
        }

    }

    private void OnTriggerStay(Collider other)
    {
        CharaterRuntimeData _CharacterRD = other.transform.root.GetComponent<CharaterRuntimeData>();


        // 플레이어 상호작용(우선순위)

        // 싱글 상호작용 자리가 비는 경우
        if (characterRD == null)
        {
            // 플레이어
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
                // 건물이라면 플레이어가 작업영역에서 정지했을때만 상호작용 실행
                //Player의 IsMoving이 false라면 작업 진행
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        CharaterRuntimeData _CharacterRD = other.transform.root.GetComponent<CharaterRuntimeData>();

        if (_CharacterRD != null)
        {
            // 일반 상호작용 비활성화
            Exit(_CharacterRD);

            // 1인 상호작용 비활성화(방금 나간 캐릭터가 상호작용중이던 캐릭터여야함)
            if (personalTaskOwner == _CharacterRD)
                Exit_PersonalTask(_CharacterRD);
        }
    }
}

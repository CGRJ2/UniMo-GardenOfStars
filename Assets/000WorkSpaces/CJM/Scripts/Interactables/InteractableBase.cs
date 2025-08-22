using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected CharaterRuntimeData characterRD;
    public CharaterRuntimeData personalTaskOwner;


    [Header("기본 상호작용 팝업(없다면 공란으로 유지)")]
    [SerializeField] Canvas interactPopUI;
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
        if (characterRD == characterRuntimeData)
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
        CharaterRuntimeData _CharacterRD = other.transform.parent.GetComponent<CharaterRuntimeData>();

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
        CharaterRuntimeData _CharacterRD = other.transform.parent.GetComponent<CharaterRuntimeData>();

        if (characterRD == null)
        {
            // 플레이어
            if (_CharacterRD != null)
            {
                characterRD ??= other.GetComponent<CharaterRuntimeData>();
                // 일반 상호작용 활성화
                Enter(_CharacterRD);

                // 1인 상호작용(먼저 상호작용 진행중인 객체가 있으면 무시)
                if (personalTaskOwner == null)
                    Enter_PersonalTask(_CharacterRD);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CharaterRuntimeData _CharacterRD = other.transform.parent.GetComponent<CharaterRuntimeData>();

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

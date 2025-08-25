using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected CharaterRuntimeData characterRD;
    public CharaterRuntimeData personalTaskOwner;

    // 상호작용 범위 진입
    public virtual void Enter(CharaterRuntimeData characterRuntimeData)
    {
        characterRD = characterRuntimeData;
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
    }

    public virtual void Exit_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        personalTaskOwner = null;
    }

    public virtual void OnDisableAdditionalActions() { }

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


public interface IWorkStation
{
    public bool GetWorkableState();
    public bool GetReserveState();
    public void SetReserveState(bool reserve);
}
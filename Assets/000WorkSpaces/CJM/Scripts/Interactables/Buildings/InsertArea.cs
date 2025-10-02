using System.Collections;
using UnityEngine;

public class InsertArea : InteractableBase, IWorkStation
{
    [HideInInspector] public ManufactureBuilding ownerInstance;

    public bool isWorkable
    { get { return ownerInstance.ingrediantStack.Count < ownerInstance.Capacity; } }
    public bool isReserved;
    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() { return isReserved; }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }
    // 데이터 구조 설계할 때 수정
    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        Manager.buildings.workStatinLists.insertAreas.Add(this);
    }

    IEnumerator AutoStacking()
    {
        var character = characterRD;

        while (isWorkable && character.IngrediantStack.Count > 0)
        {
            bool isStackable = false;

            // 건물에 스택 가능한 최대 수량만큼 쌓여있다면 스택 취소
            if (ownerInstance.Capacity > ownerInstance.ingrediantStack.Count)
                isStackable = true;
            else isStackable = false;

            // 스택 자리가 빌 때까지 대기
            if (!isStackable)
            {
                yield return null;
                continue;
            }

            // 플레이어 손에 재료가 있는지 체크
            IngrediantInstance poppedProd;
            if (character.IngrediantStack.TryPop(out poppedProd))
            {
                // 맨 위의 재료와 투입 가능 재료가 같은 종류일 때 넣어주기
                if (poppedProd.Data.ID == ownerInstance.originData.RequireProdID)
                {
                    poppedProd.AttachToTarget(ownerInstance.attachPoint, ownerInstance.ingrediantStack.Count);
                    ownerInstance.ingrediantStack.Push(poppedProd);
                }

                // 다음 투입까지 딜레이 시간 설정
                yield return new WaitForSeconds(ownerInstance.insertDelayTime);
            }
            // 플레이어 손에 재료가 없으면 바로 return
            else
            {
                yield return null;
            }
        }
    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);
        //Debug.Log($"건물재료삽입영역({buildingInstance.name}): 즉발형 상호작용 실행");
        
        IngrediantInstance peekedProd;
        if (characterRD.IngrediantStack.TryPeek(out peekedProd))
        {
            if (peekedProd is Item_Building) return;
            
            else StartCoroutine(AutoStacking());
        }
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);
    }


    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //Debug.Log($"건물재료삽입영역({buildingInstance.name}): 팝업형 상호작용 비활성화");
    }

    protected override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.insertAreas?.Remove(this);
    }
}

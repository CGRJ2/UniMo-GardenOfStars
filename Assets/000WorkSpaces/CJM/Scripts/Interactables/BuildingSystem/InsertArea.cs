using System.Collections;
using UnityEngine;

public class InsertArea : InteractableBase
{
    [HideInInspector] public ManufactureBuilding instance;

    public bool isWorkable
    { get { return instance.ingrediantStack.Count < instance.runtimeData.maxStackableCount; }}

    // 데이터 구조 설계할 때 수정
    public void Init(ManufactureBuilding instance)
    {
        this.instance = instance;
    }

    IEnumerator AutoStacking()
    {
        while (characterRD != null)
        {
            bool isStackable = false;


            // 건물에 스택 가능한 최대 수량만큼 쌓여있다면 스택 취소
            if (instance.runtimeData.maxStackableCount > instance.ingrediantStack.Count)
                isStackable = true;
            else isStackable = false;

            // 스택 자리가 빌 때까지 대기
            yield return new WaitUntil(() => isStackable);

            // 스택이 비었지만 플레이어가 나가면 쌓지 않고 break;
            if (characterRD == null) yield break;

            // 플레이어 손에 재료가 있는지 체크
            IngrediantInstance instanceProd;
            if (characterRD.IngrediantStack.TryPeek(out instanceProd))
            {
                // 맨 위의 재료와 투입 가능 재료가 같은 종류일 때 넣어주기
                if (instanceProd.Data.ID == instance.originData.RequireProdID)
                {
                    IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                    popedProd.AttachToTarget(instance.attachPoint, instance.ingrediantStack.Count);
                    instance.ingrediantStack.Push(instanceProd);
                }

                // 다음 투입까지 딜레이 시간 설정
                yield return new WaitForSeconds(instance.insertDelayTime);
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

        StartCoroutine(AutoStacking());
    }


    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);
        //Debug.Log($"건물재료삽입영역({buildingInstance.name}): 팝업형 상호작용 비활성화");
    }
}

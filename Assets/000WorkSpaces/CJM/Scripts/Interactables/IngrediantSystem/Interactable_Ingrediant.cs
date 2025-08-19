using System.Collections;
using UnityEngine;

// 풀 오브젝트를 인스턴스에서 상속받기 때문에 상호작용 역할을 별도의 MonoBehavior 클래스로 적용함
public class Interactable_Ingrediant : InteractableBase
{
    IngrediantInstance ingrediantInstance;
    [SerializeField] float absorbAcceleration = 3f;
    [SerializeField] Vector3 stackOffset;

    bool isInteracted = false;
    bool isAttached = false;

    

    private void Awake()
    {
        ingrediantInstance = GetComponent<IngrediantInstance>();
    }

    IEnumerator AttachToTarget(Transform targetAttachTransform, int stackOrder)
    {
        if (targetAttachTransform == null) yield break;

        float currentSpeed = 0f;
        Quaternion startRot = transform.rotation;

        Vector3 firstTargetPos = targetAttachTransform.position + stackOffset * stackOrder;
        float startDist = Vector3.Distance(transform.position, firstTargetPos);
        if (startDist < 0.0001f) startDist = 0.0001f;

        while (!isAttached)
        {
            if (targetAttachTransform == null) yield break;

            Vector3 targetPos = targetAttachTransform.position + stackOffset * stackOrder;
            Quaternion targetRot = targetAttachTransform.rotation;

            // 이동
            currentSpeed += absorbAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            // 회전 (가까워질수록 t → 1, 0.3 거리에서 이미 회전 완료)
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist > startDist) startDist = dist; // 타겟이 멀어지면 기준 갱신
            float t = Mathf.InverseLerp(startDist, 0.3f, dist); 
            t = Mathf.Clamp01(t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            // 도착 스냅
            if (dist < 0.01f)
            {
                transform.position = targetPos;
                transform.rotation = targetRot; // ← 마지막에 정확히 맞춰주기
                isAttached = true;
                break;
            }

            yield return null;
        }
    }

    public override void ImediateInteract()
    {
        base.ImediateInteract();

        // 1회 상호작용 후 막기
        if (isInteracted) return;
        isInteracted = true;

        Debug.Log($"재료 인스턴스({ingrediantInstance?.ingrediantSO?.Name}): 즉발형 상호작용 실행");

        // 플레이어 위치로 설정
        // 임시----------------------------------
        transform.SetParent(pc.prodsAttachPoint); 
        ingrediantInstance.owner = pc.gameObject;
        StartCoroutine(AttachToTarget(pc.prodsAttachPoint, pc.ingrediantStack.Count));
        pc.ingrediantStack.Push(ingrediantInstance.ingrediantSO);
        //---------------------------------------
    }

    public override void ActivePopUpInteract()
    {
        base.ActivePopUpInteract();
        
        // 1회 상호작용 후 막기
        if (isInteracted) return;
        isInteracted = true;

        //Debug.Log($"재료 인스턴스({ingrediantInstance.ingrediantSO.Name}): 팝업형 상호작용 활성화");
    }

    public override void DeactivePopUpInteract()
    {
        base.DeactivePopUpInteract();

        // 1회 상호작용 후 막기
        if (isInteracted) return;
        isInteracted = true;

        //Debug.Log($"재료 인스턴스({ingrediantInstance.ingrediantSO.Name}): 팝업형 상호작용 비활성화");
    }
}

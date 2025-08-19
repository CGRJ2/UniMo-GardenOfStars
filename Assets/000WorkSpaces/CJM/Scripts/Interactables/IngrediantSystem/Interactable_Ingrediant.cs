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
        if (targetAttachTransform == null) yield break; // 타겟이 사라졌을 때에 대한 안전장치

        float currentSpeed = 0;
        /*Quaternion startRot = transform.rotation;
        float startDist = (transform.position - (targetAttachTransform.position + stackOffset * stackOrder)).magnitude;
        if (startDist < 0.0001f) startDist = 0.0001f; // 0 방지*/

        while (!isAttached)
        {
            if (targetAttachTransform == null) yield break; // 타겟이 사라졌을 때에 대한 안전장치

            Vector3 targetPos = targetAttachTransform.position + stackOffset * stackOrder;
            Quaternion targetRot = targetAttachTransform.rotation;

            // 서서히 이동
            currentSpeed += absorbAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

            // 회전
            /*float dist = Vector3.Distance(transform.position, targetPos);
            if (dist > startDist) startDist = dist; // 타겟이 멀어지면 기준 갱신(타겟 이동 대응)
            float t = 1f - Mathf.InverseLerp(startDist, 0.3f, dist); // 0→1
            t = Mathf.Clamp01(t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);*/

            Debug.Log("타겟 위치로 이동 중");
            if ((targetPos - transform.position).magnitude < 0.1f)
            {
                transform.position = targetPos;
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

        Debug.Log($"재료 인스턴스({ingrediantInstance.ingrediantSO.Name}): 즉발형 상호작용 실행");

        // 플레이어 위치로 설정
        // 임시----------------------------------
        transform.SetParent(pc.transform); 
        ingrediantInstance.owner = pc.gameObject;
        StartCoroutine(AttachToTarget(pc.prodsAttachPoint, pc.TempStackorder));
        pc.TempStackorder += 1;
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

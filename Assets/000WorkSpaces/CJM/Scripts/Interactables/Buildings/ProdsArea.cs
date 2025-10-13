using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ProdsArea : InteractableBase, IWorkStation
{
    [Header("Floating 효과 설정")]
    [SerializeField] private float floatHeight = 0.5f;   // 위아래 이동 거리
    [SerializeField] private float floatDuration = 2f;   // 위아래 이동 시간
    [SerializeField] private float rotationSpeed = 50f;  // 초당 회전 속도(도 단위)
    GameObject prodObjectView;
    private Tween floatTween;
    private Tween rotateTween;
    ////////////////////////////////////////////////////////////////////////

    public bool isWorkable { get { return ProdsCount.Value > 0; } }
    bool isReserved;
    public bool GetWorkableState() { return isWorkable; }
    public bool GetReserveState() 
    {
        if (!isReserved) return ownerInstance.originData.ProductID == StageManager.Instance.finalProdID; 
        return isReserved; 
    }
    public void SetReserveState(bool reserve) { isReserved = reserve; }
    public Vector3 GetPosition() { return transform.position; }

    [HideInInspector] public ManufactureBuilding ownerInstance;
    public ObjectPool pool;

    public ObservableProperty<int> ProdsCount = new();

    [SerializeField] Canvas canvas_ProdsResult;
    [SerializeField] TMP_Text tmp_Count;
    
    public void Init(ManufactureBuilding instance)
    {
        this.ownerInstance = instance;
        string key = instance.originData.ProductID; // 임시로 Id를 이름으로 설정

        // 생산될 재료 인스턴스 풀 지정 or 생성
        Addressables.LoadAssetAsync<GameObject>(key).Completed += task =>
        {
            GameObject product = task.Result;

            pool = Manager.pool.GetPoolBundle(product).instancePool;
        };
        
        Manager.buildings.workStatinLists.prodsAreas.Add(this);
        UpdateView(ProdsCount.Value);
        ProdsCount.Subscribe(UpdateView);
    }


    public void PickUp()
    {
        // 들고 있는 재료와 다른 재료라면 or 손에 최대 수량만큼 들고 있을 시 => 줍지 않게 만들기
        IngrediantInstance instanceProd;
        if (characterRD.IngrediantStack.TryPeek(out instanceProd))
        {
            if (instanceProd.ID != ownerInstance.originData.ProductID) return;
            if (characterRD.IngrediantStack.Count >= characterRD.GetMaxCapacity()) return;
        }

        // 오브젝트 풀에서 활성화
        GameObject disposedObject = pool.DisposePooledObj(transform.position, transform.rotation);
        IngrediantInstance _SpawnedProduct = disposedObject.GetComponent<IngrediantInstance>();

        _SpawnedProduct.AttachToTarget(characterRD.ProdsAttachPoint, characterRD.IngrediantStack.Count, characterRD);
        characterRD.IngrediantStack.Push(_SpawnedProduct);
        ProdsCount.Value -= 1;
    }

    IEnumerator PickUpRoutine()
    {
        while (characterRD != null)
        {
            yield return new WaitUntil(() => ProdsCount.Value > 0 || characterRD == null);

            if (characterRD == null) break;

            PickUp();
            //_SpawnedProduct = null;
            //tmp_Count.text = $"{ProdsCount}";
            yield return new WaitForSeconds(ownerInstance.insertDelayTime);
        }
    }

    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);

        StartCoroutine(PickUpRoutine());
    }

    protected override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();
        Manager.buildings?.workStatinLists.prodsAreas?.Remove(this);
        ProdsCount.Unsubscribe(UpdateView);
    }

    void UpdateView(int value)
    {
        // 생산된 재료가 있다면
        if (value > 0)
        {
            // 오브젝트 관련
            if (prodObjectView == null)
            {
                prodObjectView = pool.DisposePooledObj(transform.position, transform.rotation);

                // 이미 실행 중이라면 무시
                if (floatTween != null && floatTween.IsActive()) return;

                // 회전 시작
                // 위아래 떠다니는 효과 (Y축 이동 반복)
                floatTween = prodObjectView.transform.DOMoveY(transform.position.y + floatHeight, floatDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo); // 무한 반복, 요요(왕복)

                // 회전 효과 (Y축 기준 회전)
                rotateTween = prodObjectView.transform.DORotate(new Vector3(0, 360f, 0), rotationSpeed, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart); // 무한 반복
            }

            // 텍스트 & 발판 표기 관련
            canvas_ProdsResult.gameObject.SetActive(true);
            tmp_Count.text = $"x{value}";

            // SFX 실행
            Manager.Audio.SfxPlay("SFX_ProdSpawn", transform);
        }
        else
        {
            if (prodObjectView != null)
            {
                prodObjectView.GetComponent<IngrediantInstance>().Despawn();

                prodObjectView = null;

                // 인스턴스 움직임 효과 정지
                floatTween?.Kill();
                rotateTween?.Kill();
                floatTween = null;
                rotateTween = null;
            }

            // 텍스트 & 발판 표기 관련
            canvas_ProdsResult.gameObject.SetActive(false);
            tmp_Count.text = $"x{0}";
        }
    }
}

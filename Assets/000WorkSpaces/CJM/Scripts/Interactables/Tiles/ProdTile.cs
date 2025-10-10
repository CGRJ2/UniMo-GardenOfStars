using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TextCore.Text;

public class ProdTile : InteractableBase
{
    ObjectPool _Pool;
    [Header("Floating 효과 설정")]
    [SerializeField] private float floatHeight = 0.5f;   // 위아래 이동 거리
    [SerializeField] private float floatDuration = 2f;   // 위아래 이동 시간
    [SerializeField] private float rotationSpeed = 50f;  // 초당 회전 속도(도 단위)
    private Tween floatTween;
    private Tween rotateTween;
    Item_Building buildingItem;

    // 구매 완료 시, DB에 구매한 BuildingId 저장
    // 설치 완료 시, DB에 Id 삭제

    // 게임 종료 후 다시 실행 시 DB에 구매한 BulidingId가 있으면 생성
    
    // 설치 효과 FX
    ObjectPool _Pool_FX_Highlighted;
    GameObject _FX_Highlight;

    private void Awake()
    {
        // 타이틀에서 스테이지 씬으로 전환될 때 실행
        //UpdateInstanceView();

        // 테스트용으로 바로 스테이지 씬에서 시작할 때 실행
        StartCoroutine(WaitAndLoad());

        // FX 불러온 후, 풀로 반환 (없으면 풀 생성)
        Addressables.LoadAssetAsync<GameObject>("FX/Highlighted.prefab").Completed += task =>
        {
            _Pool_FX_Highlighted = Manager.pool.GetPoolBundle(task.Result, 1).instancePool;
        };
    }

    IEnumerator WaitAndLoad()
    {
        yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
        yield return new WaitUntil(() => Manager.firebase.UserData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.PurchasedBuildingID.IsInit);

        //yield return new WaitForSeconds(1f);

        UpdateInstanceView();
    }

    public void UpdateInstanceView()
    {
        string buildingId = Manager.firebase.UserData.CurStageData.PurchasedBuildingID.Value;
        if (string.IsNullOrEmpty(buildingId)) return;

        Addressables.LoadAssetAsync<GameObject>($"it_{buildingId}").Completed += task =>
        {
            GameObject product = task.Result;
            _Pool = Manager.pool.GetPoolBundle(product, 3).instancePool;   // 해당 건물(재료) 인스턴스 풀 반환(없으면 생성)

            // 오브젝트 풀에서 활성화
            GameObject disposedObject = _Pool.DisposePooledObj(transform.position, transform.rotation);
            buildingItem = disposedObject.GetComponent<Item_Building>();
            buildingItem.buildingId = buildingId;

            // 이미 실행 중이라면 무시
            if (floatTween != null && floatTween.IsActive()) return;

            // 회전 시작
            // 위아래 떠다니는 효과 (Y축 이동 반복)
            floatTween = disposedObject.transform.DOMoveY(transform.position.y + floatHeight, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo); // 무한 반복, 요요(왕복)

            // 회전 효과 (Y축 기준 회전)
            rotateTween = disposedObject.transform.DORotate(new Vector3(0, 360f, 0), rotationSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart); // 무한 반복

            // 빛 효과 추가
            PlayHighLightFX(disposedObject.transform);
        };
    }
    public void PlayHighLightFX(Transform target, bool isMultipleAdd = false)
    {
        // 타겟이 없으면 효과 제거
        if (target == null)
        {
            _Pool_FX_Highlighted.ReturnPooledObj(_FX_Highlight);
        }
        else
        {
            // 효과 활성화
            _FX_Highlight = (_Pool_FX_Highlighted.DisposePooledObj(target.position + Vector3.up * 1.2f, target.rotation));
        }
    }

    public void PickUp()
    {
        // 손에 다른 뭔가가 있다면 줍지 않게 만들기
        if (characterRD.IngrediantStack.Count > 0) return;

        if (buildingItem == null) return;

        // 인스턴스 움직임 효과 정지
        floatTween?.Kill();
        rotateTween?.Kill();
        floatTween = null;
        rotateTween = null;

        // 빛 효과 제거
        PlayHighLightFX(null);

        // 플레이어 보유 스택에 올려주기
        buildingItem.AttachToTarget(characterRD.ProdsAttachPoint);
        Manager.Audio.ChainedSFXPlay("Get");

        characterRD.IngrediantStack.Push(buildingItem);

        // 건축모드 활성화
        Manager.buildings.BuildModEvent?.Invoke(true, buildingItem.buildingId);
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);
        
        PickUp();
    }

    protected override void OnDisableAdditionalActions()
    {
        base.OnDisableAdditionalActions();

        StopAllCoroutines();
    }
}

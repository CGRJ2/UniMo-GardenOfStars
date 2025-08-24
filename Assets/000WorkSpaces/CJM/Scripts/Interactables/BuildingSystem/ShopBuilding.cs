using System.Collections;
using TMPro;
using UnityEngine;

public class ShopBuilding : BuildingInstance
{
    [SerializeField] float insertDelayTime = 0.1f;
    [SerializeField] Transform attachPoint;

    [Header("정산 UI 설정")]
    [SerializeField] Canvas Canvas_PriceData;
    [SerializeField] CanvasGroup panel_Price;
    [SerializeField] TMP_Text tmp_soldPrice;
    [SerializeField] float popDuration;
    [SerializeField] float fadeOutTime;

    Coroutine popPricePanelRoutine;

    void Awake() => Init();
    void Init()
    {
        panel_Price?.gameObject?.SetActive(false);
        if (Canvas_PriceData != null)
            Canvas_PriceData.worldCamera = Camera.main;
    }

    IEnumerator AutoSelling()
    {
        // 플레이어 손에 있는 재료가 퀘스트 조건에 포함되는지 체크
        IngrediantInstance instanceProd;
        if (characterRD.IngrediantStack.TryPeek(out instanceProd))
        {
            int soldItemCount = 0;
            long price = instanceProd.Data.Price;
            while(characterRD.IngrediantStack.Count > 0)
            {
                IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                popedProd.MoveToTargetAndShrink(attachPoint);
                soldItemCount += 1;

                yield return new WaitForSeconds(insertDelayTime);
            }

            // 전부 투입 완료 된 후 정산 & UI활성화
            // 여기서 정산하면 됨 => 플레이어 데이터에 soldItemCount * price 만큼 획득
            tmp_soldPrice.text = $" {price}($) x {soldItemCount} = {soldItemCount * price}$";


            // 가격 정산 UI 페이드아웃 팝핑
            if (popPricePanelRoutine == null)
            {
                popPricePanelRoutine = StartCoroutine(SoldPanelFadeOutRoutine());
            }
            else
            {
                StopCoroutine(popPricePanelRoutine);
                popPricePanelRoutine = StartCoroutine(SoldPanelFadeOutRoutine());
            }
        }
        // 플레이어 손에 재료가 없으면 바로 return
        else
        {
            yield return null;
        }
    }

    IEnumerator SoldPanelFadeOutRoutine()
    {
        float curTime = 0;
        panel_Price.alpha = 1;
        panel_Price?.gameObject?.SetActive(true);
        yield return new WaitForSeconds(popDuration);

        while (curTime < fadeOutTime)
        {
            curTime += Time.deltaTime;
            float alpha = 1f - curTime / fadeOutTime;
            panel_Price.alpha = alpha;
            yield return null;
        }
        panel_Price?.gameObject?.SetActive(false);
        yield return null;
    }

    // 건물 활성화 범위 상호작용
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            StartCoroutine(AutoSelling());
        }
    }

    // 건물 활성화 범위 상호작용
    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);

        // 상호작용한 주체가 플레이어라면 (플레이어 한정)
        if (characterRuntimeData is PlayerRunTimeData)
        {

        }
    }
}

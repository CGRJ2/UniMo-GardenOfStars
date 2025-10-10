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
    new void Init()
    {
        panel_Price?.gameObject?.SetActive(false);
        if (Canvas_PriceData != null)
            Canvas_PriceData.worldCamera = Camera.main;
    }

    IEnumerator AutoSelling()
    {
        // 플레이어 손에 있는 재료가 퀘스트 조건에 포함되는지 체크
        IngrediantInstance instanceProd;
        CharaterRuntimeData characterRD_cach;
        characterRD_cach = characterRD;
        if (characterRD_cach.IngrediantStack.TryPeek(out instanceProd))
        {
            // 건물(재료)라면 => 건물 구매 가격에 다시 판매
            if (instanceProd is Item_Building building)
            {
                // 스테이지 배수 연산
                string curStageID = Manager.firebase.UserData.CurStage.Value;
                long price = Manager.data.Building[building.buildingId].Cost * Manager.data.Stage.Values[curStageID].StageInflationRate;

                IngrediantInstance popedProd = characterRD_cach.IngrediantStack.Pop();
                popedProd.MoveToTargetAndShrink(attachPoint, () =>
                {
                    // 판매 완료
                    CaculateSoldResult(price, 1, true);

                    // 구매한 건물 ID => DB에서 초기화
                    Manager.firebase.UserData.CurStageData.PurchasedBuildingID.Value = "";

                    // 건축모드 비활성화
                    Manager.buildings.BuildModEvent?.Invoke(false, null);
                });
            }
            // 일반 재료라면 계산식을 통해 판매 ///// 흥정 수치 계산식에 포함해야됨. 어떤 식으로 할건가요?
            else
            {
                int soldItemCount = 0;
                long price = instanceProd.Data.Price;

                while (characterRD_cach.IngrediantStack.Count > 0)
                {
                    IngrediantInstance popedProd = characterRD_cach.IngrediantStack.Pop();

                    if (characterRD_cach.IngrediantStack.Count > 0)
                    {
                        popedProd.MoveToTargetAndShrink(attachPoint);
                    }
                    else // 마지막 재료일 때
                    {
                        popedProd.MoveToTargetAndShrink(attachPoint, () => CaculateSoldResult(price, soldItemCount));
                    }
                    soldItemCount += 1;

                    yield return new WaitForSeconds(insertDelayTime);
                }
            }
        }

        // 플레이어 손에 재료가 없으면 바로 return
        else
        {
            yield return null;
        }
    }

    void CaculateSoldResult(long price, int soldItemCount = 1, bool isBuilding = false)
    {
        if (isBuilding)
        {
            // 건물 판매는 흥정레벨 반영 안됨
            tmp_soldPrice.text = $" {price}($) x {soldItemCount} = {soldItemCount * price}$";
            Manager.player.Data.Money.Value += (int)(soldItemCount * price);
        }
        else
        {
            float negoValue = Manager.firebase.UserData.Player.Nego;

            // 전부 투입 완료 된 후 정산 & UI활성화
            tmp_soldPrice.text = $" {price}($) x {soldItemCount} = {soldItemCount * price}$ + {soldItemCount * price * negoValue / 100f }$(흥정레벨 보너스 {negoValue}%)";
            Manager.player.Data.Money.Value += (int)(soldItemCount * price * (100f + negoValue) / 100f); //long으로 해야하는지? 일단 기획에서 요구한 건 long임
        }

        // 정산 SFX 실행
        Manager.Audio.SfxPlay("SFX_Money", transform);

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

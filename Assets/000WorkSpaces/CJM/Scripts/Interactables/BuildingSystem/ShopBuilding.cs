using System.Collections;
using TMPro;
using UnityEngine;

public class ShopBuilding : BuildingInstance
{
    [SerializeField] float insertDelayTime = 0.1f;
    [SerializeField] Transform attachPoint;

    [Header("���� UI ����")]
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
        // �÷��̾� �տ� �ִ� ��ᰡ ����Ʈ ���ǿ� ���ԵǴ��� üũ
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

            // ���� ���� �Ϸ� �� �� ���� & UIȰ��ȭ
            // ���⼭ �����ϸ� �� => �÷��̾� �����Ϳ� soldItemCount * price ��ŭ ȹ��
            tmp_soldPrice.text = $" {price}($) x {soldItemCount} = {soldItemCount * price}$";


            // ���� ���� UI ���̵�ƿ� ����
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
        // �÷��̾� �տ� ��ᰡ ������ �ٷ� return
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

    // �ǹ� Ȱ��ȭ ���� ��ȣ�ۿ�
    public override void Enter(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter(characterRuntimeData);

        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (characterRuntimeData is PlayerRunTimeData)
        {
            StartCoroutine(AutoSelling());
        }
    }

    // �ǹ� Ȱ��ȭ ���� ��ȣ�ۿ�
    public override void Exit(CharaterRuntimeData characterRuntimeData)
    {
        base.Exit(characterRuntimeData);

        // ��ȣ�ۿ��� ��ü�� �÷��̾��� (�÷��̾� ����)
        if (characterRuntimeData is PlayerRunTimeData)
        {

        }
    }
}

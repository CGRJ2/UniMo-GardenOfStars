using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KYS;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
public class OfflineRewardPopup : BaseUI
{
    private TextMeshProUGUI tmp_RewardDescriptionText => GetUI<TextMeshProUGUI>("RewardDescriptionText");
    private TextMeshProUGUI tmp_RunMoneyText1 => GetUI<TextMeshProUGUI>("RunMoneyText1");
    private TextMeshProUGUI tmp_RunMoneyText2 => GetUI<TextMeshProUGUI>("RunMoneyText2");

    private Button btn_Ad => GetUI<Button>("ConfirmButton");
    private Button btn_Esc => GetUI<Button>("CloseButton");

    protected override void Awake()
    {
        base.Awake();

        btn_Ad.onClick.AddListener(() =>
        {
            Manager.ad.ShowRewardedAd(() =>
            {
                Manager.firebase.UserData.Player.Money.Value += 4 * StageManager.Instance.GetTotalAutoReward();
                Manager.ui.ClosePopup();
            });
        });

        btn_Esc.onClick.AddListener(() => Manager.ui.ClosePopup());
    }

    private void OnEnable()
    {
        int reward = StageManager.Instance.GetTotalAutoReward();

        if (reward >= 1_000_000)
            tmp_RunMoneyText1.text = $"{(reward / 1_000_000f).ToString("0.#")}M";
        else if (reward >= 1_000)
            tmp_RunMoneyText1.text = $"{(reward / 1_000f).ToString("0.#")}K";
        else
            tmp_RunMoneyText1.text = reward.ToString();

        int rewardAd = reward * 5;
        if (rewardAd >= 1_000_000)
            tmp_RunMoneyText2.text = $"{(rewardAd / 1_000_000f).ToString("0.#")}M";
        else if (rewardAd >= 1_000)
            tmp_RunMoneyText2.text = $"{(rewardAd / 1_000f).ToString("0.#")}K";
        else
            tmp_RunMoneyText2.text = rewardAd.ToString();


        double diffSeconds = StageManager.Instance.GetStageAutoEarnTime();
        float hours = (float)diffSeconds / 3600f;              // 초 → 시간 변환

        // 시간
        tmp_RewardDescriptionText.text = $"누적시간: 약 {hours.ToString("0.0")}h, 누적보상:{reward}";
    }
}

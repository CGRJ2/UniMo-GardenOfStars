using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class AdPanel : KYS.BaseUI
{
    [SerializeField] private string _adPanelId;

    private Button _adButton => GetUI<Button>("AdButton");

    private TextMeshProUGUI _countText => GetUI<TextMeshProUGUI>("CountText");
    private TextMeshProUGUI _buttonText => GetUI<TextMeshProUGUI>("ButtonText");

    private Image _completeImage => GetUI<Image>("CompletedBG");
    private Image _bangMarkImage => GetUI<Image>("BangMark");

    private DailyAdData Data => Manager.firebase.UserData.DailyAdList.Get(_adPanelId);

    protected override void Awake()
    {
        base.Awake();

        _adButton.onClick.AddListener(OnClick);

        Manager.firebase.UserData.DailyAdList.OnAdded.AddListener(InitInfo);

        DailyAdData data = Manager.firebase.UserData.DailyAdList.Get(_adPanelId);

        if (data == null)
        {
            Manager.firebase.UserData.DailyAdList.Add(_adPanelId);
            return;
        }

        DateTime lastClaimUtc = DateTimeOffset.FromUnixTimeMilliseconds(data.LastTime.Value).UtcDateTime;

        DateTime lastClaimKst = lastClaimUtc.AddHours(9);
        DateTime nowKst = DateTime.UtcNow.AddHours(9);

        if (lastClaimKst.Day < nowKst.Day)
        {
            data.Count.Value = 0;
            InitInfo();
        }
        else
        {
            InitInfo(Manager.firebase.UserData.DailyAdList.Get(_adPanelId));
        }

        var update = new Dictionary<string, object>();
        update["LastTime"] = Firebase.Database.ServerValue.Timestamp;

        Manager.firebase.Database.RootReference.Child(Manager.firebase.UserData.DailyAdList.Get(_adPanelId).Path).UpdateChildrenAsync(update);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Manager.firebase.UserData.DailyAdList.OnAdded.RemoveListener(InitInfo);
    }

    private void InitInfo(DailyAdData data)
    {
        if (data.Id != _adPanelId) return;

        _countText.text = $"{data.Count.Value}/2";

        if(data.Count.Value < 2)
        {
            _buttonText.text = "보상 받기";
        }
        else
        {
            _bangMarkImage.gameObject.SetActive(false);
            _completeImage.gameObject.SetActive(true);
        }
    }

    private void InitInfo()
    {
        _bangMarkImage.gameObject.SetActive(true);
        _completeImage.gameObject.SetActive(false);
        _countText.text = $"0/2";
        _buttonText.text = "보상 받기";
    }

    private void OnClick()
    {
        if (Data.Count.IsInUpdate || Data.Count.Value >= 2) return;

        if (Manager.firebase.UserData.AdRemoved.Value)
        {
            GetReward();
            _countText.text = $"{Data.Count.Value + 1}/2";
            if (Data.Count.Value + 1 >= 2)
            {
                _bangMarkImage.gameObject.SetActive(false);
                _completeImage.gameObject.SetActive(true);
            }
            Manager.firebase.UserData.DailyAdList.Get(_adPanelId).Count.Value++;
            return;
        }

        Manager.ad.ShowRewardedAd(() =>
        {
            GetReward();
            _countText.text = $"{Data.Count.Value + 1}/2";
            if(Data.Count.Value + 1 >= 2)
            {
                _bangMarkImage.gameObject.SetActive(false);
                _completeImage.gameObject.SetActive(true);
            }
            Manager.firebase.UserData.DailyAdList.Get(_adPanelId).Count.Value++;
        });
    }

    protected abstract void GetReward();
}

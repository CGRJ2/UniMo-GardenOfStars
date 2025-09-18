using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleCanvas : KYS.BaseUI
{
    protected override void Awake()
    {
        base.Awake();
        Manager.Audio.BgmPlay("TitleBgm");
    }

    private void OnEnable()
    {
        GetEvent("Panel").Click += OnClick;
        Manager.firebase.UserData.StageList.OnAdded.AddListener(GoTutorialScene);
    }

    private void OnDisable()
    {
        GetEvent("Panel").Click -= OnClick;
        Manager.firebase.UserData.StageList.OnAdded.RemoveListener(GoTutorialScene);
    }

    private void OnClick(PointerEventData data)
    {
        if (Manager.firebase.UserData.CurStage.Value == "Tutorial" && Manager.firebase.UserData.StageList.Get("Tutorial") == null)
        {
            Manager.firebase.UserData.StageList.Add("Tutorial");
            return;
        }

        Addressables.LoadSceneAsync("StageScene");
    }

    private void GoTutorialScene(StageData data)
    {
        Addressables.LoadSceneAsync("StageScene");
    }
}

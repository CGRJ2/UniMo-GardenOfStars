using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleCanvas : KYS.BaseUI
{
    private TextMeshProUGUI tapScreen => GetUI<TextMeshProUGUI>("TapScreenText");

    protected override void Awake()
    {
        base.Awake();
        // Manager.Audio.BgmPlay("TitleBgm");
    }

    private void OnEnable()
    {
        tapScreen.text = GetLocalizedText("ui_titlescene_touch_screen");
        GetEvent("TitlePanel").Click += OnClick;
        Manager.firebase.UserData.StageList.OnAdded.AddListener(GoTutorialScene);
    }

    private void OnDisable()
    {
        GetEvent("TitlePanel").Click -= OnClick;
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
        Manager.ui.ShowUltraSimpleLoadingScreen(2);
    }

    private void GoTutorialScene(StageData data)
    {
        Addressables.LoadSceneAsync("StageScene");
        Manager.ui.ShowUltraSimpleLoadingScreen(2);
    }
}

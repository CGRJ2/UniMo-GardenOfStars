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

        StartCoroutine(WaitUserDataInit());
    }

    private void OnDisable()
    {
        GetEvent("TitlePanel").Click -= OnClick;
        Manager.firebase.UserData.StageList.OnAdded.RemoveListener(GoTutorialScene);
    }

    private IEnumerator WaitUserDataInit()
    {
        yield return new WaitUntil(() => Manager.firebase.UserData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);

        Manager.firebase.UserData.StageList.OnAdded.AddListener(GoTutorialScene);
    }

    private void OnClick(PointerEventData data)
    {
        if (Manager.firebase.UserData.CurStage.Value == "Tutorial" && Manager.firebase.UserData.StageList.Get("Tutorial") == null)
        {
            Manager.firebase.UserData.StageList.Add("Tutorial");
            return;
        }

        Manager.ui.ShowUltraSimpleLoadingScreen(2);
        Addressables.LoadSceneAsync("StageScene");
    }

    private void GoTutorialScene(StageData data)
    {
        Manager.ui.ShowUltraSimpleLoadingScreen(2);
        Addressables.LoadSceneAsync("StageScene");
    }
}

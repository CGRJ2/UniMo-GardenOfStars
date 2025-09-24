using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas_Title : KYS.BaseUI
{
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private Slider _slider;

    protected override void Awake()
    {
        base.Awake();
        _loadingText = GetUI<TextMeshProUGUI>("LoadingText");
        _slider = GetUI<Slider>("DownloadSlider");
    }

    public IEnumerator StartDownloadCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(1f);

        yield return new WaitUntil(() => Manager.game.initialized || Manager.game.inDownloading);
        yield return new WaitUntil(() => Manager.localization != null);
        _loadingText.text = GetLocalizedText("ui_titlescene_download_check");

        if (Manager.game.initialized)
        {
            yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
            yield return new WaitUntil(() => Manager.firebase.UserData != null);
            yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
            yield break;
        }

        _loadingText.text = GetLocalizedText("ui_titlescene_downloading");
        while (!Manager.game.initialized)
        {
            _slider.value = Manager.game.downloadProgress.Value;
            yield return null;
        }
        _slider.value = 1f;

        yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
        yield return new WaitUntil(() => Manager.firebase.UserData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);

        yield return delay;
    }
}

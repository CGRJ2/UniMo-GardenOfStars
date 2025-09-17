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

        _loadingText.text = "다운로드 받을 파일이 있는지 확인중...";
        yield return new WaitUntil(() => Manager.game.initialized || Manager.game.inDownloading);

        if (Manager.game.initialized)
        {
            yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
            yield return new WaitUntil(() => Manager.firebase.UserData != null);
            yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
            yield break;
        }

        _loadingText.text = "다운로드 중..";
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

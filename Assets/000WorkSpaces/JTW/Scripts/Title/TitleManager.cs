using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private AutoLoginController _autoLogin;

    [SerializeField] private LoadingCanvas_Title _loadingCanvas;

    [SerializeField] private LoginCanvas _loginCanvas;

    [SerializeField] private TitleCanvas _titleCanvas;

    [Header("테스트 용")]
    [SerializeField] private Button _testBtn;


    private void Start()
    {
        Manager.firebase.StartNetworkCoroutine();
        StartCoroutine(DownloadingCoroutine());
        if(_testBtn != null)
        {
            _testBtn.onClick.AddListener(TestButtonClick);
        }
    }

    private void OnDestroy()
    {
        if (_testBtn != null)
        {
            _testBtn.onClick.RemoveListener(TestButtonClick);
        }
    }

    // 테스트용 함수
    private void TestButtonClick()
    {
        Manager.firebase.Auth.SignOut();
        Debug.Log(_loginCanvas);
        Debug.Log(_loginCanvas.LoginControl);
        Debug.Log(_loginCanvas.LoginControl.IsLogined);
        _loginCanvas.LoginControl.IsLogined.Value = false;
        _titleCanvas.gameObject.SetActive(false);
        StartCoroutine(WaitLogin());
    }

    private IEnumerator DownloadingCoroutine()
    {
        yield return _loadingCanvas.StartDownloadCoroutine();

        StartCoroutine(WaitAutoLogin());
    }

    private IEnumerator WaitAutoLogin()
    {
        yield return new WaitUntil(() => _autoLogin.IsPlayGameLoginEnd);

        if (_autoLogin.IsLogined)
        {
            Manager.firebase.InitUserData();
            yield return new WaitForSeconds(1f);
            _titleCanvas.gameObject.SetActive(true);
        }
        else
        {
            StartCoroutine(WaitLogin());
        }

        _loadingCanvas.gameObject.SetActive(false);
    }

    private IEnumerator WaitLogin()
    {
        _loginCanvas.gameObject.SetActive(true);

        yield return _loginCanvas.WaitLoginEnd();
        Manager.firebase.InitUserData();

        _loginCanvas.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        _titleCanvas.gameObject.SetActive(true);
    }
}

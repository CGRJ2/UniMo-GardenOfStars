using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private AutoLoginController _autoLogin;

    [SerializeField] private LoadingCanvas_Title _loadingCanvas;

    [SerializeField] private LoginCanvas _loginCanvas;

    [SerializeField] private TitleCanvas _titleCanvas;

    private void Start()
    {
        Manager.firebase.StartNetworkCoroutine();
        StartCoroutine(DownloadingCoroutine());
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginCanvas : KYS.BaseUI
{
    private LoginController _login;
    public LoginController LoginControl { get
        {
            if(_login == null)
            {
                Awake();
            }

            return _login;
        } }

    private Button _playGamesLoginBtn;
    private Button _guestLoginBtn;

    private WaitUntil delayUntil;

    protected override void Awake()
    {
        base.Awake();
        _login = GetComponent<LoginController>();
        _playGamesLoginBtn = GetUI<Button>("PlayGamesLoginButton");
        _guestLoginBtn = GetUI<Button>("GuestLoginButton");

        _login.IsLoggingIn.Subscribe(ButtonSetDisable);
        delayUntil = new WaitUntil(() => _login.IsLogined.Value);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _login.IsLoggingIn.Unsubscribe(ButtonSetDisable);
    }

    private void ButtonSetDisable(bool value)
    {
        _guestLoginBtn.interactable = !value;
        _playGamesLoginBtn.interactable = !value;
    }

    public IEnumerator WaitLoginEnd()
    {
        yield return delayUntil;
    }
}

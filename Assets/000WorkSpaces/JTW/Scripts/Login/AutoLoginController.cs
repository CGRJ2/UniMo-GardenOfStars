using Firebase.Auth;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

// 타이틀 등 맨 처음 씬에 넣을 용도.
public class AutoLoginController : MonoBehaviour 
{
    public bool IsPlayGameLoginEnd;
    public bool IsLogined;
    private FirebaseAuth Auth => Manager.firebase.Auth;

    private void Start()
    {
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(OnAuthenticate);
    }

    private void OnAuthenticate(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("PlayGame로그인 성공");

            if (Auth == null)
            {
                Manager.firebase.OnFirebaseInit -= PlayGamesToFirebase;
                Manager.firebase.OnFirebaseInit += PlayGamesToFirebase;
                return;
            }

            PlayGamesToFirebase();
        }
        else
        {
            Debug.Log($"PlayGames 로그인 실패 : {status}");

            if(Auth == null)
            {
                Manager.firebase.OnFirebaseInit -= CheckAnonymousLogin;
                Manager.firebase.OnFirebaseInit += CheckAnonymousLogin;
                return;
            }

            CheckAnonymousLogin();
        }
    }

    private void CheckAnonymousLogin()
    {
        if (Auth.CurrentUser != null && Auth.CurrentUser.IsAnonymous)
        {
            IsLogined = true;
            IsPlayGameLoginEnd = true;
            return;
        }

        IsLogined = false;
        IsPlayGameLoginEnd = true;
    }

    private void PlayGamesToFirebase()
    {
        if (Auth.CurrentUser != null)
        {
            Auth.SignOut();
        }

        PlayGamesPlatform.Instance.RequestServerSideAccess(false, authCode =>
        {
            Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

            Manager.firebase.Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Manager.firebase.NetworkDisconnected();
                    return;
                }

                Debug.Log("파이어베이스 연동 성공");
                IsLogined = true;
                IsPlayGameLoginEnd = true;
            });
        });
    }
}

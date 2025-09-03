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

            if (Auth.CurrentUser != null && Auth.CurrentUser.IsAnonymous)
            {
                // TODO : 타이틀 화면으로
                IsLogined = true;
                IsPlayGameLoginEnd = true;
                return;
            }

            IsLogined = false;
            IsPlayGameLoginEnd = true;
            // TODO : 로그인 화면으로 전환
        }
    }

    // TODO : Firebase 서버의 문제로 중단되면 그에 맞는 처리 추가.
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
                if (task.IsCanceled)
                {
                    Debug.Log("파이어베이스 연동 중단");
                    IsPlayGameLoginEnd = true;
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.Log($"파이어베이스 연동 실패 : {task.Exception}");
                    IsPlayGameLoginEnd = true;
                    return;
                }

                Debug.Log("파이어베이스 연동 성공");
                IsLogined = true;
                IsPlayGameLoginEnd = true;
            });
        });
    }
}

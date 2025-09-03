using Firebase.Auth;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class LoginController : MonoBehaviour
{
    public ObservableProperty<bool> IsLogined = new();

    private FirebaseAuth Auth => Manager.firebase.Auth;

    public void PlayGameLogin()
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate(OnMaunuallyAuthenticate);
    }

    private void OnMaunuallyAuthenticate(SignInStatus status)
    {
        if (status != SignInStatus.Success)
        {
            Debug.Log($"PlayGames 로그인 실패 : {status}");
            return;
        }

        Debug.Log("PlayGame로그인 성공");

        PlayGamesToFirebase();
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
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.Log($"파이어베이스 연동 실패 : {task.Exception}");
                    return;
                }

                Debug.Log("파이어베이스 연동 성공");
                IsLogined.Value = true;
            });
        });
    }

    public void GusetLogin()
    {
        Manager.firebase.Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("익명 로그인 중단");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.Log($"익명 로그인 실패 : {task.Exception}");
                return;
            }

            Debug.Log("익명 로그인 성공");
            IsLogined.Value = true;
        });
    }
}

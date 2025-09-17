using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using System.Runtime.CompilerServices;
using Firebase.Database;

public class LinkPlayGamesController : MonoBehaviour
{
    public bool IsLinked;

    public void LinkPlayGames()
    {
        IsLinked = false;
        PlayGamesPlatform.Instance.ManuallyAuthenticate(OnLinkPlayGames);
    }

    private void OnLinkPlayGames(SignInStatus status)
    {
        if (status != SignInStatus.Success)
        {
            Debug.Log("계정 연동 실패");
            return;
        }

        PlayGamesPlatform.Instance.RequestServerSideAccess(false, authCode =>
        {
            Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

            Manager.firebase.Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("계정 연동 중단");
                    Manager.firebase.NetworkDisconnected();
                    return;
                }

                if (task.IsFaulted)
                {
                    foreach (var e in task.Exception.Flatten().InnerExceptions)
                    {
                        if (e is FirebaseAccountLinkException ex)
                        {
                            var errcode = (AuthError)ex.ErrorCode;
                            if (errcode == AuthError.CredentialAlreadyInUse)
                            {
                                Debug.Log("이미 연동한적 있는 PlayGame 계정입니다.");

                                string guestUserId = Manager.firebase.Auth.CurrentUser.UserId;
                                Manager.firebase.Auth.SignOut();

                                Manager.firebase.Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                                {
                                    if(task.IsCanceled || task.IsFaulted)
                                    {
                                        Debug.Log("파이어베이스 로그인 실패");
                                        Manager.firebase.NetworkDisconnected();
                                        return;
                                    }

                                    FirebaseUser user = task.Result;

                                    string playGamesUserId = user.UserId;

                                    DatabaseReference playGamesRef = Manager.firebase.Database.RootReference.Child($"UserData/{playGamesUserId}");
                                    DatabaseReference guestRef = Manager.firebase.Database.RootReference.Child($"UserData/{guestUserId}");

                                    

                                    guestRef.GetValueAsync().ContinueWithOnMainThread(guestValue =>
                                    {
                                        if (guestValue.IsCanceled || guestValue.IsFaulted)
                                        {
                                            Debug.Log("게스트 데이터 불러오기 실패");
                                            Manager.firebase.NetworkDisconnected();
                                            return;
                                        }

                                        playGamesRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
                                        {
                                            DataSnapshot snapshot = guestValue.Result;

                                            playGamesRef.SetValueAsync(snapshot.Value).ContinueWithOnMainThread(task =>
                                            {
                                                if (task.IsCanceled || task.IsFaulted)
                                                {
                                                    Debug.Log("게스트 데이터 저장 실패");
                                                    Manager.firebase.NetworkDisconnected();
                                                    return;
                                                }

                                                IsLinked = true;
                                                Debug.Log("데이터 이전 완료");
                                                guestRef.RemoveValueAsync();
                                            });
                                        });
                                    });
                                });

                                return;
                            }
                            else
                            {
                                Debug.Log($"계정 연동 실패 : {e.Message}");
                            }
                        }
                    }
                    return;
                }

                Debug.Log("계정 연동 성공");
                IsLinked = true;
            });
        });
    }
}

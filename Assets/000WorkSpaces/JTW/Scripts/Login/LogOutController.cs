using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogOutController : MonoBehaviour
{
    private FirebaseAuth Auth => Manager.firebase.Auth;

    public void LogOut()
    {
        // 익명만 로그아웃 가능
        if (!Auth.CurrentUser.IsAnonymous) return;

        Auth.SignOut();
        // TODO : 로그인 화면으로
    }
}

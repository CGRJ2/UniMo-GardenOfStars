using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackendManager : Singleton<BackendManager>
{
    public static FirebaseApp App { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }

    private void Awake() => Init();

    public void Init()
    {
        base.SingletonInit();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("[BackendManager] Firebase dependencies ok");
                App = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                Database = FirebaseDatabase.DefaultInstance;
            }
            else
            {
                Debug.LogError($"[BackendManager] Firebase dependency error: {dependencyStatus}");
                App = null; Auth = null; Database = null;
            }
        });
    }


}

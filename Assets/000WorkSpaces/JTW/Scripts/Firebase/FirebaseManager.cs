using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : Singleton<FirebaseManager>
{
    private static FirebaseApp _app;
    public static FirebaseApp App => _app;

    private static FirebaseAuth _auth;
    public static FirebaseAuth Auth => _auth;

    private static FirebaseDatabase _database;
    public static FirebaseDatabase Database => _database;

    private DatabaseReference _userRef;

    private DatabaseReference UserRef 
    { 
        get
        {
            if(_userRef == null)
            {
                _userRef = _database.RootReference.Child($"UserData/{_auth.CurrentUser.UserId}");
            }

            return _userRef;
        }
    }

    private DatabaseReference _stageRef;

    private DatabaseReference StageRef
    {
        get
        {
            if (_stageRef == null)
            {
                // TODO : 이후에 GameManager.CurStage로 변경.
                _stageRef = _userRef.Child($"StageData/StageId");
            }

            return _stageRef;
        }
    }

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                _app = FirebaseApp.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                _database = FirebaseDatabase.DefaultInstance;
                Debug.Log("파이어베이스 연결 성공");
            }
            else
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                _app = null;
                _auth = null;
                _database = null;
            }
        });
    }

    public void SetDataEvent(string path, EventHandler<ValueChangedEventArgs> func)
    {
        // 추후에 로그인 로직이 생기면 변경 예정
        if (_userRef == null)
        {
            _database.RootReference.Child("Userdata").Child(path).ValueChanged += func;
            return;
        }

        UserRef.Child(path).ValueChanged += func;
    }


    public void SaveData(string path, object value)
    {
        // 추후에 로그인 로직이 생기면 변경 예정
        if (_userRef == null)
        {
            _database.RootReference.Child("Userdata").Child(path).SetValueAsync(value);
            return;
        }

        UserRef.Child(path).SetValueAsync(value);
    }

    public void SaveStageData(string path, object value)
    {
        // StageID도 통합 진행.
        // path = $"{Manager.game.CurStageId}/{path}";

        StageRef.Child(path).SetValueAsync(value);
    }

    public void SaveJsonData(string path, string json)
    {
        UserRef.Child(path).SetRawJsonValueAsync(json);
    }
    public void SaveStageJsonData(string path, string json)
    {
        // StageID도 통합 진행.
        // path = $"{Manager.game.CurStageId}/{path}";

        StageRef.Child(path).SetRawJsonValueAsync(json);
    }
}

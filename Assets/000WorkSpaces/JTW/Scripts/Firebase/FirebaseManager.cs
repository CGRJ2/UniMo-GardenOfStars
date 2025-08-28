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
        _database.RootReference.Child(path).ValueChanged += func;
    }

    public void SetDataListEvent(string path, EventHandler<ChildChangedEventArgs> func)
    {
        _database.RootReference.Child(path).ChildAdded += func;
    }


    public void SaveData(string path, object value)
    {
        _database.RootReference.Child(path).SetValueAsync(value);
    }

    public void SaveJsonData(string path, string json)
    {
        _database.RootReference.Child(path).SetRawJsonValueAsync(json);
    }
}

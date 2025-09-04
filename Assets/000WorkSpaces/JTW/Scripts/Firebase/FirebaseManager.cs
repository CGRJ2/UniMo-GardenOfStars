using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEngine;

public class FirebaseManager : Singleton<FirebaseManager>
{
    private FirebaseApp _app;
    public FirebaseApp App => _app;

    private FirebaseAuth _auth;
    public FirebaseAuth Auth => _auth;

    private FirebaseDatabase _database;
    public FirebaseDatabase Database => _database;

    public UserData UserData;

    public event Action OnFirebaseInit;

    public bool IsFirebaseInit;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                _app = FirebaseApp.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                _database = FirebaseDatabase.DefaultInstance;
                Debug.Log("파이어베이스 연결 성공");

                UserData = new UserData("UserData", "");

                OnFirebaseInit.Invoke();

                IsFirebaseInit = true;
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

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private DataSnapshot _rootDataSnapshot;
    private bool _isUserDataInit;

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

                // 테스트를 원활하게 하기위해 일단 실행
                // 추후에 게임이 완성에 가까우면 뺄 수도 있음.

                OnFirebaseInit?.Invoke();

                InitUserData();

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

    public void InitUserData()
    {
        _isUserDataInit = false;

        string userPath;

        if(_auth.CurrentUser == null)
        {
            userPath = $"UserData/testUser1234";
        }
        else
        {
            userPath = $"UserData/{_auth.CurrentUser.UserId}";
            Debug.LogWarning($"현재 UserId : {_auth.CurrentUser.UserId}");
        }

        _database.RootReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            _rootDataSnapshot = task.Result;

            UserData = new UserData(userPath, "");

            StartCoroutine(CheckUserDataInit());
        });
    }

    private IEnumerator CheckUserDataInit()
    {
        yield return new WaitUntil(() => UserData.IsInit);

        Debug.LogWarning("[FirebaseManager] UserData 초기화 완료");

        _isUserDataInit = true;
    }
    
    public bool SetDataEvent<T>(string path, EventHandler<ValueChangedEventArgs> func, T setValue, out T value)
    {
        if (_isUserDataInit)
        {
            value = setValue;
            _database.RootReference.Child(path).SetValueAsync(value).ContinueWithOnMainThread(task =>
            {
                if(task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("FirebaseProperty 값 변경 실패");
                    return;
                }

                _database.RootReference.Child(path).ValueChanged += func;
            });
            return true;
        }
        else
        {
            if (!_rootDataSnapshot.Child(path).Exists)
            {
                value = setValue;
            }
            else
            {
                DataSnapshot snapshot = _rootDataSnapshot.Child(path);

                if (typeof(T) == typeof(int))
                {
                    long valueT = (long)snapshot.Value;
                    value = (T)(object)(int)valueT;
                }
                else if (typeof(T) == typeof(float))
                {
                    double valueT = (double)snapshot.Value;
                    value = (T)(object)(float)valueT;
                }
                else
                {
                    value = (T)snapshot.Value;
                }
            }

            return false;
        }
        
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

    public bool CheckInit(string path, out int count)
    {
        count = 0;

        if (_isUserDataInit) return true;

        DataSnapshot data = _rootDataSnapshot.Child(path);

        if(!data.Exists || !data.HasChildren) return true;

        count = (int)data.ChildrenCount;
        return false;
    }
}

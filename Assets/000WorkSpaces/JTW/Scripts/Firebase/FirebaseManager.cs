using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FirebaseManager;

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

    private Coroutine _checkInitCoroutine;
    private WaitForSeconds _delay = new WaitForSeconds(0.1f);

    private Queue<CheckInitData> _checkInitQueue = new Queue<CheckInitData>();

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
                InitUserData();

                OnFirebaseInit?.Invoke();

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
        if(_auth.CurrentUser == null)
        {
            UserData = new UserData($"UserData/testUser1234", "");
        }
        else
        {
            UserData = new UserData($"UserData/{_auth.CurrentUser.UserId}", "");
        }
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

    public class CheckInitData
    {
        public string Path;
        public Action<DataSnapshot> OnCompleted;
    }

    public void CheckInit(string path, Action<DataSnapshot> onCompleted)
    {
        CheckInitData data = new CheckInitData();
        data.Path = path;
        data.OnCompleted = onCompleted;

        _checkInitQueue.Enqueue(data);

        if(_checkInitCoroutine == null)
        {
            _checkInitCoroutine = StartCoroutine(CheckInitCoroutine());
        }
    }

    private IEnumerator CheckInitCoroutine()
    {
        while(_checkInitQueue.Count > 0)
        {
            CheckInitData data = _checkInitQueue.Dequeue();

            Debug.LogWarning($"{data.Path} 경로 초기화 검색");

            bool done = false;
            var task = _database.RootReference.Child(data.Path).GetValueAsync();

            yield return task;

            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogWarning($"{data.Path} 경로 초기화 실패");
                continue;
            }

            data.OnCompleted(task.Result);
            done = true;

            yield return _delay;
        }

        _checkInitCoroutine = null;
    }
}

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseManager : Singleton<FirebaseManager>
{
    private FirebaseApp _app;
    public FirebaseApp App => _app;

    private FirebaseAuth _auth;
    public FirebaseAuth Auth => _auth;

    private FirebaseDatabase _database;
    public FirebaseDatabase Database => _database;

    public ObservableProperty<UserData> UserDataProperty = new();

    public UserData UserData => UserDataProperty.Value;
    private DataSnapshot _rootDataSnapshot;
    private bool _isUserDataInit;

    public event Action OnFirebaseInit;

    public bool IsFirebaseInit;

    private void Awake()
    {
        InitFirebase();
    }

    private void InitFirebase()
    {
        if (IsFirebaseInit) return;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                _app = FirebaseApp.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                _database = FirebaseDatabase.DefaultInstance;
                _database.SetPersistenceEnabled(false);
                _database.GoOnline();
                Debug.Log("파이어베이스 연결 성공");

                OnFirebaseInit?.Invoke();

#if UNITY_EDITOR
                InitUserData();
#endif

                IsFirebaseInit = true;
            }
            else
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                _app = null;
                _auth = null;
                _database = null;
                NetworkDisconnected();
            }
        });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitFirebase();
        StartCoroutine(Manager.game.Fetch());
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool _isNetworkDisconected;
    public void NetworkDisconnected()
    {
        if (_isNetworkDisconected) return;
        _isNetworkDisconected = true;

        Manager.ui.ShowMessagePopUpAsync("인터넷 연결을 다시 확인해주세요.", () =>
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("TitleScene");

            _isNetworkDisconected = false;
        });
    }

    public void InitUserData()
    {
        _isUserDataInit = false;

        string userPath;

        if (_auth.CurrentUser == null)
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
            if (task.IsCanceled || task.IsFaulted)
            {
                NetworkDisconnected();
                return;
            }

            _rootDataSnapshot = task.Result;

            UserDataProperty.Value = new UserData(userPath, "");

            StartCoroutine(CheckUserDataInit());
        });
    }

    private IEnumerator CheckUserDataInit()
    {
        yield return new WaitUntil(() => UserData.IsInit);

        Debug.LogWarning($"[FirebaseManager] UserData 초기화 완료. 현재 스테이지 {UserData.CurStage.Value}");

        _isUserDataInit = true;
    }

    public bool SetTimeDataEvent<T>(string id, string path, string parentPath, EventHandler<ValueChangedEventArgs> func)
    {
        var update = new Dictionary<string, object>();
        update[id] = ServerValue.Timestamp;

        Manager.firebase.Database.RootReference.Child(parentPath).UpdateChildrenAsync(update).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                NetworkDisconnected();
                return;
            }

            _database.RootReference.Child(path).ValueChanged += func;
        });
        return true;
    }

    public bool SetDataEvent<T>(string path, EventHandler<ValueChangedEventArgs> func, T setValue, bool isInit, out T value)
    {
        if (!isInit)
        {
            value = setValue;
            _database.RootReference.Child(path).SetValueAsync(value).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    NetworkDisconnected();
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
        _database.RootReference.Child(path).SetValueAsync(value).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                NetworkDisconnected();
                return;
            }
        });
    }

    public void SaveTransactionData<T>(string path, object value)
    {
        _database.RootReference.Child(path).RunTransaction(data =>
        {
            object dataValue = data.Value ?? default(T);

            if (typeof(T) == typeof(int))
            {
                int curValue = Convert.ToInt32(dataValue);
                data.Value = curValue + (int)value;
            }
            else if (typeof(T) == typeof(long))
            {
                long curValue = Convert.ToInt64(dataValue);
                data.Value = curValue + (long)value;
            }
            else if (typeof(T) == typeof(float))
            {
                float curValue = Convert.ToSingle(dataValue);
                data.Value = curValue + (float)value;
            }
            else if (typeof(T) == typeof(double))
            {
                double curValue = Convert.ToDouble(dataValue);
                data.Value = curValue + (double)value;
            }
            else
            {
                data.Value = value;
            }

            return TransactionResult.Success(data);
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                NetworkDisconnected();
                return;
            }
        });
    }

    public void SaveJsonData(string path, string json)
    {
        _database.RootReference.Child(path).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                NetworkDisconnected();
                return;
            }
        });
    }

    public bool CheckInit(string path, out int count)
    {
        count = 0;

        if (_isUserDataInit) return true;

        DataSnapshot data = _rootDataSnapshot.Child(path);

        if (!data.Exists || !data.HasChildren) return true;

        count = (int)data.ChildrenCount;
        return false;
    }

    private WaitForSeconds _pingDelay = new WaitForSeconds(5f);
    private Coroutine _pingCoroutine;

    public void StartNetworkCoroutine()
    {
        if (_pingCoroutine != null)
        {
            StopCoroutine(_pingCoroutine);
            _pingCoroutine = null;
        }

        _pingCoroutine = StartCoroutine(NetworkCoroutine());
    }

    private IEnumerator NetworkCoroutine()
    {
        yield return _pingDelay;

        while (true)
        {
            Ping ping = new Ping("8.8.8.8");
            float startTime = Time.time;
            float timeout = 5f;

            while (!ping.isDone)
            {
                if (Time.time - startTime > timeout)
                {
                    NetworkDisconnected();
                    Debug.Log("네트워크 연결 안됨 (Ping Timeout)");
                    yield break;
                }
                yield return null;
            }

            if (ping.time >= 0)
            {
                Debug.Log($"네트워크 연결됨 (Ping {ping.time}ms)");
            }
            else
            {
                NetworkDisconnected();
                Debug.Log("네트워크 연결 실패");
                yield break;
            }

            yield return _pingDelay;
        }
    }
}

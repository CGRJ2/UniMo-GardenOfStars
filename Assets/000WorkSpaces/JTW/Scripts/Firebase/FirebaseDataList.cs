using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseDataList<T> : FirebaseData where T : FirebaseData
{
    private List<T> _list = new();

    private Func<string, string, T> _factory;

    public UnityEvent<T> OnAdded = new();

    public int Count => _list.Count;

    public List<T> List { 
        get
        {
            return _list;
        } 
    }

    public FirebaseDataList(string id, string parentPath, Func<string, string, T> factory) : base(id, parentPath)
    {
        _factory = factory;

        Manager.firebase.SetDataListEvent(Path, OnFirebaseChanged);

        IsInitSelf = Manager.firebase.CheckInit(Path, out ListInitCount);
    }

    private void OnFirebaseChanged(object sender, ChildChangedEventArgs args)
    {
        T child = _factory(args.Snapshot.Key, Path);

        _list.Add(child);
        InitList.Add(child);

        WaitUntilAsync(child);
    }

    public void Add(IUsableId value)
    {
        string json = JsonUtility.ToJson(value);

        Manager.firebase.SaveJsonData($"{Path}/{value.GetId()}", json);
    }

    public void Add(string Id)
    {
        Manager.firebase.SaveData($"{Path}/{Id}", true);
    }

    public T Get(string id)
    {
        return _list.Find(value => value.GetId() == id);
    }

    private async Task WaitUntilAsync(T child)
    {
        while (!child.IsInit)
        {
            await Task.Delay(50);
        }

        OnAdded.Invoke(child);
    }
}

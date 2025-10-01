using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseDataList<T> : FirebaseData where T : FirebaseData
{
    private List<T> _list = new();
    private List<string> _keyList = new();

    private Func<string, string, T> _factory;

    public UnityEvent<T> OnAdded = new();

    public int Count => _list.Count;

    public List<T> List { 
        get
        {
            return _list.OrderBy(item => item.Id).ToList();
        } 
    }

    public FirebaseDataList(string id, string parentPath, Func<string, string, T> factory) : base(id, parentPath)
    {
        _factory = factory;

        Manager.firebase.SetDataListEvent(Path, OnFirebaseChanged);

        IsInitSelf = Manager.firebase.CheckInit(Path, out ListInitCount);
    }

    private async void OnFirebaseChanged(object sender, ChildChangedEventArgs args)
    {
        T child = _factory(args.Snapshot.Key, Path);

        _list.Add(child);
        InitList.Add(child);

        await WaitUntilAsync(child);
    }

    public void Add(IUsableId value)
    {
        string json = JsonUtility.ToJson(value);

        Manager.firebase.SaveJsonData($"{Path}/{value.GetId()}", json);
    }

    public void Add(string Id)
    {
        if (_keyList.Contains(Id)) return;

        Manager.firebase.SaveData($"{Path}/{Id}", true);
        ListInitCount++;
        _keyList.Add(Id);
    }

    public void AddListItem(T item)
    {
        _list.Add(item);
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

        OnAdded?.Invoke(child);
    }
}

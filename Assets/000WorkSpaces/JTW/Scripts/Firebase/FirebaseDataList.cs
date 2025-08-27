using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseDataList<T> : FirebaseData where T : FirebaseData
{
    private List<T> _list;

    private string Path => ParentPath != null ? $"{ParentPath}/{Id}" : Id;

    private Func<string, string, T> _factory;

    public FirebaseDataList(string id, Func<string, string, T> factory, string parentPath = null) : base(id, parentPath)
    {
        _factory = factory;

        Manager.firebase.SetUserDataListEvent(Path, OnFirebaseChanged);
    }

    private void OnFirebaseChanged(object sender, ChildChangedEventArgs args)
    {
        T child = _factory(args.Snapshot.Key, Path);

        _list.Add(child);
    }

    public void Add(T value)
    {
        Manager.firebase.SaveUserData($"{Path}/{value.GetId()}", new Dictionary<string, object>());
    }

    public T Get(string id)
    {
        return _list.Find(value => value.GetId() == id);
    }
}

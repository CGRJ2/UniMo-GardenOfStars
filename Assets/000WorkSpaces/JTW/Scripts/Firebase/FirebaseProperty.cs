using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FirebaseProperty<T> : FirebaseData
{
    [SerializeField] private T _value;

    private string Path => ParentPath != null ? $"{ParentPath}/{Id}" : Id;

    public T Value
    {
        get => _value;
        set
        {
            if (object.Equals(_value, value)) return;
            Manager.firebase.SaveData(Path, value);
        }
    }
    private UnityEvent<T> _onValueChanged = new();

    public FirebaseProperty(string id, string parentPath = null) : base(id, parentPath)
    {
        Manager.firebase.SetDataEvent(Path, OnFirebaseChanged);
    }

    private void OnFirebaseChanged(object sender, ValueChangedEventArgs args)
    {
        _value = (T)args.Snapshot.Value;
        Notify();
    }

    public void Subscribe(UnityAction<T> action)
    {
        _onValueChanged.AddListener(action);
    }

    public void Unsubscribe(UnityAction<T> action)
    {
        _onValueChanged.RemoveListener(action);
    }

    public void UnsubscribeAll()
    {
        _onValueChanged.RemoveAllListeners();
    }

    private void Notify()
    {
        _onValueChanged?.Invoke(Value);
    }
}

using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FirebaseProperty<T>
{
    [SerializeField] private T _value;

    private string _path;

    public T Value
    {
        get => _value;
        set
        {
            if (object.Equals(_value, value)) return;
            Manager.firebase.SaveData(_path, value);
        }
    }
    private UnityEvent<T> _onValueChanged = new();

    public FirebaseProperty(string path)
    {
        _path = path;

        Manager.firebase.SetDataEvent(path, OnFirebaseChanged);
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

    private void OnFirebaseChanged(object sender, ValueChangedEventArgs args)
    {
        _value = (T)args.Snapshot.Value;
        Notify();
    }
}

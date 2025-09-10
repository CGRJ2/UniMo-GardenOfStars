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
    private T _default;

    public T Value
    {
        get => _value;
        set
        {
            if (object.Equals(_value, value)) return;

            if (!_isFirebaseConnected)
            {
                _isFirebaseConnected = Manager.firebase.SetDataEvent<T>(Path, OnFirebaseChanged, out _value);
            }

            Manager.firebase.SaveData(Path, value);
        }
    }
    private UnityEvent<T> _onValueChanged = new();

    private bool _isFirebaseConnected;

    public FirebaseProperty(string id, string parentPath, T value = default) : base(id, parentPath)
    {
        _default = value;
        _isFirebaseConnected = Manager.firebase.SetDataEvent<T>(Path, OnFirebaseChanged, out _value);

        if (!_isFirebaseConnected) IsInitSelf = true;
    }

    private void OnFirebaseChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.Snapshot.Value == null)
        {
            Manager.firebase.SaveData(Path, _default);
            return;
        }

        if(typeof(T) == typeof(int))
        {
            long value = (long)args.Snapshot.Value;
            _value = (T)(object)(int)value;
        }
        else if(typeof(T) == typeof(float))
        {
            double value = (double)args.Snapshot.Value;
            _value = (T)(object)(float)value;
        }
        else
        {
            _value = (T)args.Snapshot.Value;
        }

        IsInitSelf = true;

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

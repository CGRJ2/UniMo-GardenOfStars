using Firebase.Database;
using System;
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
    private bool _isImmediate;

    public T Value
    {
        get => _value;
        set
        {
            if (object.Equals(_value, value)) return;

            if (_isImmediate)
            {
                object result = null;

                if (typeof(T) == typeof(int))
                {
                    int curValue = Convert.ToInt32(_value);
                    int updataValue = Convert.ToInt32(value);
                    result = updataValue - curValue;
                }
                else if (typeof(T) == typeof(long))
                {
                    long curValue = Convert.ToInt64(_value);
                    long updataValue = Convert.ToInt64(value);
                    result = updataValue - curValue;
                }
                else if (typeof(T) == typeof(float))
                {
                    float curValue = Convert.ToSingle(_value);
                    float updataValue = Convert.ToSingle(value);
                    result = updataValue - curValue;
                }
                else if (typeof(T) == typeof(double))
                {
                    double curValue = Convert.ToDouble(_value);
                    double updataValue = Convert.ToDouble(value);
                    result = updataValue - curValue;
                }
                else
                {
                    result = value;
                }

                Manager.firebase.SaveTransactionData<T>(Path, result);
                _value = value;
                Notify();
                return;
            }

            IsInUpdate = true;

            if (!_isFirebaseConnected)
            {
                _isFirebaseConnected = Manager.firebase.SetDataEvent<T>(Path, OnFirebaseChanged, value, false, out _value);
                return;
            }

            Manager.firebase.SaveData(Path, value);
        }
    }
    private UnityEvent<T> _onValueChanged = new();

    private bool _isFirebaseConnected;

    public bool IsInUpdate;

    public FirebaseProperty(string id, string parentPath, T value = default, bool isImmediate = false) : base(id, parentPath)
    {
        _default = value;
        _isImmediate = isImmediate;

        Manager.firebase.SetDataEvent<T>(Path, OnFirebaseChanged, _default, true, out _value);

        IsInitSelf = true;
    }

    private void OnFirebaseChanged(object sender, ValueChangedEventArgs args)
    {
        if (_isImmediate) return;

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

        Notify();

        IsInUpdate = false;
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

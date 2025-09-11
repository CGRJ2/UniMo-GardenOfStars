using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class FirebaseData : IUsableId
{
    public string Id;
    protected string ParentPath;
    protected string Path => string.IsNullOrEmpty(ParentPath) ? Id : $"{ParentPath}/{Id}";

    protected List<FirebaseData> InitList = new();
    public int ListInitCount;

    protected bool IsInitSelf;
    public bool IsInit => InitList.Count == 0 ? IsInitSelf : InitList.All(data => data.IsInit) && ListInitCount <= InitList.Count;

    public string GetId()
    {
        return Id;
    }

    public FirebaseData(string id, string parentPath)
    {
        Id = id;
        ParentPath = parentPath;
    }
}
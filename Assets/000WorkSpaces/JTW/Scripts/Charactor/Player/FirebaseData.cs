using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FirebaseData : IUsableId
{
    public string Id;
    protected string ParentPath;
    protected string Path => string.IsNullOrEmpty(ParentPath) ? Id : $"{ParentPath}/{Id}";
    public string GetId()
    {
        return Id;
    }

    public FirebaseData(string id, string parentPath = null)
    {
        Id = id;
        ParentPath = parentPath;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FirebaseData : IUsableId
{
    protected string Id;
    protected string ParentPath;

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

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
    public bool IsInit { get
        {
            bool result = InitList.Count == 0 ? IsInitSelf : InitList.All(data => data.IsInit) && ListInitCount <= InitList.Count;

            if(result == false)
            {
                if (InitList.Count == 0 && IsInitSelf == false)
                {
                    Debug.LogWarning($"{Path}가 Init되지 않음");
                }
                else if(InitList.Count < ListInitCount)
                {
                    Debug.LogWarning($"{Path}의 자식이 Add만큼 생성되지 않음 Add 횟수 : {ListInitCount}, 자식 개수 : {InitList.Count}");
                }
            }

            return result;
        } }
        

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
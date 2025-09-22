using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EquipSkinData : FirebaseData
{
    public EquipSkinData(string id, string parentPath) : base(id, parentPath)
    {
        IsInitSelf = true;
    }
}

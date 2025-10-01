using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkinData : FirebaseData
{
    public CharacterSkinData(string id, string parentPath) : base(id, parentPath)
    {
        IsInitSelf = true;
    }
}

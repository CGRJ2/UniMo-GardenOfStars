using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinData : FirebaseData
{
    public FirebaseDataList<CharacterSkinData> CharacterSkinList;
    public FirebaseDataList<EquipSkinData> EquipSkinList;

    public SkinData(string id, string parentPath) : base(id, parentPath)
    {
        CharacterSkinList = new FirebaseDataList<CharacterSkinData>("CharacterSkinList", Path, (id, parentPath) =>
        {
            return new CharacterSkinData(id, parentPath);
        });

        EquipSkinList = new FirebaseDataList<EquipSkinData>("EquipSkinList", Path, (id, parentPath) =>
        {
            return new EquipSkinData(id, parentPath);
        });

        InitList.Add(CharacterSkinList);
        InitList.Add(EquipSkinList);
    }
}

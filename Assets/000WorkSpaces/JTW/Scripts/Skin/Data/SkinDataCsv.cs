using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinDataCsv : IUsableId
{
    public string Id;

    public SkinTypes Type;

    public string Name_Kr;
    public string Name_En;

    public string Name
    {
        get
        {
            switch (Manager.localization.CurrentLanguage)
            {
                case SystemLanguage.Korean:
                    return Name_Kr;
                case SystemLanguage.English:
                    return Name_En;
            }

            return Name_Kr;
        }
    }

    public int Cost;

    public Sprite Sprite;
    public GameObject Skin;

    public string GetId()
    {
        return Id;
    }
}

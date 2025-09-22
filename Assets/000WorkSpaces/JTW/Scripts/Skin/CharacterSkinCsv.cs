using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkinCsv : IUsableId
{
    public string Id;

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

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{

    [SerializeField] private bool _isCharacterSkinAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _characterSkinDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _characterSkinAdress = "CharacterSkinCsv";

    public DataTableParser<CharacterSkinCsv> CharacterSkin;
    private async void CharacterSkinCsvRoutine()
    {
        string dataCsv;

        if (_isCharacterSkinAdressable)
        {
            dataCsv = await GetDataString(_isCharacterSkinAdressable, _characterSkinAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isCharacterSkinAdressable, _characterSkinDataTableURL);
        }

        CharacterSkin = new DataTableParser<CharacterSkinCsv>((words, dict) =>
        {
            CharacterSkinCsv skin = new CharacterSkinCsv();

            skin.Id = words[dict["CharacterSkinID"]];

            skin.Name_Kr = words[dict["Name_KR"]];
            skin.Name_En = words[dict["Name_UN"]];

            int.TryParse(words[dict["Cost"]], out skin.Cost);
            

            return skin;
        });

        CharacterSkin.Load(dataCsv);
    }
}

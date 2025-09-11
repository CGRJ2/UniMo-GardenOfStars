using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CharacterDataCsv : IUsableId
{
    public string Id;

    public string Name_Kr;
    public string Name_En;
    public string Description;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isCharacterAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _characterDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _characterAdress = "CharacterCsv";

    public DataTableParser<CharacterDataCsv> CharacterCost;
    private async void CharacterRoutine()
    {
        string dataCsv;

        if (_isCharacterAdressable)
        {
            dataCsv = await GetDataString(_isCharacterAdressable, _characterAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isCharacterAdressable, _characterDataTableURL);
        }

        CharacterCost = new DataTableParser<CharacterDataCsv>((words, dict) =>
        {
            CharacterDataCsv character = new CharacterDataCsv();

            character.Id = words[dict["CharacterID"]];

            character.Name_Kr = words[dict["Name_Kr"]];
            character.Description = words[dict["Desc"]];

            return character;
        });

        CharacterCost.Load(dataCsv);
    }
}

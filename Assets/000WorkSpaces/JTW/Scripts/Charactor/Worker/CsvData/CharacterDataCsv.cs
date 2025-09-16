using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CharacterDataCsv : IUsableId
{
    public string Id;

    public string Name_Kr;
    public string Name_En;
    public string Description;

    public Sprite Sprite;
    public GameObject Avatar;

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

    public DataTableParser<CharacterDataCsv> Character;
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

        Character = new DataTableParser<CharacterDataCsv>((words, dict) =>
        {
            CharacterDataCsv character = new CharacterDataCsv();

            character.Id = words[dict["CharacterID"]];

            character.Name_Kr = words[dict["Name_Kr"]];
            character.Description = words[dict["Desc"]];

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"CharacterImage/{words[dict["CharacterID"]]}.png", typeof(Sprite), out var locations)))
            {
                Addressables.LoadAssetAsync<Sprite>($"CharacterImage/{words[dict["CharacterID"]]}.png").Completed += task =>
                {
                    if (task.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError("캐릭터 이미지 데이터 다운로드 실패");
                        return;
                    }
                    character.Sprite = task.Result;
                };
            }

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"CharacterPrefab/{words[dict["CharacterID"]]}.prefab", typeof(GameObject), out var locations)))
            {
                Addressables.LoadAssetAsync<GameObject>($"CharacterPrefab/{words[dict["CharacterID"]]}.prefab").Completed += task =>
                {
                    if (task.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError("캐릭터 이미지 데이터 다운로드 실패");
                        return;
                    }
                    character.Avatar = task.Result;
                };
            }

            return character;
        });

        Character.Load(dataCsv);
    }
}

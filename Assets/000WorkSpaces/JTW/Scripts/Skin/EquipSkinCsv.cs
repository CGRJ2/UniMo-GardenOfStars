using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EquipSkinCsv : IUsableId
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

    public Sprite Sprite;
    public GameObject EquipSkin;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{

    [SerializeField] private bool _isEquipSkinAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _equipSSkinDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _equipSSkinAdress = "EquipSSkinCsv";

    public DataTableParser<EquipSkinCsv> EquipSSkin;
    private async void EquipSkinCsvRoutine()
    {
        string dataCsv;

        if (_isEquipSkinAdressable)
        {
            dataCsv = await GetDataString(_isEquipSkinAdressable, _equipSSkinAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isEquipSkinAdressable, _equipSSkinDataTableURL);
        }

        EquipSSkin = new DataTableParser<EquipSkinCsv>((words, dict) =>
        {
            EquipSkinCsv skin = new EquipSkinCsv();

            skin.Id = words[dict["CharacterSkinID"]];

            skin.Name_Kr = words[dict["Name_KR"]];
            skin.Name_En = words[dict["Name_UN"]];

            int.TryParse(words[dict["Cost"]], out skin.Cost);

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"Image/Equip/{skin.Id}.png", typeof(Sprite), out var locations)))
            {
                Addressables.LoadAssetAsync<Sprite>($"Image/Equip/{skin.Id}.png").Completed += task =>
                {
                    if (task.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogWarning("WheelSprite 로드 실패");
                        return;
                    }

                    skin.Sprite = task.Result;
                };
            }

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"Skin/Equip/{skin.Id}.prefab", typeof(GameObject), out var locations)))
            {
                Addressables.LoadAssetAsync<GameObject>($"Skin/Equip/{skin.Id}.prefab").Completed += task =>
                {
                    if (task.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogWarning("WheelSprite 로드 실패");
                        return;
                    }

                    skin.EquipSkin = task.Result;
                };
            }

            return skin;
        });

        EquipSSkin.Load(dataCsv);
    }
}

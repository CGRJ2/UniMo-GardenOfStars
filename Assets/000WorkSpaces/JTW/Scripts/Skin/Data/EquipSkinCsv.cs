using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class DataManager
{
    [SerializeField] private bool _isEquipSkinAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _equipSSkinDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _equipSSkinAdress = "EquipSkinCsv";

    public DataTableParser<SkinDataCsv> EquipSkin;
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

        EquipSkin = new DataTableParser<SkinDataCsv>((words, dict) =>
        {
            SkinDataCsv skin = new SkinDataCsv();

            skin.Id = words[dict["CharacterSkinID"]];

            skin.Type = SkinTypes.Equip;

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

                    skin.Skin = task.Result;
                };
            }

            return skin;
        });

        EquipSkin.Load(dataCsv);
    }
}

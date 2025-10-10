using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BuildingData : IUsableId
{
    public string ID;
    public string Name_KR;
    public string Name_EN;
    public string Description_KR;
    public string Description_EN;
    public string ProductID;
    public int Cost;
    public UpgradableStat<float> Stat_ProdTime;
    public Sprite Sprite;

    public string GetId()
    {
        return ID;
    }
}

[Serializable]
public class UpgradableStat<T>
{
    public int MaxLevel;
    public List<T> Values;
    public List<float> Cost;

}
public class HarvestBD : BuildingData { }
public class ManufactureBD : BuildingData
{
    //[field: Header("작업시간 : 총생산시간 비율(준비시간)")]
    public float PrepareTimeRate;

    //[field: Header("투입 재료(소모) Id와 수량")]
    public string RequireProdID;
    public int RequireProdCount;

    //[field: Header("재료 스택 가능 개수(업그레이드 표)")]
    public UpgradableStat<int> Stat_Capacity;
}

public partial class DataManager
{
    public DataTableParser<BuildingData> Building;

    // Addressable 에셋 주소
    private const string _buildingDataAdress = "BuildingDataSheetCsv";

    public async void BuildingDataInitRoutine()
    {
        string dataCsv;
        dataCsv = await GetDataString(true, _buildingDataAdress);

        Building = new DataTableParser<BuildingData>((words, dict) =>
        {
            BuildingData building;
            if (string.IsNullOrEmpty(words[dict["RequireProdID"]]) || words[dict["RequireProdID"]] == "none")
                building = new HarvestBD();
            else building = new ManufactureBD();

            building.ID = words[dict["ID"]];
            building.Name_KR = words[dict["Name_KR"]];
            building.Name_EN = words[dict["Name_EN"]];
            building.Description_KR = words[dict["Description_KR"]];
            building.Description_EN = words[dict["Description_EN"]];
            building.ProductID = words[dict["ProductID"]];
            int.TryParse(words[dict["Cost"]], out building.Cost);

            List<float> Stat_ProdTime_List
            = words[dict["Stat_ProdTime"]].Trim('{', '}').Split(',').Select(s => float.Parse(s.Trim())).ToList();
            List<float> Stat_ProdTimeCost_List
            = words[dict["Stat_ProdTimeCost"]].Trim('{', '}').Split(',').Select(s => float.Parse(s.Trim())).ToList();
            building.Stat_ProdTime = new()
            {
                Values = Stat_ProdTime_List,
                Cost = Stat_ProdTimeCost_List,
                MaxLevel = Stat_ProdTimeCost_List.Count
            };

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"Sprite/{building.ID}.png", typeof(Sprite), out var locations)))
            {
                Addressables.LoadAssetAsync<Sprite>($"Sprite/{building.ID}.png").Completed += task =>
                {
                    building.Sprite = task.Result;
                };
            }
            else
            {
                Debug.LogError($"[{building.ID}] Sprite경로:[Sprite/{building.ID}.png] Addressable주소에 해당 건물의 Sprite가 없습니다");
            }


            if (building is ManufactureBD manufacture)
            {
                manufacture.RequireProdID = words[dict["RequireProdID"]];
                int.TryParse(words[dict["RequireProdCount"]], out manufacture.RequireProdCount);
                float.TryParse(words[dict["PrepareTimeRate"]], out manufacture.PrepareTimeRate);

                List<int> Stat_Capacity_List
                = words[dict["Stat_Capacity"]].Trim('{', '}').Split(',').Select(s => int.Parse(s.Trim())).ToList();
                List<float> Stat_CapacityCost_List
                = words[dict["Stat_CapacityCost"]].Trim('{', '}').Split(',').Select(s => float.Parse(s.Trim())).ToList();
                manufacture.Stat_Capacity = new()
                {
                    Values = Stat_Capacity_List,
                    Cost = Stat_CapacityCost_List,
                    MaxLevel = Stat_CapacityCost_List.Count
                };
            }

            return building;
        });
        Building.Load(dataCsv);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeCostDataCsv : IUsableId
{
    public string Lv;

    public int Speed;
    public int Capacity;
    public int Nego;

    public string GetId()
    {
        return Lv;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isPlayerUpgradeCostAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _playerUpgradeCostTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _playerUpgradeCostAdress = "PlayerUpgradeCostCsv";

    public DataTableParser<PlayerUpgradeCostDataCsv> PlayerUpgradeCost;
    private async void PlayerUpgradeCostRoutine()
    {
        string dataCsv;

        if (_isPlayerUpgradeCostAdressable)
        {
            dataCsv = await GetDataString(_isPlayerUpgradeCostAdressable, _playerUpgradeCostAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isPlayerUpgradeCostAdressable, _playerUpgradeCostTableURL);
        }

        PlayerUpgradeCost = new DataTableParser<PlayerUpgradeCostDataCsv>((words, dict) =>
        {
            PlayerUpgradeCostDataCsv cost = new PlayerUpgradeCostDataCsv();

            cost.Lv = words[dict["LV"]];

            int.TryParse(words[dict["SpeedValue"]], out cost.Speed);
            int.TryParse(words[dict["CapacityValue"]], out cost.Capacity);
            int.TryParse(words[dict["NegoValue"]], out cost.Nego);

            return cost;
        });

        PlayerUpgradeCost.Load(dataCsv);
    }
}

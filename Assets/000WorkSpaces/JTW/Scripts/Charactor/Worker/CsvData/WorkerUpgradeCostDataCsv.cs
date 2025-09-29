using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerUpgradeCostDataCsv : IUsableId
{
    public string Id;

    public string Lv;
    public int Rank;

    public int Speed;
    public int Capacity;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isWorkerUpgradeCostAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _workerUpgradeCostDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _workerUpgradeCostAdress = "WorkerUpgradeCostCsv";

    public DataTableParser<WorkerUpgradeCostDataCsv> WorkerUpgradeCost;
    private async void WorkerUpgradeCostRoutine()
    {
        string dataCsv;

        if (_isWorkerUpgradeCostAdressable)
        {
            dataCsv = await GetDataString(_isWorkerUpgradeCostAdressable, _workerUpgradeCostAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isWorkerUpgradeCostAdressable, _workerUpgradeCostDataTableURL);
        }

        WorkerUpgradeCost = new DataTableParser<WorkerUpgradeCostDataCsv>((words, dict) =>
        {
            WorkerUpgradeCostDataCsv cost = new WorkerUpgradeCostDataCsv();

            cost.Id = $"{words[dict["LV"]]}_{words[dict["CharacterRank"]]}";

            cost.Lv = words[dict["LV"]];
            int.TryParse(words[dict["CharacterRank"]], out cost.Rank);

            int.TryParse(words[dict["SpeedBaseCost"]], out cost.Speed);
            int.TryParse(words[dict["CapacityBaseCost"]], out cost.Capacity);

            return cost;
        });

        WorkerUpgradeCost.Load(dataCsv);
    }
}

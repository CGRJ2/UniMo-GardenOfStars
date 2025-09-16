using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerEmployCostDataCsv : IUsableId
{
    public string Id;

    public int Cost;

    public int BMCost;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isWorkerEmployCostAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _workerEmployCostDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _workerEmployCostAdress = "WorkerEmployCostCsv";

    public DataTableParser<WorkerEmployCostDataCsv> WorkerEmployCost;
    private async void WorkerEmployCostRoutine()
    {
        string dataCsv;

        if (_isWorkerEmployCostAdressable)
        {
            dataCsv = await GetDataString(_isWorkerEmployCostAdressable, _workerEmployCostAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isWorkerEmployCostAdressable, _workerEmployCostDataTableURL);
        }

        WorkerEmployCost = new DataTableParser<WorkerEmployCostDataCsv>((words, dict) =>
        {
            WorkerEmployCostDataCsv cost = new WorkerEmployCostDataCsv();

            cost.Id = words[dict["CharacterID"]];
            int.TryParse(words[dict["Cost"]], out cost.Cost);
            int.TryParse(words[dict["BMCost"]], out cost.BMCost);

            return cost;
        });

        WorkerEmployCost.Load(dataCsv);
    }
}

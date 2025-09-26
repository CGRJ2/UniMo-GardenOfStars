using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeMultiDataCsv : IUsableId
{
    public string Id;
    public float Multi;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isUpgradeMultiAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _upgradeMultiDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _upgradeMultiAdress = "UpgradeMultiCsv";

    public DataTableParser<UpgradeMultiDataCsv> UpgradeMulti;
    private async void UpgradeMultiRoutine()
    {
        string dataCsv;

        if (_isUpgradeMultiAdressable)
        {
            dataCsv = await GetDataString(_isUpgradeMultiAdressable, _upgradeMultiAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isUpgradeMultiAdressable, _upgradeMultiDataTableURL);
        }

        UpgradeMulti = new DataTableParser<UpgradeMultiDataCsv>((words, dict) =>
        {
            UpgradeMultiDataCsv multi = new UpgradeMultiDataCsv();

            multi.Id = words[dict["StageID"]];

            float.TryParse(words[dict["UpgradeMultiplier"]], out multi.Multi);

            return multi;
        });

        UpgradeMulti.Load(dataCsv);
    }
}

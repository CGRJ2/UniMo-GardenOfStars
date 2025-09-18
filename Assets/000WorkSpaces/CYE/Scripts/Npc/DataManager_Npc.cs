using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using GameQuest;
using System;

public class NpcDataCsv : IUsableId
{
    public string Id;
    public string NpcName;
    public string StageId;
    public string NpcImageLocation;
    public string[] FocusText;
    public string Description;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isNpcAdressable = true;

    private const string _npcDataTableURL = "https://docs.google.com/spreadsheets/d/1MhKkfv6aIljgeisYmWOvPiUNaa8x4Ydqii7pZkyk4yk/export?format=csv";
    private const string _npcAddress = "CYE/SampleNpcData";

    public DataTableParser<NpcDataCsv> Npc;

    public async void NpcRoutine()
    {
        string rawData = await GetDataString((_isNpcAdressable), (_isNpcAdressable) ? _npcAddress : _npcDataTableURL);
        if (rawData != null)
        {
            Npc = new DataTableParser<NpcDataCsv>((words, dict) =>
            {
                NpcDataCsv npc = new NpcDataCsv();

                npc.Id = words[dict["Id"]];
                npc.NpcName = words[dict["NpcName"]];
                npc.StageId = words[dict["StageId"]];
                npc.NpcImageLocation = words[dict["NpcImageLocation"]];
                npc.FocusText = ConvertTextToArray(words[dict["FocusText"]]);
                npc.Description = words[dict["Description"]];

                return npc;
            });

            Npc.Load(rawData);
        }
    }

    private string[] ConvertTextToArray(string rawText)
    {
        return rawText.Split("@@@");
    }
}
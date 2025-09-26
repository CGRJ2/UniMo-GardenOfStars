using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class NpcDataCsv : IUsableId
{
    public string NpcID;
    public string Name_KR;
    public string Name_EN;
    public Sprite Sprite_Default;
    public string Description;

    public string GetId()
    {
        return NpcID;
    }
}

public class NpcTextLineDataCsv : IUsableId
{
    public string TextLineID;
    public string NpcId;
    public string TextLine_KR;
    public string TextLine_EN;
    public string GetId()
    {
        return TextLineID;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isNpcAdressable = true;
    [SerializeField] private bool _isNpcTextLinesAdressable = true;

    private const string _npcDataTableURL = "https://docs.google.com/spreadsheets/d/1MhKkfv6aIljgeisYmWOvPiUNaa8x4Ydqii7pZkyk4yk/export?format=csv";
    private const string _npcAddress = "CYE/SampleNpcData";

    private const string _npcTextLinesDataTableURL = "https://docs.google.com/spreadsheets/d/1uLuoyuNNRqDrVu9tCOZ9Q0qMntUPH38xkJcX4Bjcu_k/export?format=csv";
    private const string _npcTextLinesaddress = "CYE/SampleNpcTextLinesData";

    public DataTableParser<NpcDataCsv> Npc;
    //public DataTableParser<NpcTextLineDataCsv> NpcTextLines;

    public async void NpcRoutine()
    {
        string rawData = await GetDataString(_isNpcAdressable, _isNpcAdressable ? _npcAddress : _npcDataTableURL);
        if (rawData != null)
        {
            Npc = new DataTableParser<NpcDataCsv>((words, dict) =>
            {
                NpcDataCsv npc = new NpcDataCsv();

                npc.NpcID = words[dict["NpcID"]];
                npc.Name_KR = words[dict["Name_KR"]];
                npc.Name_EN = words[dict["Name_EN"]];
                npc.Description = words[dict["Description"]];

                string spritePath_Default = words[dict["SpritePath_Default"]];
                if (Addressables.ResourceLocators.Any(locator => locator.Locate($"{spritePath_Default}", typeof(Sprite), out var locations)))
                {
                    Addressables.LoadAssetAsync<Sprite>($"{spritePath_Default}").Completed += task =>
                    {
                        npc.Sprite_Default = task.Result;
                    };
                }
                else
                {
                    Debug.LogError($"[{npc.NpcID}]의 키:[{spritePath_Default}] 에 해당하는 스프라이트 없음");
                }
                return npc;
            });

            Npc.Load(rawData);
        }
    }
    /*public async void NpcTextLinesRoutine()
    {
        string rawData = await GetDataString(_isNpcTextLinesAdressable, _isNpcTextLinesAdressable ? _npcTextLinesaddress : _npcTextLinesDataTableURL);
        if (rawData != null)
        {
            NpcTextLines = new DataTableParser<NpcTextLineDataCsv>((words, dict) =>
            {
                NpcTextLineDataCsv npcTextLines = new NpcTextLineDataCsv();

                npcTextLines.TextLineID = words[dict["TextLineID"]];
                npcTextLines.NpcId = words[dict["NpcId"]];
                npcTextLines.TextLine_KR = words[dict["TextLine_KR"]];
                npcTextLines.TextLine_EN = words[dict["TextLine_EN"]];

                return npcTextLines;
            });

            NpcTextLines.Load(rawData);
        }
    }*/
}
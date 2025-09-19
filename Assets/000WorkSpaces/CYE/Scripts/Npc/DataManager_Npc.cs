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

public partial class DataManager
{
    [SerializeField] private bool _isNpcAdressable;

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
}
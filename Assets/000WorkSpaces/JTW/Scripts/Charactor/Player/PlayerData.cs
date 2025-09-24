using System.Collections.Generic;
using UnityEngine;

public class PlayerData : FirebaseData
{
    private Dictionary<string, CharacterLvDataCsv> LvCsv => Manager.data.CharacterLv.Values;
    private PlayerDataCsv PlayerCsv => Manager.data.Player.Values["201"];

    public float MoveSpeed => LvCsv[(MoveSpeedLv.Value + 2).ToString()].Speed;
    public FirebaseProperty<int> MoveSpeedLv;
    public int MoveSpeedMaxLv => PlayerCsv.MaxSpeedMaxLv;
    public bool IsMoveSpeedMaxLv => MoveSpeedLv.Value >= MoveSpeedMaxLv;

    public int MaxCapacity => LvCsv[(MaxCapacityLv.Value).ToString()].Capacity;
    public FirebaseProperty<int> MaxCapacityLv;
    public int MaxCapacityMaxLv => PlayerCsv.MaxCapacityMaxLv;
    public bool IsMaxCapacityMaxLv => MaxCapacityLv.Value >= MaxCapacityMaxLv;

    public float ProductionSpeed => PlayerCsv.PDSpeed;

    public float Nego => LvCsv[NegoLv.Value.ToString()].Nego;
    public FirebaseProperty<int> NegoLv;
    public int NegoMaxLv => PlayerCsv.NegoMaxLv;
    public bool IsNegoMaxLv => NegoLv.Value >= NegoMaxLv;

    public FirebaseProperty<int> Money;
    public FirebaseProperty<int> Gem;

    public FirebaseProperty<string> CharacterSkinId;
    public FirebaseProperty<string> EquipSkinId;

    public PlayerData(string id, string parentPath) : base(id, parentPath)
    {
        MoveSpeedLv = new FirebaseProperty<int>("MoveSpeedLv", Path, 1);

        MaxCapacityLv = new FirebaseProperty<int>("MaxCapacityLv", Path, 1);

        NegoLv = new FirebaseProperty<int>("NegoLv", Path, 1);

        Money = new FirebaseProperty<int>("Money", Path, 0, true);

        Gem = new FirebaseProperty<int>("Gem", Path, 0);

        CharacterSkinId = new FirebaseProperty<string>("CharacterSkinId", Path, "101");
        EquipSkinId = new FirebaseProperty<string>("EquipSkinId", Path, "201");

        InitList.Add(MoveSpeedLv);
        InitList.Add(MaxCapacityLv);
        InitList.Add(NegoLv);
        InitList.Add(Money);
        InitList.Add(Gem);
        InitList.Add(CharacterSkinId);
        InitList.Add(EquipSkinId);
    }

    public void UpgradeMoveSpeed()
    {
        if (!IsMoveSpeedMaxLv)
        {
            MoveSpeedLv.Value += 1;
        }
    }

    public void UpgradeMaxCapacity()
    {
        if (!IsMaxCapacityMaxLv)
        {
            MaxCapacityLv.Value += 1;
        }
    }

    public void UpgradeNego()
    {
        if (!IsNegoMaxLv)
        {
            NegoLv.Value += 1;
        }
    }
}

public class PlayerDataCsv : IUsableId
{
    public string Id;

    public float BMSpeed;
    public int MaxSpeedMaxLv;

    public int BMCapacity;
    public int MaxCapacityMaxLv;

    public int NegoMaxLv;

    public float PDSpeed;

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    private bool _isPlayerAdressable = true;

    // 구글 스프레드 시트 다운로드 주소
    private const string _playerDataTableURL = "https://docs.google.com/spreadsheets/d/1CwrcyyODjYAwjCgYkofKQl815o-vOWkUH7yy6mdUtY4/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _playerAdress = "PlayerCsv";

    public DataTableParser<PlayerDataCsv> Player;
    private async void PlayerRoutine()
    {
        string dataCsv;

        if (_isPlayerAdressable)
        {
            dataCsv = await GetDataString(_isPlayerAdressable, _playerAdress);
        }
        else
        {
            dataCsv = await GetDataString(_isPlayerAdressable, _playerDataTableURL);
        }

        Player = new DataTableParser<PlayerDataCsv>((words, dict) =>
        {
            PlayerDataCsv player = new PlayerDataCsv();

            player.Id = words[dict["CharacterFixedID"]];

            float.TryParse(words[dict["BMSpeed"]], out player.BMSpeed);
            int.TryParse(words[dict["MaxSpeedLV"]], out player.MaxSpeedMaxLv);

            int.TryParse(words[dict["BMCapacity"]], out player.BMCapacity);
            int.TryParse(words[dict["MaxCapacityLV"]], out player.MaxCapacityMaxLv);

            int.TryParse(words[dict["MaxNegoLV"]], out player.NegoMaxLv);

            float.TryParse(words[dict["PDSpeed"]], out player.PDSpeed);

            return player;
        });

        Player.Load(dataCsv);
    }
}

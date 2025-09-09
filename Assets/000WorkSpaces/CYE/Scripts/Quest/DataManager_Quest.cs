using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using GameQuest;

public class QuestDataCsv : IUsableId
{
    public string Id;
    // public string QuestId;
    public string NpcId;
    public int QuestOrder;
    public string QuestName;
    public QuestType QuestType;
    public string Description;

    public string GetId()
    {
        return Id;
    }
}

public class QuestContentDataCsv : IUsableId
{
    public string Id;
    public string QuestId;
    public string ContentTargetId;
    public int ContentTargetCount;

    public string GetId()
    {
        return Id;
    }
}

public class QuestUserDataJson : IUsableId
{
    public string Id;
    public string QuestId;
    public string UserId;
    public int QuestState; // QuestState

    public string GetId()
    {
        return Id;
    }
}

public class QuestUserProgressDataJson : IUsableId
{
    public string Id;
    public int QuestId;
    public string UserId;
    public string ContentTargetId;
    public int ContentTargetCount;
    public int ProgressCount;
    public int ProgressState; // QuestProgressState

    public string GetId()
    {
        return Id;
    }
}

public partial class DataManager
{
    [SerializeField] private bool _isQuestAdressable;
    [SerializeField] private bool _isQuestContentAdressable;

    private const string _questDataTableURL = "https://docs.google.com/spreadsheets/d/14olmAKBDTc8EEL4fDtRC5j_vtVXu5ZZ3EXckaP9uGo8/export?format=csv";
    private const string _questAddress = "CYE/SampleQuestData";

    private const string _questContentDataTableURL = "https://docs.google.com/spreadsheets/d/1rggi6yeem8h4WlM2IUpQ9oGQ_Cm8G6ZHFX4RiCsXtGA/export?format=csv";
    private const string _questContentAddress = "CYE/SampleQuestContentData";

    public DataTableParser<QuestDataCsv> Quest;
    public DataTableParser<QuestContentDataCsv> QuestContent;

    public async void QuestRoutine()
    {
        string rawData = await GetDataString((_isQuestAdressable), (_isQuestAdressable) ? _questAddress : _questDataTableURL);
        if (rawData != null)
        {
            Quest = new DataTableParser<QuestDataCsv>((words, dict) =>
            {
                QuestDataCsv quest = new QuestDataCsv();

                quest.Id = words[dict["Id"]];
                quest.NpcId = words[dict["NpcId"]];
                int.TryParse(words[dict["QuestOrder"]], out quest.QuestOrder);
                quest.QuestName = words[dict["QuestName"]];
                int.TryParse(words[dict["QuestType"]], out int tempQuestType);
                quest.QuestType = (GameQuest.QuestType)tempQuestType;
                quest.Description = words[dict["Description"]];

                return quest;
            });

            Quest.Load(rawData);
        }
    }
    
    public async void QuestContentRoutine()
    {
        string rawData = await GetDataString((_isQuestContentAdressable), (_isQuestContentAdressable) ? _questContentAddress : _questContentDataTableURL);
        if (rawData != null)
        {
            QuestContent = new DataTableParser<QuestContentDataCsv>((words, dict) =>
            {
                QuestContentDataCsv questContent = new QuestContentDataCsv();

                questContent.Id = words[dict["Id"]];
                questContent.QuestId = words[dict["QuestId"]];
                questContent.ContentTargetId = words[dict["ContentTargetId"]];
                int.TryParse(words[dict["ContentTargetCount"]], out questContent.ContentTargetCount);

                return questContent;
            });

            QuestContent.Load(rawData);
        }
    }
}
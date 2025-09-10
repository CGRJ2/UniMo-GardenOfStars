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

public class QuestRewardDataCsv : IUsableId
{
    public string Id;
    public string QuestId;
    public string RewardId;
    public int RewardAmount;

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
    //public int ContentTargetCount;

    public string GetId()
    {
        return Id;
    }
}

public class QuestContentStepDataCsv : IUsableId
{
    public string Id;
    public string QuestContentId;
    public int TargetAmount;
    public string RewardId;
    public int RewardAmount;
    public int ContentOrder;
    public string GetId()
    {
        return Id;
    }
}

/*public class QuestUserDataJson : IUsableId
{
    public string Id;
    public string QuestId;
    public string UserId;
    public int QuestState; // QuestState

    public string GetId()
    {
        return Id;
    }
}*/

/*public class QuestUserProgressDataJson : IUsableId
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
}*/

public partial class DataManager
{
    [SerializeField] private bool _isQuestAdressable = true;
    [SerializeField] private bool _isQuestRewardAdressable = true;
    [SerializeField] private bool _isQuestContentAdressable = true;
    [SerializeField] private bool _isQuestContentStepAdressable = true;

    private const string _questDataTableURL = "https://docs.google.com/spreadsheets/d/14olmAKBDTc8EEL4fDtRC5j_vtVXu5ZZ3EXckaP9uGo8/export?format=csv";
    private const string _questAddress = "CYE/SampleQuestData";
    
    private const string _questRewardDataTableURL = "https://docs.google.com/spreadsheets/d/1rggi6yeem8h4WlM2IUpQ9oGQ_Cm8G6ZHFX4RiCsXtGA/export?format=csv";
    private const string _questRewardAddress = "CYE/SampleQuestRewardData";

    private const string _questContentDataTableURL = "https://docs.google.com/spreadsheets/d/1rggi6yeem8h4WlM2IUpQ9oGQ_Cm8G6ZHFX4RiCsXtGA/export?format=csv";
    private const string _questContentAddress = "CYE/SampleQuestContentData";
    
    private const string _questContentStepDataTableURL = "https://docs.google.com/spreadsheets/d/1rggi6yeem8h4WlM2IUpQ9oGQ_Cm8G6ZHFX4RiCsXtGA/export?format=csv";
    private const string _questContentStepAddress = "CYE/SampleQuestContentStepData";

    public DataTableParser<QuestDataCsv> Quest;
    public DataTableParser<QuestRewardDataCsv> QuestReward;
    public DataTableParser<QuestContentDataCsv> QuestContent;
    public DataTableParser<QuestContentStepDataCsv> QuestContentStep;

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

    public async void QuestRewardRoutine()
    {
        string rawData = await GetDataString((_isQuestRewardAdressable), (_isQuestRewardAdressable) ? _questRewardAddress : _questRewardDataTableURL);
        if (rawData != null)
        {
            QuestReward = new DataTableParser<QuestRewardDataCsv>((words, dict) =>
            {
                QuestRewardDataCsv questReward = new QuestRewardDataCsv();

                questReward.Id = words[dict["Id"]];
                questReward.QuestId = words[dict["QuestId"]];
                questReward.RewardId = words[dict["RewardId"]];
                int.TryParse(words[dict["RewardAmount"]], out questReward.RewardAmount);

                return questReward;
            });

            QuestReward.Load(rawData);
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

                return questContent;
            });

            QuestContent.Load(rawData);
        }
    }

    public async void QuestContentStepRoutine()
    {
        string rawData = await GetDataString((_isQuestContentStepAdressable), (_isQuestContentStepAdressable) ? _questContentStepAddress : _questContentStepDataTableURL);
        if (rawData != null)
        {
            QuestContentStep = new DataTableParser<QuestContentStepDataCsv>((words, dict) =>
            {
                QuestContentStepDataCsv questContentStep = new QuestContentStepDataCsv();

                questContentStep.Id = words[dict["Id"]];
                questContentStep.QuestContentId = words[dict["QuestContentId"]];
                int.TryParse(words[dict["TargetAmount"]], out questContentStep.TargetAmount);
                questContentStep.RewardId = words[dict["RewardId"]];
                int.TryParse(words[dict["RewardAmount"]], out questContentStep.RewardAmount);
                int.TryParse(words[dict["ContentOrder"]], out questContentStep.ContentOrder);

                return questContentStep;
            });

            QuestContentStep.Load(rawData);
        }
    }
}
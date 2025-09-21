using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using GameQuest;

public class QuestDataCsv : IUsableId
{
    public string Id;
    public string NpcId;
    public int QuestOrder;
    public string QuestName;
    public QuestType QuestType;
    public string RewardId;
    public int RewardAmount;
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
    public ContentStep[] ContentSteps = new ContentStep[5];

    public string GetId()
    {
        return Id;
    }
}

public struct ContentStep
{
    public int TargetAmount;
    public string RewardId;
    public int RewardAmount;
    public ContentStep(int targetAmount, string rewardId, int rewardAmount)
    {
        this.TargetAmount = targetAmount;
        this.RewardId = rewardId;
        this.RewardAmount = rewardAmount;
    }
    public bool IsEmpty()
    {
        return (TargetAmount == default && RewardId == default && RewardAmount == default) || (TargetAmount == 0 && RewardId == "" && RewardAmount == 0);
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

public partial class DataManager
{
    [SerializeField] private bool _isQuestAdressable = true;
    // [SerializeField] private bool _isQuestRewardAdressable = true;
    [SerializeField] private bool _isQuestContentAdressable = true;
    // [SerializeField] private bool _isQuestContentStepAdressable = true;

    private const string _questDataTableURL = "https://docs.google.com/spreadsheets/d/14olmAKBDTc8EEL4fDtRC5j_vtVXu5ZZ3EXckaP9uGo8/export?format=csv";
    private const string _questAddress = "CYE/SampleQuestData";
    
    // private const string _questRewardDataTableURL = "https://docs.google.com/spreadsheets/d/1rggi6yeem8h4WlM2IUpQ9oGQ_Cm8G6ZHFX4RiCsXtGA/export?format=csv";
    // private const string _questRewardAddress = "CYE/SampleQuestRewardData";

    private const string _questContentDataTableURL = "https://docs.google.com/spreadsheets/d/1rggi6yeem8h4WlM2IUpQ9oGQ_Cm8G6ZHFX4RiCsXtGA/export?format=csv";
    private const string _questContentAddress = "CYE/SampleQuestContentData";
    
    // private const string _questContentStepDataTableURL = "https://docs.google.com/spreadsheets/d/1rggi6yeem8h4WlM2IUpQ9oGQ_Cm8G6ZHFX4RiCsXtGA/export?format=csv";
    // private const string _questContentStepAddress = "CYE/SampleQuestContentStepData";

    public DataTableParser<QuestDataCsv> Quest;
    // public DataTableParser<QuestRewardDataCsv> QuestReward;
    public DataTableParser<QuestContentDataCsv> QuestContent;
    // public DataTableParser<QuestContentStepDataCsv> QuestContentStep;

    public async void QuestRoutine()
    {
        string rawData = await GetDataString(_isQuestAdressable, _isQuestAdressable ? _questAddress : _questDataTableURL);
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
                quest.RewardId = words[dict["RewardId"]];
                int.TryParse(words[dict["RewardAmount"]], out quest.RewardAmount);
                quest.Description = words[dict["Description"]];

                return quest;
            });

            Quest.Load(rawData);
        }
    }

    // public async void QuestRewardRoutine()
    // {
    //     string rawData = await GetDataString(_isQuestRewardAdressable, _isQuestRewardAdressable ? _questRewardAddress : _questRewardDataTableURL);
    //     if (rawData != null)
    //     {
    //         QuestReward = new DataTableParser<QuestRewardDataCsv>((words, dict) =>
    //         {
    //             QuestRewardDataCsv questReward = new QuestRewardDataCsv();

    //             questReward.Id = words[dict["Id"]];
    //             questReward.QuestId = words[dict["QuestId"]];
    //             questReward.RewardId = words[dict["RewardId"]];
    //             int.TryParse(words[dict["RewardAmount"]], out questReward.RewardAmount);

    //             return questReward;
    //         });

    //         QuestReward.Load(rawData);
    //     }
    // }

    public async void QuestContentRoutine()
    {
        string rawData = await GetDataString(_isQuestContentAdressable, _isQuestContentAdressable ? _questContentAddress : _questContentDataTableURL);
        if (rawData != null)
        {
            QuestContent = new DataTableParser<QuestContentDataCsv>((words, dict) =>
            {
                QuestContentDataCsv questContent = new QuestContentDataCsv();

                questContent.Id = words[dict["Id"]];
                questContent.QuestId = words[dict["QuestId"]];
                questContent.ContentTargetId = words[dict["ContentTargetId"]];
                
                int.TryParse(words[dict["TargetAmount01"]], out int targetAmount01);
                string rewardId01 = words[dict["RewardId01"]];
                int.TryParse(words[dict["RewardAmount01"]], out int rewardAmount01);
                questContent.ContentSteps[0] = new ContentStep(targetAmount01, rewardId01, rewardAmount01);
                
                int.TryParse(words[dict["TargetAmount02"]], out int targetAmount02);
                string rewardId02 = words[dict["RewardId02"]];
                int.TryParse(words[dict["RewardAmount02"]], out int rewardAmount02);
                questContent.ContentSteps[1] = new ContentStep(targetAmount02, rewardId02, rewardAmount02);
                
                int.TryParse(words[dict["TargetAmount03"]], out int targetAmount03);
                string rewardId03 = words[dict["RewardId03"]];
                int.TryParse(words[dict["RewardAmount03"]], out int rewardAmount03);
                questContent.ContentSteps[2] = new ContentStep(targetAmount03, rewardId03, rewardAmount03);
                
                int.TryParse(words[dict["TargetAmount04"]], out int targetAmount04);
                string rewardId04 = words[dict["RewardId04"]];
                int.TryParse(words[dict["RewardAmount04"]], out int rewardAmount04);
                questContent.ContentSteps[3] = new ContentStep(targetAmount04, rewardId04, rewardAmount04);
                
                int.TryParse(words[dict["TargetAmount05"]], out int targetAmount05);
                string rewardId05 = words[dict["RewardId05"]];
                int.TryParse(words[dict["RewardAmount05"]], out int rewardAmount05);
                questContent.ContentSteps[4] = new ContentStep(targetAmount05, rewardId05, rewardAmount05);

                return questContent;
            });

            QuestContent.Load(rawData);
        }
    }

    // public async void QuestContentStepRoutine()
    // {
    //     string rawData = await GetDataString(_isQuestContentStepAdressable, _isQuestContentStepAdressable ? _questContentStepAddress : _questContentStepDataTableURL);
    //     if (rawData != null)
    //     {
    //         QuestContentStep = new DataTableParser<QuestContentStepDataCsv>((words, dict) =>
    //         {
    //             QuestContentStepDataCsv questContentStep = new QuestContentStepDataCsv();

    //             questContentStep.Id = words[dict["Id"]];
    //             questContentStep.QuestContentId = words[dict["QuestContentId"]];
    //             int.TryParse(words[dict["TargetAmount"]], out questContentStep.TargetAmount);
    //             questContentStep.RewardId = words[dict["RewardId"]];
    //             int.TryParse(words[dict["RewardAmount"]], out questContentStep.RewardAmount);
    //             int.TryParse(words[dict["ContentOrder"]], out questContentStep.ContentOrder);

    //             return questContentStep;
    //         });

    //         QuestContentStep.Load(rawData);
    //     }
    // }
}
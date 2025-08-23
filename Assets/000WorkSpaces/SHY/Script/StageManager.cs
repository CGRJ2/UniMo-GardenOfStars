using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class StageManager : Singleton<StageManager>,IQuestObserver
{
    //public static StageManager instance;
    [Header("So �׷���")]
    public StageData[] originalStageSOs;
    public SceneChanger sceneChanger;

    private List<StageData> activeStageInstances = new(); // ������ ����Ʈ

    private List<bool> loadedData = new List<bool>();
    private string savePath;
    

    public List<StageData> GetStages()
    {
        return activeStageInstances;
    }
    /*������*************************************************/
    public void OnQuestProgressChanged(int stageIndex, int newProgress)
    {
        if (stageIndex < 0 || stageIndex >= activeStageInstances.Count) return;

        var stage = activeStageInstances[stageIndex];
        
        //stage.StageClear = newProgress >= stage.condition;
        stage.init(newProgress);

        TryUnlockNextStage(stageIndex);
    }
    void OnEnable()
    {
        QuestEventBus.OnQuestProgressChanged += OnQuestProgressChanged;
    }

    void OnDisable()
    {
        QuestEventBus.OnQuestProgressChanged -= OnQuestProgressChanged;
    }


    private void Awake()
    {
        base.SingletonInit();
        //if (instance == null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}
        savePath = Path.Combine(Application.persistentDataPath, "SaveFiles", "stageData.json");
    }
    private void Start()
    {
        InitializeStages();
    }

    public void InitializeStages()
    {
        activeStageInstances.Clear();

        for (int i = 0; i < originalStageSOs.Length; i++)
        {
            StageData instance = Instantiate(originalStageSOs[i]); // ������ ����
                                                                   //���̺� ������ �ҷ��ý� �ڵ� �߰�����
            activeStageInstances.Add(instance);
        }
        LoadStageData(); // �ε� ��� �߰�

    }
    public void TryUnlockNextStage(int currentStageIndex)
    {
        if (currentStageIndex + 1 >= activeStageInstances.Count) return;

        var currentStage = activeStageInstances[currentStageIndex];
        var nextStage = activeStageInstances[currentStageIndex + 1];
        if (currentStage.StageClear && !nextStage.Unlock)
        {
            
            nextStage.Unlock = true;
            Debug.Log($"Stage {nextStage.StageId} �رݵ�");

            var buttons = FindObjectsOfType<StageButton>();
            foreach (var btn in buttons)
            {
                if (btn.stageIndex == currentStageIndex + 1)
                {
                    btn.UpdateVisual();
                    break;
                }
            }

        }

    }

    public void UnlockStage(int stageIndex) // ���ΰ�ħ�� �������� �������� �Ⱦ���.
    {
        if (stageIndex < 0 || stageIndex >= activeStageInstances.Count) return;

        activeStageInstances[stageIndex].Unlock = true;
    }
    public List<bool> GetUnlockStates()
    {
        List<bool> unlocks = new();
        foreach (var stage in activeStageInstances)
            unlocks.Add(stage.Unlock);

        return unlocks;
    }
    public void GetUnlockStates2()
    {
        loadedData.Clear();
        foreach (var stage in activeStageInstances)
            loadedData.Add(stage.Unlock);
    }

    // ������ so��ü�ε� ����. ��������� ���� �ر�üũ���� ����Ʈ�� ����.
    //�������� �Ŵ����� ui�� Ȱ��ȭ ���� ����?
    public void SaveStageData()
    {
        SaveData data = new SaveData();
        foreach (var stage in activeStageInstances)
        {
            data.unlockStates.Add(stage.Unlock); //���� �ʿ�� �������� ������ ����.
            data.questRates.Add(stage.QuestRate);
            data.stageClears.Add(stage.StageClear);
        }

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
    }

    public void LoadStageData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            for (int i = 0; i < activeStageInstances.Count && i < data.unlockStates.Count; i++)
            {
                //activeStageInstances[i].Unlock = data.unlockStates[i]; //�رݻ��� �ҷ����� �ʿ��������.
                activeStageInstances[i].QuestRate = data.questRates[i];
                activeStageInstances[i].StageClear = data.stageClears[i];

                activeStageInstances[i].init();
            }
        }
    }



}


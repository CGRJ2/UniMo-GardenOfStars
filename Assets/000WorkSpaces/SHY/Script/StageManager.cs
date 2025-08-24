using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class StageManager : Singleton<StageManager>,IQuestObserver
{
    //public static StageManager instance;
    [Header("So 그룹등록")]
    public StageData[] originalStageSOs;
    public SceneChanger sceneChanger;

    private List<StageData> activeStageInstances = new(); // 복제본 리스트

    private List<bool> loadedData = new List<bool>();
    private string savePath;
    

    public List<StageData> GetStages()
    {
        return activeStageInstances;
    }
    /*옵저버*************************************************/
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
            StageData instance = Instantiate(originalStageSOs[i]); // 복제본 생성
                                                                   //세이브 데이터 불러올시 코드 추가영역
            activeStageInstances.Add(instance);
        }
        LoadStageData(); // 로드 기능 추가

    }
    public void TryUnlockNextStage(int currentStageIndex)
    {
        if (currentStageIndex + 1 >= activeStageInstances.Count) return;

        var currentStage = activeStageInstances[currentStageIndex];
        var nextStage = activeStageInstances[currentStageIndex + 1];
        if (currentStage.StageClear && !nextStage.Unlock)
        {
            
            nextStage.Unlock = true;
            Debug.Log($"Stage {nextStage.StageId} 해금됨");

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

    public void UnlockStage(int stageIndex) // 새로고침된 언락기능이 있음으로 안쓰임.
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

    // 생성된 so자체로도 가능. 어떤게편할지 몰라 해금체크전용 리스트도 만듬.
    //스테이지 매니저가 ui가 활성화 여부 관리?
    public void SaveStageData()
    {
        SaveData data = new SaveData();
        foreach (var stage in activeStageInstances)
        {
            data.unlockStates.Add(stage.Unlock); //저장 필요는 없을지도 논의후 결정.
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
                //activeStageInstances[i].Unlock = data.unlockStates[i]; //해금상태 불러오기 필요없을지도.
                activeStageInstances[i].QuestRate = data.questRates[i];
                activeStageInstances[i].StageClear = data.stageClears[i];

                activeStageInstances[i].init();
            }
        }
    }



}


using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    [Header("So 그룹등록")]
    public StageData[] originalStageSOs;

    private List<StageData> activeStageInstances = new(); // 복제본 리스트

    private List<bool> loadedData = new List<bool>();


    private void Start()
    {
        if(instance == null)
        {
            instance = this;

        }
        DontDestroyOnLoad(gameObject);

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
        
    }
    public void UnlockStage(int stageIndex)
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
   
}


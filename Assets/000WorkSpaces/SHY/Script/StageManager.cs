using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    [Header("So �׷���")]
    public StageData[] originalStageSOs;

    private List<StageData> activeStageInstances = new(); // ������ ����Ʈ

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
            StageData instance = Instantiate(originalStageSOs[i]); // ������ ����
            //���̺� ������ �ҷ��ý� �ڵ� �߰�����
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

    // ������ so��ü�ε� ����. ��������� ���� �ر�üũ���� ����Ʈ�� ����.
    //�������� �Ŵ����� ui�� Ȱ��ȭ ���� ����?
   
}


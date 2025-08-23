using UnityEngine;
using TMPro;

public class StageButtonSpawner : MonoBehaviour
{
    [Header("버튼 프리팹")]
    public GameObject stageButtonPrefab;

    [Header("버튼 생성 위치 부모")]
    public Transform buttonParent;

    void Start() //초기화 구성 오류 업데이트에서 진행.
    {
        SpawnStageButtons();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnStageButtons();
        }
    }
    void SpawnStageButtons()
    {
        
        var stages = StageManager.instance.GetStages();
        Debug.Log($"{stages.Count}");
        for (int i = 0; i < stages.Count; i++)
        {
            
            GameObject buttonObj = Instantiate(stageButtonPrefab, buttonParent);
            StageButton button = buttonObj.GetComponent<StageButton>();

            if (button != null)
            {
                button.stageIndex = i;
                button.Initialize(stages[i]);
            }
        }
    }
}

using UnityEngine;
using TMPro;

public class StageButtonSpawner : MonoBehaviour
{
    [Header("��ư ������")]
    public GameObject stageButtonPrefab;

    [Header("��ư ���� ��ġ �θ�")]
    public Transform buttonParent;

    void Start() //�ʱ�ȭ ���� ���� ������Ʈ���� ����.
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

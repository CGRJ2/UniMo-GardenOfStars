using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;


public class StageButton : MonoBehaviour
{
    [Header("�� ��ư�� ����ϴ� �������� �ε���")]
    public int stageIndex;
    

    [Header("UI ���")]
    public TextMeshProUGUI label;

    private StageData stageData;

    void Start()  //������ �Ⱦ���
    {
        // StageManager���� �ش� �������� ������ ��������
        stageData = Manager.stage.GetStages()[stageIndex];

        UpdateVisual();
    }
    public void Initialize(StageData data) //������ȭ�Ȱ� ����.
    {
        stageData = data;
        UpdateVisual();
    }


    public void OnClick()
    {
        if (!stageData.Unlock)
        {
            Debug.Log($"{stageData.name}�� ��� ���������Դϴ�.");
            return;
        }

        //string currentScene = SceneManager.GetActiveScene().name;
        //string targetScene = stageData.StageId.ToString(); // �ܹ���
        //string targetScene = stageData.StageName;

        if (Manager.stage.sceneChanger.CurSceneName == stageData.StageName) // �̸� üũ ���.
        {
            Debug.Log("�̹� ���� ���������Դϴ�.");
            return;
        }
        //int cs = SceneManager.GetActiveScene().buildIndex;
        //int ts = stageData.StageId;
        if (Manager.stage.sceneChanger.CurSceneID == stageData.StageId)//�ε��� üũ ���.
        {
            Debug.Log("�̹� ���� ���������Դϴ�.");
            return;
        }
        
        Manager.stage.sceneChanger.LoadSceneByIndex(stageIndex);
        //Manager.stage.sceneChanger.LoadSceneByName(stageData.StageName);
    }

    public void UpdateVisual()
    {
        if (label == null) return;

        if (stageData.Unlock)
        {
            label.text = $"Stage {stageData.StageId} (����)";
            label.color = Color.black; // �ر� ���� ����
        }
        else
        {
            label.text = $"Stage {stageData.StageId} (���)";
            label.color = Color.gray; // ��� ���� ����
        }
    }
}

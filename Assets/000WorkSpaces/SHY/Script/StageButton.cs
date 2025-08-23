using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class StageButton : MonoBehaviour
{
    [Header("�� ��ư�� ����ϴ� �������� �ε���")]
    public int stageIndex;

    [Header("UI ���")]
    public TextMeshProUGUI label;

    private StageData stageData;

    //void Start()  //������ �Ⱦ���
    //{
    //    // StageManager���� �ش� �������� ������ ��������
    //    stageData = StageManager.instance.GetStages()[stageIndex];

    //    UpdateVisual();
    //}
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

        string currentScene = SceneManager.GetActiveScene().name;
        //string targetScene = stageData.StageId.ToString(); // �ܹ���
        string targetScene = stageData.StageName;

        if (currentScene == targetScene) // �̸� üũ ���.
        {
            Debug.Log("�̹� ���� ���������Դϴ�.");
            return;
        }
        int cs = SceneManager.GetActiveScene().buildIndex;
        int ts = stageData.StageId;
        if (cs == ts)
        {
            Debug.Log("�̹� ���� ���������Դϴ�.");
            return;
        }
        SceneChanger.instance.LoadSceneByIndex(stageIndex);
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

using UnityEngine;

public class QuestProgressSimulator : MonoBehaviour
{
    [Header("�׽�Ʈ�� �������� �ε���")]
    public int targetStageIndex = 0;

    [Header("���൵ ��ġ")]
    public int simulatedProgress = 0;

    [Header("�ڵ� ���� ����")]
    public bool autoTriggerOnStart = false;

    void Start()
    {
        if (autoTriggerOnStart)
        {
            PublishProgress();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // ��: P Ű�� ����
        {
            PublishProgress();
        }
    }

    public void PublishProgress()
    {
        Debug.Log($"[Simulator] Stage {targetStageIndex}�� ���൵ {simulatedProgress} ����");
        QuestEventBus.Publish(targetStageIndex, simulatedProgress);
    }
}

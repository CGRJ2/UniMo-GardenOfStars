using UnityEngine;

public class QuestProgressSimulator : MonoBehaviour
{
    [Header("테스트용 스테이지 인덱스")]
    public int targetStageIndex = 0;

    [Header("진행도 수치")]
    public int simulatedProgress = 0;

    [Header("자동 발행 여부")]
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
        if (Input.GetKeyDown(KeyCode.P)) // 예: P 키로 발행
        {
            PublishProgress();
        }
    }

    public void PublishProgress()
    {
        Debug.Log($"[Simulator] Stage {targetStageIndex}에 진행도 {simulatedProgress} 발행");
        QuestEventBus.Publish(targetStageIndex, simulatedProgress);
    }
}

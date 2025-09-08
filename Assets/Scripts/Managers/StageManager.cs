using Cinemachine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    string stageId;
    StageData stageData;

    private void Awake() => Init();
    void Init()
    {
        // 현재 스테이지 Id 정보와 다음 스테이지 언락 조건 저장

        // 임시 테스트용(인게임씬으로 바로 실행하는 경우)
        if (string.IsNullOrEmpty(Manager.game.curStageId))
        {
            Manager.game.curStageId = "Stage00";
            Addressables.LoadSceneAsync($"MapScene_Stage00", LoadSceneMode.Additive);
        }
        // 타이틀 씬에서 인게임씬으로 이동 시, 아래 적용
        else
        {
            stageId = Manager.game.curStageId;
            stageData = Manager.game.stageDataDic[stageId];
            Addressables.LoadSceneAsync($"MapScene_{Manager.game.curStageId}", LoadSceneMode.Additive).Completed += task =>
            {
                // 맵 씬 로드 완료 이후에 로딩 해제
            };
        }
        
        Manager.ui.ShowAllHUDElements();
    }

    public void TryUnlockNextStage(int curQuestIndex)
    {
        //Debug.Log($"클리어 이후 진행도 {curQuestIndex}");
        // 언락 인덱스가 -면 다음 스테이지가 없음
        if (stageData.requiredQuestIndex < 0)
        {
            Debug.Log("다음 스테이지가 없음, 언락 조건 체크 안할거임");
            return;
        }

        // 언락조건에 도달 안되면 return
        if (stageData.requiredQuestIndex > curQuestIndex) return;

        // 언락 조건에 도달 시
        Manager.game.StageUnlock(stageData.nextStageId);
    }

    void OnEnable()
    {
        QuestEventBus.QuestProgressChangedEvent += TryUnlockNextStage;
    }

    void OnDisable()
    {
        QuestEventBus.QuestProgressChangedEvent -= TryUnlockNextStage;
    }
}

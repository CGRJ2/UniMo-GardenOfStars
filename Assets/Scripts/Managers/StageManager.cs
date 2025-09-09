using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    //string stageId;
    //StageData stageData;

    private void Awake()
    {
        // 타이틀에서 시작할 때
        //Init();

        // 스테이지 씬에서 시작할 때
        StartCoroutine(WaitAndInit());
    }

    IEnumerator WaitAndInit()
    {
        yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
        yield return new WaitUntil(() => Manager.firebase.UserData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.IsInit);
        Init();
    }

    void Init()
    {
        // 현재 스테이지 Id 정보와 다음 스테이지 언락 조건 저장

        // 임시 테스트용(인게임씬으로 바로 실행하는 경우)
        if (string.IsNullOrEmpty(Manager.firebase.UserData.CurStage.Value))
        {
            Manager.firebase.UserData.CurStage.Value = "Tutorial";
            Addressables.LoadSceneAsync($"MapScene_Tutorial", LoadSceneMode.Additive);
        }
        // 타이틀 씬에서 인게임씬으로 이동 시, 아래 적용
        else
        {
            Addressables.LoadSceneAsync($"MapScene_{Manager.firebase.UserData.CurStage.Value}", LoadSceneMode.Additive).Completed += task =>
            {
                // 맵 씬 로드 완료 이후에 로딩 해제
            };
        }
        
        Manager.ui.ShowAllHUDElements();

        // 현재 스테이지의 NPC 설정
        Manager.npc.SetCurrentNpc();
    }

    public void TryUnlockNextStage(int curQuestIndex)
    {
        //Debug.Log($"클리어 이후 진행도 {curQuestIndex}");
        // 언락 인덱스가 -면 다음 스테이지가 없음
        StageData curStageData = Manager.firebase.UserData.CurStageData;
        if (curStageData.requiredQuestIndex < 0)
        {
            Debug.Log("다음 스테이지가 없음, 언락 조건 체크 안할거임");
            return;
        }

        // 언락조건에 도달 안되면 return
        if (curStageData.requiredQuestIndex > curQuestIndex) return;

        // 언락 조건에 도달 시
        Manager.game.StageUnlock(curStageData.nextStageId);
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

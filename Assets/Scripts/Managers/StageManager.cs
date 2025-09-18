using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class StageManager : MonoBehaviour
{
    private static StageManager _instance;
    public static StageManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StageManager>();
            }
            return _instance;
        }
    }

    public string finalProdID => GetFinalProdID();

    private void Awake()
    {
        Manager.player.SpawnPlayer();

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
        Debug.LogWarning("UserData Inited");

        //Manager.firebase.UserData.CurStage.Value = "Tutorial";
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.IsInit);
        Debug.LogWarning("CurStageData Inited");
        //Manager.firebase.UserData.CurStage.Value = "Stage00";


        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc.IsInit);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc.CurrentQuestID.IsInit);

        Debug.LogWarning("Npc Inited");


        Manager.quest.CurStageQuestDataInit();

        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc.QuestList.IsInit);
        //yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc.QuestList.Count > 0);
        Debug.LogWarning("QuestList Inited");

        Init();
    }

    void Init()
    {
        Manager.Audio.BgmPlay(Manager.firebase.UserData.CurStage.Value, 0.5f);

        // 임시 테스트용(인게임씬으로 바로 실행하는 경우)
        if (string.IsNullOrEmpty(Manager.firebase.UserData.CurStage.Value))
        {
            Debug.LogError("현재 스테이지 ID가 적용되지 않음");
            //Manager.firebase.UserData.CurStage.Value = "Tutorial";
            //Addressables.LoadSceneAsync($"MapScene_Tutorial", LoadSceneMode.Additive);
        }
        // 튜토리얼 씬을 불러온다
        else if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
        {
            Addressables.LoadSceneAsync($"TutorialScene", LoadSceneMode.Additive).Completed += task =>
            {
                // 맵 씬 로드 완료 이후에 로딩 해제
            };
        }

        // 일반 스테이지 씬의 경우
        else
        {
            Manager.ui.SwitchToNormalHUD();

            Addressables.LoadSceneAsync($"MapScene_{Manager.firebase.UserData.CurStage.Value}", LoadSceneMode.Additive).Completed += task =>
            {
                // 맵 씬 로드 완료 이후에 로딩 해제
            };
        }

        // 현재 스테이지의 Npc에서, 진행중인 퀘스트 ID 등록
        var npc = Manager.firebase.UserData.CurStageData.Npc;
        //foreach (var value in npc.QuestList.List)

        bool allQuestCleared = true;
        for (int i = 0; i < npc.QuestList.List.Count; i ++)
        {
            //if (Manager.data.Quest.Values[value.QuestId].)
            var value = npc.QuestList.List[i];

            if (value.QuestState.Value != 3) // 클리어된 퀘스트가 아니라면
            {
                npc.CurrentQuestID.Value = value.QuestId;
                Debug.LogWarning($"CurrentQuestID 설정됨: {value.QuestId}");
                allQuestCleared = false;
                break;
            }
        }

        // 전부 다 클리어 된 상태일 때 => 마지막 퀘스트만 넣어주기
        if (allQuestCleared) 
        {
            npc.CurrentQuestID.Value = npc.QuestList.List[npc.QuestList.List.Count - 1].QuestId;
            Debug.LogWarning($"현재 스테이지 내의 모든 퀘스트를 완료하여 마지막 퀘스트ID가 설정됨. CurrentQuestID: {npc.QuestList.List[npc.QuestList.List.Count - 1].QuestId}");
        }

        Manager.camera.cam_PlayerFocus.Follow = Manager.player.PlayerObj.transform;
    }

    // 스테이지 별로 최종 생산물 설정
    string GetFinalProdID()
    {
        string finalProdID = "";
        foreach (var value in Manager.firebase.UserData.CurStageData.PlaceTileList.List)
        {
            if (!Manager.data.Building.ContainsKey(value.BuildingID.Value)) continue;

            // 작업형 건물의 가장 높은 ID의 재료를 반환하도록
            if (Manager.data.Building[value.BuildingID.Value] is ManufactureBD bd)
            {
                int result = finalProdID.CompareTo(bd.ProductID);

                // 기존ID 보다 값이 더 크다면
                if (result > 0) finalProdID = bd.ProductID;
            }
        }
        return finalProdID;
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

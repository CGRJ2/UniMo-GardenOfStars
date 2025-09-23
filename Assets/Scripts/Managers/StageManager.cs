using System;
using System.Collections;
using System.Collections.Generic;
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

        // 기다렸다 배너 광고 띄우기
        yield return new WaitForSeconds(5f); // UI 조정이 끝난 후 실행되도록 대기
        Manager.ad.ApplyBannerState(Manager.firebase.UserData.AdRemoved.Value);
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

        // 플레이어로 카메라 맞춰주기
        Manager.camera.cam_PlayerFocus.Follow = Manager.player.PlayerObj.transform;

        // 스테이지 ExitTime 체크

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


    // 현재 스테이지에 쌓인 재화 반환 (시간 * 건물 수(임시)로 계산)
    public void CheckStageExitTime(out double diffTime)
    {
        var stageData = Manager.firebase.UserData.CurStageData;

        StageExitTimeData data = Manager.firebase.UserData.StageExitTimeList.Get(stageData.Id);

        if (data == null)
        {
            Manager.firebase.UserData.StageExitTimeList.Add(stageData.Id);
            diffTime = 0;
            return;
        }

        DateTime lastClaimUtc = DateTimeOffset.FromUnixTimeMilliseconds(data.LastTime.Value).UtcDateTime;
        
        DateTime lastClaimKst = lastClaimUtc.AddHours(9);
        DateTime nowKst = DateTime.UtcNow.AddHours(9);

        TimeSpan diff = nowKst - lastClaimKst;
        
        // 초 단위로 변환
        double seconds = diff.TotalSeconds;
        diffTime = seconds;

        var update = new Dictionary<string, object>();
        update[stageData.Id] = Firebase.Database.ServerValue.Timestamp;

        Manager.firebase.Database.RootReference.Child(Manager.firebase.UserData.StageExitTimeList.Path).UpdateChildrenAsync(update);
    }
}

public class StageExitTimeData : FirebaseData
{
    public FirebaseProperty<long> LastTime;

    public StageExitTimeData(string id, string parentPath) : base(id, parentPath)
    {
        LastTime = new FirebaseProperty<long>("LastExitTime", Path);
        InitList.Add(LastTime);
    }
}


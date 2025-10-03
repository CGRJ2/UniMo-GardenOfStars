using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc.QuestList.Count >= 0);
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.Npc.QuestList.IsInit);
        Debug.LogWarning("QuestList Inited");


        yield return new WaitUntil(() => Manager.player.PlayerObj != null);
        yield return new WaitUntil(() => Manager.camera.cam_PlayerFocus != null);
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
        
        bool allQuestCleared = true;

        
        for (int i = 0; i < npc.QuestList.List.Count; i++)
        {
            //if (Manager.data.Quest.Values[value.QuestId].)
            var value = npc.QuestList.List[i];

            if (value.QuestState.Value != 3) // 클리어 후 대화 완료된 퀘스트가 아니라면
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
        _StageID = Manager.firebase.UserData.CurStage.Value;

        // 오프라인 보상 팝업 여부
        if (_StageID == "Tutorial") return;
        var questList = npc.QuestList.List;
        // 마지막 퀘스트까지 클리어된 상태라면 오프라인 보상 체크
        if (questList[questList.Count - 1].State == GameQuest.QuestState.TalkEnd)
        {
            double diffTime = GetStageAutoEarnTime(_StageID);
            if (diffTime > _AutoRewardMinTime)
            {
                //Debug.LogError("보상 팝업 열기");
                // 보상 팝업 열기
                Manager.ui.ShowPopUpAsync<OfflineRewardPopup>((popup) =>
                {
                    // 보상 팝업 닫힐 때 기본 보상 지급 & 시간 체크 루틴 실행
                    StartCoroutine(OfflineRewardInitAfterPopupClose(popup.gameObject));

                    // 마지막 퀘스트 완료된 상태면 체크 안해도 됨
                    // 완료 퀘스트 체크
                    /*bool questCleared;
                    Manager.quest.CheckCurQuestCleared(out questCleared);*/
                });
            }
            else
            {
                //Debug.LogError("보상 팝업 안열고 그냥 진행");
                // 보상 팝업 없이 바로 시간 체크 루틴 실행
                OfflineRewardInited();

                // 마지막 퀘스트 완료된 상태면 체크 안해도 됨
                // 완료 퀘스트 체크
                /*bool questCleared;
                Manager.quest.CheckCurQuestCleared(out questCleared);*/
            }
        }
        else
        {
            // 완료 퀘스트 체크
            bool questCleared;
            Manager.quest.CheckCurQuestCleared(out questCleared);
        }
    }

    IEnumerator OfflineRewardInitAfterPopupClose(GameObject obj)
    {
        yield return new WaitUntil(() => obj == null);
        Manager.firebase.UserData.Player.Money.Value += GetTotalAutoReward();
        OfflineRewardInited();
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

    #region 오프라인 보상

    // 클리어된 스테이지 한정
    // 최대 누적 2시간 => 최대보상
    // 현재 스테이지에 쌓인 재화 반환
    string _StageID;
    float _AutoRewardMinTime = 10f;     // 최소 보상 시간
    float _AutoRewardMaxTime = 7200f;   // 최대 보상 시간
    bool _AutoRewardInited = false;

    public void CheckStageExitTime(string targetStageID = null)
    {
        string _stageID;
        if (targetStageID == null)
            _stageID = _StageID;
        else
            _stageID = targetStageID;

        var stageData = Manager.firebase.UserData.CurStageData;

        // 현재 시간 저장
        Manager.firebase.UserData.StageList.Get(_stageID).StageLastExitTime.SaveCurTime();
    }

    public double GetStageAutoEarnTime(string targetStageID = null)
    {
        string _stageID;
        if (targetStageID == null)
            _stageID = _StageID;
        else
            _stageID = targetStageID;

        var stageData = Manager.firebase.UserData.CurStageData;
        long t = Manager.firebase.UserData.StageList.Get(_stageID).StageLastExitTime.Value;

        DateTime lastClaimUtc = DateTimeOffset.FromUnixTimeMilliseconds(t).UtcDateTime;

        DateTime lastClaimKst = lastClaimUtc.AddHours(9);
        DateTime nowKst = DateTime.UtcNow.AddHours(9);

        TimeSpan diff = nowKst - lastClaimKst;

        // 초 단위로 변환
        double seconds = diff.TotalSeconds;
        return seconds;
    }

    public int GetTotalAutoReward(string targetStageID = null)
    {
        string _stageID;
        if (targetStageID == null)
             _stageID = _StageID;
        else
            _stageID = targetStageID;

        int fullReward = Manager.data.Stage.Values[_stageID].StageAutoReward;

        float rewardPercent = Mathf.Clamp01((int)GetStageAutoEarnTime(_stageID) / _AutoRewardMaxTime);    // 최대보상 => 2시간

        int finalReward = (int)(fullReward * rewardPercent);

        //Debug.LogWarning($"방치 시간:{GetStageAutoEarnTime(_stageID)}, 보상 퍼센트: {rewardPercent}, 최종 보상: {finalReward}");

        return finalReward;
    }
    

    private IEnumerator ExitTimeCheckRoutine()
    {
        while (true)
        {
            Debug.LogWarning("현재 시간 저장");
            CheckStageExitTime(_StageID);
            yield return new WaitForSeconds(_AutoRewardMinTime / 2f);
        }
    }

    public void OfflineRewardInited()
    {
        _AutoRewardInited = true;
        StartCoroutine(ExitTimeCheckRoutine());
    }

    #endregion
    private void OnDestroy()
    {
        if (_AutoRewardInited)
            CheckStageExitTime(_StageID);
    }



    // 튜토리얼 스킵 키(임시)
    private void Update()
    {
#if UNITY_EDITOR
        if (TutorialManager.Instance != null)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                SkipTutorial();
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            RestartTutorial();
        }
#endif
    }

    #region 튜토리얼 스킵 & 다시하기 기능

    public void RestartTutorial()
    {
        Manager.firebase.UserData.TutorialSequence.Value = 0;
        Manager.firebase.UserData.CurStage.Subscribe(TransitionToStage);
        Manager.firebase.UserData.CurStage.Value = "Tutorial";
    }

    public void SkipTutorial()
    {
        //// 튜토리얼 진행 중단
        StopAllCoroutines();
        Manager.player.IsControl = true;
        Manager.camera.cam_PlayerFocus.Priority = 11;
        Manager.camera.cam_NpcFocus.Priority = 10;
        
        // 대화 상태 정리 (패널 닫기 전에)
        if (Manager.dialogue != null && Manager.dialogue.IsDialogueActive)
        {
            Manager.dialogue.EndDialogue();
        }
        ////-----------------------------------------------------////

        Manager.firebase.UserData.CurStage.Subscribe(TransitionToStage);

        foreach (var kvp in Manager.data.Stage.Values)
        {
            if (kvp.Value.Id == Manager.firebase.UserData.CurStage.Value)
            {
                var stageList = Manager.firebase.UserData.StageList.List;
                bool isStageDataInited = stageList.Any(s => s.Id == kvp.Value.NextStageId);
                
                // 열어줄 StageID 데이터가 이미 있으면 Add 안함
                if (!isStageDataInited)
                {
                    Debug.LogError("Add실행");
                    Manager.firebase.UserData.StageList.OnAdded.AddListener(StageFirstAdded);
                    Manager.firebase.UserData.StageList.Add(kvp.Value.NextStageId);
                }
                else
                {
                    Manager.firebase.UserData.CurStage.Value = kvp.Value.NextStageId; // 스테이지 ID 변경
                }

                break;
            }
        }
    }

    private void StageFirstAdded(StageData stageData)
    {
        Manager.firebase.UserData.StageList.OnAdded.RemoveListener(StageFirstAdded);
        Manager.firebase.UserData.CurStage.Value = stageData.Id; // 스테이지 ID 변경
    }

    private void TransitionToStage(string stageId)
    {
        Manager.firebase.UserData.CurStage.Unsubscribe(TransitionToStage);
        // 1. 패널 닫기 (씬 이동 전에 UI 정리)
        //Manager.ui.CloseAllPanels();
        //Manager.ui.CloseAllPopups();

        // 2. 씬 이동 실행
        StartCoroutine(LoadSceneAsyncAddressables());
    }
    /// <summary>
    /// Addressables를 사용한 비동기 씬 로딩
    /// </summary>
    private IEnumerator LoadSceneAsyncAddressables()
    {
        // 로딩 화면 표시 (필요한 경우)
        // ShowLoadingScreen();
        Manager.ui.ShowUltraSimpleLoadingScreen(1);

        Manager.Audio.SfxPlay("Portal");
        var handle = Addressables.LoadSceneAsync("StageScene");

        while (!handle.IsDone)
        {
            float progress = handle.PercentComplete;
            // UpdateLoadingProgress(progress);
            yield return null;
        }
    }

    #endregion
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


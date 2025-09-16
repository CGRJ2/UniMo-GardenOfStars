using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

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

    // 임시로 넣어둠. StageDataCSV에서 최종 생산물(일꾼이 들면 안되는 생산물) ID를 지정해줘야 함
    public string restrictedProdID = "it10121";

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
        foreach (var value in npc.QuestList.List)
        {
            if (value.QuestState.Value != 3) // 클리어된 퀘스트가 아니라면
            {
                npc.CurrentQuestID.Value = value.QuestId;
                Debug.LogWarning($"CurrentQuestID 설정됨: {value.QuestId}");
                break;
            }
        }
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

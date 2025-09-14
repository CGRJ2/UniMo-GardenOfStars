using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using GameNpc;
using GameQuest;
using KYS;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TutorialManager>();
            }
            return _instance;
        }
    }

    public ObservableProperty<int> Sequence = new(); // firebaseProperty로 바꾸면 됨
    

    public NpcController tutorialNPC;
    [SerializeField] CinemachineBrain cineBrain;
    public List<CinemachineVirtualCamera> cameras_TutoCutScene = new();
    [Header("튜토리얼 #0 설정")]
    [Tooltip("튜토리얼 씬이 시작되고 첫 카메라가 이동을 시작할 때 까지 대기하는 시간")]
    [SerializeField] float _Cut01_CamMoveWaitTime;

    [Tooltip("이동시작 후 상호작용 설명 팝업이 뜰 때까지 대기 시간")]
    [SerializeField] float _Cut01_InteractTutoPopWaitTime;

    [Header("튜토리얼 #2 설정")]
    [SerializeField] ProdsArea prodsArea;


    private void Awake() => Init();

    private void Start()
    {
        //Manager.ui.SwitchToTutorialProgressHUD(); // 튜토리얼용 HUD 일부 띄우기
    }
    private void Init()
    {
        Debug.LogError("튜토매니저 초기화");


        cineBrain = Camera.main.GetComponent<CinemachineBrain>();

        // 카메라 순서대로 정렬(게임오브젝트 이름 기준)
        cameras_TutoCutScene.Sort((p1, p2) => p1.gameObject.name.CompareTo(p2.gameObject.name));

        SwitchTutorialSequence(Sequence.Value);
        Sequence.Subscribe(SwitchTutorialSequence);
    }

    private void SwitchTutorialSequence(int value)
    {
        // 해당 튜토리얼 진행도는 Firebase/UserData 안에 넣거나, UserData/StageList/Tutorial만 따로 빼서 넣을 수 있을지 물어보자
        int sequence = value;
        switch (sequence)
        {
            case 0:
                TutorialSequence00();
                break;
            case 1:
                TutorialSequence01();
                break;
            case 2:
                TutorialSequence02();
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
        }

    }

    // Sequence 마지막에 대화 종료를 기점으로 진행도 저장
    public void SequenceEnd(DialogueData dialogueData = null)
    {
        //tutorialNPC.UpdateQuestData();
        Manager.dialogue.OnDialogueCompleted -= SequenceEnd;

        // 튜토리얼 진행도 상승 & 저장
        Sequence.Value += 1; // 튜토 진행도는 Firebase에서 관리. 이부분은 추후에 수정해야됨
    }

    private void TutorialSequence00()
    {
        // 1. 플레이어 조작 막기

        // 2. 시퀀스00 컷씬 시작
        StartCoroutine(Sequence00_CutScene());
    }
    IEnumerator Sequence00_CutScene()
    {
        cameras_TutoCutScene[0].Priority = 11;
        Manager.camera.cam_PlayerFocus.Priority = 10;
        yield return new WaitForSeconds(_Cut01_CamMoveWaitTime);
        cameras_TutoCutScene[0].Priority = 10;
        cameras_TutoCutScene[1].Priority = 11;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);
        cameras_TutoCutScene[1].Priority = 10;
        Manager.camera.cam_PlayerFocus.Priority = 11;

        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        // 플레이어 포커싱 카메라 전환 완료 시,
        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_move", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            StartCoroutine(Sequence00_Move());
        }, 
        (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때 동시에 손가락모양으로 조이스틱 움직이는 시늉 해주기");
        });
    }
    IEnumerator Sequence00_Move()
    {
        Debug.LogError(Manager.player.pc.Data.IsMove.Value);
        // 플레이어가 움직이면 그때 메세지 발생
        yield return new WaitUntil(() => Manager.player.pc.Data.IsMove.Value);
        yield return new WaitForSeconds(_Cut01_InteractTutoPopWaitTime);

        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_interact", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
        },
        (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때 NPC 방향 화살표 발판 보여주기");
        });

        // 이후에 NPC 영역에 접근하면 대화 진행 후, 대화 종료 시 Sequence00 완료, Sequence01로 전환
    }

    

    public void TutorialSequence01()
    {
        // 1. 플레이어 조작 막기

        // 2. 시퀀스01 컷씬 시작

        StartCoroutine(Sequence01_CutScene());
    }

    IEnumerator Sequence01_CutScene()
    {
        yield return new WaitUntil(() => prodsArea.pool != null);
        var userData = Manager.firebase.UserData;
        yield return new WaitUntil(() => userData.IsInit);
        yield return new WaitUntil(() => userData.CurStageData.IsInit);
        yield return new WaitUntil(() => userData.CurStageData.Npc.IsInit);
        yield return new WaitUntil(() => userData.CurStageData.Npc.QuestList.IsInit);
        var currentQuest = userData.CurStageData.Npc.CurQuestData;
        yield return new WaitUntil(() => currentQuest.IsInit);
        yield return new WaitUntil(() => currentQuest.QuestContentList.IsInit);
        
        // 목표 수량만큼 생산물 미리 설정(퀘스트 진행중이었다면 진행중인 양 빼고 넣어두기)
        prodsArea.ProdsCount.Value = currentQuest.QuestContentList.List[0].CurrentTargetCount - currentQuest.QuestContentList.List[0].ProgressdProdsCount.Value; 

        cameras_TutoCutScene[2].Priority = 11;
        Manager.camera.cam_PlayerFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        // 생산 건물로 포커싱 카메라 전환 완료 시,
        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence01", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence02", () =>
            {
                // 카메라 복귀
                cameras_TutoCutScene[2].Priority = 10;
                Manager.camera.cam_PlayerFocus.Priority = 11;

                Debug.LogWarning("팝업 닫음 콜백 함수 실행");
                Debug.LogWarning("마지막 팝업 닫을 때 생산 건물 방향 화살표 발판 보여주기");


            });
        });
    }


    public void TutorialSequence02()
    {
        Debug.LogError("시퀀스02 시작");
        

    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}

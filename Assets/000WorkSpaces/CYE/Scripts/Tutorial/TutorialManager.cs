using Cinemachine;
using GameNpc;
using KYS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public NpcController tutorialNPC;
    [SerializeField] CinemachineBrain cineBrain;
    public List<CinemachineVirtualCamera> cameras_TutoCutScene = new();

    public List<GameObject> arrows;

    [Header("튜토리얼 #0 설정")]
    [Tooltip("튜토리얼 씬이 시작되고 첫 카메라가 이동을 시작할 때 까지 대기하는 시간")]
    [SerializeField] float _Cut01_CamMoveWaitTime;

    [Tooltip("이동시작 후 상호작용 설명 팝업이 뜰 때까지 대기 시간")]
    [SerializeField] float _Cut01_InteractTutoPopWaitTime;

    [Header("튜토리얼 #1 설정")]
    [SerializeField] ProdsArea prodsArea;

    [Header("튜토리얼 #2 설정")]
    public string tutoHarvestBuildingID;

    [Header("튜토리얼 #9 설정")]
    [SerializeField] GameObject portal;
    [SerializeField] float portalFocusTime = 2f;

    private void Awake() => StartCoroutine(WaitAndInit());
    IEnumerator WaitAndInit()
    {
        yield return new WaitUntil(() => Manager.firebase.IsFirebaseInit);
        yield return new WaitUntil(() => Manager.firebase.UserData != null);
        yield return new WaitUntil(() => Manager.firebase.UserData.IsInit);

        Init();
    }

    private void Init()
    {
        Debug.LogWarning("튜토매니저 초기화");


        cineBrain = Camera.main.GetComponent<CinemachineBrain>();

        // 카메라 순서대로 정렬(게임오브젝트 이름 기준)
        cameras_TutoCutScene.Sort((p1, p2) => p1.gameObject.name.CompareTo(p2.gameObject.name));

        SwitchTutorialSequence(Manager.firebase.UserData.TutorialSequence.Value);
        Manager.firebase.UserData.TutorialSequence.Subscribe(SwitchTutorialSequence);
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
                TutorialSequence03();
                break;
            case 4:
                TutorialSequence04();
                break;
            case 5:
                TutorialSequence05();
                break;
            case 6:
                TutorialSequence06();
                break;
            case 7:
                TutorialSequence07();
                break;
            case 8:
                TutorialSequence08();
                break;
            case 9:
                TutorialSequence09();
                break;
        }
    }

    // Sequence 마지막에 대화 종료를 기점으로 진행도 저장
    public void SequenceEnd(DialogueData dialogueData = null)
    {
        //tutorialNPC.UpdateQuestData();
        Manager.dialogue.OnDialogueCompleted -= SequenceEnd;
        Debug.LogWarning($"시퀀스 0{Manager.firebase.UserData.TutorialSequence.Value} 종료");

        // 튜토리얼 진행도 상승 & 저장
        Manager.firebase.UserData.TutorialSequence.Value += 1;
    }

    private void TutorialSequence00()
    {
        // 1. 플레이어 조작 막기
        Manager.player.IsControl = false;

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
        // 플레이어 조작 가능상태로 전환
        Manager.player.IsControl = true;

        //Debug.LogWarning(Manager.player.pc.Data.IsMove.Value);
        // 플레이어의 조작을 감지하면, 몇 초 후 NPC로 이동하라는 팝업 활성화
        yield return new WaitUntil(() => Manager.player.PlayerObj.GetComponent<PlayerRunTimeData>().IsMove.Value);
        yield return new WaitForSeconds(_Cut01_InteractTutoPopWaitTime);

        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_interact", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
        },
        (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때 NPC 방향 화살표 발판 보여주기");
            arrows[0].SetActive(true);
        });

        // 이후에 NPC 영역에 접근하면 대화 진행 후, 대화 종료 시 Sequence00 완료, Sequence01로 전환
    }



    public void TutorialSequence01()
    {
        Manager.camera.cam_PlayerFocus.Priority = 11;

        Debug.LogWarning("시퀀스01 시작");

        // 1. 플레이어 조작 막기
        Manager.player.IsControl = false;

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

        /////// 퀘스트가 완료된 상황인데, 대사를 완료하지 않고 종료해서 현재 단계를 스킵하면서 퀘스트 대사만 나오도록 한 부분
        bool questCleared;
        Manager.quest.CheckCurQuestCleared(out questCleared);
        if (questCleared)
        {
            yield break;
        }
        ////////////////////////////////////////////////////

        // 목표 수량만큼 생산물 미리 설정(퀘스트 진행중이었다면 진행중인 양 빼고 넣어두기)
        prodsArea.ProdsCount.Value = currentQuest.QuestContentList.List[0].CurrentTargetCount - currentQuest.QuestContentList.List[0].ProgressdProdsCount.Value;

        cameras_TutoCutScene[2].Priority = 11;
        Manager.camera.cam_PlayerFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        // 생산 건물로 포커싱 카메라 전환 완료 시,
        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence01-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence02-2", () =>
            {
                // 카메라 복귀
                cameras_TutoCutScene[2].Priority = 10;
                Manager.camera.cam_PlayerFocus.Priority = 11;

                // 플레이어 조작 가능상태로 전환
                Manager.player.IsControl = true;

                Debug.LogWarning("팝업 닫음 콜백 함수 실행");
                Debug.LogWarning("마지막 팝업 닫을 때 생산 건물 방향 화살표 발판 보여주기");

                arrows[1].SetActive(true);
            });
        });
    }


    public void TutorialSequence02()
    {
        Debug.LogWarning("시퀀스02 시작");
        Manager.camera.cam_PlayerFocus.Priority = 11;


        // 플레이어 조작 막기
        Manager.player.IsControl = false;


        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence02-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            // 팝업 닫으면서 재화 UI활성화
            Manager.ui.SwitchToTutorialProgressHUD(); // 튜토리얼용 HUD 일부(재화) 띄우기

            // 플레이어 조작 활성화
            Manager.player.IsControl = true;
        },
        (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때 부동산 방향 화살표 발판 보여주기");
            arrows[3].SetActive(true);

            // 퀘스트 발판 활성화
            tutorialNPC.ShowQuestTiles();
        });
    }

    public void TutorialSequence03()
    {
        Manager.camera.cam_PlayerFocus.Priority = 11;

        Debug.LogWarning("시퀀스03 시작");

        StartCoroutine(Sequence03_CutScene());
    }

    IEnumerator Sequence03_CutScene()
    {
        // 플레이어가 건물(재료)를 손에 넣을 때 까지 대기
        while (true)
        {
            IngrediantInstance building;
            Manager.player.PlayerObj.GetComponent<PlayerRunTimeData>().IngrediantStack.TryPeek(out building);
            if (building == null)
            {
                yield return null;
                continue;
            }
            else
            {
                if (building is Item_Building)
                {
                    yield return null;
                    break;
                }
            }
        }

        Debug.LogWarning("진행됨");
            
        // 플레이어 조작 비활성화
        Manager.player.IsControl = false;

        // 공터 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[3].Priority = 11;
        Manager.camera.cam_PlayerFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);


        // 포커스 완료 시 팝업 메세지 띄우기
        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence03-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // 카메라 복귀
            cameras_TutoCutScene[3].Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 플레이어 조작 활성화
            Manager.player.IsControl = true;
        },
        (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때");
        });

        // 이어서 건설 모드 상에서 플레이어가 공터로 건물(재료) 설치 진행
    }

    public void TutorialSequence04()
    {
        Debug.LogWarning("시퀀스04 시작");

        // 플레이어 조작 비활성화
        Manager.player.IsControl = false;

        StartCoroutine(Sequence04_CutScene01());
    }

    IEnumerator Sequence04_CutScene01()
    {
        // 공터 포커스 카메라 컷씬 진행(수확형 건물이 설치된 공터 포커싱)
        cameras_TutoCutScene[3].Priority = 11;
        Manager.camera.cam_PlayerFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        // 포커스 완료 시 팝업 메세지 띄우기
        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence04-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            StartCoroutine(Sequence04_CutScene02());
        }, (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때, 수확형 건물이 빛나는 효과 실행");
        });
    }

    IEnumerator Sequence04_CutScene02()
    {
        // 작업형 건물 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[3].Priority = 10;
        cameras_TutoCutScene[2].Priority = 11;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence04-2", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // 카메라 복귀
            cameras_TutoCutScene[2].Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;

            // 시퀀스04 종료
            SequenceEnd();
        }, (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때, 작업형 건물이 빛나는 효과 실행");
        });
    }
    public void TutorialSequence05()
    {
        Manager.camera.cam_PlayerFocus.Priority = 11;

        // 이건 퀘스트 매니저에서 처리해서 따로 추가할 게 없음
        Debug.LogWarning("시퀀스05 시작");

        // NPC 방향 화살표 활성화
        arrows[2].SetActive(true);

        StartCoroutine(Sequence05());

    }

    IEnumerator Sequence05()
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

        /////// 퀘스트가 완료된 상황인데, 대사를 완료하지 않고 종료해서 현재 단계를 스킵하면서 퀘스트 대사만 나오도록 한 부분
        bool questCleared;
        Manager.quest.CheckCurQuestCleared(out questCleared);
        if (questCleared)
        {
            yield break;
        }
        ////////////////////////////////////////////////////
        
        Debug.LogWarning("작업형 건물에서 재단 방향으로 화살표 정도만 띄워주면 될듯");
    }

    public void TutorialSequence06()
    {
        Debug.LogWarning("시퀀스06 시작");

        // 플레이어 조작 막기
        Manager.player.IsControl = false;

        StartCoroutine(Sequence06_CutScene01());
    }

    IEnumerator Sequence06_CutScene01()
    {
        yield return new WaitUntil(() => Manager.camera.cam_NpcFocus.Follow != null);

        // NPC 포커스 카메라 컷씬 진행
        Manager.camera.cam_NpcFocus.Priority = 11;
        Manager.camera.cam_PlayerFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence06-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            StartCoroutine(Sequence06_CutScene02());
        }, (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때, 석상에서 빛나는 효과");
        });
    }

    IEnumerator Sequence06_CutScene02()
    {
        // 일꾼 건물 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[4].Priority = 11;
        Manager.camera.cam_NpcFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence06-2", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // 카메라 복귀
            cameras_TutoCutScene[4].Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;

            // 인력사무소 방향 화살표 활성화
            arrows[4].SetActive(true);


        }, (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때, 인력사무소 빛나는 효과");

            // 인력사무소 상호작용 발판 활성화
            Manager.buildings.workerBuilding.ShowWaitingTile();
        });
    }


    // 일꾼이 소환 or 일꾼 구매 버튼을 누르는 시점에서
    // 튜토리얼 씬이라면 SequenceEnd(); 06 -> 07

    public void TutorialSequence07()
    {
        Debug.LogWarning("시퀀스07 시작");

        Manager.camera.cam_PlayerFocus.Priority = 11;

        // 인력사무소 방향 화살표 비활성화
        arrows[4].SetActive(false);

        // 플레이어 조작 막기
        Manager.player.IsControl = false;

        StartCoroutine(Sequence07());
    }

    IEnumerator Sequence07()
    {
        // 일꾼 카메라 Follow 등록까지 대기
        yield return new WaitUntil(() => cameras_TutoCutScene[5].Follow != null);

        // 일꾼 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[5].Priority = 11;
        Manager.camera.cam_NpcFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence07-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            //StartCoroutine(Sequence07_CutScene01());

            // 카메라 복귀
            cameras_TutoCutScene[5].Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 우선 업그레이드 관한 설명 없이 다음 단계로 진행
            // 원래대로라면 업그레이드 진행 후, 시퀀스 적용? 모르겠다
            SequenceEnd();

        }, (msg) =>
        {
            //Debug.LogWarning("팝업 열었을 때, ");
        });
    }

    IEnumerator Sequence07_CutScene01()
    {
        // 일꾼 건물 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[4].Priority = 11;
        cameras_TutoCutScene[5].Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        Manager.ui.ShowMessagePopUpWithKeyAsync("msg_tutorial_questSquence07-2", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // 카메라 복귀
            cameras_TutoCutScene[4].Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;
        }, (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때, 인력사무소 빛나는 효과");

            // 인력사무소 상호작용 발판 활성화
            Manager.buildings.workerBuilding.ShowWaitingTile();

            // 이후에 인력사무소 패널에서 업그레이드 진행
        });
    }

    // 퀘스트 3번 진행
    public void TutorialSequence08()
    {
        Debug.LogWarning("시퀀스08 시작");

        Manager.camera.cam_PlayerFocus.Priority = 11;

        // NPC 방향 화살표 활성화
        arrows[2].SetActive(true);

        // 플레이어 조작 활성화
        Manager.player.IsControl = true;

        StartCoroutine(Sequence08());
    }

    IEnumerator Sequence08()
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

        /////// 퀘스트가 완료된 상황인데, 대사를 완료하지 않고 종료해서 현재 단계를 스킵하면서 퀘스트 대사만 나오도록 한 부분
        bool questCleared;
        Manager.quest.CheckCurQuestCleared(out questCleared);
        if (questCleared)
        {
            yield break;
        }
        ////////////////////////////////////////////////////

        // 퀘스트 발판 활성화
        tutorialNPC.ShowQuestTiles();
    }


    public void TutorialSequence09()
    {
        Debug.LogWarning("시퀀스09 시작");

        Manager.camera.cam_PlayerFocus.Priority = 11;

        // 플레이어 조작 비활성화
        Manager.player.IsControl = false;

        StartCoroutine(Sequence09());
    }

    IEnumerator Sequence09()
    {
        portal.SetActive(true);

        // 포탈 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[7].Priority = 11;
        Manager.camera.cam_PlayerFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);
        yield return new WaitForSeconds(portalFocusTime);

        // 카메라 복귀
        cameras_TutoCutScene[7].Priority = 10;
        Manager.camera.cam_PlayerFocus.Priority = 11;

        // 플레이어 조작 활성화
        Manager.player.IsControl = true;
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}

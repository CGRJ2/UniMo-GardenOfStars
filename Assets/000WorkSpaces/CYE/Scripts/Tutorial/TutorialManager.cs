using Cinemachine;
using GameNpc;
using KYS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

    [Header("튜토리얼 #3 설정")]
    [SerializeField] PlaceTile placeTile;

    [Header("튜토리얼 #8 설정")]
    [Tooltip("석상 활성화(재질 디졸브) 이후 대화가 출력되기 까지 대기 시간")]
    [SerializeField] float waitTimeAfterDissolve = 1f;
    [SerializeField] ShopBuilding shopBuilding;

    [Header("튜토리얼 #9 설정")]
    [SerializeField] NpcInteractAreaUI talk_Button;
    public GameObject handPointer_PlayerUpradeBtn;
    public GameObject handPointer_InPlayerUpradePanel;

    [Header("튜토리얼 #11 설정")]
    [SerializeField] GameObject portal;
    [SerializeField] float portalFocusTime = 2f;

    // 강조 FX 효과
    ObjectPool _Pool_FX_Highlighted;
    List<GameObject> _FX_Highlighteds = new();

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

        // FX 불러온 후, 풀로 반환 (없으면 풀 생성)
        Addressables.LoadAssetAsync<GameObject>("FX/Highlighted.Prefab").Completed += task =>
        {
            _Pool_FX_Highlighted = Manager.pool.GetPoolBundle(task.Result, 1).instancePool;
        };
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
            case 10:
                TutorialSequence10();
                break;
            case 11:
                TutorialSequence11();
                break;
        }
    }

    public void PlayHighLightFX(Transform target, bool isMultipleAdd = false)
    {
        if (!isMultipleAdd)
        {
            // 기존 실행 중이던 강조 효과 제거
            if (_FX_Highlighteds.Count > 0)
            {
                foreach (GameObject fx in _FX_Highlighteds)
                {
                    _Pool_FX_Highlighted.ReturnPooledObj(fx);
                }
                _FX_Highlighteds = new();
            }
        }

        // 타겟이 없으면 그냥 효과 제거만 함
        if (target == null) return;

        // 효과 추가 활성화
        _FX_Highlighteds.Add(_Pool_FX_Highlighted.DisposePooledObj(target.position + Vector3.up * 1.2f, target.rotation));
    }

    // Sequence 마지막에 대화 종료를 기점으로 진행도 저장
    public void SequenceEnd(DialogueData dialogueData = null)
    {
        //tutorialNPC.UpdateQuestData();
        Manager.dialogue.OnDialogueCompleted -= SequenceEnd;
        Debug.LogWarning($"시퀀스 0{Manager.firebase.UserData.TutorialSequence.Value} 종료");

        // 튜토리얼 진행도 상승 & 저장
        Manager.firebase.UserData.TutorialSequence.Value += 1;

        // 강조 효과 제거
        PlayHighLightFX(null);
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
        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_move", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;

            StartCoroutine(Sequence00_Move());
        },
        (msg) =>
        {
            // 플레이어 조작 불가능상태로 전환
            Manager.player.IsControl = false;

            Debug.LogWarning("팝업 열었을 때 동시에 손가락모양으로 조이스틱 움직이는 시늉 해주기");
        });
    }
    IEnumerator Sequence00_Move()
    {
        //Debug.LogWarning(Manager.player.pc.Data.IsMove.Value);
        // 플레이어의 조작을 감지하면, 몇 초 후 NPC로 이동하라는 팝업 활성화
        yield return new WaitUntil(() => Manager.player.PlayerObj.GetComponent<PlayerRunTimeData>().IsMove.Value);
        yield return new WaitForSeconds(_Cut01_InteractTutoPopWaitTime);

        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_interact", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // NPC 강조효과 실행
            PlayHighLightFX(tutorialNPC.transform);

            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;
        },
        (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때 NPC 방향 화살표 발판 보여주기");
            arrows[0].SetActive(true);
            // 플레이어 조작 불가능상태로 전환
            Manager.player.IsControl = false;
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
        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence01-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence02-2", () =>
            {
                // 카메라 복귀
                cameras_TutoCutScene[2].Priority = 10;
                Manager.camera.cam_PlayerFocus.Priority = 11;

                // 플레이어 조작 가능상태로 전환
                Manager.player.IsControl = true;

                Debug.LogWarning("팝업 닫음 콜백 함수 실행");
                Debug.LogWarning("마지막 팝업 닫을 때 생산 건물 방향 화살표 발판 보여주기");

                // NPC -> 생산 건물 방향 화살표
                arrows[1].SetActive(true);
            });
        }, (popUpOn) =>
        {
            //생산 건물 강조효과
            PlayHighLightFX(prodsArea.transform);
        });
    }


    public void TutorialSequence02()
    {
        Debug.LogWarning("시퀀스02 시작");
        Manager.camera.cam_PlayerFocus.Priority = 11;

        // 플레이어 조작 막기
        Manager.player.IsControl = false;


        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence02-1", () =>
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

            // 부동산 강조 효과 실행
            PlayHighLightFX(Manager.buildings.buildingSeller.transform);
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
        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence03-1", () =>
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

            // 공터 강조 효과
            PlayHighLightFX(placeTile.transform);
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
        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence04-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            StartCoroutine(Sequence04_CutScene02());
        }, (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때, 수확형 건물이 빛나는 효과 실행");
            PlayHighLightFX(placeTile.transform);
        });
    }

    IEnumerator Sequence04_CutScene02()
    {
        // 작업형 건물 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[3].Priority = 10;
        cameras_TutoCutScene[2].Priority = 11;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence04-2", () =>
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
            PlayHighLightFX(prodsArea.ownerInstance.transform);
        });
    }
    public void TutorialSequence05()
    {
        Manager.camera.cam_PlayerFocus.Priority = 11;

        // 이건 퀘스트 매니저에서 처리해서 따로 추가할 게 없음
        Debug.LogWarning("시퀀스05 시작");

        // NPC 방향 화살표 활성화
        arrows[2].SetActive(true);
        // 수확 -> 생산 건물 방향 화살표 활성화
        arrows[5].SetActive(true);

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

        // 퀘스트 절반 이상 진행했을 때,
        yield return new WaitUntil(() => (currentQuest.QuestContentList.List[0].ProgressdProdsCount.Value >= currentQuest.QuestContentList.List[0].CurrentTargetCount / 2));

        // NPC 방향 화살표 비활성화
        arrows[2].SetActive(false);
        // 수확 -> 생산 건물 방향 화살표 비활성화
        arrows[5].SetActive(false);

        // 퀘스트 타일 숨기기
        tutorialNPC.HideQuestTiles();

        // 플레이어 조작 불가능상태로 전환
        Manager.player.IsControl = false;

        // 부동산 상호작용 발판 활성화
        Manager.buildings.buildingSeller.ShowWaitingTile();

        // 부동산 건물 포커스 카메라 컷씬 진행
        Manager.camera.cam_PlayerFocus.Priority = 10;
        cameras_TutoCutScene[6].Priority = 11;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        // 부동산 강조효과 실행
        PlayHighLightFX(Manager.buildings.buildingSeller.transform);

        // 부동산을 통해 건물의 생산 능력을 강화할 수 있습니다
        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence05-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // 카메라 복귀
            cameras_TutoCutScene[6].Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;

            // 부동산쪽으로 다시 유도하는 표기, 
            arrows[3].SetActive(true);

            StartCoroutine(Sequence05_UpgradeCheck());

            // TODO 업그레이드 창 열었을 때, 업그레이드 버튼을 유도하는 표기
        });
    }

    IEnumerator Sequence05_UpgradeCheck()
    {
        // 수확 건물을 강화해? 생산 건물을 강화해?

        // 여기서 퀘스트 발판을 막고, 업그레이드를 해야 퀘스트 발판이 다시 생기게 만들어서 업그레이드를 강제해야할 듯

        string[] buildingIDs = Manager.data.Stage.Values["Tutorial"].GetBuildingIdList();
        string buildingID = buildingIDs[0];

        UpgradeData upgradeData = Manager.buildings.GetUpgradeData(buildingID);

        yield return new WaitUntil(() => upgradeData.level_ProdTime > 0); // 생산시간 업그레이드 한 번 했을 때 진행

        // 업그레이드 패널 닫기
        Manager.ui.CloseAllPanels();
        Manager.ui.CloseAllPopups();

        PlayHighLightFX(null);
        tutorialNPC.ShowQuestTiles();

        // NPC 방향 화살표 활성화
        arrows[2].SetActive(true);
        // 수확 -> 생산 건물 방향 화살표 활성화
        arrows[5].SetActive(true);
        // 부동산 방향 화살표 비활성화
        arrows[3].SetActive(false);
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

        // npc 강조효과 실행
        PlayHighLightFX(tutorialNPC.transform);

        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence06-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");
            StartCoroutine(Sequence06_CutScene02());
        });
    }

    IEnumerator Sequence06_CutScene02()
    {
        // 일꾼 건물 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[4].Priority = 11;
        Manager.camera.cam_NpcFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence06-2", () =>
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
            PlayHighLightFX(Manager.buildings.workerBuilding.transform);

            // 인력사무소 상호작용 발판 활성화
            Manager.buildings.workerBuilding.ShowWaitingTile();
        });
    }


    // 일꾼이 소환 or 일꾼 구매 버튼을 누르는 시점에서
    // 튜토리얼 씬이라면 SequenceEnd(); 06 -> 07

    public void TutorialSequence07()
    {
        Debug.LogWarning("시퀀스07 시작");

        // 일꾼 건물 발판 비활성화
        Manager.buildings.workerBuilding.HideWaitingTile();

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

        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence07-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            StartCoroutine(Sequence07_CutScene01());

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

        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence07-2", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // 카메라 복귀
            cameras_TutoCutScene[4].Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;

            SequenceEnd(); // 시퀀스07 종료
        }, (msg) =>
        {
            PlayHighLightFX(Manager.buildings.workerBuilding.transform);
        });
    }

    // 퀘스트 3번 진행
    public void TutorialSequence08()
    {
        Debug.LogWarning("시퀀스08 시작");

        Manager.camera.cam_PlayerFocus.Priority = 11;

        

        // 플레이어 조작 활성화
        Manager.player.IsControl = true;

        // 인력사무소 상호작용 발판 활성화
        Manager.buildings.workerBuilding.ShowWaitingTile();

        StartCoroutine(Sequence08());
    }


    IEnumerator Sequence08()
    {
        var userData = Manager.firebase.UserData;
        yield return new WaitUntil(() => userData.IsInit);
        yield return new WaitUntil(() => userData.CurStageData.IsInit);
        yield return new WaitUntil(() => userData.CurStageData.Npc.IsInit);
        yield return new WaitUntil(() => userData.CurStageData.Npc.QuestList.IsInit);
        var currentQuest = userData.CurStageData.Npc.CurQuestData;
        yield return new WaitUntil(() => currentQuest.IsInit);
        yield return new WaitUntil(() => currentQuest.QuestContentList.IsInit);

        // 퀘스트 발판 비활성화 (일꾼 업그레이드를 진행해야 퀘스트 타일이 보이도록)
        yield return new WaitUntil(() => tutorialNPC != null);
        tutorialNPC.HideQuestTiles();
        yield return new WaitUntil(() => Manager.buildings.workerBuilding != null);
        PlayHighLightFX(Manager.buildings.workerBuilding.transform);

        /////// 퀘스트가 완료된 상황인데, 대사를 완료하지 않고 종료해서 현재 단계를 스킵하면서 퀘스트 대사만 나오도록 한 부분
        bool questCleared;
        Manager.quest.CheckCurQuestCleared(out questCleared);
        if (questCleared)
        {
            yield break;
        }
        ////////////////////////////////////////////////////

        // 일꾼이 업그레이드 된 상태라면
        yield return new WaitUntil(() => Manager.firebase.UserData.CurStageData.WorkerList.List[0].MaxCapacityLv.Value > 1 &&
            Manager.firebase.UserData.CurStageData.WorkerList.List[0].MoveSpeedLv.Value > 1);

        // 업그레이드 패널 닫기
        Manager.ui.CloseAllPanels();
        Manager.ui.CloseAllPopups();

        // 전당포 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[8].Priority = 11;
        Manager.camera.cam_PlayerFocus.Priority = 10;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        // 남는 재료는 전당포에 팔아 재화를 얻을 수 있습니다.
        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence08-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            StartCoroutine(Sequence08_CutScene02());
            PlayHighLightFX(null);

        }, (msg) =>
        {
            Debug.LogWarning("팝업 열었을 때, 전당포 빛나는 효과");
            PlayHighLightFX(shopBuilding.transform);
        });

        // 퀘스트 발판 활성화
        tutorialNPC.ShowQuestTiles();

        // NPC 방향 화살표 활성화
        arrows[2].SetActive(true);
    }

    IEnumerator Sequence08_CutScene02()
    {
        // NPC 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[8].Priority = 10;
        Manager.camera.cam_NpcFocus.Priority = 11;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        // 곧 석상이 깨어날 거에요. 별가루를 모아봅시다
        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence08-2", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // 카메라 복귀
            Manager.camera.cam_NpcFocus.Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;

            // 이후 퀘스트 3번 진행

        });
    }
    public IEnumerator TutoQuest03ClearCutScene()
    {
        // 석상 깨어나는 연출 대기

        // 석상 빛나는 연출?
        yield return new WaitUntil(() => _Pool_FX_Highlighted != null);
        PlayHighLightFX(tutorialNPC.transform);

        // 석상 매터리얼 디졸브
        Material dissolveMat = tutorialNPC.view_Dissolve.GetComponent<Renderer>().materials[0];
        dissolveMat.SetFloat("_Dissolve", 0);
        float value = 0;
        while (value < 1)
        {
            value = dissolveMat.GetFloat("_Dissolve") + Time.deltaTime;
            dissolveMat.SetFloat("_Dissolve", value);
            yield return null;
        }

        yield return new WaitForSeconds(waitTimeAfterDissolve);
        PlayHighLightFX(null);

        // 연출 끝나고 대화 시작
        var npc = Manager.firebase.UserData.CurStageData.Npc;
        Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, Manager.firebase.UserData.CurStage.Value, $"Quest_{npc.NpcID.Value}_{npc.CurrentQuestID.Value}");
    }

    public void TutorialSequence09()
    {
        Debug.LogWarning("시퀀스 09 시작");

        StartCoroutine(Sequence09_CutScene01());
    }



    IEnumerator Sequence09_CutScene01()
    {
        // 플레이어 조작 비활성화
        Manager.player.IsControl = false;

        // NPC 포커스 카메라 컷씬 진행
        Manager.camera.cam_PlayerFocus.Priority = 10;
        Manager.camera.cam_NpcFocus.Priority = 11;
        yield return new WaitUntil(() => cineBrain.IsBlending);
        yield return new WaitUntil(() => !cineBrain.IsBlending);

        // 별자리를 통해 리비의 능력을 강화할 수 있습니다
        Manager.ui.ShowTutorialPopUpWithKeyAsync("msg_tutorial_questSquence09-1", () =>
        {
            Debug.LogWarning("팝업 닫음 콜백 함수 실행");

            // 카메라 복귀
            Manager.camera.cam_NpcFocus.Priority = 10;
            Manager.camera.cam_PlayerFocus.Priority = 11;

            // 플레이어 조작 가능상태로 전환
            Manager.player.IsControl = true;

            // 대화하기 버튼 활성화
            talk_Button.Init();
            talk_Button.gameObject.SetActive(true);

            // 이후 NPC 접근 후 대화하기 버튼으로 창을 열고, 업그레이드하기
            StartCoroutine(Sequence09_CheckUpgradeState());
        });
    }

    IEnumerator Sequence09_CheckUpgradeState()
    {
        var userData = Manager.firebase.UserData;
        yield return new WaitUntil(() => userData.IsInit);
        yield return new WaitUntil(() => userData.CurStageData.IsInit);
        yield return new WaitUntil(() => userData.Player.IsInit);

        // 이동속도만 1 업그레이드 하면 진행됨
        yield return new WaitUntil(() => userData.Player.MoveSpeedLv.Value > 1);

        // 업그레이드 확인 시 시퀀스 종료
        SequenceEnd();
    }

    public void TutorialSequence10()
    {
        Debug.LogWarning("시퀀스 10 시작");

        talk_Button.gameObject.SetActive(false);

        // 업그레이드 패널 닫기
        Manager.ui.CloseAllPanels();
        Manager.ui.CloseAllPopups();

        Manager.camera.cam_PlayerFocus.Priority = 10;
        Manager.camera.cam_NpcFocus.Priority = 11;

        // 플레이어 조작 비활성화
        Manager.player.IsControl = false;

        // 그냥 키를 넣기
        // 해당 대화가 종료되면 콜백함수로 Sequence10 종료
        var npc = Manager.firebase.UserData.CurStageData.Npc;
        Manager.dialogue.OnDialogueCompleted += SequenceEnd; // 대화 완료 시, 시퀀스 10종료
        Manager.dialogue.StartDialogueWithPanel(npc.NpcID.Value, "Tutorial", $"tutorial_End");
    }

    public void TutorialSequence11()
    {
        Debug.LogWarning("시퀀스 11 시작");

        // 플레이어 조작 비활성화
        Manager.player.IsControl = false;

        StartCoroutine(Sequence11());
    }

    IEnumerator Sequence11()
    {
        portal.SetActive(true);

        // 포탈 강조 FX
        PlayHighLightFX(portal.transform);

        // 포탈 포커스 카메라 컷씬 진행
        cameras_TutoCutScene[7].Priority = 11;
        Manager.camera.cam_NpcFocus.Priority = 10;
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


    // 튜토리얼 스킵 키(임시)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Manager.firebase.UserData.TutorialSequence.Value = 11;

            foreach (var kvp in Manager.data.Stage.Values)
            {
                if (kvp.Value.Id == Manager.firebase.UserData.CurStage.Value)
                {
                    Manager.firebase.UserData.StageList.Add(kvp.Value.NextStageId);
                    break;
                }
            }
        }
    }

}

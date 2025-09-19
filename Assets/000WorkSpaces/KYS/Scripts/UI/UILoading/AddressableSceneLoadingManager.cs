using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace KYS
{
    /// <summary>
    /// Addressables를 사용한 씬 로딩과 LoadingScreen을 연동하는 매니저
    /// </summary>
    public class AddressableSceneLoadingManager : MonoBehaviour
    {

        [ContextMenu("UIMnagerLoadingScreen Method 실행")]
        public void UIManagerLoadingScreen()
        {
            // LoadingScreen 테스트용 메서드
            UIManager.Instance.ShowLoadingScreenByKey("loading_data", "데이터를 불러오는 중...");

            UIManager.Instance.HideLoadingScreen();
        }


        [ContextMenu("UIMnagerLoadingScreen Method 실행2")]
        public async Task UIManagerLoadingScreen2()
        {
            string[] keys = { "loading_text", "loading_data", "loading_wait", "loading_almost_done" };
            string[] fallbacks = { "초기화 중...", "데이터 로딩 중...", "리소스 준비 중...", "완료!" };
            float[] progressValues = { 0.25f, 0.5f, 0.75f, 1.0f };
            int[] delays = { 1000, 1500, 1000, 500 }; // 각 단계별 지연 시간 (밀리초)

            // 완전체 메서드 사용 - 자동으로 HideLoadingScreen() 포함
            await UIManager.Instance.ExecuteStepLoadingByKeysAsync(
                keys,
                fallbacks,
                progressValues,
                delays,
                animationDuration: 0.8f, // 애니메이션 지속시간
                autoHide: true, // 자동 숨기기 활성화
                completionKey: "loading_complete", // 완료 메시지 키 (선택사항)
                completionFallback: "로딩 완료!" // 완료 메시지 폴백
            );
        }

        [ContextMenu("완전체 메서드 테스트")]
        public async Task CompleteMethodTest()
        {
            string[] keys = { "loading_text", "loading_data", "loading_wait", "loading_almost_done" };
            string[] fallbacks = { "초기화 중...", "데이터 로딩 중...", "리소스 준비 중...", "완료!" };
            float[] progressValues = { 0.25f, 0.5f, 0.75f, 1.0f };
            int[] delays = { 1000, 1500, 1000, 500 };

            // 완전체 메서드 사용 - 모든 기능 포함
            await UIManager.Instance.ExecuteStepLoadingByKeysAsync(
                keys,
                fallbacks,
                progressValues,
                delays,
                animationDuration: 1.0f, // 긴 애니메이션
                autoHide: true, // 자동 숨기기
                completionKey: "loading_complete", // 완료 메시지
                completionFallback: "모든 준비가 완료되었습니다!"
            );
        }

        [ContextMenu("수동 숨기기 테스트")]
        public async Task ManualHideTest()
        {
            string[] keys = { "loading_text", "loading_data", "loading_wait" };
            string[] fallbacks = { "초기화 중...", "데이터 로딩 중...", "리소스 준비 중..." };
            float[] progressValues = { 0.33f, 0.66f, 0.99f };
            int[] delays = { 800, 1200, 800 };

            // 자동 숨기기 비활성화
            await UIManager.Instance.ExecuteStepLoadingByKeysAsync(
                keys,
                fallbacks,
                progressValues,
                delays,
                animationDuration: 0.6f,
                autoHide: false // 수동으로 숨기기
            );

            // 수동으로 추가 작업 후 숨기기
            await System.Threading.Tasks.Task.Delay(2000);
            UIManager.Instance.HideLoadingScreen();
        }

        [ContextMenu("빠른 애니메이션 테스트")]
        public async Task FastAnimationTest()
        {
            string[] keys = { "loading_text", "loading_data", "loading_wait", "loading_almost_done" };
            string[] fallbacks = { "초기화 중...", "데이터 로딩 중...", "리소스 준비 중...", "완료!" };
            float[] progressValues = { 0.25f, 0.5f, 0.75f, 1.0f };
            int[] delays = { 500, 800, 500, 300 }; // 빠른 지연 시간

            // 빠른 애니메이션으로 실행
            await UIManager.Instance.ExecuteStepLoadingByKeysAsync(
                keys,
                fallbacks,
                progressValues,
                delays,
                animationDuration: 0.3f, // 빠른 애니메이션
                autoHide: true,
                completionKey: "loading_complete",
                completionFallback: "빠른 로딩 완료!"
            );
        }




        /// <summary>
        /// 타이틀에서 인게임으로 이동하는 완전체 로딩 시스템
        /// </summary>
        [ContextMenu("Task 사용")]
        public async System.Threading.Tasks.Task Temp_InGameLoadAsync()
        {
            // 완전체 로딩 시스템 사용 (자동 숨김 포함)
            await LoadSceneWithCompleteLoadingAsync("StageScene",
                LoadingLocalizationKeys.STAGE_PREPARE,
                LoadingLocalizationKeys.STAGE_LOADING,
                LoadingLocalizationKeys.STAGE_COMPLETE);
        }

        [ContextMenu("코루틴 사용")]
        public void Temp_INGameLoadbyCoroutine()
        {
            StartCoroutine(Temp_InGameLoad());
        }

        /// <summary>
        /// 타이틀에서 인게임으로 이동하는 완전체 로딩 시스템 (코루틴 버전)
        /// </summary>

        public IEnumerator Temp_InGameLoad()
        {
            // 완전체 로딩 시스템 사용 (자동 숨김 포함)
            yield return StartCoroutine(LoadSceneWithCompleteLoading("StageScene",
                LoadingLocalizationKeys.STAGE_PREPARE,
                LoadingLocalizationKeys.STAGE_LOADING,
                LoadingLocalizationKeys.STAGE_COMPLETE));
        }

        /// <summary>
        /// 완전체 로딩 시스템으로 씬 로드 (로컬라이제이션 + 애니메이션 + 자동 숨김) - 비동기 버전
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름</param>
        /// <param name="prepareKey">준비 메시지 키</param>
        /// <param name="loadingKey">로딩 메시지 키</param>
        /// <param name="completeKey">완료 메시지 키</param>
        public async System.Threading.Tasks.Task LoadSceneWithCompleteLoadingAsync(string sceneName, string prepareKey = "loading_prepare", string loadingKey = "loading_progress", string completeKey = "loading_complete")
        {
            Debug.Log($"[GameManager] 완전체 로딩 시작 (비동기): {sceneName}");

            // 1단계: 준비 단계 (0.2초)
            await Manager.ui.ExecuteStepLoadingByKeysAsync(
                new string[] { prepareKey },
                new string[] { "준비 중..." },
                new float[] { 0.2f },
                new int[] { 200 },
                0.3f,
                false,
                null,
                null
            );

            // 2단계: 씬 로드 시작
            var loadSceneHandle = Addressables.LoadSceneAsync(sceneName);

            // 3단계: 로딩 진행률 모니터링 (0.8초)
            await Manager.ui.ExecuteStepLoadingByKeysAsync(
                new string[] { loadingKey },
                new string[] { "로딩 중..." },
                new float[] { 0.8f },
                new int[] { 800 },
                0.4f,
                false,
                null,
                null
            );

            // 4단계: 씬 로드 완료 대기
            while (!loadSceneHandle.IsDone)
            {
                // 로딩 진행률을 실시간으로 업데이트
                float progress = loadSceneHandle.PercentComplete;
                Manager.ui.SetLoadingProgressAnimated(progress, 0.1f);
                await System.Threading.Tasks.Task.Yield();
            }

            await loadSceneHandle.Task;

            // 5단계: 완료 메시지 표시 및 자동 숨김 (0.5초)
            await Manager.ui.ExecuteStepLoadingByKeysAsync(
                new string[] { completeKey },
                new string[] { "완료 중..." },
                new float[] { 1.0f },
                new int[] { 500 },
                0.3f,
                true,
                "loading_success",
                "로딩이 완료되었습니다!"
            );

            Debug.Log($"[GameManager] 완전체 로딩 완료 (비동기): {sceneName}");
        }

        /// <summary>
        /// 완전체 로딩 시스템으로 씬 로드 (로컬라이제이션 + 애니메이션 + 자동 숨김) - 코루틴 버전
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름</param>
        /// <param name="prepareKey">준비 메시지 키</param>
        /// <param name="loadingKey">로딩 메시지 키</param>
        /// <param name="completeKey">완료 메시지 키</param>
        public IEnumerator LoadSceneWithCompleteLoading(string sceneName, string prepareKey = "loading_prepare", string loadingKey = "loading_progress", string completeKey = "loading_complete")
        {
            Debug.Log($"[GameManager] 완전체 로딩 시작: {sceneName}");

            // 1단계: 준비 단계 (0.2초)
            yield return StartCoroutine(Manager.ui.ExecuteStepLoadingByKeys(
                new string[] { prepareKey },
                new string[] { "준비 중..." },
                new float[] { 0.2f },
                new int[] { 200 },
                0.3f,
                false,
                null,
                null
            ));

            // 2단계: 씬 로드 시작
            var loadSceneHandle = Addressables.LoadSceneAsync(sceneName);

            // 3단계: 로딩 진행률 모니터링 (0.8초)
            yield return StartCoroutine(Manager.ui.ExecuteStepLoadingByKeys(
                new string[] { loadingKey },
                new string[] { "로딩 중..." },
                new float[] { 0.8f },
                new int[] { 800 },
                0.4f,
                false,
                null,
                null
            ));

            // 4단계: 씬 로드 완료 대기
            while (!loadSceneHandle.IsDone)
            {
                // 로딩 진행률을 실시간으로 업데이트
                float progress = loadSceneHandle.PercentComplete;
                Manager.ui.SetLoadingProgressAnimated(progress, 0.1f);
                yield return null;
            }

            yield return loadSceneHandle;

            // 5단계: 완료 메시지 표시 및 자동 숨김 (0.5초)
            yield return StartCoroutine(Manager.ui.ExecuteStepLoadingByKeys(
                new string[] { completeKey },
                new string[] { "완료 중..." },
                new float[] { 1.0f },
                new int[] { 500 },
                0.3f,
                true,
                "loading_success",
                "로딩이 완료되었습니다!"
            ));

            Debug.Log($"[GameManager] 완전체 로딩 완료: {sceneName}");
        }


        [ContextMenu("대화 시스템 로드 테스트 tutorial_scn001")]
        public void Temp_DialogueSystemTest()
        {


            //Manager.dialogue.StartDialogueWithPanel("narration", "scn_01", "npc000_quest_Tuto01");
        }


        [ContextMenu("대화 시스템 로드 테스트 NPC002")]
        public void Temp_DialogueSystemTest2()
        {
            //Manager.dialogue.StartDialogueWithPanel("npc002", "stage_01", "npc000_quest_Tuto03");
        }

        [ContextMenu("대화 시스템 로드 테스트 NPC003")]
        public void Temp_DialogueSystemTest3()
        {
            Manager.dialogue.StartDialogueWithPanel("npc003", "stage_02");
        }

        [ContextMenu("대화 시스템 로드 테스트 NPC004")]
        public void Temp_DialogueSystemTest4()
        {
            Manager.dialogue.StartDialogueWithPanel("npc004", "", "npc004_start");
        }

        [ContextMenu("대화 시스템 로드 테스트 NPC001")]
        public void Temp_DialogueSystemTest5()
        {
            Manager.dialogue.StartDialogueWithPanel("npc001", "stage_01", "npc001_start");
        }

        [ContextMenu("TutorialPopUpAfterActionClose2")]
        public  void Temp_TutorialPopUpTest2()
        {
            
             Manager.dialogue.ShowTutorialPopUp("npc001_quest0001", TutorialPopUp.TutorialPositionType.Top);
        }


        [ContextMenu("ShowHUDUI 활용 기본 UI 활성화")]
        public void Temp_ShowHUDUI()
        {

            UIManager.Instance.ShowHUDUI<HUDAllPanel>();
        }

        [ContextMenu("ShowHUDUI 활용 Toturial UI 활성화")]
        public void Temp_ShowTutorialHUD()
        {
            // HUDAllPanel 활성화
            UIManager.Instance.ShowHUDUI<HUDAllPanel>();

            // HUDAllPanel 찾기 (GetHUDUI 대신 직접 찾기)
            HUDAllPanel hudAllPanel = UIManager.Instance.HUDCanvas.GetComponentInChildren<HUDAllPanel>();

            if (hudAllPanel != null)
            {
                hudAllPanel.SwitchToTutorialProgressMode();
            }
            else
            {
                Debug.LogError("[AddressableSceneLoadingManager] HUDAllPanel을 찾을 수 없습니다.");
            }
        }

        [ContextMenu("ShowHUDUI 활용 일반 UI 활성화")]
        public void Temp_HideHUDUI()
        {
            // HUDAllPanel 활성화
            UIManager.Instance.ShowHUDUI<HUDAllPanel>();

            // HUDAllPanel 찾기 (GetHUDUI 대신 직접 찾기)
            HUDAllPanel hudAllPanel = UIManager.Instance.HUDCanvas.GetComponentInChildren<HUDAllPanel>();

            if (hudAllPanel != null)
            {
                hudAllPanel.SwitchToNormalMode();
            }
            else
            {
                Debug.LogError("[AddressableSceneLoadingManager] HUDAllPanel을 찾을 수 없습니다.");
            }
        }



        [ContextMenu("CheckPopUp 메시지 주입 확인")]
        public void OnSaveButtonClicked()
        {
            Manager.ui.ShowConfirmPopUpAsync("저장하시겠습니까?", "저장", "취소",
                () =>
                {
                    Debug.Log("저장 실행");
                    // 저장 로직
                },
                () =>
                {
                    Debug.Log("저장 취소");
                });
        }

        [ContextMenu("MessagePopUp 메시지 주입 확인")]
        public void OnMessagePopUpTest()
        {
            // MessagePopUp 테스트용 메서드
            UIManager.Instance.ShowMessagePopUpAsync("메시지 내용", () =>
            {
                Debug.Log("팝업이 닫혔습니다.");
            });


        }

        [ContextMenu("ShowPopUpAsync 메시지 키로 주입 확인")]
        public void OnMessagePopUpTest2()
        {
            // MessagePopUp 테스트용 메서드
            Manager.ui.ShowMessagePopUpWithKeyAsync("stage_prepare", () =>
            {
                Debug.Log("이게 되네");
            });
        }

        [ContextMenu("ShowPanel PlayerUpgradePanel")]
        public void OnPlayerUpgradePanelTest()
        {
            // PlayerUpgradePanel 테스트용 메서드
            UIManager.Instance.ShowPanelAsync<PlayerUpgradePanel>();
        }


        [ContextMenu("ShowDialogue UpgradeButton")]
        public void OnDialogueUpgradeButtonTest()
        {
            // DialoguePanel의 UpgradeButton 테스트용 메서드
            Manager.dialogue.StartDialogueWithPanel("","","Test_upgrade");
        }
        [ContextMenu("ShowDialogue Test_upgrade_choice_1")]
        public void OnDialogueUpgradeChoice1Test()
        {
            // DialoguePanel의 UpgradeButton 테스트용 메서드
            Manager.dialogue.StartDialogueWithPanel("", "", "Test_upgrade_choice_1");
        }

    }
}
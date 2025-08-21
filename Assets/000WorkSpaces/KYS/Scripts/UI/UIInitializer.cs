using UnityEngine;
using System.Collections;


namespace KYS
{
    public class UIInitializer : MonoBehaviour
    {
        [Header("시작 UI 설정")]
        [SerializeField] private bool showMenuOnStart = true;
        [SerializeField] private bool showTitleOnStart = false;
        [SerializeField] private float delayBeforeShow = 0.5f;
        
        [Header("디버그 설정")]
        [SerializeField] private bool enableDebugLog = true;

        private void Start()
        {
            if (enableDebugLog)
                Debug.Log("[UIInitializer] UI 초기화 시작");
                
            StartCoroutine(ShowInitialUI());
        }

        private IEnumerator ShowInitialUI()
        {
            if (enableDebugLog)
                Debug.Log("[UIInitializer] UIManager 초기화 대기 중...");

            // UIManager 초기화 대기
            while (UIManager.Instance == null)
            {
                yield return null;
            }

            if (enableDebugLog)
                Debug.Log("[UIInitializer] UIManager 인스턴스 발견. Canvas 초기화 대기 중...");

            // Canvas 초기화 완료 대기
            while (!UIManager.Instance.AreCanvasesInitialized)
            {
                yield return null;
            }

            if (enableDebugLog)
                Debug.Log("[UIInitializer] Canvas 초기화 완료. 추가 대기 시간...");

            // 추가 대기 시간
            yield return new WaitForSeconds(delayBeforeShow);

            if (enableDebugLog)
                Debug.Log("[UIInitializer] UI 표시 시작...");

            // 시작 UI 표시
            if (showMenuOnStart)
            {
                if (enableDebugLog)
                    Debug.Log("[UIInitializer] MenuPopUp 표시 시도...");
                UIManager.Instance.ShowPopUpAsync<MenuPopUp>((popup) => {
                    if (popup != null)
                    {
                        Debug.Log("[UIInitializer] MenuPopUp 생성 완료");
                    }
                    else
                    {
                        Debug.LogError("[UIInitializer] MenuPopUp 생성 실패");
                    }
                });
            }
            
            if (showTitleOnStart)
            {
                if (enableDebugLog)
                    Debug.Log("[UIInitializer] TitlePanel 표시 시도...");
                UIManager.Instance.ShowPopUpAsync<TitlePanel>((panel) => {
                    if (panel != null)
                    {
                        Debug.Log("[UIInitializer] TitlePanel 생성 완료");
                    }
                    else
                    {
                        Debug.LogError("[UIInitializer] TitlePanel 생성 실패");
                    }
                });
            }

            if (enableDebugLog)
                Debug.Log("[UIInitializer] UI 초기화 완료");
        }

        // 테스트용 버튼 메서드들
        [ContextMenu("테스트 - 메뉴 팝업 표시")]
        public void TestShowMenuPopUp()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPopUpAsync<MenuPopUp>((popup) => {
                    if (popup != null)
                    {
                        Debug.Log("[UIInitializer] 테스트: 메뉴 팝업 생성 완료");
                    }
                    else
                    {
                        Debug.LogError("[UIInitializer] 테스트: 메뉴 팝업 생성 실패");
                    }
                });
            }
        }

        [ContextMenu("테스트 - 타이틀 패널 표시")]
        public void TestShowTitlePanel()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPopUpAsync<TitlePanel>((panel) => {
                    if (panel != null)
                    {
                        Debug.Log("[UIInitializer] 테스트: 타이틀 패널 생성 완료");
                    }
                    else
                    {
                        Debug.LogError("[UIInitializer] 테스트: 타이틀 패널 생성 실패");
                    }
                });
            }
        }

        [ContextMenu("테스트 - 모든 패널 닫기")]
        public void TestCloseAllPanels()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseAllPanels();
                Debug.Log("[UIInitializer] 테스트: 모든 패널 닫기");
            }
        }

        [ContextMenu("테스트 - 스택 상태 확인")]
        public void TestDebugStackStatus()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.DebugStackStatus();
            }
        }

        [ContextMenu("테스트 - 로컬라이제이션 상태 확인")]
        public void TestLocalizationStatus()
        {
            if (Manager.localization != null)
            {
                Manager.localization.PrintLocalizationInfo();
            }
            else
            {
                Debug.LogError("[UIInitializer] LocalizationManager가 null입니다.");
            }
        }

        [ContextMenu("테스트 - 한국어로 강제 설정")]
        public void TestForceKoreanLanguage()
        {
            if (Manager.localization != null)
            {
                Manager.localization.SetLanguage(SystemLanguage.Korean);
                Debug.Log("[UIInitializer] 한국어로 강제 설정 완료");
            }
            else
            {
                Debug.LogError("[UIInitializer] LocalizationManager가 null입니다.");
            }
        }

        [ContextMenu("테스트 - 영어로 강제 설정")]
        public void TestForceEnglishLanguage()
        {
            if (Manager.localization != null)
            {
                Manager.localization.SetLanguage(SystemLanguage.English);
                Debug.Log("[UIInitializer] 영어로 강제 설정 완료");
            }
            else
            {
                Debug.LogError("[UIInitializer] LocalizationManager가 null입니다.");
            }
        }

        [ContextMenu("테스트 - SafeAreaPanel 상태 확인")]
        public void TestSafeAreaPanelStatus()
        {
            if (UIManager.Instance != null && UIManager.Instance.safeAreaManager != null)
            {
                UIManager.Instance.safeAreaManager.CheckSafeAreaPanelDetails();
            }
            else
            {
                Debug.LogError("[UIInitializer] SafeAreaManager가 null입니다.");
            }
        }

        [ContextMenu("테스트 - SafeAreaPanel 강제 생성")]
        public void TestForceCreateSafeAreaPanel()
        {
            if (UIManager.Instance != null && UIManager.Instance.safeAreaManager != null)
            {
                Canvas panelCanvas = UIManager.Instance.PanelCanvas;
                if (panelCanvas != null)
                {
                    Debug.Log("[UIInitializer] PanelCanvas에 SafeAreaPanel 강제 생성 시도");
                    UIManager.Instance.safeAreaManager.ApplySafeAreaToCanvas(panelCanvas);
                }
                else
                {
                    Debug.LogError("[UIInitializer] PanelCanvas가 null입니다.");
                }
            }
            else
            {
                Debug.LogError("[UIInitializer] SafeAreaManager가 null입니다.");
            }
        }

        [ContextMenu("테스트 - TitlePanel 부모 확인")]
        public void TestTitlePanelParentCheck()
        {
            if (UIManager.Instance != null)
            {
                Transform targetParent = UIManager.Instance.GetCanvasForUI<TitlePanel>();
                if (targetParent != null)
                {
                    Debug.Log($"[UIInitializer] TitlePanel 부모: {targetParent.name}");
                    Debug.Log($"[UIInitializer] 부모가 SafeAreaPanel인가? {targetParent.GetComponent<SafeAreaPanel>() != null}");
                    Debug.Log($"[UIInitializer] 부모가 Canvas인가? {targetParent.GetComponent<Canvas>() != null}");
                }
                else
                {
                    Debug.LogError("[UIInitializer] TitlePanel 부모를 찾을 수 없습니다.");
                }
            }
        }



        [ContextMenu("테스트 - 터치 차단 상태 확인")]
        public void TestTouchBlockingStatus()
        {
            if (UIManager.Instance != null)
            {
                Debug.Log("[UIInitializer] === 터치 차단 상태 확인 ===");
                
                // HUD Canvas 터치 상태 확인
                if (UIManager.Instance.HUDCanvas != null)
                {
                    CanvasGroup hudCanvasGroup = UIManager.Instance.HUDCanvas.GetComponent<CanvasGroup>();
                    if (hudCanvasGroup != null)
                    {
                        Debug.Log($"[UIInitializer] HUD Canvas 터치 상태: interactable={hudCanvasGroup.interactable}, blocksRaycasts={hudCanvasGroup.blocksRaycasts}");
                    }
                }
                
                // Panel Canvas 터치 상태 확인
                if (UIManager.Instance.PanelCanvas != null)
                {
                    CanvasGroup panelCanvasGroup = UIManager.Instance.PanelCanvas.GetComponent<CanvasGroup>();
                    if (panelCanvasGroup != null)
                    {
                        Debug.Log($"[UIInitializer] Panel Canvas 터치 상태: interactable={panelCanvasGroup.interactable}, blocksRaycasts={panelCanvasGroup.blocksRaycasts}");
                    }
                }
                
                // Popup Canvas 터치 상태 확인
                if (UIManager.Instance.PopupCanvas != null)
                {
                    CanvasGroup popupCanvasGroup = UIManager.Instance.PopupCanvas.GetComponent<CanvasGroup>();
                    if (popupCanvasGroup != null)
                    {
                        Debug.Log($"[UIInitializer] Popup Canvas 터치 상태: interactable={popupCanvasGroup.interactable}, blocksRaycasts={popupCanvasGroup.blocksRaycasts}");
                    }
                }
            }
            else
            {
                Debug.LogError("[UIInitializer] UIManager가 null입니다.");
            }
        }
    }
}

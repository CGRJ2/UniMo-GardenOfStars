using UnityEngine;
using UnityEngine.EventSystems;

namespace KYS
{
    /// <summary>
    /// MenuPopUp의 비즈니스 로직을 담당하는 Presenter
    /// </summary>
    public class MenuPopUpPresenter : BaseUIPresenter
    {
        private MenuPopUpModel menuModel;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            menuModel = model as MenuPopUpModel;
            
            // 이벤트 구독
            if (view is MenuPopUp menuView)
            {
                menuView.OnStartButtonClicked += HandleStartButtonClicked;
                menuView.OnSettingsButtonClicked += HandleSettingsButtonClicked;
                menuView.OnExitButtonClicked += HandleExitButtonClicked;
                menuView.OnCloseButtonClicked += HandleCloseButtonClicked;
            }
        }
        
        protected override void OnCleanup()
        {
            base.OnCleanup();
            
            // 이벤트 구독 해제
            if (view is MenuPopUp menuView)
            {
                menuView.OnStartButtonClicked -= HandleStartButtonClicked;
                menuView.OnSettingsButtonClicked -= HandleSettingsButtonClicked;
                menuView.OnExitButtonClicked -= HandleExitButtonClicked;
                menuView.OnCloseButtonClicked -= HandleCloseButtonClicked;
            }
        }
        
        #region Event Handlers
        
        private void HandleStartButtonClicked(PointerEventData data)
        {
            Debug.Log("[MenuPopUpPresenter] 시작 버튼 클릭 처리");
            
            // 게임 시작 로직
            menuModel?.SetGameState(GameState.Starting);
            
            // UI 닫기
            UIManager.Instance.ClosePopup();
            
            // 게임 시작 이벤트 발생 (실제 구현 시 사용)
            // GameEvents.OnGameStart?.Invoke();
        }
        
        private void HandleSettingsButtonClicked(PointerEventData data)
        {
            Debug.Log("[MenuPopUpPresenter] 설정 버튼 클릭 처리");

            // 설정 팝업 열기 (실제 구현 시 사용)
            // UIManager.Instance.ShowPopUpAsync<SettingsPopUp>();

            Manager.ui.ShowPopUpAsync<LanguageSettingsPanel>();
        }
        
        private void HandleExitButtonClicked(PointerEventData data)
        {
            Debug.Log("[MenuPopUpPresenter] 종료 버튼 클릭 처리");
            
            // 종료 확인 팝업 표시
            CheckPopUp.ShowCheckPopUp(
                "게임을 종료하시겠습니까?",
                "종료",
                "취소",
                () => {
                    Debug.Log("[MenuPopUpPresenter] 게임 종료 확인");
                    Application.Quit();
                },
                null
            );
        }
        
        private void HandleCloseButtonClicked(PointerEventData data)
        {
            Debug.Log("[MenuPopUpPresenter] 닫기 버튼 클릭 처리");
            
            // 팝업 닫기
            UIManager.Instance.ClosePopup();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 메뉴 상태 업데이트
        /// </summary>
        public void UpdateMenuState()
        {
            if (menuModel != null)
            {
                // 메뉴 상태에 따른 UI 업데이트
                bool canStart = menuModel.CanStartGame();
                bool canSettings = menuModel.CanOpenSettings();
                
                if (view is MenuPopUp menuView)
                {
                    menuView.SetStartButtonInteractable(canStart);
                    menuView.SetSettingsButtonInteractable(canSettings);
                }
            }
        }
        
        #endregion
    }
}

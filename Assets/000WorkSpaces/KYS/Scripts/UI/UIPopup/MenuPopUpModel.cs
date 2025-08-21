using UnityEngine;

namespace KYS
{
    /// <summary>
    /// MenuPopUp의 데이터와 상태를 관리하는 Model
    /// </summary>
    public class MenuPopUpModel : BaseUIModel
    {
        [Header("Menu State")]
        [SerializeField] private GameState currentGameState = GameState.MainMenu;
        [SerializeField] private bool isGameReady = false;
        [SerializeField] private bool isSettingsAvailable = true;
        
        [Header("Menu Data")]
        [SerializeField] private string lastPlayedLevel = "";
        [SerializeField] private float lastPlayTime = 0f;
        
        // 이벤트
        public System.Action<GameState> OnGameStateChanged;
        public System.Action<bool> OnGameReadyChanged;
        
        #region Properties
        
        public GameState CurrentGameState => currentGameState;
        public bool IsGameReady => isGameReady;
        public bool IsSettingsAvailable => isSettingsAvailable;
        public string LastPlayedLevel => lastPlayedLevel;
        public float LastPlayTime => lastPlayTime;
        
        #endregion
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 저장된 데이터 로드
            LoadMenuData();
            
            // 게임 상태 확인
            CheckGameState();
        }
        
        protected override void OnCleanup()
        {
            base.OnCleanup();
            
            // 데이터 저장
            SaveMenuData();
        }
        
        #region Game State Management
        
        /// <summary>
        /// 게임 상태 설정
        /// </summary>
        public void SetGameState(GameState newState)
        {
            if (currentGameState != newState)
            {
                currentGameState = newState;
                OnGameStateChanged?.Invoke(newState);
                
                Debug.Log($"[MenuPopUpModel] 게임 상태 변경: {newState}");
            }
        }
        
        /// <summary>
        /// 게임 준비 상태 설정
        /// </summary>
        public void SetGameReady(bool ready)
        {
            if (isGameReady != ready)
            {
                isGameReady = ready;
                OnGameReadyChanged?.Invoke(ready);
                
                Debug.Log($"[MenuPopUpModel] 게임 준비 상태: {ready}");
            }
        }
        
        /// <summary>
        /// 설정 사용 가능 여부 설정
        /// </summary>
        public void SetSettingsAvailable(bool available)
        {
            isSettingsAvailable = available;
        }
        
        #endregion
        
        #region Business Logic
        
        /// <summary>
        /// 게임 시작 가능 여부 확인
        /// </summary>
        public bool CanStartGame()
        {
            return isGameReady && currentGameState == GameState.MainMenu;
        }
        
        /// <summary>
        /// 설정 열기 가능 여부 확인
        /// </summary>
        public bool CanOpenSettings()
        {
            return isSettingsAvailable;
        }
        
        /// <summary>
        /// 게임 종료 가능 여부 확인
        /// </summary>
        public bool CanExitGame()
        {
            return currentGameState != GameState.Playing;
        }
        
        /// <summary>
        /// 게임 상태 확인
        /// </summary>
        private void CheckGameState()
        {
            // 게임 매니저에서 상태 확인 (실제 구현 시 사용)
            // if (Manager.game != null)
            // {
            //     SetGameReady(Manager.game.IsGameReady());
            // }
            
            // 임시로 게임 준비 상태를 true로 설정
            SetGameReady(true);
            
            // 설정 사용 가능 여부 확인
            SetSettingsAvailable(true); // 실제로는 설정 파일이나 서버에서 확인
        }
        
        #endregion
        
        #region Data Management
        
        /// <summary>
        /// 메뉴 데이터 로드
        /// </summary>
        private void LoadMenuData()
        {
            lastPlayedLevel = PlayerPrefs.GetString("LastPlayedLevel", "");
            lastPlayTime = PlayerPrefs.GetFloat("LastPlayTime", 0f);
            
            Debug.Log($"[MenuPopUpModel] 메뉴 데이터 로드: Level={lastPlayedLevel}, Time={lastPlayTime}");
        }
        
        /// <summary>
        /// 메뉴 데이터 저장
        /// </summary>
        private void SaveMenuData()
        {
            PlayerPrefs.SetString("LastPlayedLevel", lastPlayedLevel);
            PlayerPrefs.SetFloat("LastPlayTime", lastPlayTime);
            PlayerPrefs.Save();
            
            Debug.Log($"[MenuPopUpModel] 메뉴 데이터 저장 완료");
        }
        
        /// <summary>
        /// 마지막 플레이 정보 업데이트
        /// </summary>
        public void UpdateLastPlayInfo(string level, float playTime)
        {
            lastPlayedLevel = level;
            lastPlayTime = playTime;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 메뉴 새로고침
        /// </summary>
        public void RefreshMenu()
        {
            LoadMenuData();
            CheckGameState();
        }
        
        /// <summary>
        /// 메뉴 데이터 초기화
        /// </summary>
        public void ResetMenuData()
        {
            lastPlayedLevel = "";
            lastPlayTime = 0f;
            SaveMenuData();
            
            Debug.Log("[MenuPopUpModel] 메뉴 데이터 초기화 완료");
        }
        
        #endregion
    }
    
    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Starting,
        Playing,
        Paused,
        GameOver,
        Victory
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace KYS
{
    /// <summary>
    /// HUD 레이어 예시 - 재화 표시, 기본 UI 패널 및 버튼들
    /// </summary>
    public class HUDAllPanel : BaseUI
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private TextMeshProUGUI questProgressText;

        // MVP Components
        private new HUDAllPanelPresenter presenter;
        private new HUDAllPanelModel model;
        
        protected override void SetupMVP()
        {
            base.SetupMVP();
            
            // Presenter와 Model 설정
            presenter = GetComponent<HUDAllPanelPresenter>();
            if (presenter == null)
            {
                presenter = gameObject.AddComponent<HUDAllPanelPresenter>();
            }
            
            model = GetComponentInChildren<HUDAllPanelModel>();
            if (model == null)
            {
                GameObject modelObj = new GameObject("HUDAllPanelModel");
                modelObj.transform.SetParent(transform);
                model = modelObj.AddComponent<HUDAllPanelModel>();
            }
            
            SetPresenter(presenter);
            SetModel(model);
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // UI 이벤트 설정
            if (menuButton != null)
            {
                GetEventWithSFX("MenuButton").Click += (data) => OnMenuButtonClicked();
            }
            
            if (inventoryButton != null)
            {
                GetEventWithSFX("InventoryButton").Click += (data) => OnInventoryButtonClicked();
            }
            
            // UIManager에 등록
            UIManager.Instance.RegisterUI(this);
        }
        
        public override void Cleanup()
        {
            base.Cleanup();
            
            // UIManager에서 제거
            UIManager.Instance.UnregisterUI(this);
        }
        
        #region UI Update Methods
        
        public void UpdateMoney(int amount)
        {
            if (moneyText != null)
            {
                moneyText.text = $"재화: {amount:N0}";
            }
        }
        
        public void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"레벨: {level}";
            }
        }

        public void UpdateQuestProgress(string progress)
        {
            if (questProgressText != null)
            {
                questProgressText.text = $"퀘스트 진행: {progress}";
            }
        }

        #endregion

        #region Event Handlers

        private void OnMenuButtonClicked()
        {
            Debug.Log("[HUDAllPanel] 메뉴 버튼 클릭");
            // 메뉴 팝업 열기
            UIManager.Instance.ShowPopUp<MenuPopUp>();
        }
        
        private void OnInventoryButtonClicked()
        {
            Debug.Log("[HUDAllPanel] 인벤토리 버튼 클릭");

        }
        
        #endregion
    }
    
    /// <summary>
    /// HUD Example Presenter
    /// </summary>
    public class HUDAllPanelPresenter : BaseUIPresenter
    {
        private HUDAllPanel hudView;
        private HUDAllPanelModel hudModel;
        
        protected override void OnInitialize()
        {
            hudView = GetView<HUDAllPanel>();
            hudModel = GetModel<HUDAllPanelModel>();
            
            // Model 이벤트 구독
            if (hudModel != null)
            {
                hudModel.OnMoneyChanged += OnMoneyChanged;
                hudModel.OnLevelChanged += OnLevelChanged;
                hudModel.OnQuestProgressChanged += OnQuestProgressChanged;
            }
        }
        
        protected override void OnCleanup()
        {
            // Model 이벤트 구독 해제
            if (hudModel != null)
            {
                hudModel.OnMoneyChanged -= OnMoneyChanged;
                hudModel.OnLevelChanged -= OnLevelChanged;
                hudModel.OnQuestProgressChanged -= OnQuestProgressChanged;
            }
        }
        
        private void OnMoneyChanged(int amount)
        {
            hudView?.UpdateMoney(amount);
        }
        
        private void OnLevelChanged(int level)
        {
            hudView?.UpdateLevel(level);
        }

        private void OnQuestProgressChanged(string progress)
        {
            hudView?.UpdateQuestProgress(progress);
        }

    }
    
    /// <summary>
    /// HUD Example Model
    /// </summary>
    public class HUDAllPanelModel : BaseUIModel
    {
        private int money = 1000;
        private int level = 1;
        
        public System.Action<int> OnMoneyChanged;
        public System.Action<int> OnLevelChanged;
        public System.Action<string> OnQuestProgressChanged;

        public int Money
        {
            get => money;
            set
            {
                if (money != value)
                {
                    money = value;
                    OnMoneyChanged?.Invoke(money);
                }
            }
        }
        
        public int Level
        {
            get => level;
            set
            {
                if (level != value)
                {
                    level = value;
                    OnLevelChanged?.Invoke(level);
                }
            }
        }
        
        protected override void OnInitialize()
        {
            // 초기 데이터 로드
            LoadHUDData();
        }
        
        private void LoadHUDData()
        {
            // 실제로는 PlayerPrefs나 데이터베이스에서 로드
            Money = PlayerPrefs.GetInt("PlayerMoney", 1000);
            Level = PlayerPrefs.GetInt("PlayerLevel", 1);
        }
        
        public void AddMoney(int amount)
        {
            Money += amount;
            PlayerPrefs.SetInt("PlayerMoney", Money);
        }
        
        public void AddExperience(int exp)
        {
            // 레벨업 로직
            int newLevel = Mathf.FloorToInt(exp / 100f) + 1;
            if (newLevel > Level)
            {
                Level = newLevel;
                PlayerPrefs.SetInt("PlayerLevel", Level);
            }
        }
    }
}

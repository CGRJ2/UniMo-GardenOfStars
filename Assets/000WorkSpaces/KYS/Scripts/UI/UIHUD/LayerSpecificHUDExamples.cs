using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KYS
{
    /// <summary>
    /// 건물 정보 전용 HUD
    /// </summary>
    public class BuildingInfoHUD : TouchInfoHUD
    {
        [Header("Building Specific UI Elements")]
        [SerializeField] private string buildingTypeTextName = "BuildingTypeText";
        [SerializeField] private string buildingLevelTextName = "BuildingLevelText";
        [SerializeField] private string buildingEfficiencyTextName = "BuildingEfficiencyText";
        [SerializeField] private string buildingStatusTextName = "BuildingStatusText";
        [SerializeField] private string upgradeButtonName = "UpgradeButton";
        [SerializeField] private string demolishButtonName = "DemolishButton";
        
        private TextMeshProUGUI buildingTypeText => GetUI<TextMeshProUGUI>(buildingTypeTextName);
        private TextMeshProUGUI buildingLevelText => GetUI<TextMeshProUGUI>(buildingLevelTextName);
        private TextMeshProUGUI buildingEfficiencyText => GetUI<TextMeshProUGUI>(buildingEfficiencyTextName);
        private TextMeshProUGUI buildingStatusText => GetUI<TextMeshProUGUI>(buildingStatusTextName);
        private Button upgradeButton => GetUI<Button>(upgradeButtonName);
        private Button demolishButton => GetUI<Button>(demolishButtonName);
        
        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "building_type",
                "building_level",
                "building_efficiency",
                "building_status",
                "upgrade_button",
                "demolish_button"
            };
        }
        
        public override void Initialize()
        {
            base.Initialize();
            SetupBuildingButtons();
        }
        
        private void SetupBuildingButtons()
        {
            // 건물별 특정 버튼 설정
            var upgradeEventHandler = GetEventWithSFX(upgradeButtonName, "SFX_ButtonClick");
            if (upgradeEventHandler != null)
            {
                upgradeEventHandler.Click += (data) => OnUpgradeButtonClicked();
            }
            
            var demolishEventHandler = GetEventWithSFX(demolishButtonName, "SFX_ButtonClick");
            if (demolishEventHandler != null)
            {
                demolishEventHandler.Click += (data) => OnDemolishButtonClicked();
            }
        }
        
        /// <summary>
        /// 건물 정보 설정
        /// </summary>
        public void SetBuildingInfo(string buildingType, int level, float efficiency, string status)
        {
            if (buildingTypeText != null)
                buildingTypeText.text = $"타입: {buildingType}";
            
            if (buildingLevelText != null)
                buildingLevelText.text = $"레벨: {level}";
            
            if (buildingEfficiencyText != null)
                buildingEfficiencyText.text = $"효율: {efficiency:P0}";
            
            if (buildingStatusText != null)
                buildingStatusText.text = $"상태: {status}";
        }
        
        private void OnUpgradeButtonClicked()
        {
            Debug.Log("[BuildingInfoHUD] 건물 업그레이드 버튼 클릭");
            // 건물 업그레이드 로직
        }
        
        private void OnDemolishButtonClicked()
        {
            Debug.Log("[BuildingInfoHUD] 건물 철거 버튼 클릭");
            // 건물 철거 로직
        }
    }
    
    /// <summary>
    /// 작업자 정보 전용 HUD
    /// </summary>
    public class WorkerInfoHUD : TouchInfoHUD
    {
        [Header("Worker Specific UI Elements")]
        [SerializeField] private string workerLevelTextName = "WorkerLevelText";
        [SerializeField] private string workerSkillTextName = "WorkerSkillText";
        [SerializeField] private string workerHappinessTextName = "WorkerHappinessText";
        [SerializeField] private string workerSalaryTextName = "WorkerSalaryText";
        [SerializeField] private string hireButtonName = "HireButton";
        [SerializeField] private string fireButtonName = "FireButton";
        [SerializeField] private string trainButtonName = "TrainButton";
        
        private TextMeshProUGUI workerLevelText => GetUI<TextMeshProUGUI>(workerLevelTextName);
        private TextMeshProUGUI workerSkillText => GetUI<TextMeshProUGUI>(workerSkillTextName);
        private TextMeshProUGUI workerHappinessText => GetUI<TextMeshProUGUI>(workerHappinessTextName);
        private TextMeshProUGUI workerSalaryText => GetUI<TextMeshProUGUI>(workerSalaryTextName);
        private Button hireButton => GetUI<Button>(hireButtonName);
        private Button fireButton => GetUI<Button>(fireButtonName);
        private Button trainButton => GetUI<Button>(trainButtonName);
        
        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "worker_level",
                "worker_skill",
                "worker_happiness",
                "worker_salary",
                "hire_button",
                "fire_button",
                "train_button"
            };
        }
        
        public override void Initialize()
        {
            base.Initialize();
            SetupWorkerButtons();
        }
        
        private void SetupWorkerButtons()
        {
            var hireEventHandler = GetEventWithSFX(hireButtonName, "SFX_ButtonClick");
            if (hireEventHandler != null)
            {
                hireEventHandler.Click += (data) => OnHireButtonClicked();
            }
            
            var fireEventHandler = GetEventWithSFX(fireButtonName, "SFX_ButtonClick");
            if (fireEventHandler != null)
            {
                fireEventHandler.Click += (data) => OnFireButtonClicked();
            }
            
            var trainEventHandler = GetEventWithSFX(trainButtonName, "SFX_ButtonClick");
            if (trainEventHandler != null)
            {
                trainEventHandler.Click += (data) => OnTrainButtonClicked();
            }
        }
        
        /// <summary>
        /// 작업자 정보 설정
        /// </summary>
        public void SetWorkerInfo(int level, string skill, float happiness, int salary)
        {
            if (workerLevelText != null)
                workerLevelText.text = $"레벨: {level}";
            
            if (workerSkillText != null)
                workerSkillText.text = $"기술: {skill}";
            
            if (workerHappinessText != null)
                workerHappinessText.text = $"만족도: {happiness:P0}";
            
            if (workerSalaryText != null)
                workerSalaryText.text = $"급여: {salary:N0}";
        }
        
        private void OnHireButtonClicked()
        {
            Debug.Log("[WorkerInfoHUD] 작업자 고용 버튼 클릭");
            // 작업자 고용 로직
        }
        
        private void OnFireButtonClicked()
        {
            Debug.Log("[WorkerInfoHUD] 작업자 해고 버튼 클릭");
            // 작업자 해고 로직
        }
        
        private void OnTrainButtonClicked()
        {
            Debug.Log("[WorkerInfoHUD] 작업자 훈련 버튼 클릭");
            // 작업자 훈련 로직
        }
    }
    
    /// <summary>
    /// 자원 정보 전용 HUD
    /// </summary>
    public class ResourceInfoHUD : TouchInfoHUD
    {
        [Header("Resource Specific UI Elements")]
        [SerializeField] private string resourceTypeTextName = "ResourceTypeText";
        [SerializeField] private string resourceQuantityTextName = "ResourceQuantityText";
        [SerializeField] private string resourceQualityTextName = "ResourceQualityText";
        [SerializeField] private string resourceStatusTextName = "ResourceStatusText";
        [SerializeField] private string mineButtonName = "MineButton";
        [SerializeField] private string transportButtonName = "TransportButton";
        
        private TextMeshProUGUI resourceTypeText => GetUI<TextMeshProUGUI>(resourceTypeTextName);
        private TextMeshProUGUI resourceQuantityText => GetUI<TextMeshProUGUI>(resourceQuantityTextName);
        private TextMeshProUGUI resourceQualityText => GetUI<TextMeshProUGUI>(resourceQualityTextName);
        private TextMeshProUGUI resourceStatusText => GetUI<TextMeshProUGUI>(resourceStatusTextName);
        private Button mineButton => GetUI<Button>(mineButtonName);
        private Button transportButton => GetUI<Button>(transportButtonName);
        
        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "resource_type",
                "resource_quantity",
                "resource_quality",
                "resource_status",
                "mine_button",
                "transport_button"
            };
        }
        
        public override void Initialize()
        {
            base.Initialize();
            SetupResourceButtons();
        }
        
        private void SetupResourceButtons()
        {
            var mineEventHandler = GetEventWithSFX(mineButtonName, "SFX_ButtonClick");
            if (mineEventHandler != null)
            {
                mineEventHandler.Click += (data) => OnMineButtonClicked();
            }
            
            var transportEventHandler = GetEventWithSFX(transportButtonName, "SFX_ButtonClick");
            if (transportEventHandler != null)
            {
                transportEventHandler.Click += (data) => OnTransportButtonClicked();
            }
        }
        
        /// <summary>
        /// 자원 정보 설정
        /// </summary>
        public void SetResourceInfo(string resourceType, int quantity, float quality, string status)
        {
            if (resourceTypeText != null)
                resourceTypeText.text = $"타입: {resourceType}";
            
            if (resourceQuantityText != null)
                resourceQuantityText.text = $"수량: {quantity:N0}";
            
            if (resourceQualityText != null)
                resourceQualityText.text = $"품질: {quality:P0}";
            
            if (resourceStatusText != null)
                resourceStatusText.text = $"상태: {status}";
        }
        
        private void OnMineButtonClicked()
        {
            Debug.Log("[ResourceInfoHUD] 자원 채굴 버튼 클릭");
            // 자원 채굴 로직
        }
        
        private void OnTransportButtonClicked()
        {
            Debug.Log("[ResourceInfoHUD] 자원 운송 버튼 클릭");
            // 자원 운송 로직
        }
    }
    
    /// <summary>
    /// 시설 정보 전용 HUD
    /// </summary>
    public class FacilityInfoHUD : TouchInfoHUD
    {
        [Header("Facility Specific UI Elements")]
        [SerializeField] private string facilityTypeTextName = "FacilityTypeText";
        [SerializeField] private string facilityCapacityTextName = "FacilityCapacityText";
        [SerializeField] private string facilityUsageTextName = "FacilityUsageText";
        [SerializeField] private string facilityStatusTextName = "FacilityStatusText";
        [SerializeField] private string useButtonName = "UseButton";
        [SerializeField] private string maintainButtonName = "MaintainButton";
        
        private TextMeshProUGUI facilityTypeText => GetUI<TextMeshProUGUI>(facilityTypeTextName);
        private TextMeshProUGUI facilityCapacityText => GetUI<TextMeshProUGUI>(facilityCapacityTextName);
        private TextMeshProUGUI facilityUsageText => GetUI<TextMeshProUGUI>(facilityUsageTextName);
        private TextMeshProUGUI facilityStatusText => GetUI<TextMeshProUGUI>(facilityStatusTextName);
        private Button useButton => GetUI<Button>(useButtonName);
        private Button maintainButton => GetUI<Button>(maintainButtonName);
        
        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "facility_type",
                "facility_capacity",
                "facility_usage",
                "facility_status",
                "use_button",
                "maintain_button"
            };
        }
        
        public override void Initialize()
        {
            base.Initialize();
            SetupFacilityButtons();
        }
        
        private void SetupFacilityButtons()
        {
            var useEventHandler = GetEventWithSFX(useButtonName, "SFX_ButtonClick");
            if (useEventHandler != null)
            {
                useEventHandler.Click += (data) => OnUseButtonClicked();
            }
            
            var maintainEventHandler = GetEventWithSFX(maintainButtonName, "SFX_ButtonClick");
            if (maintainEventHandler != null)
            {
                maintainEventHandler.Click += (data) => OnMaintainButtonClicked();
            }
        }
        
        /// <summary>
        /// 시설 정보 설정
        /// </summary>
        public void SetFacilityInfo(string facilityType, int capacity, int currentUsage, string status)
        {
            if (facilityTypeText != null)
                facilityTypeText.text = $"타입: {facilityType}";
            
            if (facilityCapacityText != null)
                facilityCapacityText.text = $"용량: {capacity:N0}";
            
            if (facilityUsageText != null)
                facilityUsageText.text = $"사용량: {currentUsage:N0}";
            
            if (facilityStatusText != null)
                facilityStatusText.text = $"상태: {status}";
        }
        
        private void OnUseButtonClicked()
        {
            Debug.Log("[FacilityInfoHUD] 시설 이용 버튼 클릭");
            // 시설 이용 로직
        }
        
        private void OnMaintainButtonClicked()
        {
            Debug.Log("[FacilityInfoHUD] 시설 유지보수 버튼 클릭");
            // 시설 유지보수 로직
        }
    }
    
    /// <summary>
    /// NPC 정보 전용 HUD
    /// </summary>
    public class NPCInfoHUD : TouchInfoHUD
    {
        [Header("NPC Specific UI Elements")]
        [SerializeField] private string npcTypeTextName = "NPCTypeText";
        [SerializeField] private string npcReputationTextName = "NPCReputationText";
        [SerializeField] private string npcQuestsTextName = "NPCQuestsText";
        [SerializeField] private string npcStatusTextName = "NPCStatusText";
        [SerializeField] private string talkButtonName = "TalkButton";
        [SerializeField] private string tradeButtonName = "TradeButton";
        [SerializeField] private string questButtonName = "QuestButton";
        
        private TextMeshProUGUI npcTypeText => GetUI<TextMeshProUGUI>(npcTypeTextName);
        private TextMeshProUGUI npcReputationText => GetUI<TextMeshProUGUI>(npcReputationTextName);
        private TextMeshProUGUI npcQuestsText => GetUI<TextMeshProUGUI>(npcQuestsTextName);
        private TextMeshProUGUI npcStatusText => GetUI<TextMeshProUGUI>(npcStatusTextName);
        private Button talkButton => GetUI<Button>(talkButtonName);
        private Button tradeButton => GetUI<Button>(tradeButtonName);
        private Button questButton => GetUI<Button>(questButtonName);
        
        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "npc_type",
                "npc_reputation",
                "npc_quests",
                "npc_status",
                "talk_button",
                "trade_button",
                "quest_button"
            };
        }
        
        public override void Initialize()
        {
            base.Initialize();
            SetupNPCButtons();
        }
        
        private void SetupNPCButtons()
        {
            var talkEventHandler = GetEventWithSFX(talkButtonName, "SFX_ButtonClick");
            if (talkEventHandler != null)
            {
                talkEventHandler.Click += (data) => OnTalkButtonClicked();
            }
            
            var tradeEventHandler = GetEventWithSFX(tradeButtonName, "SFX_ButtonClick");
            if (tradeEventHandler != null)
            {
                tradeEventHandler.Click += (data) => OnTradeButtonClicked();
            }
            
            var questEventHandler = GetEventWithSFX(questButtonName, "SFX_ButtonClick");
            if (questEventHandler != null)
            {
                questEventHandler.Click += (data) => OnQuestButtonClicked();
            }
        }
        
        /// <summary>
        /// NPC 정보 설정
        /// </summary>
        public void SetNPCInfo(string npcType, int reputation, int availableQuests, string status)
        {
            if (npcTypeText != null)
                npcTypeText.text = $"타입: {npcType}";
            
            if (npcReputationText != null)
                npcReputationText.text = $"평판: {reputation}";
            
            if (npcQuestsText != null)
                npcQuestsText.text = $"퀘스트: {availableQuests}개";
            
            if (npcStatusText != null)
                npcStatusText.text = $"상태: {status}";
        }
        
        private void OnTalkButtonClicked()
        {
            Debug.Log("[NPCInfoHUD] NPC 대화 버튼 클릭");
            // NPC 대화 로직
        }
        
        private void OnTradeButtonClicked()
        {
            Debug.Log("[NPCInfoHUD] NPC 거래 버튼 클릭");
            // NPC 거래 로직
        }
        
        private void OnQuestButtonClicked()
        {
            Debug.Log("[NPCInfoHUD] NPC 퀘스트 버튼 클릭");
            // NPC 퀘스트 로직
        }
    }
}

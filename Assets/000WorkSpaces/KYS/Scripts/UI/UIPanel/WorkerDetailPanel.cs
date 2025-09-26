using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KYS
{
    public enum CharacterRanks
    {
        C = 1, B, A, S
    }


    public class WorkerDetailPanel : BaseUI
    {
        private string _closeButtonName = "CloseButton";
        private string _speedUpgradeButtonName = "SpeedUpgradeButton";
        private string _capacityUpgradeButtonName = "CapacityUpgradeButton";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private Image _workerImage => GetUI<Image>("WorkerImage");
        private Button _closeButton => GetUI<Button>(_closeButtonName);
        private Button _speedUpgradeButton => GetUI<Button>(_speedUpgradeButtonName);
        private Button _capacityUpgradeButton => GetUI<Button>(_capacityUpgradeButtonName);
        private TextMeshProUGUI _workerNameText => GetUI<TextMeshProUGUI>("RunWorkerName");
        private TextMeshProUGUI _workerRankText => GetUI<TextMeshProUGUI>("WorkerRankText");
        private TextMeshProUGUI _effectRankText => GetUI<TextMeshProUGUI>("RunStatusEffectRankText");
        private TextMeshProUGUI _productSpeedRankText => GetUI<TextMeshProUGUI>("RunProductSpeedRankText");
        private TextMeshProUGUI _curSpeedText => GetUI<TextMeshProUGUI>("RunCurSpeedText");
        private TextMeshProUGUI _curCapacityText => GetUI<TextMeshProUGUI>("RunCurCapacityText");
        private TextMeshProUGUI _upgradeSpeedText => GetUI<TextMeshProUGUI>("RunUpgradeSpeedText");
        private TextMeshProUGUI _upgradeCapacityText => GetUI<TextMeshProUGUI>("RunUpgradeCapacityText");
        private TextMeshProUGUI _upgradeSpeedCostText => GetUI<TextMeshProUGUI>("UpgradeSpeedCostText");
        private TextMeshProUGUI _upgradeCapacityCostText => GetUI<TextMeshProUGUI>("UpgradeCapacityCostText");
        private Transform moveSpeedBlockContainer => GetUI<Transform>("blockContainer1");
        private Transform capacityBlockContainer => GetUI<Transform>("blockContainer2");
        private WorkerData _worker;

        private int _upgradeSpeedCost;
        private int _upgradeCapacityCost;

        private bool _isInSpeedProgress;
        private bool _isInCapacityProgress;


        [Header("LevelBlock 프리팹")]
        [SerializeField] GameObject levelBlockPrefab;

        // Block 관리용 리스트
        private List<GameObject> moveSpeedBlockList = new List<GameObject>();  // 이동 속도 블록 리스트
        private List<GameObject> capacityBlockList = new List<GameObject>();  // 운반 개수 블록 리스트


        protected override void Awake()
        {
            base.Awake();
            if (TutorialManager.Instance != null)
            {
                BlockAllImages(new() { "SpeedUpgradeButton" });
                TutorialManager.Instance.overlayPanel_HRPanelBtn.SetActive(false);
                TutorialManager.Instance.overlayPanel_WorkerUpgradeBtn.SetActive(true);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _worker.MoveSpeedLv.Unsubscribe(OnSpeedChanged);
            _worker.MaxCapacityLv.Unsubscribe(OnCapacityChanged);
            
            // LevelBlock 정리
            ClearMoveSpeedBlocks();
            ClearCapacityBlocks();
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        public void SetInfo(WorkerData worker)
        {
            _worker = worker;

            _worker.MoveSpeedLv.Unsubscribe(OnSpeedChanged);
            _worker.MaxCapacityLv.Unsubscribe(OnCapacityChanged);
            _worker.MoveSpeedLv.Subscribe(OnSpeedChanged);
            _worker.MaxCapacityLv.Subscribe(OnCapacityChanged);

            _workerImage.sprite = _worker.Sprite;

            _workerNameText.text = _worker.Name;
            _workerRankText.text = ((CharacterRanks)_worker.Rank).ToString();

            _effectRankText.text = _worker.StunRank;
            _productSpeedRankText.text = _worker.ProductionSpeedRank;

            UpdateSpeedInfo();
            UpdateCapacityInfo();

            // LevelBlock 업데이트
            UpdateMoveSpeedBlockLevels(_worker.MoveSpeedLv.Value, _worker.MoveSpeedMaxLv);
            UpdateCapacityBlockLevels(_worker.MaxCapacityLv.Value, _worker.MaxCapacityMaxLv);

            _worker.MoveSpeedLv.Subscribe(UpdateSpeedInfo);
            _worker.MaxCapacityLv.Subscribe(UpdateCapacityInfo);
        }

        private void UpdateSpeedInfo(int value = 0)
        {
            _curSpeedText.text = _worker.MoveSpeed.ToString();

            if (_worker.IsMoveSpeedMaxLv)
            {
                _upgradeSpeedText.text = "Max";
                _upgradeSpeedCostText.text = "Max";
            }
            else
            {
                _upgradeSpeedText.text = Manager.data.CharacterLv
                    .Values[(_worker.MoveSpeedLv.Value + 1).ToString()].Speed.ToString();

                int cost = Manager.data.WorkerUpgradeCost.Values[$"{(_worker.MoveSpeedLv.Value + 1)}_{_worker.Rank}"].Speed;

                if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
                {
                    cost = 0;
                }

                _upgradeSpeedCost = (int)(cost);

                _upgradeSpeedCostText.text = _upgradeSpeedCost.ToString();
            }

            // LevelBlock 업데이트
            UpdateMoveSpeedBlockLevels(_worker.MoveSpeedLv.Value, _worker.MoveSpeedMaxLv);

            _speedUpgradeButton.interactable = true;
        }

        private void UpdateCapacityInfo(int value = 0)
        {
            _curCapacityText.text = _worker.MaxCapacity.ToString();

            if (_worker.IsMaxCapacityMaxLv)
            {
                _upgradeCapacityText.text = "Max";
                _upgradeCapacityCostText.text = "Max";
            }
            else
            {
                _upgradeCapacityText.text = Manager.data.CharacterLv
                    .Values[(_worker.MaxCapacityLv.Value + 1).ToString()].Capacity.ToString();

                int cost = Manager.data.WorkerUpgradeCost.Values[$"{(_worker.MaxCapacityLv.Value + 1)}_{_worker.Rank}"].Capacity;

                if(Manager.firebase.UserData.CurStage.Value == "Tutorial")
                {
                    cost = 0;
                }

                _upgradeCapacityCost = (int)(cost);

                _upgradeCapacityCostText.text = _upgradeCapacityCost.ToString();
            }

            // LevelBlock 업데이트
            UpdateCapacityBlockLevels(_worker.MaxCapacityLv.Value, _worker.MaxCapacityMaxLv);

            _capacityUpgradeButton.interactable = true;
        }

        private void SetupButtons()
        {
            Debug.Log($"[WorkerDetailPanel] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                Debug.Log($"[WorkerDetailPanel] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var eventHandler = GetEventWithSFX(_closeButtonName, "SFX_ButtonClickBack");
            if (eventHandler != null)
            {
                eventHandler.Click += (data) => OnCloseButtonClicked();
            }

            eventHandler = GetEventWithSFX(_speedUpgradeButtonName, "SFX_ButtonClick");
            if (eventHandler != null)
            {
                eventHandler.Click += (data) => OnUpgradeSpeedButtonClicked();
            }

            eventHandler = GetEventWithSFX(_capacityUpgradeButtonName, "SFX_ButtonClick");
            if (eventHandler != null)
            {
                eventHandler.Click += (data) => OnUpgradeCapacityButtonClicked();
            }

            isButtonsSetup = true; // 설정 완료 플래그
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[WorkerDetailPanel] 패널 닫기");
            Manager.ui.ClosePopup();
        }

        private void OnUpgradeSpeedButtonClicked()
        {
            if (_isInSpeedProgress) return;

            if ((Manager.player.Data.Money.Value < _upgradeSpeedCost || _worker.IsMoveSpeedMaxLv)) return;

            Manager.Audio.SfxPlay("Money");

            _speedUpgradeButton.interactable = false;
            _isInSpeedProgress = true;

            Manager.player.Data.Money.Value -= _upgradeSpeedCost;
            _worker.MoveSpeedLv.Value++;
        }

        private void OnSpeedChanged(int value)
        {
            _isInSpeedProgress = false;
            _speedUpgradeButton.interactable = true;
        }

        private void OnUpgradeCapacityButtonClicked()
        {
            if (_isInCapacityProgress) return;

            if ((Manager.player.Data.Money.Value < _upgradeCapacityCost || _worker.IsMaxCapacityMaxLv)) return;

            Manager.Audio.SfxPlay("Money");

            _capacityUpgradeButton.interactable = false;
            _isInCapacityProgress = true;

            Manager.player.Data.Money.Value -= _upgradeCapacityCost;
            _worker.MaxCapacityLv.Value++;
        }

        private void OnCapacityChanged(int value)
        {
            _isInCapacityProgress = false;
            _capacityUpgradeButton.interactable = true;
        }

        /// <summary>
        /// 이동 속도 Block 레벨 표시 업데이트
        /// </summary>
        private void UpdateMoveSpeedBlockLevels(int currentLevel, int maxLevel)
        {
            Debug.Log($"[WorkerDetailPanel] 이동 속도 LevelBlock 업데이트 시작 - 현재 레벨: {currentLevel}, 최대 레벨: {maxLevel}");

            // 기존 Block들 제거
            ClearMoveSpeedBlocks();

            // 최대 레벨만큼 LevelBlock 생성
            for (int i = 0; i < maxLevel; i++)
            {
                GameObject block = CreateBlock(i, currentLevel, moveSpeedBlockContainer);
                if (block != null)
                {
                    moveSpeedBlockList.Add(block);
                }
            }

            Debug.Log($"[WorkerDetailPanel] 이동 속도 LevelBlock 업데이트 완료 - 총 {moveSpeedBlockList.Count}개 블록 생성");
        }

        /// <summary>
        /// 최대 운반량 Block 레벨 표시 업데이트
        /// </summary>
        private void UpdateCapacityBlockLevels(int currentLevel, int maxLevel)
        {
            Debug.Log($"[WorkerDetailPanel] 최대 운반량 LevelBlock 업데이트 시작 - 현재 레벨: {currentLevel}, 최대 레벨: {maxLevel}");

            // 기존 Block들 제거
            ClearCapacityBlocks();

            // 최대 레벨만큼 LevelBlock 생성
            for (int i = 0; i < maxLevel; i++)
            {
                GameObject block = CreateBlock(i, currentLevel, capacityBlockContainer);
                if (block != null)
                {
                    capacityBlockList.Add(block);
                }
            }

            Debug.Log($"[WorkerDetailPanel] 최대 운반량 LevelBlock 업데이트 완료 - 총 {capacityBlockList.Count}개 블록 생성");
        }

        /// <summary>
        /// LevelBlock 생성
        /// </summary>
        private GameObject CreateBlock(int blockIndex, int currentLevel, Transform container)
        {
            if (levelBlockPrefab == null)
            {
                Debug.LogError("[WorkerDetailPanel] LevelBlock 프리팹이 설정되지 않았습니다!");
                return null;
            }

            if (container == null)
            {
                Debug.LogError("[WorkerDetailPanel] Block 컨테이너가 설정되지 않았습니다!");
                return null;
            }

            // 프리팹으로 LevelBlock 생성
            GameObject instantiatedBlock = Instantiate(levelBlockPrefab, container);

            // Block 위치 설정 (Grid 형태)
            SetBlockPosition(instantiatedBlock, blockIndex);

            // Block 상태 설정
            SetBlockState(instantiatedBlock, blockIndex, currentLevel);

            Debug.Log($"[WorkerDetailPanel] LevelBlock 생성 완료 - Index: {blockIndex}, CurrentLevel: {currentLevel}");

            return instantiatedBlock;
        }

        /// <summary>
        /// LevelBlock 위치 설정 (Grid 형태)
        /// </summary>
        private void SetBlockPosition(GameObject block, int blockIndex)
        {
            RectTransform rectTransform = block.GetComponent<RectTransform>();

            // Grid 설정 (2열)
            int columns = 2;
            int row = blockIndex / columns;
            int col = blockIndex % columns;

            // LevelBlock 크기와 간격
            float blockSize = 80f;
            float spacing = 20f;

            // 위치 계산
            float x = col * (blockSize + spacing);
            float y = -row * (blockSize + spacing);

            rectTransform.anchoredPosition = new Vector2(x, y);
        }

        /// <summary>
        /// LevelBlock 상태 설정
        /// </summary>
        private void SetBlockState(GameObject block, int blockIndex, int currentLevel)
        {
            Transform emptyImage = block.transform.Find("EmptyImage");
            Transform currentImage = block.transform.Find("CurrentImage");
            Transform completedImage = block.transform.Find("CompletedImage");

            if (emptyImage == null || currentImage == null || completedImage == null)
            {
                Debug.LogWarning($"[WorkerDetailPanel] LevelBlock {blockIndex}에서 이미지 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            // 모든 이미지 비활성화
            emptyImage.gameObject.SetActive(false);
            currentImage.gameObject.SetActive(false);
            completedImage.gameObject.SetActive(false);

            // 레벨에 따라 이미지 활성화
            if (blockIndex < currentLevel)
            {
                // 업그레이드된 레벨 (완료된 레벨)
                completedImage.gameObject.SetActive(true);
                Debug.Log($"[WorkerDetailPanel] LevelBlock {blockIndex}: CompletedImage 활성화 (업그레이드 완료)");
            }
            else if (blockIndex == currentLevel)
            {
                // 현재 레벨 (다음 업그레이드 대상)
                currentImage.gameObject.SetActive(true);
                Debug.Log($"[WorkerDetailPanel] LevelBlock {blockIndex}: CurrentImage 활성화 (현재 레벨)");
            }
            else
            {
                // 업그레이드가 안된 레벨 (빈 레벨)
                emptyImage.gameObject.SetActive(true);
                Debug.Log($"[WorkerDetailPanel] LevelBlock {blockIndex}: EmptyImage 활성화 (미완성 레벨)");
            }
        }

        /// <summary>
        /// 기존 이동 속도 LevelBlock들 제거
        /// </summary>
        private void ClearMoveSpeedBlocks()
        {
            foreach (GameObject block in moveSpeedBlockList)
            {
                if (block != null)
                {
                    DestroyImmediate(block);
                }
            }
            moveSpeedBlockList.Clear();
        }

        /// <summary>
        /// 기존 최대 운반량 LevelBlock들 제거
        /// </summary>
        private void ClearCapacityBlocks()
        {
            foreach (GameObject block in capacityBlockList)
            {
                if (block != null)
                {
                    DestroyImmediate(block);
                }
            }
            capacityBlockList.Clear();
        }
    }
}
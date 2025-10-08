using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace KYS
{

    public class PropertyPanel : BaseUI
    {
        [SerializeField] private string propertyTextName = "PropertyText";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string moneyTextName = "RunMoneyButtonText";
        [SerializeField] private string gemTextName = "RunGemButtonText";
        [SerializeField] private string runDetailMoneyTextName = "RunDetailMoneyText";
        [SerializeField] private string runDetailGemTextName = "RunDetailGemText";
        [SerializeField] private string assetToggleName = "AssetToggle";
        [SerializeField] private string assetDetailName = "AssetDetail";
        [SerializeField] private string assetToggleBackgroundName = "AssetToggleBackground"; // AssetToggle의 배경 오브젝트
        [SerializeField] private string assetToggleCheckmarkName = "AssetToggleCheckmark"; // AssetToggle의 체크마크 오브젝트
        [SerializeField] Transform contentParent;
        [SerializeField] GameObject contentPrefab;

        private TextMeshProUGUI propertyText => GetUI<TextMeshProUGUI>(propertyTextName);
        private TextMeshProUGUI moneyText => GetUI<TextMeshProUGUI>(moneyTextName);
        private TextMeshProUGUI gemText => GetUI<TextMeshProUGUI>(gemTextName);
        private TextMeshProUGUI runDetailMoneyText => GetUI<TextMeshProUGUI>(runDetailMoneyTextName);
        private TextMeshProUGUI runDetailGemText => GetUI<TextMeshProUGUI>(runDetailGemTextName);
        private GameObject assetToggle => GetUI(assetToggleName);
        private GameObject assetDetail => GetUI(assetDetailName);
        private GameObject assetToggleBackground => GetUI(assetToggleBackgroundName);
        private GameObject assetToggleCheckmark => GetUI(assetToggleCheckmarkName);


        // 추가 변수 선언
        private int currentMoney = 0;
        private int currentGem = 0;

        //Dictionary<string, BuildingData> buildingDatas = new();
        Dictionary<string, PropertyContent> contentInstances = new();

        // AssetDetail 토글 상태 관리
        private bool isAssetDetailVisible = false;

        protected override void Awake()
        {
            base.Awake();
            // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
            if (layerType == UILayerType.Panel) // BaseUI의 기본값
            {
                layerType = UILayerType.Panel;
            }

            Initialize();

            Manager.Audio.SfxPlay("DoorBell");

            if (TutorialManager.Instance != null)
            {
                BlockAllImages();
            }
        }
        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                // 여기에 자동 현지화 키 추가
            };
        }
        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            SetupAutoLocalization();


            // 초기 값 설정
            UpdateMoney(Manager.player.Data.Money.Value);
            UpdateGem(Manager.player.Data.Gem.Value);
            UpdateRunDetailMoney(Manager.player.Data.Money.Value);
            UpdateRunDetailGem(Manager.player.Data.Gem.Value);

            // AssetDetail 초기 상태 설정
            //Debug.Log($"[PropertyPanel] AssetDetail 초기화 - assetDetailName: {assetDetailName}");
            //Debug.Log($"[PropertyPanel] AssetDetail GameObject: {(assetDetail != null ? assetDetail.name : "null")}");
            
            if (assetDetail != null)
            {
                assetDetail.SetActive(isAssetDetailVisible);
                //Debug.Log($"[PropertyPanel] AssetDetail 초기 상태 설정: {isAssetDetailVisible}");
            }
            else
            {
                Debug.LogError($"[PropertyPanel] AssetDetail을 찾을 수 없습니다! assetDetailName: {assetDetailName}");
            }
            
            // 토글 버튼의 초기 시각적 상태 설정
            UpdateToggleVisualState();

            // ObservableProperty 구독 - 실시간 돈 업데이트
            Manager.player.Data.Money.Subscribe(OnMoneyChanged);
            Manager.player.Data.Gem.Subscribe(OnGemChanged);

            // 현재 스테이지에 판매 중인 건물들만 불러와서 생성
            string curStageID = Manager.firebase.UserData.CurStage.Value;
            string[] buildingIDs = Manager.data.Stage.Values[curStageID].GetBuildingIdList();

            foreach (string id in buildingIDs)
            {
                // 건물 정보 슬롯 생성 (중복 생성 방지)
                if (!contentInstances.ContainsKey(id))
                {
                    BuildingData bd = Manager.data.Building[id];
                    UpgradeData upgradeData = Manager.firebase.UserData.BuildingUpgradeList.Get(id);

                    PropertyContent content = Instantiate(contentPrefab, contentParent).GetComponent<PropertyContent>();
                    contentInstances.Add(id, content);

                    // PropertyContent 초기화
                    content.Initialize();
                    content.SetBuildingData(bd, upgradeData);
                }
            }
        }
        public override void Cleanup()
        {
            // 생성된 PropertyContent 인스턴스들 정리
            foreach (var content in contentInstances.Values)
            {
                if (content != null)
                {
                    Destroy(content.gameObject);
                }
            }
            contentInstances.Clear();

            // ObservableProperty 구독 해제
            Manager.player?.Data?.Money.Unsubscribe(OnMoneyChanged);
            Manager.player?.Data?.Gem.Unsubscribe(OnGemChanged);

            base.Cleanup();
        }


        private void SetupButtons()
        {
            //Debug.Log($"[PropertyPanel] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                //Debug.Log($"[PropertyPanel] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClickBack");
            if (closeEventHandler != null)
            {
                //Debug.Log($"[PropertyPanel] 새 이벤트 구독 추가 - Time: {Time.time}");
                closeEventHandler.Click += OnCloseButton;
            }
            else
            {
                Debug.LogError($"[PropertyPanel] 닫기 버튼 이벤트 설정 실패: {closeButtonName}");
            }

            // AssetToggle 설정 - AssetDetail 온오프 기능
            //Debug.Log($"[PropertyPanel] AssetToggle 이벤트 설정 - assetToggleName: {assetToggleName}");
            var assetToggleEventHandler = GetEventWithSFX(assetToggleName, "SFX_ButtonClick");
            if (assetToggleEventHandler != null)
            {
                assetToggleEventHandler.Click += (data) => OnAssetToggleClicked();
                //Debug.Log($"[PropertyPanel] AssetToggle 이벤트 설정 완료");
            }
            else
            {
                Debug.LogError($"[PropertyPanel] AssetToggle 이벤트 설정 실패! assetToggleName: {assetToggleName}");
            }

            isButtonsSetup = true; // 설정 완료 플래그
        }


        public void UpdateMoney(int amount)
        {

            currentMoney = amount; // 현재 값 저장
            if (moneyText != null)
            {
                // BaseUI의 돈 포맷팅 사용 (소수점 없음)
                moneyText.text = FormatMoney(amount, true);
            }
        }

        public void UpdateGem(int amount)
        {
            currentGem = amount; // 현재 값 저장
            if (gemText != null)
            {
                // BaseUI의 돈 포맷팅 사용 (소수점 없음)
                gemText.text = FormatMoney(amount, true);
            }
        }

        public void UpdateRunDetailMoney(int amount)
        {
            if (runDetailMoneyText != null)
            {
                // BaseUI의 콤마 포맷팅 사용 (100,000 형식)
                runDetailMoneyText.text = FormatMoneyWithCommas(amount);
            }
        }

        public void UpdateRunDetailGem(int amount)
        {
            if (runDetailGemText != null)
            {
                // BaseUI의 콤마 포맷팅 사용 (100,000 형식)
                runDetailGemText.text = FormatMoneyWithCommas(amount);
            }
        }

        /// <summary>
        /// ObservableProperty Money 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnMoneyChanged(int newMoneyValue)
        {
            UpdateMoney(newMoneyValue);
            UpdateRunDetailMoney(newMoneyValue);
        }

        /// <summary>
        /// ObservableProperty Gem 값 변경 시 호출되는 콜백
        /// </summary>
        private void OnGemChanged(int newGemValue)
        {
            UpdateGem(newGemValue);
            UpdateRunDetailGem(newGemValue);
        }


        private void OnCloseButton(PointerEventData data)
        {
            //Debug.Log($"[PropertyPanel] OnCloseButton 호출됨 - Time: {Time.time}");
            Manager.ui.ClosePanel();
        }

        private void OnAssetToggleClicked()
        {
            //Debug.Log("[PropertyPanel] AssetToggle 클릭됨");
            //Debug.Log($"[PropertyPanel] assetDetail null 체크: {assetDetail == null}");
            //Debug.Log($"[PropertyPanel] assetDetailName: {assetDetailName}");
            //Debug.Log($"[PropertyPanel] 현재 isAssetDetailVisible: {isAssetDetailVisible}");
            
            // AssetDetail 토글
            ToggleAssetDetail();
        }

        /// <summary>
        /// AssetDetail 표시/숨김 토글
        /// </summary>
        private void ToggleAssetDetail()
        {
            isAssetDetailVisible = !isAssetDetailVisible;
            
            //Debug.Log($"[PropertyPanel] ToggleAssetDetail 호출됨 - 새로운 상태: {isAssetDetailVisible}");
            //Debug.Log($"[PropertyPanel] assetDetail GameObject: {(assetDetail != null ? assetDetail.name : "null")}");
            
            // AssetDetail 토글
            if (assetDetail != null)
            {
                assetDetail.SetActive(isAssetDetailVisible);
                //Debug.Log($"[PropertyPanel] AssetDetail.SetActive({isAssetDetailVisible}) 호출됨");
                //Debug.Log($"[PropertyPanel] AssetDetail 활성 상태: {assetDetail.activeInHierarchy}");
            }
            else
            {
                Debug.LogError($"[PropertyPanel] AssetDetail을 찾을 수 없습니다! assetDetailName: {assetDetailName}");
            }
            
            // 토글 버튼의 시각적 상태 변경
            UpdateToggleVisualState();
            
            //Debug.Log($"[PropertyPanel] AssetDetail {(isAssetDetailVisible ? "표시" : "숨김")}");
        }

        /// <summary>
        /// 토글 버튼의 시각적 상태 업데이트
        /// </summary>
        private void UpdateToggleVisualState()
        {
            // AssetDetail이 표시될 때: 체크마크 표시, 배경 숨김
            // AssetDetail이 숨겨질 때: 체크마크 숨김, 배경 표시
            if (assetToggleCheckmark != null)
            {
                assetToggleCheckmark.SetActive(isAssetDetailVisible);
                
                // 체크마크가 활성화되면 최상위로 이동
                if (isAssetDetailVisible)
                {
                    assetToggleCheckmark.transform.SetAsLastSibling();
                }
                
                //Debug.Log($"[PropertyPanel] 체크마크 {(isAssetDetailVisible ? "표시" : "숨김")} - 오브젝트: {assetToggleCheckmark.name}, 활성상태: {assetToggleCheckmark.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[PropertyPanel] assetToggleCheckmark를 찾을 수 없습니다.");
            }
            
            if (assetToggleBackground != null)
            {
                assetToggleBackground.SetActive(!isAssetDetailVisible);
                //Debug.Log($"[PropertyPanel] 배경 {(!isAssetDetailVisible ? "표시" : "숨김")} - 오브젝트: {assetToggleBackground.name}, 활성상태: {assetToggleBackground.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[PropertyPanel] assetToggleBackground를 찾을 수 없습니다.");
            }
        }






    }




}
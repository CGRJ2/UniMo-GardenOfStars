using KYS;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using System.Collections.Generic;
public class InfoPanel_Harvest2 : BaseUI
{
    HarvestBD targetBD;

    [Header("건물 정보")]
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Description;

    [Header("생산 재료 정보")]
    [SerializeField] TMP_Text tmp_ProdName;
    [SerializeField] Image image_Prod;

    [Header("생산 속도 스탯")]
    //[SerializeField] TMP_Text tmp_ProdTimeLevel;
    [SerializeField] TMP_Text tmp_CurProdTime;
    [SerializeField] TMP_Text tmp_AfterUpProdTime;

    [Header("생산 속도 업그레이드 비용")]
    [SerializeField] TMP_Text tmp_ProdTimeUpCost;

    [Header("업그레이드 버튼")]
    [SerializeField] Button btn_ProdTimeUpgrade;

    [Header("패널 닫기 버튼")]
    [SerializeField] Button btn_Close;

    [Header("레벨 표시 Block 컨테이너")]
    [SerializeField] Transform blockContainer;

    [Header("LevelBlock 프리팹")]
    [SerializeField] GameObject levelBlockPrefab;

    // Block 관리용 리스트
    private List<GameObject> blockList = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        // 인스펙터에서 설정한 값이 있으면 그대로 사용, 없으면 기본값 설정
        if (layerType == UILayerType.Panel) // BaseUI의 기본값
        {
            layerType = UILayerType.Panel;
        }

        Init();
    }

    public void Init()  // 초기화를 어디서 해줘야 할까요?
    {
        Manager.buildings.upgradeEvent += OnUpgradeEvent;

        btn_ProdTimeUpgrade.onClick.AddListener(UpgradeProdTime);
        btn_Close.onClick.AddListener(Close);
        // 언어 변경 이벤트 구독
        BuildingLocalizationHelper.SubscribeToLanguageChanged(OnLanguageChanged);
        IngrediantLocalizationHelper.SubscribeToLanguageChanged(OnLanguageChanged);
    }

    void UpgradeProdTime()
    {
        // 돈 차감
        UpgradeData upgradeData = Manager.buildings.GetUpgradeData(targetBD.ID);
        int curLevel_ProdTime = upgradeData == null ? 0 : upgradeData.level_ProdTime;

        Manager.player.Data.Money.Value -= (int)targetBD.Stat_ProdTime.cost[curLevel_ProdTime];

        // 업그레이드 스탯 적용
        Manager.buildings.UpdateUpgradedData(targetBD.ID, 1);

        // 패널 정보 업데이트
        SetUpgradeData(targetBD);
    }

    public void SetUpgradeData(HarvestBD harvest)
    {
        HarvestBD data = harvest;
        targetBD = data;

        int curMoney = Manager.player.Data.Money.Value;

        UpgradeData upgradeData = Manager.buildings.GetUpgradeData(targetBD.ID);
        int curLevel_ProdTime = upgradeData == null ? 0 : upgradeData.level_ProdTime;

        // 기존 LocalizationManager와 DataManager를 활용한 번역 시스템 사용
        tmp_Name.text = BuildingLocalizationHelper.GetBuildingName(data.ID);
        tmp_Description.text = BuildingLocalizationHelper.GetBuildingDescription(data.ID);

        // 새 방법 (0918 최재민)
        tmp_ProdName.text = IngrediantLocalizationHelper.GetIngrediantText(data.ProductID);
        image_Prod.sprite = Manager.data.Ingrediant[data.ProductID].Sprite;

        // 어드레서블로 불러올 필요가 없어짐 (0918 최재민)
        //Addressables.LoadAssetAsync<IngrediantData>(data.ProductID).Completed += prodData =>
        //{
        //    tmp_ProdName.text = IngrediantLocalizationHelper.GetIngrediantText(prodData.Result.ID);
        //    image_Prod.sprite = prodData.Result.Sprite;
        //};

        if (curLevel_ProdTime < data.Stat_ProdTime.MaxLevel)
        {
            tmp_ProdTimeUpCost.text = $"{data.Stat_ProdTime.cost[curLevel_ProdTime]}";
            tmp_CurProdTime.text = $"{data.Stat_ProdTime.Values[curLevel_ProdTime]}";
            tmp_AfterUpProdTime.text = $"{data.Stat_ProdTime.Values[curLevel_ProdTime + 1]}";

            if (curMoney > data.Stat_ProdTime.cost[curLevel_ProdTime])
            {
                btn_ProdTimeUpgrade.interactable = true;
            }
            else
            {
                btn_ProdTimeUpgrade.interactable = false;
            }
        }
        else
        {
            Debug.Log("생산 속도가 최대 단계입니다");
            tmp_CurProdTime.text = $"{data.Stat_ProdTime.Values[curLevel_ProdTime]}";
            tmp_AfterUpProdTime.text = Manager.localization.GetText("MaxLevelReached");

            tmp_ProdTimeUpCost.text = Manager.localization.GetText("MaxLevel");

            btn_ProdTimeUpgrade.interactable = false;
        }

        // LevelBlock들 업데이트 (레벨에 따른 시각적 상태 표시)
        UpdateBlockLevels(curLevel_ProdTime, data.Stat_ProdTime.MaxLevel);
    }
    void OnUpgradeEvent(int value)
    {
        // 패널 정보 업데이트
        SetUpgradeData(targetBD);
    }
    private void Close()
    {
        Manager.Audio.SfxPlay("SFX_ButtonClickBack");
        Manager.ui.ClosePopup();
    }

    protected override void OnDestroy()
    {
        // 언어 변경 이벤트 구독 해제
        BuildingLocalizationHelper.UnsubscribeFromLanguageChanged(OnLanguageChanged);
        IngrediantLocalizationHelper.UnsubscribeFromLanguageChanged(OnLanguageChanged);
        
        // LevelBlock들 정리
        ClearBlocks();
    }

    /// <summary>
    /// 언어가 변경될 때 호출되는 메서드
    /// </summary>
    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        if (targetBD != null)
        {
            // 건물 정보 다시 로드
            tmp_Name.text = BuildingLocalizationHelper.GetBuildingName(targetBD.ID);
            tmp_Description.text = BuildingLocalizationHelper.GetBuildingDescription(targetBD.ID);
            tmp_ProdName.text = IngrediantLocalizationHelper.GetIngrediantText(targetBD.ProductID);
        }
    }

    /// <summary>
    /// Block 레벨 표시 업데이트
    /// </summary>
    private void UpdateBlockLevels(int currentLevel, int maxLevel)
    {
        Debug.Log($"[InfoPanel_Harvest2] LevelBlock 업데이트 시작 - 현재 레벨: {currentLevel}, 최대 레벨: {maxLevel}");
        
        // 기존 Block들 제거
        ClearBlocks();

        // 최대 레벨만큼 LevelBlock 생성
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject block = CreateBlock(i, currentLevel);
            if (block != null)
            {
                blockList.Add(block);
            }
        }
        
        Debug.Log($"[InfoPanel_Harvest2] LevelBlock 업데이트 완료 - 총 {blockList.Count}개 블록 생성");
    }

    /// <summary>
    /// LevelBlock 생성
    /// </summary>
    private GameObject CreateBlock(int blockIndex, int currentLevel)
    {
        if (levelBlockPrefab == null)
        {
            Debug.LogError("[InfoPanel_Harvest2] LevelBlock 프리팹이 설정되지 않았습니다!");
            return null;
        }

        // 프리팹으로 LevelBlock 생성
        GameObject instantiatedBlock = Instantiate(levelBlockPrefab, blockContainer);
        
        // Block 위치 설정 (Grid 형태)
        SetBlockPosition(instantiatedBlock, blockIndex);
        
        // Block 상태 설정
        SetBlockState(instantiatedBlock, blockIndex, currentLevel);
        
        Debug.Log($"[InfoPanel_Harvest2] LevelBlock 생성 완료 - Index: {blockIndex}, CurrentLevel: {currentLevel}");
        
        return instantiatedBlock;
    }


    /// <summary>
    /// LevelBlock 위치 설정 (Grid 형태)
    /// </summary>
    private void SetBlockPosition(GameObject block, int blockIndex)
    {
        RectTransform rectTransform = block.GetComponent<RectTransform>();
        
        // Grid 설정 (3열)
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
            Debug.LogWarning($"[InfoPanel_Harvest2] LevelBlock {blockIndex}에서 이미지 컴포넌트를 찾을 수 없습니다.");
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
            Debug.Log($"[InfoPanel_Harvest2] LevelBlock {blockIndex}: CompletedImage 활성화 (업그레이드 완료)");
        }
        else if (blockIndex == currentLevel)
        {
            // 현재 레벨 (다음 업그레이드 대상)
            currentImage.gameObject.SetActive(true);
            Debug.Log($"[InfoPanel_Harvest2] LevelBlock {blockIndex}: CurrentImage 활성화 (현재 레벨)");
        }
        else
        {
            // 업그레이드가 안된 레벨 (빈 레벨)
            emptyImage.gameObject.SetActive(true);
            Debug.Log($"[InfoPanel_Harvest2] LevelBlock {blockIndex}: EmptyImage 활성화 (미완성 레벨)");
        }
    }

    /// <summary>
    /// 기존 LevelBlock들 제거
    /// </summary>
    private void ClearBlocks()
    {
        foreach (GameObject block in blockList)
        {
            if (block != null)
            {
                DestroyImmediate(block);
            }
        }
        blockList.Clear();
    }

}

using KYS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingActivePopUI : MonoBehaviour
{
    BuildingInstance buildingInstance;
    [SerializeField] Button btn_Info;
    [SerializeField] TMP_Text tmp_BtnState;

    public void Init(BuildingInstance buildingInstance)
    {
        this.buildingInstance = buildingInstance;

        if (buildingInstance is ManufactureBuilding)
        {
            btn_Info.onClick.AddListener(OpenInfoPanel_Manufacture);
        }
        else if (buildingInstance is HarvestBuilding)
        {
            btn_Info.onClick.AddListener(OpenInfoPanel_Harvest);
        }
    }
    public void OpenInfoPanel_Harvest()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[건물 정보 패널] UIManager.Instance가 null입니다!");
            return;
        }

        // 이미 TitlePanel이 열려있는지 확인
        var existingPopups = Manager.ui.GetUIsByLayer(UILayerType.Popup);
        foreach (var Popup in existingPopups)
        {
            if (Popup is InfoPanel_Harvest)
            {
                //Debug.Log("[HUDAllPanel] 이미 TitlePanel이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 업그레이드 패널 열기
        Manager.ui.ShowPopUpAsync<InfoPanel_Harvest>((Popup) =>
        {
            if (Popup != null)
            {
                //Debug.Log("[HUDAllPanel] TitlePanel 성공적으로 열림");

                if (buildingInstance is HarvestBuilding harvesst)
                    Popup.SetUpgradeData(harvesst.originData);
            }
            else
            {
                //Debug.LogError("[HUDAllPanel]  열기 실패");
            }
        });
    }
    public void OpenInfoPanel_Manufacture()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[건물 정보 패널] UIManager.Instance가 null입니다!");
            return;
        }

        // 이미 TitlePanel이 열려있는지 확인
        var existingPopups = Manager.ui.GetUIsByLayer(UILayerType.Popup);
        foreach (var Popup in existingPopups)
        {
            if (Popup is InfoPanel_Manufacture)
            {
                //Debug.Log("[HUDAllPanel] 이미 TitlePanel이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 업그레이드 패널 열기
        Manager.ui.ShowPopUpAsync<InfoPanel_Manufacture>((Popup) =>
        {
            if (Popup != null)
            {
                //Debug.Log("[HUDAllPanel] TitlePanel 성공적으로 열림");

                if (buildingInstance is ManufactureBuilding manufacture)
                    Popup.SetUpgradeData(manufacture.originData);
            }
            else
            {
                //Debug.LogError("[HUDAllPanel]  열기 실패");
            }
        });
    }

    public void ActiveUpgradeBtnView()
    {
        tmp_BtnState.text = "UP"; // 임시
    }

    public void ActiveInfoBtnView()
    {
        tmp_BtnState.text = "!"; // 임시
    }
}

using KYS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingSellerPopUI : MonoBehaviour
{
    [SerializeField] Button btn_Info;
    [SerializeField] TMP_Text tmp_BtnState;

    public void Init()
    {
        btn_Info.onClick.AddListener(OpenPanel);
    }
    public void OpenPanel()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[건물 정보 패널] UIManager.Instance가 null입니다!");
            return;
        }

        // 이미 TitlePanel이 열려있는지 확인
        var existingPanels = Manager.ui.GetUIsByLayer(UILayerType.Panel);
        foreach (var panel in existingPanels)
        {
            if (panel is InfoPanel_Harvest)
            {
                //Debug.Log("[HUDAllPanel] 이미 TitlePanel이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 업그레이드 패널 열기
        Manager.ui.ShowPanelAsync<InfoPanel_Harvest>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("[HUDAllPanel] TitlePanel 성공적으로 열림");

                /*if (buildingInstance is HarvestBuilding harvesst)
                    panel.SetUpgradeData(harvesst.originData);*/
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

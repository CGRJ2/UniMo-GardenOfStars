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
            Debug.LogError("[�ǹ� ���� �г�] UIManager.Instance�� null�Դϴ�!");
            return;
        }

        // �̹� TitlePanel�� �����ִ��� Ȯ��
        var existingPanels = Manager.ui.GetUIsByLayer(UILayerType.Panel);
        foreach (var panel in existingPanels)
        {
            if (panel is InfoPanel_Harvest)
            {
                //Debug.Log("[HUDAllPanel] �̹� TitlePanel�� �����ֽ��ϴ�. �ߺ� ȣ�� ����");
                return;
            }
        }

        // ���׷��̵� �г� ����
        Manager.ui.ShowPanelAsync<InfoPanel_Harvest>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("[HUDAllPanel] TitlePanel ���������� ����");

                if (buildingInstance is HarvestBuilding harvesst)
                    panel.SetUpgradeData(harvesst);
            }
            else
            {
                //Debug.LogError("[HUDAllPanel]  ���� ����");
            }
        });
    }
    public void OpenInfoPanel_Manufacture()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[�ǹ� ���� �г�] UIManager.Instance�� null�Դϴ�!");
            return;
        }

        // �̹� TitlePanel�� �����ִ��� Ȯ��
        var existingPanels = Manager.ui.GetUIsByLayer(UILayerType.Panel);
        foreach (var panel in existingPanels)
        {
            if (panel is InfoPanel_Manufacture)
            {
                //Debug.Log("[HUDAllPanel] �̹� TitlePanel�� �����ֽ��ϴ�. �ߺ� ȣ�� ����");
                return;
            }
        }

        // ���׷��̵� �г� ����
        Manager.ui.ShowPanelAsync<InfoPanel_Manufacture>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("[HUDAllPanel] TitlePanel ���������� ����");

                if (buildingInstance is ManufactureBuilding manufacture)
                    panel.SetUpgradeData(manufacture);
            }
            else
            {
                //Debug.LogError("[HUDAllPanel]  ���� ����");
            }
        });
    }

    public void ActiveUpgradeBtn()
    {
        tmp_BtnState.text = "UP"; // �ӽ�
    }

    public void ActiveInfoBtn()
    {
        tmp_BtnState.text = "!"; // �ӽ�
    }
}

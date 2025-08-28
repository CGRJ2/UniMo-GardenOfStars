using KYS;
using UnityEngine;
using UnityEngine.UI;

public class PortalPopUI : MonoBehaviour
{
    [SerializeField] Button btn_RegionMove;

    public void Init()
    {
        btn_RegionMove.onClick.AddListener(OpenStageListPanel);
        GetComponent<Canvas>().worldCamera = Camera.main;
        gameObject.SetActive(false);
    }

    void OpenStageListPanel()
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
            if (panel is StageListPanel)
            {
                //Debug.Log("[HUDAllPanel] 이미 TitlePanel이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 업그레이드 패널 열기
        Manager.ui.ShowPanelAsync<StageListPanel>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("[HUDAllPanel] TitlePanel 성공적으로 열림");
            }
            else
            {
                //Debug.LogError("[HUDAllPanel]  열기 실패");
            }
        });
    }

}

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
            Debug.LogError("[스테이지 패널] UIManager.Instance가 null입니다!");
            return;
        }

        // 이미 패널이 열려있는지 확인
        var existingPanels = Manager.ui.GetUIsByLayer(UILayerType.Panel);
        foreach (var panel in existingPanels)
        {
            if (panel is StageTransitionPanel)
            {
                //Debug.Log("이미 스테이지 패널이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 업그레이드 패널 열기
        Manager.ui.ShowPanelAsync<StageTransitionPanel>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("스테이지 패널 성공적으로 열림");
            }
            else
            {
                //Debug.LogError("스테이지 패널  열기 실패");
            }
        });
    }

}

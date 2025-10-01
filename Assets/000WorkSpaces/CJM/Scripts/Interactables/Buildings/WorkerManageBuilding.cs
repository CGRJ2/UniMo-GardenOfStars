using KYS;
using UnityEngine;

public class WorkerManageBuilding : InteractableBase
{
    [SerializeField] WaitingTile interactTile;
    [SerializeField] Vector3 spawnPointOffset;
    public Vector3 GetSpawnPos()
    {
        return transform.position + spawnPointOffset;
    }


    private void Awake()
    {
        Manager.buildings.workerBuilding = this;

        interactTile.WaitingCompletedAction = OpenWorkerPanel;
        interactTile.Init();

        if (Manager.firebase.UserData.CurStage.Value == "Tutorial")
        {
            HideWaitingTile();
        }
    }

    public void ShowWaitingTile()
    {
        interactTile.gameObject.SetActive(true);
    }
    public void HideWaitingTile()
    {
        interactTile.gameObject.SetActive(false);
    }

    public void OpenWorkerPanel()
    {
        Debug.Log("일꾼 패널 열기");

        if (UIManager.Instance == null)
        {
            Debug.LogError("[일꾼 패널] UIManager.Instance가 null입니다!");
            return;
        }

        // 이미 부동산 패널이 열려있는지 확인
        var existingPanels = Manager.ui.GetUIsByLayer(UILayerType.Panel);
        foreach (var panel in existingPanels)
        {
            if (panel is HRRoomPanel)
            {
                //Debug.Log("[일꾼 패널] 이미 패널이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 부동산 패널 열기
        Manager.ui.ShowPanelAsync<HRRoomPanel>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("[일꾼 패널] 성공적으로 열림");
            }
            else
            {
                //Debug.LogError("[일꾼 패널]  열기 실패");
            }
        });
    }
}

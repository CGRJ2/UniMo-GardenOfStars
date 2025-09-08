using KYS;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaitingTile : InteractableBase
{
    [SerializeField] Slider progressBar;
    [SerializeField] float interactTime;
    [SerializeField] float progressedTime;

    public void Init()
    {
        progressBar.gameObject.SetActive(false);
    }

    IEnumerator ProgressingTask()
    {
        while (characterRD != null) // 영역 안에 있을 때 진행
        {
            // 작업 진행 중, 영역 내에서 움직인 경우 대기
            if (characterRD.IsMove.Value)
            {
                progressBar.gameObject.SetActive(false);
                progressedTime = 0; // 진행도 초기화
                yield return null;
                continue;
            }

            // 작업 영역 밖으로 나가는 경우
            if (characterRD == null)
            {
                progressedTime = 0; // 진행도 초기화
                yield return null;
                break;
            }

            // 작업 시작 시, 진행도 표기
            progressBar.gameObject.SetActive(true);

            progressedTime += Time.deltaTime;

            // 설치가 완료된 경우
            if (interactTime < progressedTime)
            {
                CompleteTask(); // 결과물 생성
                progressedTime = 0; // 진행도 초기화
                break;
            }

            // 진행도 게이지 업데이트
            progressBar.value = progressedTime / interactTime;

            yield return null;
        }

        // 진행도 표기 비활성화
        progressBar.gameObject.SetActive(false);
        yield return null;
    }

    public void CompleteTask()
    {
        // UI 열기
        Debug.Log("부동산 패널 열기");
        OpenEstatePanel();
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);

        StartCoroutine(ProgressingTask());
    }


    // 부동산 패널 열기
    public void OpenEstatePanel()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[부동산 패널] UIManager.Instance가 null입니다!");
            return;
        }

        // 이미 부동산 패널이 열려있는지 확인
        var existingPanels = Manager.ui.GetUIsByLayer(UILayerType.Panel);
        foreach (var panel in existingPanels)
        {
            if (panel is PropertyPanel)
            {
                //Debug.Log("[부동산 패널] 이미 부동산 패널이 열려있습니다. 중복 호출 무시");
                return;
            }
        }

        // 부동산 패널 열기
        Manager.ui.ShowPanelAsync<PropertyPanel>((panel) =>
        {
            if (panel != null)
            {
                //Debug.Log("[부동산 패널] 부동산 패널 성공적으로 열림");

                /*if (buildingInstance is HarvestBuilding harvesst)
                    panel.SetUpgradeData(harvesst.originData);*/
            }
            else
            {
                //Debug.LogError("[부동산 패널]  열기 실패");
            }
        });
    }


}

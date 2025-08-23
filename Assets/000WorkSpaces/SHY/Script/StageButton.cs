using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;


public class StageButton : MonoBehaviour
{
    [Header("이 버튼이 담당하는 스테이지 인덱스")]
    public int stageIndex;
    

    [Header("UI 요소")]
    public TextMeshProUGUI label;

    private StageData stageData;

    void Start()  //프리펩 안쓸때
    {
        // StageManager에서 해당 스테이지 데이터 가져오기
        stageData = Manager.stage.GetStages()[stageIndex];

        UpdateVisual();
    }
    public void Initialize(StageData data) //프리펩화된거 쓸때.
    {
        stageData = data;
        UpdateVisual();
    }


    public void OnClick()
    {
        if (!stageData.Unlock)
        {
            Debug.Log($"{stageData.name}은 잠긴 스테이지입니다.");
            return;
        }

        //string currentScene = SceneManager.GetActiveScene().name;
        //string targetScene = stageData.StageId.ToString(); // 외버전
        //string targetScene = stageData.StageName;

        if (Manager.stage.sceneChanger.CurSceneName == stageData.StageName) // 이름 체크 방식.
        {
            Debug.Log("이미 현재 스테이지입니다.");
            return;
        }
        //int cs = SceneManager.GetActiveScene().buildIndex;
        //int ts = stageData.StageId;
        if (Manager.stage.sceneChanger.CurSceneID == stageData.StageId)//인덱스 체크 방식.
        {
            Debug.Log("이미 현재 스테이지입니다.");
            return;
        }
        
        Manager.stage.sceneChanger.LoadSceneByIndex(stageIndex);
        //Manager.stage.sceneChanger.LoadSceneByName(stageData.StageName);
    }

    public void UpdateVisual()
    {
        if (label == null) return;

        if (stageData.Unlock)
        {
            label.text = $"Stage {stageData.StageId} (열림)";
            label.color = Color.black; // 해금 상태 색상
        }
        else
        {
            label.text = $"Stage {stageData.StageId} (잠김)";
            label.color = Color.gray; // 잠금 상태 색상
        }
    }
}

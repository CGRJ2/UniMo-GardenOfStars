using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Btn_Stage : MonoBehaviour
{
    [SerializeField] Button btn_Self;
    StageData stageData;
    [SerializeField] TMP_Text tmp_StageName;

    private void Awake()
    {
        btn_Self ??= GetComponent<Button>();
        btn_Self.onClick.AddListener(OnClickSelf);
    }

    public void Init(StageData stageData)
    {
        this.stageData = stageData;
        tmp_StageName.text = stageData.StageName;
    }

    void OnClickSelf()
    {
        if (Manager.game.curStageId == stageData.StageId)
        {
            Debug.Log("이미 해당 스테이지에 위치함");
            return;
        }

        Manager.game.curStageId = stageData.StageId;
        Addressables.LoadSceneAsync("StageScene");
    }

    public void UpdateView()
    {
        bool isUnlock = Manager.game.GetStageUnlockCheck(stageData.StageId);

        if (isUnlock)
        {
            tmp_StageName.color = Color.black; // 해금 상태 색상
            btn_Self.interactable = true;
        }
        else
        {
            tmp_StageName.color = Color.red; // 잠금 상태 색상
            btn_Self.interactable = false;
        }
    }
}

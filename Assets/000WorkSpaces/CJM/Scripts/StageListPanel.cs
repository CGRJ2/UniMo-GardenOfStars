using KYS;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageListPanel : BaseUI
{
    Btn_Stage[] stageBtnList;
    [Header("스테이지 버튼들의 부모 위치 Transform")]
    [SerializeField] Transform btnsParent;
    [SerializeField] Button btn_Exit;

    protected override void Awake()
    {
        base.Awake();
        if (layerType == UILayerType.Panel) // BaseUI의 기본값
        {
            layerType = UILayerType.Panel;
        }
        Init();
        btn_Exit.onClick.AddListener(() => UIManager.Instance.ClosePanel());
    }

    public void Init()
    {
        stageBtnList = btnsParent.GetComponentsInChildren<Btn_Stage>();

        List<KeyValuePair<string, StageData>> kvpList = new();

        // 현재 존재하는 모든 스테이지 데이터 불러와서 kvp리스트에 저장
        foreach(var kvp in Manager.game.stageDataDic)
        {
            kvpList.Add(kvp);
        }

        // 스테이지 데이터들을 각각의 버튼에 넣어주기
        for(int i = 0; i < kvpList.Count; i++)
        {
            // 스테이지 데이터 수량보다 버튼이 더 적을 경우
            if (stageBtnList.Length < i + 1)
            {
                //Debug.LogWarning($"스테이지 버튼 수 보다 데이터 수가 더 많아서 다 표기할 수 없음. (버튼 수: {stageBtnList.Length})/(데이터 수 :{kvpList.Count})");
                return;
            }
            stageBtnList[i].Init(kvpList[i].Value);
        }

        // 스테이지 데이터 수량보다 버튼이 더 많을 경우
        if (stageBtnList.Length > kvpList.Count)
        {
            // 남은 버튼들 비활성화 해주기
            for (int i = kvpList.Count; i < stageBtnList.Length; i++)
            {
                //Debug.LogWarning($"버튼({stageBtnList[i].name})에 할당할 데이터가 없어서 비활성화 또는 잠금 상태로 변경");
                stageBtnList[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        List<Btn_Stage> activatedBtnList = new();
        foreach(Btn_Stage btn in stageBtnList)
        {
            if (btn.gameObject.activeSelf)
                activatedBtnList.Add(btn);
        }

        foreach (Btn_Stage btn in activatedBtnList)
        {
            btn.UpdateView();
        }
    }
}

using GameQuest;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class QuestRequireTile : InteractableBase
{
    public Action RequirementCompleteAction;

    public QuestContentProgressData QC_Data; // <= 현재 퀘스트 조건 중 하나 할당해주기
    [SerializeField] Transform attachPoint;
    [SerializeField] float insertDelayTime = 0.1f;

    [SerializeField] CanvasGroup group_Require;
    [SerializeField] CanvasGroup group_Complete;
    [SerializeField] Image image_Ingrediant;
    [SerializeField] TMP_Text tmp_Count;

    [SerializeField] Transform lampParent;
    Lamp_QuestContent[] lamps;

    private IngrediantData ingrediantData;

    private void Awake()
    {
        lamps = lampParent.GetComponentsInChildren<Lamp_QuestContent>();
    }

    public void SetUp()
    {
        // 스텝 램프 초기화
        for (int i = 0; i < lamps.Length; i++)
        {
            if (i > QC_Data.StepIndexForClearContent)
            {
                lamps[i].gameObject.SetActive(false);
            }
            else
            {
                lamps[i].gameObject.SetActive(true);
                lamps[i].UpdateView(false);
            }
        }

        // QC 진행도가 없다면 => Analytics 퀘스트 Content 시작 등록 (퀘스트 시작 / 퀘스트 발판 시작)
        if (QC_Data.ProgressdIndex.Value == 0 && QC_Data.ProgressdProdsCount.Value == 0)
        {
            QC_Data.QC_StartTime.Value = DateTime.UtcNow.Second;
        }

        // 등록 후 업데이트 1회 실행
        UpdateLampView(QC_Data.ProgressdIndex.Value);
        UpdateTileView(QC_Data.ProgressdProdsCount.Value);

        // 업데이트 함수 이벤트 등록
        QC_Data.ProgressdProdsCount.Subscribe(UpdateTileView);
        QC_Data.ProgressdIndex.Subscribe(UpdateLampView);
    }

    public void UpdateLampView(int progressIndex)
    {
        for (int i = 0; i < lamps.Length; i++)
        {
            if (i < progressIndex)
            {
                lamps[i].UpdateView(true);
            }
            else lamps[i].UpdateView(false);
        }
    }

    public void UpdateTileView(int progressdProdsCount)
    {
        Debug.Log("발판 상태 업데이트");

        if (QC_Data.IsContentClear)
        {
            group_Require.gameObject.SetActive(false);
            group_Complete.gameObject.SetActive(true);
        }
        else
        {
            group_Complete.gameObject.SetActive(false);
            group_Require.gameObject.SetActive(true);

            tmp_Count.text = $"{progressdProdsCount}/{QC_Data.CurrentTargetCount}";

            // 이미 재료 데이터가 있는데, 현재 조건의 재료 데이터와 같다면 => 불러오지 않아도 됨. return;
            if (ingrediantData != null)
            {
                if (ingrediantData.ID == QC_Data.ContentTargetId)
                {
                    return;
                }
            }

            ingrediantData = Manager.data.Ingrediant[QC_Data.ContentTargetId];
            image_Ingrediant.sprite = ingrediantData.Sprite;
        }

    }

    IEnumerator AutoInserting()
    {
        while (characterRD != null)
        {
            // 플레이어 손에 있는 재료가 퀘스트 조건에 포함되는지 체크
            IngrediantInstance instanceProd;
            if (characterRD.IngrediantStack.TryPeek(out instanceProd))
            {
                // 퀘스트 조건이 할당되지 않은 발판이라면
                if (QC_Data == null) { Debug.LogError("해당 퀘스트 발판에 퀘스트 조건 데이터가 할당되지 않음"); break; }

                // 손에 있는 재료가 퀘스트 조건이 아니면 || 퀘스트가 이미 완료된 상황이면
                if (instanceProd.Data == null || instanceProd.Data.ID != QC_Data.ContentTargetId || QC_Data.IsContentClear)
                {
                    break;  // 상호작용 취소
                }

                // 현재 진행도에 개수 추가
                if (QC_Data.ProgressdProdsCount.Value < QC_Data.CurrentTargetCount)
                {
                    QC_Data.ProgressdProdsCount.Value += 1;
                    IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                    popedProd.MoveToTargetAndShrink(attachPoint, () =>
                    {

                    });
                }
                else // 필요 재료 수량만큼 다 넣으면 조건 완료처리 후 정지
                {
                    break;
                }

                // 다음 투입까지 딜레이 시간 설정
                yield return new WaitForSeconds(insertDelayTime);
                yield return new WaitUntil(() => QC_Data.ProgressdProdsCount.IsInUpdate == false);

            }
            // 플레이어 손에 재료가 없으면 바로 return
            else
            {
                yield return null;
            }
        }
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);
        if (characterRuntimeData is PlayerRunTimeData)
        {
            //Debug.LogError("타일 들어옴");
            StartCoroutine(AutoInserting());
        }
    }
}

using GameNpc;
using GameQuest;
using System;
using System.Collections;
using System.Xml.Linq;
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

    public void Init()
    {
        lamps = lampParent.GetComponentsInChildren<Lamp_QuestContent>();
        UpdateView();

        QC_Data.ProgressdProdsCount.Subscribe(UpdateView);
    }


    public void UpdateView(int a = -1)
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

            Debug.LogWarning(QC_Data.ProgressdProdsCount.Value);
            Debug.LogWarning(a);

            if (a >= 0)
                tmp_Count.text = $"{a}/{QC_Data.CurrentTargetCount}";
            else
                tmp_Count.text = $"{QC_Data.ProgressdProdsCount.Value}/{QC_Data.CurrentTargetCount}";

            // 이미 재료 데이터가 있는데, 현재 조건의 재료 데이터와 같다면 => 불러오지 않아도 됨. return;
            if (ingrediantData != null)
            {
                if (ingrediantData.ID == QC_Data.ContentTargetId)
                {
                    return;
                }
            }

            Addressables.LoadAssetAsync<IngrediantData>(QC_Data.ContentTargetId).Completed += task =>
            {
                ingrediantData = task.Result;
                image_Ingrediant.sprite = ingrediantData.Sprite;
            };
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
                if (instanceProd.Data.ID != QC_Data.ContentTargetId || QC_Data.IsContentClear)
                {
                    break;  // 상호작용 취소
                }

                
                // 현재 진행도에 개수 추가
                if (QC_Data.ProgressdProdsCount.Value < QC_Data.CurrentTargetCount)
                {
                    QC_Data.ProgressdProdsCount.Value += 1;
                    IngrediantInstance popedProd = characterRD.IngrediantStack.Pop();
                    popedProd.MoveToTargetAndShrink(attachPoint);
                }
                else // 필요 재료 수량만큼 다 넣으면 조건 완료처리 후 정지
                {
                    break;
                }

                // 다음 투입까지 딜레이 시간 설정
                yield return new WaitForSeconds(insertDelayTime);

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
        Debug.Log("타일 들어옴");
        StartCoroutine(AutoInserting());
    }
}

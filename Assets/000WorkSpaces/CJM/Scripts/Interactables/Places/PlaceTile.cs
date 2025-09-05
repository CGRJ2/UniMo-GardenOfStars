using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlaceTile : InteractableBase
{
    [SerializeField] Slider progressBar;

    float installingTime;
    float progressedTime;

    public void Init()
    {
        progressBar.gameObject.SetActive(false);
    }

    IEnumerator ProgressingTask()
    {
        // 정지 상태까지 대기했다가 작업 실행
        yield return new WaitUntil(() => !characterRD.IsMove.Value);

        // 작업 시작 시, 진행도 표기
        progressBar.gameObject.SetActive(true);

        while (characterRD != null) // 영역 안에 있을 때 진행
        {
            // 작업 진행 중, 영역 내에서 움직인 경우 대기
            if (characterRD.IsMove.Value)
            {
                progressBar.gameObject.SetActive(false);
                yield return new WaitUntil(() => !characterRD.IsMove.Value);
                progressBar.gameObject.SetActive(true);
            }

            // 손에 건물이 없을 시, continue
            if (characterRD.IngrediantStack.Peek() == null) // <- 손에 든 재료가 건물이 아닐 때,
            {
                continue;
            }


            // 작업 영역 밖으로 나가는 경우
            if (characterRD == null) break;

            progressedTime += Time.deltaTime;

            if (installingTime < progressedTime)
            {
                CompleteTask(); // 결과물 생성
                progressedTime = 0; // 진행도 초기화
            }

            // 진행도 게이지 업데이트
            progressBar.value = progressedTime / installingTime;

            yield return null;
        }

        // 진행도 표기 비활성화
        progressBar.gameObject.SetActive(false);
        yield return null;
    }

    public void CompleteTask()
    {
        // 설치 완료
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);

        StartCoroutine(ProgressingTask());
    }
}

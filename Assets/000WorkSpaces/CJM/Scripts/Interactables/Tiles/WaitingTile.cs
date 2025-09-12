using KYS;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaitingTile : InteractableBase
{
    [SerializeField] CircularProgressUI progressBar;
    [SerializeField] float interactTime;
    [SerializeField] float progressedTime;

    public Action WaitingCompletedAction;

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
            progressBar.SetValue(progressedTime / interactTime);

            yield return null;
        }

        // 진행도 표기 비활성화
        progressBar.gameObject.SetActive(false);
        yield return null;
    }

    public void CompleteTask()
    {
        WaitingCompletedAction?.Invoke();
    }

    public override void Enter_PersonalTask(CharaterRuntimeData characterRuntimeData)
    {
        base.Enter_PersonalTask(characterRuntimeData);

        StartCoroutine(ProgressingTask());
    }
}

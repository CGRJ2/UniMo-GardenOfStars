using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveChack : MonoBehaviour
{
    public GameObject[] active;
    public GameObject panelopen;

    private void Awake()
    {
        //panelopen = GetComponent<GameObject>();
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.F))
        {
            Debug.Log("지역 이동에 대한 대화를 진행한다는 가정 키 입력");
            panelopen.SetActive(true);
        }
    }
    public void chack() // 미사용할듯.
    {
        List<bool> get = StageManager.instance.GetUnlockStates();
        for (int i = 0; i < active.Length; i++)
        {
            active[i+1].SetActive(get[i]);// 기초 값은 항상 언락중이어야함 그러니 스테이지0 은 항상 열려있음으로 제외
            // 제외된 다음 수 스테이지 부터 해당 스테이지의 언락여부를 결정 함. 0스테이지를 클리어 했을시 1스테이지로
            //이동 가능하게 함을 +1로 구성.
        }
    }
}

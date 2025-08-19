using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;

public class QuestManager : Singleton<QuestManager>
{
    // 배열 형태와 현재 index를 지정하도록
    private LinkedList<Quest> _questList;
    // 현재
    private LinkedListNode<Quest> _currentQuest;

    // 목표치

    private void Awake()
    {
        base.SingletonInit();
        Init();
    }

    private void Init()
    {
        // 초기화
    }

    public void GetQuestsInRegion(string regionId)
    {
        // DB에서 해당 지역의 퀘스트 목록을 가져와서 
        // _questList 에 목록 지정

        // _questList를 돌면서
        // 미완인 퀘스트를 만나면 node 지정
    }
}

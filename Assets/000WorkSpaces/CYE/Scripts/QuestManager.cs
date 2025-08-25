using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameQuest;

public class QuestManager : Singleton<QuestManager>
{
    private Quest[] _currentQuestList;
    private int _currentQuestIndex;
    private int _targetCompleteCount;

    private void Awake()
    {
        base.SingletonInit();
        Init();
    }

    private void Init()
    {
        // 초기화
        // 현재 Scene의 Npc를 검색하여 가져옴
    }

    public void SetQuestsInRegion(string regionId)
    {
        // DB에서 regionId를 통해 해당 지역의 퀘스트 목록을 가져온다
        // InitQuestList(목록 수);
        // 가져온 목록을 _currentQuestList에 지정한다
        // 

        // _currentQuestList를 돌면서
        // 미완인 퀘스트를 만나면 node 지정
    }

    public Quest GetCurrentQuest()
    {
        return _currentQuestList[_currentQuestIndex];
    }

    private void InitQuestList(int listCount)
    {
        _currentQuestList = new Quest[listCount];
        _currentQuestIndex = 0;
        _targetCompleteCount = listCount;
    }
}

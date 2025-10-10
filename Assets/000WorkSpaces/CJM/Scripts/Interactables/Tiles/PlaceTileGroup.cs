using GameQuest;
using UnityEngine;

public class PlaceTileGroup : MonoBehaviour
{
    PlaceTile[] _Tiles;
    [Header("해당 순번의 퀘스트를 클리어 시 언락(0 기입 시, 기본 언락)")]
    [SerializeField] int _TargetQuestIndex;


    private void Awake()
    {
        _Tiles = GetComponentsInChildren<PlaceTile>();
        foreach (PlaceTile tile in _Tiles)
        {
            tile._parentGroup = this;
        }
    }

    public bool IsUnlocked()
    {
        var questList = Manager.firebase.UserData.CurStageData.Npc.QuestList;

        // 설정한 Index가 0 이하면 공터 바로 언락
        if (_TargetQuestIndex - 1 < 0)
        {
            return true;
        }
        else if (_TargetQuestIndex - 1 >= questList.Count)
        {
            Debug.LogError($"설정한 공터 그룹({name})의 언락 조건이 퀘스트 수보다 높습니다. 설정한 퀘스트 순번: {_TargetQuestIndex}/ 퀘스트 개수: {questList.Count}");
            return false;
        }

        if (questList.List[_TargetQuestIndex - 1].State == QuestState.TalkEnd) return true;
        else return false;
    }
}

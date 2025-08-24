using UnityEngine;

public static class Manager
{
    public static GameManager game => GameManager.Instance;
    public static PoolManager pool => PoolManager.Instance;
    public static PlayerManager player => PlayerManager.Instance;
    public static BuildingManager buildings => BuildingManager.Instance;
    public static QuestManager quest => QuestManager.Instance;
    public static NpcManager npc => NpcManager.Instance;
    public static StageManager stage => StageManager.Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initailize()
    {
        GameManager.CreateInstance();
        PoolManager.CreateInstance();
        PlayerManager.CreateInstance();
        BuildingManager.CreateInstance();
        StageManager.CreateInstance();
        QuestManager.CreateInstance();
        NpcManager.CreateInstance();
    }
}
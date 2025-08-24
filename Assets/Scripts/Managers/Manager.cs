using UnityEngine.SceneManagement;
using UnityEngine;

public static class Manager
{
    public static GameManager game => GameManager.Instance;
    public static PoolManager pool => PoolManager.Instance;
    public static QuestManager quest => QuestManager.Instance;
    public static NpcManager npc => NpcManager.Instance;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initailize()
    {
        GameManager.CreateInstance();
        PoolManager.CreateInstance();
        // QuestManager.CreateInstance();
        NpcManager.CreateInstance();
    }
}
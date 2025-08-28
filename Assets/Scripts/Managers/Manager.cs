using UnityEngine;
using KYS;

public static class Manager
{
    public static GameManager game => GameManager.Instance;
    public static PoolManager pool => PoolManager.Instance;
    public static PlayerManager player => PlayerManager.Instance;
    public static BuildingManager buildings => BuildingManager.Instance;
    public static QuestManager quest => QuestManager.Instance;
    public static NpcManager npc => NpcManager.Instance;
    public static UIManager ui => UIManager.Instance;
    public static LocalizationManager localization => LocalizationManager.Instance;
    public static CameraManager camera => CameraManager.Instance;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initailize()
    {
        GameManager.CreateInstance();
        PoolManager.CreateInstance();
        PlayerManager.CreateInstance();
        BuildingManager.CreateInstance();
        QuestManager.CreateInstance();
        NpcManager.CreateInstance();
        UIManager.CreateInstance();
        LocalizationManager.CreateInstance();
        CameraManager.CreateInstance();
    }
}
using UnityEngine;

public static class Manager
{
    public static GameManager game => GameManager.Instance;
    public static PoolManager pool => PoolManager.Instance;
    public static PlayerManager player => PlayerManager.Instance;
    public static BuildingManager buildings => BuildingManager.Instance;
    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initailize()
    {
        GameManager.CreateInstance();
        PoolManager.CreateInstance();
        PlayerManager.CreateInstance();
        BuildingManager.CreateInstance();
    }
}
using UnityEngine.SceneManagement;
using UnityEngine;
using KYS;

public static class Manager
{
    public static GameManager game => GameManager.Instance;
    public static PoolManager pool => PoolManager.Instance;
    public static PlayerManager player => PlayerManager.Instance;
    public static UIManager ui => UIManager.Instance;
    public static LocalizationManager localization => LocalizationManager.Instance;

    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initailize()
    {
        GameManager.CreateInstance();
        PoolManager.CreateInstance();
        PlayerManager.CreateInstance();
        UIManager.CreateInstance();
        LocalizationManager.CreateInstance();
      
    }
}
using UnityEngine.SceneManagement;
using UnityEngine;

public static class Manager
{
    public static GameManager game => GameManager.Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initailize()
    {
        GameManager.CreateInstance();
    }
}
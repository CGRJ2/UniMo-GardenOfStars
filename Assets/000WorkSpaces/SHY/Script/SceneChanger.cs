using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class SceneChanger : Singleton<SceneChanger>
{
    //public static SceneChanger instance; //임시 사용.
    public int CurSceneID { get; private set; }
    public string CurSceneName { get; private set; }
    private void Awake()
    {
        //if (instance ==null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //   Destroy(gameObject);
        //}
        base.SingletonInit();
    }

    public void LoadSceneByName(string sceneName)
    {
        //SceneManager.LoadScene(sceneName); // 기존 씬은 언로드됨
        Addressables.LoadSceneAsync(sceneName).Completed += task =>
        {
            CurSceneName = sceneName;
        };
    }

    public void LoadSceneByIndex(int sceneIndex)
    {

        //SceneManager.LoadScene(sceneIndex);
        Addressables.LoadSceneAsync($"MapScene_Stage0{sceneIndex}").Completed += task =>
        {
            CurSceneID = sceneIndex;
        };
    }
    public void GoBaseScene()
    {
        Addressables.LoadSceneAsync("StageScene");
        
    }
    public void SetTarget(int id ,string name)
    {
        CurSceneID = id;
        CurSceneName = name;
    }
}

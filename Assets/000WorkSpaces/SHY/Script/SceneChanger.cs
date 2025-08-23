using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
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
        Addressables.LoadSceneAsync($"stage{sceneIndex}").Completed += task =>
        {
            CurSceneID = sceneIndex;
        };


    }


}

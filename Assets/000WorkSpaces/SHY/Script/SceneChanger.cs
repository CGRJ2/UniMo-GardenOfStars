using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    //public static SceneChanger instance; //임시 사용.
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
        SceneManager.LoadScene(sceneName); // 기존 씬은 언로드됨
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    //public static SceneChanger instance; //�ӽ� ���.
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
        SceneManager.LoadScene(sceneName); // ���� ���� ��ε��
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

}

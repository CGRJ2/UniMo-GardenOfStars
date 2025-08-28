using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TestTitle : MonoBehaviour
{
    [SerializeField] Button btn_Self;
    [SerializeField] CanvasGroup testPanel;

    private void Awake()
    {
        StartCoroutine(TestBlockFade());
        btn_Self.onClick.AddListener(() =>
        {
            StartCoroutine(Manager.game.Temp_InGameLoad());
        });
    }

    IEnumerator TestBlockFade()
    {
        yield return new WaitUntil(() => Manager.game.initialized);

        float time = 0;
        while (time < 2f)
        {
            time += Time.deltaTime;
            testPanel.alpha = 1f - time / 2f;
            Debug.Log(1f - time / 2f);
            yield return null;
        }
        testPanel.gameObject.SetActive(false);
    }
}

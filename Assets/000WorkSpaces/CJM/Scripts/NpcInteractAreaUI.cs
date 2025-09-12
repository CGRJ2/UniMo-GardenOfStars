using KYS;
using UnityEngine;
using UnityEngine.UI;

public class NpcInteractAreaUI : MonoBehaviour
{
    [SerializeField] Button btn_Talk;

    public void Init()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        btn_Talk.onClick.AddListener(StartConversation);
    }

    public void StartConversation()
    {
        
    }
}

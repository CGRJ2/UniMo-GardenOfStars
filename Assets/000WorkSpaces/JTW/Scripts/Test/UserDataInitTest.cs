using UnityEngine;

public class UserDataInitTest : MonoBehaviour
{
    public UserData UserData;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("K");
            UserData = new UserData("UserData", "");
        }
    }
}

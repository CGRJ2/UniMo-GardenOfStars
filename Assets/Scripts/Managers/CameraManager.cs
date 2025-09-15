using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineVirtualCamera cam_NpcFocus;
    public CinemachineVirtualCamera cam_PlayerFocus;
    private void Awake() => Init();

    void Init()
    {
        base.SingletonInit();
    }

    public void FocusPlayer()
    {
        cam_PlayerFocus.Priority = 11;
        cam_NpcFocus.Priority = 10;
    }

    public void FocusNPC()
    {
        cam_PlayerFocus.Priority = 10;
        cam_NpcFocus.Priority = 11;
    }

}

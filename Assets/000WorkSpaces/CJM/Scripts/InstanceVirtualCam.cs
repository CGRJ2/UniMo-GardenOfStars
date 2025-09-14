using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceVirtualCam : MonoBehaviour
{
    [SerializeField] CameraType type;

    private void Awake() => Init();

    void Init()
    {
        CinemachineVirtualCamera virCam = GetComponent<CinemachineVirtualCamera>();

        switch (type)
        {
            case CameraType.PlayerCam:
                Manager.camera.cam_PlayerFocus = virCam;
                break;
            case CameraType.NpcFocusCam:
                Manager.camera.cam_NpcFocus = virCam;
                break;
        }
    }
}

public enum CameraType
{
    PlayerCam, NpcFocusCam
}
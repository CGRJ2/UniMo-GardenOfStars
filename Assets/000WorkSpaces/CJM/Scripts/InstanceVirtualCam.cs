using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceVirtualCam : MonoBehaviour
{
    [Header("플레이어 카메라인지 NPC 카메라인지 여부(카메라 많아지면 enum타입으로 수정)")]
    [SerializeField] bool isPlayerCam;

    private void Awake() => Init();

    void Init()
    {
        CinemachineVirtualCamera virCam = GetComponent<CinemachineVirtualCamera>();
        if (isPlayerCam)
            Manager.camera.cam_PlayerFocus = virCam;
        else
            Manager.camera.cam_NpcFocus = virCam;
    }
}

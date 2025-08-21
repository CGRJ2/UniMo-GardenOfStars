using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera _cam;   // 비워두면 자동으로 main 사용
    [SerializeField] private Joystick _joy;

    [Header("Test용")]
    [SerializeField] private PlayerManager _playerManager;

    private PlayerRunTimeData _data;
    public PlayerRunTimeData Data => _data;

    // 호환성을 위해 일단 프로퍼티로 남겨둠.
    public Stack<IngrediantInstance> ingrediantStack => _data.IngrediantStack;
    public Transform prodsAttachPoint;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb ??= GetComponent<Rigidbody>();
        if (_cam == null) _cam = Camera.main;
        _data = GetComponent<PlayerRunTimeData>();
    }

    private void FixedUpdate()
    {
        _rb.velocity = _data.Direction * _playerManager.Data.MoveSpeed;
    }

    private void Update()
    {
        float x = 0;
        float z = 0;

        // 조이스틱이 우선순위 입력 감지
        if (_joy.Horizontal != 0) 
            x = _joy.Horizontal;
        else if (Input.GetAxis("Horizontal") != 0)
            x = Input.GetAxis("Horizontal");

        if (_joy.Vertical != 0)
            z = _joy.Vertical;
        else if (Input.GetAxis("Vertical") != 0)
            z = Input.GetAxis("Vertical");


        // 카메라 기준의 우/앞 벡터를 XZ 평면에 투영
        Vector3 camForward = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(_cam.transform.right, Vector3.up).normalized;

        // 입력을 카메라 기준으로 변환
        _data.Direction = (camRight * x + camForward * z);
        if (_data.Direction.sqrMagnitude > 1f) _data.Direction.Normalize();   // 대각선 속도 보정

        // 입력 없으면 정지
        if (_data.Direction.sqrMagnitude < 0.01f) _data.Direction = Vector3.zero;
    }


}

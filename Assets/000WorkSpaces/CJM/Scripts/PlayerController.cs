using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera cam;   // 비워두면 자동으로 main 사용
    [SerializeField] private Joystick joy;
    [SerializeField] private float speed;
    [SerializeField] private PlayerView view;
    private Vector3 dir;
    private Rigidbody rb;
    
    private void Awake()
    {
        rb ??= GetComponent<Rigidbody>();
        if (cam == null) cam = Camera.main;
    }

    private void FixedUpdate()
    {
        rb.velocity = dir * speed;
    }

    private void Update()
    {
        float x = 0;
        float z = 0;

        // 조이스틱이 우선순위 입력 감지
        if (joy.Horizontal != 0) 
            x = joy.Horizontal;
        else if (Input.GetAxis("Horizontal") != 0)
            x = Input.GetAxis("Horizontal");

        if (joy.Vertical != 0)
            z = joy.Vertical;
        else if (Input.GetAxis("Vertical") != 0)
            z = Input.GetAxis("Vertical");


        // 카메라 기준의 우/앞 벡터를 XZ 평면에 투영
        Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        // 입력을 카메라 기준으로 변환
        dir = (camRight * x + camForward * z);
        if (dir.sqrMagnitude > 1f) dir.Normalize();   // 대각선 속도 보정

        // 입력 없으면 정지
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.zero;

        if (view != null)
        {
            // 움직일 때만 부드럽게 회전시키고, 정지 시에는 마지막 바라보던 방향 유지
            view.SetForwardToMoveDir(dir, Time.deltaTime);
        }
    }


}

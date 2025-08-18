using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera cam;   // ����θ� �ڵ����� main ���
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

        // ���̽�ƽ�� �켱���� �Է� ����
        if (joy.Horizontal != 0) 
            x = joy.Horizontal;
        else if (Input.GetAxis("Horizontal") != 0)
            x = Input.GetAxis("Horizontal");

        if (joy.Vertical != 0)
            z = joy.Vertical;
        else if (Input.GetAxis("Vertical") != 0)
            z = Input.GetAxis("Vertical");


        // ī�޶� ������ ��/�� ���͸� XZ ��鿡 ����
        Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        // �Է��� ī�޶� �������� ��ȯ
        dir = (camRight * x + camForward * z);
        if (dir.sqrMagnitude > 1f) dir.Normalize();   // �밢�� �ӵ� ����

        // �Է� ������ ����
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.zero;

        if (view != null)
        {
            // ������ ���� �ε巴�� ȸ����Ű��, ���� �ÿ��� ������ �ٶ󺸴� ���� ����
            view.SetForwardToMoveDir(dir, Time.deltaTime);
        }
    }


}

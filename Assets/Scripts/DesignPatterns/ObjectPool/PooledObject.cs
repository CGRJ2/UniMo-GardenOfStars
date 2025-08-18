using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public ObjectPool ParentPool { get; set; }

    private bool isInit = false;

    private void OnEnable()
    {
        if (!isInit)
        {
            return;
        }
        OnPooledEnable();
    }
    private void OnDisable()
    {
        if (!isInit)
        {
            isInit = true;
            return;
        }
        OnPooledDisable();
    }

    // OnEnable ��, �����ؾ��� ����� �ִٸ� �������̵� �ؼ� �߰� (�������� �߰��� ����� ���⿡ �߰�)
    protected virtual void OnPooledEnable() { }

    

    // OnDisable ��, �����ؾ��� ����� �ִٸ� �������̵� �ؼ� �߰� (�������� �߰��� ����� ���⿡ �߰�)
    protected virtual void OnPooledDisable() { }
}

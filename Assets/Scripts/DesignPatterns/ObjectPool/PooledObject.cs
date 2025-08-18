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

    // OnEnable 시, 구현해야할 기능이 있다면 오버라이드 해서 추가 (공통으로 추가할 기능은 여기에 추가)
    protected virtual void OnPooledEnable() { }

    

    // OnDisable 시, 구현해야할 기능이 있다면 오버라이드 해서 추가 (공통으로 추가할 기능은 여기에 추가)
    protected virtual void OnPooledDisable() { }
}

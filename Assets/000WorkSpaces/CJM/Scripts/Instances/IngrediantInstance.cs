using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngrediantInstance : PooledObject
{
    [field: SerializeField] public IngrediantSO ingrediantSO { get; private set; }
    public int count;
    [SerializeField] float destroyTime = 120f;
    float proceededtime;

    public GameObject owner; // CharacterBase �ϼ� �� ���� ����

    void Update()
    {
        if (destroyTime < -90) return; // destroyTime�� -100���� �����Ͽ�, �����ֱ� ��Ȱ��ȭ ����

        proceededtime += Time.deltaTime;
        if (destroyTime < proceededtime) Despawn();

        // �׽�Ʈ �뵵
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (owner != null) Despawn();
        }
    }

    protected override void OnPooledEnable()
    {
        base.OnPooledEnable();
        proceededtime = 0;
    }

    protected override void OnPooledDisable()
    {
        base.OnPooledDisable();
    }

    public void Despawn()
    {
        owner = null;

        ParentPool.ReturnPooledObj(gameObject); // �� ������� ����
    }

    public void SetIngrediantSO(IngrediantSO ingrediantSO)
    {
        this.ingrediantSO = ingrediantSO;
    }
}

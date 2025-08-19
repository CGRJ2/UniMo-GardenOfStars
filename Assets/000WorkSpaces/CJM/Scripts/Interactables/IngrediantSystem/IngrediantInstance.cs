using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngrediantInstance : PooledObject
{
    [field: SerializeField] public IngrediantSO ingrediantSO { get; private set; }
    public int count;
    [SerializeField] float destroyTime = 120f;
    float proceededtime;

    public GameObject owner; // CharacterBase 완성 후 변수 수정

    void Update()
    {
        if (destroyTime < -90) return; // destroyTime을 -100으로 설정하여, 생존주기 비활성화 가능

        proceededtime += Time.deltaTime;
        if (destroyTime < proceededtime) Despawn();

        // 테스트 용도
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

        ParentPool.ReturnPooledObj(gameObject); // 이 방법으로 디스폰
    }

    public void SetIngrediantSO(IngrediantSO ingrediantSO)
    {
        this.ingrediantSO = ingrediantSO;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveChack : MonoBehaviour
{
    public GameObject[] active;
    public GameObject panelopen;

    private void Awake()
    {
        //panelopen = GetComponent<GameObject>();
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.F))
        {
            Debug.Log("���� �̵��� ���� ��ȭ�� �����Ѵٴ� ���� Ű �Է�");
            panelopen.SetActive(true);
        }
    }
    public void chack() // �̻���ҵ�.
    {
        List<bool> get = StageManager.instance.GetUnlockStates();
        for (int i = 0; i < active.Length; i++)
        {
            active[i+1].SetActive(get[i]);// ���� ���� �׻� ������̾���� �׷��� ��������0 �� �׻� ������������ ����
            // ���ܵ� ���� �� �������� ���� �ش� ���������� ������θ� ���� ��. 0���������� Ŭ���� ������ 1����������
            //�̵� �����ϰ� ���� +1�� ����.
        }
    }
}

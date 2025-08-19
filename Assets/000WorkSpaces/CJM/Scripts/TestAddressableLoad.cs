using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TestAddressableLoad : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(test());

    }

    /////////////
    System.Collections.IEnumerator test()
    {
        // �ʱ�ȭ
        yield return Addressables.InitializeAsync();

        // ������� �ִ� �� üũ
        UnityEngine.Debug.Log("������Ʈ �ڵ� Ȯ��");
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Result.Count > 0)
        {
            UnityEngine.Debug.Log("�߰��� Ű �ڵ� Ȯ��");
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);   // ������Ʈ �� īŻ�α� ����
            yield return updateHandle;

            var locators = updateHandle.Result; // IList<IResourceLocator>
            var allKeys = new List<object>();
            foreach (var loc in locators)
                allKeys.AddRange(loc.Keys);

            // �ٿ� ������ Ȯ��
            //var sizeHandle = Addressables.GetDownloadSizeAsync(allKeys);

            //UnityEngine.Debug.Log(sizeHandle.Result);

            UnityEngine.Debug.Log("�ٿ�ε� ����");
            // �ٿ�ε�
            Addressables.DownloadDependenciesAsync(updateHandle.Result).Completed += aa;
        }

        //if (������ == 0) yield break;

        //yield return Addressables.DownloadDependenciesAsync();
    }

    public void aa(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle handle)
    {
        UnityEngine.Debug.Log(handle.Result);
    }
}

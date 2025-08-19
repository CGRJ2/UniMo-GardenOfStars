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
        // 초기화
        yield return Addressables.InitializeAsync();

        // 변경사항 있는 지 체크
        UnityEngine.Debug.Log("업데이트 핸들 확인");
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Result.Count > 0)
        {
            UnityEngine.Debug.Log("추가된 키 핸들 확인");
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);   // 업데이트 된 카탈로그 갱신
            yield return updateHandle;

            var locators = updateHandle.Result; // IList<IResourceLocator>
            var allKeys = new List<object>();
            foreach (var loc in locators)
                allKeys.AddRange(loc.Keys);

            // 다운 사이즈 확인
            //var sizeHandle = Addressables.GetDownloadSizeAsync(allKeys);

            //UnityEngine.Debug.Log(sizeHandle.Result);

            UnityEngine.Debug.Log("다운로드 진행");
            // 다운로드
            Addressables.DownloadDependenciesAsync(updateHandle.Result).Completed += aa;
        }

        //if (사이즈 == 0) yield break;

        //yield return Addressables.DownloadDependenciesAsync();
    }

    public void aa(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle handle)
    {
        UnityEngine.Debug.Log(handle.Result);
    }
}

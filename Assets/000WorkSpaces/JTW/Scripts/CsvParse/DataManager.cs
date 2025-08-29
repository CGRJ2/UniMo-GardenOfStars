using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class DataManager : Singleton<DataManager>
{
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
    }

    private async Task<string> GetDataString(bool isAdressable)
    {
        if (isAdressable)
        {
            AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(_workerAdress);
            try
            {
                TextAsset asset = await handle.Task;

                return asset.text;
            }
            catch
            {
                Debug.Log("[DataManager] Addressable 루틴 실패");
                return null;
            }
            finally
            {
                Addressables.Release(handle);
            }
        }
        else
        {
            var tcs = new TaskCompletionSource<UnityWebRequestAsyncOperation>();

            UnityWebRequest request = UnityWebRequest.Get(_workerDataTableURL);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            operation.completed += _ => tcs.SetResult(operation);

            await tcs.Task;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"[DataManager] 다운로드 루틴 실패 : {request.error}");
                return null;
            }

            return request.downloadHandler.text;
        }
    }
}

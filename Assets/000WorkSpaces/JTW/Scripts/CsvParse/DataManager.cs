using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class DataManager : Singleton<DataManager>
{
    public bool IsDataInit
    {
        get
        {
            return Worker.IsInit
                && CharacterLv.IsInit
                && Dialogue.IsInit
                && BuildingLocalization.IsInit
                && IngrediantLocalization.IsInit
                && Npc.IsInit
                && Quest.IsInit
                && QuestContent.IsInit
                && WorkerUpgradeCost.IsInit
                && WorkerEmployCost.IsInit
                && Stage.IsInit
                && Character.IsInit
                && PlayerUpgradeCost.IsInit
                && Player.IsInit
                && UpgradeMulti.IsInit
                && Buy.IsInit
                && CharacterSkin.IsInit
                && EquipSkin.IsInit;
        }
    }

    private void Awake()
    {
        StartCoroutine(WaitInit());
    }

    IEnumerator WaitInit()
    {
        yield return new WaitUntil(() => Manager.game.initialized);
        Init();
    }

    public void Init()
    {
        WorkerRoutine();
        CharacterLvRoutine();
        DialogueRoutine();
        BuildingLocalizationRoutine();
        IngrediantLocalizationRoutine();
        //NpcTextLinesRoutine(); // 반드시 NpcRoutine 이전에 실행되어야함.
        NpcRoutine();
        QuestRoutine();
        QuestContentRoutine();
        WorkerUpgradeCostRoutine();
        WorkerEmployCostRoutine();
        StageRoutine();
        CharacterRoutine();
        PlayerUpgradeCostRoutine();
        PlayerRoutine();
        UpgradeMultiRoutine();
        BuildingDataInitRoutine();
        IngrediantDataInitRoutine();
        BuyRoutine();
        CharacterSkinCsvRoutine();
        EquipSkinCsvRoutine();
    }

    private async Task<string> GetDataString(bool isAdressable, string address)
    {
        if (isAdressable)
        {
            AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(address);
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

            UnityWebRequest request = UnityWebRequest.Get(address);
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

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
            return Worker != null && Worker.IsInit
                && CharacterLv != null && CharacterLv.IsInit
                && Dialogue != null && Dialogue.IsInit
                && BuildingLocalization != null && BuildingLocalization.IsInit
                && IngrediantLocalization != null && IngrediantLocalization.IsInit
                && Npc != null && Npc.IsInit
                && Quest != null && Quest.IsInit
                && QuestContent != null && QuestContent.IsInit
                && WorkerUpgradeCost != null && WorkerUpgradeCost.IsInit
                && WorkerEmployCost != null && WorkerEmployCost.IsInit
                && Stage != null && Stage.IsInit
                && Character != null && Character.IsInit
                && PlayerUpgradeCost != null && PlayerUpgradeCost.IsInit
                && Player != null && Player.IsInit
                && UpgradeMulti != null && UpgradeMulti.IsInit
                && Buy != null && Buy.IsInit
                && CharacterSkin != null && CharacterSkin.IsInit
                && EquipSkin != null && EquipSkin.IsInit;
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

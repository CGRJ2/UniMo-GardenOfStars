using KYS;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// DataManager의 Dialogue 관련 확장
/// </summary>
public partial class DataManager
{
    [SerializeField] private bool _isDialogueAddressable;

    // 구글 스프레드 시트 다운로드 주소
    private const string _dialogueDataTableURL = "https://docs.google.com/spreadsheets/d/1WYUtLt6DeDwyz0qJYcoNi3cWAWHa2qb7pwhpDcIl62U/export?format=csv&gid=800218594";

    // Addressable 에셋 주소
    private const string _dialogueAddress = "DialogueData_CSV";

    public DataTableParser<DialogueDataCsv> Dialogue;

    // Addressable 이미지 관리
    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();
    private Dictionary<string, AsyncOperationHandle<Sprite>> loadingHandles = new Dictionary<string, AsyncOperationHandle<Sprite>>();

    private async void DialogueRoutine()
    {
        string dataCsv;

        if (_isDialogueAddressable)
        {
            dataCsv = await GetDataString(true, _dialogueAddress);
        }
        else
        {
            dataCsv = await GetDataString(false, _dialogueDataTableURL);
        }

        if (dataCsv != null)
        {
            Dialogue = new DataTableParser<DialogueDataCsv>((words, dict) =>
            {
                DialogueDataCsv dialogue = new DialogueDataCsv();

                dialogue.Id = GetFieldValue(words, dict, "Id");
                dialogue.NpcId = GetFieldValue(words, dict, "NpcId");
                dialogue.StageId = GetFieldValue(words, dict, "StageId");
                dialogue.NodeType = GetFieldValue(words, dict, "NodeType");
                dialogue.Speaker = GetFieldValue(words, dict, "Speaker");
                dialogue.Speaker_Korea = GetFieldValue(words, dict, "Speaker_Korea");
                dialogue.Speaker_English = GetFieldValue(words, dict, "Speaker_English");
                dialogue.DialogueText = GetFieldValue(words, dict, "DialogueText");
                dialogue.DialogueText_Korea = GetFieldValue(words, dict, "DialogueText_Korea");
                dialogue.DialogueText_English = GetFieldValue(words, dict, "DialogueText_English");
                dialogue.NextNodeId = GetFieldValue(words, dict, "NextNodeId");
                dialogue.NextNodeIdAfterClear = GetFieldValue(words, dict, "NextNodeIdAfterClear");
                dialogue.NextNodeIdCondition = GetFieldValue(words, dict, "NextNodeIdCondition");
                dialogue.ChoiceText1 = GetFieldValue(words, dict, "ChoiceText1");
                dialogue.ChoiceText1_Korea = GetFieldValue(words, dict, "ChoiceText1_Korea");
                dialogue.ChoiceText1_English = GetFieldValue(words, dict, "ChoiceText1_English");
                dialogue.ChoiceNext1 = GetFieldValue(words, dict, "ChoiceNext1");
                dialogue.ChoiceText2 = GetFieldValue(words, dict, "ChoiceText2");
                dialogue.ChoiceText2_Korea = GetFieldValue(words, dict, "ChoiceText2_Korea");
                dialogue.ChoiceText2_English = GetFieldValue(words, dict, "ChoiceText2_English");
                dialogue.ChoiceNext2 = GetFieldValue(words, dict, "ChoiceNext2");
                dialogue.ChoiceText3 = GetFieldValue(words, dict, "ChoiceText3");
                dialogue.ChoiceText3_Korea = GetFieldValue(words, dict, "ChoiceText3_Korea");
                dialogue.ChoiceText3_English = GetFieldValue(words, dict, "ChoiceText3_English");
                dialogue.ChoiceNext3 = GetFieldValue(words, dict, "ChoiceNext3");
                dialogue.ChoiceText4 = GetFieldValue(words, dict, "ChoiceText4");
                dialogue.ChoiceText4_Korea = GetFieldValue(words, dict, "ChoiceText4_Korea");
                dialogue.ChoiceText4_English = GetFieldValue(words, dict, "ChoiceText4_English");
                dialogue.ChoiceNext4 = GetFieldValue(words, dict, "ChoiceNext4");
                dialogue.Choice2Condition = GetFieldValue(words, dict, "Choice2Condition");
                dialogue.Choice3Condition = GetFieldValue(words, dict, "Choice3Condition");
                dialogue.Choice4Condition = GetFieldValue(words, dict, "Choice4Condition");
                dialogue.CharacterImage = GetFieldValue(words, dict, "CharacterImage");
                dialogue.CharacterImagePosition = GetFieldValue(words, dict, "CharacterImagePosition");
                dialogue.UseTypingEffect = GetFieldValue(words, dict, "UseTypingEffect");
                dialogue.BackgroundImage = GetFieldValue(words, dict, "BackgroundImage");
                dialogue.ConstellationImage = GetFieldValue(words, dict, "ConstellationImage");
                dialogue.CenterImage = GetFieldValue(words, dict, "CenterImage");
                dialogue.CenterImageDuration = GetFieldValue(words, dict, "CenterImageDuration");
                dialogue.CenterImageFadeInTime = GetFieldValue(words, dict, "CenterImageFadeInTime");
                dialogue.CenterImageFadeOutTime = GetFieldValue(words, dict, "CenterImageFadeOutTime");
                dialogue.HideCharacterImages = GetFieldValue(words, dict, "HideCharacterImages");
                dialogue.AutoAdvanceDelay = float.TryParse(GetFieldValue(words, dict, "AutoAdvanceDelay"), out float delay) ? delay : 0f;
                dialogue.ConditionType = GetFieldValue(words, dict, "ConditionType");
                dialogue.ConditionValue = GetFieldValue(words, dict, "ConditionValue");
                dialogue.EffectType = GetFieldValue(words, dict, "EffectType");
                dialogue.EffectValue = GetFieldValue(words, dict, "EffectValue");

                return dialogue;
            });

            Dialogue.Load(dataCsv);
            //Debug.Log($"[DataManager] Dialogue 데이터 로드 완료 - 총 {Dialogue.Values.Count}개");

            // 이미지 로딩 시작
            await LoadAllCharacterImages();
        }
        else
        {
            Debug.LogError("[DataManager] Dialogue 데이터 로드 실패");
        }
    }


    private string GetFieldValue(string[] fields, Dictionary<string, int> nameToIndexDict, string fieldName)
    {
        if (nameToIndexDict.TryGetValue(fieldName, out int index) && index < fields.Length)
        {
            return fields[index];
        }
        return "";
    }

    /// <summary>
    /// 모든 캐릭터 이미지 로드
    /// </summary>
    private async Task LoadAllCharacterImages()
    {
        if (Dialogue?.Values == null) return;

        var uniqueImageKeys = new HashSet<string>();

        // 고유한 이미지 키 수집
        foreach (var dialogue in Dialogue.Values.Values)
        {
            if (!string.IsNullOrEmpty(dialogue.CharacterImage))
            {
                uniqueImageKeys.Add(dialogue.CharacterImage);
            }
            if (!string.IsNullOrEmpty(dialogue.BackgroundImage))
            {
                uniqueImageKeys.Add(dialogue.BackgroundImage);
            }
            if (!string.IsNullOrEmpty(dialogue.ConstellationImage))
            {
                uniqueImageKeys.Add(dialogue.ConstellationImage);
            }
            if (!string.IsNullOrEmpty(dialogue.CenterImage))
            {
                uniqueImageKeys.Add(dialogue.CenterImage);
            }
        }

        //Debug.Log($"[DataManager] 로드할 대화 이미지: {uniqueImageKeys.Count}개");

        // 병렬로 이미지 로드
        var loadTasks = new List<Task>();
        foreach (var imageKey in uniqueImageKeys)
        {
            loadTasks.Add(LoadCharacterImageAsync(imageKey));
        }

        await Task.WhenAll(loadTasks);
        //Debug.Log("[DataManager] 모든 대화 이미지 로드 완료");
    }

    /// <summary>
    /// 이미지 캐시에서 직접 가져오기 (동기)
    /// </summary>
    public Sprite GetCachedCharacterImage(string imageKey)
    {
        if (string.IsNullOrEmpty(imageKey))
        {
            return null;
        }

        if (imageCache.TryGetValue(imageKey, out Sprite cachedSprite))
        {
            Debug.Log($"[DataManager] 캐시에서 이미지 반환: {imageKey}");
            return cachedSprite;
        }

        Debug.LogWarning($"[DataManager] 캐시에 이미지 없음: {imageKey}");
        return null;
    }

    /// <summary>
    /// 특정 캐릭터 이미지 비동기 로드
    /// </summary>
    public async Task<Sprite> LoadCharacterImageAsync(string imageKey)
    {
        if (string.IsNullOrEmpty(imageKey))
        {
            Debug.LogWarning("[DataManager] 이미지 키가 비어있습니다.");
            return null;
        }

        // 이미 캐시에 있으면 반환
        if (imageCache.TryGetValue(imageKey, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        // 이미 로딩 중이면 기다림
        if (loadingHandles.TryGetValue(imageKey, out AsyncOperationHandle<Sprite> existingHandle))
        {
            await existingHandle.Task;
            return existingHandle.Result;
        }

        try
        {
            if (Addressables.ResourceLocators.Any(locator => locator.Locate(imageKey, typeof(Sprite), out var locations)))
            {
                // Addressable에서 이미지 로드
                var handle = Addressables.LoadAssetAsync<Sprite>(imageKey);
                loadingHandles[imageKey] = handle;

                var sprite = await handle.Task;

                if (sprite != null)
                {
                    imageCache[imageKey] = sprite;
                    //Debug.Log($"[DataManager] 이미지 로드 성공: {imageKey}");
                }
                else
                {
                    Debug.LogWarning($"[DataManager] 이미지 로드 실패: {imageKey}");
                }

                // 로딩 완료 후 핸들 정리
                loadingHandles.Remove(imageKey);
                return sprite;
            }
            else return null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataManager] 이미지 로드 중 오류 발생: {imageKey} - {e.Message}");
            loadingHandles.Remove(imageKey);
            return null;
        }
    }
}

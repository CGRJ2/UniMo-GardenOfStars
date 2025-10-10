using KYS;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// DataManager의 Ingrediant Localization 관련 확장
/// </summary>
public partial class DataManager
{
    [SerializeField] private bool _isIngrediantLocalizationAddressable;

    // 구글 스프레드 시트 다운로드 주소 (필요시)
    private const string _ingrediantLocalizationDataTableURL = "https://docs.google.com/spreadsheets/d/YOUR_SHEET_ID/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _ingrediantLocalizationAddress = "IngrediantDataLocalizationCsv";

    public DataTableParser<IngrediantLocalizationDataCsv> IngrediantLocalization;

    private async void IngrediantLocalizationRoutine()
    {
        //Debug.Log("[DataManager] IngrediantLocalizationRoutine 시작");
        string dataCsv;

        if (_isIngrediantLocalizationAddressable)
        {
            //Debug.Log($"[DataManager] Addressable에서 로드 시도: {_ingrediantLocalizationAddress}");
            dataCsv = await GetDataString(true, _ingrediantLocalizationAddress);
        }
        else
        {
            Debug.Log($"[DataManager] URL에서 로드 시도: {_ingrediantLocalizationDataTableURL}");
            dataCsv = await GetDataString(false, _ingrediantLocalizationDataTableURL);
        }

        if (dataCsv != null)
        {
            //Debug.Log($"[DataManager] CSV 데이터 로드 성공, 길이: {dataCsv.Length}");
            IngrediantLocalization = new DataTableParser<IngrediantLocalizationDataCsv>((words, dict) =>
            {
                IngrediantLocalizationDataCsv ingrediant = new IngrediantLocalizationDataCsv();

                ingrediant.IngrediantID = GetFieldValue(words, dict, "ID");
                ingrediant.NameKorean = GetFieldValue(words, dict, "Name_Korean");
                ingrediant.NameEnglish = GetFieldValue(words, dict, "Name_English");

                return ingrediant;
            });

            IngrediantLocalization.Load(dataCsv);
            //Debug.Log($"[DataManager] Ingrediant Localization 데이터 로드 완료 - 총 {IngrediantLocalization.Values.Count}개");
            
            // 로드된 데이터 확인
            foreach (var kvp in IngrediantLocalization.Values)
            {
                //Debug.Log($"[DataManager] 로드된 재료: {kvp.Key} -> {kvp.Value.NameKorean} / {kvp.Value.NameEnglish}");
            }
        }
        else
        {
            Debug.LogError("[DataManager] Ingrediant Localization 데이터 로드 실패");
        }
    }

    /// <summary>
    /// 재료 ID로 번역된 이름을 가져옵니다
    /// </summary>
    public string GetIngrediantLocalizedName(string ingrediantID, SystemLanguage language = SystemLanguage.Korean)
    {
        // 재료 데이터에서 직접 번역 정보 가져오기
        if (Ingrediant?.Values != null)
        {
            if (Ingrediant.Values.TryGetValue(ingrediantID, out IngrediantData ingrediantData))
            {
                string result = language == SystemLanguage.Korean ? ingrediantData.Name_KR : ingrediantData.Name_EN;
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
        }

        // 기존 IngrediantLocalization 데이터 폴백 (호환성 유지)
        if (IngrediantLocalization?.Values != null)
        {
            if (IngrediantLocalization.Values.TryGetValue(ingrediantID, out IngrediantLocalizationDataCsv data))
            {
                string result = language == SystemLanguage.Korean ? data.NameKorean : data.NameEnglish;
                return result;
            }
        }

        Debug.LogWarning($"[DataManager] 재료 ID '{ingrediantID}'에 대한 번역 데이터를 찾을 수 없습니다.");
        return ingrediantID; // 폴백
    }

}

/// <summary>
/// CSV 파싱용 재료 번역 데이터
/// </summary>
public class IngrediantLocalizationDataCsv : IUsableId
{
    public string IngrediantID;
    public string NameKorean;
    public string NameEnglish;

    public string GetId()
    {
        return IngrediantID;
    }
}

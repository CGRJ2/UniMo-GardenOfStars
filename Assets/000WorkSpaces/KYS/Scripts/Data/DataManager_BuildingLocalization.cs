using KYS;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// DataManager의 Building Localization 관련 확장
/// </summary>
public partial class DataManager
{
    [SerializeField] private bool _isBuildingLocalizationAddressable;

    // 구글 스프레드 시트 다운로드 주소 (필요시)
    private const string _buildingLocalizationDataTableURL = "https://docs.google.com/spreadsheets/d/YOUR_SHEET_ID/export?format=csv&gid=0";

    // Addressable 에셋 주소
    private const string _buildingLocalizationAddress = "BuildingDataLocalizationCsv";

    public DataTableParser<BuildingLocalizationDataCsv> BuildingLocalization;

    private async void BuildingLocalizationRoutine()
    {
        string dataCsv;

        if (_isBuildingLocalizationAddressable)
        {
            dataCsv = await GetDataString(true, _buildingLocalizationAddress);
        }
        else
        {
            dataCsv = await GetDataString(false, _buildingLocalizationDataTableURL);
        }

        if (dataCsv != null)
        {
            BuildingLocalization = new DataTableParser<BuildingLocalizationDataCsv>((words, dict) =>
            {
                BuildingLocalizationDataCsv building = new BuildingLocalizationDataCsv();

                building.BuildingID = GetFieldValue(words, dict, "ID");
                building.NameKorean = GetFieldValue(words, dict, "Name_Korean");
                building.NameEnglish = GetFieldValue(words, dict, "Name_English");
                building.DescriptionKorean = GetFieldValue(words, dict, "Description_Korean");
                building.DescriptionEnglish = GetFieldValue(words, dict, "Description_English");

                return building;
            });

            BuildingLocalization.Load(dataCsv);
            //Debug.Log($"[DataManager] Building Localization 데이터 로드 완료 - 총 {BuildingLocalization.Values.Count}개");
            
            // 로드된 데이터 확인
            foreach (var kvp in BuildingLocalization.Values)
            {
                //Debug.Log($"[DataManager] 로드된 건물: {kvp.Key} -> {kvp.Value.NameKorean} / {kvp.Value.NameEnglish}");
            }
        }
        else
        {
            Debug.LogError("[DataManager] Building Localization 데이터 로드 실패");
        }
    }

    /// <summary>
    /// 건물 ID로 번역된 이름을 가져옵니다
    /// </summary>
    public string GetBuildingLocalizedName(string buildingID, SystemLanguage language = SystemLanguage.Korean)
    {
        //Debug.Log($"[DataManager] GetBuildingLocalizedName 호출: {buildingID}, 언어: {language}");
        
        if (BuildingLocalization?.Values == null)
        {
            Debug.LogWarning("[DataManager] Building Localization 데이터가 로드되지 않았습니다.");
            return buildingID; // 폴백
        }

        //Debug.Log($"[DataManager] BuildingLocalization 데이터 로드됨: {BuildingLocalization.Values.Count}개");
        
        // 사용 가능한 건물 ID들 출력
        //Debug.Log($"[DataManager] 사용 가능한 건물 ID들: {string.Join(", ", BuildingLocalization.Values.Keys)}");

        if (BuildingLocalization.Values.TryGetValue(buildingID, out BuildingLocalizationDataCsv data))
        {
            string result = language == SystemLanguage.Korean ? data.NameKorean : data.NameEnglish;
            //Debug.Log($"[DataManager] 번역 성공: {buildingID} -> {result}");
            return result;
        }

        Debug.LogWarning($"[DataManager] 건물 ID '{buildingID}'에 대한 번역 데이터를 찾을 수 없습니다.");
        return buildingID; // 폴백
    }

    /// <summary>
    /// 건물 ID로 번역된 설명을 가져옵니다
    /// </summary>
    public string GetBuildingLocalizedDescription(string buildingID, SystemLanguage language = SystemLanguage.Korean)
    {
        if (BuildingLocalization?.Values == null)
        {
            Debug.LogWarning("[DataManager] Building Localization 데이터가 로드되지 않았습니다.");
            return ""; // 폴백
        }

        if (BuildingLocalization.Values.TryGetValue(buildingID, out BuildingLocalizationDataCsv data))
        {
            return language == SystemLanguage.Korean ? data.DescriptionKorean : data.DescriptionEnglish;
        }

        Debug.LogWarning($"[DataManager] 건물 ID '{buildingID}'에 대한 번역 데이터를 찾을 수 없습니다.");
        return ""; // 폴백
    }
}

/// <summary>
/// CSV 파싱용 건물 번역 데이터
/// </summary>
public class BuildingLocalizationDataCsv : IUsableId
{
    public string BuildingID;
    public string NameKorean;
    public string NameEnglish;
    public string DescriptionKorean;
    public string DescriptionEnglish;

    public string GetId()
    {
        return BuildingID;
    }
}

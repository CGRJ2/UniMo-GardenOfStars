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
        // 건물 데이터에서 직접 번역 정보 가져오기
        if (Building?.Values != null)
        {
            if (Building.Values.TryGetValue(buildingID, out BuildingData buildingData))
            {
                string result = language == SystemLanguage.Korean ? buildingData.Name_KR : buildingData.Name_EN;
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
        }

        // 기존 BuildingLocalization 데이터 폴백 (호환성 유지)
        if (BuildingLocalization?.Values != null)
        {
            if (BuildingLocalization.Values.TryGetValue(buildingID, out BuildingLocalizationDataCsv data))
            {
                string result = language == SystemLanguage.Korean ? data.NameKorean : data.NameEnglish;
                return result;
            }
        }

        Debug.LogWarning($"[DataManager] 건물 ID '{buildingID}'에 대한 번역 데이터를 찾을 수 없습니다.");
        return buildingID; // 폴백
    }

    /// <summary>
    /// 건물 ID로 번역된 설명을 가져옵니다
    /// </summary>
    public string GetBuildingLocalizedDescription(string buildingID, SystemLanguage language = SystemLanguage.Korean)
    {
        // 건물 데이터에서 직접 번역 정보 가져오기
        if (Building?.Values != null)
        {
            if (Building.Values.TryGetValue(buildingID, out BuildingData buildingData))
            {
                string result = language == SystemLanguage.Korean ? buildingData.Description_KR : buildingData.Description_EN;
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
        }

        // 기존 BuildingLocalization 데이터 폴백 (호환성 유지)
        if (BuildingLocalization?.Values != null)
        {
            if (BuildingLocalization.Values.TryGetValue(buildingID, out BuildingLocalizationDataCsv data))
            {
                return language == SystemLanguage.Korean ? data.DescriptionKorean : data.DescriptionEnglish;
            }
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

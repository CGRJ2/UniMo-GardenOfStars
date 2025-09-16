using KYS;
using UnityEngine;

namespace KYS
{
    /// <summary>
    /// 건물 번역을 위한 헬퍼 클래스
    /// LocalizationManager와 DataManager를 활용하여 건물 번역을 처리합니다
    /// </summary>
    public static class BuildingLocalizationHelper
    {
        /// <summary>
        /// 건물 ID로 번역된 이름을 가져옵니다
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <returns>현재 언어에 맞는 건물 이름</returns>
        public static string GetBuildingName(string buildingID)
        {
            //Debug.Log($"[BuildingLocalizationHelper] GetBuildingName 호출: {buildingID}");
            
            // 1. LocalizationManager를 통한 번역 시도
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                string localizationKey = $"building_{buildingID}_name";
                string localizedName = LocalizationManager.Instance.GetText(localizationKey);
                
                //Debug.Log($"[BuildingLocalizationHelper] LocalizationManager 시도: {localizationKey} -> {localizedName}");
                
                // 번역이 성공했고 키와 다른 경우 (실제 번역된 텍스트)
                if (!string.IsNullOrEmpty(localizedName) && localizedName != localizationKey)
                {
                    Debug.Log($"[BuildingLocalizationHelper] LocalizationManager 성공: {localizedName}");
                    return localizedName;
                }
            }

            // 2. DataManager를 통한 번역 시도
            if (Manager.data != null)
            {
                SystemLanguage currentLanguage = LocalizationManager.Instance?.CurrentLanguage ?? SystemLanguage.Korean;
                //Debug.Log($"[BuildingLocalizationHelper] DataManager 시도: {buildingID}, 언어: {currentLanguage}");
                
                // DataManager의 BuildingLocalization 데이터 상태 확인
                if (Manager.data.BuildingLocalization?.Values != null)
                {
                    //Debug.Log($"[BuildingLocalizationHelper] BuildingLocalization 데이터 로드됨: {Manager.data.BuildingLocalization.Values.Count}개");
                }
                else
                {
                    Debug.LogWarning("[BuildingLocalizationHelper] BuildingLocalization 데이터가 null입니다.");
                }
                
                string dataManagerName = Manager.data.GetBuildingLocalizedName(buildingID, currentLanguage);
                //Debug.Log($"[BuildingLocalizationHelper] DataManager 결과: {dataManagerName}");
                
                if (!string.IsNullOrEmpty(dataManagerName) && dataManagerName != buildingID)
                {
                    //Debug.Log($"[BuildingLocalizationHelper] DataManager 성공: {dataManagerName}");
                    return dataManagerName;
                }
            }
            else
            {
                Debug.LogWarning("[BuildingLocalizationHelper] Manager.data가 null입니다.");
            }

            // 3. 폴백: 건물 ID 반환
            Debug.LogWarning($"[BuildingLocalizationHelper] 건물 '{buildingID}'의 번역을 찾을 수 없습니다.");
            return buildingID;
        }

        /// <summary>
        /// 건물 ID로 번역된 설명을 가져옵니다
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <returns>현재 언어에 맞는 건물 설명</returns>
        public static string GetBuildingDescription(string buildingID)
        {
            // 1. LocalizationManager를 통한 번역 시도
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                string localizationKey = $"building_{buildingID}_description";
                string localizedDescription = LocalizationManager.Instance.GetText(localizationKey);
                
                // 번역이 성공했고 키와 다른 경우 (실제 번역된 텍스트)
                if (!string.IsNullOrEmpty(localizedDescription) && localizedDescription != localizationKey)
                {
                    return localizedDescription;
                }
            }

            // 2. DataManager를 통한 번역 시도
            if (Manager.data != null)
            {
                SystemLanguage currentLanguage = LocalizationManager.Instance?.CurrentLanguage ?? SystemLanguage.Korean;
                string dataManagerDescription = Manager.data.GetBuildingLocalizedDescription(buildingID, currentLanguage);
                
                if (!string.IsNullOrEmpty(dataManagerDescription))
                {
                    return dataManagerDescription;
                }
            }

            // 3. 폴백: 빈 문자열 반환
            Debug.LogWarning($"[BuildingLocalizationHelper] 건물 '{buildingID}'의 설명 번역을 찾을 수 없습니다.");
            return "";
        }

        /// <summary>
        /// 현재 언어를 가져옵니다
        /// </summary>
        /// <returns>현재 설정된 언어</returns>
        public static SystemLanguage GetCurrentLanguage()
        {
            if (LocalizationManager.Instance != null)
            {
                return LocalizationManager.Instance.CurrentLanguage;
            }
            return SystemLanguage.Korean; // 기본값
        }

        /// <summary>
        /// 언어 변경 이벤트에 구독합니다
        /// </summary>
        /// <param name="callback">언어 변경 시 호출될 콜백</param>
        public static void SubscribeToLanguageChanged(System.Action<SystemLanguage> callback)
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += callback;
            }
        }

        /// <summary>
        /// 언어 변경 이벤트 구독을 해제합니다
        /// </summary>
        /// <param name="callback">구독 해제할 콜백</param>
        public static void UnsubscribeFromLanguageChanged(System.Action<SystemLanguage> callback)
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= callback;
            }
        }
    }
}

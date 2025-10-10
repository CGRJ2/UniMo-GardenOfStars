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
        /// 확장 가능한 다국어 지원 방식
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <returns>현재 언어에 맞는 건물 이름</returns>
        public static string GetBuildingName(string buildingID)
        {
            return ExtensibleLocalizationHelper.GetBuildingName(buildingID);
        }

        /// <summary>
        /// 건물 ID로 번역된 설명을 가져옵니다
        /// 확장 가능한 다국어 지원 방식
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <returns>현재 언어에 맞는 건물 설명</returns>
        public static string GetBuildingDescription(string buildingID)
        {
            return ExtensibleLocalizationHelper.GetBuildingDescription(buildingID);
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

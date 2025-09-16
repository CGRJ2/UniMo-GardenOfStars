using KYS;
using UnityEngine;

namespace KYS
{
    /// <summary>
    /// 재료 번역을 위한 헬퍼 클래스
    /// LocalizationManager와 DataManager를 활용하여 재료 번역을 처리합니다
    /// </summary>
    public static class IngrediantLocalizationHelper
    {
        /// <summary>
        /// 재료 ID로 번역된 이름을 가져옵니다
        /// </summary>
        /// <param name="ingrediantID">재료 ID</param>
        /// <returns>현재 언어에 맞는 재료 이름</returns>
        public static string GetIngrediantName(string ingrediantID)
        {
            Debug.Log($"[IngrediantLocalizationHelper] GetIngrediantName 호출: {ingrediantID}");
            
            // 1. DataManager를 통한 번역 시도 (BuildingLocalizationHelper와 동일한 패턴)
            if (Manager.data != null)
            {
                SystemLanguage currentLanguage = LocalizationManager.Instance?.CurrentLanguage ?? SystemLanguage.Korean;
                Debug.Log($"[IngrediantLocalizationHelper] 현재 언어: {currentLanguage}");
                
                string dataManagerName = Manager.data.GetIngrediantLocalizedName(ingrediantID, currentLanguage);
                Debug.Log($"[IngrediantLocalizationHelper] DataManager 결과: {dataManagerName}");
                
                if (!string.IsNullOrEmpty(dataManagerName) && dataManagerName != ingrediantID)
                {
                    Debug.Log($"[IngrediantLocalizationHelper] 번역 성공: {ingrediantID} -> {dataManagerName}");
                    return dataManagerName;
                }
            }
            else
            {
                Debug.LogWarning("[IngrediantLocalizationHelper] Manager.data가 null입니다.");
            }

            // 2. 폴백: 재료 ID 반환
            Debug.LogWarning($"[IngrediantLocalizationHelper] 재료 '{ingrediantID}'의 번역을 찾을 수 없습니다.");
            return ingrediantID;
        }

        /// <summary>
        /// 언어 변경 이벤트 구독
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
        /// 언어 변경 이벤트 구독 해제
        /// </summary>
        /// <param name="callback">구독 해제할 콜백</param>
        public static void UnsubscribeFromLanguageChanged(System.Action<SystemLanguage> callback)
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= callback;
            }
        }

        /// <summary>
        /// 재료 ID로 번역된 이름을 가져옵니다 (BuildingLocalizationHelper와 동일한 패턴)
        /// </summary>
        /// <param name="ingrediantID">재료 ID</param>
        /// <returns>현재 언어에 맞는 재료 이름</returns>
        public static string GetIngrediantText(string ingrediantID)
        {
            return GetIngrediantName(ingrediantID);
        }
    }
}

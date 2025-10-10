using System;
using System.Collections.Generic;
using UnityEngine;

namespace KYS
{
    /// <summary>
    /// 확장 가능한 번역 헬퍼 클래스
    /// 새로운 언어 추가 시 코드 수정 없이 동작
    /// </summary>
    public static class ExtensibleLocalizationHelper
    {
        /// <summary>
        /// 언어 코드 매핑 (CSV 헤더와 일치)
        /// </summary>
        private static readonly Dictionary<SystemLanguage, string> LanguageCodeMap = new()
        {
            { SystemLanguage.Korean, "KR" },
            { SystemLanguage.English, "EN" },
            { SystemLanguage.Japanese, "JP" },
            { SystemLanguage.Chinese, "CN" },
            { SystemLanguage.French, "FR" },
            { SystemLanguage.German, "DE" },
            { SystemLanguage.Spanish, "ES" },
            { SystemLanguage.Italian, "IT" },
            { SystemLanguage.Portuguese, "PT" },
            { SystemLanguage.Russian, "RU" },
            // 새로운 언어 추가 시 여기에만 추가하면 됨
        };

        /// <summary>
        /// 건물 이름을 동적으로 가져오기 (확장 가능)
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <param name="language">언어 (null이면 현재 언어)</param>
        /// <returns>번역된 건물 이름</returns>
        public static string GetBuildingName(string buildingID, SystemLanguage? language = null)
        {
            if (string.IsNullOrEmpty(buildingID))
                return string.Empty;

            var targetLanguage = language ?? (LocalizationManager.Instance?.CurrentLanguage ?? SystemLanguage.Korean);

            // 1. LocalizationManager CSV에서 시도
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                string localizationKey = $"building_{buildingID}_name";
                string result = LocalizationManager.Instance.GetText(localizationKey, targetLanguage);
                
                if (result != localizationKey && !string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            // 2. 건물 데이터에서 동적으로 가져오기
            if (Manager.data?.Building?.Values != null)
            {
                if (Manager.data.Building.Values.TryGetValue(buildingID, out BuildingData buildingData))
                {
                    string result = GetLocalizedTextFromBuildingData(buildingData, targetLanguage, "Name");
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }

            return buildingID;
        }

        /// <summary>
        /// 건물 설명을 동적으로 가져오기 (확장 가능)
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <param name="language">언어 (null이면 현재 언어)</param>
        /// <returns>번역된 건물 설명</returns>
        public static string GetBuildingDescription(string buildingID, SystemLanguage? language = null)
        {
            if (string.IsNullOrEmpty(buildingID))
                return string.Empty;

            var targetLanguage = language ?? (LocalizationManager.Instance?.CurrentLanguage ?? SystemLanguage.Korean);

            // 1. LocalizationManager CSV에서 시도
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                string localizationKey = $"building_{buildingID}_description";
                string result = LocalizationManager.Instance.GetText(localizationKey, targetLanguage);
                
                if (result != localizationKey && !string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            // 2. 건물 데이터에서 동적으로 가져오기
            if (Manager.data?.Building?.Values != null)
            {
                if (Manager.data.Building.Values.TryGetValue(buildingID, out BuildingData buildingData))
                {
                    string result = GetLocalizedTextFromBuildingData(buildingData, targetLanguage, "Description");
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 재료 이름을 동적으로 가져오기 (확장 가능)
        /// </summary>
        /// <param name="ingrediantID">재료 ID</param>
        /// <param name="language">언어 (null이면 현재 언어)</param>
        /// <returns>번역된 재료 이름</returns>
        public static string GetIngrediantName(string ingrediantID, SystemLanguage? language = null)
        {
            if (string.IsNullOrEmpty(ingrediantID))
                return string.Empty;

            var targetLanguage = language ?? (LocalizationManager.Instance?.CurrentLanguage ?? SystemLanguage.Korean);

            // 1. LocalizationManager CSV에서 시도
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                string localizationKey = $"ingrediant_{ingrediantID}_name";
                string result = LocalizationManager.Instance.GetText(localizationKey, targetLanguage);
                
                if (result != localizationKey && !string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            // 2. 재료 데이터에서 동적으로 가져오기
            if (Manager.data?.Ingrediant?.Values != null)
            {
                if (Manager.data.Ingrediant.Values.TryGetValue(ingrediantID, out IngrediantData ingrediantData))
                {
                    string result = GetLocalizedTextFromIngrediantData(ingrediantData, targetLanguage);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }

            return ingrediantID;
        }

        /// <summary>
        /// 건물 데이터에서 동적으로 번역 텍스트 가져오기
        /// </summary>
        private static string GetLocalizedTextFromBuildingData(BuildingData buildingData, SystemLanguage language, string fieldType)
        {
            // 언어 코드 가져오기
            if (!LanguageCodeMap.TryGetValue(language, out string languageCode))
            {
                // 지원하지 않는 언어면 영어로 폴백
                languageCode = "EN";
            }

            // 리플렉션을 사용하여 동적으로 필드 접근
            string fieldName = $"{fieldType}_{languageCode}";
            var field = typeof(BuildingData).GetField(fieldName);
            
            if (field != null)
            {
                string value = field.GetValue(buildingData) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            // 폴백: 영어 시도
            if (languageCode != "EN")
            {
                fieldName = $"{fieldType}_EN";
                field = typeof(BuildingData).GetField(fieldName);
                if (field != null)
                {
                    string value = field.GetValue(buildingData) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            // 최종 폴백: 한국어 시도
            if (languageCode != "KR")
            {
                fieldName = $"{fieldType}_KR";
                field = typeof(BuildingData).GetField(fieldName);
                if (field != null)
                {
                    string value = field.GetValue(buildingData) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 재료 데이터에서 동적으로 번역 텍스트 가져오기
        /// </summary>
        private static string GetLocalizedTextFromIngrediantData(IngrediantData ingrediantData, SystemLanguage language)
        {
            // 언어 코드 가져오기
            if (!LanguageCodeMap.TryGetValue(language, out string languageCode))
            {
                // 지원하지 않는 언어면 영어로 폴백
                languageCode = "EN";
            }

            // 리플렉션을 사용하여 동적으로 필드 접근
            string fieldName = $"Name_{languageCode}";
            var field = typeof(IngrediantData).GetField(fieldName);
            
            if (field != null)
            {
                string value = field.GetValue(ingrediantData) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            // 폴백: 영어 시도
            if (languageCode != "EN")
            {
                fieldName = "Name_EN";
                field = typeof(IngrediantData).GetField(fieldName);
                if (field != null)
                {
                    string value = field.GetValue(ingrediantData) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            // 최종 폴백: 한국어 시도
            if (languageCode != "KR")
            {
                fieldName = "Name_KR";
                field = typeof(IngrediantData).GetField(fieldName);
                if (field != null)
                {
                    string value = field.GetValue(ingrediantData) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 지원하는 모든 언어로 번역 가져오기
        /// </summary>
        /// <param name="buildingID">건물 ID</param>
        /// <returns>모든 언어의 번역</returns>
        public static Dictionary<SystemLanguage, string> GetAllBuildingNames(string buildingID)
        {
            var result = new Dictionary<SystemLanguage, string>();
            
            foreach (var language in LanguageCodeMap.Keys)
            {
                result[language] = GetBuildingName(buildingID, language);
            }
            
            return result;
        }

        /// <summary>
        /// 지원하는 모든 언어로 재료 이름 가져오기
        /// </summary>
        /// <param name="ingrediantID">재료 ID</param>
        /// <returns>모든 언어의 번역</returns>
        public static Dictionary<SystemLanguage, string> GetAllIngrediantNames(string ingrediantID)
        {
            var result = new Dictionary<SystemLanguage, string>();
            
            foreach (var language in LanguageCodeMap.Keys)
            {
                result[language] = GetIngrediantName(ingrediantID, language);
            }
            
            return result;
        }

        /// <summary>
        /// 새로운 언어 지원 추가
        /// </summary>
        /// <param name="language">언어</param>
        /// <param name="code">언어 코드</param>
        public static void AddLanguageSupport(SystemLanguage language, string code)
        {
            if (!LanguageCodeMap.ContainsKey(language))
            {
                LanguageCodeMap[language] = code;
                Debug.Log($"[ExtensibleLocalizationHelper] 새로운 언어 지원 추가: {language} ({code})");
            }
        }

        /// <summary>
        /// 지원하는 언어 목록 가져오기
        /// </summary>
        /// <returns>지원하는 언어 목록</returns>
        public static SystemLanguage[] GetSupportedLanguages()
        {
            var languages = new SystemLanguage[LanguageCodeMap.Count];
            LanguageCodeMap.Keys.CopyTo(languages, 0);
            return languages;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KYS
{
    /// <summary>
    /// Firebase 상속 대화 데이터 클래스
    /// </summary>
    public class DialogueData : FirebaseData
    {
        // CSV에서 가져오는 정적 데이터
        public string NpcId => Manager.data.Dialogue.Values[Id].NpcId;
        public string StageId => Manager.data.Dialogue.Values[Id].StageId;
        public string NodeType => Manager.data.Dialogue.Values[Id].NodeType;
        public string Speaker => Manager.data.Dialogue.Values[Id].Speaker;
        public string DialogueText => Manager.data.Dialogue.Values[Id].DialogueText;
        public string NextNodeId => Manager.data.Dialogue.Values[Id].NextNodeId;
        public string ChoiceText1 => Manager.data.Dialogue.Values[Id].ChoiceText1;
        public string ChoiceNext1 => Manager.data.Dialogue.Values[Id].ChoiceNext1;
        public string ChoiceText2 => Manager.data.Dialogue.Values[Id].ChoiceText2;
        public string ChoiceNext2 => Manager.data.Dialogue.Values[Id].ChoiceNext2;
        public string ChoiceText3 => Manager.data.Dialogue.Values[Id].ChoiceText3;
        public string ChoiceNext3 => Manager.data.Dialogue.Values[Id].ChoiceNext3;
        public string ChoiceText4 => Manager.data.Dialogue.Values[Id].ChoiceText4;
        public string ChoiceNext4 => Manager.data.Dialogue.Values[Id].ChoiceNext4;
        public string CharacterImage => Manager.data.Dialogue.Values[Id].CharacterImage;
        public string CharacterImagePosition => Manager.data.Dialogue.Values[Id].CharacterImagePosition;
        public string UseTypingEffect => Manager.data.Dialogue.Values[Id].UseTypingEffect;
        public string BackgroundImage => Manager.data.Dialogue.Values[Id].BackgroundImage;
        public string ConstellationImage => Manager.data.Dialogue.Values[Id].ConstellationImage;
        public string CenterImage => Manager.data.Dialogue.Values[Id].CenterImage;
        public float CenterImageDuration => float.TryParse(Manager.data.Dialogue.Values[Id].CenterImageDuration, out float duration) ? duration : 3f;
        public bool CenterImageInfinite => string.IsNullOrEmpty(Manager.data.Dialogue.Values[Id].CenterImageDuration) ||
                                          Manager.data.Dialogue.Values[Id].CenterImageDuration.ToLower() == "infinite" ||
                                          Manager.data.Dialogue.Values[Id].CenterImageDuration.ToLower() == "inf";
        public float CenterImageFadeInTime => float.TryParse(Manager.data.Dialogue.Values[Id].CenterImageFadeInTime, out float fadeIn) ? fadeIn : 0.5f;
        public float CenterImageFadeOutTime => float.TryParse(Manager.data.Dialogue.Values[Id].CenterImageFadeOutTime, out float fadeOut) ? fadeOut : 0.5f;
        public bool HideCharacterImages => bool.TryParse(Manager.data.Dialogue.Values[Id].HideCharacterImages, out bool hide) ? hide : false;
        public float AutoAdvanceDelay => Manager.data.Dialogue.Values[Id].AutoAdvanceDelay;
        public string ConditionType => Manager.data.Dialogue.Values[Id].ConditionType;
        public string ConditionValue => Manager.data.Dialogue.Values[Id].ConditionValue;
        public string EffectType => Manager.data.Dialogue.Values[Id].EffectType;
        public string EffectValue => Manager.data.Dialogue.Values[Id].EffectValue;

        // Addressable 이미지 관련 프로퍼티
        public Sprite CharacterSprite => Manager.data.Dialogue.Values[Id].CharacterSprite;
        public bool IsCharacterImageLoaded => Manager.data.Dialogue.Values[Id].IsImageLoaded;
        public bool IsCharacterImageLoading => Manager.data.Dialogue.Values[Id].IsImageLoading;

        // NPC 이미지 관련 프로퍼티 (NPC ID로 직접 접근)
        public Sprite NpcSprite => GetNpcSpriteDirect();

        // Firebase에서 관리하는 동적 데이터
        public FirebaseProperty<bool> IsCompleted; // 대화 완료 여부
        public FirebaseProperty<int> ChoiceSelected; // 선택한 선택지 인덱스
        public FirebaseProperty<long> LastPlayedTime; // 마지막 재생 시간

        public DialogueData(string id, string parentPath = null) : base(id, parentPath)
        {
            IsCompleted = new FirebaseProperty<bool>("IsCompleted", Path);
            ChoiceSelected = new FirebaseProperty<int>("ChoiceSelected", Path);
            LastPlayedTime = new FirebaseProperty<long>("LastPlayedTime", Path);
        }

        /// <summary>
        /// 대화 노드가 완료되었는지 확인
        /// </summary>
        public bool IsDialogueCompleted => IsCompleted.Value;

        /// <summary>
        /// 선택지가 있는 노드인지 확인
        /// </summary>
        public bool HasChoices => !string.IsNullOrEmpty(ChoiceText1) || !string.IsNullOrEmpty(ChoiceText2) ||
                                  !string.IsNullOrEmpty(ChoiceText3) || !string.IsNullOrEmpty(ChoiceText4);

        /// <summary>
        /// 자동 진행 노드인지 확인
        /// </summary>
        public bool IsAutoAdvance => AutoAdvanceDelay > 0;

        /// <summary>
        /// 선택지 개수 반환
        /// </summary>
        public int ChoiceCount
        {
            get
            {
                int count = 0;
                if (!string.IsNullOrEmpty(ChoiceText1)) count++;
                if (!string.IsNullOrEmpty(ChoiceText2)) count++;
                if (!string.IsNullOrEmpty(ChoiceText3)) count++;
                if (!string.IsNullOrEmpty(ChoiceText4)) count++;
                return count;
            }
        }

        /// <summary>
        /// 선택지 텍스트 배열 반환
        /// </summary>
        public string[] GetChoiceTexts()
        {
            var choices = new List<string>();
            if (!string.IsNullOrEmpty(ChoiceText1)) choices.Add(ChoiceText1);
            if (!string.IsNullOrEmpty(ChoiceText2)) choices.Add(ChoiceText2);
            if (!string.IsNullOrEmpty(ChoiceText3)) choices.Add(ChoiceText3);
            if (!string.IsNullOrEmpty(ChoiceText4)) choices.Add(ChoiceText4);
            return choices.ToArray();
        }

        /// <summary>
        /// 선택지 다음 노드 ID 배열 반환
        /// </summary>
        public string[] GetChoiceNextIds()
        {
            var nextIds = new List<string>();
            if (!string.IsNullOrEmpty(ChoiceNext1)) nextIds.Add(ChoiceNext1);
            if (!string.IsNullOrEmpty(ChoiceNext2)) nextIds.Add(ChoiceNext2);
            if (!string.IsNullOrEmpty(ChoiceNext3)) nextIds.Add(ChoiceNext3);
            if (!string.IsNullOrEmpty(ChoiceNext4)) nextIds.Add(ChoiceNext4);
            return nextIds.ToArray();
        }

        /// <summary>
        /// 현재 언어에 맞는 Speaker 이름 반환 (동적 언어 지원)
        /// </summary>
        public string GetLocalizedSpeaker(SystemLanguage language = SystemLanguage.Korean)
        {
            var csvData = Manager.data.Dialogue.Values[Id];

            // 1. 직접 번역이 있으면 사용 (동적 방식)
            string localizedSpeaker = GetLocalizedTextByLanguage(csvData, "Speaker", language);
            Debug.Log($"[DialogueData] GetLocalizedSpeaker - ID: {Id}, 언어: {language}, 로컬라이즈 결과: '{localizedSpeaker}', 기본 Speaker: '{csvData.Speaker}'");

            if (!string.IsNullOrEmpty(localizedSpeaker))
            {
                return localizedSpeaker;
            }

            // 2. 기본 Speaker 반환
            return csvData.Speaker;
        }

        /// <summary>
        /// 현재 언어에 맞는 대화 텍스트 반환 (동적 언어 지원)
        /// </summary>
        public string GetLocalizedDialogueText(SystemLanguage language = SystemLanguage.Korean)
        {
            var csvData = Manager.data.Dialogue.Values[Id];

            // 1. 직접 번역이 있으면 사용 (동적 방식)
            string localizedText = GetLocalizedTextByLanguage(csvData, "DialogueText", language);
            if (!string.IsNullOrEmpty(localizedText))
            {
                return localizedText;
            }

            // 2. 기본 텍스트 반환
            return csvData.DialogueText;
        }

        /// <summary>
        /// 언어별 번역 텍스트 가져오기 (동적 방식)
        /// </summary>
        private string GetLocalizedTextByLanguage(DialogueDataCsv csvData, string baseFieldName, SystemLanguage language)
        {
            string languageSuffix = GetLanguageSuffix(language);
            if (string.IsNullOrEmpty(languageSuffix))
            {
                Debug.Log($"[DialogueData] GetLocalizedTextByLanguage - 언어 접미사 없음: {language}");
                return null;
            }

            // 리플렉션을 사용하여 동적으로 필드 접근
            var fieldName = $"{baseFieldName}_{languageSuffix}";
            var field = csvData.GetType().GetField(fieldName);

            Debug.Log($"[DialogueData] GetLocalizedTextByLanguage - 필드명: '{fieldName}', 필드 존재: {field != null}");

            if (field != null)
            {
                string value = field.GetValue(csvData) as string;
                Debug.Log($"[DialogueData] GetLocalizedTextByLanguage - 필드 값: '{value}'");
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// 언어 코드 접미사 반환
        /// </summary>
        private string GetLanguageSuffix(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Korean: return "Korea";
                case SystemLanguage.English: return "English";
                case SystemLanguage.Chinese: return "Chinese";
                case SystemLanguage.French: return "French";
                case SystemLanguage.German: return "German";
                case SystemLanguage.Spanish: return "Spanish";
                case SystemLanguage.Italian: return "Italian";
                case SystemLanguage.Portuguese: return "Portuguese";
                case SystemLanguage.Russian: return "Russian";
                default: return null;
            }
        }

        /// <summary>
        /// 현재 언어에 맞는 선택지 텍스트 배열 반환 (동적 언어 지원)
        /// </summary>
        public string[] GetLocalizedChoiceTexts(SystemLanguage language = SystemLanguage.Korean)
        {
            var csvData = Manager.data.Dialogue.Values[Id];
            var choices = new List<string>();

            // 4개 선택지 모두 확인 (ChoiceTextX가 비어있어도 ChoiceTextX_Language에서 번역된 텍스트 확인)
            for (int i = 1; i <= 4; i++)
            {
                string localizedText = GetLocalizedChoiceTextDynamic(csvData, i, language);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    choices.Add(localizedText);
                }
            }

            return choices.ToArray();
        }

        /// <summary>
        /// 조건부 선택지 텍스트 배열 반환 (조건에 따라 필터링)
        /// </summary>
        public string[] GetFilteredChoiceTexts(SystemLanguage language = SystemLanguage.Korean)
        {
            var csvData = Manager.data.Dialogue.Values[Id];
            var choices = new List<string>();

            Debug.Log($"[DialogueData] GetFilteredChoiceTexts 시작 - 노드 ID: {Id}");

            // 4개 선택지 모두 확인하고 조건 체크
            for (int i = 1; i <= 4; i++)
            {
                string localizedText = GetLocalizedChoiceTextDynamic(csvData, i, language);
                bool shouldShow = ShouldShowChoice(i);
                string nextId = GetChoiceNextByIndex(i);
                
                Debug.Log($"[DialogueData] Choice{i} - 텍스트: '{localizedText}', 표시여부: {shouldShow}, NextId: '{nextId}'");
                
                if (!string.IsNullOrEmpty(localizedText) && shouldShow)
                {
                    choices.Add(localizedText);
                    Debug.Log($"[DialogueData] Choice{i} 추가됨: '{localizedText}'");
                }
            }

            Debug.Log($"[DialogueData] GetFilteredChoiceTexts 완료 - 총 {choices.Count}개 선택지: [{string.Join(", ", choices)}]");
            return choices.ToArray();
        }

        /// <summary>
        /// 조건부 선택지 다음 노드 ID 배열 반환 (조건에 따라 필터링)
        /// </summary>
        public string[] GetFilteredChoiceNextIds()
        {
            var nextIds = new List<string>();
            
            // GetFilteredChoiceTexts와 동일한 로직으로 필터링
            var csvData = Manager.data.Dialogue.Values[Id];
            
            Debug.Log($"[DialogueData] GetFilteredChoiceNextIds 시작 - 노드 ID: {Id}");
            
            for (int i = 1; i <= 4; i++)
            {
                // GetFilteredChoiceTexts와 동일하게 로컬라이즈된 텍스트 확인
                string localizedText = GetLocalizedChoiceTextDynamic(csvData, i, SystemLanguage.Korean);
                bool shouldShow = ShouldShowChoice(i);
                string nextId = GetChoiceNextByIndex(i);
                
                Debug.Log($"[DialogueData] Choice{i} NextId - 텍스트: '{localizedText}', 표시여부: {shouldShow}, NextId: '{nextId}'");
                
                if (!string.IsNullOrEmpty(localizedText) && shouldShow)
                {
                    // NextId가 비어있으면 "end"로 대화 종료 처리
                    string finalNextId = string.IsNullOrEmpty(nextId) ? "end" : nextId;
                    nextIds.Add(finalNextId);
                    
                    if (!string.IsNullOrEmpty(nextId))
                    {
                        Debug.Log($"[DialogueData] Choice{i} NextId 추가됨: '{nextId}'");
                    }
                    else
                    {
                        Debug.Log($"[DialogueData] Choice{i} NextId 추가됨: 'end' (대화 종료용)");
                    }
                }
            }

            Debug.Log($"[DialogueData] GetFilteredChoiceNextIds 완료 - 총 {nextIds.Count}개 NextId: [{string.Join(", ", nextIds)}]");
            return nextIds.ToArray();
        }

        /// <summary>
        /// 선택지 텍스트 가져오기
        /// </summary>
        private string GetChoiceTextByIndex(DialogueDataCsv csvData, int index)
        {
            string fieldName = $"ChoiceText{index}";
            var field = csvData.GetType().GetField(fieldName);
            return field?.GetValue(csvData)?.ToString() ?? "";
        }

        /// <summary>
        /// 선택지 다음 노드 ID 가져오기
        /// </summary>
        private string GetChoiceNextByIndex(int index)
        {
            switch (index)
            {
                case 1: return ChoiceNext1;
                case 2: return ChoiceNext2;
                case 3: return ChoiceNext3;
                case 4: return ChoiceNext4;
                default: return "";
            }
        }

        /// <summary>
        /// 선택지 표시 조건 체크
        /// </summary>
        private bool ShouldShowChoice(int choiceIndex)
        {
            // Choice1, Choice4는 항상 표시 (기본 대화, 대화 종료)
            if (choiceIndex == 1 || choiceIndex == 4)
            {
                Debug.Log($"[DialogueData] Choice{choiceIndex} 표시 - 항상 표시 (기본/종료)");
                return true;
            }

            // Choice2, Choice3는 조건 체크
            string condition = GetChoiceCondition(choiceIndex);
            bool shouldShow = CheckChoiceCondition(condition);
            
            Debug.Log($"[DialogueData] Choice{choiceIndex} 표시 조건 체크 - 조건: '{condition}', 결과: {shouldShow}");
            
            return shouldShow;
        }

        /// <summary>
        /// 선택지 조건 가져오기
        /// </summary>
        private string GetChoiceCondition(int choiceIndex)
        {
            var csvData = Manager.data.Dialogue.Values[Id];
            string conditionFieldName = $"Choice{choiceIndex}Condition";
            var conditionField = csvData.GetType().GetField(conditionFieldName);
            
            Debug.Log($"[DialogueData] GetChoiceCondition - choiceIndex: {choiceIndex}, fieldName: '{conditionFieldName}', fieldFound: {conditionField != null}");
            
            if (conditionField != null)
            {
                string conditionValue = conditionField.GetValue(csvData)?.ToString() ?? "";
                Debug.Log($"[DialogueData] GetChoiceCondition - choiceIndex: {choiceIndex}, conditionValue: '{conditionValue}'");
                return conditionValue;
            }
            
            Debug.Log($"[DialogueData] GetChoiceCondition - choiceIndex: {choiceIndex}, field not found, returning empty string");
            return "";
        }

        /// <summary>
        /// 선택지 조건 체크
        /// </summary>
        private bool CheckChoiceCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                Debug.Log($"[DialogueData] 조건이 비어있음 - true 반환");
                return true;
            }

            // "quest_cleared:quest0001,quest0005" 형식 파싱
            if (condition.StartsWith("quest_cleared:"))
            {
                string questIds = condition.Substring("quest_cleared:".Length);
                string[] requiredQuests = questIds.Split(',');
                
                string currentQuest = GetCurrentQuestId();
                
                Debug.Log($"[DialogueData] quest_cleared 조건 체크 - 필수 퀘스트: [{string.Join(", ", requiredQuests)}], 현재 퀘스트: '{currentQuest}'");
                
                // 모든 필수 퀘스트가 클리어되었는지 체크
                foreach (string requiredQuest in requiredQuests)
                {
                    if (!IsQuestCleared(requiredQuest.Trim(), currentQuest))
                    {
                        Debug.Log($"[DialogueData] 퀘스트 클리어 조건 실패 - requiredQuest: '{requiredQuest.Trim()}'");
                        return false;
                    }
                }
                
                Debug.Log($"[DialogueData] 모든 퀘스트 클리어 조건 만족");
                return true;
            }

            Debug.Log($"[DialogueData] 알 수 없는 조건 형식: '{condition}' - true 반환");
            return true;
        }

        /// <summary>
        /// 현재 퀘스트 ID 가져오기
        /// </summary>
        private string GetCurrentQuestId()
        {
            // 현재 퀘스트 ID 가져오기 (실제 구현에 맞게 수정 필요)
            if (Manager.firebase?.UserData?.CurStageData?.Npc?.CurQuestData != null)
            {
                return Manager.firebase.UserData.CurStageData.Npc.CurQuestData.Id;
            }
            
            return "";
        }

        /// <summary>
        /// 퀘스트 클리어 여부 체크
        /// </summary>
        private bool IsQuestCleared(string requiredQuest, string currentQuest)
        {
            if (string.IsNullOrEmpty(requiredQuest) || string.IsNullOrEmpty(currentQuest))
            {
                Debug.Log($"[DialogueData] 퀘스트 클리어 체크 실패 - requiredQuest: '{requiredQuest}', currentQuest: '{currentQuest}'");
                return false;
            }

            // 퀘스트 ID에서 숫자 추출
            int requiredNum = ExtractQuestNumber(requiredQuest);
            int currentNum = ExtractQuestNumber(currentQuest);
            
            bool isCleared = currentNum > requiredNum;
            
            Debug.Log($"[DialogueData] 퀘스트 클리어 체크 - requiredQuest: '{requiredQuest}' ({requiredNum}), currentQuest: '{currentQuest}' ({currentNum}), 결과: {isCleared}");
            
            // 현재 퀘스트가 더 높으면 이전 퀘스트들은 클리어된 것으로 간주
            // 예: 현재 퀘스트가 2면, 퀘스트 1은 클리어된 상태
            return isCleared;
        }

        /// <summary>
        /// 퀘스트 ID에서 숫자 추출
        /// </summary>
        private int ExtractQuestNumber(string questId)
        {
            // "quest0001" -> 1
            string numberPart = questId.Replace("quest", "").TrimStart('0');
            return int.TryParse(numberPart, out int number) ? number : 0;
        }

        /// <summary>
        /// 동적 방식으로 선택지 번역 텍스트 가져오기
        /// </summary>
        private string GetLocalizedChoiceTextDynamic(DialogueDataCsv csvData, int choiceIndex, SystemLanguage language)
        {
            // 1. 직접 번역이 있으면 사용 (동적 방식)
            string localizedText = GetLocalizedTextByLanguage(csvData, $"ChoiceText{choiceIndex}", language);
            if (!string.IsNullOrEmpty(localizedText))
            {
                return localizedText;
            }

            // 2. 기본 텍스트 가져오기 (ChoiceTextX 필드)
            string baseFieldName = $"ChoiceText{choiceIndex}";
            var baseField = csvData.GetType().GetField(baseFieldName);
            if (baseField != null)
            {
                string baseText = baseField.GetValue(csvData) as string;

                // 3. 기본 텍스트가 있으면 LocalizationManager로 번역 시도
                if (!string.IsNullOrEmpty(baseText) && LocalizationManager.Instance != null)
                {
                    string translatedText = LocalizationManager.Instance.GetText(baseText, language);
                    // 번역이 성공했으면 번역된 텍스트 반환, 실패했으면 기본 텍스트 반환
                    return (translatedText != baseText) ? translatedText : baseText;
                }

                // 4. 기본 텍스트가 있으면 반환
                if (!string.IsNullOrEmpty(baseText))
                {
                    return baseText;
                }
            }

            // 5. 모든 방법이 실패하면 null 반환
            return null;
        }

        /// <summary>
        /// NPC 스프라이트 가져오기 (간단한 직접 접근)
        /// 1. CharacterImage 키로 Dialogue 캐시에서 시도
        /// 2. NpcId로 NPC 데이터에서 직접 가져오기
        /// </summary>
        private Sprite GetNpcSpriteDirect()
        {
            // 1. CharacterImage 키로 Dialogue 캐시에서 시도
            if (!string.IsNullOrEmpty(CharacterImage))
            {
                Sprite dialogueSprite = Manager.data.GetCachedCharacterImage(CharacterImage);
                if (dialogueSprite != null)
                {
                    return dialogueSprite;
                }
            }

            // 2. NpcId로 NPC 데이터에서 직접 가져오기
            if (!string.IsNullOrEmpty(NpcId))
            {
                var npcData = Manager.data.Npc?.Values?.Values?.FirstOrDefault(n => n.NpcID == NpcId);
                return npcData?.Sprite_Default; // 직접 접근
            }

            return null;
        }

    }

    /// <summary>
    /// JSON 저장용 대화 데이터
    /// </summary>
    public class DialogueDataJson : IUsableId
    {
        public string Id;
        public bool IsCompleted;
        public int ChoiceSelected;
        public long LastPlayedTime;

        public string GetId()
        {
            return Id;
        }
    }

    /// <summary>
    /// CSV 파싱용 대화 데이터
    /// </summary>
    public class DialogueDataCsv : IUsableId
    {
        public string Id;
        public string NpcId;
        public string StageId;
        public string NodeType;
        public string Speaker;
        public string Speaker_Korea;
        public string Speaker_English;
        public string DialogueText;
        public string DialogueText_Korea;
        public string DialogueText_English;
        public string NextNodeId;
        public string ChoiceText1;
        public string ChoiceText1_Korea;
        public string ChoiceText1_English;
        public string ChoiceNext1;
        public string ChoiceText2;
        public string ChoiceText2_Korea;
        public string ChoiceText2_English;
        public string ChoiceNext2;
        public string ChoiceText3;
        public string ChoiceText3_Korea;
        public string ChoiceText3_English;
        public string ChoiceNext3;
        public string ChoiceText4;
        public string ChoiceText4_Korea;
        public string ChoiceText4_English;
        public string ChoiceNext4;
        public string Choice2Condition; // Choice2 표시 조건
        public string Choice3Condition; // Choice3 표시 조건
        public string Choice4Condition; // Choice4 표시 조건
        public string CharacterImage;
        public string CharacterImagePosition; // left, right, center
        public string UseTypingEffect; // true, false
        public string BackgroundImage; // 배경 이미지 Addressable 키
        public string ConstellationImage; // 별자리 이미지 Addressable 키
        public string CenterImage; // 가운데 이미지 Addressable 키
        public string CenterImageDuration; // 가운데 이미지 표시 시간 (초)
        public string CenterImageFadeInTime; // 가운데 이미지 페이드인 시간
        public string CenterImageFadeOutTime; // 가운데 이미지 페이드아웃 시간
        public string HideCharacterImages; // 캐릭터 이미지 숨김 여부 (true/false)
        public float AutoAdvanceDelay;
        public string ConditionType;
        public string ConditionValue;
        public string EffectType;
        public string EffectValue;

        // Addressable 이미지 관련
        [System.NonSerialized] public Sprite CharacterSprite; // 로드된 스프라이트
        [System.NonSerialized] public bool IsImageLoaded = false; // 이미지 로드 상태
        [System.NonSerialized] public bool IsImageLoading = false; // 이미지 로딩 중 상태

        public string GetId()
        {
            return Id;
        }
    }
}

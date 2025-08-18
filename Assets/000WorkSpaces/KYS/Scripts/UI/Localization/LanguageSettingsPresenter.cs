using UnityEngine;
using KYS.UI.MVP;

namespace KYS.UI.Localization
{
    /// <summary>
    /// 다국어 설정 MVP Presenter
    /// </summary>
    public class LanguageSettingsPresenter : BaseUIPresenter
    {
        private new LanguageSettingsPanel view;
        private new LanguageModel model;
        
        private SystemLanguage selectedLanguage;
        
        protected override void OnInitialize()
        {
            view = GetView<LanguageSettingsPanel>();
            model = GetModel<LanguageModel>();
            
            if (model != null)
            {
                // Model 이벤트 구독
                model.OnLanguageChanged += OnLanguageChanged;
                model.OnActiveLanguagesLoaded += OnActiveLanguagesLoaded;
                
                // 초기 언어 설정
                selectedLanguage = model.CurrentLanguage;
            }
        }
        
        protected override void OnCleanup()
        {
            if (model != null)
            {
                model.OnLanguageChanged -= OnLanguageChanged;
                model.OnActiveLanguagesLoaded -= OnActiveLanguagesLoaded;
            }
        }
        
        /// <summary>
        /// 언어 드롭다운 설정
        /// </summary>
        public void SetupLanguageDropdown()
        {
            if (view == null || model == null) return;
            
            // 활성 언어만 표시
            view.SetupLanguageDropdown(model.ActiveLanguages, model.GetCurrentLanguageIndex());
        }
        
        /// <summary>
        /// 언어 드롭다운 변경 처리
        /// </summary>
        public void OnLanguageDropdownChanged(int index)
        {
            if (model == null) return;
            
            selectedLanguage = model.GetLanguageByIndex(index);
            Debug.Log($"[LanguageSettingsPresenter] 언어 선택: {model.GetLanguageName(selectedLanguage)}");
        }
        
        /// <summary>
        /// 언어 적용 버튼 클릭 처리
        /// </summary>
        public void OnApplyLanguage()
        {
            if (model == null) return;
            
            // Model을 통해 언어 변경
            model.SetLanguage(selectedLanguage);
            
            Debug.Log($"[LanguageSettingsPresenter] 언어가 적용되었습니다: {model.GetLanguageName(selectedLanguage)}");
            
            // View 닫기
            view?.Hide();
        }
        
        /// <summary>
        /// 닫기 버튼 클릭 처리
        /// </summary>
        public void OnClose()
        {
            Debug.Log("[LanguageSettingsPresenter] 언어 설정 닫기");
            view?.Hide();
        }
        
        #region Event Handlers
        
        private void OnLanguageChanged(SystemLanguage language)
        {
            Debug.Log($"[LanguageSettingsPresenter] 언어 변경 이벤트 수신: {model?.GetLanguageName(language)}");
            // 필요한 경우 View 업데이트
        }
        
        private void OnActiveLanguagesLoaded(SystemLanguage[] languages)
        {
            Debug.Log($"[LanguageSettingsPresenter] 활성 언어 로드 완료: {languages.Length}개 언어");
            // 드롭다운 다시 설정
            SetupLanguageDropdown();
        }
        
        #endregion
    }
}

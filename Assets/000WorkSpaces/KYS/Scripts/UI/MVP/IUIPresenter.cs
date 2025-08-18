namespace KYS.UI.MVP
{
    /// <summary>
    /// MVP 패턴의 Presenter 인터페이스
    /// </summary>
    public interface IUIPresenter
    {
        /// <summary>
        /// Presenter 초기화
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Presenter 정리
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// UI 표시
        /// </summary>
        void Show();
        
        /// <summary>
        /// UI 숨기기
        /// </summary>
        void Hide();
        
        /// <summary>
        /// UI가 활성화되어 있는지 확인
        /// </summary>
        bool IsActive { get; }
    }
}

namespace KYS.UI.MVP
{
    /// <summary>
    /// MVP 패턴의 Model 인터페이스
    /// </summary>
    public interface IUIModel
    {
        /// <summary>
        /// Model 초기화
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Model 정리
        /// </summary>
        void Cleanup();
    }
}

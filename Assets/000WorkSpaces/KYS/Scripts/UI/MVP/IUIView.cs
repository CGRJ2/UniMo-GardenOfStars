using UnityEngine;

namespace KYS
{
    /// <summary>
    /// MVP 패턴의 View 인터페이스
    /// </summary>
    public interface IUIView
    {
        /// <summary>
        /// UI 활성화
        /// </summary>
        void Show();
        
        /// <summary>
        /// UI 비활성화
        /// </summary>
        void Hide();
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// UI 정리
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// UI가 활성화되어 있는지 확인
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// UI GameObject
        /// </summary>
        GameObject GameObject { get; }
    }
}

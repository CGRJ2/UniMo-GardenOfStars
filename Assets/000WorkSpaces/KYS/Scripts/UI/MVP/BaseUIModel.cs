using UnityEngine;


namespace KYS
{
    /// <summary>
    /// MVP 패턴의 기본 Model 클래스
    /// </summary>
    public abstract class BaseUIModel : MonoBehaviour, IUIModel
    {
        protected bool isInitialized = false;
        
        #region IUIModel Implementation
        
        public virtual void Initialize()
        {
            if (isInitialized) return;
            
            OnInitialize();
            isInitialized = true;
        }
        
        public virtual void Cleanup()
        {
            if (!isInitialized) return;
            
            OnCleanup();
            isInitialized = false;
        }
        
        #endregion
        
        #region Lifecycle Methods (Override in derived classes)
        
        protected virtual void OnInitialize()
        {
            // Override in derived classes
        }
        
        protected virtual void OnCleanup()
        {
            // Override in derived classes
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            // 자동 초기화 방지 - 명시적으로 Initialize() 호출 필요
        }
        
        protected virtual void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
    }
}

using UnityEngine;


namespace KYS
{
    /// <summary>
    /// MVP 패턴의 기본 Presenter 클래스
    /// </summary>
    public abstract class BaseUIPresenter : MonoBehaviour, IUIPresenter
    {
        protected IUIView view;
        protected IUIModel model;
        
        protected bool isInitialized = false;
        protected bool isActive = false;
        
        public bool IsActive => isActive;
        
        #region IUIPresenter Implementation
        
        public virtual void Initialize()
        {
            if (isInitialized) return;
            
            SetupMVP();
            OnInitialize();
            isInitialized = true;
        }
        
        public virtual void Cleanup()
        {
            if (!isInitialized) return;
            
            OnCleanup();
            isInitialized = false;
        }
        
        public virtual void Show()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            
            if (isActive) return;
            
            OnShow();
            isActive = true;
        }
        
        public virtual void Hide()
        {
            if (!isActive) return;
            
            OnHide();
            isActive = false;
        }
        
        #endregion
        
        #region MVP Setup
        
        protected virtual void SetupMVP()
        {
            // View와 Model 설정
            view = GetComponent<IUIView>();
            if (view == null)
            {
                Debug.LogError($"[{GetType().Name}] IUIView 컴포넌트를 찾을 수 없습니다.");
            }
            
            // Model은 자식 오브젝트에서 찾거나 생성
            model = GetComponentInChildren<IUIModel>();
            if (model == null)
            {
                Debug.LogWarning($"[{GetType().Name}] IUIModel 컴포넌트를 찾을 수 없습니다.");
            }
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
        
        protected virtual void OnShow()
        {
            view?.Show();
        }
        
        protected virtual void OnHide()
        {
            view?.Hide();
        }
        
        #endregion
        
        #region Utility Methods
        
        protected T GetView<T>() where T : class, IUIView
        {
            return view as T;
        }
        
        protected T GetModel<T>() where T : class, IUIModel
        {
            return model as T;
        }
        
        #endregion
    }
}

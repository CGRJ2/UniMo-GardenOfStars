using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KYS.UI;
using KYS.UI.MVP;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace KYS.UI.Examples
{
    /// <summary>
    /// Addressable 기반 UI 관리 시스템 사용 예시
    /// </summary>
    public class AddressableUIExamples : MonoBehaviour
    {
        [Header("Addressable Keys (문자열)")]
        [SerializeField] private string hudPanelKey = "UI/HUD/StatusPanel";
        [SerializeField] private string mainMenuKey = "UI/Panel/MainMenu";
        [SerializeField] private string settingsKey = "UI/Panel/Settings";
        [SerializeField] private string confirmPopupKey = "UI/Popup/ConfirmDialog";
        [SerializeField] private string loadingKey = "UI/Loading/LoadingScreen";
        
        [Header("AssetReference (타입 안전)")]
        [SerializeField] private AssetReferenceGameObject hudPanelReference;
        [SerializeField] private AssetReferenceGameObject mainMenuReference;
        [SerializeField] private AssetReferenceGameObject settingsReference;
        [SerializeField] private AssetReferenceGameObject confirmPopupReference;
        [SerializeField] private AssetReferenceGameObject loadingReference;
        
        [Header("Labels")]
        [SerializeField] private string hudLabel = "UI_HUD";
        [SerializeField] private string panelLabel = "UI_Panel";
        [SerializeField] private string popupLabel = "UI_Popup";
        
        // 로드된 UI 인스턴스들
        private Dictionary<string, BaseUI> loadedUIs = new Dictionary<string, BaseUI>();
        
        private void Start()
        {
            // UI 미리 로드
            PreloadUIs();
        }
        
        /// <summary>
        /// UI 프리팹들 미리 로드
        /// </summary>
        private async void PreloadUIs()
        {
            Debug.Log("[AddressableUIExamples] UI 미리 로드 시작");
            
            // HUD UI 미리 로드
            await UIManager.Instance.PreloadUIAsync<BaseUI>(hudPanelKey);
            
            // 패널 UI들 미리 로드
            await UIManager.Instance.PreloadUIAsync<BaseUI>(mainMenuKey);
            await UIManager.Instance.PreloadUIAsync<BaseUI>(settingsKey);
            
            // 팝업 UI들 미리 로드
            await UIManager.Instance.PreloadUIAsync<BaseUI>(confirmPopupKey);
            
            Debug.Log("[AddressableUIExamples] UI 미리 로드 완료");
        }
        
        #region HUD UI Examples
        
        /// <summary>
        /// HUD UI 로드 및 표시 (문자열 키 사용)
        /// </summary>
        public async void LoadAndShowHUD()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] HUD UI 로드 시작 (문자열 키)");
                
                // HUD UI 로드
                BaseUI hudUI = await UIManager.Instance.LoadUIAsync<BaseUI>(hudPanelKey);
                
                if (hudUI != null)
                {
                    loadedUIs[hudPanelKey] = hudUI;
                    hudUI.Show();
                    Debug.Log("[AddressableUIExamples] HUD UI 표시 완료");
                }
                else
                {
                    Debug.LogError("[AddressableUIExamples] HUD UI 로드 실패");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] HUD UI 로드 중 오류: {e.Message}");
            }
        }
        
        /// <summary>
        /// HUD UI 로드 및 표시 (AssetReference 사용)
        /// </summary>
        public async void LoadAndShowHUDAssetReference()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] HUD UI 로드 시작 (AssetReference)");
                
                if (hudPanelReference == null || !hudPanelReference.RuntimeKeyIsValid())
                {
                    Debug.LogError("[AddressableUIExamples] 유효하지 않은 HUD Panel AssetReference입니다.");
                    return;
                }
                
                // HUD UI 로드 (AssetReference 사용)
                BaseUI hudUI = await UIManager.Instance.LoadUIAsync<BaseUI>(hudPanelReference);
                
                if (hudUI != null)
                {
                    string key = hudPanelReference.RuntimeKey.ToString();
                    loadedUIs[key] = hudUI;
                    hudUI.Show();
                    Debug.Log("[AddressableUIExamples] HUD UI 표시 완료 (AssetReference)");
                }
                else
                {
                    Debug.LogError("[AddressableUIExamples] HUD UI 로드 실패 (AssetReference)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] HUD UI 로드 중 오류: {e.Message}");
            }
        }
        
        /// <summary>
        /// 라벨로 HUD UI들 일괄 로드
        /// </summary>
        public async void LoadHUDsByLabel()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] HUD UI 라벨 로드 시작");
                
                List<BaseUI> hudUIs = await UIManager.Instance.LoadUIsByLabelAsync<BaseUI>(hudLabel);
                
                foreach (BaseUI hudUI in hudUIs)
                {
                    loadedUIs[hudUI.name] = hudUI;
                    hudUI.Show();
                }
                
                Debug.Log($"[AddressableUIExamples] {hudUIs.Count}개의 HUD UI 로드 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] HUD UI 라벨 로드 중 오류: {e.Message}");
            }
        }
        
        #endregion
        
        #region Panel UI Examples
        
        /// <summary>
        /// 메인 메뉴 패널 로드 및 열기
        /// </summary>
        public async void LoadAndOpenMainMenu()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] 메인 메뉴 로드 시작");
                
                BaseUI mainMenu = await UIManager.Instance.LoadUIAsync<BaseUI>(mainMenuKey);
                
                if (mainMenu != null)
                {
                    loadedUIs[mainMenuKey] = mainMenu;
                    UIManager.Instance.OpenPanel(mainMenu);
                    Debug.Log("[AddressableUIExamples] 메인 메뉴 열기 완료");
                }
                else
                {
                    Debug.LogError("[AddressableUIExamples] 메인 메뉴 로드 실패");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] 메인 메뉴 로드 중 오류: {e.Message}");
            }
        }
        
        /// <summary>
        /// 설정 패널 로드 및 열기
        /// </summary>
        public async void LoadAndOpenSettings()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] 설정 패널 로드 시작");
                
                BaseUI settings = await UIManager.Instance.LoadUIAsync<BaseUI>(settingsKey);
                
                if (settings != null)
                {
                    loadedUIs[settingsKey] = settings;
                    UIManager.Instance.OpenPanel(settings);
                    Debug.Log("[AddressableUIExamples] 설정 패널 열기 완료");
                }
                else
                {
                    Debug.LogError("[AddressableUIExamples] 설정 패널 로드 실패");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] 설정 패널 로드 중 오류: {e.Message}");
            }
        }
        
        /// <summary>
        /// 라벨로 패널 UI들 일괄 로드
        /// </summary>
        public async void LoadPanelsByLabel()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] 패널 UI 라벨 로드 시작");
                
                List<BaseUI> panelUIs = await UIManager.Instance.LoadUIsByLabelAsync<BaseUI>(panelLabel);
                
                foreach (BaseUI panelUI in panelUIs)
                {
                    loadedUIs[panelUI.name] = panelUI;
                    // 패널은 기본적으로 숨겨진 상태로 유지
                    panelUI.Hide();
                }
                
                Debug.Log($"[AddressableUIExamples] {panelUIs.Count}개의 패널 UI 로드 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] 패널 UI 라벨 로드 중 오류: {e.Message}");
            }
        }
        
        #endregion
        
        #region Popup UI Examples
        
        /// <summary>
        /// 확인 팝업 로드 및 표시
        /// </summary>
        public async void LoadAndShowConfirmPopup()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] 확인 팝업 로드 시작");
                
                BaseUI confirmPopup = await UIManager.Instance.LoadUIAsync<BaseUI>(confirmPopupKey);
                
                if (confirmPopup != null)
                {
                    loadedUIs[confirmPopupKey] = confirmPopup;
                    UIManager.Instance.OpenPopup(confirmPopup);
                    Debug.Log("[AddressableUIExamples] 확인 팝업 표시 완료");
                }
                else
                {
                    Debug.LogError("[AddressableUIExamples] 확인 팝업 로드 실패");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] 확인 팝업 로드 중 오류: {e.Message}");
            }
        }
        
        /// <summary>
        /// 라벨로 팝업 UI들 일괄 로드
        /// </summary>
        public async void LoadPopupsByLabel()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] 팝업 UI 라벨 로드 시작");
                
                List<BaseUI> popupUIs = await UIManager.Instance.LoadUIsByLabelAsync<BaseUI>(popupLabel);
                
                foreach (BaseUI popupUI in popupUIs)
                {
                    loadedUIs[popupUI.name] = popupUI;
                    // 팝업은 기본적으로 숨겨진 상태로 유지
                    popupUI.Hide();
                }
                
                Debug.Log($"[AddressableUIExamples] {popupUIs.Count}개의 팝업 UI 로드 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] 팝업 UI 라벨 로드 중 오류: {e.Message}");
            }
        }
        
        #endregion
        
        #region Loading UI Examples
        
        /// <summary>
        /// 로딩 화면 로드 및 표시
        /// </summary>
        public async void LoadAndShowLoading()
        {
            try
            {
                Debug.Log("[AddressableUIExamples] 로딩 화면 로드 시작");
                
                BaseUI loadingUI = await UIManager.Instance.LoadUIAsync<BaseUI>(loadingKey);
                
                if (loadingUI != null)
                {
                    loadedUIs[loadingKey] = loadingUI;
                    loadingUI.Show();
                    Debug.Log("[AddressableUIExamples] 로딩 화면 표시 완료");
                }
                else
                {
                    Debug.LogError("[AddressableUIExamples] 로딩 화면 로드 실패");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableUIExamples] 로딩 화면 로드 중 오류: {e.Message}");
            }
        }
        
        #endregion
        
        #region UI Management Examples
        
        /// <summary>
        /// 특정 UI 해제
        /// </summary>
        public void ReleaseUI(string key)
        {
            if (loadedUIs.ContainsKey(key))
            {
                UIManager.Instance.ReleaseUI(key);
                loadedUIs.Remove(key);
                Debug.Log($"[AddressableUIExamples] UI 해제 완료: {key}");
            }
            else
            {
                Debug.LogWarning($"[AddressableUIExamples] 해제할 UI를 찾을 수 없음: {key}");
            }
        }
        
        /// <summary>
        /// 모든 UI 해제
        /// </summary>
        public void ReleaseAllUIs()
        {
            UIManager.Instance.ReleaseAllAddressables();
            loadedUIs.Clear();
            Debug.Log("[AddressableUIExamples] 모든 UI 해제 완료");
        }
        
        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void ClosePanel()
        {
            UIManager.Instance.ClosePanel();
        }
        
        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public void ClosePopup()
        {
            UIManager.Instance.ClosePopup();
        }
        
        /// <summary>
        /// 모든 패널 닫기
        /// </summary>
        public void CloseAllPanels()
        {
            UIManager.Instance.CloseAllPanels();
        }
        
        /// <summary>
        /// 모든 팝업 닫기
        /// </summary>
        public void CloseAllPopups()
        {
            UIManager.Instance.CloseAllPopups();
        }
        
        #endregion
        
        #region Canvas Management Examples
        
        /// <summary>
        /// 특정 레이어의 Canvas 정보 출력
        /// </summary>
        public void PrintCanvasInfo(UILayerType layerType)
        {
            Canvas canvas = UIManager.Instance.GetCanvasByLayer(layerType);
            
            if (canvas != null)
            {
                Debug.Log($"[AddressableUIExamples] {layerType} Canvas: {canvas.name}");
                Debug.Log($"[AddressableUIExamples] Canvas Sort Order: {canvas.sortingOrder}");
                Debug.Log($"[AddressableUIExamples] Canvas Active: {canvas.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning($"[AddressableUIExamples] {layerType} Canvas를 찾을 수 없음");
            }
        }
        
        /// <summary>
        /// 모든 Canvas 정보 출력
        /// </summary>
        public void PrintAllCanvasInfo()
        {
            Debug.Log("[AddressableUIExamples] 모든 Canvas 정보:");
            
            PrintCanvasInfo(UILayerType.HUD);
            PrintCanvasInfo(UILayerType.Panel);
            PrintCanvasInfo(UILayerType.Popup);
            PrintCanvasInfo(UILayerType.Loading);
        }
        
        #endregion
        
        #region Layer Management Examples
        
        /// <summary>
        /// 특정 레이어의 모든 UI 출력
        /// </summary>
        public void PrintUIsByLayer(UILayerType layerType)
        {
            List<BaseUI> uis = UIManager.Instance.GetUIsByLayer(layerType);
            
            Debug.Log($"[AddressableUIExamples] {layerType} 레이어의 UI들 ({uis.Count}개):");
            
            foreach (BaseUI ui in uis)
            {
                Debug.Log($"  - {ui.name} (Active: {ui.IsActive})");
            }
        }
        
        /// <summary>
        /// 모든 레이어의 UI 정보 출력
        /// </summary>
        public void PrintAllLayerUIs()
        {
            Debug.Log("[AddressableUIExamples] 모든 레이어의 UI 정보:");
            
            PrintUIsByLayer(UILayerType.HUD);
            PrintUIsByLayer(UILayerType.Panel);
            PrintUIsByLayer(UILayerType.Popup);
            PrintUIsByLayer(UILayerType.Loading);
        }
        
        #endregion
    }
}

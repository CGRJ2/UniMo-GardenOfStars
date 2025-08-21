namespace KYS
{
    /// <summary>
    /// UI 레이어 타입 정의
    /// </summary>
    public enum UILayerType
    {
        /// <summary>
        /// HUD 그룹 (재화 표시, 기본 UI 패널 및 버튼들, 가장 뒤에 위치)
        /// </summary>
        HUD = 0,
        
        /// <summary>
        /// 패널 그룹 (열고 닫기 가능한 UI패널)
        /// </summary>
        Panel = 1,
        
        /// <summary>
        /// 팝업 UI 그룹 (상호작용 영역 진입 시 정보 표기 or 상호작용 버튼)
        /// </summary>
        Popup = 2,
        
        /// <summary>
        /// 로딩UI (모든 화면을 덮는 최상위 레이어 = 가장 앞에 위치)
        /// </summary>
        Loading = 3
    }
}

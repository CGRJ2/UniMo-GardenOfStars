namespace KYS
{
    /// <summary>
    /// 패널 그룹 내 하위 그룹 분류
    /// </summary>
    public enum UIPanelGroup
    {
        /// <summary>
        /// 게임 종료 그룹 (최상위 위치, 게임을 종료하시겠습니까?패널, 경고 패널)
        /// </summary>
        GameExit = 0,
        
        /// <summary>
        /// 상점 그룹 (판매 패널, 구매 패널, 인벤토리 패널 등)
        /// </summary>
        Shop = 1,
        
        /// <summary>
        /// 진행 상황 그룹 (별자리 해금 상태 UI 패널, 지역 패널, 건물 보유 패널 등)
        /// </summary>
        Progress = 2,
        
        /// <summary>
        /// 설정 그룹
        /// </summary>
        Settings = 3,
        
        /// <summary>
        /// 기타 그룹
        /// </summary>
        Other = 4
    }
}

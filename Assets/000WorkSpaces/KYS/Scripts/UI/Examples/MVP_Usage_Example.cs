using UnityEngine;
using UnityEngine.UI;

namespace KYS
{
    /// <summary>
    /// MVP 구조 사용 예시
    /// </summary>
    public class MVP_Usage_Example : MonoBehaviour
    {
        [Header("MVP 구조 테스트")]
        [SerializeField] private Button testMenuButton;
        [SerializeField] private Button testSimpleButton;
        
        private void Start()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (testMenuButton != null)
            {
                testMenuButton.onClick.AddListener(TestMenuPopUpMVP);
            }
            
            if (testSimpleButton != null)
            {
                testSimpleButton.onClick.AddListener(TestSimpleUI);
            }
        }
        
        /// <summary>
        /// MenuPopUp MVP 구조 테스트
        /// </summary>
        [ContextMenu("Test MenuPopUp MVP")]
        public void TestMenuPopUpMVP()
        {
            Debug.Log("[MVP_Usage_Example] MenuPopUp MVP 구조 테스트 시작");
            
            // MenuPopUp 표시 (MVP 구조 적용됨)
            MenuPopUp.ShowMenuPopUp();
            
            // MVP 구조 설명
            Debug.Log(@"
=== MenuPopUp MVP 구조 ===
1. View (MenuPopUp.cs): UI 요소와 사용자 입력만 담당
   - 버튼 클릭 이벤트를 Presenter로 전달
   - UI 상태 업데이트 메서드 제공

2. Presenter (MenuPopUpPresenter.cs): 비즈니스 로직 담당
   - View의 이벤트를 구독하여 처리
   - Model의 데이터를 사용하여 로직 처리
   - View의 UI 상태 업데이트

3. Model (MenuPopUpModel.cs): 데이터와 상태 관리
   - 게임 상태, 메뉴 데이터 관리
   - PlayerPrefs를 통한 데이터 저장/로드
   - 비즈니스 규칙 검증
            ");
        }
        
        /// <summary>
        /// 간단한 UI 테스트 (View만 사용)
        /// </summary>
        [ContextMenu("Test Simple UI")]
        public void TestSimpleUI()
        {
            Debug.Log("[MVP_Usage_Example] 간단한 UI 테스트 시작");
            
            // CheckPopUp 표시 (View만 사용)
            CheckPopUp.ShowCheckPopUp(
                "간단한 UI는 View만 사용합니다.",
                "확인",
                "취소",
                () => Debug.Log("확인 버튼 클릭"),
                () => Debug.Log("취소 버튼 클릭")
            );
            
            Debug.Log(@"
=== 간단한 UI 구조 ===
- CheckPopUp: View만 사용 (BaseUI 상속)
- 복잡한 비즈니스 로직이 없어서 MVP 전체 적용 불필요
- 버튼 클릭 시 직접 처리
            ");
        }
        
        /// <summary>
        /// MVP vs 단순 구조 비교
        /// </summary>
        [ContextMenu("Compare MVP vs Simple")]
        public void CompareMVPvsSimple()
        {
            Debug.Log(@"
=== MVP vs 단순 구조 비교 ===

1. 복잡한 UI (MenuPopUp) - MVP 적용 권장
   - 장점: 비즈니스 로직 분리, 테스트 용이, 확장성
   - 단점: 파일 수 증가, 초기 설정 복잡
   - 파일: MenuPopUp.cs + MenuPopUpPresenter.cs + MenuPopUpModel.cs

2. 간단한 UI (CheckPopUp) - View만 사용
   - 장점: 간단함, 빠른 개발
   - 단점: 로직이 View에 혼재
   - 파일: CheckPopUp.cs

3. 선택 기준:
   - 간단한 UI: 버튼 클릭만, 데이터 없음 → View만
   - 보통 UI: 여러 버튼, 간단한 상태 → View + Presenter
   - 복잡한 UI: 데이터 저장/로드, 복잡한 로직 → MVP 전체
            ");
        }
        
        private void OnDestroy()
        {
            if (testMenuButton != null)
            {
                testMenuButton.onClick.RemoveListener(TestMenuPopUpMVP);
            }
            
            if (testSimpleButton != null)
            {
                testSimpleButton.onClick.RemoveListener(TestSimpleUI);
            }
        }
    }
}

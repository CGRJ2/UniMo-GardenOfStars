using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace KYS
{
    [System.Serializable]
    public class ZodiacStageData
    {
        public string stageId;
        public string stageName; // 성좌 이름 (예: "물고기자리")
        public Sprite wheelImage; // 돌림판에 표시될 이미지
        public Sprite centerImage; // 중앙 상단에 표시될 이미지
    }

    public class StageTransitionPanel : BaseUI
    {
        #region Inspector Fields
        [Header("돌림판 설정")]
        [SerializeField] private Transform wheelParent; // 회전할 부모 오브젝트 (RadialLayout)
        //[SerializeField] private float wheelRadius = 200f; // 돌림판 반지름
        [SerializeField] private float rotationSpeed = 30f; // 회전 속도 (낮을수록 느림)
        [SerializeField] private float snapDuration = 0.5f; // 스냅 애니메이션 시간

        [Header("스테이지 강조 설정")]
        [SerializeField] private float selectedScale = 1.3f; // 선택된 스테이지 크기
        [SerializeField] private float normalScale = 1.0f; // 일반 스테이지 크기
        [SerializeField] private float selectedYOffset = 20f; // 선택된 스테이지 위로 이동 거리
        [SerializeField] private Color selectedColor = Color.white; // 선택된 스테이지 색상
        [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 0.8f, 0.8f); // 일반 스테이지 색상
        [SerializeField] private float highlightAnimationDuration = 0.3f; // 강조 애니메이션 시간
        [SerializeField] private bool hideUnselectedButtons = false; // 선택되지 않은 버튼 이미지 비활성화

        [Header("12성좌 데이터")]
        [SerializeField] private List<ZodiacStageData> zodiacStages = new List<ZodiacStageData>();

        [Header("UI 요소")]
        [SerializeField] private string stageNameTextName = "RunConstellationText"; // 스테이지 이름 텍스트
        [SerializeField] private string stageIconImageName = "ConstellationCharacterImage"; // 스테이지 아이콘 이미지
        [SerializeField] private string backButtonName = "CloseButton"; // 뒤로가기 버튼
        #endregion

        #region UI References
        // UI 요소들
        private TextMeshProUGUI stageNameText => GetUI<TextMeshProUGUI>(stageNameTextName);
        private Image stageIconImage => GetUI<Image>(stageIconImageName);
        private Button backButton => GetUI<Button>(backButtonName);
        #endregion

        #region Private Variables
        // 돌림판 관련 변수
        private bool isDragging = false;
        private Vector2 lastTouchPos;
        private float currentRotation = 0f;
        private bool isSnapping = false;

        // 스테이지 버튼 관리
        private List<Button> stageButtons = new List<Button>();
        private List<RectTransform> stageButtonTransforms = new List<RectTransform>();
        private List<Image> stageButtonImages = new List<Image>();
        private List<Vector3> originalPositions = new List<Vector3>(); // 원래 위치 저장

        // 애니메이션
        private Coroutine snapCoroutine;
        private Coroutine highlightCoroutine;
        
        // 현재 활성화된 스테이지 인덱스
        private int _lastActiveStageIndex = -1;
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();

            Manager.Audio.BgmPlay("StageTran", 0.5f);
            Manager.Audio.SfxPlay("Portal");

            zodiacStages.Clear();
            foreach (StageDataCsv data in Manager.data.Stage.Values.Values)
            {
                ZodiacStageData zodiac = new ZodiacStageData();

                zodiac.stageId = data.Id;
                zodiac.stageName = data.Name_KR;
                zodiac.wheelImage = data.WheelSprite;
                zodiac.centerImage = data.CenterSprite;

                zodiacStages.Add(zodiac);
            }

            Initialize();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_stage_select",
                "ui_stage_select_button",
                "ui_back_button"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            SetupWheel();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            StopAllCoroutines();
        }

        private void Update()
        {
            HandleInput();
        }
        #endregion

        #region Initialization
        private void SetupButtons()
        {
            Debug.Log($"[StageTransitionPanel] SetupButtons() 시작 - Time: {Time.time}, isButtonsSetup: {isButtonsSetup}");

            // 이미 설정되었으면 중복 호출 방지
            if (isButtonsSetup)
            {
                Debug.Log($"[StageTransitionPanel] SetupButtons 이미 완료됨 - 중복 호출 방지");
                return;
            }

            // 뒤로가기 버튼 설정
            var backEventHandler = GetEventWithSFX(backButtonName, "SFX_ButtonClickBack");
            if (backEventHandler != null)
            {
                backEventHandler.Click += (data) => OnBackButtonClicked();
            }

            isButtonsSetup = true; // 설정 완료 플래그
        }

        private void SetupWheel()
        {
            if (wheelParent == null)
            {
                Debug.LogError("wheelParent가 설정되지 않았습니다! Inspector에서 RadialLayout을 설정해주세요.");
                return;
            }

            // 기존 스테이지 버튼들 찾기
            FindExistingStageButtons();

            // 초기 회전 설정
            currentRotation = 0f;
            if (wheelParent != null)
            {
                wheelParent.rotation = Quaternion.Euler(0, 0, currentRotation);
            }

            // 초기 선택된 스테이지 설정
            UpdateSelectedStage();
        }

        private void FindExistingStageButtons()
        {
            // 기존 리스트들 클리어
            stageButtons.Clear();
            stageButtonTransforms.Clear();
            stageButtonImages.Clear();
            originalPositions.Clear();

            if (wheelParent == null) return;

            Debug.Log($"=== 스테이지 버튼 찾기 시작 ===");
            Debug.Log($"zodiacStages.Count: {zodiacStages.Count}");

            // RadialLayout 하위의 모든 Image 컴포넌트 찾기 (RadialLayout 자체 제외)
            Image[] allImages = wheelParent.GetComponentsInChildren<Image>();
            List<Image> stageImages = new List<Image>();

            // RadialLayout 자체는 제외하고 실제 스테이지 버튼들만 필터링
            for (int i = 0; i < allImages.Length; i++)
            {
                Image image = allImages[i];
                // RadialLayout 자체가 아닌 자식 오브젝트들만 포함
                if (image.transform != wheelParent)
                {
                    stageImages.Add(image);
                    Debug.Log($"스테이지 이미지 추가: {image.name} (부모: {image.transform.parent.name})");
                }
                else
                {
                    Debug.Log($"RadialLayout 자체 제외: {image.name}");
                }
            }

            if (stageImages.Count == 0)
            {
                Debug.LogWarning("RadialLayout 하위에 스테이지 이미지가 없습니다!");
                return;
            }

            Debug.Log($"찾은 스테이지 이미지 개수: {stageImages.Count}");
            Debug.Log($"처리할 스테이지 개수: {Mathf.Min(stageImages.Count, zodiacStages.Count)}");

            // 각 이미지를 버튼으로 처리 (데이터 개수에 맞춰 처리)
            for (int i = 0; i < stageImages.Count && i < zodiacStages.Count; i++)
            {
                Image stageImage = stageImages[i];
                RectTransform rectTransform = stageImage.GetComponent<RectTransform>();

                Debug.Log($"--- 스테이지 {i} 처리 시작 ---");
                Debug.Log($"이미지 이름: {stageImage.name}");
                Debug.Log($"RectTransform 존재: {rectTransform != null}");

                if (rectTransform != null)
                {
                    // Image를 Button으로 변환하거나 기존 Button 찾기
                    Button button = stageImage.GetComponent<Button>();
                    if (button == null)
                    {
                        // Image에 Button 컴포넌트가 없으면 추가
                        button = stageImage.gameObject.AddComponent<Button>();
                        Debug.Log($"버튼 컴포넌트 추가됨: {stageImage.name}");
                    }

                    // 12성좌 이미지 설정 (돌림판용 이미지)
                    if (i < zodiacStages.Count && zodiacStages[i].wheelImage != null)
                    {
                        stageImage.sprite = zodiacStages[i].wheelImage;
                        stageImage.color = Color.white;
                        Debug.Log($"스테이지 {i} 이미지 설정: {zodiacStages[i].stageName} -> {zodiacStages[i].wheelImage.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"스테이지 {i}의 wheelImage가 설정되지 않았습니다: {zodiacStages[i].stageName}");
                    }

                    stageButtons.Add(button);
                    stageButtonTransforms.Add(rectTransform);
                    stageButtonImages.Add(stageImage);
                    originalPositions.Add(rectTransform.position); // 원래 위치 저장

                    // 버튼 클릭 이벤트 연결
                    button.onClick.RemoveAllListeners();
                    int stageIndex = i;
                    button.onClick.AddListener(() => OnStageButtonClicked(stageIndex));

                    Debug.Log($"스테이지 {i} 추가 완료: {stageImage.name}");
                }
                else
                {
                    Debug.LogWarning($"스테이지 {i}에 RectTransform이 없습니다: {stageImage.name}");
                }

                Debug.Log($"--- 스테이지 {i} 처리 완료 ---");
            }

            Debug.Log($"=== 스테이지 버튼 찾기 완료 ===");
            Debug.Log($"총 {stageButtons.Count}개의 스테이지가 설정되었습니다.");
            Debug.Log($"stageButtons.Count: {stageButtons.Count}");
            Debug.Log($"stageButtonTransforms.Count: {stageButtonTransforms.Count}");
            Debug.Log($"stageButtonImages.Count: {stageButtonImages.Count}");
            Debug.Log($"originalPositions.Count: {originalPositions.Count}");

            // 기존 RadialLayout 위치를 그대로 사용하므로 자동 배치 제거
        }
        #endregion

        #region Input Handling
        private void HandleInput()
        {
            if (isSnapping) return;

            // 터치 입력 처리 (모바일)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnInputBegan(touch.position);
                        break;
                    case TouchPhase.Moved:
                        OnInputMoved(touch.position);
                        break;
                    case TouchPhase.Ended:
                        OnInputEnded(touch.position);
                        break;
                }
            }
            // 마우스 입력 처리 (에디터)
            else if (Input.GetMouseButtonDown(0))
            {
                OnInputBegan(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                OnInputMoved(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnInputEnded(Input.mousePosition);
            }
        }

        private float _startAngle;
        private Vector2 _startTouchPostion;

        private void OnInputBegan(Vector2 position)
        {
            isDragging = true;
            lastTouchPos = position;

            _startAngle = wheelParent.rotation.eulerAngles.z;
            _startTouchPostion = position;

            // 진행 중인 애니메이션 중지
            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
                snapCoroutine = null;
            }
        }

        private void OnInputMoved(Vector2 position)
        {
            if (!isDragging) return;

            Vector2 vector1 = _startTouchPostion - (Vector2)wheelParent.position;
            Vector2 vector2 = position - (Vector2)wheelParent.position;

            float angle = Vector2.Angle(vector1, vector2);

            float cross = vector1.x * vector2.y - vector1.y * vector2.x;

            if (cross < 0)
            {
                angle *= -1;
            }

            Quaternion moveRotation = Quaternion.Euler(0, 0, _startAngle) * Quaternion.Euler(0, 0, angle);

            currentRotation = _startAngle + angle;

            wheelParent.rotation = moveRotation;

            lastTouchPos = position;
            return;
        }

        private void OnInputEnded(Vector2 position)
        {
            if (!isDragging) return;

            isDragging = false;

            // 12시 방향으로 스냅
            SnapToNearestStage();
        }
        #endregion

        #region Wheel Rotation & Snapping
        private void SnapToNearestStage()
        {
            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
            }
            snapCoroutine = StartCoroutine(SnapCoroutine());
        }

        private IEnumerator SnapCoroutine()
        {
            isSnapping = true;

            // 가장 가까운 스테이지 각도 계산 (12시 방향 기준)
            float targetAngle = CalculateSnapAngle(currentRotation);

            float startRotation = currentRotation;
            float duration = snapDuration;
            float elapsed = 0f;

            Debug.Log($"스냅 시작: {startRotation:F1}° -> {targetAngle:F1}°");

            // 회전 방향 최적화 (가장 짧은 경로 선택)
            float rotationDifference = Mathf.DeltaAngle(startRotation, targetAngle);
            Debug.Log($"회전 차이: {rotationDifference:F1}° (양수: 시계방향, 음수: 반시계방향)");

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 부드러운 스냅
                t = 1f - Mathf.Pow(1f - t, 3f);

                currentRotation = Mathf.Lerp(startRotation, targetAngle, t);

                if (wheelParent != null)
                {
                    Vector3 currentRotationEuler = wheelParent.rotation.eulerAngles;
                    wheelParent.rotation = Quaternion.Euler(currentRotationEuler.x, currentRotationEuler.y, currentRotation);
                }

                yield return null;
            }

            // 정확한 목표 각도로 설정
            currentRotation = targetAngle;
            if (wheelParent != null)
            {
                Vector3 currentRotationEuler = wheelParent.rotation.eulerAngles;
                wheelParent.rotation = Quaternion.Euler(currentRotationEuler.x, currentRotationEuler.y, currentRotation);
            }

            Debug.Log($"스냅 완료: 최종 각도 {currentRotation:F1}°");

            UpdateSelectedStage();
            isSnapping = false;
        }

        private float CalculateSnapAngle(float currentAngle)
        {
            if (stageButtons.Count == 0) return currentAngle;

            // 큰 각도를 먼저 정규화 (360도 이상인 경우)
            float normalizedInputAngle = currentAngle;
            while (normalizedInputAngle >= 360f)
            {
                normalizedInputAngle -= 360f;
            }
            while (normalizedInputAngle < 0f)
            {
                normalizedInputAngle += 360f;
            }

            // 12시 방향(0도)을 기준으로 가장 가까운 스테이지 찾기
            float angleStep = 360f / stageButtons.Count;

            Debug.Log($"=== 스냅 각도 계산 ===");
            Debug.Log($"입력 각도: {currentAngle:F1}°");
            Debug.Log($"정규화된 입력 각도: {normalizedInputAngle:F1}°");
            Debug.Log($"각도 스텝: {angleStep:F1}°");

            // 각 스테이지의 12시 방향 각도 계산 (0도부터 시작)
            float[] stageAngles = new float[stageButtons.Count];
            for (int i = 0; i < stageButtons.Count; i++)
            {
                stageAngles[i] = i * angleStep;
                Debug.Log($"스테이지 {i} 스냅 각도: {stageAngles[i]:F1}°");
            }

            // 현재 각도에서 가장 가까운 스테이지 인덱스 찾기
            float minDistance = float.MaxValue;
            int targetStageIndex = 0;

            for (int i = 0; i < stageAngles.Length; i++)
            {
                // Mathf.DeltaAngle 사용하여 정확한 각도 차이 계산 (360도 롤백 방지)
                float distance = Mathf.Abs(Mathf.DeltaAngle(normalizedInputAngle, stageAngles[i]));

                Debug.Log($"스테이지 {i} 거리: {distance:F1}° (현재: {normalizedInputAngle:F1}°, 목표: {stageAngles[i]:F1}°)");

                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetStageIndex = i;
                }
            }

            // 목표 각도가 현재 각도와 너무 멀면 현재 각도 유지
            if (minDistance > 90f)
            {
                Debug.LogWarning($"목표 각도가 너무 멀어서 현재 각도 유지: {normalizedInputAngle:F1}°");
                return currentAngle;
            }

            // 정확한 회전 방향과 회전량 계산
            float targetNormalizedAngle = stageAngles[targetStageIndex];
            float rotationAmount = Mathf.DeltaAngle(normalizedInputAngle, targetNormalizedAngle);

            // 현재 각도에 회전량을 더해서 최종 목표 각도 계산
            float targetAngle = currentAngle + rotationAmount;

            Debug.Log($"목표 정규화 각도: {targetNormalizedAngle:F1}°");
            Debug.Log($"회전량: {rotationAmount:F1}° (양수: 시계방향, 음수: 반시계방향)");
            Debug.Log($"최종 스냅 각도: {targetAngle:F1}° (거리: {minDistance:F1}°)");

            return targetAngle;
        }
        #endregion

        #region Stage Management
        private void UpdateSelectedStage()
        {
            if (stageButtons.Count == 0) return;

            // 큰 각도를 먼저 정규화 (360도 이상인 경우)
            float normalizedCurrentRotation = currentRotation;
            while (normalizedCurrentRotation >= 360f)
            {
                normalizedCurrentRotation -= 360f;
            }
            while (normalizedCurrentRotation < 0f)
            {
                normalizedCurrentRotation += 360f;
            }

            // 현재 12시 방향에 있는 스테이지 인덱스 계산
            float angleStep = 360f / stageButtons.Count;

            Debug.Log($"=== 스테이지 선택 계산 ===");
            Debug.Log($"현재 회전 각도: {currentRotation:F1}°");
            Debug.Log($"정규화된 회전 각도: {normalizedCurrentRotation:F1}°");
            Debug.Log($"각도 스텝: {angleStep:F1}°");

            // 각 스테이지의 예상 각도 출력
            for (int i = 0; i < stageButtons.Count; i++)
            {
                float stageAngle = i * angleStep;
                Debug.Log($"스테이지 {i} 예상 각도: {stageAngle:F1}°");
            }

            // 가장 가까운 스테이지 인덱스 찾기 (Mathf.DeltaAngle 사용)
            float minDistance = float.MaxValue;
            int currentStageIndex = 0;

            for (int i = 0; i < stageButtons.Count; i++)
            {
                float stageAngle = i * angleStep;
                // Mathf.DeltaAngle 사용하여 정확한 각도 차이 계산 (360도 롤백 방지)
                float distance = Mathf.Abs(Mathf.DeltaAngle(normalizedCurrentRotation, stageAngle));

                Debug.Log($"스테이지 {i} 거리 계산: 현재각도={normalizedCurrentRotation:F1}°, 스테이지각도={stageAngle:F1}°, 거리={distance:F1}°");

                if (distance < minDistance)
                {
                    minDistance = distance;
                    currentStageIndex = i;
                }
            }

            Debug.Log($"선택된 스테이지: {currentStageIndex} (각도: {normalizedCurrentRotation:F1}°, 가장 가까운 거리: {minDistance:F1}°)");

            UpdateUI(currentStageIndex);
            UpdateStageHighlight(currentStageIndex);

            // 12시 방향에 위치하지 않은 버튼들은 클릭 불가능하게 설정
            UpdateButtonInteractability(currentStageIndex);
        }

        private void UpdateUI(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= stageButtonImages.Count) return;
            if (stageIndex >= zodiacStages.Count) return;

            ZodiacStageData currentStageData = zodiacStages[stageIndex];

            // 스테이지 이름 업데이트 (12성좌 이름 사용)
            if (stageNameText != null)
            {
                string stageName = currentStageData.stageName;
                bool isUnlocked = IsStageUnlocked(stageIndex);

                // 잠긴 스테이지인 경우 이름에 잠금 표시 추가
                if (!isUnlocked)
                {
                    stageName += " [잠김]";
                }

                stageNameText.text = stageName;

                // 잠금 상태에 따른 텍스트 색상 변경
                stageNameText.color = isUnlocked ? Color.white : Color.red;
            }

            // 스테이지 아이콘 업데이트 (중앙 상단 전용 이미지 사용)
            if (stageIconImage != null && currentStageData.centerImage != null)
            {
                stageIconImage.sprite = currentStageData.centerImage;

                // 잠금 상태에 따른 아이콘 색상 변경
                bool isUnlocked = IsStageUnlocked(stageIndex);
                stageIconImage.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f);
            }
        }

        private void OnStageButtonClicked(int stageIndex)
        {
            ZodiacStageData stageData = zodiacStages[stageIndex];

            // 스테이지 해금 상태 확인
            if (Manager.firebase.UserData.StageList.Get(stageData.stageId) == null)
            {
                Debug.Log($"스테이지가 잠겨있음: {stageIndex}");
                return;
            }

            // 클릭된 스테이지로 즉시 이동
            JumpToStage(stageIndex);

            // 스테이지 전환 로직 추가
            if (stageIndex < zodiacStages.Count)
            {
                try
                {
                    // 이미 해당 스테이지에 위치한 경우
                    if (Manager.firebase.UserData.CurStage.Value == stageData.stageId)
                    {
                        Debug.Log("이미 해당 스테이지에 위치함");
                        // 현재 위치라도 패널은 닫기
                        Manager.Audio.BgmPlay(Manager.firebase.UserData.CurStage.Value, 0.5f);
                        Manager.ui.ClosePanel();
                        return;
                    }

                    Manager.firebase.UserData.CurStage.Subscribe(TransitionToStage);
                    Manager.firebase.UserData.CurStage.Value = stageData.stageId;
                    Debug.Log($"스테이지 전환: {zodiacStages[stageIndex].stageName} ({stageIndex})");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[StageTransitionPanel] 스테이지 클릭 처리 중 오류 발생: {e.Message}");
                }
            }
        }

        public void JumpToStage(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= stageButtons.Count) return;

            float targetAngle = stageIndex * (360f / stageButtons.Count);

            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
            }

            snapCoroutine = StartCoroutine(JumpToStageCoroutine(targetAngle));
        }

        private IEnumerator JumpToStageCoroutine(float targetAngle)
        {
            float startRotation = currentRotation;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                t = 1f - Mathf.Pow(1f - t, 3f);

                currentRotation = Mathf.Lerp(startRotation, targetAngle, t);

                if (wheelParent != null)
                {
                    Vector3 currentRotationEuler = wheelParent.rotation.eulerAngles;
                    wheelParent.rotation = Quaternion.Euler(currentRotationEuler.x, currentRotationEuler.y, currentRotation);
                }

                yield return null;
            }

            currentRotation = targetAngle;
            if (wheelParent != null)
            {
                Vector3 currentRotationEuler = wheelParent.rotation.eulerAngles;
                wheelParent.rotation = Quaternion.Euler(currentRotationEuler.x, currentRotationEuler.y, currentRotation);
            }

            UpdateSelectedStage();
        }
        #endregion

        #region Scene Transition
        /// <summary>
        /// 선택된 스테이지로 씬 이동
        /// </summary>
        private void TransitionToStage(string stageId)
        {
            Manager.firebase.UserData.CurStage.Unsubscribe(TransitionToStage);
            // 1. 패널 닫기 (씬 이동 전에 UI 정리)
            Manager.ui.ClosePanel();

            // 2. 씬 이동 실행
            StartCoroutine(LoadSceneAsyncAddressables());
        }

        /// <summary>
        /// Addressables를 사용한 비동기 씬 로딩
        /// </summary>
        private IEnumerator LoadSceneAsyncAddressables()
        {
            // 로딩 화면 표시 (필요한 경우)
            // ShowLoadingScreen();

            Manager.Audio.SfxPlay("Portal");
            var handle = Addressables.LoadSceneAsync("StageScene");

            while (!handle.IsDone)
            {
                float progress = handle.PercentComplete;
                // UpdateLoadingProgress(progress);
                yield return null;
            }
        }
        #endregion

        #region Visual Effects
        private void UpdateButtonInteractability(int activeStageIndex)
        {
            for (int i = 0; i < stageButtons.Count; i++)
            {
                if (stageButtons[i] != null)
                {
                    // 12시 방향에 위치한 버튼만 클릭 가능하고, 해금된 스테이지여야 함
                    bool isActive = (i == activeStageIndex);
                    bool isUnlocked = IsStageUnlocked(i);
                    stageButtons[i].interactable = isActive && isUnlocked;

                    // 잠긴 스테이지 시각적 표시
                    UpdateStageLockVisual(i, isUnlocked);
                }
            }
        }

        private void UpdateStageHighlight(int stageIndex)
        {
            _lastActiveStageIndex = stageIndex; // 현재 활성화된 스테이지 인덱스 저장
            
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }
            highlightCoroutine = StartCoroutine(HighlightCoroutine(stageIndex));
        }

        /// <summary>
        /// 스테이지 해금 상태 확인
        /// </summary>
        private bool IsStageUnlocked(int stageIndex)
        {
            return Manager.firebase.UserData.StageList.Get(zodiacStages[stageIndex].stageId) != null;
        }

        /// <summary>
        /// 스테이지 잠금 상태 시각적 표시
        /// </summary>
        private void UpdateStageLockVisual(int stageIndex, bool isUnlocked)
        {
            if (stageIndex < 0 || stageIndex >= stageButtonImages.Count) return;

            Image stageImage = stageButtonImages[stageIndex];
            if (stageImage == null) return;

            if (isUnlocked)
            {
                // 해금된 스테이지: 이미지 비활성화 옵션 고려
                if (hideUnselectedButtons)
                {
                    // 이미지 비활성화 옵션이 활성화된 경우, HighlightCoroutine에서 처리하도록 함
                    // 여기서는 색상 변경하지 않음
                }
                else
                {
                    // 일반 모드: 정상 색상
                    stageImage.enabled = true;
                    stageImage.color = Color.white;
                }
            }
            else
            {
                // 잠긴 스테이지: 숨김 옵션이 켜져 있다면 비선택 시 이미지 비활성화
                if (hideUnselectedButtons && stageIndex != _lastActiveStageIndex)
                {
                    stageImage.enabled = false;
                }
                else
                {
                    // 선택되었거나 숨김 옵션이 꺼진 경우 어두운 색상으로 표시
                    stageImage.enabled = true;
                    stageImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                }
            }
        }

        private IEnumerator HighlightCoroutine(int stageIndex)
        {
            // 모든 스테이지 버튼을 일반 상태로 초기화
            for (int i = 0; i < stageButtonImages.Count; i++)
            {
                if (stageButtonImages[i] != null)
                {
                    // 잠금 상태 확인
                    bool isUnlocked = IsStageUnlocked(i);
                    
                    if (!isUnlocked)
                    {
                        // 잠긴 스테이지: 숨김 옵션이 켜져 있다면 비선택 시 이미지 비활성화
                        if (hideUnselectedButtons && i != stageIndex)
                        {
                            stageButtonImages[i].enabled = false;
                        }
                        else
                        {
                            // 선택되었거나 숨김 옵션이 꺼진 경우 어두운 색상으로 표시
                            stageButtonImages[i].enabled = true;
                            stageButtonImages[i].color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                        }
                    }
                    else
                    {
                        // 해금된 스테이지: 이미지 비활성화 옵션
                        if (hideUnselectedButtons && i != stageIndex)
                        {
                            // 선택되지 않은 버튼은 이미지 비활성화
                            stageButtonImages[i].enabled = false;
                        }
                        else
                        {
                            // 선택된 버튼은 이미지 활성화하고 정상 색상
                            stageButtonImages[i].enabled = true;
                            stageButtonImages[i].color = normalColor;
                        }
                    }
                }
                if (stageButtonTransforms[i] != null)
                {
                    stageButtonTransforms[i].localScale = Vector3.one * normalScale;
                    stageButtonTransforms[i].position = originalPositions[i]; // 원래 위치로 복원
                }

                // 12시 방향에 위치하지 않은 버튼은 클릭 불가능하게 설정
                if (stageButtons[i] != null)
                {
                    stageButtons[i].interactable = (i == stageIndex);
                }
            }

            yield return new WaitForEndOfFrame();

            // 선택된 스테이지 강조
            if (stageIndex < stageButtonImages.Count && stageIndex < stageButtonTransforms.Count)
            {
                Image selectedImage = stageButtonImages[stageIndex];
                Transform selectedTransform = stageButtonTransforms[stageIndex];

                if (selectedImage != null && selectedTransform != null)
                {
                    float elapsed = 0f;
                    float duration = highlightAnimationDuration;

                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / duration;

                        // 색상 변경
                        selectedImage.color = Color.Lerp(normalColor, selectedColor, t);

                        // 크기 변경
                        float scale = Mathf.Lerp(normalScale, selectedScale, t);
                        selectedTransform.localScale = Vector3.one * scale;

                        // 위치 변경 (위로 이동)
                        float moveAmount = Mathf.Lerp(0f, selectedYOffset, t);
                        selectedTransform.position = originalPositions[stageIndex] + new Vector3(0, moveAmount, 0);

                        yield return null;
                    }

                    // 최종 상태 설정
                    selectedImage.color = selectedColor;
                    selectedTransform.localScale = Vector3.one * selectedScale;
                    selectedTransform.position = originalPositions[stageIndex] + new Vector3(0, selectedYOffset, 0);
                }
            }
        }
        #endregion

        #region UI Events
        private void OnBackButtonClicked()
        {
            Debug.Log("뒤로가기");
            Manager.Audio.BgmPlay(Manager.firebase.UserData.CurStage.Value, 0.5f);
            Manager.ui.ClosePanel();
        }
        #endregion

        #region Debug & Utilities
        [ContextMenu("돌림판 정보 출력")]
        public void PrintWheelInfo()
        {
            Debug.Log($"현재 회전 각도: {currentRotation}");
            Debug.Log($"버튼 수: {stageButtons.Count}");
            Debug.Log($"WheelParent: {(wheelParent != null ? "설정됨" : "설정되지 않음")}");
        }
        #endregion

        #region Test Methods
        /// <summary>
        /// 테스트용 씬 이동 (ContextMenu)
        /// </summary>
        [ContextMenu("테스트 - 양자리로 이동")]
        public void TestTransitionToAries()
        {
            TransitionToStage(zodiacStages[0].stageId);
        }

        [ContextMenu("테스트 - 황소자리로 이동")]
        public void TestTransitionToTaurus()
        {
            TransitionToStage(zodiacStages[1].stageId);
        }

        [ContextMenu("테스트 - 쌍둥이자리로 이동")]
        public void TestTransitionToGemini()
        {
            TransitionToStage(zodiacStages[2].stageId);
        }

        [ContextMenu("테스트 - 게자리로 이동")]
        public void TestTransitionToCancer()
        {
            TransitionToStage(zodiacStages[3].stageId);
        }

        [ContextMenu("테스트 - 사자자리로 이동")]
        public void TestTransitionToLeo()
        {
            TransitionToStage(zodiacStages[4].stageId);
        }

        [ContextMenu("테스트 - 처녀자리로 이동")]
        public void TestTransitionToVirgo()
        {
            TransitionToStage(zodiacStages[5].stageId);
        }

        [ContextMenu("테스트 - 천칭자리로 이동")]
        public void TestTransitionToLibra()
        {
            TransitionToStage(zodiacStages[6].stageId);
        }

        [ContextMenu("테스트 - 전갈자리로 이동")]
        public void TestTransitionToScorpio()
        {
            TransitionToStage(zodiacStages[7].stageId);
        }

        [ContextMenu("테스트 - 궁수자리로 이동")]
        public void TestTransitionToSagittarius()
        {
            TransitionToStage(zodiacStages[8].stageId);
        }

        [ContextMenu("테스트 - 염소자리로 이동")]
        public void TestTransitionToCapricorn()
        {
            TransitionToStage(zodiacStages[9].stageId);
        }

        [ContextMenu("테스트 - 물병자리로 이동")]
        public void TestTransitionToAquarius()
        {
            TransitionToStage(zodiacStages[10].stageId);
        }

        [ContextMenu("테스트 - 물고기자리로 이동")]
        public void TestTransitionToPisces()
        {
            TransitionToStage(zodiacStages[11].stageId);
        }

        /// <summary>
        /// 스테이지 해금 상태 정보 출력 (디버깅용)
        /// </summary>
        [ContextMenu("스테이지 해금 상태 정보 출력")]
        public void PrintStageUnlockInfo()
        {
            Debug.Log("[StageTransitionPanel] === 스테이지 해금 상태 정보 ===");

            for (int i = 0; i < zodiacStages.Count; i++)
            {
                string zodiacName = zodiacStages[i].stageName;
                string stageId = zodiacStages[i].stageId;
                bool isUnlocked = IsStageUnlocked(i);

                Debug.Log($"스테이지 {i}: {zodiacName} -> {stageId} [{(isUnlocked ? "해금" : "잠금")}]");
            }
        }
        #endregion
    }
}

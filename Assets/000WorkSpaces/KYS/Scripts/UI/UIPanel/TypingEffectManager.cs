using UnityEngine;
using TMPro;
using DG.Tweening;

namespace KYS
{
    /// <summary>
    /// 타이핑 효과를 관리하는 클래스
    /// StoryPanel과 분리하여 재사용 가능하도록 설계
    /// </summary>
    public class TypingEffectManager : MonoBehaviour
    {
        [Header("타이핑 효과 설정")]
        [SerializeField] private float typingSpeed = 0.05f; // 글자당 딜레이
        [SerializeField] private bool enableTypingEffect = true; // 타이핑 효과 활성화
        [SerializeField] private AudioClip typingSound; // 타이핑 사운드 (선택사항)

        // 타이핑 효과 관련 변수
        private Tween typingTween;
        private string currentTypingText = "";
        private bool isTyping = false;
        private bool isTypingCompleted = false; // 타이핑 완료 후 상태 추적

        // 이벤트
        public System.Action OnTypingStarted;
        public System.Action OnTypingCompleted;
        public System.Action OnTypingInterrupted;

        #region Properties

        /// <summary>
        /// 현재 타이핑 중인지 확인
        /// </summary>
        public bool IsTyping => isTyping;

        /// <summary>
        /// 타이핑이 방금 완료되었는지 확인
        /// </summary>
        public bool IsTypingCompleted => isTypingCompleted;

        /// <summary>
        /// 타이핑 효과 활성화 여부
        /// </summary>
        public bool EnableTypingEffect
        {
            get => enableTypingEffect;
            set => enableTypingEffect = value;
        }

        /// <summary>
        /// 타이핑 속도
        /// </summary>
        public float TypingSpeed
        {
            get => typingSpeed;
            set => typingSpeed = value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 타이핑 효과 시작
        /// </summary>
        /// <param name="fullText">전체 텍스트</param>
        /// <param name="targetText">타겟 TextMeshProUGUI</param>
        /// <param name="useTyping">타이핑 효과 사용 여부</param>
        public void StartTypingEffect(string fullText, TextMeshProUGUI targetText, bool useTyping = true)
        {
            if (!useTyping || !enableTypingEffect)
            {
                // 타이핑 효과 없이 즉시 텍스트 설정
                targetText.text = fullText;
                return;
            }

            // 기존 타이핑 효과 정리
            if (typingTween != null && typingTween.IsActive())
            {
                typingTween.Kill();
                isTyping = false;
                isTypingCompleted = false;
            }

            currentTypingText = fullText;
            isTyping = true;
            isTypingCompleted = false;
            targetText.text = "";

            // DoTween을 사용한 타이핑 효과
            typingTween = DOTween.To(() => 0, (int value) => {
                if (value < fullText.Length)
                {
                    targetText.text = fullText.Substring(0, value + 1);
                    
                    // 타이핑 사운드 재생 (선택사항)
                    if (typingSound != null)
                    {
                        AudioSource.PlayClipAtPoint(typingSound, Camera.main.transform.position, 0.3f);
                    }
                }
            }, fullText.Length - 1, fullText.Length * typingSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                isTyping = false;
                isTypingCompleted = true;
                currentTypingText = "";
                targetText.text = fullText;
                OnTypingCompleted?.Invoke();
                Debug.Log("[TypingEffectManager] 타이핑 효과 완료");
            });

            OnTypingStarted?.Invoke();
        }

        /// <summary>
        /// 타이핑 효과 즉시 완료
        /// </summary>
        /// <param name="targetText">타겟 TextMeshProUGUI</param>
        public void CompleteTyping(TextMeshProUGUI targetText)
        {
            if (typingTween != null && typingTween.IsActive())
            {
                // 타이핑 효과 즉시 완료
                typingTween.Kill();
                
                // 현재 타이핑 중인 텍스트를 즉시 완성
                if (targetText != null)
                {
                    targetText.text = currentTypingText;
                }
                
                // 타이핑 상태 정리
                isTyping = false;
                isTypingCompleted = true;
                currentTypingText = "";
                
                OnTypingInterrupted?.Invoke();
                Debug.Log("[TypingEffectManager] 타이핑 효과 즉시 완료됨");
            }
        }

        /// <summary>
        /// 타이핑 효과 중지
        /// </summary>
        public void StopTyping()
        {
            if (typingTween != null && typingTween.IsActive())
            {
                typingTween.Kill();
                isTyping = false;
                isTypingCompleted = false;
                currentTypingText = "";
                
                OnTypingInterrupted?.Invoke();
                Debug.Log("[TypingEffectManager] 타이핑 효과 중지됨");
            }
        }

        /// <summary>
        /// 타이핑 완료 상태 초기화 (다음 클릭을 위해)
        /// </summary>
        public void ResetTypingCompleted()
        {
            isTypingCompleted = false;
        }

        /// <summary>
        /// 타이핑 사운드 설정
        /// </summary>
        /// <param name="sound">타이핑 사운드</param>
        public void SetTypingSound(AudioClip sound)
        {
            typingSound = sound;
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            // 컴포넌트 제거 시 타이핑 효과 정리
            if (typingTween != null && typingTween.IsActive())
            {
                typingTween.Kill();
            }
        }

        #endregion
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMoveUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] RectTransform target;     // 비워두면 자기 RectTransform 사용

    [Header("Motion")]
    [SerializeField] float amplitude = 30f;    // 위아래 이동 거리(px, Canvas Scaler 기준)
    [SerializeField] float duration = 0.8f;    // 올라가는데 걸리는 시간(한쪽 편도)
    [SerializeField] float startDelay = 0f;    // 시작 지연
    [SerializeField] Ease ease = Ease.InOutSine;
    [SerializeField] bool playOnEnable = true;

    [Header("Optional: 투명도 펄스")]
    [SerializeField] bool pulseAlpha = false;
    [SerializeField] float minAlpha = 0.6f;
    [SerializeField] float pulseDuration = 0.8f; // 왕복과 같은 템포로 두는 게 자연스러움

    Tween moveTween;
    Tween alphaTween;
    Vector2 baseAnchoredPos;
    CanvasGroup cg;

    void Awake()
    {
        if (target == null) target = GetComponent<RectTransform>();
        baseAnchoredPos = target.anchoredPosition;

        if (pulseAlpha)
        {
            cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void OnEnable()
    {
        if (playOnEnable) Play();
    }

    void OnDisable()
    {
        KillTweens();
        // 위치 원복을 원하면 주석 해제
        // if (target != null) target.anchoredPosition = baseAnchoredPos;
    }

    public void Play()
    {
        if (target == null) return;
        KillTweens();

        // 위아래 왕복
        var upPos = baseAnchoredPos + new Vector2(0f, amplitude);

        moveTween = target.DOAnchorPos(upPos, duration)
            .SetEase(ease)
            .SetDelay(startDelay)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true); // TimeScale 영향 안 받게 하려면 SetUpdate(true) 유지

        if (pulseAlpha && cg != null)
        {
            alphaTween = cg.DOFade(minAlpha, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
    }

    public void Stop(bool resetPosition = false)
    {
        KillTweens();
        if (resetPosition && target != null)
        {
            target.anchoredPosition = baseAnchoredPos;
            if (pulseAlpha && cg != null) cg.alpha = 1f;
        }
    }

    public void SetAmplitude(float newAmplitude)
    {
        amplitude = newAmplitude;
        // 실시간 반영하려면 다시 Play
        if (moveTween != null && moveTween.IsActive()) Play();
    }

    void KillTweens()
    {
        moveTween?.Kill();
        moveTween = null;
        alphaTween?.Kill();
        alphaTween = null;
    }
}

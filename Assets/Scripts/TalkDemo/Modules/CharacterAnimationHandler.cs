using System.Collections;
using UnityEngine;

// ============================================================
// CharacterAnimationHandler.cs
// 純 C# 類別：角色情緒動畫（取代原本的 CharacterAnimator MonoBehaviour）
//
// 透過建構子接收 MonoBehaviour 引用來代理 Coroutine 呼叫，
// 不需要自己掛在 GameObject 上
// ============================================================

public class CharacterAnimationHandler
{
    private readonly RectTransform characterTransform;
    private readonly MonoBehaviour coroutineHost;
    private readonly float animationSpeed;

    private Vector2 originalPosition;
    private Quaternion originalRotation;
    private Coroutine currentAnimation;
    private Coroutine idleAnimation;

    public CharacterAnimationHandler(RectTransform target, MonoBehaviour host, float speed = 1f)
    {
        characterTransform = target;
        coroutineHost = host;
        animationSpeed = speed;
    }

    // --------------------------------------------------------
    // 初始化：記錄起始狀態並啟動待機動畫
    // --------------------------------------------------------

    public void Initialize()
    {
        if (characterTransform == null) return;

        originalPosition = characterTransform.anchoredPosition;
        originalRotation = characterTransform.localRotation;
        StartIdleAnimation();
    }

    // --------------------------------------------------------
    // 公開方法：播放情緒動畫
    // --------------------------------------------------------

    public void PlayEmotion(EmotionType emotion)
    {
        if (currentAnimation != null)
            coroutineHost.StopCoroutine(currentAnimation);
        if (idleAnimation != null)
            coroutineHost.StopCoroutine(idleAnimation);

        switch (emotion)
        {
            case EmotionType.Happy:
                currentAnimation = coroutineHost.StartCoroutine(PlayHappy());
                break;
            case EmotionType.Sad:
                currentAnimation = coroutineHost.StartCoroutine(PlaySad());
                break;
            case EmotionType.Surprised:
                currentAnimation = coroutineHost.StartCoroutine(PlaySurprised());
                break;
            case EmotionType.Angry:
                currentAnimation = coroutineHost.StartCoroutine(PlayAngry());
                break;
            case EmotionType.Calm:
            case EmotionType.Neutral:
            default:
                StartIdleAnimation();
                break;
        }
    }

    // --------------------------------------------------------
    // 待機動畫：平靜狀態下持續輕微上下浮動
    // --------------------------------------------------------

    private void StartIdleAnimation()
    {
        idleAnimation = coroutineHost.StartCoroutine(PlayIdleFloat());
    }

    private IEnumerator PlayIdleFloat()
    {
        float elapsed = 0f;
        float floatRange = 6f;
        float floatSpeed = 1.2f;

        while (true)
        {
            elapsed += Time.deltaTime * floatSpeed * animationSpeed;
            float offsetY = Mathf.Sin(elapsed) * floatRange;
            characterTransform.anchoredPosition =
                originalPosition + new Vector2(0, offsetY);
            yield return null;
        }
    }

    // --------------------------------------------------------
    // 開心動畫：快速跳起再落下，重複兩次
    // --------------------------------------------------------

    private IEnumerator PlayHappy()
    {
        float jumpHeight = 30f;
        float jumpDuration = 0.25f / animationSpeed;

        for (int i = 0; i < 2; i++)
        {
            yield return MoveY(0, jumpHeight, jumpDuration);
            yield return MoveY(jumpHeight, 0, jumpDuration);
        }

        yield return ResetToOriginal();
        StartIdleAnimation();
    }

    // --------------------------------------------------------
    // 難過動畫：緩慢往下沈，停一下，再慢慢回來
    // --------------------------------------------------------

    private IEnumerator PlaySad()
    {
        float sinkDistance = 20f;
        float sinkDuration = 0.6f / animationSpeed;
        float holdDuration = 0.8f / animationSpeed;

        yield return MoveY(0, -sinkDistance, sinkDuration, easeIn: true);
        yield return new WaitForSeconds(holdDuration);
        yield return MoveY(-sinkDistance, 0, sinkDuration, easeIn: false);

        yield return ResetToOriginal();
        StartIdleAnimation();
    }

    // --------------------------------------------------------
    // 驚訝動畫：快速往後傾斜再回正
    // --------------------------------------------------------

    private IEnumerator PlaySurprised()
    {
        float tiltAngle = -15f;
        float tiltDuration = 0.15f / animationSpeed;
        float holdDuration = 0.3f / animationSpeed;

        yield return RotateZ(0, tiltAngle, tiltDuration);
        yield return new WaitForSeconds(holdDuration);
        yield return RotateZ(tiltAngle, 0, tiltDuration * 2);

        yield return ResetToOriginal();
        StartIdleAnimation();
    }

    // --------------------------------------------------------
    // 憤怒動畫：快速左右搖晃三次
    // --------------------------------------------------------

    private IEnumerator PlayAngry()
    {
        float shakeDistance = 12f;
        float shakeDuration = 0.08f / animationSpeed;

        for (int i = 0; i < 3; i++)
        {
            yield return MoveX(0, shakeDistance, shakeDuration);
            yield return MoveX(shakeDistance, -shakeDistance, shakeDuration);
            yield return MoveX(-shakeDistance, 0, shakeDuration);
        }

        yield return ResetToOriginal();
        StartIdleAnimation();
    }

    // --------------------------------------------------------
    // 底層動畫工具：位移與旋轉
    // --------------------------------------------------------

    private IEnumerator MoveY(float fromY, float toY, float duration, bool easeIn = false)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easeIn ? t * t : 1 - (1 - t) * (1 - t);
            float currentY = Mathf.Lerp(fromY, toY, easedT);
            characterTransform.anchoredPosition =
                originalPosition + new Vector2(0, currentY);
            yield return null;
        }
    }

    private IEnumerator MoveX(float fromX, float toX, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentX = Mathf.Lerp(fromX, toX, t);
            characterTransform.anchoredPosition =
                originalPosition + new Vector2(currentX, 0);
            yield return null;
        }
    }

    private IEnumerator RotateZ(float fromAngle, float toAngle, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentAngle = Mathf.Lerp(fromAngle, toAngle, t);
            characterTransform.localRotation =
                Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }
    }

    private IEnumerator ResetToOriginal()
    {
        characterTransform.anchoredPosition = originalPosition;
        characterTransform.localRotation = originalRotation;
        yield return null;
    }
}

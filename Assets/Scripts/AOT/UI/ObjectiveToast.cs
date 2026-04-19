using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FPS.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.UI
{
    public sealed class ObjectiveToast : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("标题文本")]
        public TextMeshProUGUI titleTextContent;

        [Tooltip("描述文本")]
        public TextMeshProUGUI descriptionTextContent;

        [Tooltip("计数文本")]
        public TextMeshProUGUI counterTextContent;

        [Tooltip("子标题物体")]
        public RectTransform subTitleRect;

        [Tooltip("任务根物体")]
        public CanvasGroup canvasGroup;

        [Tooltip("任务布局组件")]
        public HorizontalOrVerticalLayoutGroup layoutGroup;

        [Header("Transitions")]
        [Tooltip("完成延迟")]
        public float completionDelay;

        [Tooltip("淡入时长")]
        public float fadeInDuration = 0.5f;

        [Tooltip("淡出时长")]
        public float fadeOutDuration = 2f;

        [Header("Sound")]
        [Tooltip("任务开始音效")]
        public AudioClip initSound;

        [Tooltip("任务完成音效")]
        public AudioClip completedSound;

        [Header("Movement")]
        [Tooltip("滑入时长")]
        public float moveInDuration = 0.5f;

        [Tooltip("滑入进度曲线")]
        public AnimationCurve moveInCurve;

        [Tooltip("滑出时长")]
        public float moveOutDuration = 2f;

        [Tooltip("滑出进度曲线")]
        public AnimationCurve moveOutCurve;

        private AudioSource m_AudioSource;
        private RectTransform m_RectTransform;

        // 用于管理和随时打断异步任务的令牌
        private CancellationTokenSource m_AnimationCts;

        private void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(string titleText, string descText, string counterText, bool isOptional, float delay)
        {
            Canvas.ForceUpdateCanvases();

            titleTextContent.text = titleText;
            descriptionTextContent.text = descText;
            counterTextContent.text = counterText;
            subTitleRect.gameObject.SetActive(!string.IsNullOrEmpty(descriptionTextContent.text));

            if (m_RectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_RectTransform);
            }

            PlayIntroSequenceAsync(delay).Forget();
        }

        public void Complete()
        {
            PlayOutroSequenceAsync().Forget();
        }

        private async UniTaskVoid PlayIntroSequenceAsync(float delay)
        {
            ResetCancellationToken();
            var ct = m_AnimationCts.Token;

            try
            {
                // 等待初始延迟
                if (delay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);
                }

                // 并行执行：淡入 + 滑入
                await UniTask.WhenAll(
                    FadeAsync(0f, 1f, fadeInDuration, ct),
                    MoveAsync(moveInCurve, moveInDuration, ct)
                );

                // 播放音效
                PlaySound(initSound);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTaskVoid PlayOutroSequenceAsync()
        {
            ResetCancellationToken();
            var ct = m_AnimationCts.Token;

            try
            {
                // 等待完成后的驻留延迟
                if (completionDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(completionDelay), cancellationToken: ct);
                }

                // 准备退出，播放音效
                PlaySound(completedSound);

                // 并行执行：淡出 + 滑出
                await UniTask.WhenAll(
                    FadeAsync(canvasGroup.alpha, 0f, fadeOutDuration, ct),
                    MoveAsync(moveOutCurve, moveOutDuration, ct)
                );

                // 彻底结束，销毁物体
                Destroy(gameObject);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask FadeAsync(float startAlpha, float endAlpha, float duration, CancellationToken ct)
        {
            var time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);

                // 等待下一帧
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: ct);
            }
            canvasGroup.alpha = endAlpha;
        }

        private async UniTask MoveAsync(AnimationCurve curve, float duration, CancellationToken ct)
        {
            var time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;

                layoutGroup.padding.left = (int) curve.Evaluate(time / duration);

                //LayoutRebuilder.MarkLayoutForRebuild(m_RectTransform);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: ct);
            }
            layoutGroup.padding.left = (int) curve.Evaluate(1f);
        }

        private void PlaySound(AudioClip sound)
        {
            if (!sound)
            {
                return;
            }

            if (!m_AudioSource)
            {
                m_AudioSource = gameObject.AddComponent<AudioSource>();
                m_AudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDObjective);
            }

            m_AudioSource.PlayOneShot(sound);
        }

        private void ResetCancellationToken()
        {
            if (m_AnimationCts != null)
            {
                m_AnimationCts.Cancel();
                m_AnimationCts.Dispose();
            }
            m_AnimationCts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            // 物体销毁时必须打断所有异步任务
            if (m_AnimationCts != null)
            {
                m_AnimationCts.Cancel();
                m_AnimationCts.Dispose();
            }
        }
    }
}
using FPS.Game;
using FPS.Game.Managers;
using FPS.Game.Shared;
using FPS.GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.UI
{
    public sealed class FeedbackFlashHUD : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("闪烁图片")]
        public Image FlashImage;

        [Tooltip("闪烁组")]
        public CanvasGroup FlashCanvasGroup;

        [Tooltip("暗角组")]
        public CanvasGroup VignetteCanvasGroup;

        [Header("Damage")]
        [Tooltip("受伤闪烁颜色")]
        public Color DamageFlashColor;

        [Tooltip("闪烁时长")]
        public float DamageFlashDuration;

        [Tooltip("受伤闪烁最大透明度")]
        public float DamageFlashMaxAlpha = 1f;

        [Header("Critical health")]
        [Tooltip("最大残血暗角透明度")]
        public float criticalHealthVignetteMaxAlpha = .8f;

        [Tooltip("残血闪烁频率")]
        public float PulsatingVignetteFrequency = 4f;

        [Header("Heal")]
        [Tooltip("回血闪烁颜色")]
        public Color HealFlashColor;

        [Tooltip("回血闪烁时长")]
        public float HealFlashDuration;

        [Tooltip("回血闪烁最大透明度")]
        public float HealFlashMaxAlpha = 1f;

        bool m_FlashActive;
        float m_LastTimeFlashStarted = Mathf.NegativeInfinity;
        Health m_PlayerHealth;
        GameFlowManager m_GameFlowManager;

        void Start()
        {
            var playerCharacterController = FindFirstObjectByType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, FeedbackFlashHUD>(
                playerCharacterController, this);

            m_PlayerHealth = playerCharacterController.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, FeedbackFlashHUD>(m_PlayerHealth, this,
                playerCharacterController.gameObject);

            m_GameFlowManager = FindFirstObjectByType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, FeedbackFlashHUD>(m_GameFlowManager, this);

            m_PlayerHealth.onDamaged += OnTakeDamage;
            m_PlayerHealth.onHealed += OnHealed;
        }

        void Update()
        {
            if (m_PlayerHealth.IsCritical())
            {
                //残血时显示暗角
                VignetteCanvasGroup.gameObject.SetActive(true);
                // 残血后血量越低暗角越大
                var vignetteAlpha =
                    (1 - (m_PlayerHealth.currentHealth / m_PlayerHealth.maxHealth /
                          m_PlayerHealth.criticalHealthRatio)) * criticalHealthVignetteMaxAlpha;

                // 游戏结束时暗角为最大不变, 否则闪烁
                if (m_GameFlowManager.gameIsEnding)
                {
                    VignetteCanvasGroup.alpha = vignetteAlpha;
                }
                else
                {
                    VignetteCanvasGroup.alpha =
                        ((Mathf.Sin(Time.time * PulsatingVignetteFrequency) / 2) + 0.5f) * vignetteAlpha;
                }
            }
            else
            {
                VignetteCanvasGroup.gameObject.SetActive(false);
            }

            // 闪烁
            if (m_FlashActive)
            {
                var normalizedTimeSinceDamage = (Time.time - m_LastTimeFlashStarted) / DamageFlashDuration;

                if (normalizedTimeSinceDamage < 1f)
                {
                    var flashAmount = DamageFlashMaxAlpha * (1f - normalizedTimeSinceDamage);
                    FlashCanvasGroup.alpha = flashAmount;
                }
                else
                {
                    FlashCanvasGroup.gameObject.SetActive(false);
                    m_FlashActive = false;
                }
            }
        }

        /// <summary>
        /// 重置闪烁
        /// </summary>
        void ResetFlash()
        {
            m_LastTimeFlashStarted = Time.time;
            m_FlashActive = true;
            FlashCanvasGroup.alpha = 0f;
            FlashCanvasGroup.gameObject.SetActive(true);
        }

        void OnTakeDamage(float dmg, GameObject damageSource)
        {
            ResetFlash();
            FlashImage.color = DamageFlashColor;
        }

        void OnHealed(float amount)
        {
            ResetFlash();
            FlashImage.color = HealFlashColor;
        }

        private void OnDestroy()
        {
            m_PlayerHealth.onDamaged -= OnTakeDamage;
            m_PlayerHealth.onHealed -= OnHealed;
        }
    }
}
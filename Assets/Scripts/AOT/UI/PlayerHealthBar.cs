using FPS.Game.Shared;
using FPS.GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.UI
{
    public sealed class PlayerHealthBar : MonoBehaviour
    {
        [Tooltip("生命值填充图")]
        public Image healthFillImage;

        private PlayerCharacterController m_Player;
        private Health m_Health;
        private float m_CurrentHealthRatio;

        private void Start()
        {
            m_Player = FindFirstObjectByType<PlayerCharacterController>();
            m_Health = m_Player.GetComponent<Health>();
        }

        private void Update()
        {
            if (m_Health != null)
            {
                m_CurrentHealthRatio = m_Health.GetRatio();
            }
            healthFillImage.fillAmount = m_CurrentHealthRatio;
        }
    }
}
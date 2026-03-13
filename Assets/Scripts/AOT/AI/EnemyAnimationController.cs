using FPS.Game.Shared;
using UnityEngine;

namespace FPS.AI
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimationController : MonoBehaviour
    {
        private Animator m_Animator;
        private EnemyController m_EnemyController;
        private Health m_Health;

        const string k_anim_attack_parameter = "Attack";
        const string k_anim_on_damaged_parameter = "OnDamaged";

        void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_EnemyController = GetComponentInParent<EnemyController>();
            m_Health = GetComponentInParent<Health>();
        }

        void Start()
        {
            // 订阅开火和受击事件
            if (m_EnemyController != null)
            {
                m_EnemyController.onAttack += PlayAttackAnimation;
            }

            if (m_Health != null)
            {
                m_Health.onDamaged += PlayDamagedAnimation;
            }
        }

        private void PlayAttackAnimation()
        {
            if (m_Animator) m_Animator.SetTrigger(k_anim_attack_parameter);
        }

        private void PlayDamagedAnimation(float amount, GameObject source)
        {
            if (m_Animator) m_Animator.SetTrigger(k_anim_on_damaged_parameter);
        }
    }
}
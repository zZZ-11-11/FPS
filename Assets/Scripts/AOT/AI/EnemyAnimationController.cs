using FPS.Game.Shared;
using UnityEngine;

namespace FPS.AI
{
    [RequireComponent(typeof(Animator))]
    public sealed class EnemyAnimationController : MonoBehaviour
    {
        private Animator m_Animator;
        private EnemyController m_EnemyController;
        private Health m_Health;
        private int m_shootLayerIndex;

        private const string k_anim_attack_parameter = "Attack";
        private const string k_anim_damaged_parameter = "Damaged";
        private const string k_anim_death_parameter = "Dead";
        private const string fire_animation_state = "Fire";

        private static readonly int s_Attack = Animator.StringToHash(k_anim_attack_parameter);
        private static readonly int s_Damaged = Animator.StringToHash(k_anim_damaged_parameter);
        private static readonly int s_Dead = Animator.StringToHash(k_anim_death_parameter);

        void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_EnemyController = GetComponentInParent<EnemyController>();
            m_Health = GetComponentInParent<Health>();
        }

        void Start()
        {
            m_shootLayerIndex = m_Animator.GetLayerIndex("Fire Layer");
            if (m_EnemyController != null)
            {
                m_EnemyController.onAttack += PlayAttackAnimation;
            }

            if (m_Health != null)
            {
                m_Health.onDamaged += PlayDamagedAnimation;
                m_Health.onDie += PlayDeadAnimation;
            }
        }

        private void PlayAttackAnimation()
        {
            if (m_Animator)
            {
                // m_Animator.SetTrigger(s_Attack);
                m_Animator.Play(fire_animation_state, m_shootLayerIndex, 0f);
            }
        }

        private void PlayDamagedAnimation(float amount, GameObject source)
        {
            if (m_Animator)
            {
                m_Animator.SetTrigger(s_Damaged);
            }
        }

        private void PlayDeadAnimation()
        {
            if (m_Animator)
            {
                m_Animator.SetBool(s_Dead, true);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Events;

namespace FPS.Game.Shared
{
    public sealed class Health : MonoBehaviour
    {
        public float maxHealth = 100f;

        public float criticalHealthRatio = 0.3f;

        public UnityAction<float, GameObject> onDamaged;
        public UnityAction<float> onHealed;
        public UnityAction onDie;

        public float currentHealth { get; set; }
        public bool invincible { get; set; }
        public bool CanBeHealed() => currentHealth < maxHealth;

        public float GetRatio() => currentHealth / maxHealth;
        public bool IsCritical() => GetRatio() <= criticalHealthRatio;

        bool m_IsDead;

        void Start()
        {
            currentHealth = maxHealth;
        }

        public void Heal(float healAmount)
        {
            var healthBefore = currentHealth;
            currentHealth += healAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            var trueHealAmount = currentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                onHealed?.Invoke(trueHealAmount);
            }
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (invincible)
            {
                return;
            }

            var healthBefore = currentHealth;
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            var trueDamageAmount = healthBefore - currentHealth;
            if (trueDamageAmount > 0f)
            {
                onDamaged?.Invoke(trueDamageAmount, damageSource);
            }

            HandleDeath();
        }

        public void Kill()
        {
            currentHealth = 0f;

            onDamaged?.Invoke(maxHealth, null);

            HandleDeath();
        }

        void HandleDeath()
        {
            if (m_IsDead)
            {
                return;
            }

            if (currentHealth > 0f)
            {
                return;
            }
            m_IsDead = true;
            onDie?.Invoke();
        }
    }
}
using UnityEngine;

namespace FPS.Game.Shared
{
    public sealed class Damageable : MonoBehaviour
    {
        [Tooltip("伤害倍率")]
        public float damageMultiplier = 1f;

        [Range(0, 1)]
        [Tooltip("自伤倍率")]
        public float sensibilityToSelfDamage = 0.5f;

        private Health health { get; set; }

        void Awake()
        {
            health = GetComponent<Health>();
            if (!health)
            {
                health = GetComponentInParent<Health>();
            }
        }

        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if (health)
            {
                var totalDamage = damage;

                //如果是范围伤害不计算倍率
                if (!isExplosionDamage)
                {
                    totalDamage *= damageMultiplier;
                }

                // 计算自伤
                if (health.gameObject == damageSource)
                {
                    totalDamage *= sensibilityToSelfDamage;
                }

                health.TakeDamage(totalDamage, damageSource);
            }
        }
    }
}
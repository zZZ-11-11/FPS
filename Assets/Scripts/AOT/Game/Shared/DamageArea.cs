using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FPS.Game.Shared
{
    public class DamageArea : MonoBehaviour
    {
        [Tooltip("伤害半径")]
        public float areaOfEffectDistance = 5f;

        [Tooltip("伤害/距离曲线")]
        public AnimationCurve damageRatioOverDistance;

        [Header("Debug")]
        [Tooltip("gizmos颜色")]
        public Color areaOfEffectColor = Color.red * 0.5f;

        public void InflictDamageInArea(float damage, Vector3 center, LayerMask layers,
            QueryTriggerInteraction interaction, GameObject owner)
        {
            //记录被伤害的物体，避免重复
            Dictionary<Health, Damageable> uniqueDamagedHealths = new Dictionary<Health, Damageable>();

            Collider[] affectedColliders = Physics.OverlapSphere(center, areaOfEffectDistance, layers, interaction);
            foreach (var coll in affectedColliders)
            {
                var damageable = coll.GetComponent<Damageable>();
                if (damageable)
                {
                    var health = damageable.GetComponentInParent<Health>();
                    if (health)
                    {
                        uniqueDamagedHealths.TryAdd(health, damageable);
                    }
                }
            }

            // 
            foreach (var uniqueDamageable in uniqueDamagedHealths.Values)
            {
                var distance = Vector3.Distance(uniqueDamageable.transform.position, transform.position);
                uniqueDamageable.InflictDamage(
                    damage * damageRatioOverDistance.Evaluate(distance / areaOfEffectDistance), true, owner);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = areaOfEffectColor;
            Gizmos.DrawSphere(transform.position, areaOfEffectDistance);
        }
    }
}
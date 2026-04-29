using UnityEngine;
using UnityEngine.Pool;

namespace FPS.GamePlay.Base
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        protected GameObject owner { get; private set; }
        protected Vector3 initialPosition { get; private set; }
        protected Vector3 inheritedMuzzleVelocity { get; private set; }
        public float initialCharge { get; private set; }

        private IObjectPool<ProjectileBase> m_Pool;

        public void SetPool(IObjectPool<ProjectileBase> pool)
        {
            m_Pool = pool;
        }

        protected void ReleaseToPool()
        {
            if (m_Pool != null)
            {
                m_Pool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public virtual void Shoot(ShootContext shootContext)
        {
            owner = shootContext.owner;
            inheritedMuzzleVelocity = shootContext.muzzleWorldVelocity;
            initialCharge = shootContext.currentCharge;

            var projectileTransform = shootContext.initialTransform;
            initialPosition = projectileTransform.position;
            transform.position = initialPosition;
            transform.rotation = Quaternion.LookRotation(shootContext.shotDirection);
        }
    }
}
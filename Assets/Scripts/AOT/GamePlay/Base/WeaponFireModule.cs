using FPS.GamePlay.Weapon;
using UnityEngine;
using UnityEngine.Pool;

namespace FPS.GamePlay.Base
{
    public abstract class WeaponFireModule : MonoBehaviour
    {
        [Tooltip("每次射击的弹道预制体")]
        public ProjectileBase projectilePrefab;

        [Tooltip("每次射击的间隔时间（秒）")]
        public float delayBetweenShots = 0.5f;

        [Tooltip("子弹散布角度")]
        public float bulletSpreadAngle;

        protected float m_LastTimeShot = Mathf.NegativeInfinity;

        public virtual bool isCharging => false;
        public abstract bool ProcessInput(bool inputDown, bool inputHeld, bool inputUp, WeaponCore weaponCore);

        public abstract float GetAmmoPerShot();

        protected ObjectPool<ProjectileBase> m_ProjectilePool;

        private Transform m_PoolContainer;

        void Start()
        {
            m_PoolContainer = new GameObject($"[{gameObject.name}] Projectile Pool").transform;

            m_ProjectilePool = new ObjectPool<ProjectileBase>(
                createFunc: CreateProjectile,
                actionOnGet: OnTakeProjectileFromPool,
                actionOnRelease: OnReturnProjectileToPool,
                actionOnDestroy: OnDestroyProjectile,
                collectionCheck: false,
                defaultCapacity: 30,
                maxSize: 100);
        }

        // 1. 创建新子弹（当池子空了的时候）
        private ProjectileBase CreateProjectile()
        {
            var projectile = Instantiate(projectilePrefab, m_PoolContainer);
            projectile.SetPool(m_ProjectilePool);
            return projectile;
        }

        // 2. 从池子取出子弹时
        private void OnTakeProjectileFromPool(ProjectileBase projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        // 3. 将子弹放回池子时
        private void OnReturnProjectileToPool(ProjectileBase projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        // 4. 超出最大容量时，直接销毁
        private void OnDestroyProjectile(ProjectileBase projectile)
        {
            Destroy(projectile.gameObject);
        }
    }
}
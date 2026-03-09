using FPS.GamePlay.Weapon.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace FPS.GamePlay.Weapon
{
    public class AutomaticFireModule : WeaponFireModule
    {
        [Tooltip("每次射击的弹道预制体")]
        public ProjectileBase projectilePrefab;

        [Tooltip("每次射击生成的子弹数量")]
        public int bulletsPerShot = 1;

        [Tooltip("每次射击消耗的弹药数量")]
        public float ammoPerShot = 1;

        [Tooltip("每次射击的弹道偏移角度")]
        public float bulletSpreadAngle = 0f;

        public override bool ProcessInput(bool inputDown, bool inputHeld, bool inputUp, WeaponCore weaponCore)
        {
            if (inputHeld)
            {
                return TryShoot(weaponCore);
            }
            return false;
        }

        private bool TryShoot(WeaponCore weaponCore)
        {
            var ammo = weaponCore.ammoModule;
            if (ammo != null && ammo.isReloading)
            {
                return false;
            }
            // 检查冷却和弹药
            if (Time.time < m_LastTimeShot + delayBetweenShots)
            {
                return false;
            }
            if (ammo != null && !ammo.HasEnoughAmmo(ammoPerShot))
            {
                return false;
            }

            // 消耗弹药
            if (ammo != null)
            {
                ammo.ConsumeAmmo(ammoPerShot);
            }

            var muzzle = weaponCore.weaponMuzzle;

            // 实际射击逻辑 (生成子弹)
            for (int i = 0; i < bulletsPerShot; i++)
            {
                var shotDirection = GetSpreadDirection(muzzle);
                var newProjectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(shotDirection));
                newProjectile.Shoot(new ShootContext(0, weaponCore.owner, weaponCore.muzzleWorldVelocity));
                weaponCore.fxModule.PlayShootFX(weaponCore.weaponMuzzle);
            }

            m_LastTimeShot = Time.time;
            return true;
        }

        private Vector3 GetSpreadDirection(Transform shootTransform)
        {
            if (bulletSpreadAngle <= 0)
            {
                return shootTransform.forward;
            }
            var spreadAngleRatio = bulletSpreadAngle / 180f;
            return Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        }
    }
}
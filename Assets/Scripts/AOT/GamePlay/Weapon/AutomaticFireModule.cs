using FPS.GamePlay.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FPS.GamePlay.Weapon
{
    public sealed class AutomaticFireModule : WeaponFireModule
    {
        [Tooltip("每次射击生成的子弹数量")]
        public int bulletsPerShot = 1;

        [Tooltip("每次射击消耗的弹药数量")]
        public float ammoPerShot = 1;

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
            for (var i = 0; i < bulletsPerShot; i++)
            {
                var shotDirection = GetSpreadDirection(muzzle);
                Transform targetTrans = null;
                if (!weaponCore.isPlayerWeapon
                    && weaponCore.cachedEnemyController != null
                    && weaponCore.cachedEnemyController.knownDetectedTarget != null)
                {
                    targetTrans = weaponCore.cachedEnemyController.knownDetectedTarget.transform;
                }

                var context = new ShootContext(
                    currentCharge: 0,
                    owner: weaponCore.owner,
                    muzzleWorldVelocity: weaponCore.muzzleWorldVelocity,
                    initialTransform: muzzle,
                    shotDirection: shotDirection,
                    ownerColliders: weaponCore.cachedOwnerColliders,
                    isPlayer: weaponCore.isPlayerWeapon,
                    weaponCameraTransform: weaponCore.cachedWeaponCameraTransform,
                    targetTransform: targetTrans
                );
                var newProjectile = m_ProjectilePool.Get();
                newProjectile.Shoot(context);
            }
            weaponCore.fxModule.PlayShootFX(weaponCore.weaponMuzzle);

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
            return Vector3.Slerp(shootTransform.forward, Random.insideUnitSphere, spreadAngleRatio);
        }

        public override float GetAmmoPerShot() => ammoPerShot;
    }
}
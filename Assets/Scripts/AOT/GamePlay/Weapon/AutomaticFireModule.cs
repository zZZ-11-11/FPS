using FPS.GamePlay.Weapon.Base;
using UnityEngine;

namespace FPS.GamePlay.Weapon
{
    public class AutomaticFireModule : WeaponFireModule
    {
        public ProjectileBase ProjectilePrefab;
        public int BulletsPerShot = 1;
        public float BulletSpreadAngle = 0f;

        public override bool ProcessInput(bool inputDown, bool inputHeld, bool inputUp, WeaponAmmoModule ammo, Transform muzzle)
        {
            if (inputHeld)
            {
                return TryShoot(ammo, muzzle);
            }
            return false;
        }

        private bool TryShoot(WeaponAmmoModule ammo, Transform muzzle)
        {
            // 检查冷却和弹药
            if (Time.time < m_LastTimeShot + DelayBetweenShots) return false;
            if (ammo != null && !ammo.HasEnoughAmmo(1f)) return false;

            // 消耗弹药
            if (ammo != null) ammo.ConsumeAmmo(1f);

            // 实际射击逻辑 (生成子弹)
            for (int i = 0; i < BulletsPerShot; i++)
            {
                Vector3 shotDirection = GetSpreadDirection(muzzle);
                ProjectileBase newProjectile = Instantiate(ProjectilePrefab, muzzle.position, Quaternion.LookRotation(shotDirection));
                // newProjectile.Shoot(weaponContext); // 需要的话可以传入引用
            }

            m_LastTimeShot = Time.time;
            return true;
        }

        private Vector3 GetSpreadDirection(Transform shootTransform)
        {
            if (BulletSpreadAngle <= 0) return shootTransform.forward;
            float spreadAngleRatio = BulletSpreadAngle / 180f;
            return Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        }
    }
}
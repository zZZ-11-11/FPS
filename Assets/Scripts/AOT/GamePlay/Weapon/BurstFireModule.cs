/*using System.Collections;
using FPS.GamePlay.Base;
using UnityEngine;

namespace FPS.GamePlay.Weapon
{
    public class BurstFireModule : WeaponFireModule
    {
        [Header("Burst Settings")]
        [Tooltip("每次点射发射的子弹数量（例如：三连发填 3）")]
        public int bulletsPerBurst = 3;

        [Tooltip("同一轮点射中，两发子弹之间的间隔时间（秒）")]
        public float delayBetweenBurstShots = 0.1f;

        [Header("Ammo Settings")]
        [Tooltip("每发子弹消耗的弹药量")]
        public float ammoPerShot = 1f;

        // 内部状态：防止玩家疯狂狂点鼠标导致协程叠加
        private bool m_IsBursting = false;

        public override bool ProcessInput(bool inputDown, bool inputHeld, bool inputUp, WeaponCore core)
        {
            if (inputDown && !m_IsBursting)
            {
                return TryStartBurst(core);
            }
            return false;
        }

        private bool TryStartBurst(WeaponCore core)
        {
            var ammo = core.ammoModule;

            // 1. 检查整体开火冷却和换弹状态
            if (Time.time < m_LastTimeShot + delayBetweenShots) return false;
            if (ammo != null && ammo.isReloading) return false;

            // 2. 检查是否至少够打第一发子弹
            if (ammo != null && !ammo.HasEnoughAmmo(ammoPerShot)) return false;

            // 3. 启动协程处理真正的点射逻辑
            StartCoroutine(BurstCoroutine(core));

            return true;
        }

        private IEnumerator BurstCoroutine(WeaponCore core)
        {
            m_IsBursting = true;
            var ammo = core.ammoModule;

            for (var i = 0; i < bulletsPerBurst; i++)
            {
                // 每次射击前再次检查弹药！
                if (ammo != null && !ammo.HasEnoughAmmo(ammoPerShot))
                {
                    break; // 弹药不足，中断点射
                }

                // 消耗弹药
                if (ammo != null)
                {
                    ammo.ConsumeAmmo(ammoPerShot);
                }

                // 生成子弹
                Vector3 shotDirection = GetSpreadDirection(core.weaponMuzzle);

                var newProjectile = m_ProjectilePool.Get();
                newProjectile.Shoot(new ShootContext(0f, core.owner, core.muzzleWorldVelocity, core.weaponMuzzle, shotDirection));

                core.fxModule.PlayShootFX(core.weaponMuzzle);

                // 如果这不是最后一发子弹，等待设定的间隔时间
                if (i < bulletsPerBurst - 1)
                {
                    yield return new WaitForSeconds(delayBetweenBurstShots);
                }
            }

            // 一轮点射彻底结束后，才重置冷却时间基准点
            m_LastTimeShot = Time.time;
            m_IsBursting = false;
        }

        private Vector3 GetSpreadDirection(Transform shootTransform)
        {
            if (bulletSpreadAngle <= 0) return shootTransform.forward;
            float spreadAngleRatio = bulletSpreadAngle / 180f;
            return Vector3.Slerp(shootTransform.forward, Random.insideUnitSphere, spreadAngleRatio);
        }

        public override float GetAmmoPerShot() => ammoPerShot;
    }
}*/


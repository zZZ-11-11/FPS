using System;
using FPS.GamePlay.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FPS.GamePlay.Weapon
{
    public class ChargeFireModule : WeaponFireModule
    {
        [Header("Projectile Settings")]
        public ProjectileBase projectilePrefab;

        [Tooltip("达到满充能时，射出的最大弹体数量（或者代表伤害倍率）")]
        public int maxBulletsPerShot = 1;

        [Header("Charge Parameters")]
        [Tooltip("充能达到最大值所需的时间（秒）")]
        public float maxChargeDuration = 2f;

        [Tooltip("充能满后是否自动开火（如果不勾选，玩家必须松开按键才开火）")]
        public bool automaticReleaseOnCharged = true;

        [Header("Ammo Consumption")]
        [Tooltip("刚按下按键开始充能时，瞬间扣除的弹药量")]
        public float ammoUsedOnStartCharge = 1f;

        [Tooltip("充能过程中，每充满 100% 进度额外消耗的弹药量")]
        public float ammoUsageRateWhileCharging = 1f;

        // 对外暴露的状态，方便 UI（准星/进度条）和表现模块（充能音效）读取
        private bool m_IsCharging;
        public override bool isCharging => m_IsCharging;
        public float currentCharge { get; private set; }

        public override bool ProcessInput(bool inputDown, bool inputHeld, bool inputUp, WeaponCore core)
        {
            bool didShoot = false;
            WeaponAmmoModule ammo = core.ammoModule;

            // 1. 如果玩家按住按键且当前未充能，尝试开始充能
            if (inputHeld && !isCharging)
            {
                TryBeginCharge(ammo);
            }

            // 2. 状态机：如果正在充能中
            if (isCharging)
            {
                // 触发开火条件：玩家松开按键，或者（勾选了自动释放 且 充能已满）
                if (inputUp || (automaticReleaseOnCharged && currentCharge >= 1f))
                {
                    didShoot = TryReleaseCharge(core);
                }
                // 否则，如果玩家仍在按住按键，继续增加充能进度
                else if (inputHeld)
                {
                    UpdateCharge(ammo);
                }
            }

            return didShoot;
        }

        private void TryBeginCharge(WeaponAmmoModule ammo)
        {
            // 检查开火冷却和换弹状态
            if (Time.time < m_LastTimeShot + delayBetweenShots) return;
            if (ammo != null && ammo.isReloading) return;

            // 检查是否有足够的弹药来“启动”充能
            if (ammo == null || ammo.HasEnoughAmmo(ammoUsedOnStartCharge))
            {
                if (ammo != null) ammo.ConsumeAmmo(ammoUsedOnStartCharge);

                // 进入充能状态
                m_IsCharging = true;
                currentCharge = 0f;
            }
        }

        private void UpdateCharge(WeaponAmmoModule ammo)
        {
            if (currentCharge < 1f)
            {
                // 计算这一帧理论上可以增加的充能比例
                float chargeAdded = (maxChargeDuration > 0f) ? (Time.deltaTime / maxChargeDuration) : 1f;
                float chargeLeft = 1f - currentCharge;
                chargeAdded = Mathf.Min(chargeAdded, chargeLeft); // 防止充能溢出 1.0

                // 换算成这一帧需要消耗的弹药量
                float ammoRequired = chargeAdded * ammoUsageRateWhileCharging;

                // 只有当弹药依然充足时，才允许继续增加充能进度
                if (ammo == null || ammo.HasEnoughAmmo(ammoRequired))
                {
                    if (ammo != null) ammo.ConsumeAmmo(ammoRequired);
                    currentCharge += chargeAdded;
                }
                else
                {
                    // 如果充能中途弹药耗尽（比如被动回复的能量被抽干了）：
                    // 充能进度会停滞在当前值，直到玩家松开按键释放，或者等能量恢复
                }
            }
        }

        private bool TryReleaseCharge(WeaponCore core)
        {
            // 计算最终要发射的子弹数量（基于充能比例）
            int bulletsToShoot = Mathf.CeilToInt(currentCharge * maxBulletsPerShot);

            // 即使充能比例很低，只要开启了充能，至少打出 1 发
            bulletsToShoot = Mathf.Max(1, bulletsToShoot);

            // 生成子弹
            for (int i = 0; i < bulletsToShoot; i++)
            {
                Vector3 shotDirection = GetSpreadDirection(core.weaponMuzzle);

                ProjectileBase newProjectile = Instantiate(projectilePrefab, core.weaponMuzzle.position, Quaternion.LookRotation(shotDirection));

                newProjectile.Shoot(new ShootContext(currentCharge, core.owner, core.muzzleWorldVelocity));

                core.fxModule.PlayShootFX(core.weaponMuzzle);
            }

            // 重置状态
            m_LastTimeShot = Time.time;
            m_IsCharging = false;
            currentCharge = 0f;

            return true;
        }

        private Vector3 GetSpreadDirection(Transform shootTransform)
        {
            if (bulletSpreadAngle <= 0) return shootTransform.forward;
            float spreadAngleRatio = bulletSpreadAngle / 180f;
            return Vector3.Slerp(shootTransform.forward, Random.insideUnitSphere, spreadAngleRatio);
        }

        public override float GetAmmoPerShot() => -1;
    }
}
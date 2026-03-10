using System;
using FPS.GamePlay.Base;
using UnityEngine;

namespace FPS.GamePlay.Weapon
{
    public class EnergyAmmoModule : WeaponAmmoModule
    {
        [Tooltip("武器的最大能量值")]
        public float maxEnergy = 100f;

        [Tooltip("每秒自动恢复的能量数值")]
        public float energyRegenRate = 20f;

        [Tooltip("停止消耗后，等待多久才开始恢复能量（秒）")]
        public float regenDelay = 1f;

        private float m_CurrentEnergy;
        private float m_LastConsumeTime;

        public float currentEnergy => m_CurrentEnergy;

        void Awake()
        {
            m_CurrentEnergy = maxEnergy;

            m_LastConsumeTime = -regenDelay;

            autoRegenerate = true;
        }

        public override bool HasEnoughAmmo(float amountNeeded) => m_CurrentEnergy >= amountNeeded;

        public override void ConsumeAmmo(float amount)
        {
            m_CurrentEnergy -= amount;

            // 防止异常情况扣成负数
            m_CurrentEnergy = Mathf.Max(0f, m_CurrentEnergy);

            // 刷新最后一次消耗的时间，打断能量恢复
            m_LastConsumeTime = Time.time;
        }

        public override float GetCurrentAmmoRatio() => m_CurrentEnergy / maxEnergy;

        public override void StartReload()
        {
        }

        void Update()
        {
            if (m_CurrentEnergy < maxEnergy && Time.time >= m_LastConsumeTime + regenDelay && autoRegenerate)
            {
                // 随时间恢复能量
                m_CurrentEnergy += energyRegenRate * Time.deltaTime;

                // 防止能量溢出上限
                m_CurrentEnergy = Mathf.Min(m_CurrentEnergy, maxEnergy);
            }
        }
    }
}
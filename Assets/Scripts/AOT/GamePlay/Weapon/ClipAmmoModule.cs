using FPS.GamePlay.Weapon.Base;
using UnityEngine;

namespace FPS.GamePlay.Weapon
{
    public class ClipAmmoModule : WeaponAmmoModule
    {
        public int ClipSize = 30;
        public int MaxBackupAmmo = 120;
        public bool AutomaticReload = true;

        private float m_CurrentClipAmmo;
        private int m_CurrentBackupAmmo;

        void Awake()
        {
            m_CurrentClipAmmo = ClipSize;
            m_CurrentBackupAmmo = MaxBackupAmmo;
        }

        public override bool HasEnoughAmmo(float amountNeeded)
        {
            return m_CurrentClipAmmo >= amountNeeded;
        }

        public override void ConsumeAmmo(float amount)
        {
            m_CurrentClipAmmo -= amount;
            if (m_CurrentClipAmmo <= 0 && AutomaticReload)
            {
                Reload();
            }
        }

        public override float GetCurrentAmmoRatio()
        {
            return m_CurrentClipAmmo / ClipSize;
        }

        public void Reload()
        {
            // 简单的换弹逻辑，可根据需要扩展
            float needed = ClipSize - m_CurrentClipAmmo;
            float available = Mathf.Min(needed, m_CurrentBackupAmmo);
            m_CurrentClipAmmo += available;
            m_CurrentBackupAmmo -= (int) available;
        }
    }
}
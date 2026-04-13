using System.Collections;
using FPS.GamePlay.Base;
using UnityEngine;

namespace FPS.GamePlay.Weapon
{
    public sealed class ClipAmmoModule : WeaponAmmoModule
    {
        public int clipSize = 30;
        public int maxBackupAmmo = 120;
        public float reloadTime = 2f;
        public bool autoReloadOnEmpty = true;

        private int m_CurrentClipAmmo;
        private int m_CurrentBackupAmmo;

        private bool m_IsReloading;

        public override bool isReloading => m_IsReloading;

        void Awake()
        {
            m_CurrentClipAmmo = clipSize;
            m_CurrentBackupAmmo = maxBackupAmmo;
            autoRegenerate = false;
        }

        public override bool HasEnoughAmmo(float amountNeeded)
        {
            var intAmount = Mathf.RoundToInt(amountNeeded);
            return m_CurrentClipAmmo >= intAmount;
        }

        public override void ConsumeAmmo(float amount)
        {
            if (m_IsReloading)
            {
                return;
            }

            var intAmount = Mathf.RoundToInt(amount);
            m_CurrentClipAmmo -= intAmount;

            // 防止异常情况扣成负数
            m_CurrentClipAmmo = Mathf.Max(0, m_CurrentClipAmmo);

            InvokeAmmoChanged();

            if (m_CurrentClipAmmo <= 0 && autoReloadOnEmpty)
            {
                StartReload();
            }
        }

        public override void PickUpAmmo()
        {
            m_CurrentClipAmmo = clipSize;
            m_CurrentBackupAmmo = Mathf.Min(maxBackupAmmo, m_CurrentBackupAmmo + clipSize);
            InvokeAmmoChanged();
        }

        public override void StartReload()
        {
            if (m_IsReloading || m_CurrentClipAmmo == clipSize || m_CurrentBackupAmmo <= 0)
            {
                return;
            }

            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            m_IsReloading = true;

            // TODO: 这里可以通过事件通知 FXModule 播放换弹动画/音效

            yield return new WaitForSeconds(reloadTime); // 等待换弹时间

            // 结算弹药数学逻辑
            var needed = clipSize - m_CurrentClipAmmo;
            var available = Mathf.Min(needed, m_CurrentBackupAmmo);
            var lastClipAmmo = m_CurrentClipAmmo;
            var lastBackupAmmo = m_CurrentBackupAmmo;
            m_CurrentClipAmmo += available;
            m_CurrentBackupAmmo -= available;
            if (lastClipAmmo != m_CurrentClipAmmo || lastBackupAmmo != m_CurrentBackupAmmo)
            {
                InvokeAmmoChanged();
            }
            m_IsReloading = false;
        }

        public override float GetCurrentAmmoRatio() => (float) m_CurrentClipAmmo / clipSize;

        public override float GetCurrentAmmo() => m_CurrentClipAmmo;
        public override float GetMaxCapacity() => clipSize;
        public override float GetBackupAmmo() => m_CurrentBackupAmmo;

        public override bool IsAmmoEmpty() => m_CurrentClipAmmo <= 0;
        public override bool HasBackupAmmo() => m_CurrentBackupAmmo > 0;
        public override bool IsPercentageBased() => false;
    }
}
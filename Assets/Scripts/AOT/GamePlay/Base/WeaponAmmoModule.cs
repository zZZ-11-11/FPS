using System;
using UnityEngine;

namespace FPS.GamePlay.Base
{
    public abstract class WeaponAmmoModule : MonoBehaviour
    {
        public abstract float GetCurrentAmmo();
        public abstract float GetMaxCapacity();
        public abstract float GetBackupAmmo();

        public abstract bool IsPercentageBased();

        public abstract bool IsAmmoEmpty();
        public abstract bool HasBackupAmmo();

        public virtual bool isReloading => false;
        public abstract bool HasEnoughAmmo(float amountNeeded);
        public abstract void ConsumeAmmo(float amount);
        public abstract float GetCurrentAmmoRatio();

        public bool autoRegenerate;
        public abstract void StartReload();

        public event Action onAmmoChanged;

        protected void InvokeAmmoChanged()
        {
            onAmmoChanged?.Invoke();
        }

        public abstract void PickUpAmmo();
    }
}
using UnityEngine;

namespace FPS.GamePlay.Base
{
    public abstract class WeaponAmmoModule : MonoBehaviour
    {
        public virtual bool isReloading => false;
        public abstract bool HasEnoughAmmo(float amountNeeded);
        public abstract void ConsumeAmmo(float amount);
        public abstract float GetCurrentAmmoRatio();

        public bool autoRegenerate;
        public abstract void StartReload();

        public abstract float GetCurrentAmmo();

        public abstract float GetBackUpAmmo();
    }
}
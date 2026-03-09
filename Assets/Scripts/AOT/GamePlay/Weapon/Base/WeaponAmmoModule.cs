using UnityEngine;
using UnityEngine.Serialization;

namespace FPS.GamePlay.Weapon.Base
{
    public abstract class WeaponAmmoModule : MonoBehaviour
    {
        public virtual bool isReloading => false;
        public abstract bool HasEnoughAmmo(float amountNeeded);
        public abstract void ConsumeAmmo(float amount);
        public abstract float GetCurrentAmmoRatio();

        public bool autoRegenerate = false;
        public abstract void StartReload();
    }
}
using UnityEngine;

namespace FPS.GamePlay.Weapon.Base
{
    public abstract class WeaponAmmoModule : MonoBehaviour
    {
        public abstract bool HasEnoughAmmo(float amountNeeded);
        public abstract void ConsumeAmmo(float amount);
        public abstract float GetCurrentAmmoRatio();
    }
}
using FPS.GamePlay.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

namespace FPS.GamePlay.Base
{
    public abstract class WeaponFireModule : MonoBehaviour
    {
        [Tooltip("每次射击的间隔时间（秒）")]
        public float delayBetweenShots = 0.5f;

        [Tooltip("子弹散布角度")]
        public float bulletSpreadAngle = 0f;

        protected float m_LastTimeShot = Mathf.NegativeInfinity;

        public virtual bool isCharging => false;
        public abstract bool ProcessInput(bool inputDown, bool inputHeld, bool inputUp, WeaponCore weaponCore);

        public abstract float GetAmmoPerShot();
    }
}
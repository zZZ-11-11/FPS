using UnityEngine;
using UnityEngine.Serialization;

namespace FPS.GamePlay.Weapon.Base
{
    public abstract class WeaponFireModule : MonoBehaviour
    {
        public float delayBetweenShots = 0.5f;
        protected float m_LastTimeShot = Mathf.NegativeInfinity;

        public virtual bool isCharging => false;
        public abstract bool ProcessInput(bool inputDown, bool inputHeld, bool inputUp, WeaponCore weaponCore);
    }
}
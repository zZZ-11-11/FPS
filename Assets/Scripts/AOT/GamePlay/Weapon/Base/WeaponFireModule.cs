using UnityEngine;

namespace FPS.GamePlay.Weapon.Base
{
    public abstract class WeaponFireModule : MonoBehaviour
    {
        public float DelayBetweenShots = 0.5f;
        protected float m_LastTimeShot = Mathf.NegativeInfinity;

        // 核心处理逻辑，返回 true 表示成功开火
        public abstract bool ProcessInput(bool inputDown, bool inputHeld, bool inputUp, WeaponAmmoModule ammo, Transform muzzle);
    }
}
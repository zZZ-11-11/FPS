using FPS.GamePlay.Weapon.Base;
using UnityEngine;
using UnityEngine.Events;

namespace FPS.GamePlay.Weapon
{
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
    }

    [System.Serializable]
    public struct CrosshairData
    {
        [Tooltip("The image that will be used for this weapon's crosshair")]
        public Sprite CrosshairSprite;

        [Tooltip("The size of the crosshair image")]
        public int CrosshairSize;

        [Tooltip("The color of the crosshair image")]
        public Color CrosshairColor;
    }

    public class WeaponCore : MonoBehaviour
    {
        [Header("Information")]
        public string WeaponName;

        public Sprite WeaponIcon;
        public Transform WeaponMuzzle;
        public CrosshairData CrosshairDataDefault;

        [Header("Modules")]
        public WeaponFireModule FireModule;

        public WeaponAmmoModule AmmoModule;
        public WeaponFXModule FXModule;

        public UnityAction OnShoot;

        void Awake()
        {
            // 自动获取挂载在同一个 GameObject 上的模块
            if (FireModule == null) FireModule = GetComponent<WeaponFireModule>();
            if (AmmoModule == null) AmmoModule = GetComponent<WeaponAmmoModule>();
            if (FXModule == null) FXModule = GetComponent<WeaponFXModule>();
        }

        // 外部（如玩家控制器）调用此方法传入输入状态
        public void HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            if (FireModule == null) return;

            // 让开火模块决定是否应该射击，并传入弹药模块供其检查和消耗
            bool didShoot = FireModule.ProcessInput(inputDown, inputHeld, inputUp, AmmoModule, WeaponMuzzle);

            if (didShoot)
            {
                if (FXModule != null) FXModule.PlayShootFX(WeaponMuzzle);
                OnShoot?.Invoke();
            }
        }
    }
}
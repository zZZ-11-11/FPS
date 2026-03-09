using FPS.GamePlay.Weapon.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace FPS.GamePlay.Weapon
{
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
    }

    public struct CrosshairData
    {
        public Sprite crosshairSprite;

        public int crosshairSize;

        public Color crosshairColor;
    }

    public class WeaponCore : MonoBehaviour
    {
        [Header("Information")]
        public string weaponName;

        public Sprite weaponIcon;

        public Transform weaponMuzzle;

        public float recoilForce = 1;

        public CrosshairData crosshairDataDefault;

        public GameObject sourcePrefab;

        public Vector3 aimOffset;

        public float aimZoomRatio;

        public GameObject weaponRoot;

        [Header("Modules")]
        public WeaponFireModule fireModule;

        public WeaponAmmoModule ammoModule;

        public WeaponFXModule fxModule;

        public UnityAction onShoot;
        public GameObject owner { get; private set; }

        private Vector3 m_LastMuzzlePosition;
        public Vector3 muzzleWorldVelocity { get; private set; }

        public bool isWeaponActive { get; private set; }

        void Awake()
        {
            if (fireModule == null)
            {
                fireModule = GetComponent<WeaponFireModule>();
            }
            if (ammoModule == null)
            {
                ammoModule = GetComponent<WeaponAmmoModule>();
            }
            if (fxModule == null)
            {
                fxModule = GetComponent<WeaponFXModule>();
            }
        }

        public void SetOwner(GameObject newOwner)
        {
            owner = newOwner;
        }

        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            if (fireModule == null) return false;

            // 让开火模块决定是否应该射击，并传入弹药模块供其检查和消耗
            var didShoot = fireModule.ProcessInput(inputDown, inputHeld, inputUp, this);

            if (didShoot)
            {
                onShoot?.Invoke();
                return true;
            }
            return false;
        }

        private void Update()
        {
            if (Time.deltaTime > 0)
            {
                var position = weaponMuzzle.position;
                muzzleWorldVelocity = (position - m_LastMuzzlePosition) / Time.deltaTime;
                m_LastMuzzlePosition = position;
            }
        }

        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            if (show)
            {
                fxModule.PlayChangeWeaponSfx();
            }

            isWeaponActive = show;
        }
    }
}
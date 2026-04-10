using FPS.Game;
using FPS.Game.Managers;
using FPS.GamePlay;
using FPS.GamePlay.Weapon;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FPS.UI
{
    [RequireComponent(typeof(FillBarColorChange))]
    public sealed class AmmoCounter : MonoBehaviour
    {
        [Tooltip("CanvasGroup to fade the ammo UI")]
        public CanvasGroup canvasGroup;

        [FormerlySerializedAs("WeaponImage")]
        [Tooltip("Image for the weapon icon")]
        public Image weaponImage;

        [FormerlySerializedAs("AmmoBackgroundImage")]
        [Tooltip("Image component for the background")]
        public Image ammoBackgroundImage;

        [FormerlySerializedAs("AmmoFillImage")]
        [Tooltip("Image component to display fill ratio")]
        public Image ammoFillImage;

        [FormerlySerializedAs("WeaponIndexText")]
        [Tooltip("Text for Weapon index")]
        public TextMeshProUGUI weaponIndexText;

        [FormerlySerializedAs("BulletCounter")]
        [Tooltip("Text for Bullet Counter")]
        public TextMeshProUGUI bulletCounter;

        [FormerlySerializedAs("Reload")]
        [Tooltip("Reload Text for Weapons with physical bullets")]
        public RectTransform reload;

        [FormerlySerializedAs("UnselectedOpacity")]
        [Header("Selection")]
        [Range(0, 1)]
        [Tooltip("Opacity when weapon not selected")]
        public float unselectedOpacity = 0.5f;

        [FormerlySerializedAs("UnselectedScale")]
        [Tooltip("Scale when weapon not selected")]
        public Vector3 unselectedScale = Vector3.one * 0.8f;

        [FormerlySerializedAs("ControlKeysRoot")]
        [Tooltip("Root for the control keys")]
        public GameObject controlKeysRoot;

        [FormerlySerializedAs("FillBarColorChange")]
        [Header("Feedback")]
        [Tooltip("Component to animate the color when empty or full")]
        public FillBarColorChange fillBarColorChange;

        [FormerlySerializedAs("AmmoFillMovementSharpness")]
        [Tooltip("Sharpness for the fill ratio movements")]
        public float ammoFillMovementSharpness = 20f;

        public int weaponCounterIndex { get; set; }

        WeaponManager m_WeaponManager;
        WeaponCore m_Weapon;

        void Awake()
        {
            EventManager.AddListener<AmmoPickupEvent>(OnAmmoPickup);
        }

        void OnAmmoPickup(AmmoPickupEvent evt)
        {
            if (evt.weapon == m_Weapon)
            {
                bulletCounter.text = m_Weapon.ammoModule.GetCurrentAmmo().ToString();
            }
        }

        public void Initialize(WeaponCore weapon, int weaponIndex)
        {
            m_Weapon = weapon;
            weaponCounterIndex = weaponIndex;
            weaponImage.sprite = weapon.weaponIcon;
            if (Mathf.Approximately(weapon.ammoModule.GetBackUpAmmo(), -1))
            {
                bulletCounter.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                bulletCounter.text = weapon.ammoModule.GetCurrentAmmo().ToString();
            }

            reload.gameObject.SetActive(false);
            m_WeaponManager = FindFirstObjectByType<WeaponManager>();
            DebugUtility.HandleErrorIfNullFindObject<WeaponManager, AmmoCounter>(m_WeaponManager, this);

            weaponIndexText.text = (weaponCounterIndex + 1).ToString();

            var ammoPerShot = m_Weapon.fireModule.GetAmmoPerShot();
            if (!Mathf.Approximately(ammoPerShot, -1))
            {
                fillBarColorChange.Initialize(1f, m_Weapon.fireModule.GetAmmoPerShot());
            }
        }

        void Update()
        {
            var currenFillRatio = m_Weapon.ammoModule.GetCurrentAmmoRatio();
            ammoFillImage.fillAmount = Mathf.Lerp(ammoFillImage.fillAmount, currenFillRatio,
                Time.deltaTime * ammoFillMovementSharpness);

            bulletCounter.text = m_Weapon.ammoModule.GetCurrentAmmo().ToString();

            var isActiveWeapon = m_Weapon == m_WeaponManager.GetActiveWeapon();

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, isActiveWeapon ? 1f : unselectedOpacity,
                Time.deltaTime * 10);
            transform.localScale = Vector3.Lerp(transform.localScale, isActiveWeapon ? Vector3.one : unselectedScale,
                Time.deltaTime * 10);
            controlKeysRoot.SetActive(!isActiveWeapon);

            fillBarColorChange.UpdateVisual(currenFillRatio);

            reload.gameObject.SetActive(m_Weapon.ammoModule.GetBackUpAmmo() > 0 && m_Weapon.ammoModule.GetCurrentAmmo() == 0 && m_Weapon.isWeaponActive);
        }

        void Destroy()
        {
            EventManager.RemoveListener<AmmoPickupEvent>(OnAmmoPickup);
        }
    }
}
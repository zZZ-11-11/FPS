using FPS.Game;
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
        [Tooltip("弹药UI画布")]
        public CanvasGroup canvasGroup;

        [Tooltip("武器图片")]
        public Image weaponImage;

        [Tooltip("弹药背景图")]
        public Image ammoBackgroundImage;

        [Tooltip("弹药填充图")]
        public Image ammoFillImage;

        [Tooltip("武器序号文本")]
        public TextMeshProUGUI weaponIndexText;

        [Tooltip("弹药计数器文本")]
        public TextMeshProUGUI ammoCounter;

        [FormerlySerializedAs("backUpBulletCounter")]
        [Tooltip("备弹计数器文本")]
        public TextMeshProUGUI backUpAmmoCounter;

        [Tooltip("换弹文本")]
        public RectTransform reload;

        [Header("Selection")]
        [Range(0, 1)]
        [Tooltip("武器未选择时的不透明度")]
        public float unselectedOpacity = 0.5f;

        [Tooltip("武器未选择时的缩放系数")]
        public Vector3 unselectedScale = Vector3.one * 0.8f;

        [Tooltip("武器序号根物体")]
        public GameObject controlKeysRoot;

        [Header("Feedback")]
        [Tooltip("弹药量颜色变化反馈")]
        public FillBarColorChange fillBarColorChange;

        [Tooltip("填充动画速度")]
        public float ammoFillMovementSharpness = 20f;

        public int weaponCounterIndex { get; set; }

        private WeaponManager m_WeaponManager;
        private WeaponCore m_Weapon;

        /// <summary>
        /// 初始化绑定的武器，下标，图标，弹药量，弹药量表，备弹量
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="weaponIndex"></param>
        public void Initialize(WeaponCore weapon, int weaponIndex)
        {
            m_Weapon = weapon;
            weaponCounterIndex = weaponIndex;
            weaponImage.sprite = weapon.weaponIcon;

            RefreshAmmoText();

            m_WeaponManager = FindFirstObjectByType<WeaponManager>();
            DebugUtility.HandleErrorIfNullFindObject<WeaponManager, AmmoCounter>(m_WeaponManager, this);

            weaponIndexText.text = (weaponCounterIndex + 1).ToString();
            fillBarColorChange.Initialize(1f, m_Weapon.fireModule.GetAmmoPerShot() / m_Weapon.ammoModule.GetMaxCapacity());
        }

        private void Start()
        {
            if (m_Weapon != null && m_Weapon.ammoModule != null)
            {
                m_Weapon.ammoModule.onAmmoChanged += RefreshAmmoText;
            }
        }

        /// <summary>
        /// 更新弹药量表，弹药数量，根据是否当前激活的武器更新透明度和缩放，隐藏部分信息
        /// </summary>
        void Update()
        {
            // 更新弹药量表
            var currenFillRatio = m_Weapon.ammoModule.GetCurrentAmmoRatio();
            ammoFillImage.fillAmount = currenFillRatio;
            fillBarColorChange.UpdateVisual(currenFillRatio);

            //根据是否当前激活的武器更新透明度和缩放
            var isActiveWeapon = m_Weapon == m_WeaponManager.GetActiveWeapon();

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, isActiveWeapon ? 1f : unselectedOpacity,
                Time.deltaTime * 10);
            transform.localScale = Vector3.Lerp(transform.localScale, isActiveWeapon ? Vector3.one : unselectedScale,
                Time.deltaTime * 10);
            controlKeysRoot.SetActive(!isActiveWeapon);

            reload.gameObject.SetActive(
                m_Weapon.ammoModule.HasBackupAmmo() &&
                m_Weapon.ammoModule.IsAmmoEmpty() &&
                m_Weapon.isWeaponActive
            );
        }

        /// <summary>
        ///  更新弹药与备用弹药UI
        /// </summary>
        private void RefreshAmmoText()
        {
            var ammoModule = m_Weapon.ammoModule;

            var currentAmmo = ammoModule.GetCurrentAmmo();
            var currentBackup = ammoModule.GetBackupAmmo();

            // 格式化当前弹药
            if (ammoModule.IsPercentageBased())
            {
                var percent = Mathf.RoundToInt(currentAmmo * 100f);
                ammoCounter.text = $"{percent}%";
            }
            else
            {
                ammoCounter.text = currentAmmo.ToString("0");
            }

            // 格式化备用弹药
            if (ammoModule.HasBackupAmmo())
            {
                backUpAmmoCounter.transform.parent.gameObject.SetActive(true);
                backUpAmmoCounter.text = currentBackup.ToString("0");
            }
            else
            {
                backUpAmmoCounter.transform.parent.gameObject.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if (m_Weapon != null && m_Weapon.ammoModule != null)
            {
                m_Weapon.ammoModule.onAmmoChanged -= RefreshAmmoText;
            }
        }
    }
}
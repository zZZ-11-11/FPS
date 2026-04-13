using System.Collections.Generic;
using System.Linq;
using FPS.Game;
using FPS.GamePlay;
using FPS.GamePlay.Weapon;
using UnityEngine;

namespace FPS.UI.Manager
{
    public sealed class WeaponHUDManager : MonoBehaviour
    {
        [Tooltip("弹药布局容器")]
        public RectTransform ammoPanel;

        [Tooltip("弹药UI预制体")]
        public GameObject ammoCounterPrefab;

        private WeaponManager m_WeaponManager;
        private readonly List<AmmoCounter> m_AmmoCounters = new List<AmmoCounter>();

        void Start()
        {
            m_WeaponManager = FindFirstObjectByType<WeaponManager>();
            DebugUtility.HandleErrorIfNullFindObject<WeaponManager, WeaponHUDManager>(m_WeaponManager, this);

            m_WeaponManager.OnAddedWeapon += AddWeapon;
            m_WeaponManager.OnRemovedWeapon += RemoveWeapon;
            m_WeaponManager.OnSwitchedToWeapon += ChangeWeapon;
            // 手动同步当前已经挂载在身上的所有武器，防止start顺序问题
            for (var i = 0; i < m_WeaponManager.maxWeaponSlots; i++)
            {
                var existingWeapon = m_WeaponManager.GetWeaponAtSlotIndex(i);
                if (existingWeapon != null)
                {
                    AddWeapon(existingWeapon, i);
                }
            }

            var activeWeapon = m_WeaponManager.GetActiveWeapon();
            if (activeWeapon)
            {
                ChangeWeapon(activeWeapon);
            }
        }

        void AddWeapon(WeaponCore newWeapon, int weaponIndex)
        {
            // 检查是否已经为这把武器生成过 UI 了，已经存在，直接忽略
            if (m_AmmoCounters.Any(t => t.weaponCounterIndex == weaponIndex))
            {
                return;
            }
            var ammoCounterInstance = Instantiate(ammoCounterPrefab, ammoPanel);
            var newAmmoCounter = ammoCounterInstance.GetComponent<AmmoCounter>();
            DebugUtility.HandleErrorIfNullGetComponent<AmmoCounter, WeaponHUDManager>(newAmmoCounter, this,
                ammoCounterInstance.gameObject);

            newAmmoCounter.Initialize(newWeapon, weaponIndex);

            m_AmmoCounters.Add(newAmmoCounter);
        }

        void RemoveWeapon(WeaponCore newWeapon, int weaponIndex)
        {
            var foundCounterIndex = -1;
            for (var i = 0; i < m_AmmoCounters.Count; i++)
            {
                if (m_AmmoCounters[i].weaponCounterIndex == weaponIndex)
                {
                    foundCounterIndex = i;
                    Destroy(m_AmmoCounters[i].gameObject);
                }
            }

            if (foundCounterIndex >= 0)
            {
                m_AmmoCounters.RemoveAt(foundCounterIndex);
            }
        }

        /// <summary>
        /// 强制 Unity 在当前帧立刻重新计算 ammoPanel 的布局
        /// </summary>
        /// <param name="weapon"></param>
        void ChangeWeapon(WeaponCore weapon)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ammoPanel);
        }

        private void OnDestroy()
        {
            m_WeaponManager.OnAddedWeapon -= AddWeapon;
            m_WeaponManager.OnRemovedWeapon -= RemoveWeapon;
            m_WeaponManager.OnSwitchedToWeapon -= ChangeWeapon;
        }
    }
}
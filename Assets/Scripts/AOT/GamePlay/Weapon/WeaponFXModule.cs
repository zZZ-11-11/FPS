using UnityEngine;

namespace FPS.GamePlay.Weapon
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class WeaponFXModule : MonoBehaviour
    {
        private static readonly int s_Attack = Animator.StringToHash("Attack");

        [Header("Audio")]
        public AudioClip shootSfx;

        public AudioClip changeWeaponSfx;

        private AudioSource m_AudioSource;

        [Header("Visuals")]
        public GameObject muzzleFlashPrefab;

        public Animator weaponAnimator;

        // 如果需要抛壳，可以在这里继续扩展 PhysicalShell 相关的对象池逻辑

        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        public void PlayShootFX(Transform muzzle)
        {
            if (shootSfx != null)
            {
                m_AudioSource.PlayOneShot(shootSfx);
            }

            if (weaponAnimator != null)
            {
                weaponAnimator.SetTrigger(s_Attack);
            }

            if (muzzleFlashPrefab != null && muzzle != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation);
                Destroy(flash, 2f);
            }
        }

        public void PlayChangeWeaponFX()
        {
            if (changeWeaponSfx != null)
            {
                m_AudioSource.PlayOneShot(changeWeaponSfx);
            }
        }
    }
}
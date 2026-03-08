using UnityEngine;

namespace FPS.GamePlay.Weapon
{
    [RequireComponent(typeof(AudioSource))]
    public class WeaponFXModule : MonoBehaviour
    {
        private static readonly int s_Attack = Animator.StringToHash("Attack");

        [Header("Audio")]
        public AudioClip ShootSfx;

        private AudioSource m_AudioSource;

        [Header("Visuals")]
        public GameObject MuzzleFlashPrefab;

        public Animator WeaponAnimator;

        // 如果需要抛壳，可以在这里继续扩展 PhysicalShell 相关的对象池逻辑

        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        public void PlayShootFX(Transform muzzle)
        {
            if (ShootSfx != null)
            {
                m_AudioSource.PlayOneShot(ShootSfx);
            }

            if (WeaponAnimator != null)
            {
                WeaponAnimator.SetTrigger(s_Attack);
            }

            if (MuzzleFlashPrefab != null && muzzle != null)
            {
                GameObject flash = Instantiate(MuzzleFlashPrefab, muzzle.position, muzzle.rotation);
                Destroy(flash, 2f);
            }
        }
    }
}
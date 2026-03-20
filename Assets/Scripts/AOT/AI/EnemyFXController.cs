using System.Collections.Generic;
using FPS.Game;
using FPS.Game.Shared;
using UnityEngine;

namespace FPS.AI
{
    [RequireComponent(typeof(EnemyController), typeof(Health))]
    public sealed class EnemyFXController : MonoBehaviour
    {
        [System.Serializable]
        public struct RendererIndexData
        {
            public Renderer renderer;

            public int materialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                this.renderer = renderer;
                materialIndex = index;
            }
        }

        [Header("Eye color")]
        public Material eyeColorMaterial;

        [ColorUsage(true, true)]
        public Color defaultEyeColor;

        [ColorUsage(true, true)]
        public Color attackEyeColor;

        [Header("Flash on hit")]
        public Material bodyMaterial;

        [GradientUsage(true)]
        public Gradient onHitBodyGradient;

        public float flashOnHitDuration = 0.5f;

        [Header("VFX")]
        public GameObject deathVfx;

        public Transform deathVfxSpawnPoint;

        [Header("SFX")]
        public AudioClip damageTick;

        private EnemyController m_Enemy;
        private Health m_Health;

        private readonly List<RendererIndexData> m_BodyRenderers = new List<RendererIndexData>();
        private MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;
        private float m_LastTimeDamaged = float.NegativeInfinity;

        private RendererIndexData m_EyeRendererData;
        private MaterialPropertyBlock m_EyeColorMaterialPropertyBlock;

        void Start()
        {
            m_Enemy = GetComponent<EnemyController>();
            m_Health = GetComponent<Health>();

            m_Enemy.onDetectedTarget += SetAttackEyeColor;
            m_Enemy.onLostTarget += SetDefaultEyeColor;
            m_Health.onDamaged += OnDamaged;
            m_Health.onDie += OnDie;

            InitializeRenderers();
        }

        void InitializeRenderers()
        {
            m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

            foreach (var renderer1 in GetComponentsInChildren<Renderer>(true))
            {
                for (var i = 0; i < renderer1.sharedMaterials.Length; i++)
                {
                    if (renderer1.sharedMaterials[i] == eyeColorMaterial)
                    {
                        m_EyeRendererData = new RendererIndexData(renderer1, i);
                    }

                    if (renderer1.sharedMaterials[i] == bodyMaterial)
                    {
                        m_BodyRenderers.Add(new RendererIndexData(renderer1, i));
                    }
                }
            }

            if (m_EyeRendererData.renderer != null)
            {
                m_EyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                SetDefaultEyeColor();
            }
        }

        void Update()
        {
            var currentColor = onHitBodyGradient.Evaluate((Time.time - m_LastTimeDamaged) / flashOnHitDuration);
            m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in m_BodyRenderers)
            {
                data.renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, data.materialIndex);
            }
            m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            m_EyeRendererData.renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.materialIndex);
        }

        void OnDamaged(float damage, GameObject source) => m_LastTimeDamaged = Time.time;

        void OnDie()
        {
            if (deathVfx != null && deathVfxSpawnPoint != null)
            {
                var vfx = Instantiate(deathVfx, deathVfxSpawnPoint.position, Quaternion.identity);
                Destroy(vfx, 5f);
            }
        }

        void SetDefaultEyeColor()
        {
            if (m_EyeRendererData.renderer != null)
            {
                m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                m_EyeRendererData.renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.materialIndex);
            }
        }

        void SetAttackEyeColor()
        {
            if (m_EyeRendererData.renderer != null)
            {
                m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", attackEyeColor);
                m_EyeRendererData.renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.materialIndex);
            }
        }

        public void PlayDamageTick()
        {
            if (damageTick != null)
            {
                AudioUtility.CreateSfx(damageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);
            }
        }
    }
}
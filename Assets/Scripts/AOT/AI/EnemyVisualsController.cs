using FPS.Game.Shared;

namespace FPS.AI
{
    using System.Collections.Generic;
    using UnityEngine;

    namespace Unity.FPS.AI
    {
        [RequireComponent(typeof(EnemyController), typeof(Health))]
        public class EnemyVisualsController : MonoBehaviour
        {
            [System.Serializable]
            public struct RendererIndexData
            {
                public Renderer Renderer;
                public int MaterialIndex;

                public RendererIndexData(Renderer renderer, int index)
                {
                    Renderer = renderer;
                    MaterialIndex = index;
                }
            }

            [Header("Eye color")]
            public Material EyeColorMaterial;

            [ColorUsageAttribute(true, true)]
            public Color DefaultEyeColor;

            [ColorUsageAttribute(true, true)]
            public Color AttackEyeColor;

            [Header("Flash on hit")]
            public Material BodyMaterial;

            [GradientUsageAttribute(true)]
            public Gradient OnHitBodyGradient;

            public float FlashOnHitDuration = 0.5f;

            [Header("VFX")]
            public GameObject DeathVfx;

            public Transform DeathVfxSpawnPoint;

            EnemyController m_Enemy;
            Health m_Health;

            List<RendererIndexData> m_BodyRenderers = new List<RendererIndexData>();
            MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;
            float m_LastTimeDamaged = float.NegativeInfinity;

            RendererIndexData m_EyeRendererData;
            MaterialPropertyBlock m_EyeColorMaterialPropertyBlock;

            void Start()
            {
                m_Enemy = GetComponent<EnemyController>();
                m_Health = GetComponent<Health>();

                // 订阅事件，解耦核心逻辑
                m_Enemy.onDetectedTarget += SetAttackEyeColor;
                m_Enemy.onLostTarget += SetDefaultEyeColor;
                m_Health.onDamaged += OnDamaged;
                m_Health.onDie += OnDie;

                InitializeRenderers();
            }

            void InitializeRenderers()
            {
                m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

                foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                {
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (renderer.sharedMaterials[i] == EyeColorMaterial)
                            m_EyeRendererData = new RendererIndexData(renderer, i);

                        if (renderer.sharedMaterials[i] == BodyMaterial)
                            m_BodyRenderers.Add(new RendererIndexData(renderer, i));
                    }
                }

                if (m_EyeRendererData.Renderer != null)
                {
                    m_EyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                    SetDefaultEyeColor();
                }
            }

            void Update()
            {
                // 处理受击闪烁
                Color currentColor = OnHitBodyGradient.Evaluate((Time.time - m_LastTimeDamaged) / FlashOnHitDuration);
                m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
                foreach (var data in m_BodyRenderers)
                {
                    data.Renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, data.MaterialIndex);
                }
            }

            void OnDamaged(float damage, GameObject source) => m_LastTimeDamaged = Time.time;

            void OnDie()
            {
                if (DeathVfx != null && DeathVfxSpawnPoint != null)
                {
                    var vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
                    Destroy(vfx, 5f);
                }
            }

            void SetDefaultEyeColor()
            {
                if (m_EyeRendererData.Renderer != null)
                {
                    m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
                    m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.MaterialIndex);
                }
            }

            void SetAttackEyeColor()
            {
                if (m_EyeRendererData.Renderer != null)
                {
                    m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", AttackEyeColor);
                    m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock, m_EyeRendererData.MaterialIndex);
                }
            }
        }
    }
}
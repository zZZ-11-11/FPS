using System.Collections.Generic;
using FPS.Game;
using FPS.Game.Shared;
using FPS.GamePlay.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace FPS.GamePlay
{
    public sealed class ProjectileStandard : ProjectileBase
    {
        [Header("General")]
        [Tooltip("碰撞检测半径")]
        public float radius = 0.01f;

        [Tooltip("子弹根部")]
        public Transform root;

        [Tooltip("子弹尖尖")]
        public Transform tip;

        [Tooltip("生存时间")]
        public float maxLifeTime = 5f;

        [Tooltip("碰撞特效")]
        public GameObject impactVfx;

        [Tooltip("碰撞特效生存时间")]
        public float impactVfxLifetime = 5f;

        [Tooltip("碰撞特效在法线上的偏移")]
        public float impactVfxSpawnOffset = 0.1f;

        [FormerlySerializedAs("ImpactSfxClip")]
        [Tooltip("碰撞音效")]
        public AudioClip impactSfxClip;

        [Tooltip("碰撞需要检测的层级")]
        public LayerMask hittableLayers = -1;

        [Header("Movement")]
        [Tooltip("子弹速度")]
        public float speed = 20f;

        [Tooltip("重力加速度")]
        public float gravityDownAcceleration;

        [Tooltip("炮弹将自行修正其飞行路线以符合预期弹道的距离,小于0不会修正")]
        public float trajectoryCorrectionDistance = -1;

        [Tooltip("是否继承武器速度")]
        public bool inheritWeaponVelocity;

        [Header("Damage")]
        [Tooltip("子弹伤害")]
        public float damage = 40f;

        [Tooltip("范围伤害")]
        public DamageArea areaOfDamage;

        [Header("Debug")]
        [Tooltip("Debug颜色")]
        public Color radiusColor = Color.cyan * 0.2f;

        private ProjectileBase m_ProjectileBase;
        private Vector3 m_LastRootPosition;
        private Vector3 m_Velocity;
        private bool m_HasTrajectoryOverride;
        private float m_ShootTime;
        private Vector3 m_TrajectoryCorrectionVector;
        private Vector3 m_ConsumedTrajectoryCorrectionVector;
        private List<Collider> m_IgnoredColliders;
        private readonly RaycastHit[] m_Hits = new RaycastHit[16];

        private
            const QueryTriggerInteraction k_trigger_interaction = QueryTriggerInteraction.Collide;

        void OnEnable()
        {
            m_ProjectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileStandard>(m_ProjectileBase, this,
                gameObject);

            m_ProjectileBase.onShoot += OnShoot;
        }

        //初始化
        void OnShoot()
        {
            // 修复子弹过长导致视觉上的穿墙bug
            var rootPosition = root.position;
            var distance = tip.position - rootPosition;

            if (CheckCollision(rootPosition, distance))
            {
                var allRenderers = GetComponentsInChildren<Renderer>();
                foreach (var r in allRenderers)
                {
                    r.enabled = false;
                }
                return;
            }

            m_ShootTime = Time.time;
            m_Velocity = transform.forward * speed;
            m_IgnoredColliders = new List<Collider>();

            var ownerColliders = m_ProjectileBase.owner.GetComponentsInChildren<Collider>();
            m_IgnoredColliders.AddRange(ownerColliders);

            var weaponManager = m_ProjectileBase.owner.GetComponent<WeaponManager>();
            var enemyController = m_ProjectileBase.owner.GetComponent<FPS.AI.EnemyController>();

            if (weaponManager)
            {
                // 玩家的弹道修正
                m_HasTrajectoryOverride = true;

                var weaponCameraTransform = weaponManager.WeaponCamera.transform;
                var cameraToMuzzle = (m_ProjectileBase.initialPosition - weaponCameraTransform.position);

                // 分解到xy上的偏移
                m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle, weaponCameraTransform.forward);

                if (trajectoryCorrectionDistance == 0)
                {
                    transform.position += m_TrajectoryCorrectionVector;
                    m_ConsumedTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
                }
                else if (trajectoryCorrectionDistance < 0)
                {
                    m_HasTrajectoryOverride = false;
                }

                // 防穿墙检测
                if (Physics.Raycast(weaponManager.WeaponCamera.transform.position, cameraToMuzzle.normalized,
                        out var hit, cameraToMuzzle.magnitude, hittableLayers, k_trigger_interaction))
                {
                    if (IsHitValid(hit)) OnHit(hit.point, hit.normal, hit.collider);
                }
            }
            else if (enemyController && enemyController.knownDetectedTarget)
            {
                // 敌人的弹道修正
                m_HasTrajectoryOverride = true;

                // 获取玩家的目标点
                var targetPos = enemyController.knownDetectedTarget.transform.position;

                print("获取玩家瞄准点坐标：" + targetPos);

                // 计算从枪管到目标点的实际向量
                var muzzleToTarget = targetPos - m_ProjectileBase.initialPosition;

                // 将该向量投影到垂直于枪管朝向的平面上
                m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(muzzleToTarget, transform.forward);

                //修正距离应为邻边
                var distanceToTarget = (muzzleToTarget - m_TrajectoryCorrectionVector).magnitude;

                // 防止敌人贴脸开枪时修正距离过短导致报错
                var enemyCorrectionDistance = Mathf.Max(distanceToTarget, 0.1f);

                trajectoryCorrectionDistance = enemyCorrectionDistance;
            }

            m_LastRootPosition = root.position;
        }

        void Update()
        {
            if (Time.time - m_ShootTime > maxLifeTime)
            {
                Destroy(gameObject);
            }
            var expectedDisplacement = m_Velocity * Time.deltaTime;
            //继承武器速度的位移
            if (inheritWeaponVelocity)
            {
                expectedDisplacement += GetWeaponVel() * Time.deltaTime;
            }

            if (m_HasTrajectoryOverride && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude <
                m_TrajectoryCorrectionVector.sqrMagnitude)
            {
                //剩下的修正位移
                var correctionLeft = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
                //当前帧的物理位移
                var distanceThisFrame = expectedDisplacement.magnitude;
                //当前帧的修正位移
                var correctionThisFrame =
                    (distanceThisFrame / trajectoryCorrectionDistance) * m_TrajectoryCorrectionVector;
                correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
                m_ConsumedTrajectoryCorrectionVector += correctionThisFrame;

                if (Mathf.Approximately(m_ConsumedTrajectoryCorrectionVector.sqrMagnitude, m_TrajectoryCorrectionVector.sqrMagnitude))
                {
                    m_HasTrajectoryOverride = false;
                }

                //总位移
                expectedDisplacement += correctionThisFrame;
            }

            // m_Velocity只与初始速度和重力有关，此处用于模拟弹道下坠
            transform.forward = m_Velocity.normalized;

            if (gravityDownAcceleration > 0)
            {
                m_Velocity += Vector3.down * (gravityDownAcceleration * Time.deltaTime);
            }

            //碰撞检测
            var displacementSinceLastFrame = tip.position + expectedDisplacement - m_LastRootPosition;
            if (CheckCollision(m_LastRootPosition, displacementSinceLastFrame))
            {
                return;
            }

            transform.position += expectedDisplacement;
            m_LastRootPosition = root.position;
        }

        private Vector3 GetWeaponVel()
        {
            // 获取武器速度
            var weaponVel = m_ProjectileBase.inheritedMuzzleVelocity;

            // 过滤掉向后的速度（如果速度方向和枪口朝向相反，点乘 < 0）
            if (Vector3.Dot(weaponVel, transform.forward) < 0)
            {
                // 将速度投影到垂直于枪口朝向的平面上（只保留横向/上下的惯性，消除后退）
                weaponVel = Vector3.ProjectOnPlane(weaponVel, transform.forward);
            }
            return weaponVel;
        }

        private bool CheckCollision(Vector3 startPoint, Vector3 displacement)
        {
            var closestHit = new RaycastHit
            {
                distance = Mathf.Infinity
            };
            var foundHit = false;

            var size = Physics.SphereCastNonAlloc(startPoint, radius, displacement.normalized, m_Hits, displacement.magnitude, hittableLayers, k_trigger_interaction);

            for (var index = 0; index < size; index++)
            {
                var hit = m_Hits[index];
                if (!IsHitValid(hit) || !(hit.distance < closestHit.distance))
                {
                    continue;
                }
                foundHit = true;
                closestHit = hit;
            }

            if (foundHit)
            {
                // 球体检测的起点，刚好已经位于一个碰撞体的内部时，直接在子弹当前位置生成特效
                if (closestHit.distance <= 0f)
                {
                    closestHit.point = root.position;
                    closestHit.normal = -transform.forward;
                }

                OnHit(closestHit.point, closestHit.normal, closestHit.collider);
                return true;
            }

            return false;
        }

        bool IsHitValid(RaycastHit hit)
        {
            // 忽略挂载了IgnoreHitDetection脚本的碰撞体,但消耗性能
            /*
            if (hit.collider.GetComponent<IgnoreHitDetection>())
            {
                return false;
            }
            */

            // 忽略没有伤害脚本的触发器
            if (hit.collider.isTrigger && !hit.collider.GetComponent<Damageable>())
            {
                return false;
            }

            // 忽略特殊碰撞体，如子弹的来源
            if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider))
            {
                return false;
            }

            return true;
        }

        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            if (areaOfDamage)
            {
                areaOfDamage.InflictDamageInArea(damage, point, hittableLayers, k_trigger_interaction,
                    m_ProjectileBase.owner);
            }
            else
            {
                var damageable = collider.GetComponent<Damageable>();
                if (damageable)
                {
                    damageable.InflictDamage(damage, false, m_ProjectileBase.owner);
                }
            }

            //生成碰撞特效
            if (impactVfx)
            {
                var impactVfxInstance = Instantiate(impactVfx, point + (normal * impactVfxSpawnOffset),
                    Quaternion.LookRotation(normal));
                if (impactVfxLifetime > 0)
                {
                    Destroy(impactVfxInstance.gameObject, impactVfxLifetime);
                }
            }

            // 生成碰撞音效
            if (impactSfxClip)
            {
                AudioUtility.CreateSfx(impactSfxClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);
            }

            Destroy(this.gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = radiusColor;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
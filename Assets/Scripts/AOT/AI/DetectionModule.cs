using System.Linq;
using FPS.Game;
using FPS.Game.Managers;
using UnityEngine;
using UnityEngine.Events;

namespace FPS.AI
{
    public class DetectionModule : MonoBehaviour
    {
        [Tooltip("射线检测的起始点（眼睛或头部）")]
        public Transform detectionSourcePoint;

        [Tooltip("最大视野距离")]
        public float detectionRange = 20f;

        [Tooltip("最大攻击距离")]
        public float attackRange = 10f;

        [Tooltip("丢失目标视野后，保持警戒记忆的时间")]
        public float knownTargetTimeout = 4f;

        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;

        public GameObject knownDetectedTarget { get; private set; }
        public bool isTargetInAttackRange { get; private set; }
        public bool isSeeingTarget { get; private set; }
        public bool hadKnownTarget { get; private set; }

        private float m_TimeLastSeenTarget = Mathf.NegativeInfinity;

        ActorsManager m_ActorsManager;

        private readonly RaycastHit[] m_RaycastHitsCache = new RaycastHit[20];

        protected virtual void Start()
        {
            m_ActorsManager = FindAnyObjectByType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, DetectionModule>(m_ActorsManager, this);
        }

        public virtual void HandleTargetDetection(Actor actor, Collider[] selfColliders)
        {
            // 1. 处理目标丢失超时
            if (knownDetectedTarget && !isSeeingTarget && (Time.time - m_TimeLastSeenTarget) > knownTargetTimeout)
            {
                knownDetectedTarget = null;
            }

            // 2. 寻找最近的可见敌对 Actor,直线检测
            var sqrDetectionRange = detectionRange * detectionRange;
            isSeeingTarget = false;
            var closestSqrDistance = Mathf.Infinity;

            foreach (var otherActor in m_ActorsManager.actors)
            {
                if (otherActor.affiliation != actor.affiliation)
                {
                    var sqrDistance = (otherActor.transform.position - detectionSourcePoint.position).sqrMagnitude;

                    // 在检测范围内，且比当前记录的最近目标还要近
                    if (sqrDistance < sqrDetectionRange && sqrDistance < closestSqrDistance)
                    {
                        var detectionSourcePosition = detectionSourcePoint.position;
                        var direction = (otherActor.aimPoint.position - detectionSourcePosition).normalized;

                        var hitCount = Physics.RaycastNonAlloc(
                            detectionSourcePosition,
                            direction,
                            m_RaycastHitsCache,
                            detectionRange,
                            -1,
                            QueryTriggerInteraction.Ignore);

                        var closestValidHitDistance = Mathf.Infinity;
                        var closestValidHit = new RaycastHit();
                        var foundValidHit = false;

                        for (var i = 0; i < hitCount; i++)
                        {
                            var hit = m_RaycastHitsCache[i];
                            // 忽略自身碰撞体，并找到最近的阻挡物
                            if (!selfColliders.Contains(hit.collider) && hit.distance < closestValidHitDistance)
                            {
                                closestValidHitDistance = hit.distance;
                                closestValidHit = hit;
                                foundValidHit = true;
                            }
                        }

                        // 如果射线最终打到的是这个目标 Actor，说明没有被墙壁等障碍物挡住
                        if (foundValidHit)
                        {
                            var hitActor = closestValidHit.collider.GetComponentInParent<Actor>();
                            if (hitActor == otherActor)
                            {
                                isSeeingTarget = true;
                                closestSqrDistance = sqrDistance;
                                m_TimeLastSeenTarget = Time.time;
                                knownDetectedTarget = otherActor.aimPoint.gameObject;
                            }
                        }
                    }
                }
            }

            isTargetInAttackRange = knownDetectedTarget != null &&
                                    Vector3.Distance(transform.position, knownDetectedTarget.transform.position) <= attackRange;

            // 3. 触发检测事件
            if (!hadKnownTarget && knownDetectedTarget != null)
            {
                OnDetect();
            }
            if (hadKnownTarget && knownDetectedTarget == null)
            {
                OnLostTarget();
            }

            hadKnownTarget = knownDetectedTarget != null;
        }

        protected virtual void OnLostTarget() => onLostTarget?.Invoke();

        protected virtual void OnDetect() => onDetectedTarget?.Invoke();

        // 受到伤害时，强制获取目标视野位置
        public virtual void OnDamaged(GameObject damageSource)
        {
            m_TimeLastSeenTarget = Time.time;
            knownDetectedTarget = damageSource;
        }
    }
}
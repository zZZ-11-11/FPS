using FPS.AI.FSM;
using FPS.Game;
using FPS.Game.Managers;
using FPS.Game.Shared;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using FPS.GamePlay.Weapon;
using Random = UnityEngine.Random;

namespace FPS.AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public sealed class EnemyController : MonoBehaviour
    {
        [Header("Parameters")]
        [Tooltip("死亡时销毁的 Y 坐标")]
        public float selfDestructYHeight = -20f;

        [Tooltip("路径节点到达判定半径")]
        public float pathReachingRadius = 2f;

        [Tooltip("转向速度")]
        public float orientationSpeed = 10f;

        [Tooltip("死亡延迟销毁时间")]
        public float deathDuration;

        [Header("Weapons Parameters")]
        [Tooltip("攻击后是否自动换武器")]
        public bool swapToNextWeapon;

        [Tooltip("切换武器后射击延迟")]
        public float delayAfterWeaponSwap;

        [Header("Loot")]
        [Tooltip("掉落物")]
        public GameObject lootPrefab;

        [Range(0, 1)]
        [Tooltip("掉落物掉落概率")]
        public float dropRate = 1f;

        private IEnemyState m_CurrentState;

        public UnityAction onAttack;
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;

        public PatrolPath patrolPath { get; set; }
        public NavMeshAgent navMeshAgent { get; private set; }
        public DetectionModule detectionModule { get; private set; }

        public GameObject knownDetectedTarget => detectionModule.knownDetectedTarget;
        public bool isTargetInAttackRange => detectionModule.isTargetInAttackRange;
        public bool isSeeingTarget => detectionModule.isSeeingTarget;
        public bool hadKnownTarget => detectionModule.hadKnownTarget;

        public EnemyDeadState deadState;
        public EnemyPatrolState patrolState;
        public EnemyAttackState attackState;
        public EnemyChaseState chaseState;

        Health m_Health;
        Actor m_Actor;
        Collider[] m_SelfColliders;
        int m_PathDestinationNodeIndex;

        EnemyManager m_EnemyManager;
        ActorsManager m_ActorsManager;
        GameFlowManager m_GameFlowManager;
        EnemyFXController m_EnemyFXController;

        WeaponCore[] m_Weapons;
        WeaponCore m_CurrentWeapon;
        int m_CurrentWeaponIndex;
        float m_LastTimeWeaponSwapped = Mathf.NegativeInfinity;
        bool m_WasDamagedThisFrame;

        private void Awake()
        {
            deadState = new EnemyDeadState();
            patrolState = new EnemyPatrolState();
            attackState = new EnemyAttackState();
            chaseState = new EnemyChaseState();
        }

        void Start()
        {
            m_EnemyManager = FindAnyObjectByType<EnemyManager>();
            m_ActorsManager = FindAnyObjectByType<ActorsManager>();
            m_GameFlowManager = FindAnyObjectByType<GameFlowManager>();
            m_EnemyManager.RegisterEnemy(this);

            // 本地组件初始化
            m_Health = GetComponent<Health>();
            m_Actor = GetComponent<Actor>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            m_SelfColliders = GetComponentsInChildren<Collider>();
            detectionModule = GetComponentInChildren<DetectionModule>();
            m_EnemyFXController = GetComponentInChildren<EnemyFXController>();
            m_Health.onDie += OnDie;
            m_Health.onDamaged += HandleDamage;

            // 初始化武器
            FindAndInitializeAllWeapons();
            GetCurrentWeapon().ShowWeapon(true);

            // 状态机初始化
            ChangeState(patrolState);
        }

        void Update()
        {
            //检测死亡高度
            if (transform.position.y < selfDestructYHeight)
            {
                Destroy(gameObject);
                return;
            }

            //索敌检测
            detectionModule.HandleTargetDetection(m_Actor, m_SelfColliders);

            // 执行当前状态
            m_CurrentState?.Update(this);

            m_WasDamagedThisFrame = false;
        }

        public void ChangeState(IEnemyState newState)
        {
            m_CurrentState?.Exit(this);
            m_CurrentState = newState;
            m_CurrentState?.Enter(this);
        }

        public void SetNavDestination(Vector3 destination) => navMeshAgent.SetDestination(destination);

        //设置路径目标为最近的节点(设置索引)
        public void SetPathDestinationToClosestNode()
        {
            if (patrolPath && patrolPath.pathNodes.Count > 0)
            {
                var closest = 0;
                for (var i = 0; i < patrolPath.pathNodes.Count; i++)
                {
                    if (patrolPath.GetDistanceToNode(transform.position, i) < patrolPath.GetDistanceToNode(transform.position, closest))
                    {
                        closest = i;
                    }
                }
                m_PathDestinationNodeIndex = closest;
            }
        }

        //获取路径目标（世界坐标）
        public Vector3 GetDestinationOnPath() => patrolPath != null ? patrolPath.GetPositionOfPathNode(m_PathDestinationNodeIndex) : transform.position;

        //到达后自动换下一个节点
        public void UpdatePathDestination()
        {
            if (patrolPath && (transform.position - GetDestinationOnPath()).magnitude <= pathReachingRadius)
            {
                m_PathDestinationNodeIndex = (m_PathDestinationNodeIndex + 1) % patrolPath.pathNodes.Count;
            }
        }

        //转向并开火
        public bool TryAttack(Vector3 enemyPosition)
        {
            if (m_GameFlowManager != null && m_GameFlowManager.GameIsEnding)
            {
                return false;
            }

            OrientTowards(enemyPosition);

            if (Time.time < m_LastTimeWeaponSwapped + delayAfterWeaponSwap)
            {
                return false;
            }

            var didFire = GetCurrentWeapon().HandleShootInputs(false, true, false);
            if (didFire)
            {
                onAttack?.Invoke();
                if (swapToNextWeapon && m_Weapons.Length > 1)
                {
                    SetCurrentWeapon((m_CurrentWeaponIndex + 1) % m_Weapons.Length);
                }
            }
            return didFire;
        }

        private void OrientTowards(Vector3 lookPosition)
        {
            var lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude != 0f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * orientationSpeed);
            }
        }

        //如果伤害源非同类，检测模块强制发现敌人，如果当前状态非死亡，进入追逐状态，如果有音效则播放，标记此帧为已受伤,一帧只会播放一次音效
        void HandleDamage(float damage, GameObject damageSource)
        {
            if (damageSource && !damageSource.GetComponent<EnemyController>())
            {
                detectionModule.OnDamaged(damageSource);

                if (m_CurrentState is not EnemyDeadState)
                {
                    ChangeState(chaseState);
                }

                if (!m_WasDamagedThisFrame)
                {
                    m_EnemyFXController.PlayDamageTick();
                }

                m_WasDamagedThisFrame = true;
            }
        }

        void OnDie()
        {
            ChangeState(deadState);
            m_EnemyManager.UnregisterEnemy(this);

            if (dropRate > 0 && lootPrefab != null && (Mathf.Approximately(dropRate, 1) || Random.value <= dropRate))
            {
                Instantiate(lootPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject, deathDuration);
        }

        void FindAndInitializeAllWeapons()
        {
            if (m_Weapons == null)
            {
                // 查找所有的 WeaponCore
                m_Weapons = GetComponentsInChildren<WeaponCore>();

                for (var i = 0; i < m_Weapons.Length; i++)
                {
                    // 使用新的 SetOwner 方法
                    m_Weapons[i].SetOwner(gameObject);
                }
            }
        }

        public WeaponCore GetCurrentWeapon()
        {
            FindAndInitializeAllWeapons();
            if (m_CurrentWeapon == null)
            {
                SetCurrentWeapon(0);
            }
            return m_CurrentWeapon;
        }

        void SetCurrentWeapon(int index)
        {
            if (m_Weapons.Length == 0)
            {
                return;
            }

            m_CurrentWeaponIndex = index;
            m_CurrentWeapon = m_Weapons[m_CurrentWeaponIndex];

            m_LastTimeWeaponSwapped = swapToNextWeapon ? Time.time : Mathf.NegativeInfinity;
        }
    }
}
using FPS.AI;
using FPS.Game.Managers;
using UnityEngine;

namespace FPS.Game.Objective

{
    public sealed class ObjectiveKillEnemies : Shared.Objective
    {
        [Tooltip("是否需要全部击杀")]
        public bool mustKillAllEnemies = true;

        [Tooltip("杀敌目标数")]
        public int killsToCompleteObjective = 5;

        [Tooltip("倒计时通知阈值")]
        public int notificationEnemiesRemainingThreshold = 3;

        //已击杀数量
        private int m_KillTotal;

        //订阅敌人死亡事件
        protected override void Start()
        {
            //计算所需杀敌数
            if (mustKillAllEnemies)
            {
                var enemyManager = FindAnyObjectByType<EnemyManager>();
                if (enemyManager != null)
                {
                    killsToCompleteObjective = enemyManager.numberOfEnemiesRemaining;
                }
                else
                {
                    Debug.LogWarning("ObjectiveKillEnemies: 场景中未找到 EnemyManager！");
                }
            }

            if (string.IsNullOrEmpty(title))
            {
                title = "杀死 " + (mustKillAllEnemies ? "所有" : killsToCompleteObjective.ToString()) +
                        " 个敌人";
            }

            if (string.IsNullOrEmpty(description))
            {
                description = GetUpdatedCounterAmount();
            }
            base.Start();

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

            EventManager.AddListener<EnemySpawnEvent>(OnEnemySpawned);
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (isCompleted)
            {
                return;
            }

            m_KillTotal++;

            //剩余杀敌数
            var targetRemaining = killsToCompleteObjective - m_KillTotal;

            switch (targetRemaining)
            {
                case 0:
                    CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "任务完成 : " + title);
                    break;
                case 1:
                {
                    var notificationText = notificationEnemiesRemainingThreshold >= targetRemaining
                        ? "剩余一个敌人"
                        : string.Empty;
                    UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
                    break;
                }
                default:
                {
                    var notificationText = notificationEnemiesRemainingThreshold >= targetRemaining
                        ? " 剩余敌人数：" + targetRemaining
                        : string.Empty;

                    UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
                    break;
                }
            }
        }

        void OnEnemySpawned(EnemySpawnEvent evt)
        {
            // 如果任务已经完成，或者不是“必须杀光全图”模式，就不需要关心新生成的敌人
            if (isCompleted || !mustKillAllEnemies)
            {
                return;
            }

            killsToCompleteObjective = evt.remainingEnemyCount + m_KillTotal;

            UpdateObjective(string.Empty, GetUpdatedCounterAmount(), "新的敌人加入战场!");
        }

        private string GetUpdatedCounterAmount() => m_KillTotal + " / " + killsToCompleteObjective;

        //防止内存泄漏
        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
            EventManager.RemoveListener<EnemySpawnEvent>(OnEnemySpawned);
        }
    }
}
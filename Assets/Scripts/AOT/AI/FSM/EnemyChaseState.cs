namespace FPS.AI.FSM
{
    public sealed class EnemyChaseState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            enemy.navMeshAgent.isStopped = false;
            enemy.onDetectedTarget?.Invoke();
        }

        public void Update(EnemyController enemy)
        {
            // 状态转换：目标丢失，回到巡逻
            if (!enemy.isSeeingTarget && !enemy.hadKnownTarget)
            {
                enemy.onLostTarget?.Invoke();
                enemy.ChangeState(enemy.patrolState);
                return;
            }

            // 状态转换：进入攻击范围，开始攻击
            if (enemy.isTargetInAttackRange)
            {
                enemy.ChangeState(enemy.attackState);
                return;
            }

            // 追击逻辑
            if (enemy.knownDetectedTarget != null)
            {
                enemy.SetNavDestination(enemy.knownDetectedTarget.transform.position);
            }
        }

        public void Exit(EnemyController enemy)
        {
        }
    }
}
namespace FPS.AI.FSM
{
    public sealed class EnemyPatrolState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            //设置路径目标为最近的节点(设置索引)
            enemy.SetPathDestinationToClosestNode();
            enemy.navMeshAgent.isStopped = false;
        }

        public void Update(EnemyController enemy)
        {
            // 状态转换：发现目标，进入追击
            if (enemy.isSeeingTarget || enemy.hadKnownTarget)
            {
                enemy.ChangeState(enemy.chaseState);
                return;
            }

            // 巡逻逻辑
            if (enemy.patrolPath != null)
            {
                enemy.SetNavDestination(enemy.GetDestinationOnPath());
                enemy.UpdatePathDestination();
            }
        }

        public void Exit(EnemyController enemy)
        {
        }
    }
}
using FPS.AI;

namespace FPS.AI.FSM
{
    public class EnemyAttackState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            // 攻击时停止移动
            enemy.navMeshAgent.isStopped = true;
        }

        public void Update(EnemyController enemy)
        {
            // 状态转换：目标离开攻击范围或丢失，切回追击/巡逻
            if (!enemy.isTargetInAttackRange || enemy.knownDetectedTarget == null)
            {
                enemy.ChangeState(new EnemyChaseState());
                return;
            }

            // 攻击逻辑
            enemy.TryAttack(enemy.knownDetectedTarget.transform.position);
        }

        public void Exit(EnemyController enemy)
        {
        }
    }
}
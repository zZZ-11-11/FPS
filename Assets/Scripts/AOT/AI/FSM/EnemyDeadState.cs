using FPS.AI;

namespace FPS.AI.FSM
{
    public class EnemyDeadState : IEnemyState
    {
        public void Enter(EnemyController enemy)
        {
            enemy.navMeshAgent.isStopped = true;
            enemy.navMeshAgent.enabled = false;
        }

        public void Update(EnemyController enemy)
        {
        }

        public void Exit(EnemyController enemy)
        {
        }
    }
}
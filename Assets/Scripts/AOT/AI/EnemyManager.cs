using System.Collections.Generic;
using FPS.Game;
using FPS.Game.Managers;
using UnityEngine;

namespace FPS.AI
{
    public sealed class EnemyManager : MonoBehaviour
    {
        public List<EnemyController> enemies { get; private set; } = new List<EnemyController>();
        public int numberOfEnemiesTotal { get; private set; }
        public int numberOfEnemiesRemaining => enemies.Count;
        private const float initial_spawn_window = 0.2f;

        void Awake()
        {
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            enemies.Add(enemy);

            numberOfEnemiesTotal++;

            var isInitial = Time.timeSinceLevelLoad < initial_spawn_window;

            if (isInitial)
            {
                return;
            }

            var evt = Events.enemySpawnEvent;
            evt.enemy = enemy.gameObject;
            evt.remainingEnemyCount = numberOfEnemiesRemaining;
            EventManager.Broadcast(evt);
        }

        public void UnregisterEnemy(EnemyController enemyKilled)
        {
            if (!enemies.Remove(enemyKilled))
            {
                Debug.LogError("EnemyManager: UnregisterEnemy failed");
                return;
            }

            var evt = Events.enemyKillEvent;
            evt.enemy = enemyKilled.gameObject;
            evt.remainingEnemyCount = numberOfEnemiesRemaining;

            EventManager.Broadcast(evt);
        }
    }
}
using System.Collections.Generic;
using FPS.Game;
using FPS.Game.Managers;
using UnityEngine;

namespace FPS.AI
{
    public class EnemyManager : MonoBehaviour
    {
        public List<EnemyController> enemies { get; private set; }
        public int numberOfEnemiesTotal { get; private set; }
        public int numberOfEnemiesRemaining => enemies.Count;

        void Awake()
        {
            enemies = new List<EnemyController>();
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            enemies.Add(enemy);

            numberOfEnemiesTotal++;
        }

        public void UnregisterEnemy(EnemyController enemyKilled)
        {
            if (!enemies.Remove(enemyKilled))
            {
                Debug.LogError("EnemyManager: UnregisterEnemy failed");
                return;
            }
            var remainingCount = enemies.Count;

            var evt = Events.enemyKillEvent;
            evt.enemy = enemyKilled.gameObject;
            evt.remainingEnemyCount = remainingCount;

            EventManager.Broadcast(evt);
        }
    }
}
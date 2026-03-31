using System.Collections.Generic;
using FPS.Game.Shared;
using UnityEngine;

namespace FPS.Game.Managers
{
    public sealed class ObjectiveManager : MonoBehaviour
    {
        private readonly List<Objective> m_Objectives = new List<Objective>();
        private bool m_ObjectivesCompleted;

        void Awake()
        {
            Objective.onObjectiveCreated += RegisterObjective;
            Objective.onObjectiveDestroyed += UnregisterObjective;
            Objective.onObjectiveCompleted += CheckAllObjectivesCompleted;
        }

        private void RegisterObjective(Objective objective) => m_Objectives.Add(objective);

        private void UnregisterObjective(Objective objective) => m_Objectives.Remove(objective);

        /// <summary>
        /// 检查所有任务是否完成
        /// </summary>
        /// <param name="completedObjective"></param>
        private void CheckAllObjectivesCompleted(Objective completedObjective)
        {
            if (m_Objectives.Count == 0 || m_ObjectivesCompleted)
            {
                return;
            }

            foreach (var objective in m_Objectives)
            {
                if (objective.IsBlocking())
                {
                    return; // 发现还有未完成的主线任务，直接返回
                }
            }

            m_ObjectivesCompleted = true;
            EventManager.Broadcast(Events.allObjectivesCompletedEvent);
        }

        private void OnDestroy()
        {
            Objective.onObjectiveCreated -= RegisterObjective;
            Objective.onObjectiveDestroyed -= UnregisterObjective;
            Objective.onObjectiveCompleted -= CheckAllObjectivesCompleted;
        }
    }
}
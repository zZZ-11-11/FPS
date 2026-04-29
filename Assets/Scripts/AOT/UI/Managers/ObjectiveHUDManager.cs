using System.Collections.Generic;
using FPS.Game;
using FPS.Game.Managers;
using FPS.Game.Shared;
using UnityEngine;

namespace FPS.UI.Manager
{
    public sealed class ObjectiveHUDManager : MonoBehaviour
    {
        [Tooltip("任务面板RectTransform")]
        public RectTransform objectivePanel;

        [Tooltip("主线任务")]
        public GameObject primaryObjectivePrefab;

        [Tooltip("支线任务")]
        public GameObject secondaryObjectivePrefab;

        Dictionary<Objective, ObjectiveToast> m_ObjectivesDictionary;

        void Awake()
        {
            m_ObjectivesDictionary = new Dictionary<Objective, ObjectiveToast>();

            EventManager.AddListener<ObjectiveUpdateEvent>(OnUpdateObjective);

            Objective.onObjectiveCreated += RegisterObjective;
            Objective.onObjectiveCompleted += UnregisterObjective;
        }

        private void RegisterObjective(Objective objective)
        {
            if (m_ObjectivesDictionary.ContainsKey(objective))
            {
                return;
            }
            // 根据主线或支线实例化UI
            var objectiveUIInstance =
                Instantiate(objective.isOptional ? secondaryObjectivePrefab : primaryObjectivePrefab, objectivePanel);

            // 置顶最新的主线任务
            if (!objective.isOptional)
            {
                objectiveUIInstance.transform.SetSiblingIndex(0);
            }

            var toast = objectiveUIInstance.GetComponent<ObjectiveToast>();
            DebugUtility.HandleErrorIfNullGetComponent<ObjectiveToast, ObjectiveHUDManager>(toast, this,
                objectiveUIInstance.gameObject);

            // 初始化文本等
            toast.Initialize(objective.title, objective.description, objective.counter, objective.isOptional, objective.delayVisible);

            // 注册到字典中
            m_ObjectivesDictionary.Add(objective, toast);
        }

        private void UnregisterObjective(Objective objective)
        {
            // 将在字典中的对应任务淡出和滑出
            if (m_ObjectivesDictionary.TryGetValue(objective, out var toast) && toast)
            {
                toast.Complete();
            }

            //移出字典
            m_ObjectivesDictionary.Remove(objective);
        }

        void OnUpdateObjective(ObjectiveUpdateEvent evt)
        {
            if (!m_ObjectivesDictionary.TryGetValue(evt.objective, out var toast) || toast == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(evt.descriptionText))
            {
                toast.descriptionTextContent.text = evt.descriptionText;
            }

            if (!string.IsNullOrEmpty(evt.counterText))
            {
                toast.counterTextContent.text = evt.counterText;
            }

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) toast.transform);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<ObjectiveUpdateEvent>(OnUpdateObjective);

            Objective.onObjectiveCreated -= RegisterObjective;
            Objective.onObjectiveCompleted -= UnregisterObjective;
        }
    }
}
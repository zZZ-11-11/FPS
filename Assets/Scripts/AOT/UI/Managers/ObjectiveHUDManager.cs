using System.Collections.Generic;
using FPS.Game;
using FPS.Game.Managers;
using FPS.Game.Shared;
using UnityEngine;

namespace FPS.UI.Manager
{
    public class ObjectiveHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying objectives")]
        public RectTransform ObjectivePanel;

        [Tooltip("Prefab for the primary objectives")]
        public GameObject PrimaryObjectivePrefab;

        [Tooltip("Prefab for the primary objectives")]
        public GameObject SecondaryObjectivePrefab;

        Dictionary<Objective, ObjectiveToast> m_ObjectivesDictionnary;

        void Awake()
        {
            m_ObjectivesDictionnary = new Dictionary<Objective, ObjectiveToast>();

            EventManager.AddListener<ObjectiveUpdateEvent>(OnUpdateObjective);

            Objective.onObjectiveCreated += RegisterObjective;
            Objective.onObjectiveCompleted += UnregisterObjective;
        }

        public void RegisterObjective(Objective objective)
        {
            // instanciate the Ui element for the new objective
            GameObject objectiveUIInstance =
                Instantiate(objective.isOptional ? SecondaryObjectivePrefab : PrimaryObjectivePrefab, ObjectivePanel);

            if (!objective.isOptional)
                objectiveUIInstance.transform.SetSiblingIndex(0);

            ObjectiveToast toast = objectiveUIInstance.GetComponent<ObjectiveToast>();
            DebugUtility.HandleErrorIfNullGetComponent<ObjectiveToast, ObjectiveHUDManager>(toast, this,
                objectiveUIInstance.gameObject);

            // initialize the element and give it the objective description
            toast.Initialize(objective.title, objective.description, "", objective.isOptional, objective.delayVisible);

            m_ObjectivesDictionnary.Add(objective, toast);

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ObjectivePanel);
        }

        public void UnregisterObjective(Objective objective)
        {
            // if the objective if in the list, make it fade out, and remove it from the list
            if (m_ObjectivesDictionnary.TryGetValue(objective, out ObjectiveToast toast) && toast != null)
            {
                toast.Complete();
            }

            m_ObjectivesDictionnary.Remove(objective);
        }

        void OnUpdateObjective(ObjectiveUpdateEvent evt)
        {
            if (m_ObjectivesDictionnary.TryGetValue(evt.objective, out ObjectiveToast toast) && toast != null)
            {
                // set the new updated description for the objective, and forces the content size fitter to be recalculated
                Canvas.ForceUpdateCanvases();
                if (!string.IsNullOrEmpty(evt.descriptionText))
                    toast.descriptionTextContent.text = evt.descriptionText;

                if (!string.IsNullOrEmpty(evt.counterText))
                    toast.counterTextContent.text = evt.counterText;

                if (toast.GetComponent<RectTransform>())
                {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(toast.GetComponent<RectTransform>());
                }
            }
        }

        void OnDestroy()
        {
            EventManager.AddListener<ObjectiveUpdateEvent>(OnUpdateObjective);

            Objective.onObjectiveCreated -= RegisterObjective;
            Objective.onObjectiveCompleted -= UnregisterObjective;
        }
    }
}
using System;
using FPS.Game.Managers;
using UnityEngine;

namespace FPS.Game.Shared
{
    public abstract class Objective : MonoBehaviour
    {
        [Tooltip("Name of the objective that will be shown on screen")]
        public string Title;

        [Tooltip("Short text explaining the objective that will be shown on screen")]
        public string Description;

        [Tooltip("Whether the objective is required to win or not")]
        public bool IsOptional;

        [Tooltip("Delay before the objective becomes visible")]
        public float DelayVisible;

        public bool IsCompleted { get; private set; }
        public bool IsBlocking() => !(IsOptional || IsCompleted);

        public static event Action<Objective> OnObjectiveCreated;
        public static event Action<Objective> OnObjectiveCompleted;

        protected virtual void Start()
        {
            OnObjectiveCreated?.Invoke(this);

            DisplayMessageEvent displayMessage = Events.displayMessageEvent;
            displayMessage.message = Title;
            displayMessage.delayBeforeDisplay = 0.0f;
            EventManager.Broadcast(displayMessage);
        }

        public void UpdateObjective(string descriptionText, string counterText, string notificationText)
        {
            ObjectiveUpdateEvent evt = Events.objectiveUpdateEvent;
            evt.objective = this;
            evt.descriptionText = descriptionText;
            evt.counterText = counterText;
            evt.notificationText = notificationText;
            evt.isComplete = IsCompleted;
            EventManager.Broadcast(evt);
        }

        public void CompleteObjective(string descriptionText, string counterText, string notificationText)
        {
            IsCompleted = true;

            ObjectiveUpdateEvent evt = Events.objectiveUpdateEvent;
            evt.objective = this;
            evt.descriptionText = descriptionText;
            evt.counterText = counterText;
            evt.notificationText = notificationText;
            evt.isComplete = IsCompleted;
            EventManager.Broadcast(evt);

            OnObjectiveCompleted?.Invoke(this);
        }
    }
}
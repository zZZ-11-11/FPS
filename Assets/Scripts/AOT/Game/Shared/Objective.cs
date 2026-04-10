using System;
using FPS.Game.Managers;
using UnityEngine;

namespace FPS.Game.Shared
{
    public abstract class Objective : MonoBehaviour
    {
        [Tooltip("标题")]
        public string title;

        [Tooltip("描述")]
        public string description;

        [Tooltip("是否支线")]
        public bool isOptional;

        [Tooltip("延迟显示时间")]
        public float delayVisible;

        //是否完成
        protected bool isCompleted { get; private set; }

        //是否阻塞（非支线且未完成），用于游戏管理器判断玩家是否可以进入下一关
        public bool IsBlocking() => !(isOptional || isCompleted);

        public static event Action<Objective> onObjectiveCreated;
        public static event Action<Objective> onObjectiveCompleted;
        public static event Action<Objective> onObjectiveDestroyed;

        //调用委托，广播事件
        protected virtual void Start()
        {
            onObjectiveCreated?.Invoke(this);

            var displayMessage = Events.displayMessageEvent;
            displayMessage.message = title;
            displayMessage.delayBeforeDisplay = delayVisible;
            EventManager.Broadcast(displayMessage);
        }

        //由子类在任务进度更新时调用（比如“收集苹果 1/5”变成了“收集苹果 2/5”），广播，更新UI
        protected void UpdateObjective(string descriptionText, string counterText, string notificationText)
        {
            var evt = Events.objectiveUpdateEvent;
            evt.objective = this;
            evt.descriptionText = descriptionText;
            evt.counterText = counterText;
            evt.notificationText = notificationText;
            evt.isComplete = isCompleted;
            EventManager.Broadcast(evt);
        }

        //完成任务，广播，更新UI，调用委托，检查游戏是否结束
        protected void CompleteObjective(string descriptionText, string counterText, string notificationText)
        {
            isCompleted = true;

            var evt = Events.objectiveUpdateEvent;
            evt.objective = this;
            evt.descriptionText = descriptionText;
            evt.counterText = counterText;
            evt.notificationText = notificationText;
            evt.isComplete = isCompleted;
            EventManager.Broadcast(evt);

            onObjectiveCompleted?.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
            onObjectiveDestroyed?.Invoke(this);
        }
    }
}
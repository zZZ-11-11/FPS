using FPS.Game.Shared;
using FPS.GamePlay.Weapon;
using UnityEngine;

namespace FPS.Game
{
    public static class Events
    {
        public static readonly ObjectiveUpdateEvent objectiveUpdateEvent = new ObjectiveUpdateEvent();
        public static AllObjectivesCompletedEvent allObjectivesCompletedEvent = new AllObjectivesCompletedEvent();
        public static GameOverEvent gameOverEvent = new GameOverEvent();
        public static readonly PlayerDeathEvent playerDeathEvent = new PlayerDeathEvent();
        public static readonly EnemyKillEvent enemyKillEvent = new EnemyKillEvent();
        public static PickupEvent pickupEvent = new PickupEvent();
        public static AmmoPickupEvent ammoPickupEvent = new AmmoPickupEvent();
        public static DamageEvent damageEvent = new DamageEvent();
        public static readonly DisplayMessageEvent displayMessageEvent = new DisplayMessageEvent();
    }

    public sealed class ObjectiveUpdateEvent : GameEvent
    {
        public Objective objective;
        public string descriptionText;
        public string counterText;
        public bool isComplete;
        public string notificationText;
    }

    public sealed class AllObjectivesCompletedEvent : GameEvent
    {
    }

    public sealed class GameOverEvent : GameEvent
    {
        public bool win;
    }

    public sealed class PlayerDeathEvent : GameEvent
    {
    }

    public sealed class EnemyKillEvent : GameEvent
    {
        public GameObject enemy;
        public int remainingEnemyCount;
    }

    public sealed class PickupEvent : GameEvent
    {
        public GameObject pickup;
    }

    public sealed class AmmoPickupEvent : GameEvent
    {
        public WeaponCore weapon;
    }

    public sealed class DamageEvent : GameEvent
    {
        public GameObject sender;
        public float damageValue;
    }

    public sealed class DisplayMessageEvent : GameEvent
    {
        public string message;
        public float delayBeforeDisplay;
    }
}
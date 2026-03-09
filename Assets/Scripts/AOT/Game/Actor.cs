using FPS.Game.Managers;
using UnityEngine;

namespace FPS.Game
{
    public sealed class Actor : MonoBehaviour
    {
        public int affiliation;

        public Transform aimPoint;

        private ActorsManager m_ActorsManager;

        void Start()
        {
            m_ActorsManager = GameObject.FindFirstObjectByType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, Actor>(m_ActorsManager, this);

            // Register as an actor
            if (!m_ActorsManager.actors.Contains(this))
            {
                m_ActorsManager.actors.Add(this);
            }
        }

        void OnDestroy()
        {
            // Unregister as an actor
            if (m_ActorsManager)
            {
                m_ActorsManager.actors.Remove(this);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Serialization;

namespace FPS.Game
{
    public sealed class TimedSelfDestruct : MonoBehaviour
    {
        public float lifeTime = 1f;

        private float m_SpawnTime;

        void Awake()
        {
            m_SpawnTime = Time.time;
        }

        void Update()
        {
            if (Time.time > m_SpawnTime + lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Events;

namespace FPS.GamePlay.Base
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        public GameObject owner { get; private set; }
        public Vector3 initialPosition { get; private set; }
        public Vector3 initialDirection { get; private set; }
        public Vector3 inheritedMuzzleVelocity { get; private set; }
        public float initialCharge { get; private set; }

        public UnityAction onShoot;

        public void Shoot(ShootContext shootContext)
        {
            owner = shootContext.owner;
            inheritedMuzzleVelocity = shootContext.muzzleWorldVelocity;
            initialCharge = shootContext.currentCharge;

            var projectileTransform = transform;
            initialPosition = projectileTransform.position;
            initialDirection = projectileTransform.forward;
            onShoot?.Invoke();
        }
    }
}
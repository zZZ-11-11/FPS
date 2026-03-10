using UnityEngine;

namespace FPS.GamePlay.Base
{
    public readonly struct ShootContext
    {
        public float currentCharge { get; }
        public GameObject owner { get; }
        public Vector3 muzzleWorldVelocity { get; }

        public ShootContext(float currentCharge, GameObject owner, Vector3 muzzleWorldVelocity)
        {
            this.currentCharge = currentCharge;
            this.owner = owner;
            this.muzzleWorldVelocity = muzzleWorldVelocity;
        }
    }
}
using UnityEngine;

namespace FPS.GamePlay.Base
{
    public readonly struct ShootContext
    {
        public readonly float currentCharge;
        public readonly GameObject owner;
        public readonly Vector3 muzzleWorldVelocity;

        public readonly Transform initialTransform;

        public readonly Vector3 shotDirection;

        public readonly Collider[] ownerColliders;
        public readonly bool isPlayer;
        public readonly Transform weaponCameraTransform;
        public readonly Transform targetTransform;

        /// <summary>
        /// 发射子弹上下文
        /// </summary>
        /// <param name="currentCharge">充能程度</param>
        /// <param name="owner">子弹来源</param>
        /// <param name="muzzleWorldVelocity">枪口速度</param>
        /// <param name="initialTransform">初始坐标</param>
        /// <param name="shotDirection">发射角度（散射角）</param>
        /// <param name="ownerColliders">来源碰撞体</param>
        /// <param name="isPlayer">是否玩家</param>
        /// <param name="weaponCameraTransform">玩家武器相机坐标</param>
        /// <param name="targetTransform">目标坐标</param>
        public ShootContext(float currentCharge, GameObject owner, Vector3 muzzleWorldVelocity, Transform initialTransform,
            Vector3 shotDirection, Collider[] ownerColliders, bool isPlayer, Transform weaponCameraTransform, Transform targetTransform)
        {
            this.currentCharge = currentCharge;
            this.owner = owner;
            this.muzzleWorldVelocity = muzzleWorldVelocity;
            this.initialTransform = initialTransform;
            this.shotDirection = shotDirection;
            this.ownerColliders = ownerColliders;
            this.isPlayer = isPlayer;
            this.weaponCameraTransform = weaponCameraTransform;
            this.targetTransform = targetTransform;
        }
    }
}
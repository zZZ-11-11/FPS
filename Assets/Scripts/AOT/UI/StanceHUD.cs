using FPS.Game;
using FPS.GamePlay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FPS.UI
{
    public class StanceHUD : MonoBehaviour
    {
        [Tooltip("状态图片物体")]
        public Image stanceImage;

        [Tooltip("战立图片")]
        public Sprite standingSprite;

        [Tooltip("蹲下图片")]
        public Sprite crouchingSprite;

        void Start()
        {
            var character = FindFirstObjectByType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, StanceHUD>(character, this);
            character.onStanceChanged += OnStanceChanged;

            OnStanceChanged(character.isCrouching);
        }

        void OnStanceChanged(bool crouched)
        {
            stanceImage.sprite = crouched ? crouchingSprite : standingSprite;
        }
    }
}
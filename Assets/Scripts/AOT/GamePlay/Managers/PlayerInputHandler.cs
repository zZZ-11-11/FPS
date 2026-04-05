using FPS.Game;
using FPS.Game.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS.GamePlay.Managers
{
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        [Tooltip("相机移动灵敏度")]
        public float lookSensitivity = 1f;

        [Tooltip("WebGL附加灵敏度系数")]
        public float webglLookSensitivityMultiplier = 0.25f;

        [Tooltip("摇杆死区")]
        public float triggerAxisThreshold = 0.4f;

        [Tooltip("翻转y轴")]
        public bool invertYAxis;

        [Tooltip("翻转x轴")]
        public bool invertXAxis;

        GameFlowManager m_GameFlowManager;
        PlayerCharacterController m_PlayerCharacterController;
        bool m_FireInputWasHeld;

        private InputAction m_MoveAction;
        private InputAction m_LookAction;
        private InputAction m_JumpAction;
        private InputAction m_FireAction;
        private InputAction m_AimAction;
        private InputAction m_SprintAction;
        private InputAction m_CrouchAction;
        private InputAction m_ReloadAction;
        private InputAction m_NextWeaponAction;

        void Start()
        {
            m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerInputHandler>(
                m_PlayerCharacterController, this, gameObject);
            m_GameFlowManager = FindFirstObjectByType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, PlayerInputHandler>(m_GameFlowManager, this);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_LookAction = InputSystem.actions.FindAction("Player/Look");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            m_FireAction = InputSystem.actions.FindAction("Player/Fire");
            m_AimAction = InputSystem.actions.FindAction("Player/Aim");
            m_SprintAction = InputSystem.actions.FindAction("Player/Sprint");
            m_CrouchAction = InputSystem.actions.FindAction("Player/Crouch");
            m_ReloadAction = InputSystem.actions.FindAction("Player/Reload");
            m_NextWeaponAction = InputSystem.actions.FindAction("Player/NextWeapon");

            m_MoveAction.Enable();
            m_LookAction.Enable();
            m_JumpAction.Enable();
            m_FireAction.Enable();
            m_AimAction.Enable();
            m_SprintAction.Enable();
            m_CrouchAction.Enable();
            m_ReloadAction.Enable();
            m_NextWeaponAction.Enable();
        }

        void LateUpdate()
        {
            m_FireInputWasHeld = GetFireInputHeld();
        }

        private bool CanProcessInput() => Cursor.lockState == CursorLockMode.Locked && !m_GameFlowManager.gameIsEnding;

        public Vector3 GetMoveInput()
        {
            if (CanProcessInput())
            {
                var input = m_MoveAction.ReadValue<Vector2>();
                var move = new Vector3(input.x, 0f, input.y);

                // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
                move = Vector3.ClampMagnitude(move, 1);

                return move;
            }

            return Vector3.zero;
        }

        public float GetLookInputsHorizontal()
        {
            if (!CanProcessInput())
            {
                return 0.0f;
            }

            var input = m_LookAction.ReadValue<Vector2>().x;

            if (invertXAxis)
            {
                input *= -1;
            }

            input *= lookSensitivity;

#if UNITY_WEBGL
            // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
            input *= WebglLookSensitivityMultiplier;
#endif

            return input;
        }

        public float GetLookInputsVertical()
        {
            if (!CanProcessInput())
            {
                return 0.0f;
            }

            var input = m_LookAction.ReadValue<Vector2>().y;

            if (invertYAxis)
            {
                input *= -1;
            }

            input *= lookSensitivity;

#if UNITY_WEBGL
            // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
            input *= WebglLookSensitivityMultiplier;
#endif

            return input;
        }

        public bool GetJumpInputDown()
        {
            if (CanProcessInput())
            {
                return m_JumpAction.WasPressedThisFrame();
            }

            return false;
        }

        public bool GetJumpInputHeld()
        {
            if (CanProcessInput())
            {
                return m_JumpAction.IsPressed();
            }

            return false;
        }

        public bool GetFireInputDown() => GetFireInputHeld() && !m_FireInputWasHeld;

        public bool GetFireInputReleased() => !GetFireInputHeld() && m_FireInputWasHeld;

        public bool GetFireInputHeld()
        {
            if (CanProcessInput())
            {
                return m_FireAction.IsPressed();
            }

            return false;
        }

        public bool GetAimInputHeld()
        {
            if (CanProcessInput())
            {
                return m_AimAction.IsPressed();
            }

            return false;
        }

        public bool GetSprintInputHeld()
        {
            if (CanProcessInput())
            {
                return m_SprintAction.IsPressed();
            }

            return false;
        }

        public bool GetCrouchInputDown()
        {
            if (CanProcessInput())
            {
                return m_CrouchAction.WasPressedThisFrame();
            }

            return false;
        }

        public bool GetCrouchInputReleased()
        {
            if (CanProcessInput())
            {
                return m_CrouchAction.WasReleasedThisFrame();
            }

            return false;
        }

        public bool GetReloadButtonDown()
        {
            if (CanProcessInput())
            {
                return m_ReloadAction.WasPressedThisFrame();
            }

            return false;
        }

        public int GetSwitchWeaponInput()
        {
            if (CanProcessInput())
            {
                var input = m_NextWeaponAction.ReadValue<float>();

                if (input > 0f)
                    return -1;

                if (input < 0f)
                    return 1;
            }

            return 0;
        }

        public int GetSelectWeaponInput()
        {
            if (CanProcessInput())
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame)
                    return 1;
                if (Keyboard.current.digit2Key.wasPressedThisFrame)
                    return 2;
                if (Keyboard.current.digit3Key.wasPressedThisFrame)
                    return 3;
                if (Keyboard.current.digit4Key.wasPressedThisFrame)
                    return 4;
                if (Keyboard.current.digit5Key.wasPressedThisFrame)
                    return 5;
                if (Keyboard.current.digit6Key.wasPressedThisFrame)
                    return 6;
                if (Keyboard.current.digit7Key.wasPressedThisFrame)
                    return 7;
                if (Keyboard.current.digit8Key.wasPressedThisFrame)
                    return 8;
                if (Keyboard.current.digit9Key.wasPressedThisFrame)
                    return 9;
            }

            return 0;
        }

        /// <summary>
        /// 切换到 UI 模式
        /// </summary>
        public void EnableUIMode()
        {
            InputSystem.actions.FindActionMap("Player").Disable();
            InputSystem.actions.FindActionMap("UI").Enable();
            // 如果需要显示鼠标
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        ///  恢复到玩家模式
        /// </summary>
        public void EnablePlayerMode()
        {
            InputSystem.actions.FindActionMap("UI").Disable();
            InputSystem.actions.FindActionMap("Player").Enable();
            // 隐藏并锁定鼠标（FPS游戏常用）
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
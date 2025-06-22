using UnityEngine;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PlayerControl
{
	public class PlayerInputSystem : MonoBehaviour
	{
        public PlayerStats playerStats;
        public enum PlayerMode { Combat, BuildMenu, Placing }
        public PlayerMode currentMode = PlayerMode.Combat;
        // 模式切换事件
        public static event Action<PlayerMode> OnModeChanged;

        [Header("Character Input Values")]
        [HideInInspector] public Vector2 move;
        [HideInInspector] public Vector2 look;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool sprint;
        [HideInInspector] public bool aim;
        [HideInInspector] public bool pickUp;
        [HideInInspector] public bool openStatusPanel;
        [HideInInspector] public bool ping;
        [HideInInspector] public bool reload; // 添加换弹输入

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")] // 如果cursorLocked = true，SetCursorState(true) 就会让鼠标隐藏+锁定在屏幕中央；否则恢复正常鼠标
        /*
         * 是否允许通过鼠标移动来控制角色视角/镜头。如果是true，OnLook()输入事件才响应鼠标操作，
         * 适合“战斗/射击”状态。进入建筑/菜单/对话/操作UI等模式时一般会关闭（false）。
         */
        public bool cursorInputForLook = true;

        [Header("Voice Input Settings")]
        public bool voiceInput; // true 表示正在按下语音键

        [Header("Cancel Input")]
        [HideInInspector] public bool cancelPressed;

        [Header("Shooting Input")]
        public InputActionReference shootAction;
        [HideInInspector] public bool shootPressed;
        [HideInInspector] public bool shootHeld;
        [HideInInspector] public bool shootReleased;

        // 添加内部状态跟踪
        private bool _lastFrameShootHeld = false;
        public bool IsShootHeld => shootAction.action.IsPressed();

        [Header("Building System")]
        [SerializeField] private BuildingSystem buildingSystem;
        private void Awake()
        {
            // 射击输入处理
            shootAction.action.Enable();
        }

        private void Start()
        {
            // 确保UI Action Map始终启用
            var inputActions = GetComponent<PlayerInput>().actions;
            inputActions.FindActionMap("UI").Enable();
        }

        private void Update()
        {
            // 更新射击输入状态
            UpdateShootingInput();
            if (currentMode == PlayerMode.Placing)
            {
                HandlePlacingInput();
            }
            Debug.Log("当前输入模式: " + currentMode);

        }
        private void UpdateShootingInput()
        {
            // 直接从 Input Action 获取当前状态
            bool currentFrameShootHeld = shootAction.action.IsPressed();

            // 计算瞬时状态
            shootPressed = currentFrameShootHeld && !_lastFrameShootHeld;
            shootReleased = !currentFrameShootHeld && _lastFrameShootHeld;
            shootHeld = currentFrameShootHeld;

            // 更新上一帧状态
            _lastFrameShootHeld = currentFrameShootHeld;
        }
#if ENABLE_INPUT_SYSTEM

        #region Player regular Keyboard Input
        public void OnMove(InputValue value)
		{
            MoveInput(value.Get<Vector2>());
        }

		public void OnLook(InputValue value)
		{
            if (cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
            JumpInput(value.isPressed);
        }

		public void OnSprint(InputValue value)
		{
            SprintInput(value.isPressed);
        }

        public void OnAim(InputValue value)
        {
            if (currentMode != PlayerMode.Combat) return; // 只有战斗模式下才能瞄准
            AimInput(value.isPressed);
        }

        //public void OnShoot(InputValue value)
        //{

        //    ShootInput(value.isPressed);
        //}

        public void OnVoiceInput(InputValue value)
        {
            VoiceInput(value.isPressed);
        }

        public void OnPickUp(InputValue value)
        {
            if (currentMode != PlayerMode.Combat) return; // 只有战斗模式下才能拾取
            PickUpInput(value.isPressed);
        }

        public void OnOpenStatusPanel(InputValue value)
        {
            OpenStatusPanelInput(value.isPressed);
        }

        public void OnOpenBuildMenu(InputValue value)
        {
            OpenBuildMenuInput(value.isPressed);
        }

        public void OnPing(InputValue value)
        {
            PingInput(value.isPressed);
        }

        // 添加取消输入处理
        public void OnCancel(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("调用Cancel!!!!");
                cancelPressed = true;
                HandleCancelInput();
            }
        }
        public void OnReload(InputValue value)
        {
            if (currentMode != PlayerMode.Combat) return; // 只在战斗模式下允许换弹
            ReloadInput(value.isPressed);
        }
        #endregion

#endif
        // 处理取消输入的逻辑
        private void HandleCancelInput()
        {
            switch (currentMode)
            {
                case PlayerMode.BuildMenu:
                    EnterCombatMode();
                    break;
                case PlayerMode.Placing:
                    EnterCombatMode();
                    break;
                case PlayerMode.Combat:
                    // 在战斗模式下，ESC 可以打开暂停菜单或其他UI
                    break;
            }
        }

        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
            look = newLookDirection;
        }

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
            if (newSprintState && playerStats.GetCurrentStamina() < 2f)
            {
                // 体力不足3时禁止冲刺
                sprint = false;
                return;
            }

            sprint = newSprintState;
            
            // 控制体力衰减
            if (newSprintState)
            {
                // 开始冲刺，启动体力衰减
                playerStats.StopStaminaRecover();
                playerStats.StartStaminaDecay();
            }
            else
            {
                // 停止冲刺，停止体力衰减
                playerStats.StopStaminaDecay();
                playerStats.StartStaminaRecover();
            }
        }

		//鼠标右键瞄准
        public void AimInput(bool newAimState)
        {
            aim = newAimState;
        }

        //public void ShootInput(bool newShootState)
        //{
        //    shoot = newShootState;
        //}

        public void VoiceInput(bool newVoiceInputState)
        {
            voiceInput = newVoiceInputState;
        }

		public void PickUpInput(bool newPickUpInputState)
		{
			pickUp = newPickUpInputState;

        }

        public void OpenStatusPanelInput(bool newStatusPanelInputState)
        {
            openStatusPanel = newStatusPanelInputState;

        }

        public void PingInput(bool newPingInputState)
        {
            ping = newPingInputState;

        }

        public void ReloadInput(bool newReloadState)
        {
            if (newReloadState) // 只在按下时触发
            {
                reload = true;
            }
        }

        /*
         * 输入模式切换
         * 摄像机Look/角色转向/瞄准/射击等操作，都应该用currentMode判断：只在Combat和Placing响应，BuildMenu时全部禁止。
         * WASD移动在所有模式都响应（不禁止）。
         */
        public void OpenBuildMenuInput(bool newState)
        {
            // B键只在战斗/建筑菜单间切换
            if (newState)
            {
                if (currentMode == PlayerMode.Combat)
                    EnterBuildMenu();
                else if (currentMode == PlayerMode.BuildMenu)
                    EnterCombatMode();
                // 在Placing模式下不响应B（只能右键/esc退出）
            }
        }
        public void EnterBuildMenu()
        {
            PlayerMode previousMode = currentMode;
            currentMode = PlayerMode.BuildMenu;
            cursorInputForLook = false;

            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.ShowCursor();
            }

            // 触发模式切换事件
            OnModeChanged?.Invoke(currentMode);
        }

        public void EnterCombatMode()
        {
            PlayerMode previousMode = currentMode;
            currentMode = PlayerMode.Combat;

            cursorInputForLook = true;

            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.HideCursor();
            }

            // 重置所有输入状态
            ResetInputStates();

            // 触发模式切换事件
            OnModeChanged?.Invoke(currentMode);
        }

        public void EnterPlacingMode()
        {
            PlayerMode previousMode = currentMode;
            currentMode = PlayerMode.Placing;

            cursorInputForLook = true;

            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.HideCursor();
            }

            // 触发模式切换事件
            OnModeChanged?.Invoke(currentMode);
        }

        // 重置输入状态
        private void ResetInputStates()
        {
            aim = false;
            jump = false;
            sprint = false;
            pickUp = false;
            shootPressed = false;
            shootHeld = false;
            shootReleased = false;
        }

        /// <summary>
        /// 处理放置模式下的输入
        /// </summary>
        private void HandlePlacingInput()
        {
            if (buildingSystem == null || !buildingSystem.IsPlacing()) return;

            if (shootPressed)
            {
                TurretData_SO buildingData = buildingSystem.GetCurrentBuildingData();
                if (buildingData != null)
                {
                    // 检查是否在营地内
                    Vector3 buildPos = buildingSystem.GetCurrentPreview().transform.position;
                    bool isInCamp = CampZoneManager.Instance?.IsPositionInCamp(buildPos) ?? false;
                    if (!isInCamp)
                    {
                        Debug.Log("无法建造：必须在营地范围内建造防御塔");
                        // 不进入战斗模式，继续保持放置模式
                        return;
                    }

                    // 检查扣除资源
                    InventoryManager inventory = FindObjectOfType<InventoryManager>();
                    bool resourceConsumed = inventory.TryConsuming(
                        buildingData.requiredWoodNum,
                        buildingData.requiredIronNum
                    );

                    if (resourceConsumed)
                    {
                        buildingSystem.ConfirmPlacement();
                        EnterCombatMode();
                    }
                    else
                    {
                        Debug.Log("资源不足，无法放置建筑");
                    }
                }
            }

            // 右键取消放置
            if (Input.GetMouseButtonDown(1)) // 检测鼠标右键
            {
                Debug.Log("[PlayerInputSystem] 右键取消放置");
                buildingSystem.CancelPlacement();

                // 取消后返回战斗模式
                EnterCombatMode();
            }
        }

        // 公共方法，供其他脚本调用
        public bool IsInCombatMode() => currentMode == PlayerMode.Combat;
        public bool IsInBuildMenuMode() => currentMode == PlayerMode.BuildMenu;
        public bool IsInPlacingMode() => currentMode == PlayerMode.Placing;

        // 在LateUpdate中重置换弹输入
        private void LateUpdate()
        {
            // 重置一次性输入事件（这些应该只在触发的那一帧为true）
            shootPressed = false;
            shootReleased = false;
            reload = false; // 重置换弹输入
        }
    }

}
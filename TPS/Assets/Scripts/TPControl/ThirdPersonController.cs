using UnityEngine;
using UnityEngine.Animations.Rigging;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/*
 Note: animations are called via the controller for both the character and capsule using animator null checks
 动画通过控制器调用，使用 animator 空检查来处理角色和胶囊体的动画
*/

namespace PlayerControl
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")] // 玩家属性
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f; // 移动速度 (米/秒)

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f; // 冲刺速度 (米/秒)

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f; // 角色朝向移动方向旋转的平滑时间

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f; // 加速和减速速率

        public AudioClip LandingAudioClip; // 着地音效
        public AudioClip[] FootstepAudioClips; // 脚步音效列表
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f; // 脚步音量

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 0.8f; // 跳跃高度

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f; // 自定义重力值

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f; // 跳跃冷却时间

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f; // 下落状态切换时间

        [Header("Player Grounded")] // 地面检测属性
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true; // 是否在地面上

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f; // 地面检测高度偏移

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f; // 地面检测球体半径

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers; // 地面图层

        [Header("Cinemachine")] // 摄像机属性
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget; // Cinemachine 摄像机跟随目标

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f; // 摄像机向上最大仰角

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f; // 摄像机向下最大俯角

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f; // 摄像机角度偏移

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false; // 是否锁定摄像机位置

        // 私有变量
        private float _cinemachineTargetYaw; // 摄像机 Y 轴旋转角度
        private float _cinemachineTargetPitch; // 摄像机 X 轴旋转角度
        private float LookSensitivity; // 鼠标/摇杆灵敏度

        private float _speed; // 当前速度
        private float _animationBlend; // 动画混合参数
        private float _targetRotation = 0.0f; // 目标旋转角度
        private float _rotationVelocity; // 旋转速度（平滑计算器）
        private float _verticalVelocity; // 垂直速度，用于跳跃和重力
        private float _terminalVelocity = 53.0f; // 最大下落速度
        private float smoothSpeed = 10f; // rig平滑切换速度参数

        private float _jumpTimeoutDelta; // 跳跃冷却倒计时
        private float _fallTimeoutDelta; // 下落倒计时

        // 动画参数 ID
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        /*冲刺*/
        private int _animIDStartSprint;
        private int _animIDStopSprint;
        private bool _wasRunning = false;
        /*射击*/
        private int _animIDStartShooting;
        private int _animIDStartAutoFire;
        private int _animIDStopShooting;
        private bool _wasShootPressed = false;
        private bool _wasShootHeld = false;
        private bool _isInAutoFire = false;
        /*瞄准*/
        private int _animIDIsAiming;
        private bool _wasAiming = false;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput; // 新输入系统对象
#endif
        private Animator _animator; // 动画控制器
        private CharacterController _controller; // 角色控制器组件
        private PlayerInputSystem _playerInputs; // 自定义输入处理脚本
        private GameObject _mainCamera; // 主摄像机引用

        private const float _threshold = 0.01f; // 判断输入阈值

        private bool _hasAnimator; // 是否存在 Animator 组件

        private bool _rotateOnMove; // 是否在移动时旋转角色

        float sprintTimer = 0f; // 奔跑时间计时器

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse"; // 当前是否为键鼠输入
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            // 获取主摄像机引用 (通过 Tag 查找)
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            // 初始化摄像机初始旋转角度
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator); // 尝试获取 Animator 组件
            _controller = GetComponent<CharacterController>(); // 获取 CharacterController 组件
            _playerInputs = GetComponent<PlayerInputSystem>(); // 获取输入处理脚本
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>(); // 获取 PlayerInput 组件
#else
#endif

            AssignAnimationIDs(); // 分配动画参数 ID

            // 重置跳跃和下落倒计时
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // 订阅模式切换事件
            PlayerInputSystem.OnModeChanged += OnPlayerModeChanged;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator); // 每帧更新 Animator 状态

            JumpAndGravity(); // 处理跳跃和重力
            GroundedCheck(); // 检测地面
            Move(); // 处理移动
            //UpdateRigWeights(); // 处理rig权重切换
        }

        private void LateUpdate()
        {
            CameraRotation(); // 在所有 Update 之后处理摄像机旋转
        }

        private void AssignAnimationIDs()
        {
            // 使用字符串哈希获取动画参数 ID，以提高效率
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDStartSprint = Animator.StringToHash("StartTrigger");
            _animIDStopSprint = Animator.StringToHash("StopTrigger");

            // 射击动画参数
            _animIDStartShooting = Animator.StringToHash("StartShooting");
            _animIDStartAutoFire = Animator.StringToHash("StartAutoFire");
            _animIDStopShooting = Animator.StringToHash("StopShooting");
            _animIDIsAiming = Animator.StringToHash("IsAiming");
        }

        private void GroundedCheck()
        {
            // 计算地面检测球体位置 (角色位置 + 偏移)
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            // 使用 Physics.CheckSphere 检测地面
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            // 更新 Animator 中的 "Grounded" 参数
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // 如果有输入且摄像机未被锁定，则根据输入旋转摄像机目标
            if (_playerInputs.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // 鼠标输入不乘以 Time.deltaTime，摇杆输入乘以
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _playerInputs.look.x * deltaTimeMultiplier * LookSensitivity;
                _cinemachineTargetPitch += _playerInputs.look.y * deltaTimeMultiplier * LookSensitivity;
            }

            // 限制摄像机俯仰角度在规定范围内
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // 应用旋转到 Cinemachine 目标
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f);
        }

        private void Move()
        {
            // 根据是否按下冲刺键选择目标速度
            float targetSpeed = _playerInputs.sprint ? SprintSpeed : MoveSpeed;

            if (_playerInputs.sprint)
            {
                sprintTimer += Time.deltaTime;
            }else
            {
                sprintTimer = 0f;
            }
            // 如果没有移动输入，则目标速度设为 0
            if (_playerInputs.move == Vector2.zero) targetSpeed = 0.0f;

            // 获取当前水平速度 (忽略垂直分量)
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f; // 速度误差阈值
            float inputMagnitude = _playerInputs.analogMovement ? _playerInputs.move.magnitude : 1f; // 模拟摇杆时使用输入幅度

            // 加速或减速到目标速度
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // 使用 Lerp 平滑过渡速度
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                // 四舍五入保留三位小数
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // 动画速度混合
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 计算输入方向 (归一化)
            Vector3 inputDirection = new Vector3(_playerInputs.move.x, 0.0f, _playerInputs.move.y).normalized;

            // 如果有移动输入则旋转角色朝向移动方向
            if (_playerInputs.move != Vector2.zero)
            {
                // 目标旋转角度 = 输入方向角度 + 摄像机 Y 轴角度
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                // 平滑计算旋转
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                // 如果允许移动旋转，则应用旋转给角色
                if (_rotateOnMove)
                {
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }

            // 计算移动方向向量
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // 执行移动，包含水平移动和垂直重力/跳跃
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            /*混合奔跑的开始和结束*/
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);

                bool isRunning = _playerInputs.sprint && _playerInputs.move.magnitude > 0.1f && Grounded;

                // 只在状态真正改变时触发
                if (isRunning != _wasRunning)
                {
                    if (isRunning)
                    {
                        _animator.SetTrigger(_animIDStartSprint);
                    }
                    if (!isRunning && sprintTimer > 1f)
                    {
                        _animator.SetTrigger(_animIDStopSprint);
                    }
                    _wasRunning = isRunning;
                }
                // 瞄准动画控制
                HandleAimingAnimation();
            }
            // 射击动画控制
            HandleShootingAnimation();
        }
        private void HandleAimingAnimation()
        {
            bool isAiming = _playerInputs.aim;

            // 只在状态改变时更新
            if (isAiming != _wasAiming)
            {
                _animator.SetBool(_animIDIsAiming, isAiming);
                _wasAiming = isAiming;
            }
        }

        private void HandleShootingAnimation()
        {
            bool shootPressed = _playerInputs.shootPressed;
            bool shootHeld = _playerInputs.shootHeld;
            bool shootReleased = _playerInputs.shootReleased;
            Debug.Log($"[ANIMATION] Frame: {Time.frameCount}, Pressed: {shootPressed}, Held: {shootHeld}, Released: {shootReleased}");
            // 获取武器信息
            WeaponManager weaponManager = GetComponent<ThirdPersonShooterController>()?.weapon;
            bool isAutomatic = weaponManager != null ? weaponManager.weaponData.isAutomatic : false;

            // 获取动画状态
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(1);
            bool isInSingleShot = currentState.IsName("Rifle_ShootOnce");
            bool isInAutoFire = currentState.IsName("Rifle_ShootLoop");

            if (isAutomatic)
            {
                // 开始射击
                if (shootPressed && !_wasShootPressed)
                {
                    _animator.SetTrigger(_animIDStartShooting);
                    _isInAutoFire = false;
                }
                // 转为连发
                else if (shootHeld && !_isInAutoFire && isInSingleShot)
                {
                    if (currentState.normalizedTime > 0.5f)
                    {
                        _animator.SetTrigger(_animIDStartAutoFire);
                        _isInAutoFire = true;
                    }
                }
                // 停止射击
                else if (shootReleased && _isInAutoFire)
                {
                    _animator.SetTrigger(_animIDStopShooting);
                    _isInAutoFire = false;
                }
            }
            else
            {
                // 单发武器
                if (shootPressed && !_wasShootPressed)
                {
                    _animator.SetTrigger(_animIDStartShooting);
                }
            }

            _wasShootPressed = shootPressed;
            _wasShootHeld = shootHeld;
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // 在地面时，重置下落倒计时
                _fallTimeoutDelta = FallTimeout;

                // 更新动画参数：跳跃和自由下落为 false
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // 地面上时，防止垂直速度无限下降
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // 如果按下跳跃键且冷却到期，则跳跃
                if (_playerInputs.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // 计算达到指定高度所需的初始垂直速度
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // 更新动画参数：跳跃开始
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // 跳跃冷却倒计时递减
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // 不在地面时，重置跳跃冷却
                _jumpTimeoutDelta = JumpTimeout;

                // 下落倒计时
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // 倒计时结束，触发自由下落动画
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // 非地面时取消跳跃输入
                _playerInputs.jump = false;
            }

            // 如果未达到终端速度，则累加重力
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            // 规范化角度到 -360 ~ 360
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            // 限制到指定范围
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            // 绘制地面检测球体的 Gizmo，用于调试
            Color transparentGreen = new Color(0.0f, 1.0f, 0.35f, 0.35f); // 地面时绿色
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f); // 非地面时红色

            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            // 在动画事件中触发脚步音效
            if (animationEvent.animatorClipInfo.weight > 0.5f && FootstepAudioClips.Length > 0)
            {
                int index = Random.Range(0, FootstepAudioClips.Length); // 随机选择音效
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            // 在着地动画事件中触发着地音效
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        /// <summary>
        /// 设置鼠标/摇杆灵敏度
        /// </summary>
        public void setLookSensitivity(float newSensitivity)
        {
            LookSensitivity = newSensitivity; // 更新灵敏度
        }

        /// <summary>
        /// 设置是否在移动时旋转角色
        /// </summary>
        public void SetRotateOnMove(bool newRotateOnMove)
        {
            _rotateOnMove = newRotateOnMove; // 更新旋转开关
        }

        private void OnDestroy()
        {
            // 取消订阅事件，防止内存泄漏
            PlayerInputSystem.OnModeChanged -= OnPlayerModeChanged;
        }

        private void OnPlayerModeChanged(PlayerInputSystem.PlayerMode newMode)
        {
            switch (newMode)
            {
                case PlayerInputSystem.PlayerMode.Combat:
                    // 启用正常移动
                    enabled = true;
                    break;
                case PlayerInputSystem.PlayerMode.BuildMenu:
                    enabled = true;
                    // 可以选择完全禁用组件或只是不处理移动输入
                    break;
                case PlayerInputSystem.PlayerMode.Placing:
                    // 在放置模式下启用移动但可能需要不同的行为
                    enabled = true;
                    break;
            }
        }
    }
}

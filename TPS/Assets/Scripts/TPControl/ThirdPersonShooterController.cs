using Cinemachine;
using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class ThirdPersonShooterController : MonoBehaviour
{
    [Header("Aim Camera Settings")]
    [SerializeField] private float aimTopClamp = 45.0f;    // 瞄准时向上最大角度
    [SerializeField] private float aimBottomClamp = -10.0f; // 瞄准时向下最大角度
    [SerializeField] private CinemachineVirtualCamera _aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();

    [Header("Rig")]
    [SerializeField] private Rig aimWeapon;
    [SerializeField] private Rig aimHandIK;
    [SerializeField] private Rig aimBody;
    [SerializeField] private Rig idleWeapon;
    [SerializeField] private Rig idleHandIK;
    [SerializeField] private Rig reloading;
    [SerializeField] private TwoBoneIKConstraint aimLeftHandIK; // 拖拽Aim_LeftHandRig上的Two Bone IK组件
    [SerializeField] private TwoBoneIKConstraint idleLeftHandIK; // 拖拽Aim_LeftHandRig上的Two Bone IK组件
    [Header("Weapon")]
    [SerializeField] public WeaponManager weapon;

    private PlayerInputSystem _playerInputs;
    private ThirdPersonController _thirdPersonController;

    private float _aimWeapon_Weight;
    private float _aimBody_Weight;
    private float _aimHandIK_Weight;
    private float _idleWeapon_Weight;
    private float _idleHandIK_Weight;
    private float _reloading_Weight;

    public GameObject aimTarget;

    public GameObject originShootPosition;

    private void Start()
    {
        // 订阅模式切换事件
        PlayerInputSystem.OnModeChanged += OnPlayerModeChanged;
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        PlayerInputSystem.OnModeChanged -= OnPlayerModeChanged;
    }

    private void Awake()
    {
        _playerInputs = GetComponent<PlayerInputSystem>();
        _thirdPersonController = GetComponent<ThirdPersonController>();
    }
    void Update()
    {
        //_playerInputs.aim = true;
        // 只在战斗模式下执行射击逻辑
        if (_playerInputs.currentMode != PlayerInputSystem.PlayerMode.Combat)
            return;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        /*鼠标所指*/
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            float hitDistanceFromCamera = Vector3.Distance(Camera.main.transform.position, raycastHit.point);
            float safeDistance = 2.5f;

            if (hitDistanceFromCamera > safeDistance)
            {
                // 距离相机足够远，正常设置
                aimTarget.transform.position = raycastHit.point;
            }
            else
            {
                // 距离太近，延长
                Debug.Log("命中点离相机太近，延长至安全距离");
                Vector3 direction = ray.direction.normalized;
                Vector3 safePosition = Camera.main.transform.position + direction * safeDistance;
                aimTarget.transform.position = safePosition;
            }
        }

        /*是否能开启瞄准*/
        UpdateRigWeights();

        IfAiming(raycastHit);

        // 添加换弹输入处理
        HandleReloadInput();
    }

    private void ForceExitAiming()
    {
        // 强制退出瞄准状态的所有效果
        _aimVirtualCamera.gameObject.SetActive(false);
        _thirdPersonController.setLookSensitivity(normalSensitivity);
        _thirdPersonController.SetRotateOnMove(true);
        _thirdPersonController.RestoreNormalCameraClamps();
    }

    private void IfAiming(RaycastHit raycastHit)
    {
        if (_playerInputs.aim)
        {
            _aimVirtualCamera.gameObject.SetActive(true);// 镜头放大
            _thirdPersonController.setLookSensitivity(aimSensitivity);// 降低Aiming状态下的灵敏度
            _thirdPersonController.SetRotateOnMove(false);
            //_animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            // 设置瞄准时的相机角度限制
            _thirdPersonController.SetAimCameraClamps(aimTopClamp, aimBottomClamp);

            Vector3 worldAimTarget = raycastHit.point;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

            weapon.HandleShooting(
                shootPressed: _playerInputs.shootPressed,
                shootHeld: _playerInputs.shootHeld,
                shootReleased: _playerInputs.shootReleased,
                raycastHit);
        }
        else
        {
            ForceExitAiming();
        }
    }

    private void UpdateRigWeights()
    {
        // 检查是否正在换弹
        bool isReloading = weapon != null && weapon.IsReloading();
        // 换弹时的rig权重逻辑
        if (isReloading)
        {
            // 换弹期间：禁用所有瞄准相关的rig，启用idle rig
            _aimWeapon_Weight = _playerInputs.aim ? 1f : 0f;
            _aimBody_Weight = _playerInputs.aim ? 1f : 0f;
            _aimHandIK_Weight = 0f;        // 换弹时禁用瞄准HandIK
            _idleWeapon_Weight = _playerInputs.aim ? 0f : 1f;
            _idleHandIK_Weight = 0f;       // 换弹时禁用idle HandIK
            // 单独控制左手Two Bone IK权重
            if (aimLeftHandIK != null)
            {
                aimLeftHandIK.weight = 0f; // 换弹时禁用左手IK
            }
            if (idleLeftHandIK != null)
            {
                idleLeftHandIK.weight = 0f;
            }
        }
        else
        {
            // 正常情况：根据瞄准状态设置rig权重
            _aimWeapon_Weight = _playerInputs.aim ? 1f : 0f;
            _aimHandIK_Weight = _playerInputs.aim ? 1f : 0f;
            _aimBody_Weight = _playerInputs.aim ? 1f : 0f;
            _idleWeapon_Weight = _playerInputs.aim ? 0f : 1f;
            _idleHandIK_Weight = _playerInputs.aim ? 0f : 1f;
            // 恢复左手Two Bone IK权重
            if (aimLeftHandIK != null)
            {
                aimLeftHandIK.weight = _playerInputs.aim ? 1f : 0f;
            }
            if (idleLeftHandIK != null)
            {
                idleLeftHandIK.weight = _playerInputs.aim ? 0f : 1f;
            }
        }

        // 应用权重变化（保持原有的平滑过渡）
        aimWeapon.weight = Mathf.Lerp(aimWeapon.weight, _aimWeapon_Weight, Time.deltaTime * 20f);
        aimHandIK.weight = Mathf.Lerp(aimHandIK.weight, _aimHandIK_Weight, Time.deltaTime * 20f);
        aimBody.weight = Mathf.Lerp(aimBody.weight, _aimBody_Weight, Time.deltaTime * 20f);
        idleWeapon.weight = Mathf.Lerp(idleWeapon.weight, _idleWeapon_Weight, Time.deltaTime * 20f);
    }

    private void OnPlayerModeChanged(PlayerInputSystem.PlayerMode newMode)
    {
        switch (newMode)
        {
            case PlayerInputSystem.PlayerMode.Combat:
                // 启用射击控制
                enabled = true;
                break;
            case PlayerInputSystem.PlayerMode.BuildMenu:
            case PlayerInputSystem.PlayerMode.Placing:
                // 在建筑相关模式下禁用射击
                enabled = false;
                // 重置瞄准状态
                if (_aimVirtualCamera != null)
                {
                    _aimVirtualCamera.gameObject.SetActive(false);
                }
                break;
        }
    }

    // 新增换弹输入处理方法
    private void HandleReloadInput()
    {
        if (_playerInputs.reload && weapon != null)
        {
            // 尝试换弹
            weapon.StartReload();
        }
    }
}

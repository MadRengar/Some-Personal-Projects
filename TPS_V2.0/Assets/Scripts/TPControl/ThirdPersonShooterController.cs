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
    [Header("Reference")]
    public GameObject aimTarget;
    public GameObject originShootPosition;
    [SerializeField] private CameraController cameraController;

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

    [Header("Aim Target Adjustment")]
    [SerializeField] private Vector3 aimTargetOffset = Vector3.zero; // 在Inspector中调整
    [SerializeField] private bool useManualOffset = true;

    [Header("Animation Layers")]
    [SerializeField] private int rifleLayerIndex = 1;      // Base Layer
    [SerializeField] private int aimingLayerIndex = 2;    // Aiming Layer  
    [SerializeField] private int reloadLayerIndex = 3;    // Reload Layer
    [SerializeField] private int hammerLayerIndex = 4;    // Hammer Layer
    [SerializeField] private int hammerSwingLayerIndex = 5;    // Hammer Layer

    private PlayerInputSystem _playerInputs;
    private ThirdPersonController _thirdPersonController;

    private float _aimWeapon_Weight;
    private float _aimBody_Weight;
    private float _aimHandIK_Weight;
    private float _idleWeapon_Weight;
    private float _idleHandIK_Weight;
    private float _reloading_Weight;

    private WeaponType currentWeaponType = WeaponType.Rifle;
    private void Start()
    {
        GameManager.OnPlayerDeath += OnPlayerDeath;
        cameraController.InitializeDeathCamera();
        WeaponSwitcher.OnWeaponChanged += OnWeaponChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnPlayerDeath -= OnPlayerDeath;
        WeaponSwitcher.OnWeaponChanged -= OnWeaponChanged;
    }

    private void Awake()
    {
        _playerInputs = GetComponent<PlayerInputSystem>();
        _thirdPersonController = GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        // 如果玩家已死亡，不执行任何射击控制逻辑
        if (GameManager.Instance.IsGameOver()) return;
        // 只在战斗模式下执行射击逻辑
        if (_playerInputs.currentMode != PlayerInputSystem.PlayerMode.Combat) return;

        if (currentWeaponType != WeaponType.Rifle) return;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        /*鼠标所指*/
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            aimTarget.transform.position = raycastHit.point + aimTargetOffset;            
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

    // 新增换弹输入处理方法
    private void HandleReloadInput()
    {
        if (_playerInputs.reload && weapon != null)
        {
            // 尝试换弹
            weapon.StartReload();
        }
    }

    private void OnPlayerDeath()
    {
        // 立即清空所有Rig权重
        ClearAllRigWeights();

        // 强制退出瞄准状态
        ForceExitAiming();

        // 禁用武器相关功能
        if (weapon != null)
        {
            // 停止所有射击行为
            // weapon.StopShooting(); // 如果武器管理器有这个方法的话
        }
        // 启动死亡摄像机序列
        cameraController.StartDeathCameraSequence();

        //Debug.Log("[玩家死亡事件]：ThirdPersonShooterController 清空所有Rig权重");
    }

    private void ClearAllRigWeights()
    {
        // 立即将所有权重设为0
        if (aimWeapon != null) aimWeapon.weight = 0f;
        if (aimHandIK != null) aimHandIK.weight = 0f;
        if (aimBody != null) aimBody.weight = 0f;
        if (idleWeapon != null) idleWeapon.weight = 0f;
        if (idleHandIK != null) idleHandIK.weight = 0f;
        if (reloading != null) reloading.weight = 0f;

        // 清空IK约束权重
        if (aimLeftHandIK != null) aimLeftHandIK.weight = 0f;
        if (idleLeftHandIK != null) idleLeftHandIK.weight = 0f;

        // 重置内部权重变量
        _aimWeapon_Weight = 0f;
        _aimBody_Weight = 0f;
        _aimHandIK_Weight = 0f;
        _idleWeapon_Weight = 0f;
        _idleHandIK_Weight = 0f;
        _reloading_Weight = 0f;
    }

    private void OnWeaponChanged(WeaponType newWeaponType)
    {
        currentWeaponType = newWeaponType;

        switch (newWeaponType)
        {
            case WeaponType.Rifle:
                SwitchToRifleMode();
                break;

            case WeaponType.Hammer:
                SwitchToHammerMode();
                break;
        }

        //Debug.Log($"[ThirdPersonShooterController] 武器切换为: {newWeaponType}");
    }

    private void SwitchToRifleMode()
    {
        // 启用步枪相关层
        SetAnimationLayerWeight(rifleLayerIndex, 1f);
        SetAnimationLayerWeight(aimingLayerIndex, 1f);
        SetAnimationLayerWeight(reloadLayerIndex, 1f);

        // 禁用锤子层
        SetAnimationLayerWeight(hammerLayerIndex, 0f);
        SetAnimationLayerWeight(hammerSwingLayerIndex, 0f);

        RestoreRifleRigWeights();
        //Debug.Log("[ThirdPersonShooterController] 切换到步枪模式");
    }

    private void SwitchToHammerMode()
    {
        // 禁用步枪相关层
        SetAnimationLayerWeight(rifleLayerIndex, 0f);
        SetAnimationLayerWeight(aimingLayerIndex, 0f);
        SetAnimationLayerWeight(reloadLayerIndex, 0f);

        // 启用锤子层
        SetAnimationLayerWeight(hammerLayerIndex, 1f);
        SetAnimationLayerWeight(hammerSwingLayerIndex, 1f);
        // 清空所有步枪 Rig 权重
        ClearAllRifleRigWeights();

        // 强制退出瞄准状态
        ForceExitAiming();

        //Debug.Log("[ThirdPersonShooterController] 切换到锤子模式，清空所有步枪 Rig");
    }

    private void SetAnimationLayerWeight(int layerIndex, float weight)
    {
        var animator = GetComponent<Animator>();
        if (animator != null && layerIndex >= 0 && layerIndex < animator.layerCount)
        {
            animator.SetLayerWeight(layerIndex, weight);
        }
    }

    private void ClearAllRifleRigWeights()
    {
        // 立即将所有步枪 Rig 权重设为 0
        if (aimWeapon != null) aimWeapon.weight = 0f;
        if (aimHandIK != null) aimHandIK.weight = 0f;
        if (aimBody != null) aimBody.weight = 0f;
        if (idleWeapon != null) idleWeapon.weight = 0f;
        if (idleHandIK != null) idleHandIK.weight = 0f;
        if (reloading != null) reloading.weight = 0f;

        // 清空 IK 约束权重
        if (aimLeftHandIK != null) aimLeftHandIK.weight = 0f;
        if (idleLeftHandIK != null) idleLeftHandIK.weight = 0f;

        // 重置内部权重变量
        _aimWeapon_Weight = 0f;
        _aimBody_Weight = 0f;
        _aimHandIK_Weight = 0f;
        _idleWeapon_Weight = 0f;
        _idleHandIK_Weight = 0f;
        _reloading_Weight = 0f;
    }

    private void RestoreRifleRigWeights()
    {
        // 根据当前状态恢复正确的 Rig 权重
        bool isCurrentlyAiming = _playerInputs != null && _playerInputs.aim;
        bool isCurrentlyReloading = weapon != null && weapon.IsReloading();

        if (isCurrentlyReloading)
        {
            // 如果正在换弹，设置换弹状态的权重
            _aimWeapon_Weight = isCurrentlyAiming ? 1f : 0f;
            _aimBody_Weight = isCurrentlyAiming ? 1f : 0f;
            _aimHandIK_Weight = 0f;
            _idleWeapon_Weight = isCurrentlyAiming ? 0f : 1f;
            _idleHandIK_Weight = 0f;
        }
        else
        {
            // 正常状态的权重
            _aimWeapon_Weight = isCurrentlyAiming ? 1f : 0f;
            _aimHandIK_Weight = isCurrentlyAiming ? 1f : 0f;
            _aimBody_Weight = isCurrentlyAiming ? 1f : 0f;
            _idleWeapon_Weight = isCurrentlyAiming ? 0f : 1f;
            _idleHandIK_Weight = isCurrentlyAiming ? 0f : 1f;
        }

        // 立即应用这些权重
        if (aimWeapon != null) aimWeapon.weight = _aimWeapon_Weight;
        if (aimHandIK != null) aimHandIK.weight = _aimHandIK_Weight;
        if (aimBody != null) aimBody.weight = _aimBody_Weight;
        if (idleWeapon != null) idleWeapon.weight = _idleWeapon_Weight;
        if (idleHandIK != null) idleHandIK.weight = _idleHandIK_Weight;

        // 恢复 IK 约束权重
        if (aimLeftHandIK != null) aimLeftHandIK.weight = isCurrentlyAiming ? 1f : 0f;
        if (idleLeftHandIK != null) idleLeftHandIK.weight = isCurrentlyAiming ? 0f : 1f;

        //Debug.Log($"[ThirdPersonShooterController] 恢复步枪 Rig 权重 - 瞄准: {isCurrentlyAiming}, 换弹: {isCurrentlyReloading}");
    }
}

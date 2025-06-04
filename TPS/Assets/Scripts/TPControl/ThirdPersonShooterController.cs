using Cinemachine;
using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform pfBulletProjectile;
    [Header("Rig")]
    [SerializeField] private Rig aimWeapon;
    [SerializeField] private Rig aimBody;
    [SerializeField] private Rig idleWeapon;
    [Header("Weapon")]
    [SerializeField] public WeaponManager weapon;

    private PlayerInputSystem _playerInputs;
    private ThirdPersonController _thirdPersonController;
    private Animator _animator;

    private float _aimWeapon_Weight;
    private float _aimBody_Weight;
    private float _idleWeapon_Weight;

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
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        // 只在战斗模式下执行射击逻辑
        if (_playerInputs.currentMode != PlayerInputSystem.PlayerMode.Combat)
            return;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        UpdateRigWeights();

        /*鼠标所指*/
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            aimTarget.transform.position = raycastHit.point;
        }

        /*是否开启瞄准*/
        IfAiming(raycastHit);
        weapon.HandleShooting(
            shootPressed: _playerInputs.shootPressed,
            shootHeld: _playerInputs.shootHeld,
            shootReleased: _playerInputs.shootReleased, 
            raycastHit);

    }

    private void IfAiming(RaycastHit raycastHit)
    {
        if (_playerInputs.aim)
        {
            _aimVirtualCamera.gameObject.SetActive(true);// 镜头放大
            _thirdPersonController.setLookSensitivity(aimSensitivity);// 降低Aiming状态下的灵敏度
            _thirdPersonController.SetRotateOnMove(false);
            //_animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            Vector3 worldAimTarget = raycastHit.point; ;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            _aimVirtualCamera.gameObject.SetActive(false);
            _thirdPersonController.setLookSensitivity(normalSensitivity);
            _thirdPersonController.SetRotateOnMove(true);
            //_animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }
    }

    private void UpdateRigWeights()
    {
        _aimWeapon_Weight = _playerInputs.aim ? 1f : 0f;
        _idleWeapon_Weight = _playerInputs.aim ? 0f : 1f;
        _aimBody_Weight = _playerInputs.aim ? 1f : 0f;

        aimWeapon.weight = Mathf.Lerp(aimWeapon.weight, _aimWeapon_Weight, Time.deltaTime * 20f);
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
}

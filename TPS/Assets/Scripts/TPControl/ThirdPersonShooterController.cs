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
    [Header("Particle")]
    [SerializeField] private ParticleSystem[] muzzleFlash;
    [SerializeField] private ParticleSystem hitEffect;
    [Header("Rig")]
    [SerializeField] private Rig aimWeapon;
    [SerializeField] private Rig aimBody;
    [SerializeField] private Rig idleWeapon;

    private PlayerInputSystem _playerInputs;
    private ThirdPersonController _thirdPersonController;
    private Animator _animator;

    private float _aimWeapon_Weight;
    private float _aimBody_Weight;
    private float _idleWeapon_Weight;

    public GameObject aimTarget;
    public GameObject originShootPosition;

    private void Awake()
    {
        _playerInputs = GetComponent<PlayerInputSystem>();
        _thirdPersonController = GetComponent<ThirdPersonController>();
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
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
        IfShooting(raycastHit);

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

    private void IfShooting(RaycastHit raycastHit)
    {
        /*是否开火*/
        if (_playerInputs.shoot)
        {
            StartFiring(raycastHit);

            /*击中到僵尸*/
            if (raycastHit.collider.CompareTag("Enemy"))
            {
                Debug.Log("命中敌人");
                hitEffect.transform.position = raycastHit.point;
                hitEffect.transform.forward = raycastHit.normal;
                hitEffect.Emit(1);
            }
            else /*击中到某物*/
            {
                Debug.Log("Miss");
            }
            _playerInputs.shoot = false;
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

    private void StartFiring(RaycastHit raycastHit)
    {
        Vector3 shootDirection = (raycastHit.point - originShootPosition.transform.position).normalized;
        Debug.DrawLine(originShootPosition.transform.position, raycastHit.point, Color.red, 1.0f);

        foreach (var effect in muzzleFlash)
        {
            effect.Emit(1);
        }
    }
}

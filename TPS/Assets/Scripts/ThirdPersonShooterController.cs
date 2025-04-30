using Cinemachine;
using StarterAssets;
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
    [SerializeField] private Transform spawnBulletPosition;
    [SerializeField] private GameObject hitGreen;
    [SerializeField] private GameObject hitred;
    [SerializeField] private Rig aimWeapon;
    [SerializeField] private Rig aimBody;
    [SerializeField] private Rig idleWeapon;

    private StarterAssetsInputs _starterAssetsInputs;
    private ThirdPersonController _thirdPersonController;
    private Animator _animator;

    private float _aimWeapon_Weight;
    private float _aimBody_Weight;
    private float _idleWeapon_Weight;

    public GameObject aimTarget;


    private void Awake()
    {
        _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        _thirdPersonController = GetComponent<ThirdPersonController>();
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        //_starterAssetsInputs.aim = true;
        UpdateRigWeights();

        /*鼠标所指*/
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
            aimTarget.transform.position = mouseWorldPosition;
        }

        /*是否开启瞄准*/
        if (_starterAssetsInputs.aim)
        {
            _aimVirtualCamera.gameObject.SetActive(true);// 镜头放大
            _thirdPersonController.setLookSensitivity(aimSensitivity);// 降低Aiming状态下的灵敏度
            _thirdPersonController.SetRotateOnMove(false);
            //_animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            Vector3 worldAimTarget = mouseWorldPosition;
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

        /*是否开火*/
        if(_starterAssetsInputs.shoot)
        {
            /*击中到某物*/
            if(hitTransform != null)
            {
                if (hitTransform.GetComponent<BulletTarget>() != null)
                {
                    //目标
                    //Instantiate(hitGreen, transform.position, Quaternion.identity);
                    Instantiate(hitGreen, raycastHit.point, Quaternion.identity);
                }
                else
                {
                    //其他的一些东西
                    Instantiate(hitred, raycastHit.point, Quaternion.identity);
                }
            }
            //Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            //Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));//取消实例化
            _starterAssetsInputs.shoot = false;
        }

    }

    private void UpdateRigWeights()
    {
        _aimWeapon_Weight = _starterAssetsInputs.aim ? 1f : 0f;
        _idleWeapon_Weight = _starterAssetsInputs.aim ? 0f : 1f;
        _aimBody_Weight = _starterAssetsInputs.aim ? 1f : 0f;

        aimWeapon.weight = Mathf.Lerp(aimWeapon.weight, _aimWeapon_Weight, Time.deltaTime * 20f);
        aimBody.weight = Mathf.Lerp(aimBody.weight, _aimBody_Weight, Time.deltaTime * 20f);
        idleWeapon.weight = Mathf.Lerp(idleWeapon.weight, _idleWeapon_Weight, Time.deltaTime * 20f);

    }
}

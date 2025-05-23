using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    public bool isAutomatic = false;
    public float fireRate = 1f; // 秒/发，例如 0.1 表示每秒10发
    public int bulletsPerShot = 1;
    public float bulletVelocity = 500f;
    public float cooldown = 0.5f;
    public int damage = 10;
    [Header("References")]
    public Transform originShootPosition;
    [Header("WeaponAudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip singleShotClip;
    [SerializeField] private AudioClip autoLoopClip;
    [Header("Particle")]
    [SerializeField] private ParticleSystem[] muzzleFlash;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem bloodEffect;

    private bool isEnemy;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }
            
    }

    public void HandleShooting(bool shootPressed, bool shootHeld, bool shootReleased, RaycastHit raycastHit)
    {
        if (isAutomatic)
        {
            if (shootHeld && cooldown <= 0f)
            {
                TryFire(raycastHit);
                cooldown = fireRate;
            }

            if (shootReleased)
            {
                // 停止自动枪音效等逻辑
                audioSource.Stop();
            }
        }
        else
        {
            if (shootPressed && cooldown <= 0f)
            {
                TryFire(raycastHit);
                cooldown = fireRate;
            }
        }
    }

    public void TryFire(RaycastHit raycastHit)
    {
        if (cooldown > 0f) return;
        Fire(raycastHit);
        HitTarget(raycastHit);
    }

    private void Fire(RaycastHit raycastHit)
    {
        // 瞄准
        Vector3 shootDirection = (raycastHit.point - originShootPosition.position).normalized;
        Debug.DrawLine(originShootPosition.position, raycastHit.point, Color.red, 1.0f);
        // 调用对象池生成子弹
        for (int i = 0; i < bulletsPerShot; i++)
        {
            GameObject bullet = BulletPool.Instance.TryGetBullet();
            bullet.transform.position = originShootPosition.position;
            bullet.transform.rotation = Quaternion.LookRotation(shootDirection);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(originShootPosition.forward * bulletVelocity, ForceMode.Impulse);
            }               
        }

        //播放声音
        PlayerFireAudio();
        //播放特效
        foreach (var effect in muzzleFlash)
        {
            effect.Emit(1);
        }
    }

    private void PlayerFireAudio()
    {
        if (!isAutomatic && singleShotClip != null)
        {
            audioSource.PlayOneShot(singleShotClip);
        }
        else if (isAutomatic && autoLoopClip != null)
        {
            audioSource.clip = autoLoopClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void HitTarget(RaycastHit raycastHit)
    {
        if (raycastHit.collider == null)
        {
            Debug.Log("射线碰撞体为空");
            return;
        }
        if (raycastHit.collider.CompareTag("Enemy"))
        {
            isEnemy = true;
            ZombieStats enemy = raycastHit.collider.GetComponent<ZombieStats>();
            if (enemy != null)
            {
                Debug.Log("-"+damage);
                enemy.TakeDamage(damage); // 设置伤害
            }
        }
    }

    public void ShowHitImpactVF(RaycastHit raycastHit, bool hitEnemy)
    {
        if (hitEffect != null)
        {
            hitEffect.transform.position = raycastHit.point;
            hitEffect.transform.forward = raycastHit.normal;
            hitEffect.Emit(1);
        }
        if (bloodEffect != null && hitEnemy)
        {
            //TODO: 血液特效
            bloodEffect.transform.position = raycastHit.point;
            bloodEffect.transform.forward = raycastHit.normal;
            bloodEffect.Play();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float bulletLifetime = 2f;
    private float lifetimer;


    private void Update()
    {
        lifetimer += Time.deltaTime;
        if (lifetimer >= bulletLifetime)
        {
            ReturnToPool();
        }
    }

    private void OnEnable()
    {
        lifetimer = 0f;
    }

    private void ReturnToPool()
    {
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            Destroy(gameObject); // Fallback：未使用对象池时销毁
        }
    }
}

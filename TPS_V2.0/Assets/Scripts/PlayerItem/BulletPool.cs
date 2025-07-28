using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public GameObject bulletPrefab;
    public int poolSize = 2;
    public Transform bulletContainer; // 用来收纳子弹实例

    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    public static BulletPool Instance;

    private void Awake()
    {
        Instance = this;

        for(int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletContainer);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }
    
    public GameObject TryGetBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        // 如果超出容量
        Debug.LogWarning("超出可用子弹对象数量！");
        GameObject newBullet = Instantiate(bulletPrefab, bulletContainer);
        return newBullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        TrailRenderer trail = bullet.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
        }
    }
}

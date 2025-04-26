using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BulletProjectTile : MonoBehaviour
{
    private Rigidbody bulletRigidbody;
    [SerializeField] private GameObject hitGreen;
    [SerializeField] private GameObject hitred;

    void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float bulletSpeed = 50f;
        bulletRigidbody.velocity = transform.forward * bulletSpeed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BulletTarget>() != null)
        {
            Instantiate(hitGreen, transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(hitred, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}

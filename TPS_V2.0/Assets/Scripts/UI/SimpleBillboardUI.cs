using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBillboardUI : MonoBehaviour
{
    private Transform camera;
    void Start()
    {
        camera = Camera.main.transform;
    }

    void LateUpdate()
    {
        // 面向相机
        if (camera != null)
        {
            transform.forward = camera.forward;
        }
    }
}

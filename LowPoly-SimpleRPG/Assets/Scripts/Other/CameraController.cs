using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //记录主角和相机的位置的偏移
    private Vector3 offset;
    //获取玩家的transform
    private Transform playerTransform;
    //增加缩放速度
    public float zoomSpeed = 5;

    // Start is called before the first frame update
    void Start()
    {
        //获取玩家位置 用玩家对象的标签来获取FindGameObjectWithTag
        playerTransform = GameObject.FindGameObjectWithTag(Tag.PLAYER).transform;
        offset = transform.position - playerTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        /*相机跟随*/
        //玩家位置坐标 + 与相机的偏移值 就可以实现相机跟随
        transform.position = playerTransform.position + offset;

        /*滑轮缩放*/
        //获取鼠标滑轮的值
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.fieldOfView -= scroll * zoomSpeed;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 37, 57);
    }
}

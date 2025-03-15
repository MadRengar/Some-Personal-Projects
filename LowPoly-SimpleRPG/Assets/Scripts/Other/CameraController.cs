using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //��¼���Ǻ������λ�õ�ƫ��
    private Vector3 offset;
    //��ȡ��ҵ�transform
    private Transform playerTransform;
    //���������ٶ�
    public float zoomSpeed = 5;

    // Start is called before the first frame update
    void Start()
    {
        //��ȡ���λ�� ����Ҷ���ı�ǩ����ȡFindGameObjectWithTag
        playerTransform = GameObject.FindGameObjectWithTag(Tag.PLAYER).transform;
        offset = transform.position - playerTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        /*�������*/
        //���λ������ + �������ƫ��ֵ �Ϳ���ʵ���������
        transform.position = playerTransform.position + offset;

        /*��������*/
        //��ȡ��껬�ֵ�ֵ
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.fieldOfView -= scroll * zoomSpeed;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 37, 57);
    }
}

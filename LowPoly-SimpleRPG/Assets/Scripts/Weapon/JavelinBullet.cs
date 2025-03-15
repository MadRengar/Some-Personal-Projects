using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*�ű����ã���ǹ����ײ��� ����*/
//Ŀ�꣺ֻҪ��ײ������Ͳ����˶� ���ٽ��ж�����ײ
public class JavelinBullet : MonoBehaviour
{
    public int atkValue = 50;//������
    private Rigidbody rgd;
    private Collider col;
    

    private void Start()
    {
        rgd = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == Tag.PLAYER) { return; }//���������� �ͷ��� �����κδ���
        //ֹͣ�˶�
        //rgd.velocity = Vector3.zero;
        //�����ܵ���������Ӱ��
        rgd.isKinematic = true;
        //��ײ�ر�
        col.enabled = false;

        /*�����ǹ��ײ�����˵����(���ڵ���������֮�ƶ�)*/
        transform.parent = collision.gameObject.transform;

        Destroy(this.gameObject, 2f);//��2s֮��������Ͷ���ı�ǹ

        if(collision.gameObject.tag == Tag.ENEMY)
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(atkValue);
        }

    }
}

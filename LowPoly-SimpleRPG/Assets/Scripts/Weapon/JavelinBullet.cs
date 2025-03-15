using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*脚本作用：标枪的碰撞检测 消除*/
//目标：只要碰撞到物体就不在运动 不再进行二次碰撞
public class JavelinBullet : MonoBehaviour
{
    public int atkValue = 50;//攻击力
    private Rigidbody rgd;
    private Collider col;
    

    private void Start()
    {
        rgd = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == Tag.PLAYER) { return; }//如果碰到玩家 就返回 不做任何处理
        //停止运动
        //rgd.velocity = Vector3.zero;
        //不在受到物理力的影响
        rgd.isKinematic = true;
        //碰撞关闭
        col.enabled = false;

        /*处理标枪碰撞到敌人的情况(扎在敌人身上随之移动)*/
        transform.parent = collision.gameObject.transform;

        Destroy(this.gameObject, 2f);//在2s之后销毁已投出的标枪

        if(collision.gameObject.tag == Tag.ENEMY)
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(atkValue);
        }

    }
}

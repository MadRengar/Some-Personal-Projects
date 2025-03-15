using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScytheWeapon : Weapon
{
    public const string ANIM_PARM_ISATTACK = "IsAttack";//设置一个静态常量(一般用大写)方便使用

    private Animator animator;

    public int atkValue = 100;//攻击力

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    //private void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.Space))
    //    {
    //        Attack();
    //    }
    //}



    public override void Attack()
    {
        //当调用Attack()方法时就会设置触发器，从而播放动画
        animator.SetTrigger(ANIM_PARM_ISATTACK);//设置攻击动画触发
    }

    /*碰撞检测*/
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == Tag.ENEMY)
        {
            //print("TriggerWith" + other.name);
            other.GetComponent<Enemy>().TakeDamage(atkValue);

        }
    }
}

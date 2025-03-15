using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScytheWeapon : Weapon
{
    public const string ANIM_PARM_ISATTACK = "IsAttack";//����һ����̬����(һ���ô�д)����ʹ��

    private Animator animator;

    public int atkValue = 100;//������

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
        //������Attack()����ʱ�ͻ����ô��������Ӷ����Ŷ���
        animator.SetTrigger(ANIM_PARM_ISATTACK);//���ù�����������
    }

    /*��ײ���*/
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == Tag.ENEMY)
        {
            //print("TriggerWith" + other.name);
            other.GetComponent<Enemy>().TakeDamage(atkValue);

        }
    }
}

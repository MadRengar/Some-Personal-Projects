using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    //敌人状态：正常 - 战斗
    //1 移动
    //2 休息
    public enum EnemyState
    {
        NormalState,   // 正常
        FightingState, // 战斗
        MovingState,   // 移动
        RestingState   // 休息
    }

    private EnemyState state = EnemyState.NormalState;//状态起始设置为“正常状态”
    private EnemyState childState = EnemyState.RestingState; //子状态起始设置为“休息状态”
    private NavMeshAgent enemyAgent;

    public float restTime = 2;//休息时间
    private float restTimer = 0;// 休息计时器

    public int HP = 100;//敌人血量
    public int EXP = 20;//敌人经验值

    void Start()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
    }

    //敌人移动逻辑
    void Update()
    {
        if (state == EnemyState.NormalState)
        {
            if (childState == EnemyState.RestingState)
            {
                restTimer += Time.deltaTime;//休息计时器计时增加
                if (restTimer > restTime)
                {
                    Vector3 randomPosition = FindRandomPosition();//获得随机目的地
                    enemyAgent.SetDestination(randomPosition);//向目的地移动
                    childState = EnemyState.MovingState;//将敌人当前状态设置为“移动状态”
                }
            }else if (childState == EnemyState.MovingState)
            {
                //如果到达目的地
                if (enemyAgent.remainingDistance <= 0)
                {
                    restTimer = 0;//将计时器归0
                    childState = EnemyState.RestingState;//修改状态为“休息状态”
                }
            }
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    TakeDamage(30);
        //}
    }
    /*在物体的当前位置周围随机生成一个新的位置向量*/
    //在游戏中创建一个敌人，让其在一个范围内随机移动
    Vector3 FindRandomPosition()
    {   //方向：得到一个随机的方向
        Vector3 randomDir = new Vector3(Random.Range(-1, 1f), 0, Random.Range(-1, 1f));
        //距离：对向量进行归一化，确保该向量具有单位长度
        return transform.position + randomDir.normalized * Random.Range(2, 5);
    }

    //敌人受到伤害
    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            Die();
        }
    }

    //敌人死亡
    private void Die()
    {
        //生成凋落物之前将item上的Collider禁用
        GetComponent<Collider>().enabled = false;
        //随机生成
        //int count = Random.Range(0, 4);//装备数量
        int count = 4;
        for (int i = 0; i < count; i++)
        {
            SpawnPickableItem();
        }

        EventCenter.EnemyDied(this);//
        Destroy(this.gameObject);
    }

    /*掉落物生成与处理*/
    private void SpawnPickableItem()
    {
        ItemSO item = ItemDBManager.Instance.GetRandomItem();//返回的是ItemSO类型

        /*实例化Item里的prefab：
            1 GameObject.Instantiate():用于创建游戏对象实例的函数。
            2 item.prefab:要实例化的预制体（Prefab）的引用
            3 Quaternion.identity:无旋转的四元数，这里是告诉 Unity 将新实例创建时不进行旋转
         */
        GameObject go = GameObject.Instantiate(item.prefab, transform.position, Quaternion.identity);
        /*！针对标枪掉落消失的情况*/
        //解决方案：将掉落物的标签改为“Interactable”
        go.tag = Tag.INTERACTABLE;
        /*禁用掉落物品为武器的动画*/
        //原因：动画会改变武器在场景中的位置
        Animator anim = go.GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }
        PickableObject po = go.AddComponent<PickableObject>();//对掉落物都增加PickableObject组件
        po.itemSO = item;


    /*针对掉落物为武器特殊情况的处理*/

        //捡起镰刀：单独处理掉落物为镰刀(武器)的情况;标枪在JavelinWeapon里处理
        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;//启用组件
            collider.isTrigger = false;//开启碰撞检测
        }
        Rigidbody rb = go.GetComponent<Rigidbody>(); 
        if (rb != null)
        {
            rb.isKinematic = false;//isKinematic为True无法进行碰撞检测
            rb.useGravity = true;//受重力影响开启
        }
        
    }
}
/*设计模式*/
/*
    这里处理掉落物的代码很多，原因主要是处理掉落物为武器的特殊情况。
    改进的思路就是将每种不同武器的prefab都制作两个：
    1 掉落物的prefab(掉落物可捡起)
    2 玩家拿在手上的prefab(武器)
 */
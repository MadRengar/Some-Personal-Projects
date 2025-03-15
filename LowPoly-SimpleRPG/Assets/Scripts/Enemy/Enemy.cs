using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    //����״̬������ - ս��
    //1 �ƶ�
    //2 ��Ϣ
    public enum EnemyState
    {
        NormalState,   // ����
        FightingState, // ս��
        MovingState,   // �ƶ�
        RestingState   // ��Ϣ
    }

    private EnemyState state = EnemyState.NormalState;//״̬��ʼ����Ϊ������״̬��
    private EnemyState childState = EnemyState.RestingState; //��״̬��ʼ����Ϊ����Ϣ״̬��
    private NavMeshAgent enemyAgent;

    public float restTime = 2;//��Ϣʱ��
    private float restTimer = 0;// ��Ϣ��ʱ��

    public int HP = 100;//����Ѫ��
    public int EXP = 20;//���˾���ֵ

    void Start()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
    }

    //�����ƶ��߼�
    void Update()
    {
        if (state == EnemyState.NormalState)
        {
            if (childState == EnemyState.RestingState)
            {
                restTimer += Time.deltaTime;//��Ϣ��ʱ����ʱ����
                if (restTimer > restTime)
                {
                    Vector3 randomPosition = FindRandomPosition();//������Ŀ�ĵ�
                    enemyAgent.SetDestination(randomPosition);//��Ŀ�ĵ��ƶ�
                    childState = EnemyState.MovingState;//�����˵�ǰ״̬����Ϊ���ƶ�״̬��
                }
            }else if (childState == EnemyState.MovingState)
            {
                //�������Ŀ�ĵ�
                if (enemyAgent.remainingDistance <= 0)
                {
                    restTimer = 0;//����ʱ����0
                    childState = EnemyState.RestingState;//�޸�״̬Ϊ����Ϣ״̬��
                }
            }
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    TakeDamage(30);
        //}
    }
    /*������ĵ�ǰλ����Χ�������һ���µ�λ������*/
    //����Ϸ�д���һ�����ˣ�������һ����Χ������ƶ�
    Vector3 FindRandomPosition()
    {   //���򣺵õ�һ������ķ���
        Vector3 randomDir = new Vector3(Random.Range(-1, 1f), 0, Random.Range(-1, 1f));
        //���룺���������й�һ����ȷ�����������е�λ����
        return transform.position + randomDir.normalized * Random.Range(2, 5);
    }

    //�����ܵ��˺�
    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            Die();
        }
    }

    //��������
    private void Die()
    {
        //���ɵ�����֮ǰ��item�ϵ�Collider����
        GetComponent<Collider>().enabled = false;
        //�������
        //int count = Random.Range(0, 4);//װ������
        int count = 4;
        for (int i = 0; i < count; i++)
        {
            SpawnPickableItem();
        }

        EventCenter.EnemyDied(this);//
        Destroy(this.gameObject);
    }

    /*�����������봦��*/
    private void SpawnPickableItem()
    {
        ItemSO item = ItemDBManager.Instance.GetRandomItem();//���ص���ItemSO����

        /*ʵ����Item���prefab��
            1 GameObject.Instantiate():���ڴ�����Ϸ����ʵ���ĺ�����
            2 item.prefab:Ҫʵ������Ԥ���壨Prefab��������
            3 Quaternion.identity:����ת����Ԫ���������Ǹ��� Unity ����ʵ������ʱ��������ת
         */
        GameObject go = GameObject.Instantiate(item.prefab, transform.position, Quaternion.identity);
        /*����Ա�ǹ������ʧ�����*/
        //�����������������ı�ǩ��Ϊ��Interactable��
        go.tag = Tag.INTERACTABLE;
        /*���õ�����ƷΪ�����Ķ���*/
        //ԭ�򣺶�����ı������ڳ����е�λ��
        Animator anim = go.GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }
        PickableObject po = go.AddComponent<PickableObject>();//�Ե����ﶼ����PickableObject���
        po.itemSO = item;


    /*��Ե�����Ϊ������������Ĵ���*/

        //�����������������������Ϊ����(����)�����;��ǹ��JavelinWeapon�ﴦ��
        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;//�������
            collider.isTrigger = false;//������ײ���
        }
        Rigidbody rb = go.GetComponent<Rigidbody>(); 
        if (rb != null)
        {
            rb.isKinematic = false;//isKinematicΪTrue�޷�������ײ���
            rb.useGravity = true;//������Ӱ�쿪��
        }
        
    }
}
/*���ģʽ*/
/*
    ���ﴦ�������Ĵ���ܶ࣬ԭ����Ҫ�Ǵ��������Ϊ���������������
    �Ľ���˼·���ǽ�ÿ�ֲ�ͬ������prefab������������
    1 �������prefab(������ɼ���)
    2 ����������ϵ�prefab(����)
 */
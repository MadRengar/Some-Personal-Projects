using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*�ű����ã����Ʊ�ǹ����Ͷ��������������������*/
public class JavelinWeapon : Weapon
{
    public float bulletSpeed;
    public GameObject bulletPrefab;
    private GameObject bulletGo;

    private void Start()
    {
        SpawnBullet();
    }

    /*����Ч��*/
    //1 ����ͨ״̬�� ��ǹ���������ϵ�
    //2 �����¿ո� ��ǹͶ�� ���ճ�ʼ�������(����·����������ƶ����ƶ�)
    //3 ������0.5s֮�ڲ��ܹ��ٴ�Ͷ����ǹ���й���
    /*�������*/
    //�����ǹΪ�� ���ǹ����λ����������� ���ı�־����ǹ��Ϊnull
    //����ǹͶ�� ���ı�־����ǹΪnull

    public override void Attack()
    {
        //��Ϊ�� �͸���ǹһ���ٶȽ��й���
        if (bulletGo != null)
        {
            bulletGo.transform.parent = null;//�����ǹ��ʱ�� ���丸�������������Ϊnull ��Ȼ��ǹ����Ժ����������ƶ����ƶ�
            //��ø���������ٶȲ���ֵ��ǰ���ٶ�(�ٶ�)
            bulletGo.GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;//���ﲻ��vector3��ǰ���� ������transform��ǰ����
            bulletGo.GetComponent<Collider>().enabled = true;//��ǹͶ�� ������ǹ����ײ
            Destroy(bulletGo, 5f);//�����ǹû��ײ�������Զ�����������

            bulletGo = null;//������ ��ǹ������Ͷ�� ��ǹ��־λΪnull
            Invoke("SpawnBullet", 0.5f);//��0.5s֮��Ż������ó���ǹ
        }
        //Ϊ��(null)�ͷ��� ��ʾ������϶��ȴ
        else
        {
            return;
        }
    }

    /*��ǹͶ���� �������ɵ��߼� + ���������Ϊ��ǹ*/
    private void SpawnBullet()
    {
        //Instantiateʵ������ǹ 
        bulletGo = GameObject.Instantiate(bulletPrefab, transform.position, transform.rotation);
        //���ñ�ǹ���������������(������)���� ����������
        bulletGo.transform.parent = transform;
        bulletGo.GetComponent<Collider>().enabled = false;//�رձ�ǹ����ײ��������������ʱ��ײ�������ᱻ����

        /*���������Ϊ��ǹʱ��ʹ�䲻��ʧ���߼�*/
        //�����ʱ��ǹ�ı�ǹΪ��interactable��˵��Ϊ������
        if (tag == Tag.INTERACTABLE)
        {
            //�Ƴ���ǹ�ϵĽű�JavelinBullet����Ϊ���е�OnCollisionEnter()��������Ʊ�ǹͶ������ʧ
            Destroy(bulletGo.GetComponent<JavelinBullet>());

     /*���������Ϊ��ǹ(��ʰ��)*/
            bulletGo.tag = Tag.INTERACTABLE;//���ı�ǩ
            PickableObject po = bulletGo.AddComponent<PickableObject>();//�����ɵı�ǹ�� ����PickableObject���
            po.itemSO = GetComponent<PickableObject>().itemSO;//����������PickableObject��� ����itemSO���ݹ�ȥ
            bulletGo.GetComponent <Collider>().enabled = true;
            Rigidbody rgd = bulletGo.GetComponent<Rigidbody>();
            rgd.constraints = ~RigidbodyConstraints.FreezeRotationY;//����Y��,Ȼ���á�~��ȡ����ȡ����Y��Ķ���
            bulletGo.transform.parent = null;
            Destroy(this.gameObject);
        }
    }
}

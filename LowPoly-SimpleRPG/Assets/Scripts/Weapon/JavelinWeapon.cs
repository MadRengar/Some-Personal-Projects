using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*脚本作用：控制标枪攻击投出、重新生成拿在手上*/
public class JavelinWeapon : Weapon
{
    public float bulletSpeed;
    public GameObject bulletPrefab;
    private GameObject bulletGo;

    private void Start()
    {
        SpawnBullet();
    }

    /*理想效果*/
    //1 在普通状态下 标枪是拿在手上的
    //2 当按下空格 标枪投出 按照初始方向飞行(飞行路径不随玩家移动而移动)
    //3 并且在0.5s之内不能够再次投出标枪进行攻击
    /*解决方案*/
    //如果标枪为空 则标枪生成位置在玩家手上 更改标志：标枪不为null
    //将标枪投出 更改标志：标枪为null

    public override void Attack()
    {
        //不为空 就给标枪一个速度进行攻击
        if (bulletGo != null)
        {
            bulletGo.transform.parent = null;//发射标枪的时候 将其父物体的坐标设置为null 不然标枪射出以后会随着玩家移动而移动
            //获得刚体组件的速度并赋值向前的速度(速度)
            bulletGo.GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;//这里不用vector3的前方向 而是用transform的前方向
            bulletGo.GetComponent<Collider>().enabled = true;//标枪投出 启动标枪的碰撞
            Destroy(bulletGo, 5f);//处理标枪没碰撞到物体飞远的情况下销毁

            bulletGo = null;//攻击后 标枪从手中投出 标枪标志位为null
            Invoke("SpawnBullet", 0.5f);//在0.5s之后才会重新拿出标枪
        }
        //为空(null)就返回 表示攻击间隙冷却
        else
        {
            return;
        }
    }

    /*标枪投出后 重新生成的逻辑 + 处理掉落物为标枪*/
    private void SpawnBullet()
    {
        //Instantiate实例化标枪 
        bulletGo = GameObject.Instantiate(bulletPrefab, transform.position, transform.rotation);
        //设置标枪的坐标生成在玩家(父物体)手上 即拿在手上
        bulletGo.transform.parent = transform;
        bulletGo.GetComponent<Collider>().enabled = false;//关闭标枪的碰撞，避免拿在手上时碰撞到东西会被消除

        /*处理掉落物为标枪时，使其不消失的逻辑*/
        //如果此时标枪的标枪为“interactable”说明为掉落物
        if (tag == Tag.INTERACTABLE)
        {
            //移除标枪上的脚本JavelinBullet，因为其中的OnCollisionEnter()方法会控制标枪投出后消失
            Destroy(bulletGo.GetComponent<JavelinBullet>());

     /*处理掉落物为标枪(可拾起)*/
            bulletGo.tag = Tag.INTERACTABLE;//更改标签
            PickableObject po = bulletGo.AddComponent<PickableObject>();//在生成的标枪上 挂载PickableObject组件
            po.itemSO = GetComponent<PickableObject>().itemSO;//调用自生的PickableObject组件 将其itemSO传递过去
            bulletGo.GetComponent <Collider>().enabled = true;
            Rigidbody rgd = bulletGo.GetComponent<Rigidbody>();
            rgd.constraints = ~RigidbodyConstraints.FreezeRotationY;//冻结Y轴,然后用“~”取反，取消对Y轴的冻结
            bulletGo.transform.parent = null;
            Destroy(this.gameObject);
        }
    }
}

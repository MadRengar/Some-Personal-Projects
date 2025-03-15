using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    //识别已装备的武器类型
    public Weapon weapon;
    public Sprite weaponIcon;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && weapon != null)
        {
            weapon.Attack();
        }
    }

    public void LoadWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }
    public void LoadWeapon(ItemSO itemSO)
    {
        //避免手上拿两把武器
        if (weapon != null)
        {
            Destroy(weapon.gameObject);
            weapon = null;
        }

        //GameObject weapon = itemSO.prefab;//得到武器的prefab
        string prefabName = itemSO.prefab.name;
        Transform weaponParent = transform.Find(prefabName + "Position");//通过名字找到父对象->目的是挂载到对应父物体下
        GameObject weaponGo = GameObject.Instantiate(itemSO.prefab);//实例化武器
        weaponGo.transform.parent = weaponParent;//将实例化的武器挂载到weaponParent下
        weaponGo.transform.localPosition = Vector3.zero;//本地坐标：0
        weaponGo.transform.localRotation = Quaternion.identity;//本地旋转：0

        this.weapon = weaponGo.GetComponent<Weapon>();//保存武器 因为攻击判定需要
        this.weaponIcon = itemSO.icon;

        PlayerPropertyUI.Instance.UpdatePlayerPropertyUI();

    }
    public void UnLoadWeapon()
    {
        weapon = null;
    }

}

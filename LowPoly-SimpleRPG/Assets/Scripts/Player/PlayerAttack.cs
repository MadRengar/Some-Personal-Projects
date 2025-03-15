using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    //ʶ����װ������������
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
        //������������������
        if (weapon != null)
        {
            Destroy(weapon.gameObject);
            weapon = null;
        }

        //GameObject weapon = itemSO.prefab;//�õ�������prefab
        string prefabName = itemSO.prefab.name;
        Transform weaponParent = transform.Find(prefabName + "Position");//ͨ�������ҵ�������->Ŀ���ǹ��ص���Ӧ��������
        GameObject weaponGo = GameObject.Instantiate(itemSO.prefab);//ʵ��������
        weaponGo.transform.parent = weaponParent;//��ʵ�������������ص�weaponParent��
        weaponGo.transform.localPosition = Vector3.zero;//�������꣺0
        weaponGo.transform.localRotation = Quaternion.identity;//������ת��0

        this.weapon = weaponGo.GetComponent<Weapon>();//�������� ��Ϊ�����ж���Ҫ
        this.weaponIcon = itemSO.icon;

        PlayerPropertyUI.Instance.UpdatePlayerPropertyUI();

    }
    public void UnLoadWeapon()
    {
        weapon = null;
    }

}

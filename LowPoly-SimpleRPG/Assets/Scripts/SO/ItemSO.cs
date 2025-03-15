using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ItemSO : ScriptableObject
{
    //这里不能用这种 public string Name { get; set; } 属性的方式，只能用成员的方式
    public int id;
    public new string name;
    public ItemType itemType;
    public string description;
    public List<Property> propertyList;
    public Sprite icon;
    public GameObject prefab;
}

public enum ItemType
{
    Weapon,
    Consumable
}

//加上序列化 才能识别到List<ItemProperty> propertyList
//序列化的目的：标识某个类可以序列化 即可以做数据存储 作为一个文件而存在
[Serializable]
public class Property
{
    public PropertyType propertyType;
    public int value;
    public Property()
    {

    }
    public Property(PropertyType propertyType, int value)
    {
        this.propertyType = propertyType;
        this.value = value;
    }
}

public enum PropertyType
{
    HPValue,
    EnergyValue,
    MentalValue,
    SpeedValue,
    AttackValue
}

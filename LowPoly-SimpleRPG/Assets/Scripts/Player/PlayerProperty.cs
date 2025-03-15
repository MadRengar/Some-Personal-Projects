using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProperty : MonoBehaviour
{
    public Dictionary<PropertyType, List<Property>> propertyDict;
    /*初始属性-频繁变动的三个属性 不适合放在字典中*/
    public int hpValue = 100;
    public int enengyValue = 100;
    public int mentalValue = 100;

    public int level = 1;
    public int currentExp = 0; 

    void Awake()
    {
        propertyDict = new Dictionary<PropertyType, List<Property>>();
        propertyDict.Add(PropertyType.HPValue, new List<Property>());
        propertyDict.Add(PropertyType.EnergyValue, new List<Property>());
        propertyDict.Add(PropertyType.MentalValue, new List<Property>());
        propertyDict.Add(PropertyType.SpeedValue, new List<Property>());
        propertyDict.Add(PropertyType.AttackValue, new List<Property>());
        
        AddProperty(PropertyType.SpeedValue, 5);
        AddProperty(PropertyType.AttackValue, 20);

        EventCenter.OnEnemyDied += OnEnenmyDie;// ?
    }

    public void AddProperty(PropertyType pt, int value)
    {
        switch(pt)
        {
            case PropertyType.HPValue:
                hpValue += value;
                return;
            case PropertyType.EnergyValue:
                enengyValue += value;
                return;
            case PropertyType.MentalValue:
                mentalValue += value;
                return;
        }
        List<Property> list;
        propertyDict.TryGetValue(pt, out list);//out对外传递\
        list.Add(new Property(pt, value));
    }

    public void RemoveProperty(PropertyType pt, int value)
    {
        switch (pt)
        {
            case PropertyType.HPValue:
                hpValue -= value;
                return;
            case PropertyType.EnergyValue:
                enengyValue -= value;
                return;
            case PropertyType.MentalValue:
                mentalValue -= value;
                return;
        }
        List<Property> list;
        propertyDict.TryGetValue(pt, out list);
        list.Remove(list.Find( x => x.value == value));//拉姆达表达式
    }

    public void UseDrug(ItemSO itemSO)
    {
        foreach (Property p in itemSO.propertyList)
        {
            AddProperty(p.propertyType, p.value);
        }
    }

    private void OnEnenmyDie(Enemy enemy)
    {
        this.currentExp += enemy.EXP;
        if (currentExp >= level * 30)
        {
            currentExp -= level * 30;
            level++;
        }
        PlayerPropertyUI.Instance.UpdatePlayerPropertyUI();
    }


}

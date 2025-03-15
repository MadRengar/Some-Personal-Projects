using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ItemDBSO方便从其他地方使用Prefab
[CreateAssetMenu()]
public class ItemDBSO : ScriptableObject
{
    public List<ItemSO> itemList;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType { Wood, Iron, Ammo, Food }

[CreateAssetMenu(fileName = "New Data", menuName = "Inventory/ResourceData")]
public class ResourceData_SO : ScriptableObject
{
    public string resourceName;
    public ResourceType type;
    public float unitWeight;
}

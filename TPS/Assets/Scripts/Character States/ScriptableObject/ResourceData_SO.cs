using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType { Wood, Iron, Ammo, Food, MedicalKit }

[CreateAssetMenu(fileName = "New Data", menuName = "InventoryManager/ResourceData")]
public class ResourceData_SO : ScriptableObject
{
    public string resourceName;
    public ResourceType type;
    public float unitWeight;
    public bool isConsuming;
    public float restoreValues;
}

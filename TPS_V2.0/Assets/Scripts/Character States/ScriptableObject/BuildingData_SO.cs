using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Building/Base Building Data")]
public class BuildingData_SO : ScriptableObject
{
    [Header("Building Basic Info")]
    public string buildingName;
    public int requiredWoodNum;
    public int requiredIronNum;
    public float requiredBuildingTime;

    [Header("Building Stats")]
    public int maxHealth = 100;        // ½¨ÖşÄÍ¾ÃÖµ

    [Header("Building Type")]
    public BuildingType buildingType;
}

public enum BuildingType
{
    Turret,
    Generator,
    Storage
}
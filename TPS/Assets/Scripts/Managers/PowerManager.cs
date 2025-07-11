using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerManager : MonoBehaviour
{

    [Header("Power System")]
    public List<TurretController> allTurrets = new List<TurretController>(); 
    public List<TurretController> level1Turrets = new List<TurretController>(); 
    public List<TurretController> level2Turrets = new List<TurretController>(); 

    [Header("Debug Info")]
    [SerializeField] private int totalPowerGeneration;
    [SerializeField] private int totalPowerConsumption;
    [SerializeField] private int activeTurrets;

    private InventoryManager inventoryManager;

    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GameManager.Instance.GetInventoryManager();

        // 订阅电力变化事件
        InventoryManager.OnResourcesChanged += CheckPowerBalance;

        // 延迟1秒初始检查，确保所有系统都已初始化
        Invoke(nameof(CheckPowerBalance), 1f);
    }

    void OnDestroy()
    {
        if (inventoryManager != null)
        {
            InventoryManager.OnResourcesChanged -= CheckPowerBalance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CheckPowerBalance()
    {
        totalPowerGeneration = inventoryManager.GetAllGeneratorPower();
        totalPowerConsumption = 0;
        activeTurrets = 0;

        foreach (var turret in allTurrets)
        {
            if (turret != null && !turret.IsDestroyed())
            {
                totalPowerConsumption += turret.GetPowerRequirement();
            }
        }

        Debug.Log($"[PowerManager] 电力检查 - 发电: {totalPowerGeneration}, 需求: {totalPowerConsumption}");

        if (totalPowerGeneration >= totalPowerConsumption)
        {
            EnableAllTurrets();
        }
        else
        {
            // 电力不足，从最后建造的开始暂停
            DisableTurretsFromLast(totalPowerGeneration);
        }
    }

    /// <summary>
    /// 注册新建造的炮台（最新的会添加到列表末尾）
    /// </summary>
    public void RegisterTurret(TurretController turret)
    {
        if (turret != null && !allTurrets.Contains(turret))
        {
            if(turret.GetTurretLevel() == 1)
            {
                level1Turrets.Add(turret);
            }
            else if(turret.GetTurretLevel() == 2)
            {
                level2Turrets.Add(turret);
            }
            allTurrets.Add(turret);

            Debug.Log($"[PowerManager] 注册炮台: {turret.name}，当前炮台数量: {allTurrets.Count}");
            CheckPowerBalance();
        }
    }

    /// <summary>
    /// 移除被摧毁的炮台
    /// </summary>
    public void UnregisterTurret(TurretController turret)
    {
        if (turret != null && allTurrets.Contains(turret))
        {
            if (turret.GetTurretLevel() == 1)
            {
                level1Turrets.Remove(turret);
            }
            else if (turret.GetTurretLevel() == 2)
            {
                level2Turrets.Remove(turret);
            }
            allTurrets.Remove(turret);
            Debug.Log($"[PowerManager] 移除炮台: {turret.name}，当前炮台数量: {allTurrets.Count}");
            CheckPowerBalance();
        }
    }

    private void EnableAllTurrets()
    {
        activeTurrets = 0;
        foreach (var turret in allTurrets)
        {
            if (turret != null && !turret.IsDestroyed())
            {
                turret.SetPowered(true);
                activeTurrets++;
            }
        }
        Debug.Log($"[PowerManager] 电力充足，启用所有 {activeTurrets} 座炮台");
    }

    /// <summary>
    /// 从最后建造的炮台开始暂停，直到电力足够
    /// </summary>
    private void DisableTurretsFromLast(int availablePower)
    {
        int usedPower = 0;
        activeTurrets = 0;

        // 从最早建造的开始分配电力
        for (int i = 0; i < allTurrets.Count; i++)
        {
            var turret = allTurrets[i];
            if (turret != null && !turret.IsDestroyed())
            {
                int powerNeed = turret.GetPowerRequirement();

                if (usedPower + powerNeed <= availablePower)
                {
                    turret.SetPowered(true);
                    usedPower += powerNeed;
                    activeTurrets++;
                }
                else
                {
                    turret.SetPowered(false);
                }
            }
        }

        Debug.Log($"[PowerManager] 电力不足，启用 {activeTurrets}/{allTurrets.Count} 座炮台，消耗电力: {usedPower}/{availablePower}");
    }

}

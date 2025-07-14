using PlayerControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoWorkbenchController : MonoBehaviour
{
    [Header("Workbench Settings")]
    public int woodCostPerBullet = 1;    // 每发子弹消耗的木头数量
    public int ironCostPerBullet = 1;    // 每发子弹消耗的铁块数量
    public int bulletsPerCraft = 30;     // 每次制作的子弹数量

    [Header("AI Support")]
    public bool aiInRange = false; // AI是否在补给区域内

    // 事件定义
    public static event Action OnPlayerInteractWithWorkbench;

    private bool playerInRange = false;

    private PlayerInputSystem playerInputSystem;

    private void Start()
    {
        playerInputSystem = GameManager.Instance.GetPlayerInputSystem();
    }

    private void Update()
    {
        // 检查交互输入
        HandleInteractionInput();
    }

    /// <summary>
    /// Unity 触发器事件 - 物体进入触发范围
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UIManager.Instance.ShowInteractionTip();
        }
        if (other.CompareTag("AIPlayer"))
        {
            Debug.Log("ai进入弹药补充区域！");
            aiInRange = true;
        }
    }

    /// <summary>
    /// Unity 触发器事件 - 物体离开触发范围
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UIManager.Instance.HideInteractionTip();
        }
        if (other.CompareTag("AIPlayer"))
        {
            aiInRange = false;
        }
    }

    /// <summary>
    /// 处理交互输入
    /// </summary>
    public void HandleInteractionInput()
    {
        if (playerInputSystem == null || !playerInRange)
            return;

        if (playerInputSystem.interact)
        {
            OnPlayerInteractWithWorkbench?.Invoke();
            playerInputSystem.EnterInteractMode();
        }
    }

    /// <summary>
    /// 计算制作指定数量子弹所需的材料
    /// </summary>
    public void GetCraftingCost(int bulletAmount, out int woodCost, out int ironCost)
    {
        int craftCount = Mathf.CeilToInt((float)bulletAmount / bulletsPerCraft);
        woodCost = craftCount * woodCostPerBullet;
        ironCost = craftCount * ironCostPerBullet;
    }

    /// <summary>
    /// 尝试制作子弹
    /// </summary>
    public bool TryCraftAmmo(int bulletAmount, InventoryManager inventoryManager, WeaponManager weaponManager)
    {
        // 计算制作成本
        GetCraftingCost(bulletAmount, out int woodCost, out int ironCost);

        // 检查玩家背包资源是否足够
        int playerWood = inventoryManager.GetPlayerResourceByType(ResourceType.Wood);
        int playerIron = inventoryManager.GetPlayerResourceByType(ResourceType.Iron);

        if (playerWood < woodCost)
        {
            Debug.Log($"木头不足！需要: {woodCost}, 拥有: {playerWood}");
            return false;
        }

        if (playerIron < ironCost)
        {
            Debug.Log($"铁块不足！需要: {ironCost}, 拥有: {playerIron}");
            return false;
        }

        // 扣除材料
        bool resourceConsumed = inventoryManager.TryConsuming(woodCost, ironCost);
        if (!resourceConsumed)
        {
            Debug.LogError("扣除制作材料失败");
            return false;
        }

        // 添加子弹到武器库存
        weaponManager.AddReserveAmmo(bulletAmount);

        Debug.Log($"成功制作 {bulletAmount} 发子弹，消耗 {woodCost} 木头, {ironCost} 铁块");
        return true;
    }

    #region Public Getters
    public int GetWoodCostPerBullet() => woodCostPerBullet;
    public int GetIronCostPerBullet() => ironCostPerBullet;
    public int GetBulletsPerCraft() => bulletsPerCraft;
    #endregion
}
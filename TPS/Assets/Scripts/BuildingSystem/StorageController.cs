using PlayerControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageController : MonoBehaviour, IBuildingController
{
    [Header("Storage Data")]
    public StorageData_SO storageData;

    [Header("Current Stats")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentCapacity;
    [SerializeField] private int currentWoodAmount;
    [SerializeField] private int currentIronAmount;

    // 从DataSO读取的数据
    [Header("Current Storage Stats (Read Only)")]
    [SerializeField] private int requiredWoodNum;
    [SerializeField] private int requiredIronNum;
    [SerializeField] private float requiredBuildingTime;
    [SerializeField] private int storageCapacity;

    private bool playerInRange = false;

    private PlayerInputSystem playerInputSystem;

    public BuildingData_SO GetBuildingData() => storageData;

    public static event Action OnPlayerEnterStorageRange;
    public static event Action OnPlayerExitStorageRange;
    public static event Action <StorageController> OnPlayerInteractWithStorage; // 与仓库交互事件

    private void Start()
    {
        playerInputSystem = GameManager.Instance.GetPlayerInputSystem();
        LoadStorageData();
        SetupInteractionTrigger();

        // 初始化存储数据
        InitializeStorageData();

        // 自动注册到 InventoryManager
        RegisterToInventoryManager();
    }

    private void InitializeStorageData()
    {
        // 确保每个新建的仓库都有独立的初始数据
        currentWoodAmount = 0;
        currentIronAmount = 0;

        Debug.Log($"仓库 {gameObject.name} 初始化完成，容量: {storageCapacity}kg");
    }

    /// <summary>
    /// 获取仓库的唯一标识（用于将来保存/加载数据）
    /// </summary>
    public string GetStorageID()
    {
        // 使用实例ID作为唯一标识
        return $"Storage_{GetInstanceID()}";
    }

    private void Update()
    {
        // 检查交互输入
        HandleInteractionInput();
    }

    /// <summary>
    /// 从DataSO加载仓库数据
    /// </summary>
    private void LoadStorageData()
    {
        if (storageData == null)
        {
            Debug.LogError($"StorageController: {gameObject.name} 未分配 StorageData_SO!");
            return;
        }

        // 读取基础建筑需求（继承自BuildingData_SO）
        requiredWoodNum = storageData.requiredWoodNum;
        requiredIronNum = storageData.requiredIronNum;
        requiredBuildingTime = storageData.requiredBuildingTime;

        // 读取仓库特有属性
        storageCapacity = storageData.storageCapacity;

        Debug.Log($"仓库数据加载完成: {gameObject.name}, 容量: {storageCapacity}");
    }

    /// <summary>
    /// 设置交互触发器
    /// </summary>
    private void SetupInteractionTrigger()
    {
        // 在主物体上添加一个专门的交互触发器
        Collider[] colliders = GetComponents<Collider>();
        bool hasTrigger = false;

        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                break;
            }
        }
    }

    /// <summary>
    /// Unity 触发器事件 - 物体进入触发范围
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEnterRange();
            OnPlayerEnterStorageRange?.Invoke();
        }
    }

    /// <summary>
    /// Unity 触发器事件 - 物体离开触发范围
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExitRange();
            OnPlayerExitStorageRange?.Invoke();
        }
    }

    public void HandleInteractionInput()
    {
        if (playerInputSystem == null || !playerInRange)
            return;

        if (playerInputSystem.interact)
        {
            Debug.Log("玩家与仓库交互！");
            // 打开UI
            OnPlayerInteractWithStorage?.Invoke(this);
            playerInputSystem.EnterInteractMode(); // 进入存储模式
        }
    }

    /// <summary>
    /// 玩家进入交互范围
    /// </summary>
    public void PlayerEnterRange()
    {
        playerInRange = true;
        Debug.Log("玩家进入仓库交互范围");
        // 这里后面可以显示交互提示UI
    }

    /// <summary>
    /// 玩家离开交互范围
    /// </summary>
    public void PlayerExitRange()
    {
        playerInRange = false;
        Debug.Log("玩家离开仓库交互范围");
        // 这里后面可以隐藏交互提示UI
    }



    #region Storage Panel
    /// <summary>
    /// 计算当前已使用的重量
    /// </summary>
    public float GetCurrentUsedWeight()
    {
        return currentWoodAmount * 1f + currentIronAmount * 2f;
    }

    /// <summary>
    /// 计算剩余容量
    /// </summary>
    public float GetRemainingCapacity()
    {
        return storageCapacity - GetCurrentUsedWeight();
    }

    /// <summary>
    /// 检查是否可以存储指定数量的资源
    /// </summary>
    public bool CanStoreResources(int woodAmount, int ironAmount)
    {
        float additionalWeight = woodAmount * 1f + ironAmount * 2f;
        return GetRemainingCapacity() >= additionalWeight;
    }

    /// <summary>
    /// 尝试存储资源到仓库
    /// </summary>
    public bool TryStoreResources(int woodAmount, int ironAmount)
    {
        // 检查容量是否足够
        if (!CanStoreResources(woodAmount, ironAmount))
        {
            Debug.Log("仓库容量不足，无法存储更多资源");
            return false;
        }

        // 存储资源
        currentWoodAmount += woodAmount;
        currentIronAmount += ironAmount;

        Debug.Log($"成功存储到仓库: {woodAmount} 木头, {ironAmount} 铁块");
        Debug.Log($"仓库当前存储: {currentWoodAmount} 木头, {currentIronAmount} 铁块");
        Debug.Log($"剩余容量: {GetRemainingCapacity()} kg");

        GameManager.Instance.GetInventoryManager().TriggerResourcesChangedEvent();
        return true;
    }

    /// <summary>
    /// 尝试从仓库取出资源
    /// </summary>
    public bool TryRetrieveResources(int woodAmount, int ironAmount)
    {
        // 检查是否有足够的资源可以取出
        if (currentWoodAmount < woodAmount || currentIronAmount < ironAmount)
        {
            Debug.Log("仓库中资源不足，无法取出指定数量");
            return false;
        }

        // 取出资源
        currentWoodAmount -= woodAmount;
        currentIronAmount -= ironAmount;

        Debug.Log($"成功从仓库取出: {woodAmount} 木头, {ironAmount} 铁块");
        Debug.Log($"仓库剩余存储: {currentWoodAmount} 木头, {currentIronAmount} 铁块");

        // 触发资源变化事件
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.TriggerResourcesChangedEvent();
        }

        return true;
    }
    #endregion




    /// <summary>
    /// 注册到 InventoryManager
    /// </summary>
    private void RegisterToInventoryManager()
    {
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.RegisterStorage(this);
        }
        else
        {
            Debug.LogError("找不到 InventoryManager，无法注册仓库");
        }
    }

    /// <summary>
    /// 在销毁时从 InventoryManager 中移除
    /// </summary>
    private void OnDestroy()
    {
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.UnregisterStorage(this);
        }
    }

    /// 检查玩家是否可以交互
    /// </summary>
    public bool CanInteract()
    {
        return playerInRange;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"发电机受到 {damage} 点伤害，剩余血量: {currentHealth}");

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    public bool IsDestroyed()
    {
        return currentHealth <= 0;
    }

    private void DestroyBuilding()
    {
        Debug.Log("发电机被摧毁！");
        // 这里可以添加爆炸效果
        Destroy(gameObject);
    }



    #region Public Getters - 供UI和其他系统使用
    public int GetCurrentCapacity() => currentCapacity;
    public int GetCurrentHealth() => currentHealth;
    public int GetStorageCapacity() => storageCapacity;
    public int GetStoredWood() => currentWoodAmount;
    public int GetStoredIron() => currentIronAmount;
    #endregion
}
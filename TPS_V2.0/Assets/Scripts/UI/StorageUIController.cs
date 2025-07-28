using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayerControl;

/// <summary>
/// 仓库UI控制器，挂载在StoragePanel上
/// </summary>
public class StorageUIController : MonoBehaviour
{
    [Header("Storage Display")]
    public TextMeshProUGUI storageWoodText;     // 仓库木头数量显示
    public TextMeshProUGUI storageIronText;     // 仓库铁块数量显示
    public TextMeshProUGUI lastCapacityText;    // 剩余容量显示

    [Header("Input Fields")]
    public TMP_InputField woodInputField;       // 木头输入框
    public TMP_InputField ironInputField;       // 铁块输入框

    [Header("Buttons")]
    public Button depositButton;                // 确认按钮
    public Button retrieveButton;                // 确认按钮
    public Button cancelButton;                 // 取消按钮

    [Header("Resource Data")]
    public ResourceData_SO woodResourceData; 
    public ResourceData_SO ironResourceData;

    [Header("References")]
    private StorageController currentStorage;     // 当前操作的仓库
    private InventoryManager inventoryManager;
    private PlayerInputSystem playerInputSystem;

    private void Start()
    {
        // 获取管理器引用
        inventoryManager = FindObjectOfType<InventoryManager>();
        playerInputSystem = GameManager.Instance.GetPlayerInputSystem();

        // 设置按钮事件
        SetupButtons();
    }

    private void OnEnable()
    {
        // 面板激活时刷新UI
        RefreshStorageUI();
        ClearInputFields();
    }

    private void OnDisable()
    {
        playerInputSystem.ExitInteractMode(); // 退出存储模式
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtons()
    {
        if (depositButton != null)
        {
            depositButton.onClick.AddListener(OnDepositButtonClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }
        if (retrieveButton != null)
        {
            retrieveButton.onClick.AddListener(OnRetrieveButtonClicked);
        }
    }

    /// <summary>
    /// 刷新仓库UI显示
    /// </summary>
    public void RefreshStorageUI()
    {
        if (currentStorage == null) return;

        // 更新仓库当前存储显示
        int storageWood = currentStorage.GetStoredWood();
        int storageIron = currentStorage.GetStoredIron();

        if (storageWoodText != null)
            storageWoodText.text = storageWood.ToString();

        if (storageIronText != null)
            storageIronText.text = storageIron.ToString();

        // 计算并显示剩余容量
        float remainingCapacity = currentStorage.GetRemainingCapacity();

        if (lastCapacityText != null)
            lastCapacityText.text = $"Last: {remainingCapacity} kg";
    }

    /// <summary>
    /// 设置当前操作的仓库
    /// </summary>
    public void SetCurrentStorage(StorageController storage)
    {
        currentStorage = storage;
        RefreshStorageUI();
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    private void OnDepositButtonClicked()
    {
        // 获取输入的数量
        int woodAmount = GetInputAmount(woodInputField);
        int ironAmount = GetInputAmount(ironInputField);

        if (woodAmount <= 0 && ironAmount <= 0)
        {
            Debug.Log("请输入要存储的资源数量");
            return;
        }

        // 尝试存储资源
        bool success = TryDepositResources(woodAmount, ironAmount);

        if (success)
        {
            Debug.Log($"成功存储 {woodAmount} 木头, {ironAmount} 铁块");
            RefreshStorageUI();
            ClearInputFields();
            inventoryManager.UpdateStorageDebugInfo();
        }
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void OnCancelButtonClicked()
    {
        CloseStoragePanel();
    }

    private void OnRetrieveButtonClicked()
    {
        // 获取输入的数量
        int woodAmount = GetInputAmount(woodInputField);
        int ironAmount = GetInputAmount(ironInputField);

        if (woodAmount <= 0 && ironAmount <= 0)
        {
            Debug.Log("请输入要取出的资源数量");
            return;
        }

        // 尝试取出资源
        bool success = TryRetrieveResources(woodAmount, ironAmount);

        if (success)
        {
            Debug.Log($"成功取出 {woodAmount} 木头, {ironAmount} 铁块");
            RefreshStorageUI();
            ClearInputFields();

            // 立即刷新InventoryManager的调试信息
            if (inventoryManager != null)
            {
                inventoryManager.UpdateStorageDebugInfo();
            }
        }
    }

    /// <summary>
    /// 尝试存储资源
    /// </summary>
    private bool TryDepositResources(int woodAmount, int ironAmount)
    {
        if (inventoryManager == null || currentStorage == null)
        {
            Debug.LogError("InventoryManager 或 CurrentStorage 为空");
            return false;
        }

        // 1. 检查玩家背包是否有足够的资源
        int playerWood = inventoryManager.GetPlayerResourceByType(ResourceType.Wood);
        int playerIron = inventoryManager.GetPlayerResourceByType(ResourceType.Iron);

        if (playerWood < woodAmount)
        {
            Debug.Log($"玩家背包木头不足！需要: {woodAmount}, 拥有: {playerWood}");
            return false;
        }

        if (playerIron < ironAmount)
        {
            Debug.Log($"玩家背包铁块不足！需要: {ironAmount}, 拥有: {playerIron}");
            return false;
        }

        // 2. 检查仓库容量是否足够
        if (!currentStorage.CanStoreResources(woodAmount, ironAmount))
        {
            Debug.Log("仓库容量不足，无法存储更多资源");
            return false;
        }

        // 3. 执行资源转移：从玩家背包扣除资源
        bool resourceConsumed = inventoryManager.TryConsuming(woodAmount, ironAmount);
        if (!resourceConsumed)
        {
            Debug.LogError("从玩家背包扣除资源失败");
            return false;
        }

        // 4. 将资源存入仓库
        bool storedSuccess = currentStorage.TryStoreResources(woodAmount, ironAmount);
        if (!storedSuccess)
        {
            Debug.LogError("存储到仓库失败！需要回滚玩家背包资源");
            return false;
        }

        Debug.Log($"成功完成资源转移: {woodAmount} 木头, {ironAmount} 铁块 从玩家背包存入仓库");
        return true;
    }

    private bool TryRetrieveResources(int woodAmount, int ironAmount)
    {
        if (inventoryManager == null || currentStorage == null)
        {
            Debug.LogError("InventoryManager 或 CurrentStorage 为空");
            return false;
        }

        // 1. 检查仓库是否有足够的资源
        int storageWood = currentStorage.GetStoredWood();
        int storageIron = currentStorage.GetStoredIron();

        if (storageWood < woodAmount)
        {
            Debug.Log($"仓库木头不足！需要: {woodAmount}, 拥有: {storageWood}");
            return false;
        }

        if (storageIron < ironAmount)
        {
            Debug.Log($"仓库铁块不足！需要: {ironAmount}, 拥有: {storageIron}");
            return false;
        }

        // 2. 检查玩家背包重量是否足够容纳这些资源
        float additionalWeight = woodAmount * 1f + ironAmount * 2f; // 木头1kg/个，铁块2kg/个
        float currentPlayerWeight = inventoryManager.GetPlayerCurrentWeight();

        if (currentPlayerWeight + additionalWeight > inventoryManager.playerMaxWeight)
        {
            Debug.Log($"玩家背包重量不足！需要: {additionalWeight}kg, 剩余容量: {inventoryManager.playerMaxWeight - currentPlayerWeight}kg");
            return false;
        }

        // 3. 从仓库取出资源
        bool retrieveSuccess = currentStorage.TryRetrieveResources(woodAmount, ironAmount);
        if (!retrieveSuccess)
        {
            Debug.LogError("从仓库取出资源失败");
            return false;
        }

        // 4. 将资源添加到玩家背包
        bool addSuccess = AddResourcesToPlayerInventory(woodAmount, ironAmount);
        if (!addSuccess)
        {
            Debug.LogError("添加资源到玩家背包失败！需要回滚仓库资源");
            // 回滚：将资源放回仓库
            currentStorage.TryStoreResources(woodAmount, ironAmount);
            return false;
        }

        Debug.Log($"成功完成资源取出: {woodAmount} 木头, {ironAmount} 铁块 从仓库取出到玩家背包");
        return true;
    }

    private bool AddResourcesToPlayerInventory(int woodAmount, int ironAmount)
    {
        bool success = true;

        // 添加木头到玩家背包
        if (woodAmount > 0)
        {
            if (woodResourceData != null)
            {
                bool woodAdded = inventoryManager.TryAddPlayer(woodResourceData, woodAmount);
                if (!woodAdded)
                {
                    Debug.LogError("添加木头到玩家背包失败");
                    success = false;
                }
            }
            else
            {
                Debug.LogError("木头ResourceData_SO未设置");
                success = false;
            }
        }

        // 添加铁块到玩家背包
        if (ironAmount > 0 && success)
        {
            if (ironResourceData != null)
            {
                bool ironAdded = inventoryManager.TryAddPlayer(ironResourceData, ironAmount);
                if (!ironAdded)
                {
                    Debug.LogError("添加铁块到玩家背包失败");
                    success = false;
                }
            }
            else
            {
                Debug.LogError("铁块ResourceData_SO未设置");
                success = false;
            }
        }

        return success;
    }

    /// <summary>
    /// 获取输入框的数量
    /// </summary>
    private int GetInputAmount(TMP_InputField inputField)
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
            return 0;

        if (int.TryParse(inputField.text, out int amount))
        {
            return Mathf.Max(0, amount); // 确保不为负数
        }
        return 0;
    }

    /// <summary>
    /// 清空输入框
    /// </summary>
    private void ClearInputFields()
    {
        if (woodInputField != null)
            woodInputField.text = "";

        if (ironInputField != null)
            ironInputField.text = "";
    }

    /// <summary>
    /// 关闭仓库面板
    /// </summary>
    private void CloseStoragePanel()
    {
        ClearInputFields();
        UIManager.Instance?.HideStoragePanel();
    }

    private void OnDestroy()
    {
        // 清理按钮事件
        if (depositButton != null)
            depositButton.onClick.RemoveAllListeners();

        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();
    }
}
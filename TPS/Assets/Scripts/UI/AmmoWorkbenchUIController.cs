using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayerControl;

/// <summary>
/// 弹药工作台UI控制器，挂载在AmmoSupplyPanel上
/// </summary>
public class AmmoWorkbenchUIController : MonoBehaviour
{
    [Header("Input Field")]
    public TMP_InputField ammoAmountInput;      // 子弹数量输入框

    [Header("Cost Display")]
    public TextMeshProUGUI woodCostText;        // 木头消耗显示
    public TextMeshProUGUI ironCostText;        // 铁块消耗显示

    [Header("Buttons")]
    public Button confirmButton;                // 确认制作按钮
    public Button cancelButton;                 // 取消按钮

    [Header("References")]
    private AmmoWorkbenchController workbench;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private WeaponManager playerWeaponManager;
    [SerializeField] private WeaponManager aiWeaponManager;
    [SerializeField] PlayerInputSystem playerInputSystem;

    private void Start()
    {
        // 设置按钮事件
        SetupButtons();

        // 设置输入框事件
        SetupInputField();

        workbench = FindObjectOfType<AmmoWorkbenchController>(); // 直接查找唯一的工作台
    }

    private void OnEnable()
    {
        // 面板激活时刷新UI
        RefreshCostDisplay();
        ClearInputField();
        // 获取管理器引用
        inventoryManager = GameManager.Instance.GetInventoryManager();
        playerWeaponManager = GameManager.Instance.GetPlayerWeaponManager();
        playerInputSystem = GameManager.Instance.GetPlayerInputSystem();

        // 显示鼠标并进入存储模式
        playerInputSystem.EnterInteractMode();
    }

    private void OnDisable()
    {
        playerInputSystem.ExitInteractMode();
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtons()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }
    }

    /// <summary>
    /// 设置输入框事件
    /// </summary>
    private void SetupInputField()
    {
        if (ammoAmountInput != null)
        {
            ammoAmountInput.onValueChanged.AddListener(OnAmmoAmountChanged);
        }
    }

    /// <summary>
    /// 子弹数量输入变化时更新成本显示
    /// </summary>
    private void OnAmmoAmountChanged(string value)
    {
        RefreshCostDisplay();
    }

    /// <summary>
    /// 刷新成本显示
    /// </summary>
    private void RefreshCostDisplay()
    {
        if (workbench == null) return;

        int bulletAmount = GetInputAmount();

        if (bulletAmount > 0)
        {
            workbench.GetCraftingCost(bulletAmount, out int woodCost, out int ironCost);

            if (woodCostText != null)
                woodCostText.text = woodCost.ToString();

            if (ironCostText != null)
                ironCostText.text = ironCost.ToString();
        }
        else
        {
            if (woodCostText != null)
                woodCostText.text = "0";

            if (ironCostText != null)
                ironCostText.text = "0";
        }
    }

    /// <summary>
    /// 确认制作按钮点击事件
    /// </summary>
    private void OnConfirmButtonClicked()
    {
        int bulletAmount = GetInputAmount();

        if (bulletAmount <= 0)
        {
            Debug.Log("请输入要制作的子弹数量");
            return;
        }

        if (workbench == null || inventoryManager == null || playerWeaponManager == null)
        {
            Debug.LogError("缺少必要的组件引用");
            return;
        }

        // 尝试制作子弹
        bool success = workbench.TryCraftAmmo(bulletAmount, inventoryManager, playerWeaponManager);

        if (success)
        {
            Debug.Log($"成功制作 {bulletAmount} 发子弹");
            ClearInputField();
            RefreshCostDisplay();
        }
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void OnCancelButtonClicked()
    {
        CloseWorkbenchPanel();
    }

    /// <summary>
    /// 获取输入的子弹数量
    /// </summary>
    private int GetInputAmount()
    {
        if (ammoAmountInput == null || string.IsNullOrEmpty(ammoAmountInput.text))
            return 0;

        if (int.TryParse(ammoAmountInput.text, out int amount))
        {
            return Mathf.Max(0, amount);
        }
        return 0;
    }

    /// <summary>
    /// 清空输入框
    /// </summary>
    private void ClearInputField()
    {
        if (ammoAmountInput != null)
            ammoAmountInput.text = "";
    }

    /// <summary>
    /// 关闭工作台面板
    /// </summary>
    private void CloseWorkbenchPanel()
    {
        ClearInputField();
        UIManager.Instance?.HideAmmoWorkbenchPanel();
    }

    private void OnDestroy()
    {
        // 清理事件
        if (confirmButton != null)
            confirmButton.onClick.RemoveAllListeners();

        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();

        if (ammoAmountInput != null)
            ammoAmountInput.onValueChanged.RemoveAllListeners();
    }
}
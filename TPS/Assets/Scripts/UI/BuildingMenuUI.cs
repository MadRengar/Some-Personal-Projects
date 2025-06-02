using UnityEngine;
using UnityEngine.UI;
using PlayerControl;

/// <summary>
/// 建筑菜单UI控制脚本
/// 挂载在建筑菜单面板(buildingMenuPanel)上
/// </summary>
public class BuildingMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Button closeButton; // 关闭按钮
    public Button[] buildingButtons; // 建筑选择按钮数组

    [Header("Building Prefabs")]
    public GameObject[] buildingPrefabs; // 对应的建筑预制体数组
    
    [Header("UI Settings")]
    public string[] buildingNames; // 建筑名称数组（用于显示）
    public Sprite[] buildingIcons; // 建筑图标数组（可选）
    
    private PlayerInputSystem playerInputSystem;
    
    private void Awake()
    {
        // 获取PlayerInputSystem引用
        playerInputSystem = FindObjectOfType<PlayerInputSystem>();
        if (playerInputSystem == null)
        {
            Debug.LogError("BuildingMenuUI: 找不到PlayerInputSystem组件！");
        }
    }

    private void Start()
    {
        SetupUI();
    }

    private void OnEnable()
    {
        // 每次菜单激活时刷新UI
        RefreshUI();
    }
    
    /// <summary>
    /// 设置UI事件监听
    /// </summary>
    private void SetupUI()
    {
        // 设置关闭按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBuildingMenu);
        }
        
        // 设置建筑选择按钮事件
        for (int i = 0; i < buildingButtons.Length; i++)
        {
            if (buildingButtons[i] != null)
            {
                int buildingIndex = i; // 闭包变量
                buildingButtons[i].onClick.AddListener(() => SelectBuilding(buildingIndex));
                
                // 设置按钮显示内容
                SetupBuildingButton(buildingButtons[i], buildingIndex);
            }
        }
    }
    
    /// <summary>
    /// 设置建筑按钮的显示内容
    /// </summary>
    private void SetupBuildingButton(Button button, int index)
    {
        // 设置建筑名称
        //if (index < buildingNames.Length)
        //{
        //    var buttonText = button.GetComponentInChildren<Text>();
        //    if (buttonText != null)
        //    {
        //        buttonText.text = buildingNames[index];
        //    }
        //}
        
        // 设置建筑图标
        if (index < buildingIcons.Length && buildingIcons[index] != null)
        {
            Image iconImage = FindIconImage(button);
            if (iconImage != null)
            {
                iconImage.sprite = buildingIcons[index];
                Debug.Log($"成功设置按钮 {index} ({button.name}) 的图标: {buildingIcons[index].name}");
            }
            else
            {
                Debug.LogWarning($"未能找到按钮 {button.name} 的图标Image组件");
            }
        }
    }

    private Image FindIconImage(Button button)
    {
        // 方法1：查找名为"Icon"的子对象
        Transform iconTransform = button.transform.Find("Item/Icon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                Debug.Log($"通过名称'Icon'找到图标组件: {button.name}/Icon");
                return iconImage;
            }
        }
        Debug.LogWarning($"未能在按钮 {button.name} 中找到合适的图标Image组件");
        return null;
    }

    /// <summary>
    /// 刷新UI显示
    /// </summary>
    private void RefreshUI()
    {
        // 可以在这里添加动态更新UI的逻辑
        // 比如根据玩家资源情况禁用某些建筑按钮
        
        for (int i = 0; i < buildingButtons.Length; i++)
        {
            if (buildingButtons[i] != null)
            {
                // 示例：检查是否可以建造该建筑
                bool canBuild = CanBuildBuilding(i);
                buildingButtons[i].interactable = canBuild;
                
                // 可以根据可建造状态改变按钮颜色等
                //UpdateButtonVisual(buildingButtons[i], canBuild);
            }
        }
    }
    
    /// <summary>
    /// 检查是否可以建造指定建筑
    /// </summary>
    private bool CanBuildBuilding(int buildingIndex)
    {
        // 这里可以添加资源检查、解锁状态检查等逻辑
        // 目前返回true表示都可以建造
        
        // 示例逻辑：
        // if (buildingIndex < buildingPrefabs.Length && buildingPrefabs[buildingIndex] != null)
        // {
        //     // 检查玩家资源是否足够
        //     // 检查是否已解锁该建筑
        //     return true;
        // }
        
        return buildingIndex < buildingPrefabs.Length && buildingPrefabs[buildingIndex] != null;
    }
    
    /// <summary>
    /// 更新按钮视觉效果
    /// </summary>
    private void UpdateButtonVisual(Button button, bool canBuild)
    {
        if (button != null)
        {
            // 设置按钮透明度
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color color = buttonImage.color;
                color.a = canBuild ? 1f : 0.5f;
                buttonImage.color = color;
            }
            
            // 设置文本颜色
            var buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.color = canBuild ? Color.white : Color.gray;
            }
        }
    }
    
    /// <summary>
    /// 选择建筑
    /// </summary>
    private void SelectBuilding(int buildingIndex)
    {
        Debug.Log("[BuildingMenuUI]:选择了" + buildingIndex);
        // 检查索引有效性
        if (buildingIndex < 0 || buildingIndex >= buildingPrefabs.Length)
        {
            Debug.LogError($"BuildingMenuUI: 无效的建筑索引 {buildingIndex}");
            return;
        }
        
        // 检查预制体是否存在
        if (buildingPrefabs[buildingIndex] == null)
        {
            Debug.LogError($"BuildingMenuUI: 建筑预制体 {buildingIndex} 未分配");
            return;
        }
        
        // 检查是否可以建造
        if (!CanBuildBuilding(buildingIndex))
        {
            Debug.Log($"BuildingMenuUI: 无法建造 {(buildingIndex < buildingNames.Length ? buildingNames[buildingIndex] : "未知建筑")}");
            //UIManager.Instance?.ShowTip("资源不足或建筑未解锁", 2f);
            return;
        }
        
        Debug.Log($"BuildingMenuUI: 选择建造 {(buildingIndex < buildingNames.Length ? buildingNames[buildingIndex] : "建筑" + buildingIndex)}");
        
        // 这里需要通知建筑系统开始放置模式
        // 你可能需要一个BuildingManager来处理具体的建筑放置逻辑
        StartBuildingPlacement(buildingIndex);
        
        // 切换到放置模式
        if (playerInputSystem != null)
        {
            playerInputSystem.EnterPlacingMode();
        }
        
        // 可选：显示提示信息
        //UIManager.Instance?.ShowTip($"开始放置 {(buildingIndex < buildingNames.Length ? buildingNames[buildingIndex] : "建筑")}", 2f);
    }
    
    /// <summary>
    /// 开始建筑放置
    /// </summary>
    private void StartBuildingPlacement(int buildingIndex)
    {
        // 这里需要与你的建筑放置系统集成
        // 示例代码：
        
        /*
        var buildingManager = FindObjectOfType<BuildingManager>();
        if (buildingManager != null)
        {
            buildingManager.StartPlacement(buildingPrefabs[buildingIndex]);
        }
        else
        {
            Debug.LogError("BuildingMenuUI: 找不到BuildingManager组件！");
        }
        */
        
        // 临时代码 - 直接在玩家位置创建建筑（仅用于测试）
        var player = FindObjectOfType<ThirdPersonController>();
        if (player != null)
        {
            Vector3 spawnPosition = player.transform.position + player.transform.forward * 3f;
            Instantiate(buildingPrefabs[buildingIndex], spawnPosition, Quaternion.identity);
            Debug.Log($"已在玩家前方放置 {buildingNames[buildingIndex]}（测试模式）");
        }
    }
    
    /// <summary>
    /// 关闭建筑菜单
    /// </summary>
    public void CloseBuildingMenu()
    {
        Debug.Log("BuildingMenuUI: 关闭建筑菜单");
        
        // 通过PlayerInputSystem切换回战斗模式
        if (playerInputSystem != null)
        {
            playerInputSystem.EnterCombatMode();
        }
        else
        {
            // 备用方案：直接通过UIManager关闭
            UIManager.Instance?.HideBuildingMenu();
        }
    }
    
    /// <summary>
    /// 处理ESC键输入（由PlayerInputSystem调用）
    /// </summary>
    public void OnCancelInput()
    {
        CloseBuildingMenu();
    }
    
    private void OnDestroy()
    {
        // 清理事件监听器
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
        }
        
        for (int i = 0; i < buildingButtons.Length; i++)
        {
            if (buildingButtons[i] != null)
            {
                buildingButtons[i].onClick.RemoveAllListeners();
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using PlayerControl;

/// <summary>
/// 建筑菜单UI控制脚本
/// 挂载在建筑菜单面板(buildingMenuPanelUI)上
/// </summary>
public class BuildingMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Button closeButton; // 关闭按钮
    public Button[] buildingButtons; // 建筑选择按钮数组

    [Header("Category System")]
    public Button[] categoryButtons; // TURRET, GENERATOR等分类按钮
    public GameObject[] categoryPanels; // 对应每个分类的建筑按钮面板

    [Header("Current Category")]
    private int currentCategoryIndex = 0; // 当前选中的分类

    [Header("Building Prefabs")]
    public GameObject[] buildingPrefabs; // 对应的建筑预制体数组
    public GameObject[] previewPrefabs;  // 透明模型 prefab 数组

    [Header("UI Settings")]
    public string[] buildingNames; // 建筑名称数组（用于显示）
    public Sprite[] buildingIcons; // 建筑图标数组（可选）


    public PlayerInputSystem playerInputSystem;
    public BuildingSystem buildingSystem;
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

        // 设置分类按钮
        SetupCategoryButtons();

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

        // 默认显示第一个分类
        ShowCategory(0);
    }
    
    /// <summary>
    /// 设置建筑按钮的显示内容
    /// </summary>
    private void SetupBuildingButton(Button button, int index)
    {
        // 可能会 设置建筑名称
        
        // 设置建筑图标
        if (index < buildingIcons.Length && buildingIcons[index] != null)
        {
            Image iconImage = FindIconImage(button);
            if (iconImage != null)
            {
                iconImage.sprite = buildingIcons[index];
                //Debug.Log($"成功设置按钮 {index} ({button.name}) 的图标: {buildingIcons[index].name}");
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
                return iconImage;
            }
        }
        Debug.LogWarning($"未能在按钮 {button.name} 中找到合适的图标Image组件");
        return null;
    }
    /// <summary>
    /// 设置分类按钮
    /// </summary>
    private void SetupCategoryButtons()
    {
        Debug.Log($"设置分类按钮，总数: {categoryButtons.Length}");
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            if (categoryButtons[i] != null)
            {
                Debug.Log($"设置分类按钮 {i}: {categoryButtons[i].name}");
                int categoryIndex = i;
                categoryButtons[i].onClick.AddListener(() => {
                    Debug.Log($"点击了分类按钮: {categoryIndex}");
                    ShowCategory(categoryIndex);
                });
            }
            else
            {
                Debug.LogError($"分类按钮 {i} 为空!");
            }
        }
    }

    /// <summary>
    /// 显示指定分类
    /// </summary>
    private void ShowCategory(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= categoryPanels.Length) return;

        currentCategoryIndex = categoryIndex;

        // 隐藏所有分类面板
        for (int i = 0; i < categoryPanels.Length; i++)
        {
            if (categoryPanels[i] != null)
            {
                categoryPanels[i].SetActive(false);
            }
        }

        // 显示当前分类面板
        if (categoryPanels[categoryIndex] != null)
        {
            categoryPanels[categoryIndex].SetActive(true);
        }

        // 更新分类按钮状态（高亮当前选中的）
        UpdateCategoryButtonStates(categoryIndex);

        Debug.Log($"切换到分类: {categoryIndex}");
    }

    /// <summary>
    /// 更新分类按钮的视觉状态
    /// </summary>
    private void UpdateCategoryButtonStates(int selectedIndex)
    {
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            if (categoryButtons[i] != null)
            {
                // 可以通过修改按钮颜色或其他方式来表示选中状态
                var colors = categoryButtons[i].colors;
                if (i == selectedIndex)
                {
                    colors.normalColor = Color.yellow; // 选中状态颜色
                }
                else
                {
                    colors.normalColor = Color.white; // 未选中状态颜色
                }
                categoryButtons[i].colors = colors;
            }
        }
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
       
        StartBuildingPlacement(buildingIndex);
       
    }
    
    /// <summary>
    /// 开始建筑放置
    /// </summary>
    private void StartBuildingPlacement(int buildingIndex)
    {
        GameObject buildingPrefab = buildingPrefabs[buildingIndex];

        IBuildingController buildingController = buildingPrefab.GetComponent<IBuildingController>();

        if (buildingSystem != null)
        {
            BuildingData_SO buildingData = buildingController.GetBuildingData();

            if (buildingSystem.CheckResourcesIsEnough(buildingData))
            {
                playerInputSystem.EnterPlacingMode();
                buildingSystem.StartPlacement(buildingPrefabs[buildingIndex], previewPrefabs[buildingIndex]);                
            }
            else
            {

                RadioPopController.Instance.ShowMessage(MessageKey.Build_no_resource, RadioPopController.MessageType.Warning);
                return;
            }
            
        }
        else
        {
            Debug.LogError("BuildingMenuUI: 找不到BuildingSystem组件！");
        }
    }
    
    /// <summary>
    /// 关闭建筑菜单
    /// </summary>
    public void CloseBuildingMenu()
    {
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
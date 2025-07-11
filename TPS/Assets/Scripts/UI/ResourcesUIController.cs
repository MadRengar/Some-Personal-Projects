using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourcesUIController : MonoBehaviour
{
    [Header("Resources UI")]
    [SerializeField] private TextMeshProUGUI woodNumUI;
    [SerializeField] private TextMeshProUGUI ironNumUI;
    [SerializeField] private TextMeshProUGUI powerUI;

    private InventoryManager inventoryManager;
    private int woodCount;
    private int ironCount;
    private int powerCount;

    void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();
        RefreshResourcesUI();
        InventoryManager.OnResourcesChanged += RefreshResourcesUI;
    }

    private void OnDestroy()
    {
        InventoryManager.OnResourcesChanged -= RefreshResourcesUI;
    }

    /// <summary>
    /// 刷新资源UI显示
    /// </summary>
    public void RefreshResourcesUI()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("ResourcesUIController: InventoryManager 为空，无法更新UI");
            return;
        }

        // 直接获取总资源数量（玩家+AI+仓库）
        woodCount = inventoryManager.GetTotalResourceIncludingAllStorage(ResourceType.Wood);
        ironCount = inventoryManager.GetTotalResourceIncludingAllStorage(ResourceType.Iron);
        powerCount = inventoryManager.GetAllGeneratorPower();
        // 更新UI显示
        if (woodNumUI != null)
            woodNumUI.text = woodCount.ToString();

        if (ironNumUI != null)
            ironNumUI.text = ironCount.ToString();
        if (powerUI != null)
            powerUI.text = powerCount.ToString();

    }
}

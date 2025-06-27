using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourcesUIController : MonoBehaviour
{
    [Header("Resources UI")]
    [SerializeField] private TextMeshProUGUI woodNumUI;
    [SerializeField] private TextMeshProUGUI ironNumUI;

    private InventoryManager inventoryManager;
    private int woodCount;
    private int ironCount;

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

        // 更新UI显示
        if (woodNumUI != null)
            woodNumUI.text = woodCount.ToString();

        if (ironNumUI != null)
            ironNumUI.text = ironCount.ToString();

        Debug.Log($"UI更新：木材={woodCount}, 铁矿={ironCount}");
    }
}

using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    public LayerMask groundLayer = 1;           // 地面图层

    [Header("Manager Ref")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Debug")]
    public bool enableDebugLog = true;

    [Header("Preview Materials")]
    public Material canBuildMaterial;
    public Material cantBuildMaterial;

    [Header("Running Data(From InventoryManager)")]
    [SerializeField] private int woodCount;
    [SerializeField] private int ironCount;
    // 运行时变量
    private Camera playerCamera;                // 主摄像机（运行时获取）
    private GameObject currentPreview;          // 当前预览的建筑
    private GameObject previewPrefab;           // 透明模型
    private GameObject buildPrefab;          // 要放置的建筑预制体
    private TurretData_SO currentBuildingData;
    private bool isPlacing = false;             // 是否处于放置状态

    private void Start()
    {
        // 获取主摄像机（Cinemachine会控制这个摄像机）
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("[BuildingSystem] 找不到主摄像机！");
        }
    }

    private void Update()
    {
        // 只在放置模式下更新预览位置
        if (isPlacing && currentPreview != null)
        {
            UpdatePreviewPosition();
        }
        GetResourcesFromInventoryManager();
    }

    /// <summary>
    /// 从 InventoryManager拿到目前的资源数
    /// </summary>
    public void GetResourcesFromInventoryManager()
    {
        woodCount = inventoryManager.GetTotalResourceByType(ResourceType.Wood);
        ironCount = inventoryManager.GetTotalResourceByType(ResourceType.Iron);
    }

    /// <summary>
    /// 检查资源是否足够
    /// </summary>
    public bool CheckResourcesIsEnough(TurretData_SO buildingData)
    {
        if (buildingData.requiredWoodNum <= woodCount && buildingData.requiredIronNum <= ironCount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// 开始放置模式
    /// </summary>
    public void StartPlacement(GameObject realBuild, GameObject preview)
    {
        if (realBuild == null || preview == null) return;

        TurretController turretController = realBuild.GetComponent<TurretController>();
        if (turretController != null)
        {
            currentBuildingData = turretController.turretData;
        }


        buildPrefab = realBuild;
        previewPrefab = preview;
        isPlacing = true;
        // 创建预览模型
        CreatePreview();
    }

    /// <summary>
    /// 创建预览模型
    /// </summary>
    private void CreatePreview()
    {
        if (previewPrefab == null) return;

        currentPreview = Instantiate(previewPrefab);
    }

    /// <summary>
    /// 更新预览位置
    /// </summary>
    private void UpdatePreviewPosition()
    {
        if (playerCamera == null || currentPreview == null) return;

        // 从屏幕中心发出射线
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);

        // 射线检测地面
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // 更新预览位置到射线击中点
            currentPreview.transform.position = hit.point;
            // 添加营地检测
            bool isInCamp = CampZoneManager.Instance?.IsPositionInCamp(hit.point) ?? false;

            ChangePreviewColor(isInCamp);
        }
    }

    /// <summary>
    /// 改变预览颜色指示是否可建造
    /// </summary>
    /// <summary>
    /// 改变预览颜色指示是否可建造
    /// </summary>
    private void ChangePreviewColor(bool canBuild)
    {
        if (currentPreview == null) return;

        Material targetMaterial = canBuild ? canBuildMaterial : cantBuildMaterial;
        if (targetMaterial == null) return;

        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            // 替换所有材质为目标材质
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = targetMaterial;
            }
            renderer.materials = materials;
        }
    }

    /// <summary>
    /// 确认放置
    /// </summary>
    public void ConfirmPlacement()
    {
        if (!isPlacing || currentPreview == null || buildPrefab == null) return;

        Vector3 pos = currentPreview.transform.position;
        Quaternion rot = currentPreview.transform.rotation;

        //GameObject building = Instantiate(buildPrefab, pos, rot);
        GameObject buildingPreview = CreateBuildablePreview(pos, rot);

    if (enableDebugLog)
        Debug.Log($"[BuildingSystem] 建筑预览已放置，等待建造: {buildingPreview.name} at {pos}");

        EndPlacement();
    }

    private GameObject CreateBuildablePreview(Vector3 position, Quaternion rotation)
    {
        // 使用相同的预览预制体，但这次是永久的
        GameObject buildingPreview = Instantiate(previewPrefab, position, rotation);

        // 添加或获取 BuildingProgress 组件
        BuildingProgress buildingProgress = buildingPreview.GetComponent<BuildingProgress>();
        if (buildingProgress == null)
        {
            buildingProgress = buildingPreview.AddComponent<BuildingProgress>();
        }

        // 设置建造数据
        buildingProgress.SetFinalBuildingPrefab(buildPrefab);
        buildingProgress.SetProgressSettings(100f, 10f); // 100% 总进度，每次 20%

        // 确保有正确的标签
        buildingPreview.tag = "BuildingPreview";

        return buildingPreview;
    }

    /// <summary>
    /// 取消放置
    /// </summary>
    public void CancelPlacement()
    {
        if (!isPlacing) return;

        if (enableDebugLog)
            Debug.Log("[BuildingSystem] 取消放置");

        // 结束放置模式
        EndPlacement();
    }

    /// <summary>
    /// 结束放置模式
    /// </summary>
    private void EndPlacement()
    {
        // 清理预览模型
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }

        // 重置状态
        isPlacing = false;
        previewPrefab = null;

        if (enableDebugLog)
            Debug.Log("[BuildingSystem] 放置模式已结束");
    }

    /// <summary>
    /// 获取当前是否处于放置状态
    /// </summary>
    public bool IsPlacing()
    {
        return isPlacing;
    }

    public TurretData_SO GetCurrentBuildingData()
    {
        return currentBuildingData;
    }

    // 在BuildingSystem.cs中添加getter
    public GameObject GetCurrentPreview()
    {
        return currentPreview;
    }
}

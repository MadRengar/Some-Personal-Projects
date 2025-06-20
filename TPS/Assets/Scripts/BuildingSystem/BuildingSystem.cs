using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    public LayerMask groundLayer = 1;           // 地面图层

    [Header("Debug")]
    public bool enableDebugLog = true;

    // 运行时变量
    private Camera playerCamera;                // 主摄像机（运行时获取）
    private GameObject currentPreview;          // 当前预览的建筑
    private GameObject buildingPrefab;          // 要放置的建筑预制体
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
    }

    /// <summary>
    /// 开始放置模式
    /// </summary>
    public void StartPlacement(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[BuildingSystem] 传入的预制体为空！");
            return;
        }

        buildingPrefab = prefab;
        isPlacing = true;

        // 创建预览模型
        CreatePreview();
    }

    /// <summary>
    /// 创建预览模型
    /// </summary>
    private void CreatePreview()
    {
        if (buildingPrefab == null) return;

        // 实例化预览模型
        currentPreview = Instantiate(buildingPrefab);

        // 设置预览状态
        SetupPreview(currentPreview);
    }

    /// <summary>
    /// 设置预览模型的状态
    /// </summary>
    private void SetupPreview(GameObject preview)
    {
        // 禁用所有碰撞器
        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // 设置预览材质 - 保持原有材质，只修改为半透明
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Material[] originalMaterials = renderer.materials;
            Material[] previewMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                // 创建原材质的副本并设置为半透明
                previewMaterials[i] = new Material(originalMaterials[i]);

                // 设置为半透明模式
                previewMaterials[i].SetFloat("_Mode", 3); // Transparent mode
                previewMaterials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                previewMaterials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                previewMaterials[i].SetInt("_ZWrite", 0);
                previewMaterials[i].DisableKeyword("_ALPHATEST_ON");
                previewMaterials[i].EnableKeyword("_ALPHABLEND_ON");
                previewMaterials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                previewMaterials[i].renderQueue = 3000;

                // 设置透明度
                Color color = previewMaterials[i].color;
                color.a = 0.5f; // 50%透明度
                previewMaterials[i].color = color;
            }

            renderer.materials = previewMaterials;
        }

        // 添加预览标签
        preview.tag = "Preview";
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
        }
    }

    /// <summary>
    /// 确认放置
    /// </summary>
    public void ConfirmPlacement()
    {
        if (!isPlacing || currentPreview == null) return;

        // 在预览位置实例化真正的建筑
        Vector3 buildPosition = currentPreview.transform.position;
        Quaternion buildRotation = currentPreview.transform.rotation;

        GameObject building = Instantiate(buildingPrefab, buildPosition, buildRotation);

        if (enableDebugLog)
            Debug.Log($"[BuildingSystem] 建筑已放置: {building.name} at {buildPosition}");

        // 结束放置模式
        EndPlacement();
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
        buildingPrefab = null;

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
}

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
    private GameObject previewPrefab;           // 透明模型
    private GameObject buildPrefab;          // 要放置的建筑预制体

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
    public void StartPlacement(GameObject realBuild, GameObject preview)
    {
        if (realBuild == null || preview == null)
        {
            Debug.LogError("[BuildingSystem] 建筑 prefab 或 预览 prefab 为空！");
            return;
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

        GameObject building = Instantiate(buildPrefab, pos, rot);

        if (enableDebugLog)
            Debug.Log($"[BuildingSystem] 建筑已放置: {building.name} at {pos}");

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
}

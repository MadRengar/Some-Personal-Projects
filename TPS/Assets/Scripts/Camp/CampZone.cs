using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public string campName = "Main Camp";
    public Vector3 campSize = new Vector3(100f, 10f, 80f); // 长x高x宽

    [Header("Visual Effects")]
    public Color boundaryColor = Color.green;
    public GameObject visualOverlay; // 手动拖拽的视觉覆盖物体

    // 营地类型
    public enum CampType
    {
        MainBase,    // 主基地
        TurretArea,     // 炮台放置区域
        GeneratorArea // 发电机区域
    }

    [Header("Camp Type")]
    public CampType campType = CampType.MainBase;

    private Collider campCollider;

    private void Start()
    {
        InitializeCampZone();
    }

    private void InitializeCampZone()
    {
        // 设置层级
        gameObject.layer = LayerMask.NameToLayer("CampZone");

        // 获取或创建碰撞器
        campCollider = GetComponent<Collider>();
        if (campCollider == null)
        {
            // 创建Box碰撞器
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = campSize;
            boxCollider.isTrigger = true;
            campCollider = boxCollider;
        }
        else
        {
            // 如果已有Box Collider，确保尺寸一致
            if (campCollider is BoxCollider boxCol)
            {
                boxCol.size = campSize;
            }
        }

        campCollider.isTrigger = true;

        // 注册到管理器
        if (CampZoneManager.Instance != null)
        {
            CampZoneManager.Instance.RegisterCampZone(this);
        }

        // 创建视觉效果
        CreateVisualEffects();
    }

    /// <summary>
    /// 检查位置是否在营地范围内
    /// </summary>
    public bool IsPositionInZone(Vector3 position)
    {
        if (campCollider == null) return false;

        // 使用碰撞器的边界检测
        return campCollider.bounds.Contains(position);
    }

    /// <summary>
    /// 显示营地区域
    /// </summary>
    public void ShowCampArea()
    {
        if (visualOverlay != null)
        {
            visualOverlay.SetActive(true);
            // 确保大小正确
            CreateVisualEffects();
        }
    }

    /// <summary>
    /// 隐藏营地区域
    /// </summary>
    public void HideCampArea()
    {
        if (visualOverlay != null)
        {
            visualOverlay.SetActive(false);
        }
    }


    /// <summary>
    /// 创建视觉效果
    /// </summary>
    private void CreateVisualEffects()
    {
        if (visualOverlay == null) return;

        if (campCollider is BoxCollider boxCol)
        {
            visualOverlay.transform.localScale = new Vector3(
                boxCol.size.x / 10f,
                1,
                boxCol.size.z / 10f
            );

            // 确保位置正确
            visualOverlay.transform.position = transform.position + new Vector3(0, 0.05f, 0);
        }
    }

    private void OnDestroy()
    {
        // 从管理器注销
        if (CampZoneManager.Instance != null)
        {
            CampZoneManager.Instance.UnregisterCampZone(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制营地范围（矩形）
        Gizmos.color = boundaryColor;
        Gizmos.DrawWireCube(transform.position, campSize);
    }
}

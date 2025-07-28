using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Ping Manager负责管理ping的所有逻辑
/// 包括：生成逻辑（位置、UI）、向其他脚本提供标记的位置、标记激活的标志
/// </summary>
public class PingMarkerManager : MonoBehaviour
{
    public GameObject markerPrefab;
    public LayerMask groundLayer; // 用来射线检测地面层



    [Header("2D UI")]
    public RectTransform markerUIIconPrefab;
    public Canvas uiCanvas;
    public GameObject tipUI;

    private GameObject currentMarker;
    private RectTransform currentMarkerUI;
    private PlayerInputSystem playerInputSystem;
    private TextMeshProUGUI distanceText;
    private float distance;

    private Vector3 currentMarkedPosition;// 标记位置
    [SerializeField] private bool pingCommandActive;// 标记状态

    // 新增：标记目标信息
    [SerializeField] private bool isCurrentTargetBuilding = false; // 当前标记是否为建筑物
    [SerializeField] private GameObject currentMarkedBuilding = null; // 当前标记的建筑物
    [SerializeField] private string currentTargetTag = ""; // 当前标记目标的Tag

    private void Start()
    {
        playerInputSystem = GameManager.Instance.GetPlayerInputSystem();
    }

    void Update()
    {
        if (playerInputSystem == null)
        {
            Debug.Log("Can't find playerInputSystem！");
            return;
        }
        if (playerInputSystem.ping)
        {
            playerInputSystem.ping = false;
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter); // 屏幕中心发射射线

            if (CancelMarkIfHitUI(screenCenter))
            {
                return;
            }

            // 先尝试检测建筑物
            if (TryMarkBuilding(ray))
            {
                return;
            }

            // 再检测地面
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, groundLayer))
            {
                CreateGroundMarker(raycastHit);
            }
        }
        Show2DUIMarker();
    }

    private bool TryMarkBuilding(Ray ray)
    {
        // 检测除地面外的所有物体
        int allLayersExceptGround = ~groundLayer.value;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, allLayersExceptGround))
        {
            GameObject hitObject = hit.collider.gameObject;

            // 检查是否是可标记的建筑物
            if (IsValidBuildingTarget(hitObject))
            {
                CreateBuildingMarker(hit, hitObject);
                return true;
            }
        }
        return false;
    }

    private bool IsValidBuildingTarget(GameObject obj)
    {
        if (obj == null) return false;
        return obj.CompareTag("PlayerBuilding");
    }

    private void CreateBuildingMarker(RaycastHit hit, GameObject building)
    {
        ClearCurrentMarker();

        currentMarkedPosition = hit.point;

        // 设置建筑物标记信息
        isCurrentTargetBuilding = true;
        currentMarkedBuilding = building;
        currentTargetTag = building.tag;

        /* 3D Marker */
        Vector3 spawnPosition = hit.point + Vector3.up * 0.5f;
        currentMarker = Instantiate(markerPrefab, spawnPosition, Quaternion.identity);
        pingCommandActive = true;

        /* 2D Marker */
        currentMarkerUI = Instantiate(markerUIIconPrefab, uiCanvas.transform);
        distanceText = currentMarkerUI.GetComponentInChildren<TextMeshProUGUI>();

        /* 2D TipUI */
        ShowTipUI();

        Debug.Log($"标记建筑物: {building.name} (Tag: {building.tag})");
    }

    private void CreateGroundMarker(RaycastHit hit)
    {
        ClearCurrentMarker();

        currentMarkedPosition = hit.point;

        // 设置地面标记信息
        isCurrentTargetBuilding = false;
        currentMarkedBuilding = null;
        currentTargetTag = "";

        /* 3D Marker */
        Vector3 spawnPosition = hit.point + Vector3.up * 0.5f;
        currentMarker = Instantiate(markerPrefab, spawnPosition, Quaternion.identity);
        pingCommandActive = true;

        /* 2D Marker */
        currentMarkerUI = Instantiate(markerUIIconPrefab, uiCanvas.transform);
        distanceText = currentMarkerUI.GetComponentInChildren<TextMeshProUGUI>();

        /* 2D TipUI */
        ShowTipUI();

        Debug.Log($"标记地面位置: {hit.point}");
    }

    private void ClearCurrentMarker()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null;
        }

        if (currentMarkerUI != null)
        {
            Destroy(currentMarkerUI.gameObject);
            currentMarkerUI = null;
        }
    }

    private void Show2DUIMarker()
    {
        if (currentMarker != null && currentMarkerUI != null)
        {
            Vector3 MarkerUIScreenPos = Camera.main.WorldToScreenPoint(currentMarker.transform.position);
            distance = Vector3.Distance(GameManager.Instance.GetPlayerTransform().position, currentMarker.transform.position); // 距离Text计算

            if (MarkerUIScreenPos.z > 0)
            {
                currentMarkerUI.gameObject.SetActive(true);
                currentMarkerUI.position = MarkerUIScreenPos;
                //显示距离Text
                if (distanceText != null)
                {
                    string markerType = isCurrentTargetBuilding ? "建筑" : "位置";
                    distanceText.text = $"{markerType}\n{distance:F1}m";
                }
            }
            else
            {
                currentMarkerUI.gameObject.SetActive(false);
            }
        }
    }

    private void ShowTipUI()
    {
        if (tipUI != null)
        {
            tipUI.SetActive(true);
        }
    }

    private void HideTipUI()
    {
        if (tipUI != null)
        {
            tipUI.SetActive(false);
        }
    }

    private bool CancelMarkIfHitUI(Vector2 screenCenter)
    {
        // 屏幕中心检测
        if (currentMarkerUI != null &&
                RectTransformUtility.RectangleContainsScreenPoint(currentMarkerUI, screenCenter, null))
        {
            ClearCurrentMarker();

            // 清空数据
            isCurrentTargetBuilding = false;
            currentMarkedBuilding = null;
            currentTargetTag = "";
            pingCommandActive = false;
            currentMarkedPosition = Vector3.zero;

            // 隐藏TipUI
            HideTipUI();
            return true;
        }
        return false;
    }

    #region Getter
    /*向其他脚本提供 激活标志、ping位置信息 这两个函数*/
    public bool GetPingCommandActive()
    {
        return pingCommandActive;
    }

    public Vector3 GetCurrentMarkedPosition()
    {
        return currentMarkedPosition;
    }

    // 新增：获取标记类型信息的简单接口
    public bool IsCurrentTargetBuilding()
    {
        return isCurrentTargetBuilding;
    }

    public GameObject GetCurrentMarkedBuilding()
    {
        return currentMarkedBuilding;
    }

    public string GetCurrentTargetTag()
    {
        return currentTargetTag;
    }
    #endregion
}
using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Ping Manager应该管理ping的所有逻辑
/// 包括：生成逻辑（位置、UI），向其他脚本提供标记的位置、标记激活的标志
/// </summary>
public class PingMarkerManager : MonoBehaviour
{
    public GameObject markerPrefab;
    public LayerMask groundLayer; // 用来射线检测地面
    public Transform markerParent; // 标记统一收纳在空物体下


    [Header("2D UI")]
    public RectTransform markerUIIconPrefab;
    public Canvas uiCanvas;

    private GameObject currentMarker;
    private RectTransform currentMarkerUI;
    private PlayerInputSystem playerInputSystem;
    private TextMeshProUGUI distanceText;
    private float distance;

    private Vector3 currentMarkedPosition;// 标记位置
    private bool pingCommandActive;// 标记状态

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

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, groundLayer))
            {
                if(CancelMarkIfHitUI(screenCenter))
                {
                    return;
                }
                currentMarkedPosition = raycastHit.point;
                /* 3D Marker*/
                if (currentMarker != null)
                {
                    Destroy(currentMarker);
                }
                Vector3 spawnPosition = raycastHit.point + Vector3.up * 0.5f; // 抬高 0.5 米
                currentMarker = Instantiate(markerPrefab, spawnPosition, Quaternion.identity, markerParent);
                pingCommandActive = true;

                /* 2D Marker*/
                if (currentMarkerUI != null)
                {
                    Destroy(currentMarkerUI.gameObject);
                }
                currentMarkerUI = Instantiate(markerUIIconPrefab, uiCanvas.transform);
                distanceText = currentMarkerUI.GetComponentInChildren<TextMeshProUGUI>();
                //Debug.Log("标记位置: " + raycastHit.point);
            }
        }
        Show2DUIMarker();
    }

    void Show2DUIMarker()
    {
        if(currentMarker != null && currentMarkerUI != null)
        {
            Vector3 MarkerUIScreenPos = Camera.main.WorldToScreenPoint(currentMarker.transform.position);
            distance = Vector3.Distance(GameManager.Instance.GetPlayerTransform().position, currentMarker.transform.position); // 距离Text计算

            if (MarkerUIScreenPos.z > 0)
            {
                currentMarkerUI.gameObject.SetActive(true);
                currentMarkerUI.position = MarkerUIScreenPos;
                //显示距离Text
                if(distanceText != null)
                {
                    distanceText.text = $"{distance:F1}m";
                }
            }
            else
            {
                currentMarkerUI.gameObject.SetActive(false);
            }
        }
    }

    bool CancelMarkIfHitUI(Vector2 screenCenter)
    {
        // 屏幕中心
        if (currentMarkerUI != null &&
                RectTransformUtility.RectangleContainsScreenPoint(currentMarkerUI, screenCenter, null))
        {
            if (currentMarker != null)
            {
                Destroy(currentMarker);
                currentMarker = null;
            }

            Destroy(currentMarkerUI.gameObject);
            currentMarkerUI = null;
            //清空数据
            pingCommandActive = false;
            currentMarkedPosition = Vector3.zero;
            return true;
        }
        return false;
    }

    /*向其他脚本提供 激活标志、ping位置信息 重要函数*/
    public bool GetPingCommandActive()
    {
        return pingCommandActive;
    }

    public Vector3 GetCurrentMarkedPosition()
    {
        return currentMarkedPosition;
    }
}

using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PingMarkerManager : MonoBehaviour
{
    public GameObject markerPrefab;
    public LayerMask groundLayer; // 用来射线检测地面
    public Transform markerParent; // 标记统一收纳在空物体下
    public Vector3 currentMarkedPosition;
    [Header("2D UI")]
    public RectTransform markerUIIconPrefab;
    public Canvas uiCanvas;

    private GameObject currentMarker;
    private RectTransform currentMarkerUI;
    private PlayerInputSystem playerInputSystem;
    private TextMeshProUGUI distanceText;
    private float distance;


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
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter); // 屏幕中心发射射线

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, groundLayer))
            {
                currentMarkedPosition = raycastHit.point;
                /* 3D Marker*/
                if (currentMarker != null)
                {
                    Destroy(currentMarker);
                }
                Vector3 spawnPosition = raycastHit.point + Vector3.up * 1.0f; // 抬高 0.5 米
                currentMarker = Instantiate(markerPrefab, spawnPosition, Quaternion.identity, markerParent);

                /* 2D Marker*/
                if (currentMarkerUI != null)
                {
                    Destroy(currentMarkerUI.gameObject);
                }
                currentMarkerUI = Instantiate(markerUIIconPrefab, uiCanvas.transform);
                distanceText = currentMarkerUI.GetComponentInChildren<TextMeshProUGUI>();

                playerInputSystem.ping = false;
                Debug.Log("标记位置: " + raycastHit.point);
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
}

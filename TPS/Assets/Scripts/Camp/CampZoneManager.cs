using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampZoneManager : MonoBehaviour
{
    [Header("Camp Zone Settings")]
    public LayerMask campZoneLayer = 1 << 10; // 营地层

    [Header("Camp Value")]
    [SerializeField] private bool playerInCampZone;
    [SerializeField] private float staminaRecoverRate = 3f;

    [Header("Camp Buildings Ref")]
    [SerializeField] private GameObject treatmentArea;
    [SerializeField] private GameObject foodSupplyArea;
    [SerializeField] private GameObject ammoSupplyArea;

    private static CampZoneManager _instance;
    public static CampZoneManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<CampZoneManager>();
            return _instance;
        }
    }

    private List<CampZone> campZones = new List<CampZone>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 订阅玩家模式切换事件
        PlayerInputSystem.OnModeChanged += HandleModeChange;

        // 初始状态隐藏所有营地区域
        HideAllCampAreas();
    }

    private void OnDestroy()
    {
        // 取消订阅
        PlayerInputSystem.OnModeChanged -= HandleModeChange;
    }

    /// <summary>
    /// 处理模式切换
    /// </summary>
    private void HandleModeChange(PlayerInputSystem.PlayerMode newMode)
    {
        switch (newMode)
        {
            case PlayerInputSystem.PlayerMode.Placing:
                ShowAllCampAreas();
                break;

            case PlayerInputSystem.PlayerMode.Combat:
                HideAllCampAreas();
                break;
            case PlayerInputSystem.PlayerMode.BuildMenu:
                ShowAllCampAreas();
                break;
        }
    }

    /// <summary>
    /// 注册营地区域
    /// </summary>
    public void RegisterCampZone(CampZone zone)
    {
        if (!campZones.Contains(zone))
        {
            campZones.Add(zone);
            //Debug.Log($"注册营地区域: {zone.name}");
        }
    }

    /// <summary>
    /// 注销营地区域
    /// </summary>
    public void UnregisterCampZone(CampZone zone)
    {
        if (campZones.Contains(zone))
        {
            campZones.Remove(zone);
            //Debug.Log($"注销营地区域: {zone.name}");
        }
    }

    /// <summary>
    /// 检查指定位置是否在营地内
    /// </summary>
    public bool IsPositionInCamp(Vector3 position)
    {
        return IsPositionInAnyCamp(position);
    }

    /// <summary>
    /// 检查指定位置是否在任何营地区域内
    /// </summary>
    public bool IsPositionInAnyCamp(Vector3 position)
    {
        foreach (var zone in campZones)
        {
            if (zone != null && zone.IsPositionInZone(position))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取位置所在的营地区域
    /// </summary>
    public CampZone GetCampZoneAtPosition(Vector3 position)
    {
        foreach (var zone in campZones)
        {
            if (zone != null && zone.IsPositionInZone(position))
            {
                return zone;
            }
        }
        return null;
    }

    /// <summary>
    /// 检查建造位置是否有效（在营地内且在地面上）
    /// </summary>
    public bool IsValidBuildPosition(Vector3 position, LayerMask groundMask)
    {
        // 1. 检查是否在地面上
        bool isOnGround = Physics.Raycast(position + Vector3.up * 5f, Vector3.down, 10f, groundMask);

        // 2. 检查是否在营地内
        bool isInCamp = IsPositionInCamp(position);

        return isOnGround && isInCamp;
    }

    /// <summary>
    /// 显示所有营地区域
    /// </summary>
    public void ShowAllCampAreas()
    {
        foreach (var zone in campZones)
        {
            if (zone != null)
            {
                zone.ShowCampArea();
            }
        }
    }

    /// <summary>
    /// 隐藏所有营地区域
    /// </summary>
    public void HideAllCampAreas()
    {
        foreach (var zone in campZones)
        {
            if (zone != null)
            {
                zone.HideCampArea();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("玩家正在营地中！");
            playerInCampZone = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCampZone = false;
        }
    }

    #region Getter
    public bool IsPlayerInCampZone()
    {
        return playerInCampZone;
    }

    public float GetStaminaRecoverRate()
    {
        return staminaRecoverRate;
    }
    public GameObject GetTreatmentArea()
    {
        return treatmentArea;
    }
    public GameObject GetAmmoSupplyArea()
    {
        return ammoSupplyArea;
    }
    public GameObject GetFoodSupplyArea()
    {
        return foodSupplyArea;
    }
    #endregion
}

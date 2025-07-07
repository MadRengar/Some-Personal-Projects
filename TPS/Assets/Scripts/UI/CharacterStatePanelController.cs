using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 角色状态面板控制器
/// 负责更新角色状态面板中的所有UI元素
/// </summary>
public class CharacterStatePanelController : MonoBehaviour
{
    [Header("Player State UI References")]
    public TextMeshProUGUI playerWoodText;
    public TextMeshProUGUI playerIronText;
    public TextMeshProUGUI playerAmmoText;
    public TextMeshProUGUI playerBagText;
    public TextMeshProUGUI playerHPText;

    [Header("AI Teammate State UI References")]
    public TextMeshProUGUI aiWoodText;
    public TextMeshProUGUI aiIronText;
    public TextMeshProUGUI aiAmmoText;
    public TextMeshProUGUI aiBagText;
    public TextMeshProUGUI aiHPText;

    [Header("Manager References")]
    private InventoryManager inventoryManager;
    private PlayerStats playerStats;
    private AITeammateState aiTeammateState;
    private WeaponManager playerWeaponManager;
    private WeaponManager aiWeaponManager;

    private void Start()
    {
        // 获取管理器引用
        GetManagerReferences();
    }

    private void OnEnable()
    {
        // 每次面板激活时更新数据
        UpdatePanelData();
    }

    /// <summary>
    /// 获取所有需要的管理器引用
    /// </summary>
    private void GetManagerReferences()
    {
        // 获取InventoryManager
        inventoryManager = FindObjectOfType<InventoryManager>();

        // 通过GameManager获取PlayerStats和AITeammateState
        if (GameManager.Instance != null)
        {
            playerStats = GameManager.Instance.GetPlayerStats();
            aiTeammateState = GameManager.Instance.GetAIAgentStats();

            // 获取玩家的WeaponManager（通过Player对象）
            GameObject player = GameManager.Instance.player;
            if (player != null)
            {
                playerWeaponManager = player.GetComponentInChildren<WeaponManager>();
            }

            // 获取AI的WeaponManager（通过AI对象）
            GameObject aiTeammate = GameManager.Instance.aiTeammate;
            if (aiTeammate != null)
            {
                aiWeaponManager = aiTeammate.GetComponentInChildren<WeaponManager>();
            }
        }
    }

    /// <summary>
    /// 更新面板数据
    /// </summary>
    public void UpdatePanelData()
    {
        // 确保所有引用都存在
        if (inventoryManager == null || playerStats == null || aiTeammateState == null)
        {
            GetManagerReferences();
        }

        UpdatePlayerState();
        UpdateAITeammateState();
    }

    /// <summary>
    /// 更新玩家状态显示
    /// </summary>
    private void UpdatePlayerState()
    {
        if (inventoryManager != null)
        {
            // 更新玩家资源
            int playerWood = inventoryManager.GetPlayerResourceByType(ResourceType.Wood);
            int playerIron = inventoryManager.GetPlayerResourceByType(ResourceType.Iron);

            if (playerWoodText != null)
                playerWoodText.text = playerWood.ToString();
            if (playerIronText != null)
                playerIronText.text = playerIron.ToString();

            // 更新玩家背包重量
            float currentWeight = inventoryManager.GetPlayerCurrentWeight();
            float maxWeight = inventoryManager.playerMaxWeight;
            if (playerBagText != null)
                playerBagText.text = $"{currentWeight:F0}/{maxWeight:F0}";
        }

        if (playerStats != null)
        {
            // 更新玩家生命值
            int currentHP = playerStats.GetCurrentHealth();
            int maxHP = playerStats.GetMaxHealth();
            if (playerHPText != null)
                playerHPText.text = $"{currentHP}/{maxHP}";
        }

        // 更新玩家弹药
        if (playerWeaponManager != null)
        {
            int currentAmmo = playerWeaponManager.GetCurrentAmmo();
            int reserveAmmo = playerWeaponManager.GetReserveAmmo();
            if (playerAmmoText != null)
                playerAmmoText.text = $"{currentAmmo}/{reserveAmmo}";
        }
        else if (playerAmmoText != null)
        {
            playerAmmoText.text = "30/300"; // 默认值
        }
    }

    /// <summary>
    /// 更新AI队友状态显示
    /// </summary>
    private void UpdateAITeammateState()
    {
        if (inventoryManager != null)
        {
            // 更新AI资源
            int aiWood = inventoryManager.GetAIResourceByType(ResourceType.Wood);
            int aiIron = inventoryManager.GetAIResourceByType(ResourceType.Iron);

            if (aiWoodText != null)
                aiWoodText.text = aiWood.ToString();
            if (aiIronText != null)
                aiIronText.text = aiIron.ToString();

            // 更新AI背包重量
            float currentWeight = inventoryManager.GetAICurrentWeight();
            float maxWeight = inventoryManager.aiPlayerMaxWeight;
            if (aiBagText != null)
                aiBagText.text = $"{currentWeight:F0}/{maxWeight:F0}";
        }

        if (aiTeammateState != null)
        {
            // 更新AI生命值
            int currentHP = aiTeammateState.GetAICurrentHealth();
            int maxHP = aiTeammateState.playerData != null ? aiTeammateState.playerData.maxHealth : 100;
            if (aiHPText != null)
                aiHPText.text = $"{currentHP}/{maxHP}";
        }

        // 更新AI弹药
        if (aiWeaponManager != null)
        {
            int currentAmmo = aiWeaponManager.GetCurrentAmmo();
            int reserveAmmo = aiWeaponManager.GetReserveAmmo();
            if (aiAmmoText != null)
                aiAmmoText.text = $"{currentAmmo}/{reserveAmmo}";
        }
        else if (aiAmmoText != null)
        {
            aiAmmoText.text = "30/100"; // 默认值
        }
    }

    /// <summary>
    /// 强制刷新面板数据（供外部调用）
    /// </summary>
    public void RefreshPanelData()
    {
        GetManagerReferences();
        UpdatePanelData();
    }
}
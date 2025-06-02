using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Ref")]
    public Canvas uiCanvas;
    [Header("Player UI Panel")]
    public GameObject buildingMenuPanel;
    [Header("2D AIInfo UI")]
    public RectTransform currentMaAIInfoUI;
    // 后续你可以在Inspector拖更多的UI面板（血条、Tip等）

    private PlayerInputSystem playerInputSystem;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        // 查找PlayerInputSystem引用
        playerInputSystem = FindObjectOfType<PlayerInputSystem>();
        if (playerInputSystem == null)
        {
            Debug.LogError("UIManager: 找不到PlayerInputSystem组件！");
        }
    }
    private void Start()
    {
        // 订阅玩家模式切换事件
        PlayerInputSystem.OnModeChanged += OnPlayerModeChanged;

        // 初始化UI状态
        InitializeUI();
    }
    private void Update()
    {
        ShowAIInfo();
    }
    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        PlayerInputSystem.OnModeChanged -= OnPlayerModeChanged;
    }

    /// <summary>
    /// 响应玩家模式切换
    /// </summary>
    private void OnPlayerModeChanged(PlayerInputSystem.PlayerMode newMode)
    {
        switch (newMode)
        {
            case PlayerInputSystem.PlayerMode.Combat:
                HideBuildingMenu();
                //ShowCombatUI();
                break;

            case PlayerInputSystem.PlayerMode.BuildMenu:
                ShowBuildingMenu();
                //HideCombatUI();
                break;

            case PlayerInputSystem.PlayerMode.Placing:
                HideBuildingMenu();
                //ShowPlacingUI();
                break;
        }
        Debug.Log($"UIManager: UI状态已切换至 {newMode} 模式");
    }

    /// <summary>
    /// 初始化UI状态
    /// </summary>
    private void InitializeUI()
    {
        // 确保建筑菜单初始状态为隐藏
        if (buildingMenuPanel != null)
        {
            buildingMenuPanel.SetActive(false);
        }

        // 初始化其他UI面板状态..
    }


    public void ShowBuildingMenu()
    {
        if (buildingMenuPanel != null)
            buildingMenuPanel.SetActive(true);
    }

    public void HideBuildingMenu()
    {
        if (buildingMenuPanel != null)
            buildingMenuPanel.SetActive(false);
    }

    public void ShowAIInfo()
    {
        Vector3 aiPlayerPos = GameManager.Instance.GetAIAgentTransform().position + Vector3.up * 2.3f;
        Vector2 aiPlayerInfoPos = Camera.main.WorldToScreenPoint(aiPlayerPos);
        if (aiPlayerPos.z > 0) // TODO： GameManager需要判断ai玩家是否存活
        {
            currentMaAIInfoUI.gameObject.SetActive(true);
            currentMaAIInfoUI.position = aiPlayerInfoPos;
        }
        else
        {
            currentMaAIInfoUI.gameObject.SetActive(false);
        }
    }
    // 后续拓展
    // public void ShowTip(string msg) { ... }
    // public void SetHealth(float val) { ... }
}

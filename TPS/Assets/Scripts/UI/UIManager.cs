using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public enum TipType { EVENT, HELP}
    public static UIManager Instance { get; private set; }

    [Header("Ref")]
    public Canvas uiCanvas;

    [Header("Player UI Panel")]
    public GameObject buildingMenuPanel;

    [Header("2D AIInfo UI")]
    public RectTransform currentMaAIInfoUI;

    [Header("Tip UI")]
    public GameObject subtitlesPanel;
    public TextMeshProUGUI subtitlesText;
    public TextMeshProUGUI subtitlesTypeText;
    public Animator subtitlesAnimator;

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
        Vector3 aiPlayerPos = GameManager.Instance.GetAIAgentTransform().position + Vector3.up * 2f;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(aiPlayerPos);
        // 检查AI是否在摄像机前方且在屏幕范围内
        if (screenPoint.z > 0 &&
            screenPoint.x >= 0 && screenPoint.x <= Screen.width &&
            screenPoint.y >= 0 && screenPoint.y <= Screen.height)
        {
            currentMaAIInfoUI.gameObject.SetActive(true);
            currentMaAIInfoUI.position = new Vector2(screenPoint.x, screenPoint.y);
        }
        else
        {
            currentMaAIInfoUI.gameObject.SetActive(false);
        }
    }

    public void ShowDayNightTip(string msg, TipType tipType) 
    {
        SetTipTitle(TipType.EVENT);
        StartCoroutine(ShowTipCoroutine(msg));
    }

    private IEnumerator ShowTipCoroutine(string msg)
    {
        subtitlesText.text = msg;
        subtitlesAnimator.SetBool("Active", true);
        // 等待3秒
        yield return new WaitForSeconds(2f);
        subtitlesAnimator.SetBool("Active", false);
    }

    private void SetTipTitle(TipType tipType)
    {
        switch (tipType)
        {
            case TipType.EVENT:
                subtitlesTypeText.text = "Event";
                break;
            case TipType.HELP:
                subtitlesTypeText.text = "Help";
                break;
        }
    }
}

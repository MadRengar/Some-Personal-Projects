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
    public GameObject playerGameScreenUI;
    public GameObject gameOverMenuUI;

    [Header("Player UI Panel")]
    public GameObject buildingMenuPanelUI;

    [Header("2D AIInfo UI")]
    public RectTransform currentMaAIInfoUI;

    [Header("Tip UI")]
    public GameObject subtitlesPanel;
    public TextMeshProUGUI subtitlesText;
    public TextMeshProUGUI subtitlesTypeText;
    public Animator subtitlesAnimator;

    [Header("PlayerDeath FadeToBlack")]
    [SerializeField] private GameObject screenFadeUI;
    [SerializeField] private Animator screenFadeAnimator;
    private bool _isDeathFadeActive = false;

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

        GameManager.OnPlayerDeath += OnPlayerDeath;
        // 初始化UI状态
        InitializeUI();
    }

    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        PlayerInputSystem.OnModeChanged -= OnPlayerModeChanged;
        GameManager.OnPlayerDeath -= OnPlayerDeath;
    }

    private void Update()
    {
        ShowAIInfo();
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
    }


    /// <summary>
    /// 初始化UI状态
    /// </summary>
    private void InitializeUI()
    {
        // 确保建筑菜单初始状态为隐藏
        if (buildingMenuPanelUI != null)
        {
            buildingMenuPanelUI.SetActive(false);
        }

        if (screenFadeAnimator != null)
        {
            screenFadeUI.SetActive(false);
            // 确保初始状态为透明
            screenFadeAnimator.SetBool("Active", false);           
        }

        if (gameOverMenuUI != null)
        {
            gameOverMenuUI.SetActive(false);
        }
    }


    public void ShowBuildingMenu()
    {
        if (buildingMenuPanelUI != null)
            buildingMenuPanelUI.SetActive(true);
    }

    public void HideBuildingMenu()
    {
        if (buildingMenuPanelUI != null)
            buildingMenuPanelUI.SetActive(false);
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

    private void OnPlayerDeath()
    {
        // 立即激活渐变UI对象
        if (screenFadeUI != null)
        {
            screenFadeUI.SetActive(true);
        }

        // 立即设置动画标志位为true，进入"in"状态
        if (screenFadeAnimator != null)
        {
            screenFadeAnimator.SetBool("Active", true);
        }
    }

    private void ShowGameOverUI()
    {
        // 立即激活渐变UI对象
        if (playerGameScreenUI != null)
        {
            playerGameScreenUI.SetActive(false);
        }
        if (gameOverMenuUI != null)
        {
            gameOverMenuUI.SetActive(true);
        }
        if (screenFadeUI != null)
        {
            screenFadeUI.SetActive(false);
        }
    }

    public void OnScreenFadeComplete()
    {
        // 这个方法会被动画事件调用
        ShowGameOverUI();
    }
}

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
    [SerializeField] private GameObject gunAmmoPanel;    // GunAmmo UI 面板
    [SerializeField] private GameObject hammerPanel;     // Hammer UI 面板

    [Header("2D AIInfo UI")]
    public RectTransform currentMaAIInfoUI;

    [Header("Tip UI")]
    public GameObject subtitlesPanel;
    public TextMeshProUGUI subtitlesText;
    public TextMeshProUGUI subtitlesTypeText;
    public Animator subtitlesAnimator;

    [Header("Interaction UI")]
    public GameObject interactionTipUI; // 拖拽交互提示UI到这里
    [Header("Storage UI")]
    public GameObject storagePanel;
    [Header("Ammo Workbench UI")]
    public GameObject ammoWorkbenchPanel; // 拖拽弹药工作台面板UI到这里

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
        // 监听武器切换事件
        WeaponSwitcher.OnWeaponChanged += OnWeaponChanged;

        GameManager.OnPlayerDeath += OnPlayerDeath;

        // 订阅仓库交互事件
        StorageController.OnPlayerEnterStorageRange += OnPlayerEnterStorageRange;
        StorageController.OnPlayerExitStorageRange += OnPlayerExitStorageRange;
        StorageController.OnPlayerInteractWithStorage += OnPlayerInteractWithStorage;
        AmmoWorkbenchController.OnPlayerInteractWithWorkbench += OnPlayerInteractWithWorkbench;
        // 初始化UI状态
        InitializeUI();
    }

    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        PlayerInputSystem.OnModeChanged -= OnPlayerModeChanged;
        GameManager.OnPlayerDeath -= OnPlayerDeath;
        WeaponSwitcher.OnWeaponChanged -= OnWeaponChanged;
        StorageController.OnPlayerEnterStorageRange -= OnPlayerEnterStorageRange;
        StorageController.OnPlayerExitStorageRange -= OnPlayerExitStorageRange;
        StorageController.OnPlayerInteractWithStorage -= OnPlayerInteractWithStorage;

        AmmoWorkbenchController.OnPlayerInteractWithWorkbench -= OnPlayerInteractWithWorkbench;
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

    private void OnWeaponChanged(WeaponType newWeaponType)
    {
        UpdateWeaponUI(newWeaponType);
    }

    private void UpdateWeaponUI(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Rifle:
                ShowGunUI();
                break;

            case WeaponType.Hammer:
                ShowHammerUI();
                break;
        }
    }

    private void ShowGunUI()
    {
        // 显示枪械弹药面板
        if (gunAmmoPanel != null)
        {
            gunAmmoPanel.SetActive(true);
        }

        // 隐藏锤子面板
        if (hammerPanel != null)
        {
            hammerPanel.SetActive(false);
        }
    }

    private void ShowHammerUI()
    {
        // 隐藏枪械弹药面板
        if (gunAmmoPanel != null)
        {
            gunAmmoPanel.SetActive(false);
        }

        // 显示锤子面板
        if (hammerPanel != null)
        {
            hammerPanel.SetActive(true);
        }
    }

    #region Storage Functions
    private void OnPlayerEnterStorageRange()
    {
        ShowInteractionTip();
    }

    /// <summary>
    /// 处理玩家离开仓库范围事件
    /// </summary>
    private void OnPlayerExitStorageRange()
    {
        HideInteractionTip();
        HideStoragePanel();
    }

    public void ShowInteractionTip()
    {
        if (interactionTipUI != null)
        {
            interactionTipUI.SetActive(true);
        }
    }

    public void HideInteractionTip()
    {
        if (interactionTipUI != null)
        {
            interactionTipUI.SetActive(false);
        }
    }

    /// <summary>
    /// 处理玩家与仓库交互事件
    /// </summary>
    private void OnPlayerInteractWithStorage(StorageController storage)
    {
        ShowStoragePanel(storage);
    }

    /// <summary>
    /// 显示仓库面板
    /// </summary>
    public void ShowStoragePanel(StorageController storage = null)
    {
        if (storagePanel != null)
        {
            storagePanel.SetActive(true);

            // 设置当前仓库引用
            StorageUIController uiController = storagePanel.GetComponent<StorageUIController>();
            if (uiController != null && storage != null)
            {
                uiController.SetCurrentStorage(storage);
            }

            HideInteractionTip();
        }
    }

    /// <summary>
    /// 隐藏仓库面板
    /// </summary>
    public void HideStoragePanel()
    {
        if (storagePanel != null)
        {
            storagePanel.SetActive(false);
        }
    }
    #endregion

    #region Ammo Workbench Functions
    private void OnPlayerInteractWithWorkbench()
    {
        ShowAmmoWorkbenchPanel();
    }

    /// <summary>
    /// 显示弹药工作台面板
    /// </summary>
    public void ShowAmmoWorkbenchPanel()
    {
        if (ammoWorkbenchPanel != null)
        {
            ammoWorkbenchPanel.SetActive(true);
            HideInteractionTip();
        }
    }

    /// <summary>
    /// 隐藏弹药工作台面板
    /// </summary>
    public void HideAmmoWorkbenchPanel()
    {
        if (ammoWorkbenchPanel != null)
        {
            ammoWorkbenchPanel.SetActive(false);
        }
    }
    #endregion
}

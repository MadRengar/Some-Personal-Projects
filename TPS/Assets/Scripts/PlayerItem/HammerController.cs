using UnityEngine;
using PlayerControl;

public class HammerController : MonoBehaviour
{
    [Header("Hammer Settings")]
    [SerializeField] private float hammerRange = 3f;
    [SerializeField] private float swingCooldown = 0.5f;
    [SerializeField] private LayerMask buildingLayerMask = -1;

    [Header("Building Progress")]
    [SerializeField] private float progressPerHit = 20f;

    [Header("Building Repair")]
    [SerializeField] private int repairPerHit = 10; // 每次挥击修复的耐久值

    [Header("Animation")]
    [SerializeField] private Animator playerAnimator; // 玩家动画控制器
    [SerializeField] private string hammerSwingTrigger = "HammerSwing"; // 锤子挥击动画触发器名称
    [SerializeField] private int hammerLayerIndex = 4; // Hammer Layer 索引
    [SerializeField] private int hammerSwingLayerIndex = 5; // HammerSwing Layer 索引

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    // 状态管理
    private bool isActive = false;
    private float cooldownTimer = 0f;
    private PlayerInputSystem playerInputSystem;

    // 新增：自己管理按键状态
    private bool lastFrameShootHeld = false;

    void Awake()
    {
        playerInputSystem = GetComponent<PlayerInputSystem>();
        playerAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        WeaponSwitcher.OnWeaponChanged += OnWeaponChanged;
        SetHammerActive(false); // 初始状态为非激活
    }

    void OnDestroy()
    {
        WeaponSwitcher.OnWeaponChanged -= OnWeaponChanged;
    }

    void Update()
    {
        // 更新冷却时间
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        // 只有在锤子激活且玩家在正确模式下才处理输入
        if (isActive && playerInputSystem != null)
        {
            HandleHammerInput();
        }
        //else if (playerInputSystem != null && playerInputSystem.shootHeld)
        //{
        //    Debug.Log($"[HammerController] 跳过输入处理 - isActive: {isActive}, 原因: 锤子未激活");
        //}
    }

    /// <summary>
    /// 响应武器切换事件
    /// </summary>
    private void OnWeaponChanged(WeaponType weaponType)
    {
        bool shouldActivate = weaponType == WeaponType.Hammer;
        SetHammerActive(shouldActivate);
    }

    /// <summary>
    /// 设置锤子激活状态
    /// </summary>
    private void SetHammerActive(bool active)
    {
        isActive = active;

        // 当锤子被停用时，重置按键状态，避免状态残留
        if (!active)
        {
            lastFrameShootHeld = false;
        }
    }

    /// <summary>
    /// 处理锤子输入
    /// </summary>
    private void HandleHammerInput()
    {
        // 首先检查锤子是否激活 - 这是最重要的检查
        if (!isActive)
        {
            return;
        }

        // 检查是否在正确的模式下
        if (playerInputSystem.currentMode != PlayerInputSystem.PlayerMode.Combat)
        {
            return;
        }

        // 自己检测按键按下事件
        bool currentFrameShootHeld = playerInputSystem.shootHeld;
        bool shootJustPressed = currentFrameShootHeld && !lastFrameShootHeld;

        // 使用我们自己检测的按下事件
        if (shootJustPressed && CanSwing())
        {
            TrySwing();
        }

        // 更新上一帧状态
        lastFrameShootHeld = currentFrameShootHeld;
    }

    /// <summary>
    /// 检查是否可以挥击
    /// </summary>
    private bool CanSwing()
    {
        return isActive && cooldownTimer <= 0f;
    }

    /// <summary>
    /// 尝试挥击
    /// </summary>
    private void TrySwing()
    {
        if (!CanSwing()) return;

        // 首先播放挥击动画
        PlayHammerSwingAnimation();

        // 执行挥击检测 - Animation Event

        // 设置冷却时间
        cooldownTimer = swingCooldown;

        //Debug.Log("[HammerController] 锤子挥击！");
    }

    /// <summary>
    /// 执行挥击检测
    /// </summary>
    private void OnHammerHit()
    {
        // 从摄像机中心发射射线检测建筑物
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hit, hammerRange, buildingLayerMask))
        {
            if (enableDebugLog)
            {
                Debug.Log($"[HammerController] 锤子击中: {hit.collider.name}");
            }

            // 检查是否是建筑预览体
            if (hit.collider.CompareTag("BuildingPreview"))
            {
                HandleBuildingHit(hit.collider.gameObject);
            }
            if (hit.collider.CompareTag("PlayerBuilding"))
            {
                HandleBuildingRepair(hit.collider.gameObject);
            }
        }
        else
        {
            if (enableDebugLog)
            {
                Debug.Log("[HammerController] 锤子挥击未击中任何物体");
            }
        }

        // 绘制调试射线
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * hammerRange, Color.yellow, 0.5f);

        // TODO: 播放挥击动画和音效
        PlaySwingEffects();
    }

    /// <summary>
    /// 处理击中建筑物
    /// </summary>
    private void HandleBuildingHit(GameObject buildingPreview)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[HammerController] 击中建筑预览，增加 {progressPerHit}% 建造进度！");
        }
        BuildingProgress buildingProgress = buildingPreview.GetComponent<BuildingProgress>();
        if (buildingProgress != null)
        {
            float progressBefore = buildingProgress.GetCurrentProgress();

            buildingProgress.AddProgress(progressPerHit);

            float progressAfter = buildingProgress.GetCurrentProgress();
            float progressPercent = buildingProgress.GetProgressPercentage() * 100;

            if (enableDebugLog)
            {
                Debug.Log($"[HammerController] 建造进度更新 - 之前: {progressBefore}, 之后: {progressAfter}, 百分比: {progressPercent:F1}%, 是否完成: {buildingProgress.IsCompleted()}");
            }
        }
    }

    #region Repair
    private void HandleBuildingRepair(GameObject playerBuilding)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[HammerController] 修复建筑物: {playerBuilding.name}，恢复 {repairPerHit} 点耐久");
        }

        // 尝试获取建筑控制器接口
        IBuildingController buildingController = playerBuilding.GetComponent<IBuildingController>();
        if (buildingController != null)
        {
            // 检查建筑是否被摧毁
            if (buildingController.IsDestroyed())
            {
                if (enableDebugLog)
                {
                    Debug.Log("[HammerController] 建筑已被摧毁，无法修复");
                }
                return;
            }

            // 根据建筑类型进行修复
            TurretController turret = playerBuilding.GetComponent<TurretController>();
            if (turret != null)
            {
                RepairTurret(turret);
                return;
            }

            GeneratorController generator = playerBuilding.GetComponent<GeneratorController>();
            if (generator != null)
            {
                RepairGenerator(generator);
                return;
            }

            StorageController storage = playerBuilding.GetComponent<StorageController>();
            if (storage != null)
            {
                RepairStorage(storage);
                return;
            }
        }

        if (enableDebugLog)
        {
            Debug.LogWarning($"[HammerController] 无法修复建筑物 {playerBuilding.name}：未找到有效的建筑控制器");
        }
    }

    /// <summary>
    /// 修复防御塔
    /// </summary>
    private void RepairTurret(TurretController turret)
    {
        // 直接调用TakeDamage，传入负值表示修复
        turret.TakeDamage(-repairPerHit);

        if (enableDebugLog)
        {
            Debug.Log($"[HammerController] 尝试修复防御塔");
        }
    }

    /// <summary>
    /// 修复发电机
    /// </summary>
    private void RepairGenerator(GeneratorController generator)
    {
        // 通过造成负伤害来"修复"
        generator.TakeDamage(-repairPerHit);

        if (enableDebugLog)
        {
            Debug.Log($"[HammerController] 发电机修复成功");
        }
    }

    /// <summary>
    /// 修复仓库
    /// </summary>
    private void RepairStorage(StorageController storage)
    {
        // 通过造成负伤害来"修复"
        storage.TakeDamage(-repairPerHit);

        if (enableDebugLog)
        {
            Debug.Log($"[HammerController] 仓库修复成功");
        }
    }

    #endregion
    /// <summary>
    /// 播放挥击效果
    /// </summary>
    private void PlaySwingEffects()
    {
        
    }

    private void PlayHammerSwingAnimation()
    {
        if (playerAnimator != null && !string.IsNullOrEmpty(hammerSwingTrigger))
        {
            playerAnimator.SetTrigger(hammerSwingTrigger);
        }
    }

    /// <summary>
    /// 检查锤子是否激活
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// 获取冷却进度 (0-1)
    /// </summary>
    public float GetCooldownProgress()
    {
        if (swingCooldown <= 0f) return 1f;
        return 1f - (cooldownTimer / swingCooldown);
    }
}
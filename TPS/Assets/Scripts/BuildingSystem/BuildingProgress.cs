using UnityEngine;
using UnityEngine.UI;

public class BuildingProgress : MonoBehaviour
{
    [Header("Building Settings")]
    [SerializeField] private GameObject finalBuildingPrefab;  // 最终建筑预制体（带 TurretController 的）
    [SerializeField] private float progressPerHit = 20f;     // 每次锤击增加的进度

    [Header("Progress UI")]
    [SerializeField] private Canvas progressCanvas;          // 进度条Canvas
    [SerializeField] private Slider progressSlider;          // 进度条Slider
    [SerializeField] private GameObject progressUI;          // 整个进度UI对象

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem hitEffect;       // 锤击特效
    [SerializeField] private AudioClip buildingHitSound;     // 建造音效
    [SerializeField] private AudioClip buildingCompleteSound; // 建造完成音效

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;

    // 当前建造进度
    private float maxProgress = 100f; // 从BuildingData_SO.requiredBuildingTime读取 
    private float currentProgress = 0f;
    private bool isCompleted = false;
    private AudioSource audioSource;

    // 建造完成事件
    public System.Action<GameObject> OnBuildingCompleted;

    void Start()
    {
        // 初始化组件
        InitializeComponents();

        // 初始化进度UI
        UpdateProgressUI();

        if (enableDebugLog)
        {
            Debug.Log("[BuildingProgress] 建筑预览初始化完成");
        }
    }

    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        // 获取或创建 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 如果没有设置进度UI，尝试在子物体中查找
        if (progressUI == null)
        {
            progressUI = transform.Find("ProgressUI")?.gameObject;
        }

        if (progressSlider == null && progressUI != null)
        {
            progressSlider = progressUI.GetComponentInChildren<Slider>();
        }
    }

    /// <summary>
    /// 增加建造进度
    /// </summary>
    public void AddProgress(float amount)
    {
        if (isCompleted) return;

        currentProgress += amount;
        currentProgress = Mathf.Clamp(currentProgress, 0f, maxProgress);

        // 更新进度UI
        UpdateProgressUI();

        if (enableDebugLog)
        {
            Debug.Log($"[BuildingProgress] 建造进度: {currentProgress:F1}/{maxProgress}");
        }

        // 检查是否完成建造
        if (currentProgress >= maxProgress && !isCompleted)
        {
            CompleteBuiding();
        }
    }

    /// <summary>
    /// 更新进度UI显示
    /// </summary>
    private void UpdateProgressUI()
    {
        if (progressSlider != null)
        {
            progressSlider.value = currentProgress / maxProgress;
        }

        // 显示进度UI（当开始建造时）
        if (progressUI != null && currentProgress > 0f && !isCompleted)
        {
            progressUI.SetActive(true);
        }
    }

    /// <summary>
    /// 完成建造
    /// </summary>
    private void CompleteBuiding()
    {
        isCompleted = true;

        if (enableDebugLog)
        {
            Debug.Log("[BuildingProgress] 建造完成！正在生成最终建筑...");
        }

        // 播放完成音效
        if (buildingCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buildingCompleteSound);
        }

        // 生成最终建筑
        SpawnFinalBuilding();
    }

    /// <summary>
    /// 生成最终建筑
    /// </summary>
    private void SpawnFinalBuilding()
    {
        if (finalBuildingPrefab == null)
        {
            Debug.LogError("[BuildingProgress] 未设置最终建筑预制体！");
            return;
        }

        // 记录当前位置和旋转
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        // 生成最终建筑
        GameObject finalBuilding = Instantiate(finalBuildingPrefab, position, rotation);

        // 触发建造完成事件
        OnBuildingCompleted?.Invoke(finalBuilding);

        if (enableDebugLog)
        {
            Debug.Log($"[BuildingProgress] 最终建筑已生成: {finalBuilding.name}");
        }

        // 销毁预览对象
        Destroy(gameObject);
    }

    /// <summary>
    /// 获取建造进度百分比 (0-1)
    /// </summary>
    public float GetProgressPercentage()
    {
        return currentProgress / maxProgress;
    }

    /// <summary>
    /// 获取当前进度
    /// </summary>
    public float GetCurrentProgress()
    {
        return currentProgress;
    }

    /// <summary>
    /// 检查是否已完成
    /// </summary>
    public bool IsCompleted()
    {
        return isCompleted;
    }

    /// <summary>
    /// 设置最终建筑预制体（供 BuildingSystem 调用）
    /// </summary>
    public void SetFinalBuildingPrefab(GameObject finalBuilding)
    {
        finalBuildingPrefab = finalBuilding;

        if (enableDebugLog)
        {
            Debug.Log($"[BuildingProgress] 设置最终建筑预制体: {finalBuilding?.name}");
        }
    }

    /// <summary>
    /// 从BuildingData设置建造进度
    /// </summary>
    public void InitializeFromBuildingData(GameObject finalBuilding)
    {
        finalBuildingPrefab = finalBuilding;

        // 从建筑数据中读取建造时间作为最大进度
        IBuildingController buildingController = finalBuilding.GetComponent<IBuildingController>();
        if (buildingController != null)
        {
            BuildingData_SO buildingData = buildingController.GetBuildingData();
            maxProgress = buildingData.requiredBuildingTime;

            if (enableDebugLog)
            {
                Debug.Log($"[BuildingProgress] 从{buildingData.buildingName}读取建造进度: {maxProgress}");
            }
        }
        else
        {
            Debug.LogWarning("[BuildingProgress] 建筑没有实现IBuildingController接口，使用默认进度100");
            maxProgress = 100f;
        }

        // 更新UI
        UpdateProgressUI();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText; // 时间显示文本
    [SerializeField] private TextMeshProUGUI dayCount;

    [Header("Time Settings")]
    [SerializeField] private float dayDurationInSeconds = 300f; // 一天的实际时长（秒）
    [SerializeField] private float dawnTime = 6f; // 黎明时间（小时）
    [SerializeField] private float nightTime = 20f; // 夜晚时间（小时）

    [Header("Game Time State")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private float currentHour = 6f; // 从黎明开始
    [SerializeField] private bool isNight = false;

    [Header("Day/Night Lighting")]
    [SerializeField] private Light sunLight; // 太阳光源（Directional Light）
    [SerializeField] private Gradient sunColor; // 太阳颜色渐变（从黎明到正午到黄昏）
    [SerializeField] private AnimationCurve sunIntensity; // 太阳强度曲线（0-24小时）

    // 时间事件
    public event Action<int> OnDayChanged;
    public event Action OnDawnStarted;
    public event Action OnNightStarted;
    public event Action<float> OnHourChanged;

    // 私有变量
    private float timeSpeed;
    private bool lastFrameIsNight;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 计算时间流逝速度（24小时 / dayDurationInSeconds）
        timeSpeed = 24f / dayDurationInSeconds; // 假设5分钟代表1天24小时，24 ÷ 300 = 0.08 小时/秒。含义： 现实中每过1秒，游戏时间就前进0.08小时（约4.8分钟）

        // 初始化状态
        lastFrameIsNight = isNight;

        Debug.Log($"游戏时间系统启动 - 第{currentDay}天 {currentHour:F1}时"); // 浮点保留一位
        dayCount.text = $"Day {currentDay}";
    }

    private void Update()
    {
        UpdateGameTime();
        UpdateLighting();
        CheckDayNightTransition();
    }

    private void UpdateGameTime()
    {
        // 更新当前小时
        /*步长调节器：
            帧率高 = 小步快走（步子小但走得频繁）
            帧率低 = 大步慢走（步子大但走得少）
            最终每秒走过的总距离是一样的
        */
        currentHour += timeSpeed * Time.deltaTime;

        // 检查是否进入新的一天
        if (currentHour >= 24f)
        {
            currentHour -= 24f;
            currentDay++;
            dayCount.text = $"Day {currentDay}";
            OnDayChanged?.Invoke(currentDay);
        }
        updateUITime();
        // 触发小时变化事件
        OnHourChanged?.Invoke(currentHour);
    }

    private void CheckDayNightTransition()
    {
        // 更新昼夜状态
        isNight = currentHour >= nightTime || currentHour < dawnTime;

        // 检查昼夜切换
        /*举例：
            19:59分：isNight=false, lastFrameIsNight=false → 不触发
            20:00分：isNight=true, lastFrameIsNight=false → 触发夜晚开始！
            20:01分：isNight=true, lastFrameIsNight=true → 不触发
            没有这个标志位的话，从20:00到次日6:00的每一帧都会触发"夜晚开始"事件！
        */
        if (isNight != lastFrameIsNight)
        {
            if (isNight)
            {
                OnNightStarted?.Invoke();
                StartNightPhase();
            }
            else
            {
                OnDawnStarted?.Invoke();
                StartDawnPhase();
            }
            lastFrameIsNight = isNight;
        }
    }

    private void StartDawnPhase()
    {
        //Debug.Log($"黎明开始 - 第{currentDay}天 {currentHour:F1}时");
        UIManager.Instance.ShowDayNightTip("We are temporarily safe.", UIManager.TipType.EVENT);

        //如果ai死亡 重新部署

        // TODO: 停止大批量僵尸生成
        // ZombieSpawnManager.Instance.StopMassiveSpawn();

        // TODO: 开始资源刷新
        // ResourceSpawner.Instance.RefreshResources();

        // TODO: 开始随机生成僵尸
        // ZombieSpawnManager.Instance.StartRandomSpawn();

        // TODO: 将现有僵尸退出狂暴状态
        // SetAllZombiesRageState(false);
    }

    private void StartNightPhase()
    {
        //Debug.Log($"夜晚开始 - 第{currentDay}天 {currentHour:F1}时");
        UIManager.Instance.ShowDayNightTip("Nihgt fell, they are coming!", UIManager.TipType.EVENT);
        // TODO: 将白天生成的僵尸设为狂暴状态
        // SetAllZombiesRageState(true);

        // TODO: 在固定点位生成大批量狂暴僵尸
        // ZombieSpawnManager.Instance.StartMassiveRageSpawn();
    }

    // TODO: 僵尸狂暴状态控制方法
    // private void SetAllZombiesRageState(bool isRage)
    // {
    //     var allZombies = FindObjectsOfType<ZombieFSM>();
    //     foreach (var zombie in allZombies)
    //     {
    //         zombie.SetRageState(isRage);
    //     }
    // }

    // TODO：FIX时间ui闪烁
    private void updateUITime()
    {
        timeText.text = GetFormattedTime();
    }

    private void UpdateLighting()
    {
        if (sunLight == null) return;

        // 计算太阳角度（0-24小时映射到0-360度）
        float sunAngle = (currentHour / 24f) * 360f - 90f; // -90度让6点时太阳在地平线

        // 设置太阳旋转（绕X轴旋转模拟太阳轨迹）
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 30f, 0f);

        // 计算时间因子（0-1，用于颜色和强度插值）
        float timeFactor = currentHour / 24f;

        // 设置太阳强度（使用曲线或简单计算）
        if (sunIntensity != null && sunIntensity.length > 0)
        {
            sunLight.intensity = sunIntensity.Evaluate(timeFactor);
        }
        else
        {
            // 简单的强度计算：白天强，夜晚弱
            if (currentHour >= dawnTime && currentHour <= nightTime)
            {
                // 白天：6点到18点
                float dayProgress = (currentHour - 6f) / 12f; // 0-1
                sunLight.intensity = Mathf.Sin(dayProgress * Mathf.PI) * 1.5f; // 正弦曲线，正午最强
            }
            else
            {
                // 夜晚：微弱月光
                sunLight.intensity = 0.1f;
            }
        }

        // 设置太阳颜色
        if (sunColor != null)
        {
            sunLight.color = sunColor.Evaluate(timeFactor);
        }
        else
        {
            // 简单的颜色计算
            if (currentHour >= 5f && currentHour <= 7f)
            {
                // 黎明：橙红色
                sunLight.color = Color.Lerp(Color.red, Color.yellow, (currentHour - 5f) / 2f);
            }
            else if (currentHour >= 7f && currentHour <= 17f)
            {
                // 白天：白色
                sunLight.color = Color.white;
            }
            else if (currentHour >= 17f && currentHour <= 19f)
            {
                // 黄昏：橙红色
                sunLight.color = Color.Lerp(Color.yellow, Color.red, (currentHour - 17f) / 2f);
            }
            else
            {
                // 夜晚：蓝色月光
                sunLight.color = new Color(0.3f, 0.3f, 0.8f);
            }
        }
    }

    #region Public Methods
    /// <summary>
    /// 获取当前天数
    /// </summary>
    public int GetCurrentDay()
    {
        return currentDay;
    }

    /// <summary>
    /// 获取当前小时
    /// </summary>
    public float GetCurrentHour()
    {
        return currentHour;
    }

    /// <summary>
    /// 获取格式化的时间字符串
    /// 浮点数时间到标准时钟格式的转换
    /// </summary>
    public string GetFormattedTime()
    {
        // 假设 currentHour = 14.75f（下午2点45分）
        int hours = Mathf.FloorToInt(currentHour); // 14
        int minutes = Mathf.FloorToInt((currentHour - hours) * 60);  // (0.75 * 60) = 45
        return $"{hours:D2}:{minutes:D2}"; // "14:45" 整型保留两位，不足补零 6 → "06"
    }

    /// <summary>
    /// 获取格式化的日期时间字符串
    /// </summary>
    public string GetFormattedDateTime()
    {
        return $"第{currentDay}天 {GetFormattedTime()}";
    }

    /// <summary>
    /// 是否为夜晚
    /// </summary>
    public bool IsNight()
    {
        return isNight;
    }

    /// <summary>
    /// 设置时间流速（调试用）
    /// </summary>
    public void SetTimeSpeed(float newDayDuration)
    {
        if (newDayDuration <= 0f)
        {
            timeSpeed = 0f; // 直接设置为0停止时间
            Debug.Log("时间已停止");
        }
        else
        {
            dayDurationInSeconds = newDayDuration;
            timeSpeed = 24f / dayDurationInSeconds;
            Debug.Log($"时间流速已设置为: {timeSpeed}");
        }
    }

    /// <summary>
    /// 跳转到指定时间（调试用）
    /// </summary>
    public void SetTime(int day, float hour)
    {
        currentDay = day;
        currentHour = hour;
        Debug.Log($"时间跳转到: 第{currentDay}天 {currentHour:F1}时");
    }
    #endregion
}

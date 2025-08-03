using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    [Header("Resource Management")]
    public ResourceSpawner resourceSpawner;

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

    [Header("Tip Tracking")]
    [SerializeField] private HashSet<string> triggeredTips = new HashSet<string>(); // 已触发的提示跟踪

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
        timeSpeed = 24f / dayDurationInSeconds; // 假设5分钟代表1天24小时，24 ÷ 300 = 0.08 小时/秒。含义：现实中每过1秒，游戏时间就前进0.08小时（约4.8分钟）

        // 初始化状态
        lastFrameIsNight = isNight;

        Debug.Log($"游戏时间系统启动 - 第{currentDay}天 {currentHour:F1}时"); // 格点保留一位
        dayCount.text = $"Day {currentDay}";
    }

    private void Update()
    {
        UpdateGameTime();
        UpdateLighting();
        CheckDayNightTransition();
        CheckTimedTips(); // 添加这个持续检查
    }

    private void UpdateGameTime()
    {
        // 更新当前小时
        /*步长调试：
            帧÷数高 = 小步快跑（步骤小但跑得频繁）
            帧÷数低 = 大步慢跑（步骤大但跑得少）
            最终每秒跑过的总距离是一致的
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

    /// <summary>
    /// 持续检查时间相关的提示（每帧都执行）
    /// </summary>
    private void CheckTimedTips()
    {
        // 第1天的提示
        if (ShouldTriggerTip(1, 8f, 9f, "day1_8am"))
        {
            StartCoroutine(ShowTipWithDelay("Switch to the build hammer with the middle mouse button, then strike a prefab to begin building.", UIManager.TipType.TIP, 0f));
        }

        if (ShouldTriggerTip(1, 9f, 10f, "day1_9am"))
        {
            StartCoroutine(ShowTipWithDelay("Defense turrets run on electricity. Use the build menu (B) to see how much power each one consumes.", UIManager.TipType.HELP, 0f));
        }

        if (ShouldTriggerTip(1, 10f, 11f, "day1_10am"))
        {
            StartCoroutine(ShowTipWithDelay("Press V and try to give orders to your teammates to help you complete tasks such as collecting resources, " +
                "repairing buildings,and cooperate to survive", UIManager.TipType.HELP, 0f));
        }

        if (ShouldTriggerTip(1, 17f, 18f, "day1_5pm"))
        {
            StartCoroutine(ShowTipWithDelay("Surviving the night is easier if you stay near the camp. Get back before darkness falls!", UIManager.TipType.TIP, 0f));
        }

        // 第2天的提示
        if (ShouldTriggerTip(2, 6f, 7f, "day2_6am"))
        {
            StartCoroutine(ShowDay2Tips());
        }

        // 第3天的提示
        if (ShouldTriggerTip(3, 6f, 7f, "day3_6am"))
        {
            StartCoroutine(ShowDay3Tips());
        }

        // 第4天的提示
        if (ShouldTriggerTip(4, 7f, 8f, "day4_7am"))
        {
            StartCoroutine(ShowDay4Tips());
        }

        // 第5天的提示
        if (ShouldTriggerTip(5, 7f, 8f, "day5_7am"))
        {
            StartCoroutine(ShowTipWithDelay("As you survive longer, more zombies will appear and their health will increase – up to a limit.", UIManager.TipType.TIP, 0f));
        }
    }

    /// <summary>
    /// 显示单个提示（带延迟）
    /// </summary>
    private IEnumerator ShowTipWithDelay(string message, UIManager.TipType tipType, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        UIManager.Instance.ShowTip(message, tipType);
    }

    /// <summary>
    /// 第2天的提示序列
    /// </summary>
    private IEnumerator ShowDay2Tips()
    {
        yield return new WaitForSeconds(5f);
        UIManager.Instance.ShowTip("Collect resources, establish a defense line, strive to survive, and wait for rescue.", UIManager.TipType.TIP);
        yield return new WaitForSeconds(5f);
        UIManager.Instance.ShowTip("Remember to restock your food daily. Check the area around the camp — food may spawn randomly nearby.", UIManager.TipType.TIP);
    }

    /// <summary>
    /// 第3天的提示序列
    /// </summary>
    private IEnumerator ShowDay3Tips()
    {
        yield return new WaitForSeconds(5f);
        UIManager.Instance.ShowTip("There will be many zombies randomly appearing around the campsite, be careful to avoid them and save bullets.", UIManager.TipType.TIP);
        yield return new WaitForSeconds(5f);
        UIManager.Instance.ShowTip("You'd better replenish your food every day, as there will be random refreshing of food around the campsite. You can also slowly replenish your satiety within the campsite.", UIManager.TipType.TIP);
    }

    /// <summary>
    /// 第4天的提示序列
    /// </summary>
    private IEnumerator ShowDay4Tips()
    {
        yield return new WaitForSeconds(5f);
        UIManager.Instance.ShowTip("Headshots matter! Shooting zombies in the head inflicts greater damage.", UIManager.TipType.TIP);
        yield return new WaitForSeconds(5f);
        UIManager.Instance.ShowTip("You can craft ammo at a supply station – but it costs resources!", UIManager.TipType.TIP);
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
            没有这个标志位的话，从20:00到达日6:00的每一帧都会触发"夜晚开始"事件！
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
        StartCoroutine(ShowDawnTipsCoroutine());
    }

    private IEnumerator ShowDawnTipsCoroutine()
    {
        // 第一句提示
        UIManager.Instance.ShowTip("We are temporarily safe.", UIManager.TipType.EVENT);

        yield return new WaitForSeconds(5f);

        // 刷新资源
        resourceSpawner.DailyResourceRefresh();

        // 第二句提示
        UIManager.Instance.ShowTip("The location of the resource has been refreshed", UIManager.TipType.EVENT);
    }

    /// <summary>
    /// 检查是否应该触发提示（避免重复触发）
    /// </summary>
    /// <param name="day">目标天数</param>
    /// <param name="startHour">开始小时（包含）</param>
    /// <param name="endHour">结束小时（不包含）</param>
    /// <param name="tipId">提示的唯一ID</param>
    /// <returns>是否应该触发提示</returns>
    private bool ShouldTriggerTip(int day, float startHour, float endHour, string tipId)
    {
        bool isCorrectDay = currentDay == day;
        bool isInTimeRange = currentHour >= startHour && currentHour < endHour;
        bool notTriggeredYet = !triggeredTips.Contains(tipId);

        if (isCorrectDay && isInTimeRange && notTriggeredYet)
        {
            triggeredTips.Add(tipId); // 标记为已触发
            return true;
        }

        return false;
    }

    /// <summary>
    /// 重置提示触发状态（用于重新开始游戏或测试）
    /// </summary>
    public void ResetTriggeredTips()
    {
        triggeredTips.Clear();
    }

    /// <summary>
    /// 手动触发指定ID的提示（调试用）
    /// </summary>
    public void ManuallyTriggerTip(string tipId)
    {
        if (triggeredTips.Contains(tipId))
        {
            triggeredTips.Remove(tipId);
        }
    }

    /// <summary>
    /// 检查提示是否已被触发
    /// </summary>
    public bool IsTipTriggered(string tipId)
    {
        return triggeredTips.Contains(tipId);
    }

    private void StartNightPhase()
    {
        StartCoroutine(ShowNightTipsCoroutine());
    }

    private IEnumerator ShowNightTipsCoroutine()
    {
        UIManager.Instance.ShowTip("Night fell, they are coming!", UIManager.TipType.EVENT);
        yield return new WaitForSeconds(4f);
        UIManager.Instance.ShowTip("Zombies enter a state of frenzy at night, and you have nowhere to hide", UIManager.TipType.TIP);
    }

    private void updateUITime()
    {
        timeText.text = GetFormattedTime();
    }

    private void UpdateLighting()
    {
        if (sunLight == null) return;

        // 计算太阳角度（0-24小时映射到0-360度）
        float sunAngle = (currentHour / 24f) * 360f - 90f; // -90度让6点时太阳在地平线

        // 设置太阳旋转（让X轴旋转模拟太阳轨迹）
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
    /// 格点数时间到标准时钟格式的转换
    /// </summary>
    public string GetFormattedTime()
    {
        // 假设 currentHour = 14.75f（下午2点45分）
        int hours = Mathf.FloorToInt(currentHour); // 14
        int minutes = Mathf.FloorToInt((currentHour - hours) * 60);  // (0.75 * 60) = 45
        return $"{hours:D2}:{minutes:D2}"; // "14:45" 这样保留两位，不足补领 6 → "06"
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
    /// 设置时间流速
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
    #endregion
}
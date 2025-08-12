using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretController : MonoBehaviour, IBuildingController
{
    [Header("Turret Data")]
    public TurretData_SO turretData;

    [Header("Attack Components")]
    public Transform muzzle; // 炮管(需要转向的部分)
    public Transform firePoint; // 射击点位置
    public LayerMask enemyLayer; // 敌人层级
    public LayerMask aimColliderLayerMask; // 射线检测层级

    [Header("Audio Components")]
    public AudioSource audioSource;

    // 防御塔状态枚举
    public enum TurretState
    {
        Idle,       // 空闲，检测敌人
        Firing,     // 连续开火中
        Resting     // 休息冷却中
    }

    // 从DataSO读取的属性
    [Header("Current Turret Stats (Read Only)")]
    [SerializeField] private int requiredWoodNum;
    [SerializeField] private int requiredIronNum;
    [SerializeField] private float requiredBuildingTime;
    [SerializeField] private int attackDamage;
    [SerializeField] private float fireRate;
    [SerializeField] private float attackRange;
    [SerializeField] private float continuousFireDuration;
    [SerializeField] private float restDuration;

    [Header("Power Running State (Read Only)")]
    [SerializeField] private bool isPowered = true;    // 当前是否有电
    [SerializeField] private int powerRequirement;
    [SerializeField] private int level;

    [Header("Particle System")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private ParticleSystem shellEjectPrefab;

    [Header("Current State (Read Only)")]
    [SerializeField] private TurretState currentState = TurretState.Idle;
    [SerializeField] private Transform currentTarget;
    [SerializeField] private int currentHealth;

    // 射击控制变量(仿照WeaponManager)
    [HideInInspector] public float cooldown = 0f; // 当前冷却时间
    private float firingTimer = 0f; // 连续开火计时器
    private float restTimer = 0f; // 休息计时器
    private float rotationSpeed = 180f; // 转向速度

    // 敌人检测
    private Coroutine enemyDetectionCoroutine;

    // Debug可视化
    [Header("Debug Visualization")]
    [SerializeField] private bool showFireTrajectory = true;

    private Vector3 lastFireStartPoint;
    private Vector3 lastFireEndPoint;
    private bool hasLastFireData = false;
    private PowerManager powerManager;
    public BuildingData_SO GetBuildingData() => turretData;

    private void Start()
    {
        LoadTurretData();

        InitializeComponents();

        RegisterToPowerManager();

        StartEnemyDetection();   
    }

    private void Update()
    {
        // 每帧递减冷却时间(仿照WeaponManager)
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }

        UpdateTurretStateMachine();
    }

    /// <summary>
    /// 从DataSO加载防御塔数据
    /// </summary>
    private void LoadTurretData()
    {
        if (turretData == null)
        {
            Debug.LogError($"TurretController: {gameObject.name} 未分配 TurretData_SO!");
            return;
        }

        // 读取建造信息
        requiredWoodNum = turretData.requiredWoodNum;
        requiredIronNum = turretData.requiredIronNum;
        requiredBuildingTime = turretData.requiredBuildingTime;
        currentHealth = turretData.maxHealth;
        powerRequirement = turretData.power;
        level = turretData.turretLevel;

        // 读取攻击信息
        attackDamage = turretData.attackDamage;
        fireRate = turretData.firerate;
        attackRange = turretData.attackRange;

        // 读取开火模式
        continuousFireDuration = turretData.continuousFireDuration;
        restDuration = turretData.restDuration;

        //Debug.Log($"防御塔数据加载完成: {gameObject.name}");
    }

    public void TakeDamage(int damage)
    {
        if (damage < 0) // 负伤害表示修复
        {
            int healAmount = -damage;
            int maxHealth = turretData.maxHealth;

            if (currentHealth >= maxHealth)
            {
                Debug.Log($"防御塔 {name} 已满血，无需修复");
                return;
            }

            // 计算实际修复量，不能超过最大生命值
            int actualHeal = Mathf.Min(healAmount, maxHealth - currentHealth);
            currentHealth += actualHeal;

            Debug.Log($"防御塔 {name} 修复 {actualHeal} 点，当前耐久: {currentHealth}/{maxHealth}");
        }
        else // 正常伤害
        {
            currentHealth -= damage;
            Debug.Log($"防御塔受到 {damage} 点伤害，剩余生命: {currentHealth}");

            if (currentHealth <= 0)
            {
                DestroyBuilding();
            }
        }
    }

    public bool IsDestroyed()
    {
        return currentHealth <= 0;
    }

    private void DestroyBuilding()
    {
        Debug.Log("炮台被摧毁！");
        UnregisterTurret();
        Destroy(gameObject);
    }

    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        powerManager = GameManager.Instance.GetPowerManager();
    }

    private void RegisterToPowerManager()
    {
        
        powerManager.RegisterTurret(this);
    }

    private void UnregisterTurret()
    {
        powerManager.UnregisterTurret(this);
    }

    #region Turret FSM
    /// <summary>
    /// 防御塔状态机
    /// </summary>
    private void UpdateTurretStateMachine()
    {
        switch (currentState)
        {
            case TurretState.Idle:
                // 空闲状态，等待检测到敌人
                break;

            case TurretState.Firing:
                HandleFiringState();
                break;

            case TurretState.Resting:
                HandleRestingState();
                break;
        }
    }

    /// <summary>
    /// 处理开火状态(仿照WeaponManager.HandleShooting)
    /// </summary>
    private void HandleFiringState()
    {
        // 检查目标是否还有效
        if (!IsTargetValid(currentTarget))
        {
            //Debug.Log("[TurretController] 目标无效，切换到空闲状态");
            SwitchToIdle();
            return;
        }

        // 持续跟踪目标(像WeaponManager更新aimTarget一样)
        UpdateMuzzleRotation();

        // 按射击频率开火(仿照WeaponManager的射击逻辑)
        if (cooldown <= 0f)
        {
            TryFire();
            cooldown = fireRate; // 重置冷却时间
        }

        // 检查连续开火时间
        firingTimer += Time.deltaTime;

        if (firingTimer >= continuousFireDuration)
        {
            SwitchToResting();
        }
    }

    /// <summary>
    /// 处理休息状态
    /// </summary>
    private void HandleRestingState()
    {
        restTimer += Time.deltaTime;

        if (restTimer >= restDuration)
        {
            SwitchToIdle();
        }
    }

    /// <summary>
    /// 更新炮管转向(持续跟踪目标)
    /// </summary>
    private void UpdateMuzzleRotation()
    {
        if (muzzle == null || currentTarget == null) return;

        Vector3 targetDirection = (currentTarget.position - muzzle.position).normalized;
        targetDirection.y = 0; // 只在水平面旋转

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        muzzle.rotation = Quaternion.RotateTowards(muzzle.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 开火(仿照WeaponManager.TryFire)
    /// </summary>
    private void TryFire()
    {
        if (currentTarget == null || firePoint == null) return;

        // 计算射击方向
        Vector3 shootDirection = (currentTarget.position - firePoint.position).normalized;

        // 记录射击起点用于Debug可视化
        lastFireStartPoint = firePoint.position;

        // 射线检测(仿照WeaponManager的射线检测)
        RaycastHit hit;
        bool hasHit = Physics.Raycast(firePoint.position, shootDirection, out hit, attackRange, aimColliderLayerMask);

        if (hasHit)
        {
            lastFireEndPoint = hit.point;
            HitTarget(hit);
        }
        else
        {
            // 没有击中任何物体，射线延伸到最大距离
            lastFireEndPoint = firePoint.position + shootDirection * attackRange;
        }

        hasLastFireData = true;

        // 播放特效和音效
        PlayFireEffects();

        // Debug射线显示
        Debug.DrawLine(lastFireStartPoint, lastFireEndPoint, Color.red, 0.1f);
    }

    /// <summary>
    /// 处理命中目标(仿照WeaponManager.HitTarget)
    /// </summary>
    private void HitTarget(RaycastHit raycastHit)
    {
        if (raycastHit.collider == null) return;

        if (raycastHit.collider.CompareTag("Enemy"))
        {
            ZombieStats enemy = raycastHit.collider.GetComponent<ZombieStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// 播放开火特效和音效
    /// </summary>
    private void PlayFireEffects()
    {
        // 播放枪口火焰
        if (muzzleFlashPrefab != null)
        {
            muzzleFlashPrefab.Emit(1);
        }

        // 播放弹壳抛射
        if (shellEjectPrefab != null)
        {
            shellEjectPrefab.Emit(1);
        }

        // 播放射击音效
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    /// <summary>
    /// 检查目标是否有效
    /// </summary>
    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;

        // 检查距离
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange) return false;

        // 检查是否还活着
        if (!target.CompareTag("Enemy")) return false;

        return true;
    }

    /// <summary>
    /// 开始敌人检测协程
    /// </summary>
    private void StartEnemyDetection()
    {
        if (enemyDetectionCoroutine != null)
        {
            StopCoroutine(enemyDetectionCoroutine);
        }
        enemyDetectionCoroutine = StartCoroutine(DetectEnemies());
    }

    /// <summary>
    /// 停止敌人检测协程
    /// </summary>
    private void StopEnemyDetection()
    {
        if (enemyDetectionCoroutine != null)
        {
            StopCoroutine(enemyDetectionCoroutine);
            enemyDetectionCoroutine = null;
        }
    }

    /// <summary>
    /// 检测敌人协程
    /// </summary>
    private IEnumerator DetectEnemies()
    {
        while (true)
        {
            // 如果没电，强制进入Idle状态
            if (!isPowered)
            {
                if (currentState != TurretState.Idle)
                {
                    SwitchToIdle();
                }
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            // 原有的敌人检测逻辑...
            if (currentState == TurretState.Idle)
            {
                Transform nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    currentTarget = nearestEnemy;
                    SwitchToFiring();
                    //Debug.Log($"[TurretController] 找到目标: {nearestEnemy.name}，切换到开火状态");
                }
            }
            else if (currentState == TurretState.Firing)
            {
                if (!IsTargetValid(currentTarget))
                {
                    //Debug.Log("[TurretController] 当前目标无效，寻找新目标");
                    Transform nearestEnemy = FindNearestEnemy();
                    if (nearestEnemy != null)
                    {
                        currentTarget = nearestEnemy;
                        //Debug.Log($"[TurretController] 切换到新目标: {nearestEnemy.name}");
                    }
                    else
                    {
                        //Debug.Log("[TurretController] 没有找到新目标，切换到空闲状态");
                        SwitchToIdle();
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// 寻找最近的敌人
    /// </summary>
    private Transform FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy.transform;
                }
            }
        }

        return nearestEnemy;
    }

    private void SwitchToIdle()
    {
        currentState = TurretState.Idle;
        currentTarget = null;
        firingTimer = 0f;
        restTimer = 0f;
        cooldown = 0f;
        //Debug.Log("[TurretController] 切换到空闲状态");
        // 继续敌人检测
    }

    private void SwitchToFiring()
    {
        currentState = TurretState.Firing;
        firingTimer = 0f;
        cooldown = 0f; // 立即开始射击
        //Debug.Log("[TurretController] 切换到开火状态");
        // 继续敌人检测以便切换目标
    }

    private void SwitchToResting()
    {
        currentState = TurretState.Resting;
        restTimer = 0f;
        //Debug.Log("[TurretController] 切换到休息状态");
        // 保持敌人检测，但休息时不会切换状态
    }

    #endregion



    /// <summary>
    /// 绘制攻击范围和射击轨迹的Gizmo
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制射击轨迹
        if (showFireTrajectory && hasLastFireData)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(lastFireStartPoint, lastFireEndPoint);

            // 在射击起点绘制一个小球
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(lastFireStartPoint, 0.1f);

            // 在射击终点绘制一个小球
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastFireEndPoint, 0.15f);
        }

        // 如果有当前目标，绘制指向目标的线
        if (currentTarget != null && firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(firePoint.position, currentTarget.position);

            // 在目标位置绘制一个标记
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
        }
    }

    private void OnDestroy()
    {
        StopEnemyDetection();
        UnregisterTurret();
    }

    #region Public Getters - 供UI和其他系统使用

    public int GetRequiredWoodNum() => requiredWoodNum;
    public int GetRequiredIronNum() => requiredIronNum;
    public float GetRequiredBuildingTime() => requiredBuildingTime;
    public int GetAttackDamage() => attackDamage;
    public float GetAttackCD() => fireRate;
    public float GetAttackRange() => attackRange;
    public float GetContinuousFireDuration() => continuousFireDuration;
    public float GetRestDuration() => restDuration;
    public AudioClip GetFireSound() => fireSound;
    public ParticleSystem GetMuzzleFlashPrefab() => muzzleFlashPrefab;
    public ParticleSystem GetShellEjectPrefab() => shellEjectPrefab;
    public TurretState GetCurrentState() => currentState;
    public Transform GetCurrentTarget() => currentTarget;
    public int GetTurretLevel() => level;
    public int GetPowerRequirement() => powerRequirement;
    public int GetCurrentHealth() => currentHealth;
    public bool IsFullHealth()
    {
        return currentHealth >= turretData.maxHealth;
    }

    // 添加电力管理方法
    /// <summary>
    /// 设置炮台电力状态
    /// </summary>
    public void SetPowered(bool powered)
    {
        if (isPowered == powered) return; // 避免重复设置

        isPowered = powered;

        if (!isPowered)
        {
            Debug.Log($"炮台 {name} 电力不足，暂停运行");
            // 断电时强制进入Idle状态
            SwitchToIdle();
        }
        else
        {
            Debug.Log($"炮台 {name} 电力恢复");
        }
    }

    public bool IsPowered() => isPowered;
    #endregion
}
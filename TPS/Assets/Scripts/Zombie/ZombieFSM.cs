using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieStates { Guard, PATROL, CHASE, ATTACK, DEAD, BERSERK_CHASE }

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieFSM : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private ZombieStates currentState;
    private Animator anim;
    private ZombieStats stats;

    [Header("Ref")]
    [SerializeField] private AITeammateState aiState;

    [Header("Basic Settings")]
    public bool isGuard = false; // 该敌人是否站桩
    public float lookAtTime = 2f; // 敌人每移动到新的位置停下来观察四周的时间
    private float remainLookAtTime; // 计时器：怪物巡逻停留剩余时间
    public float sightRadius; // 敌人发现敌人的半径
    private float speed; // 记录敌人追击前的初始速度

    [Header("Patrol State")] // ----------敌人巡逻设置
    public float patrolRange; // 随机生成新巡逻位置的范围
    private Vector3 guardPos;
    private Vector3 patrolPoint;
    private GameObject attackTarget;

    /*动画切换标志*/
    private bool isRunning;
    private bool isWalking;

    /*Root Motion*/
    private Vector2 smoothDeltaPosition;
    private Vector2 velocity;
    private float animSpeed;

    /* 攻击相关属性 */
    [Header("Attack State")]
    [SerializeField] private float bonusAttackRange = 2f;
    public float dealDamageRange;
    private float attackRange;
    private float attackCD;
    private float attackCooldownTimer = 0f; // 攻击冷却计时器
    private float attackDuration = 1.0f; // 攻击动画持续时间
    private float attackTimer = 0f; // 攻击状态持续计时器
    private bool isInAttackState = false; // 是否正在攻击状态
    private bool hasTriggeredAttack = false; // 本次攻击是否已经触发过动画和伤害
    private bool hasDealDamage = false; // 本次攻击是否已经造成过伤害（防止重复扣血）
    private ZombieStats zombieStats; // 引用ZombieStats来读取SO数据

    /* 狂暴模式相关 */
    private bool isBerserk = false; // 是否为狂暴状态
    private Transform playerTransform; // 玩家Transform缓存
    [Header("Berserk Settings")]
    public float berserkSpeed = 10f; // 狂暴模式移动速度

    [Header("Target Selection")]
    [Range(0f, 1f)]
    public float aiPlayerTargetChance = 0.4f; // AI玩家被选择为目标的概率
    public LayerMask targetLayerMask = -1; // 目标层级掩码，可在Inspector中设置
    [Range(0f, 1f)]
    public float targetSwitchChance = 0.2f; // 切换目标的概率（比选择概率更低）

    [Header("Berserk Target Selection")]
    [Range(0f, 1f)]
    public float berserkPlayerLockChance = 0.8f; // 狂暴状态锁定玩家概率
    [Range(0f, 1f)]
    public float berserkTargetSwitchChance = 0.2f; // AI队友→玩家的切换概率
    public float playerProximityThreshold = 10f; // 玩家和AI队友的距离阈值

    [Header("Global Target Selection")]
    [Range(0f, 1f)]
    public float globalBuildingTargetChance = 0.3f; // 选择建筑目标的概率
    public float targetSwitchInterval = 10f; // 目标切换间隔（秒）
    [Range(0f, 1f)]
    public float runtimeTargetSwitchChance = 0.15f; // 运行时切换概率

    public enum BerserkTargetType
    {
        Survival,      // 攻击玩家/AI队友
        PlayerBuilding // 攻击建筑
    }

    [SerializeField] private BerserkTargetType currentBerserkType;
    private float lastTargetSwitchTime = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        zombieStats = GetComponent<ZombieStats>();
        playerTransform = GameManager.Instance.GetPlayerTransform();
        stats = GetComponent<ZombieStats>();
        aiState = GameManager.Instance.GetAIAgentStats();
    }

    void Start()
    {
        LoadAttackDataFromSO();
        CheckBerserkState();
    }

    void Update()
    {
        // 玩家死亡 游戏结束 停止FSM
        if (GameManager.Instance.IsGameOver())
        {
            stats.Die();
            return;
        }

        if (currentState != ZombieStates.DEAD)
        {
            CheckTargetValidity();

            // 更新攻击冷却计时器
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= Time.deltaTime;
            }

            if (isInAttackState && attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }

            // 根据是否狂暴选择不同的更新逻辑
            if (isBerserk)
            {
                BerserkUpdate(); // 简化的狂暴模式逻辑
            }
            else
            {
                StateUpdate(); // 原有的完整状态机
            }

            SyncWithNavMeshAgentRootMotion(); // 每帧更新 RootMotion 位置同步
        }
    }

    /*更新敌人状态*/
    void StateUpdate()
    {

        // 只有在非攻击状态时才检查是否切换到追击
        if (currentState != ZombieStates.ATTACK && FoundPlayer())
        {
            if (currentState != ZombieStates.CHASE)
            {
                currentState = ZombieStates.CHASE;
                //Debug.Log("[ZombieFSM] 发现玩家，切换到追击状态");
            }
        }

        switch (currentState)
        {
            case ZombieStates.Guard:
                Guard();
                break;
            case ZombieStates.PATROL:
                Patrol();
                break;
            case ZombieStates.CHASE:
                Chase();
                break;
            case ZombieStates.ATTACK:
                Attack();
                break;
        }
    }

    public void ResetZombieFSM()
    {
        // 恢复导航组件
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;

            guardPos = transform.position;
            speed = agent.speed;

            agent.updatePosition = false; // 关闭自动位置更新
            agent.updateRotation = true; // 保留旋转控制
            anim.applyRootMotion = true; // 启用 Root Motion
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
        // 重新检查狂暴状态（这里会设置正确的速度和状态）
        CheckBerserkState();

        // 根据狂暴状态设置初始状态
        if (isBerserk)
        {
            // 狂暴模式直接进入狂暴追击状态
            currentState = ZombieStates.BERSERK_CHASE;
            if (playerTransform != null)
            {
                attackTarget = playerTransform.gameObject;
            }
        }
        else
        {
            // 普通模式根据是否守卫设置状态
            if (isGuard)
            {
                currentState = ZombieStates.Guard;
            }
            else
            {
                currentState = ZombieStates.PATROL;
                GetNewPatrolPoint(); // 选择新的巡逻点
            }
        }

        attackCooldownTimer = 0f;
    }

    #region 狂暴状态
    /// <summary>
    /// 检查狂暴状态
    /// </summary>
    private void CheckBerserkState()
    {
        if (zombieStats != null)
        {
            isBerserk = zombieStats.isBerserk;

            if (isBerserk)
            {
                 // 狂暴模式下立即锁定目标
            LockBerserkTarget();
                // 狂暴模式下直接进入狂暴追击状态
                currentState = ZombieStates.BERSERK_CHASE;
                //if (playerTransform != null)
                //{
                //    attackTarget = playerTransform.gameObject;
                //}
                // 设置狂暴速度
                agent.speed = berserkSpeed;
                speed = berserkSpeed;
            }
        }
    }

    private void LockBerserkTarget()
    {
        // 全图随机选择目标类型
        bool shouldAttackBuilding = Random.Range(0f, 1f) < globalBuildingTargetChance;

        if (shouldAttackBuilding)
        {
            currentBerserkType = BerserkTargetType.PlayerBuilding;
            LockGlobalBuildingTarget();
        }
        else
        {
            currentBerserkType = BerserkTargetType.Survival;
            LockPlayerTarget();
        }

        lastTargetSwitchTime = Time.time;
        //Debug.Log($"[ZombieFSM] {name} 狂暴目标选择 - 类型: {currentBerserkType}, 目标: {(attackTarget?.name ?? "无")}");
    }

    // 3. 添加新的方法：全图建筑目标选择
    private void LockGlobalBuildingTarget()
    {
        // 找到场景中所有玩家建筑
        GameObject[] allBuildings = GameObject.FindGameObjectsWithTag("PlayerBuilding");
        List<GameObject> validBuildings = new List<GameObject>();

        // 筛选有效建筑
        foreach (var building in allBuildings)
        {
            IBuildingController buildingController = building.GetComponent<IBuildingController>();
            if (buildingController != null && !buildingController.IsDestroyed())
            {
                validBuildings.Add(building);
            }
        }

        if (validBuildings.Count > 0)
        {
            // 优先选择发电机（60%概率）
            List<GameObject> generators = validBuildings.FindAll(b => b.GetComponent<GeneratorController>() != null);

            if (generators.Count > 0 && Random.Range(0f, 1f) < 0.6f)
            {
                // 随机选择一个发电机
                attackTarget = generators[Random.Range(0, generators.Count)];
                Debug.Log($"[ZombieFSM] {name} 全图选择发电机目标: {attackTarget.name}");
            }
            else
            {
                // 随机选择任意建筑
                attackTarget = validBuildings[Random.Range(0, validBuildings.Count)];
                Debug.Log($"[ZombieFSM] {name} 全图选择建筑目标: {attackTarget.name}");
            }
        }
        else
        {
            // 没有建筑，切换到攻击玩家
            Debug.Log($"[ZombieFSM] {name} 没有可攻击的建筑，切换到攻击玩家");
            currentBerserkType = BerserkTargetType.Survival;
            LockPlayerTarget();
        }
    }

    // 4. 添加新的方法：分离出来的玩家目标选择
    private void LockPlayerTarget()
    {
        GameObject player = null;
        GameObject aiPlayer = null;

        // 获取玩家
        if (playerTransform != null)
        {
            player = playerTransform.gameObject;
        }

        aiPlayer = GameManager.Instance.GetAIAgentTransform().gameObject;

        // 根据概率锁定目标
        if (player != null && aiPlayer != null)
        {
            // 80%概率锁定玩家，20%概率锁定AI队友
            bool lockPlayer = Random.Range(0f, 1f) < berserkPlayerLockChance;
            attackTarget = lockPlayer ? player : aiPlayer;
        }
        else if (player != null)
        {
            attackTarget = player;
        }
        else if (aiPlayer != null)
        {
            attackTarget = aiPlayer;
        }

        Debug.Log($"[ZombieFSM] {name} 全图选择玩家目标: {(attackTarget != null ? attackTarget.name : "无目标")}");
    }

    /// <summary>
    /// 狂暴模式的简化更新逻辑
    /// </summary>
    private void BerserkUpdate()
    {
        if (playerTransform == null) return;

        CheckRuntimeTargetSwitch();

        // 狂暴僵尸只有两种状态：狂暴追击和攻击
        switch (currentState)
        {
            case ZombieStates.BERSERK_CHASE:
                BerserkChase();
                break;
            case ZombieStates.ATTACK:
                Attack(); // 攻击逻辑保持不变
                break;
            default:
                // 其他状态强制切换到狂暴追击
                currentState = ZombieStates.BERSERK_CHASE;
                break;
        }
    }

    private void CheckRuntimeTargetSwitch()
    {
        // 检查是否到了切换时间
        if (Time.time - lastTargetSwitchTime < targetSwitchInterval) return;

        // 按概率决定是否切换
        if (Random.Range(0f, 1f) > runtimeTargetSwitchChance) return;

        // 执行目标切换
        bool currentlyAttackingBuilding = (currentBerserkType == BerserkTargetType.PlayerBuilding);

        // 50%概率切换目标类型，50%概率保持类型但换目标
        bool switchType = Random.Range(0f, 1f) < 0.5f;

        if (switchType)
        {
            // 切换目标类型
            if (currentlyAttackingBuilding)
            {
                Debug.Log($"[ZombieFSM] {name} 运行时切换：建筑 -> 玩家");
                currentBerserkType = BerserkTargetType.Survival;
                LockPlayerTarget();
            }
            else
            {
                Debug.Log($"[ZombieFSM] {name} 运行时切换：玩家 -> 建筑");
                currentBerserkType = BerserkTargetType.PlayerBuilding;
                LockGlobalBuildingTarget();
            }
        }
        else
        {
            // 保持类型，换个目标
            if (currentlyAttackingBuilding)
            {
                Debug.Log($"[ZombieFSM] {name} 运行时切换建筑目标");
                LockGlobalBuildingTarget();
            }
            else
            {
                Debug.Log($"[ZombieFSM] {name} 运行时切换玩家目标");
                LockPlayerTarget();
            }
        }

        lastTargetSwitchTime = Time.time;
    }

    /// <summary>
    /// 狂暴模式的追击逻辑
    /// </summary>
    private void BerserkChase()
    {
        isWalking = false;
        isRunning = true;

        // 确保使用狂暴速度
        agent.speed = berserkSpeed;

        // 检查是否需要切换目标（仅当当前目标是AI队友时）
        CheckBerserkTargetSwitch();

        if (attackTarget != null)
        {
            // 直接追击锁定的目标
            agent.SetDestination(attackTarget.transform.position);

            // 检查是否可以攻击
            float distanceToTarget = Vector3.Distance(transform.position, attackTarget.transform.position);

            float attackBonus = GetBuildingAttackBonus(attackTarget);
            float effectiveAttackRange = attackRange + attackBonus;

            if (distanceToTarget <= effectiveAttackRange && attackCooldownTimer <= 0f)
            {
                // 切换到攻击状态
                currentState = ZombieStates.ATTACK;
                isInAttackState = true;
                attackTimer = attackDuration;
                hasTriggeredAttack = false;
                hasDealDamage = false;
            }
        }
        else
        {
            // 目标丢失，重新锁定
            LockBerserkTarget();
        }
    }
    #endregion

    /// <summary>
    /// 检查当前目标是否已经死亡，如果是 AI 队友且死亡，则切换目标到玩家
    /// </summary>
    private void CheckTargetValidity()
    {
        if (attackTarget == null) return;

        if (attackTarget.CompareTag("AIPlayer"))
        {
            if (aiState != null && !aiState.IsAlive())
            {
                Debug.Log("[ZombieFSM] 目标 AI 队友已死亡，重新选择目标");

                if (isBerserk)
                {
                    // 狂暴状态下重新执行完整的目标选择逻辑（就像刚生成时一样）
                    LockBerserkTarget();
                    currentState = ZombieStates.BERSERK_CHASE;
                }
                else
                {
                    // 非狂暴状态切换到玩家
                    if (playerTransform != null)
                    {
                        attackTarget = playerTransform.gameObject;
                        currentState = ZombieStates.CHASE;
                    }
                }
            }
        }
        else if (attackTarget.CompareTag("PlayerBuilding"))
        {
            IBuildingController building = attackTarget.GetComponent<IBuildingController>();
            if (building != null && building.IsDestroyed())
            {
                Debug.Log("[ZombieFSM] 目标建筑已被摧毁，重新选择目标");

                if (isBerserk)
                {
                    // 狂暴状态下重新执行完整的目标选择逻辑（就像刚生成时一样）
                    LockBerserkTarget();
                    currentState = ZombieStates.BERSERK_CHASE;
                }
                else
                {
                    // 非狂暴状态回到正常逻辑
                    attackTarget = null;
                    currentState = ZombieStates.CHASE;
                }
            }
        }
        else if (attackTarget.CompareTag("Player"))
        {
            // 检查玩家是否死亡
            PlayerStats playerStats = attackTarget.GetComponent<PlayerStats>();
            if (playerStats != null && playerStats.GetCurrentHealth() <= 0) // 使用血量判断
            {
                Debug.Log("[ZombieFSM] 目标玩家已死亡，重新选择目标");

                if (isBerserk)
                {
                    // 狂暴状态下重新执行完整的目标选择逻辑
                    LockBerserkTarget();
                    currentState = ZombieStates.BERSERK_CHASE;
                }
                else
                {
                    // 非狂暴状态寻找其他目标或回到巡逻
                    attackTarget = null;
                    if (isGuard)
                    {
                        currentState = ZombieStates.Guard;
                        remainLookAtTime = lookAtTime;
                    }
                    else
                    {
                        currentState = ZombieStates.PATROL;
                        GetNewPatrolPoint();
                    }
                }
            }
        }
    }

    private float GetBuildingAttackBonus(GameObject building)
    {
        if (!building.CompareTag("PlayerBuilding")) return 0f;

        // 根据建筑类型返回不同的攻击范围补偿
        if (building.GetComponent<StorageController>() != null)
        {
            return bonusAttackRange; // 仓库比较大，需要4米补偿
        }
        return 0f;
    }

    private string GetBuildingTypeName(GameObject building)
    {
        if (building.GetComponent<GeneratorController>() != null)
            return "发电机";
        else if (building.GetComponent<TurretController>() != null)
            return "炮台";
        else if (building.GetComponent<StorageController>() != null)
            return "仓库";
        else
            return "建筑";
    }

    private void Guard()
    {
        isRunning = false;
        if (agent.transform.position != guardPos) // 返回站桩点
        {
            isWalking = true;
            agent.SetDestination(guardPos);
            agent.speed = speed * 0.5f;
            if (Vector3.Distance(guardPos, transform.position) <= agent.stoppingDistance)
            {
                isWalking = false;
            }
        }
    }

    private void Patrol()
    {
        //Debug.Log("[ZombieFSM] 处于Patrol状态");
        agent.speed = speed * 0.5f;
        if (Vector3.Distance(patrolPoint, transform.position) <= agent.stoppingDistance)
        {
            isWalking = false; // Animation Flag
            agent.speed = 0f;
            if (remainLookAtTime > 0)
            {
                remainLookAtTime -= Time.deltaTime;
            }
            else
            {
                GetNewPatrolPoint();
            }
        }
        else
        {
            isWalking = true;
            agent.destination = patrolPoint;
        }
    }

    private void Chase()
    {
        isWalking = false;
        isRunning = true;
        agent.speed = speed;
        if (!FoundPlayer()) // 玩家拉脱僵尸——返回上一个状态
        {
            isRunning = false;
            if (remainLookAtTime > 0) // 脱战停在原地
            {
                agent.destination = transform.position;
                remainLookAtTime -= Time.deltaTime;
            }
            else if (isGuard) // 如果是站桩 就回到站桩点
            {
                currentState = ZombieStates.Guard;
                remainLookAtTime = lookAtTime; // 重置时间计数器，希望它也能：脱战停在原地
            }
            else // 返回巡逻状态
            {
                currentState = ZombieStates.PATROL;
            }
        }
        else
        {
            //Debug.Log("[ZombieFSM] 处于Chasing状态");
            isRunning = true;
            // 检查是否可以攻击
            float distanceToPlayer = Vector3.Distance(transform.position, attackTarget.transform.position);
            if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0f)
            {
                currentState = ZombieStates.ATTACK;
                isInAttackState = true;
                attackTimer = attackDuration;
                hasTriggeredAttack = false; // 重置攻击触发标记
                hasDealDamage = false; // 重置伤害标记
                //Debug.Log("[ZombieFSM] 进入攻击状态！距离: " + distanceToPlayer);
                return;
            }
            agent.destination = attackTarget.transform.position;
        }
    }

    private void Attack()
    {
        isWalking = false;
        isRunning = false;



        // 面向目标
        if (attackTarget != null)
        {
            Vector3 directionToTarget = (attackTarget.transform.position - transform.position).normalized;
            directionToTarget.y = 0; // 只在水平面旋转
            if (directionToTarget != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(directionToTarget), Time.deltaTime * 5f);
            }
        }

        // 只在刚进入攻击状态时执行一次
        if (isInAttackState && !hasTriggeredAttack)
        {
            // 50%概率停止移动
            bool shouldStop = Random.Range(0f, 1f) < 0.5f;
            if (shouldStop)
            {
                agent.SetDestination(transform.position);

            }

            // 随机选择攻击动画
            bool useAttack2 = Random.Range(0f, 1f) < 0.5f;
            string attackTrigger = useAttack2 ? "Attack2" : "Attack";

            // 触发攻击动画
            if (anim != null)
            {
                anim.SetTrigger(attackTrigger);
            }

            // 设置冷却时间
            attackCooldownTimer = attackCD;

            // 标记已经触发过攻击
            hasTriggeredAttack = true;
        }
        if (attackTimer <= 0f)
        {
            FinishAttack();
        }
    }

    // 完成攻击的统一处理方法
    private void FinishAttack()
    {
        // 重置攻击状态
        isInAttackState = false;
        hasTriggeredAttack = false;
        hasDealDamage = false;

        // *** 重要：重置Attack Trigger，防止动画卡住 ***
        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.ResetTrigger("Attack2");
        }

        // 狂暴模式下攻击完成后直接继续狂暴追击
        if (isBerserk)
        {
            currentState = ZombieStates.BERSERK_CHASE;
            return;
        }

        // 检查目标状态决定下一步
        if (attackTarget != null && Vector3.Distance(transform.position, attackTarget.transform.position) <= sightRadius)
        {
            currentState = ZombieStates.CHASE;
        }
        else
        {
            // 失去目标，返回原状态
            if (isGuard)
            {
                currentState = ZombieStates.Guard;
                remainLookAtTime = lookAtTime;
            }
            else
            {
                currentState = ZombieStates.PATROL;
                GetNewPatrolPoint();
            }
        }
    }

    // *** 新增：动画事件回调 - 在动画中造成伤害 ***
    public void OnAttackHit()
    {
        if (currentState == ZombieStates.ATTACK && !hasDealDamage)
        {
            DealDamage();
            hasDealDamage = true;
        }
    }

    // 造成伤害
    private void DealDamage()
    {
        if (attackTarget == null) return;
        // 再次检查距离
        float distanceToTarget = Vector3.Distance(transform.position, attackTarget.transform.position);

        float damageBonus = GetBuildingAttackBonus(attackTarget);
        float effectiveDamageRange = dealDamageRange + damageBonus;

        if (distanceToTarget <= effectiveDamageRange)
        {
            if (attackTarget.CompareTag("Player"))
            {
                // 对玩家造成标准伤害
                PlayerStats playerStats = attackTarget.GetComponent<PlayerStats>();
                if (playerStats != null && zombieStats != null && zombieStats.zombieAttackData != null)
                {
                    int damage = zombieStats.zombieAttackData.attackDamage;
                    playerStats.TakeDamage(damage);
                    //Debug.Log($"[ZombieFSM] 对玩家造成 {damage} 点伤害！");
                }
            }
            else if (attackTarget.CompareTag("AIPlayer"))
            {
                // 对AI队友造成伤害（可以调整倍率）
                AITeammateState aiPlayerStats = attackTarget.GetComponent<AITeammateState>();
                if (aiPlayerStats != null && zombieStats != null && zombieStats.zombieAttackData != null)
                {
                    int baseDamage = zombieStats.zombieAttackData.attackDamage;
                    int aiDamage = Mathf.RoundToInt(baseDamage * 1.0f); // 可调整倍率
                    aiPlayerStats.AITakeDamage(aiDamage);
                    //Debug.Log($"[ZombieFSM] 对AI队友造成 {aiDamage} 点伤害！");
                }
            }
            else if (attackTarget.CompareTag("PlayerBuilding"))
            {
                // 对建筑造成伤害
                IBuildingController building = attackTarget.GetComponent<IBuildingController>();

                int buildingDamage = zombieStats.zombieAttackData.attackDamage;
                building.TakeDamage(buildingDamage);

                string buildingType = GetBuildingTypeName(attackTarget);
                Debug.Log($"[ZombieFSM] 对{buildingType}造成 {buildingDamage} 点伤害！");

                // 检查建筑是否被摧毁
                if (building.IsDestroyed())
                {
                    Debug.Log($"[ZombieFSM] {buildingType}已被摧毁！");
                    // 重新选择目标
                    if (isBerserk)
                    {
                        LockBerserkTarget();
                    }
                }
            }
        }
       
    }

    public void EnterDeadState(bool isAlive)
    {
        currentState = ZombieStates.DEAD;

        // 停止导航移动
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
            anim.SetBool("isAlive", isAlive);
        }

        // 禁用碰撞体
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    /// <summary>
    /// 检查狂暴状态下是否切换目标（AI队友→玩家）
    /// </summary>
    private void CheckBerserkTargetSwitch()
    {
        // 只有当前目标是AI队友时才考虑切换
        if (attackTarget == null || !attackTarget.CompareTag("AIPlayer"))
            return;

        // 检查玩家是否存在
        if (playerTransform == null)
            return;

        // 计算AI队友和玩家的距离
        float distanceToPlayer = Vector3.Distance(attackTarget.transform.position, playerTransform.position);

        // 只有当距离小于阈值时才考虑切换
        if (distanceToPlayer < playerProximityThreshold)
        {
            // 20%概率切换到玩家
            if (Random.Range(0f, 1f) < berserkTargetSwitchChance)
            {
                attackTarget = playerTransform.gameObject;
                Debug.Log($"[ZombieFSM] 狂暴僵尸切换目标到玩家（距离: {distanceToPlayer:F1}m）");
            }
        }
    }

    private bool FoundPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRadius, targetLayerMask);

        GameObject playerTarget = null;
        GameObject aiPlayerTarget = null;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                playerTarget = col.gameObject;
            }
            else if (col.CompareTag("AIPlayer"))
            {
                aiPlayerTarget = col.gameObject;
            }
        }

        // 智能目标选择逻辑
        GameObject selectedTarget = SmartTargetSelection(playerTarget, aiPlayerTarget);

        if (selectedTarget != null)
        {
            attackTarget = selectedTarget;
            return true;
        }

        attackTarget = null;
        return false;
    }

    /// <summary>
    /// 智能目标选择：优先保持当前目标，按概率考虑切换
    /// </summary>
    private GameObject SmartTargetSelection(GameObject playerTarget, GameObject aiPlayerTarget)
    {
        // 情况1：没有目标
        if (playerTarget == null && aiPlayerTarget == null)
            return null;

        // 情况2：只有一个目标，直接选择
        if (playerTarget != null && aiPlayerTarget == null)
            return playerTarget;
        if (aiPlayerTarget != null && playerTarget == null)
            return aiPlayerTarget;

        // 情况3：两个目标都存在
        if (playerTarget != null && aiPlayerTarget != null)
        {
            // 如果已经有目标，优先保持当前目标（减少频繁切换）
            if (attackTarget != null)
            {
                // 检查当前目标是否还在视野内
                if (attackTarget == playerTarget || attackTarget == aiPlayerTarget)
                {
                    // 小概率切换目标（避免过于固执）
                    if (Random.Range(0f, 1f) < targetSwitchChance)
                    {
                        return attackTarget == playerTarget ? aiPlayerTarget : playerTarget;
                    }
                    return attackTarget; // 保持当前目标
                }
            }

            // 首次发现或当前目标丢失，按概率选择新目标
            return Random.Range(0f, 1f) < aiPlayerTargetChance ? aiPlayerTarget : playerTarget;
        }

        return null;
    }

    private void GetNewPatrolPoint()
    {
        remainLookAtTime = lookAtTime; // 充值时间计数器
        //Debug.Log("选择新巡逻地点！");
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPatrolPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        patrolPoint = NavMesh.SamplePosition(randomPatrolPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    /*绘画敌人巡逻范围的Gizmo*/
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
        // 攻击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void LoadAttackDataFromSO()
    {
        if (zombieStats != null && zombieStats.zombieAttackData != null)
        {
            attackRange = zombieStats.zombieAttackData.attackRange;
            attackCD = zombieStats.zombieAttackData.attackCD;
            //Debug.Log($"加载攻击数据：范围={attackRange}, 冷却={attackCD}");
        }
        else
        {
            // 默认值
            attackRange = 2.0f;
            attackCD = 1.0f;
            Debug.LogWarning("未找到攻击数据SO，使用默认值");
        }
    }
    /// <summary>
    /// 在使用 Root Motion 时，同步 NavMeshAgent 与 transform 的位置/动画速度，避免滑动/漂移
    /// 应在 Update() 中每帧调用
    /// </summary>
    /// 
    private void SyncWithNavMeshAgentRootMotion()
    {
        if (agent == null || anim == null) return;

        // 计算 NavMeshAgent 期望移动的位置与角色当前 transform 的差值
        Vector3 worldDelta = agent.nextPosition - transform.position;
        worldDelta.y = 0;

        // 将差值转换为本地空间（forward/right）
        float dx = Vector3.Dot(transform.right, worldDelta);
        float dy = Vector3.Dot(transform.forward, worldDelta);
        Vector2 delta = new Vector2(dx, dy);

        // 平滑过渡（防止跳变）
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, delta, Time.deltaTime * 10f);
        velocity = smoothDeltaPosition / Mathf.Max(Time.deltaTime, 0.001f);
        animSpeed = velocity.magnitude;

        // 设置动画速度参数
        anim.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
     
        // 如果偏差较大，强制同步位置（避免滑动）
        float deltaMag = worldDelta.magnitude;

        /*FIX BUG
         * 处理transform.position 和 agent.nextPosition 出现了持续偏移，
         * 导致 velocity 被误算为一个异常大的值，从而造成动画 Speed 参数异常飙升（17~19）。
         */
        transform.position = agent.nextPosition; 
    }
}

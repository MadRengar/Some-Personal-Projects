using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieStates { Guard, PATROL, CHASE, ATTACK, DEAD }

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieFSM : MonoBehaviour
{
    private NavMeshAgent agent;
    private ZombieStates currentState;
    private Animator anim;

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
    private float attackRange;
    private float attackCD;
    private float attackCooldownTimer = 0f; // 攻击冷却计时器
    private float attackDuration = 1.0f; // 攻击动画持续时间
    private float attackTimer = 0f; // 攻击状态持续计时器
    private bool isInAttackState = false; // 是否正在攻击状态
    private bool hasTriggeredAttack = false; // 本次攻击是否已经触发过动画和伤害
    private bool hasDealDamage = false; // 本次攻击是否已经造成过伤害（防止重复扣血）
    private ZombieStats zombieStats; // 引用ZombieStats来读取SO数据

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        zombieStats = GetComponent<ZombieStats>();
    }

    void Start()
    {
        LoadAttackDataFromSO();
    }

    void Update()
    {
        if (currentState != ZombieStates.DEAD)
        {
            // 更新攻击冷却计时器
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= Time.deltaTime;
            }

            if (isInAttackState && attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }

            StateUpdate();
            SyncWithNavMeshAgentRootMotion(); // 每帧更新 RootMotion 位置同步
        }
    }

    // SO文件加载攻击数据
    private void LoadAttackDataFromSO()
    {
        if (zombieStats != null && zombieStats.zombieAttackData != null)
        {
            attackRange = zombieStats.zombieAttackData.attackRange;
            attackCD = zombieStats.zombieAttackData.attackCD;
            Debug.Log($"加载攻击数据：范围={attackRange}, 冷却={attackCD}");
        }
        else
        {
            // 默认值
            attackRange = 2.0f;
            attackCD = 1.0f;
            Debug.LogWarning("未找到攻击数据SO，使用默认值");
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
                Debug.Log("[ZombieFSM] 发现玩家，切换到追击状态");
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

        /*判断敌人是站桩类型的 or 巡逻状态*/
        if (isGuard)
        {
            currentState = ZombieStates.Guard;
        }
        else
        {
            currentState = ZombieStates.PATROL;
            GetNewPatrolPoint(); // 选择新的巡逻点
        }
        attackCooldownTimer = 0f;
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

        // 停止移动
        agent.SetDestination(transform.position);

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
            // 触发攻击动画
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            // 设置冷却时间
            attackCooldownTimer = attackCD;

            // 标记已经触发过攻击
            hasTriggeredAttack = true;
        }
        if (attackTimer <= 0f)
        {
            Debug.Log("[ZombieFSM] 攻击计时器结束，强制结束攻击");
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
        Debug.Log($"[ZombieFSM] OnAttackHit被调用！当前状态: {currentState}, hasDealDamage: {hasDealDamage}");
        if (currentState == ZombieStates.ATTACK && !hasDealDamage)
        {
            Debug.Log("1");
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
        if (distanceToTarget <= attackRange)
        {         
            PlayerStats playerStats = attackTarget.GetComponent<PlayerStats>();
            if (playerStats != null && zombieStats != null && zombieStats.zombieAttackData != null)
            {
                int damage = zombieStats.zombieAttackData.attackDamage;
                playerStats.TakeDamage(damage);
                Debug.Log($"[ZombieFSM] 对玩家造成 {damage} 点伤害！");
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

    private bool FoundPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                attackTarget = col.gameObject; // 将攻击目标选择为玩家
                //Debug.Log("[ZombieFSM] 发现玩家");
                return true;
            }
        }
        attackTarget = null;
        return false;
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
    //TODO:僵尸攻击
}

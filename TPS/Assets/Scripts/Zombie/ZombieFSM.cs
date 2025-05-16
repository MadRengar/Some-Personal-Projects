using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieState { Guard, PATROL, CHASE, ATTACK, DEAD }

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieFSM : MonoBehaviour
{
    private NavMeshAgent agent;
    private ZombieState currentState;
    private Animator anim;
    private ZombieStats stats;

    [Header("Basic Settings")]
    public bool isGuard = false; // 该敌人是否站桩
    public float lookAtTime = 2f; // 敌人每移动到新的位置停下来观察四周的时间
    private float remainLookAtTime; // 计时器：怪物巡逻停留剩余时间
    public float sightRadius; // 敌人发现敌人的半径
    private float speed; // 记录敌人追击前的初始速度
    [SerializeField]private float attackRange;
    [SerializeField]private float attackCD;

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


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        stats = GetComponent<ZombieStats>();
        guardPos = transform.position;
        speed = agent.speed;
    }

    void Start()
    {
        agent.updatePosition = false; // 关闭自动位置更新
        agent.updateRotation = true; // 保留旋转控制
        anim.applyRootMotion = true; // 启用 Root Motion

        /*判断敌人是站桩类型的 or 巡逻状态*/
        if (isGuard)
        {
            currentState = ZombieState.Guard;
        }    
        else
        {
            currentState = ZombieState.PATROL;
            GetNewPatrolPoint(); // 选择新的巡逻点
        }
    }

    void Update()
    {
        if (currentState != ZombieState.DEAD)
        {
            StateUpdate();
            SyncWithNavMeshAgentRootMotion(); // 每帧更新 RootMotion 位置同步
        }
    }

    /*更新敌人状态*/
    void StateUpdate()
    {
        if (FoundPlayer())
        {
            currentState = ZombieState.CHASE;
        }

        switch (currentState)
        {
            case ZombieState.Guard:
                Guard();
                break;
            case ZombieState.PATROL:
                Patrol();
                break;
            case ZombieState.CHASE:
                Chase();
                break;
            case ZombieState.ATTACK:
                Attack();
                break;
        }
    }

    void Guard()
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

    void Patrol()
    {
        Debug.Log("处于Patrol状态");
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

    void Chase()
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
                currentState = ZombieState.Guard;
                remainLookAtTime = lookAtTime; // 重置时间计数器，希望它也能：脱战停在原地
            }
            else // 返回巡逻状态
            {
                currentState = ZombieState.PATROL;
            }
        }
        else
        {
            Debug.Log("处于Chasing状态");
            isRunning = true;
            agent.destination = attackTarget.transform.position;
        }
    }

    void Attack()
    {
        Debug.Log("处于Attack状态");
    }

    bool FoundPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                attackTarget = col.gameObject; // 将攻击目标选择为玩家
                Debug.Log("发现玩家");
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    void GetNewPatrolPoint()
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
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
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

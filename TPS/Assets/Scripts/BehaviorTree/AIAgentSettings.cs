using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 所有行为树中的节点需要使用到的具体数据，都应该从外部获取。
/// 因此这个脚本的职责，就是统一设置AIAgent行为树中的数据的。
/// </summary>
public class AIAgentSettings : MonoBehaviour
{
    [Header("AI Agent Basic Setting")]
    public float stopDistance = 2.5f; // 跟随玩家的距离
    public float idleDurationBeforePatrol = 3.0f; //玩家静止超过该时间（秒）后，AI 开始巡逻
    public float patrolRadiusAroundPlayer = 5.0f; // AI 围绕玩家巡逻的最大半径
    public float patrolWaitTime = 1.5f; // AI 在每个巡逻点等待的时间

    public float minDistanceToPing = 3.0f; // 可选：与玩家的最小距离，避免 AI 原地执行
    /*TODO：容忍ai错误寻路时间
      由于一些原因阻止ai代理无法真正的到达目标点，从而在目标点附近附近持续转圈
     */
    public float timeToFindPathCount = 0f;
    public float timeToFindPath = 2.0f; 
    public float margin = 1.0f; // 容忍范围

    /*NavMesh Agent Settings*/
    private Animator animator;
    private float agentCurrentSpeed;
    private NavMeshAgent agent;
    private Vector2 smoothDeltaPosition;
    private Vector2 velocity;
    private float speed;


    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        /// <summary>
        /// NavMeshAgent：只负责路径规划（关闭位置更新）
        /// Animator + Root Motion：负责实际移动
        /// OnAnimatorMove()：同步 Root Motion 与 NavMeshAgent
        /// 
        /// 目标：通过 NavMeshAgent 的位置 → 推导出“移动方向和速度”，
        /// 然后平滑地传递给动画系统，让 Blend Tree 不卡顿、不抖动。
        /// </summary>
        agent.updatePosition = false; // 关闭自动位置更新
        agent.updateRotation = true; // 保留旋转控制
        animator.applyRootMotion = true; // 启用 Root Motion
    }

    private void Update()
    {
        if (agent == null || animator == null) return;

        /*
         * 实际位置偏差:
         * NavMeshAgent 想去的“下一个点” - 当前的位置
         */
        Vector3 worldDelta = agent.nextPosition - transform.position;
        worldDelta.y = 0f;

        /*
         * 转换为局部方向:
         * 把世界空间的偏移转换为角色局部空间：
         * dx：向右方向的偏移
         * dy：向前方向的偏移
         */
        float dx = Vector3.Dot(transform.right, worldDelta);
        float dy = Vector3.Dot(transform.forward, worldDelta);
        Vector2 delta = new Vector2(dx, dy);

        // 平滑过渡
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, delta, Time.deltaTime * 10f);
        velocity = smoothDeltaPosition / Mathf.Max(Time.deltaTime, 0.001f); // 位移 / 时间

        /*
         * 这是 Agent 的“结果速度”用于动画驱动，不是设定的 agent.speed
         * 例如：正常行走 → 稳定在 2.0 ~ 3.5 之间（取决于 agent.speed）；
         *      到达终点 → 缓慢减小 → 最终为 0。
         */
        speed = velocity.magnitude;

        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime); // 平滑传递速度

        

        // 停止漂移（过大偏差时）
        float deltaMag = worldDelta.magnitude;
        if (deltaMag > agent.radius * 0.5f)
        {
            transform.position = Vector3.Lerp(transform.position, agent.nextPosition, Time.deltaTime * 5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在 Scene 视图中画出 AI 停止距离圈和巡逻范围
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); // 半透明橙色
        Gizmos.DrawWireSphere(transform.position, patrolRadiusAroundPlayer);
    }

    private void OnAnimatorMove()
    {
        if (agent == null) return;

        // 获取 Animator 提供的位置（Root Motion）
        Vector3 rootPosition = animator.rootPosition;
        rootPosition.y = agent.nextPosition.y; // 确保 Y 值与 NavMeshAgent 匹配

        transform.position = rootPosition;
        agent.nextPosition = rootPosition;
        // 可选：同步旋转（若动画有 root rotation）
    }
}

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

    public float minDistanceToPing = 3.0f;    // 可选：与玩家的最小距离，避免 AI 原地执行

    private void Reset()
    {
        // 同步 NavMeshAgent 停止距离
        GetComponent<NavMeshAgent>().stoppingDistance = stopDistance;
    }

    private void OnDrawGizmosSelected()
    {
        // 在 Scene 视图中画出 AI 停止距离圈和巡逻范围
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f); // 半透明橙色
        Gizmos.DrawWireSphere(transform.position, patrolRadiusAroundPlayer);
    }
}

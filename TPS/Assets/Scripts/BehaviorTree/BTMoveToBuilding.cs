using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTMoveToBuilding : Action
{
    public SharedBool pingCommandActive; // 指令是否激活
    public SharedVector3 pingPosition; // 建筑物位置
    public SharedString currentCommand; // 当前指令
    public SharedBool isTargetBuilding; // 标记目标是否为建筑物

    private NavMeshAgent agent;
    private AIAgentSettings agentSettings;
    private PingMarkerManager pingManager;
    private float stopDistanceToMark;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        agentSettings = GetComponent<AIAgentSettings>();
        pingManager = GameManager.Instance.GetPingMarkerManager();
        stopDistanceToMark = agentSettings.stopDistanceToMark;
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null)
        {
            Debug.LogError("BTMoveToBuilding: 缺少NavMeshAgent！");
            return TaskStatus.Failure;
        }

        // 检查指令是否仍然有效
        if (!pingCommandActive.Value || (currentCommand.Value != "repair_building" && currentCommand.Value != "defend_building"))
        {
            //Debug.Log("建筑物指令已取消或改变");
            agent.ResetPath();
            currentCommand.Value = "";
            return TaskStatus.Failure;
        }

        // 检查是否仍然标记着建筑物
        if (pingManager != null && !pingManager.IsCurrentTargetBuilding())
        {
            //Debug.Log("不再标记建筑物");
            agent.ResetPath();
            currentCommand.Value = "";
            return TaskStatus.Failure;
        }

        // 检查建筑物是否仍然存在
        GameObject markedBuilding = null;
        if (pingManager != null)
        {
            markedBuilding = pingManager.GetCurrentMarkedBuilding();
            if (markedBuilding == null)
            {
                Debug.LogWarning("标记的建筑物已消失");
                agent.ResetPath();
                currentCommand.Value = "";
                return TaskStatus.Failure;
            }
        }

        float dist = Vector3.Distance(transform.position, pingPosition.Value);

        // 检查是否到达建筑物
        if (dist <= stopDistanceToMark)
        {
            Debug.Log($"已到达建筑物: {(markedBuilding != null ? markedBuilding.name : "未知建筑")}");
            agent.ResetPath();
            return TaskStatus.Success;
        }
        else
        {
            // 继续移动到建筑物
            Debug.Log($"移动到建筑物中... 剩余距离: {dist:F1}m");
            agent.SetDestination(pingPosition.Value);
            return TaskStatus.Running;
        }
    }
}
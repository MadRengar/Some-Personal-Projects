using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTHasValidPingCommand : Conditional
{
    public SharedBool pingCommandActive;
    public SharedVector3 pingPosition;
    public SharedTransform player;
    private AIAgentSettings agentSettings;

    public override void OnStart()
    {
        agentSettings = GetComponent<AIAgentSettings>();
    }
    public override TaskStatus OnUpdate()
    {
        if (!pingCommandActive.Value)
        {
            Debug.Log("Ping命令未激活");
            return TaskStatus.Failure;
        }
        if (pingPosition.Value == Vector3.zero)
        {
            Debug.Log("Ping位置无效");
            return TaskStatus.Failure;
        }
        if (player.Value != null && agentSettings != null)
        {
            float distance = Vector3.Distance(player.Value.position, pingPosition.Value);
            if(distance < agentSettings.minDistanceToPing)
            {
                Debug.Log("Ping位置距离太近，无需前往");
                return TaskStatus.Failure;
            }
        }
        Debug.Log("有效的 Ping 命令 + 合法位置");
        return TaskStatus.Success;
    }
}

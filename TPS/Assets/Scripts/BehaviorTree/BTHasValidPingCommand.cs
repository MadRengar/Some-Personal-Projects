using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTHasValidPingCommand : Conditional
{
    public SharedBool pingCommandActive; // 指令是否激活
    public SharedVector3 pingPosition; // Marker的位置
    public SharedTransform player;
    public SharedString currentCommand; // 当前指令
    private AIAgentSettings agentSettings;
    private float minDistanceToPing; // 最短ping距离
    public override void OnStart()
    {
        agentSettings = GetComponent<AIAgentSettings>();
    }
    public override TaskStatus OnUpdate()
    {
        if (pingCommandActive.Value && currentCommand.Value == "move_to_mark")
        {
            if (pingPosition.Value == Vector3.zero)
            {
                //Debug.Log("Ping位置无效");
                return TaskStatus.Failure;
            }
            if (player.Value != null && agentSettings != null)
            {
                float distance = Vector3.Distance(player.Value.position, pingPosition.Value);
                if (agentSettings != null)
                {
                    minDistanceToPing = agentSettings.minDistanceToPing;
                }

                if (distance < minDistanceToPing)
                {
                    //Debug.Log("Ping位置距离太近，无需前往");
                    return TaskStatus.Failure;
                }
            }
            //Debug.Log("有效的 Ping 命令 + 合法位置");
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}

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
    public SharedBool isTargetBuilding; //标记目标是否为建筑物

    private AIAgentSettings agentSettings;
    private float minDistanceToPing; // 最短ping距离
    private PingMarkerManager pingManager;

    public override void OnStart()
    {
        agentSettings = GetComponent<AIAgentSettings>();
        pingManager = GameManager.Instance.GetPingMarkerManager();
    }
    public override TaskStatus OnUpdate()
    {
        if (pingCommandActive.Value && currentCommand.Value == "move_to_mark")
        {
            if (pingPosition.Value == Vector3.zero)
            {
                RadioPopController.Instance.ShowMessage(MessageKey.PingMove_unsuccess, RadioPopController.MessageType.Error);
                return TaskStatus.Failure;
            }

            if (pingManager != null)
            {
                bool isBuilding = isTargetBuilding.Value;

                if (isBuilding)
                {
                    RadioPopController.Instance.ShowMessage(MessageKey.PingMove_unsuccess, RadioPopController.MessageType.Error);
                    return TaskStatus.Failure;
                }
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
                    RadioPopController.Instance.ShowMessage(MessageKey.Ping_tooclose, RadioPopController.MessageType.Warning);
                    return TaskStatus.Failure;
                }
            }
            RadioPopController.Instance.ShowMessage(MessageKey.PingMove_success, RadioPopController.MessageType.Info);
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}

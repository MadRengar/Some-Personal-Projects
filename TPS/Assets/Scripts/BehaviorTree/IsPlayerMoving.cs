using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using PlayerControl;

public class IsPlayerMoving : Conditional
{
    // SharedTransform（Shared Variable）类型之一，用于在行为树中的节点之间传递 Unity 中的 Transform 引用
    public SharedTransform player;
    // 判断移动的速度阈值，小于此值认为玩家静止
    public float inputThreshold = 0.1f;

    public override TaskStatus OnUpdate()
    {
        Debug.Log("正在检测玩家是否在移动...");
        if (player.Value == null)
        {
            Debug.LogWarning("IsPlayerMoving: player 未设置！");
            return TaskStatus.Failure;
        }

        ThirdPersonController controller = player.Value.GetComponent<ThirdPersonController>();
        if (controller == null)
        {
            Debug.LogWarning("IsPlayerMoving: 找不到 ThirdPersonController 脚本！");
            return TaskStatus.Failure;
        }

        PlayerInputSystem inputSystem = controller.GetComponent<PlayerInputSystem>();
        if (inputSystem == null)
        {
            Debug.LogWarning("IsPlayerMoving: 找不到 PlayerInputSystem！");
            return TaskStatus.Failure;
        }

        if(inputSystem.move.magnitude > inputThreshold)
        {
            Debug.Log("玩家正在移动");
            return TaskStatus.Success;
        }
        Debug.Log("玩家未移动");
        return TaskStatus.Failure;
    }
}

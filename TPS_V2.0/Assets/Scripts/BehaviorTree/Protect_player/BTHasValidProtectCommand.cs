using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTHasValidProtectCommand : Conditional
{
    [Header("Shared Variables")]
    public SharedString currentCommand; // 当前指令
    public SharedTransform player; // 玩家引用
    public SharedBool protectMode; // 保护模式标志位    

    public override TaskStatus OnUpdate()
    {
        // 检查指令是否为保护玩家
        if (currentCommand.Value != "protect_player")
        {
            // 不是保护指令，关闭保护模式
            protectMode.Value = false;
            return TaskStatus.Failure;
        }

        // 检查玩家是否存在
        if (player.Value == null)
        {
            protectMode.Value = false;
            return TaskStatus.Failure;
        }

        // 所有条件满足，激活保护模式
        protectMode.Value = true;
        return TaskStatus.Success;
    }
}
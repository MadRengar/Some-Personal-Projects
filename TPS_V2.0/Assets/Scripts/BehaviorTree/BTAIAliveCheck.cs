using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// AI存活检查节点 - 放在行为树根节点附近
/// 如果AI死亡，直接返回失败，阻止执行后续所有行为
/// </summary>
public class BTAIAliveCheck : Conditional
{
    public SharedBool isAIAlive;

    private AITeammateState aiTeammateState;

    public override void OnStart()
    {
        aiTeammateState = GetComponent<AITeammateState>();
    }

    public override TaskStatus OnUpdate()
    {
        if (aiTeammateState == null)
        {
            Debug.LogError("BTAIAliveCheck: 找不到 AITeammateState 组件");
            return TaskStatus.Failure;
        }

        // 检查AI是否存活
        isAIAlive.Value = GameManager.Instance.CheckAIIsAlive();

        if (!isAIAlive.Value)
        {
            Debug.Log("AI已死亡，停止所有行为树执行");
            return TaskStatus.Failure;
        }

        return TaskStatus.Success;
    }
}
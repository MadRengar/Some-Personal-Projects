using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class InitAIVariables : MonoBehaviour
{

    public BehaviorTree behaviorTree;
    public Transform player;

    void Start()
    {
        if (behaviorTree == null || player == null)
        {
            Debug.LogError("[InitAIVariables] 缺少 BehaviorTree 或 Player 引用！");
            return;
        }

        // 注入共享变量 "player"，对应 SharedTransform 类型变量
        behaviorTree.SetVariableValue("player", player);
    }
}

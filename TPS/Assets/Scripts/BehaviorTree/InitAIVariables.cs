using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class InitAIVariables : MonoBehaviour
{

    public BehaviorTree behaviorTree;

    void Start()
    {
        if (behaviorTree == null)
        {
            Debug.LogError("[InitAIVariables] 缺少 BehaviorTree 引用！");
            return;
        }
        // 注入共享变量 "player"，对应 SharedTransform 类型变量
        behaviorTree.SetVariableValue("player", GameManager.Instance.GetPlayerTransform());
    }
    void Update()
    {
        behaviorTree.SetVariableValue("pingCommandActive", GameManager.Instance.GetPingMarkerManager().pingCommandActive);
        behaviorTree.SetVariableValue("pingPosition", GameManager.Instance.GetPingMarkerManager().currentMarkedPosition);
    }
}

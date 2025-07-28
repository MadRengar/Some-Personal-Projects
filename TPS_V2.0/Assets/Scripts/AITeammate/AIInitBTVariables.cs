using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

/*
关于行为树中的变量声明和使用步骤：
1. 在“黑板”也就是Behavior Designer 中的 Variables 面板中申明变量。
2. 在下面通过SetVariableValueh函数赋值相关变量。
3. 在节点的脚本中申明对应类型的变量，然后在 Behavior Tree 编辑器中选中该节点
   通过Inspector窗口的小圆点来选择“黑板”中的变量。
 */
public class AIInitBTVariables : MonoBehaviour
{

    public BehaviorTree behaviorTree;
    public Transform ammoSupplyPos;
    public AmmoWorkbenchController ammoWorkbenchController;
    void Start()
    {
        if (behaviorTree == null)
        {
            Debug.LogError("[AIInitBTVariables] 缺少 BehaviorTree 引用！");
            return;
        }
        // Player Ref
        behaviorTree.SetVariableValue("player", GameManager.Instance.GetPlayerTransform());
        // 弹药补给点
        behaviorTree.SetVariableValue("ammoSupplyPos", ammoSupplyPos);
        
    }
    void Update()
    {
        // ping Ref
        behaviorTree.SetVariableValue("pingCommandActive", GameManager.Instance.GetPingMarkerManager().GetPingCommandActive());
        behaviorTree.SetVariableValue("pingPosition", GameManager.Instance.GetPingMarkerManager().GetCurrentMarkedPosition());
        behaviorTree.SetVariableValue("isTargetBuilding", GameManager.Instance.GetPingMarkerManager().IsCurrentTargetBuilding());
        //behaviorTree.SetVariableValue("currentCommand", GameManager.Instance.currentCommand);

        behaviorTree.SetVariableValue("aiIsInsideAmmoSupply", ammoWorkbenchController.aiInRange);
 
    }
}

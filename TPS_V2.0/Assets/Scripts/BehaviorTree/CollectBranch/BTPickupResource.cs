using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTPickupResource : Action
{
    // 黑板变量：目标资源的Transform
    public SharedTransform nearestResource;
    private AIAgentSettings agentSettings;
    // 拾取范围，建议与停靠距离一致
    public float pickupRange = 2.5f;
    public override void OnStart()
    {
        agentSettings = GetComponent<AIAgentSettings>();
    }

    public override TaskStatus OnUpdate()
    {
        if (nearestResource.Value == null)
        {
            Debug.LogWarning("BTPickupResource: 没有目标资源！");
            return TaskStatus.Failure;
        }

        // 判断距离，AI是否到达资源
        float dist = Vector3.Distance(transform.position, nearestResource.Value.position);
        if (dist > pickupRange)
        {
            return TaskStatus.Failure; // 还未到达，建议回行为树上一节点“继续移动”
        }

        // 获取PickupItem组件
        var pickup = nearestResource.Value.GetComponent<PickupItem>();
        if (pickup == null)
        {
            Debug.LogWarning("BTPickupResource: 找不到PickupItem组件！");
            nearestResource.Value = null;
            return TaskStatus.Failure;
        }

        // 执行拾取，TryAdd自动判断背包重量、回收对象
        pickup.TryPickupByAI();


        nearestResource.Value = null;
        return TaskStatus.Failure;
    }
}

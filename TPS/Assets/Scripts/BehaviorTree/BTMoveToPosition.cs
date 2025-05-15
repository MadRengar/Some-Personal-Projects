using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class BTMoveToPosition : Action
{
    public SharedVector3 pingPosition;
    public SharedBool pingCommandActive;
    public SharedString currentCommand;

    private NavMeshAgent agent;
    private AIAgentSettings settings;
    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        settings = GetComponent<AIAgentSettings>();
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null)
        {
            Debug.LogError("FollowPlayer: AI 缺少 NavMeshAgent 组件！");
            return TaskStatus.Failure;
        }

        float dist = Vector3.Distance(agent.transform.position, pingPosition.Value);
        /*
         到达判断
        1.!agent.pathPending：NavMeshAgent 是否已经完成路径计算
        2.agent.remainingDistance <= agent.stoppingDistance：剩余距离是否小于停止距离
        （重大bug！ agent.remainingDistance导致if else中的代码频繁被交替执行）
         */
        if (dist >= agent.stoppingDistance)
        {
            Debug.Log("移动中！");
            if(pingCommandActive.Value) // 如果指令没有被玩家取消
            {
                agent.SetDestination(pingPosition.Value);
                return TaskStatus.Running;
            }
            else // 指令在agent移动中取消，停在原地
            {
                agent.ResetPath();
                currentCommand.Value = "";
                Debug.LogError("指令取消！");
                return TaskStatus.Failure;
            }
        }
        else // 到达目的地
        {
            Debug.Log("到达目的地！");
            agent.ResetPath(); // 停下来，不再持续 SetDestination
            currentCommand.Value = "";
            return TaskStatus.Success;
        }
    }
}

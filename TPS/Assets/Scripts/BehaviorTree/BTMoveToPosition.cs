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
         */
        bool arrived = !agent.pathPending &&
                       agent.remainingDistance <= agent.stoppingDistance &&
                       (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        if(arrived)
        {

            //settings.timeToFindPathCount = 0f;
            //if(agent.remainingDistance <= agent.stoppingDistance + settings.margin)
            //{
            //    if(settings.timeToFindPathCount < settings.timeToFindPath)
            //    {
            //        settings.timeToFindPathCount += Time.deltaTime;
            //        return TaskStatus.Running;
            //    }
            //    else
            //    {
            //        agent.ResetPath(); // 停下来，不再持续 SetDestination
            //        return TaskStatus.Success;
            //    }
            //}

            if(pingCommandActive.Value) // 如果指令没有被玩家取消
            {
                agent.SetDestination(pingPosition.Value);
                return TaskStatus.Running;
            }
            else // 指令在agent移动中取消，停在原地
            {
                agent.ResetPath();
                Debug.LogError("指令取消！");
                return TaskStatus.Failure;
            }
        }
        else // 到达目的地
        {
            agent.ResetPath(); // 停下来，不再持续 SetDestination
            return TaskStatus.Success;
        }
    }
}

using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTCheckNeedStorage : Conditional
{
    public SharedBool needStorage;

    public override TaskStatus OnUpdate()
    {
        if (needStorage.Value)
        {
            //Debug.Log("[BTCheckNeedStorage] 需要存储资源");
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }
    }
}
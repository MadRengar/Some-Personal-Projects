using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;


public class BTHasHealingCommand : Conditional
{
    [Header("Shared Variables")]
    public SharedString currentCommand; // µ±«∞÷∏¡Ó
    public SharedTransform treatmentPos;

    public override TaskStatus OnUpdate()
    {
        if (currentCommand.Value != "go_heal")
        {
            return TaskStatus.Failure;
        }
        if (treatmentPos.Value == null)
        {
            return TaskStatus.Failure;
        }
        return TaskStatus.Success;
    }
}

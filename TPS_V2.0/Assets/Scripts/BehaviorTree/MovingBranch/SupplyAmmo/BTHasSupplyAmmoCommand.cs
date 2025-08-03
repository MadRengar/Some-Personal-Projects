using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTHasSupplyAmmoCommand : Conditional
{
    [Header("Shared Variables")]
    public SharedString currentCommand; // µ±«∞÷∏¡Ó
    public SharedTransform ammoSupplyPos;

    public override TaskStatus OnUpdate()
    {
        if (currentCommand.Value != "replenish_ammo")
        {
            return TaskStatus.Failure;
        }
        if (ammoSupplyPos.Value == null)
        {
            return TaskStatus.Failure;
        }
        return TaskStatus.Success;
    }
}

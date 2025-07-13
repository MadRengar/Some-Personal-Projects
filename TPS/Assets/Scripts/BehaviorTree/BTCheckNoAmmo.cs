using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTCheckNoAmmo : Conditional
{
    public SharedBool outOfAmmo;

    private WeaponManager weaponManager;

    public override void OnStart()
    {
        weaponManager = GameManager.Instance.GetAIPlayerWeaponManager();
    }

    public override TaskStatus OnUpdate()
    {
        if (weaponManager == null) return TaskStatus.Failure;
        // 检查是否完全没子弹
        bool hasNoAmmo = weaponManager.GetCurrentAmmo() == 0 && weaponManager.GetReserveAmmo() == 0;

        outOfAmmo.Value = hasNoAmmo;

        return hasNoAmmo ? TaskStatus.Success : TaskStatus.Failure;
    }
}
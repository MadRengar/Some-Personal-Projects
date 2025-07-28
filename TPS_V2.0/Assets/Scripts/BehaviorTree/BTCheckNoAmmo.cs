using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTCheckNoAmmo : Conditional
{
    // 所有其他检查弹药的地方从这里读取
    public SharedBool outOfAmmo;
    public SharedBool hasAmmo;

    private WeaponManager weaponManager;

    public override void OnStart()
    {
        weaponManager = GameManager.Instance.GetAIPlayerWeaponManager();
    }

    public override TaskStatus OnUpdate()
    {
        if (weaponManager == null) return TaskStatus.Failure;
        // 检查是否完全没子弹
        bool currentlyOutOfAmmo = weaponManager.GetCurrentAmmo() == 0 && weaponManager.GetReserveAmmo() == 0;
        bool currentlyHasAmmo = !currentlyOutOfAmmo;

        // 更新两个标志位
        outOfAmmo.Value = currentlyOutOfAmmo;
        hasAmmo.Value = currentlyHasAmmo;

        return currentlyOutOfAmmo ? TaskStatus.Success : TaskStatus.Failure;
    }
}
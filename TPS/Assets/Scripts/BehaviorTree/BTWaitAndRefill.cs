using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class BTWaitAndRefill : Action
{
    public SharedBool outOfAmmo;

    private WeaponManager weaponManager;
    private static float timer = 0f; // 改为静态变量
    private static bool hasStarted = false; // 标记是否已经开始计时

    public override void OnStart()
    {
        weaponManager = GameManager.Instance.GetAIPlayerWeaponManager();
        if (!hasStarted)
        {
            timer = 0f;
            hasStarted = true;
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (weaponManager == null)
        {
            return TaskStatus.Failure;
        }

        timer += Time.deltaTime;
        Debug.Log("时间" + timer);

        if (timer >= 2f)
        {
            if (weaponManager.GetWeaponData() != null)
            {
                weaponManager.AddReserveAmmo(weaponManager.GetWeaponData().maxReserveAmmo);
            }

            outOfAmmo.Value = false;
            timer = 0f;
            hasStarted = false;
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }
}
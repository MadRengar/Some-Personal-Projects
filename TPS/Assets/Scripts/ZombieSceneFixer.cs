using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSceneFixer : MonoBehaviour
{

    /*辅助测试的代码
     * 修复：拖入场景的僵尸无法移动，FSM 正常
     * 原因：没有启用 Root Motion，并且又没有开启 NavMeshAgent.updatePosition = true，所以角色根本不会移动
     *      对象池管理的僵尸之所以能动，是因为之后通过 SyncWithNavMeshAgentRootMotion() 方法在 Update() 里手动同步了 transform。
     *      拖入场景的僵尸：Start() 方法未运行 FSM 初始化；
     *      所以没有执行 SyncWithNavMeshAgentRootMotion()；
     *      即便 FSM 状态被设置为 CHASE，也只会更新动画播放，而不会推动角色位移。
     */
    void Start()
    {
        ZombieStats stats = GetComponent<ZombieStats>();
        if (stats != null)
        {
            //Debug.Log("[ZombieSceneInit] 手动触发 ResetZombie()");
            stats.ResetZombie();
        }
    }
}

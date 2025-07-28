using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIRespawn : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint; // 重生位置（营地中的空游戏对象）
    public float respawnDelay = 1f; // 重生延迟时间

    [Header("Ref")]
    private AITeammateState aiState;
    private InventoryManager inventoryManager;
    private AIAnimationController animController;
    private WeaponManager weaponManager;

    private bool isDead = false;
    private bool hasRespawned = false; // 防止同一天重复重生

    void Start()
    {
        GetAIRespawnComponent();

        GameTimeManager.Instance.OnDawnStarted += OnDawnStarted;
        GameManager.OnAIPlayerDeath += OnAIPlayerDied;
    }

    public void OnAIPlayerDied()
    {
        isDead = true;
        hasRespawned = false; // 重置重生标志
        Debug.Log("AI队友死亡，等待黎明重生...");
    }

    public void OnDawnStarted()
    {
        if (isDead && !hasRespawned)
        {
            Debug.Log("黎明到来，开始重生AI队友...");
            StartCoroutine(RespawnAI());
        }
    }

    #region AI重生
    /// <summary>
    /// 重生AI队友协程
    /// </summary>
    private IEnumerator RespawnAI()
    {
        // 等待延迟时间
        yield return new WaitForSeconds(respawnDelay);

        // 1. 重置位置
        ResetPosition();

        // 2. 重置AI状态
        ResetAIState();

        // 3. 清空背包
        ClearInventory();

        // 4. 重置动画状态
        ResetAnimationState();

        // 5. 重置武器状态
        ResetWeaponState();

        // 6. 重置导航代理
        ResetNavMeshAgent();

        // 7. 清空当前指令
        ClearCurrentCommand();

        // 标记为已重生
        isDead = false;
        hasRespawned = true;

        Debug.Log("AI队友重生完成！新的幸存者已加入营地。");
    }

    private void ResetPosition()
    {
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
    }

    private void ResetAIState()
    {
        aiState.InitializeAIState();
    }

    private void ClearInventory()
    {
        inventoryManager.ClearAIInventory();
    }

    private void ResetAnimationState()
    {
        animController.SetDead(false);
        animController.InitializeRigWeights();
        animController.SetStateFlag(AIAnimationController.AIStateFlags.Moving, false);
        animController.SetStateFlag(AIAnimationController.AIStateFlags.Firing, false);
        animController.SetStateFlag(AIAnimationController.AIStateFlags.Reloading, false);
        animController.SetAlive();
    }

    private void ResetWeaponState()
    {
        weaponManager.InitializeWeapon();
    }

    private void ResetNavMeshAgent()
    {
        var agent = GetComponent<NavMeshAgent>();
        agent.isStopped = false;
        agent.velocity = Vector3.zero;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void ClearCurrentCommand()
    {
        GameManager.Instance.ClearAIBehaviorCommand();
    }
    #endregion

    private void OnDestroy()
    {
        GameTimeManager.Instance.OnDawnStarted -= OnDawnStarted;
        GameManager.OnAIPlayerDeath -= OnAIPlayerDied;
    }

    private void GetAIRespawnComponent()
    {
        aiState = GetComponent<AITeammateState>();
        inventoryManager = GameManager.Instance.GetInventoryManager();
        animController = GetComponent<AIAnimationController>();
        weaponManager = GameManager.Instance.GetAIPlayerWeaponManager();
    }

}

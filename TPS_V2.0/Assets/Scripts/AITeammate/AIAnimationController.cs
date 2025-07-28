using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AIAnimationController : MonoBehaviour
{
    [System.Serializable]
    public class AnimationState
    {
        public string name;
        public Rig[] rigs;
        [Range(0f, 1f)]
        public float rigWeight = 1f;
        [HideInInspector]
        public float currentRigWeight = 0f;
    }

    [Header("Animation System")]
    public Animator animator;

    [Header("Weapon Manager Reference")]
    [SerializeField] private WeaponManager weaponManager;

    [Header("Animation States")]
    public AnimationState IdleState = new AnimationState { name = "Idle" };
    public AnimationState firingState = new AnimationState { name = "Firing" };
    public AnimationState deathState = new AnimationState { name = "Death" };

    [Header("Animator Parameters")]
    public string isFiringParameter = "IsFiring";
    public string isReloadingParameter = "IsReloading";
    public string isMovingParameter = "IsMoving";
    //public string moveDirectionParameter = "MoveDirection";
    public string speedParameter = "Speed";
    public string isAliveParameter = "IsAlive";
    public string isRepairingParameter = "HammerSwing";

    [Header("AI Weapon Objects")]
    public GameObject aiRifleObject;  // AI步枪对象
    public GameObject aiHammerObject; // AI锤子对象

    // 统一的状态管理系统
    [System.Flags]
    public enum AIStateFlags
    {
        None = 0,
        Moving = 1,        // 正在移动
        Firing = 2,        // 正在射击
        Reloading = 4,      // 正在换弹
        Dead = 8,            // ai死亡
        Repairing = 16     // 正在维修
    }

    [Header("Current State")]
    [SerializeField] private AIStateFlags currentStateFlags = AIStateFlags.None;

    void Start()
    {
        animator = GetComponent<Animator>();

        FindAndSubscribeToWeaponManager();
        InitializeRigWeights();
        InitializeWeapons();
    }

    void Update()
    {
        HandleReloadAnimation();
        UpdateRigWeights();
        UpdateAnimatorParameters();
        UpdateRepairLayer();
    }

    #region 弹药检查逻辑

    /// <summary>
    /// 检查是否有弹药
    /// </summary>
    private bool HasAmmo()
    {
        if (weaponManager == null) return false;

        return weaponManager.GetCurrentAmmo() > 0 || weaponManager.GetReserveAmmo() > 0;
    }

    /// <summary>
    /// 检查是否完全没有弹药
    /// </summary>
    private bool IsOutOfAmmo()
    {
        if (weaponManager == null) return true;

        return weaponManager.GetCurrentAmmo() == 0 && weaponManager.GetReserveAmmo() == 0;
    }

    #endregion


    #region 状态管理

    public void SetStateFlag(AIStateFlags flag, bool value)
    {
        if (value)
        {
            currentStateFlags |= flag;
        }
        else
        {
            currentStateFlags &= ~flag;
        }

        //Debug.Log($"AI状态更新: {currentStateFlags}");
    }

    public bool HasStateFlag(AIStateFlags flag)
    {
        return (currentStateFlags & flag) != 0;
    }

    public AIStateFlags GetCurrentStateFlags()
    {
        return currentStateFlags;
    }

    #endregion

    #region 死亡处理
    public void SetDead(bool isDead)
    {
        SetStateFlag(AIStateFlags.Dead, isDead);

        if (isDead)
        {
            // 死亡时清除所有其他状态
            SetStateFlag(AIStateFlags.Moving, false);
            SetStateFlag(AIStateFlags.Firing, false);
            SetStateFlag(AIStateFlags.Reloading, false);
            SetStateFlag(AIStateFlags.Repairing, false);

            animator.SetFloat(speedParameter, 0f);
            animator.SetBool(isMovingParameter, false);
            animator.SetBool(isFiringParameter, false);
            animator.SetBool(isReloadingParameter, false);
            animator.SetBool(isAliveParameter, false);
        }
    }

    public bool IsDead()
    {
        return HasStateFlag(AIStateFlags.Dead);
    }

    #endregion


    #region 公共接口

    public void SetMoving(bool moving, float speed)
    {
        SetStateFlag(AIStateFlags.Moving, moving);
        animator.SetFloat(speedParameter, speed);
    }

    //public void SetMoveDirection(float direction)
    //{
    //    if (animator != null)
    //    {
    //        animator.SetFloat(moveDirectionParameter, direction);
    //    }
    //}

    public void SetFiring(bool firing)
    {
        // 没有弹药时不能开火
        if (firing && IsOutOfAmmo())
        {
            Debug.Log("AI没有弹药，无法开火！");
            return;
        }

        // 换弹时不能射击
        if (firing && HasStateFlag(AIStateFlags.Reloading))
        {
            return;
        }

        // 维修时不能射击
        if (firing && HasStateFlag(AIStateFlags.Repairing))
        {
            return;
        }

        SetStateFlag(AIStateFlags.Firing, firing);
    }

    public void SetReloading(bool reloading)
    {
        SetStateFlag(AIStateFlags.Reloading, reloading);

        if (reloading)
        {
            // 开始换弹时停止射击
            SetStateFlag(AIStateFlags.Firing, false);
            SetStateFlag(AIStateFlags.Repairing, false);
        }
    }

    public void SetRepairing(bool repairing)
    {
        SetStateFlag(AIStateFlags.Repairing, repairing);

        if (repairing)
        {
            // 开始维修时停止射击和换弹
            SetStateFlag(AIStateFlags.Firing, false);
            SetStateFlag(AIStateFlags.Reloading, false);

            // 切换到锤子
            SwitchToHammer();
        }
        else
        {
            // 停止维修时切换回步枪
            SwitchToRifle();
        }

        Debug.Log($"AI维修状态: {repairing}");
    }

    // Only use for setting AIReSpawn
    public void SetAlive()
    {
        animator.SetBool(isAliveParameter, true);
    }

    #endregion

    #region AI武器切换
    private void InitializeWeapons()
    {
        // 初始化时显示步枪，隐藏锤子
        if (aiRifleObject != null)
            aiRifleObject.SetActive(true);

        if (aiHammerObject != null)
            aiHammerObject.SetActive(false);
    }

    private void SwitchToHammer()
    {
        if (aiRifleObject != null)
            aiRifleObject.SetActive(false);

        if (aiHammerObject != null)
            aiHammerObject.SetActive(true);

        //Debug.Log("AI切换到锤子");
    }

    private void SwitchToRifle()
    {
        if (aiRifleObject != null)
            aiRifleObject.SetActive(true);

        if (aiHammerObject != null)
            aiHammerObject.SetActive(false);

        //Debug.Log("AI切换到步枪");
    }
    #endregion


    #region 维修层控制
    private void UpdateRepairLayer()
    {
        bool shouldRepair = HasStateFlag(AIStateFlags.Repairing) && !HasStateFlag(AIStateFlags.Dead);

        if (shouldRepair)
        {
            // 维修时：启用Repair Layer，禁用战斗相关层

            animator.SetLayerWeight(5, 1f); // IdleWithHammer Layer

            animator.SetLayerWeight(4, 1f); // Repair Layer

            animator.SetLayerWeight(3, 0f); // Firing Layer

            animator.SetLayerWeight(2, 0f); // Reloading Layer

            animator.SetLayerWeight(1, 0f); // IdleWithRifle Layer
        }
        else
        {
            // 非维修时：恢复正常层权重
            animator.SetLayerWeight(5, 0f); // IdleWithHammer Layer

            animator.SetLayerWeight(4, 0f); // Repair Layer

            animator.SetLayerWeight(3, 1f); // Firing Layer

            animator.SetLayerWeight(2, 1f); // Reloading Layer

            animator.SetLayerWeight(1, 1f); // Rifle Idle Layer
        }
    }

    public void TriggerHammerSwing()
    {
        animator.SetTrigger(isRepairingParameter); // 使用HammerSwing trigger
        Debug.Log("AI触发锤子挥击动画");
    }
    #endregion

    #region 动画参数更新

    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        // 检查弹药状态
        bool canFire = HasAmmo() && !HasStateFlag(AIStateFlags.Reloading) && !HasStateFlag(AIStateFlags.Dead);
        bool shouldFire = HasStateFlag(AIStateFlags.Firing) && canFire;

        animator.SetBool(isMovingParameter, HasStateFlag(AIStateFlags.Moving));
        animator.SetBool(isFiringParameter, shouldFire);
        animator.SetBool(isReloadingParameter, HasStateFlag(AIStateFlags.Reloading));
    }

    #endregion

    #region Rig权重管理

    private void UpdateRigWeights()
    {
        // 维修时清除所有Rig，因为维修动画由单独的层控制，不需要Rig
        if (HasStateFlag(AIStateFlags.Repairing))
        {
            UpdateStateRigWeights(IdleState, 0f);
            UpdateStateRigWeights(firingState, 0f);
            return; // 维修时直接返回，不处理其他状态
        }

        // 检查是否可以开火
        bool canFire = HasAmmo() && !HasStateFlag(AIStateFlags.Reloading) && !HasStateFlag(AIStateFlags.Dead);
        bool shouldUseFiringRig = HasStateFlag(AIStateFlags.Firing) && canFire;

        if (shouldUseFiringRig)
        {
            UpdateStateRigWeights(IdleState, 0f);
            UpdateStateRigWeights(firingState, 1f);
        }
        else
        {
            UpdateStateRigWeights(IdleState, 1f);
            UpdateStateRigWeights(firingState, 0f);
        }

        // 换弹时清除所有Rig
        if (HasStateFlag(AIStateFlags.Reloading))
        {
            UpdateStateRigWeights(IdleState, 0f);
            UpdateStateRigWeights(firingState, 0f);
        }

        if (HasStateFlag(AIStateFlags.Dead))
        {
            UpdateStateRigWeights(IdleState, 0f);
            UpdateStateRigWeights(firingState, 0f);
        }
    }

    private void UpdateStateRigWeights(AnimationState state, float targetWeight)
    {
        state.currentRigWeight = targetWeight * state.rigWeight;

        foreach (var rig in state.rigs)
        {
            if (rig != null)
                rig.weight = state.currentRigWeight;
        }
    }

    public void InitializeRigWeights()
    {
        SetRigWeights(IdleState, 1f);
        SetRigWeights(firingState, 0f);
    }

    private void SetRigWeights(AnimationState state, float weight)
    {
        state.currentRigWeight = weight;
        foreach (var rig in state.rigs)
        {
            if (rig != null)
                rig.weight = weight;
        }
    }

    #endregion

    #region 武器管理

    private void FindAndSubscribeToWeaponManager()
    {
        if (weaponManager != null)
        {
            weaponManager.OnReloadStateChanged += OnWeaponReloadStateChanged;
        }
        else
        {
            Debug.LogWarning("AIAnimationController: 未找到WeaponManager");
        }
    }

    private void OnWeaponReloadStateChanged(bool isReloadingNow)
    {
        SetReloading(isReloadingNow);
    }

    private void HandleReloadAnimation()
    {
        bool weaponIsReloading = weaponManager != null && weaponManager.IsReloading();
        bool currentlyReloading = HasStateFlag(AIStateFlags.Reloading);

        // 只在状态变化时更新
        if (weaponIsReloading != currentlyReloading)
        {
            SetReloading(weaponIsReloading);
        }
    }

    private void OnDestroy()
    {
        if (weaponManager != null)
        {
            weaponManager.OnReloadStateChanged -= OnWeaponReloadStateChanged;
        }
    }

    #endregion
}
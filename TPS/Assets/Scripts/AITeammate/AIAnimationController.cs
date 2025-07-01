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
        public string animatorStateName;
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
    public AnimationState IdleState = new AnimationState
    {
        name = "Idle",
        animatorStateName = "Idle"
    };

    public AnimationState aimingState = new AnimationState
    {
        name = "Aiming",
        animatorStateName = "Aiming"
    };

    public AnimationState firingState = new AnimationState
    {
        name = "Firing",
        animatorStateName = "Firing"
    };

    [Header("Transition Settings")]
    public float rigTransitionSpeed = 10f;
    public float animationTransitionSpeed = 0.2f;

    [Header("Animator Parameters")]
    public string isFiringParameter = "IsFiring";
    public string isAimingParameter = "IsAiming";
    public string isReloadingParameter = "IsReloading";
    public string isMovingParameter = "IsMoving"; // 新增移动参数

    // 新的状态标志位系统
    [System.Flags]
    public enum AIStateFlags
    {
        None = 0,
        Moving = 1,
        Aiming = 2,
        Firing = 4,
        Reloading = 8
    }

    [Header("Multi-State Support")]
    [SerializeField] private AIStateFlags currentStateFlags = AIStateFlags.None;

    // 保留旧的状态枚举用于兼容
    public enum AIState
    {
        Idle,
        Aiming,
        Firing,
        Reloading
    }

    [Header("Current State (Legacy - for compatibility)")]
    [SerializeField] private AIState currentState = AIState.Idle;

    [Header("Reload Settings")]
    public bool isReloading = false;

    private AIState stateBeforeReload = AIState.Idle;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        FindAndSubscribeToWeaponManager();
        InitializeRigWeights();

        // 初始状态设置
        SetStateFlag(AIStateFlags.None, true);
    }

    void Update()
    {
        HandleReloadAnimation();
        UpdateRigWeights();
        UpdateAnimatorParameters(); // 每帧更新动画参数
    }

    #region 新的状态管理系统

    /// <summary>
    /// 设置状态标志位
    /// </summary>
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

        Debug.Log($"AI状态更新: {currentStateFlags}");
    }

    /// <summary>
    /// 检查是否有指定状态标志
    /// </summary>
    public bool HasStateFlag(AIStateFlags flag)
    {
        return (currentStateFlags & flag) != 0;
    }

    /// <summary>
    /// 获取当前所有状态标志
    /// </summary>
    public AIStateFlags GetCurrentStateFlags()
    {
        return currentStateFlags;
    }

    /// <summary>
    /// 更新动画参数 - 基于状态标志位
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        // 根据状态标志位设置动画参数
        animator.SetBool(isMovingParameter, HasStateFlag(AIStateFlags.Moving));
        animator.SetBool(isAimingParameter, HasStateFlag(AIStateFlags.Aiming));
        animator.SetBool(isFiringParameter, HasStateFlag(AIStateFlags.Firing));
        animator.SetBool(isReloadingParameter, HasStateFlag(AIStateFlags.Reloading));
    }

    #endregion

    #region 兼容旧系统的方法 (保留以防需要)

    public void ChangeState(AIState newState)
    {
        if (currentState == newState) return;

        Debug.Log($"AI状态切换 (Legacy): {currentState} -> {newState}");
        currentState = newState;

        // 根据旧状态设置新的标志位
        switch (newState)
        {
            case AIState.Idle:
                SetStateFlag(AIStateFlags.Aiming | AIStateFlags.Firing, false);
                break;
            case AIState.Aiming:
                SetStateFlag(AIStateFlags.Aiming, true);
                SetStateFlag(AIStateFlags.Firing, false);
                break;
            case AIState.Firing:
                SetStateFlag(AIStateFlags.Aiming | AIStateFlags.Firing, true);
                break;
            case AIState.Reloading:
                SetStateFlag(AIStateFlags.Reloading, true);
                SetStateFlag(AIStateFlags.Aiming | AIStateFlags.Firing, false);
                break;
        }
    }

    #endregion

    #region 公共接口方法 - 使用新的状态系统

    public void SetMoving(bool moving)
    {
        SetStateFlag(AIStateFlags.Moving, moving);
    }

    public void SetAiming(bool aiming)
    {
        SetStateFlag(AIStateFlags.Aiming, aiming);
    }

    public void SetFiring(bool firing)
    {
        SetStateFlag(AIStateFlags.Firing, firing);
    }

    public void SetReloading(bool reloading)
    {
        SetStateFlag(AIStateFlags.Reloading, reloading);
        isReloading = reloading;
    }

    // 保留旧的接口用于兼容
    public void SetIdle() => ChangeState(AIState.Idle);
    public void OnStartAiming() => SetAiming(true);
    public void OnStartFiring() => SetFiring(true);
    public void OnStopFiring() => SetFiring(false);
    public void OnLostEnemyTarget()
    {
        SetAiming(false);
        SetFiring(false);
    }

    #endregion

    #region 原有的逻辑保持不变

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
        if (isReloadingNow)
        {
            if (!HasStateFlag(AIStateFlags.Reloading))
            {
                stateBeforeReload = currentState;
                Debug.Log($"换弹开始，记录换弹前状态: {stateBeforeReload}");
            }
        }
        else
        {
            Debug.Log($"换弹结束，考虑恢复到状态: {stateBeforeReload}");
            QuickRestoreFromReload();
        }
    }

    private void QuickRestoreFromReload()
    {
        SetReloading(false);
        ChangeState(stateBeforeReload);
    }

    private void HandleReloadAnimation()
    {
        bool isReloading = weaponManager != null && weaponManager.IsReloading();
        if (isReloading)
        {
            SetReloading(true);
            Debug.Log("AI正在在换弹！！！！！！！！！");
        }
    }

    private void InitializeRigWeights()
    {
        SetRigWeights(IdleState, 0f);
        SetRigWeights(aimingState, 0f);
        SetRigWeights(firingState, 0f);
    }

    private void UpdateRigWeights()
    {
        // 根据状态标志位更新Rig权重
        if (HasStateFlag(AIStateFlags.Firing))
        {
            UpdateStateRigWeights(IdleState, 0f);
            UpdateStateRigWeights(aimingState, 0f);
            UpdateStateRigWeights(firingState, 1f);
        }
        else if (HasStateFlag(AIStateFlags.Aiming))
        {
            UpdateStateRigWeights(IdleState, 0f);
            UpdateStateRigWeights(aimingState, 1f);
            UpdateStateRigWeights(firingState, 0f);
        }
        else
        {
            UpdateStateRigWeights(IdleState, 1f);
            UpdateStateRigWeights(aimingState, 0f);
            UpdateStateRigWeights(firingState, 0f);
        }

        // 换弹时清空所有Rig
        if (HasStateFlag(AIStateFlags.Reloading))
        {
            UpdateStateRigWeights(IdleState, 0f);
            UpdateStateRigWeights(aimingState, 0f);
            UpdateStateRigWeights(firingState, 0f);
        }
    }

    private void UpdateStateRigWeights(AnimationState state, float targetWeight)
    {
        state.currentRigWeight = Mathf.Lerp(state.currentRigWeight,
                                           targetWeight * state.rigWeight,
                                           Time.deltaTime * rigTransitionSpeed);

        foreach (var rig in state.rigs)
        {
            if (rig != null)
                rig.weight = state.currentRigWeight;
        }
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

    private void OnDestroy()
    {
        if (weaponManager != null)
        {
            weaponManager.OnReloadStateChanged -= OnWeaponReloadStateChanged;
        }
    }

    // 状态查询方法
    public AIState GetCurrentState() => currentState;
    public bool IsInState(AIState state) => currentState == state;
    public bool IsReloading() => HasStateFlag(AIStateFlags.Reloading) || isReloading;

    #endregion
}
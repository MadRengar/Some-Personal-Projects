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

    // AI状态枚举
    public enum AIState
    {
        Idle,
        Aiming,
        Firing,
        Reloading
    }

    [Header("Current State")]
    [SerializeField] private AIState currentState = AIState.Idle;
    [SerializeField] private AIState targetState = AIState.Idle;

    [Header("Reload Settings")]
    public bool isReloading = false;

    private AIState stateBeforeReload = AIState.Idle; // 记录换弹前的状态
    // Start is called before the first frame update
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // 查找并订阅WeaponManager事件
        FindAndSubscribeToWeaponManager();

        // 初始化所有Rig权重
        InitializeRigWeights();

        // 设置初始状态
        ChangeState(AIState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        HandleReloadAnimation();
        UpdateRigWeights();      
    }
    private void FindAndSubscribeToWeaponManager()
    {
        if (weaponManager != null)
        {
            // 订阅换弹状态变化事件
            weaponManager.OnReloadStateChanged += OnWeaponReloadStateChanged;
            Debug.Log("AI成功订阅WeaponManager换弹事件");
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
            // 换弹开始：记录当前状态
            if (currentState != AIState.Reloading)
            {
                stateBeforeReload = currentState;
                Debug.Log($"换弹开始，记录换弹前状态: {stateBeforeReload}");
            }
        }
        else
        {
            // 换弹结束：快速恢复到换弹前的状态
            Debug.Log($"换弹结束，快速恢复到状态: {stateBeforeReload}");
            QuickRestoreFromReload();
        }
    }

    private void QuickRestoreFromReload()
    {
        isReloading = false;

        // 立即切换回换弹前的状态
        ChangeState(stateBeforeReload);
    }


    private void HandleReloadAnimation()
    {
        bool isReloading = weaponManager != null && weaponManager.IsReloading();
        if (isReloading)
        {
            SetReloading();
            isReloading = true;
            Debug.Log("AI队友在换弹！！！！！！！！");
        }
    }

    /* 初始化所有Rig权重为0 */
    private void InitializeRigWeights()
    {
        SetRigWeights(IdleState, 0f);
        SetRigWeights(aimingState, 0f);
        SetRigWeights(firingState, 0f);
    }

    /* 改变AI状态 */
    public void ChangeState(AIState newState)
    {
        if (targetState == newState) return;

        //Debug.Log($"AI状态切换: {currentState} -> {newState}");
        targetState = newState;

        // 设置动画参数
        SetAnimatorParameters(newState);
    }

    /// <summary>
    /// 设置动画参数
    /// </summary>
    private void SetAnimatorParameters(AIState state)
    {
        if (animator == null) return;

        // 重置所有布尔参数
        animator.SetBool(isFiringParameter, false);
        animator.SetBool(isAimingParameter, false);
        animator.SetBool(isReloadingParameter, false);

        // 根据状态设置参数
        switch (state)
        {
            case AIState.Idle:
                break;
            case AIState.Aiming:
                animator.SetBool(isAimingParameter, true);
                break;
            case AIState.Firing:
                animator.SetBool(isFiringParameter, true);
                animator.SetBool(isAimingParameter, true);
                break;
            case AIState.Reloading:
                animator.SetBool(isReloadingParameter, true);
                break;
        }
    }

    /// <summary>
    /// 更新Rig权重
    /// </summary>
    private void UpdateRigWeights()
    {
        // 根据目标状态设置Rig权重
        switch (targetState)
        {
            case AIState.Idle:
                UpdateStateRigWeights(IdleState, 1f);
                UpdateStateRigWeights(aimingState, 0f);
                UpdateStateRigWeights(firingState, 0f);
                break;

            case AIState.Aiming:
                UpdateStateRigWeights(IdleState, 0f);
                UpdateStateRigWeights(aimingState, 1f);
                UpdateStateRigWeights(firingState, 0f);
                break;

            case AIState.Firing:
                UpdateStateRigWeights(IdleState, 0f);
                UpdateStateRigWeights(aimingState, 0f);
                UpdateStateRigWeights(firingState, 1f);
                break;
            case AIState.Reloading:
                UpdateStateRigWeights(IdleState, 0f);
                UpdateStateRigWeights(aimingState, 0f);
                UpdateStateRigWeights(firingState, 0f);
                break;
        }

        // 检查是否完成过渡
        if (IsRigTransitionComplete())
        {
            currentState = targetState;
        }
    }

    /// <summary>
    /// 更新单个状态的Rig权重
    /// </summary>
    private void UpdateStateRigWeights(AnimationState state, float targetWeight)
    {
        state.currentRigWeight = Mathf.Lerp(state.currentRigWeight,
                                           targetWeight * state.rigWeight,
                                           Time.deltaTime * rigTransitionSpeed);

        // 应用到所有Rig
        foreach (var rig in state.rigs)
        {
            if (rig != null)
                rig.weight = state.currentRigWeight;
        }
    }

    /// <summary>
    /// 设置Rig权重（立即）
    /// </summary>
    private void SetRigWeights(AnimationState state, float weight)
    {
        state.currentRigWeight = weight;
        foreach (var rig in state.rigs)
        {
            if (rig != null)
                rig.weight = weight;
        }
    }

    /// <summary>
    /// 检查Rig过渡是否完成
    /// </summary>
    private bool IsRigTransitionComplete()
    {
        float threshold = 0.01f;

        switch (targetState)
        {
            case AIState.Idle:
                return Mathf.Abs(IdleState.currentRigWeight - IdleState.rigWeight) < threshold;

            case AIState.Aiming:
                return Mathf.Abs(aimingState.currentRigWeight - aimingState.rigWeight) < threshold;

            case AIState.Firing:
                return Mathf.Abs(firingState.currentRigWeight - firingState.rigWeight) < threshold;
            default:
                return true;
        }
    }

    private void OnDestroy()
    {
        if (weaponManager != null)
        {
            weaponManager.OnReloadStateChanged -= OnWeaponReloadStateChanged;
        }
    }


    // 公共接口方法
    public void SetIdle() => ChangeState(AIState.Idle);
    public void SetAiming() => ChangeState(AIState.Aiming);
    public void SetFiring() => ChangeState(AIState.Firing);
    public void SetReloading() => ChangeState(AIState.Reloading);

    // 状态查询
    public AIState GetCurrentState() => currentState;
    public AIState GetTargetState() => targetState;
    public bool IsInState(AIState state) => currentState == state;
    public bool IsTransitioningTo(AIState state) => targetState == state;

    // 特殊方法：用于行为树
    public void OnLostEnemyTarget() => SetIdle();
    public void OnStartAiming() => SetAiming();
    public void OnStartFiring() => SetFiring();
    public void OnStopFiring() => SetIdle();
    public void OnStartReloading()
    {
        isReloading = true;
        SetReloading();
    }
    public void OnStopReloading()
    {
        isReloading = false;
        SetIdle();
    }

    // 新增：供WeaponManager调用的换弹控制方法
    public void StartReload()
    {
        if (!isReloading)
        {
            isReloading = true;
            Debug.Log("AI开始换弹动画");
        }
    }

    public void StopReload()
    {
        if (isReloading)
        {
            isReloading = false;
            Debug.Log("AI换弹动画结束");
        }
    }

    // 检查是否正在换弹
    public bool IsReloading()
    {
        return isReloading || currentState == AIState.Reloading;
    }
}

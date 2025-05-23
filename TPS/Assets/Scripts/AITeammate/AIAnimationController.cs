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

    [Header("Animation States")]
    public AnimationState combatIdleState = new AnimationState
    {
        name = "Combat Idle",
        animatorStateName = "Combat Idle"
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
    public float rigTransitionSpeed = 5f;
    public float animationTransitionSpeed = 0.2f;

    [Header("Animator Parameters")]
    public string speedParameter = "Speed";
    public string isFiringParameter = "IsFiring";
    public string isAimingParameter = "IsAiming";

    // AI状态枚举
    public enum AIState
    {
        Idle,
        CombatIdle,
        Aiming,
        Firing
    }

    [Header("Current State")]
    [SerializeField] private AIState currentState = AIState.Idle;
    [SerializeField] private AIState targetState = AIState.Idle;

    // 移动相关
    private float currentSpeed = 0f;
    private Vector3 lastPosition;
    // Start is called before the first frame update
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        lastPosition = transform.position;

        // 初始化所有Rig权重
        InitializeRigWeights();

        // 设置初始状态
        ChangeState(AIState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateMovementSpeed();
        UpdateRigWeights();
        //UpdateAnimatorParameters();
    }

    /* 初始化所有Rig权重为0 */
    private void InitializeRigWeights()
    {
        SetRigWeights(combatIdleState, 0f);
        SetRigWeights(aimingState, 0f);
        SetRigWeights(firingState, 0f);
    }

    /* 改变AI状态 */
    public void ChangeState(AIState newState)
    {
        if (targetState == newState) return;

        Debug.Log($"AI状态切换: {currentState} -> {newState}");
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

        // 根据状态设置参数
        switch (state)
        {
            case AIState.Idle:
                // Idle状态只需要Speed参数
                break;

            case AIState.CombatIdle:
                // Combat Idle状态
                break;

            case AIState.Aiming:
                animator.SetBool(isAimingParameter, true);
                break;

            case AIState.Firing:
                animator.SetBool(isFiringParameter, true);
                animator.SetBool(isAimingParameter, true);
                break;
        }
    }

    /// <summary>
    /// 更新移动速度
    /// </summary>
    private void UpdateMovementSpeed()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        currentSpeed = Mathf.Lerp(currentSpeed, velocity.magnitude, Time.deltaTime * 10f);
        lastPosition = transform.position;
    }

    /// <summary>
    /// 更新动画参数
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        animator.SetFloat(speedParameter, currentSpeed);
    }

    /// <summary>
    /// 更新Rig权重
    /// </summary>
    private void UpdateRigWeights()
    {
        // 根据目标状态设置Rig权重
        switch (targetState)
        {
            case AIState.CombatIdle:
                UpdateStateRigWeights(combatIdleState, 1f);
                UpdateStateRigWeights(aimingState, 0f);
                UpdateStateRigWeights(firingState, 0f);
                break;

            case AIState.Aiming:
                UpdateStateRigWeights(combatIdleState, 0.3f); // 保持一些基础约束
                UpdateStateRigWeights(aimingState, 1f);
                UpdateStateRigWeights(firingState, 0f);
                break;

            case AIState.Firing:
                UpdateStateRigWeights(combatIdleState, 0.2f); // 保持一些基础约束
                UpdateStateRigWeights(aimingState, 0.8f);     // 保持瞄准约束
                UpdateStateRigWeights(firingState, 1f);
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
            case AIState.CombatIdle:
                return Mathf.Abs(combatIdleState.currentRigWeight - combatIdleState.rigWeight) < threshold;

            case AIState.Aiming:
                return Mathf.Abs(aimingState.currentRigWeight - aimingState.rigWeight) < threshold;

            case AIState.Firing:
                return Mathf.Abs(firingState.currentRigWeight - firingState.rigWeight) < threshold;

            default:
                return true;
        }
    }

    // 公共接口方法
    public void SetCombatIdle() => ChangeState(AIState.CombatIdle);
    public void SetAiming() => ChangeState(AIState.Aiming);
    public void SetFiring() => ChangeState(AIState.Firing);

    // 状态查询
    public AIState GetCurrentState() => currentState;
    public AIState GetTargetState() => targetState;
    public bool IsInState(AIState state) => currentState == state;
    public bool IsTransitioningTo(AIState state) => targetState == state;

    // 特殊方法：用于行为树
    public void OnEnemyDetected() => SetCombatIdle();
    public void OnStartAiming() => SetAiming();
    public void OnStartFiring() => SetFiring();
    public void OnStopFiring() => SetCombatIdle();
}

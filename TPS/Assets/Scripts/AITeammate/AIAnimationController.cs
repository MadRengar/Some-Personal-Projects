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

    // 统一的状态管理系统
    [System.Flags]
    public enum AIStateFlags
    {
        None = 0,
        Moving = 1,        // 正在移动
        Firing = 2,        // 正在射击
        Reloading = 4,      // 正在换弹
        Dead = 8            // ai死亡
    }

    [Header("Current State")]
    [SerializeField] private AIStateFlags currentStateFlags = AIStateFlags.None;

    void Start()
    {
        animator = GetComponent<Animator>();

        FindAndSubscribeToWeaponManager();
        InitializeRigWeights();
    }

    void Update()
    {
        HandleReloadAnimation();
        UpdateRigWeights();
        UpdateAnimatorParameters();
    }

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

        Debug.Log($"AI状态更新: {currentStateFlags}");
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
        // 换弹时不能射击
        if (firing && HasStateFlag(AIStateFlags.Reloading))
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
        }
    }

    #endregion

    #region 动画参数更新

    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;
        animator.SetBool(isMovingParameter, HasStateFlag(AIStateFlags.Moving));
        animator.SetBool(isFiringParameter, HasStateFlag(AIStateFlags.Firing));
        animator.SetBool(isReloadingParameter, HasStateFlag(AIStateFlags.Reloading));
    }

    #endregion

    #region Rig权重管理

    private void UpdateRigWeights()
    {
        if (HasStateFlag(AIStateFlags.Firing))
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

    private void InitializeRigWeights()
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
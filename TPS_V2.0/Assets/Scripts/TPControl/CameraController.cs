using Cinemachine;
using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Ref")]
    [SerializeField] private CinemachineVirtualCamera _aimVirtualCamera;
    // 死亡摄像机设置
    [Header("Death Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera _deathVirtualCamera; // 死亡摄像机
    [SerializeField] private float deathTransitionDuration = 3f; // 过渡时间
    [SerializeField] private Vector3 deathFinalOffset = new Vector3(0, 10, 0); // 最终偏移
    [SerializeField] private float deathFinalFOV = 45f; // 最终视野
    [SerializeField] private AnimationCurve deathTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    /*Player Death Camera*/
    private Vector3 _deathInitialOffset = new Vector3(0, 3, 0);        // 备份的初始Follow Offset
    private float _deathInitialFOV = 35f;             // 备份的初始FOV
    private int _deathInitialPriority = 1;          // 备份的初始Priority
    private bool _isDeathSequenceActive = false; // 死亡序列是否激活
    private CinemachineTransposer _deathTransposer; // 死亡摄像机的Transposer组件引用

    private void Start()
    {
        InitializeDeathCamera();
    }
    // 初始化死亡摄像机
    public void InitializeDeathCamera()
    {
        if (_deathVirtualCamera != null)
        {
            // 获取Transposer组件引用
            _deathTransposer = _deathVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();

            // 应用初始设置
            ApplyDeathCameraSettings(_deathInitialOffset, _deathInitialFOV, _deathInitialPriority);
        }
        else
        {
            Debug.LogWarning("死亡摄像机未分配！请在Inspector中设置Death Virtual Camera");
        }
    }

    public void ApplyDeathCameraSettings(Vector3 offset, float fov, int priority)
    {
        if (_deathVirtualCamera == null) return;

        // 设置Priority
        _deathVirtualCamera.Priority = priority;

        // 设置FOV
        _deathVirtualCamera.m_Lens.FieldOfView = fov;

        // 设置Follow Offset
        if (_deathTransposer != null)
        {
            _deathTransposer.m_FollowOffset = offset;
        }
    }

    public void StartDeathCameraSequence()
    {
        if (_deathVirtualCamera != null && !_isDeathSequenceActive)
        {
            _isDeathSequenceActive = true;
            StartCoroutine(DeathCameraTransition());
        }
    }

    // 死亡摄像机过渡协程
    private IEnumerator DeathCameraTransition()
    {
        // 1. 立即切换到死亡摄像机
        SwitchToDeathCamera();

        // 2. 等待一小段时间让玩家看到死亡动画
        yield return new WaitForSeconds(0.5f);

        // 3. 开始摄像机拉远过渡
        yield return StartCoroutine(AnimateDeathCamera());
    }

    // 切换到死亡摄像机
    public void SwitchToDeathCamera()
    {
        // 关闭瞄准摄像机
        if (_aimVirtualCamera != null)
            _aimVirtualCamera.Priority = 5;

        // 启用死亡摄像机（优先级最高）
        if (_deathVirtualCamera != null)
            _deathVirtualCamera.Priority = 15;
    }

    // 死亡摄像机动画
    private IEnumerator AnimateDeathCamera()
    {
        var transposer = _deathVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < deathTransitionDuration)
        {
            float t = elapsedTime / deathTransitionDuration;
            float curveValue = deathTransitionCurve.Evaluate(t);

            // 插值偏移位置
            Vector3 currentOffset = Vector3.Lerp(_deathInitialOffset, deathFinalOffset, curveValue);
            transposer.m_FollowOffset = currentOffset;

            // 插值视野
            float currentFOV = Mathf.Lerp(_deathInitialFOV, deathFinalFOV, curveValue);
            _deathVirtualCamera.m_Lens.FieldOfView = currentFOV;

            elapsedTime += Time.unscaledDeltaTime; // 使用unscaledDeltaTime因为可能时间被暂停
            yield return null;
        }

        // 确保最终值
        transposer.m_FollowOffset = deathFinalOffset;
        _deathVirtualCamera.m_Lens.FieldOfView = deathFinalFOV;
    }

    // 重置死亡摄像机
    public void ResetDeathCamera()
    {
        _isDeathSequenceActive = false;

        // 直接使用预设的初始值重置
        ApplyDeathCameraSettings(_deathInitialOffset, _deathInitialFOV, _deathInitialPriority);

        Debug.Log($"死亡摄像机已重置到预设状态: Offset={_deathInitialOffset}, FOV={_deathInitialFOV}, Priority={_deathInitialPriority}");
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RadioPopController : MonoBehaviour
{
    public static RadioPopController Instance { get; private set; }

    [Header("Ref")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject RadioTextObj;

    [Header("Animation Attribution")]
    [SerializeField] private float stayTime = 2f;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private TextMeshProUGUI typeText;

    [Header("Notification Colors")]
    [SerializeField] private Color commandColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color infoColor = Color.white;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 万能通知接口 - 通过参数控制消息类型
    /// </summary>
    public void ShowMessage(MessageKey messageKey, MessageType messageType)
    {
        string message = RadioMessages.Get(messageKey);

        switch (messageType)
        {
            case MessageType.Command:
                ShowNotification(message, "Command", commandColor);
                break;
            case MessageType.Warning:
                ShowNotification(message, "Warning", warningColor);
                break;
            case MessageType.Error:
                ShowNotification(message, "Error", errorColor);
                break;
            case MessageType.Info:
            default:
                ShowNotification(message, "Tip", infoColor);
                break;
        }
    }

    public void ShowMessage(string message, MessageType messageType)
    {
        switch (messageType)
        {
            case MessageType.Command:
                ShowNotification(message, "Command", commandColor);
                break;
            case MessageType.Warning:
                ShowNotification(message, "Warning", warningColor);
                break;
            case MessageType.Error:
                ShowNotification(message, "Error", errorColor);
                break;
            case MessageType.Info:
            default:
                ShowNotification(message, "Tip", infoColor);
                break;
        }
    }

    public enum MessageType
    {
        Info,       // 信息
        Command,    // 指挥官
        Warning,    // 警告
        Error       // 错误
    }

    #region Core Logic

    /// <summary>
    /// 显示通知 - 立即覆盖当前通知
    /// </summary>
    private void ShowNotification(string message, string type, Color color)
    {
        // 设置文本内容和颜色
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.color = color;
        }

        if (typeText != null)
        {
            typeText.text = type;
            typeText.color = color;
        }

        // 直接播放弹出动画（会覆盖当前显示的内容）
        PlayPopup();
    }

    public void PlayPopup()
    {
        RadioTextObj.SetActive(true);
        animator.ResetTrigger("Hide");
        animator.SetTrigger("Show");

        CancelInvoke(nameof(HidePopup));
        Invoke(nameof(HidePopup), stayTime);
    }

    private void HidePopup()
    {
        animator.ResetTrigger("Show");
        animator.SetTrigger("Hide");

        StartCoroutine(WaitForHideToEnd());
    }

    private IEnumerator WaitForHideToEnd()
    {
        // 等待进入 Hide 状态
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hide"))
            yield return null;

        // 等待动画播放完成
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        Finish();
    }

    private void Finish()
    {
        RadioTextObj.SetActive(false);
    }

    #endregion
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [Header("UI按钮引用")]
    public Button restartButton;
    public Button quitButton;

    // Start is called before the first frame update
    void Start()
    {
        // 绑定按钮事件
        SetupButtonEvents();

        // 确保鼠标可见且解锁
        SetupCursor();
    }

    /// <summary>
    /// 设置按钮事件绑定
    /// </summary>
    private void SetupButtonEvents()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        else
        {
            Debug.LogError("[GameOverUI] restartButton未设置！");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        else
        {
            Debug.LogError("[GameOverUI] quitButton未设置！");
        }
    }

    /// <summary>
    /// 设置鼠标状态
    /// </summary>
    private void SetupCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// 重新开始按钮点击事件
    /// </summary>
    public void OnRestartButtonClicked()
    {
        // 播放按钮音效
        //PlayButtonSound();

        // 调用GameManager的重新开始方法
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            // 备用方案：直接重新加载当前场景
            Debug.LogWarning("[GameOverUI] GameManager.Instance为空，使用备用重启方案");
        }
    }

    /// <summary>
    /// 退出游戏按钮点击事件
    /// </summary>
    public void OnQuitButtonClicked()
    {
        //PlayButtonSound();

        // 调用GameManager的退出方法
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            // 备用方案：直接退出
            Debug.LogWarning("[GameOverUI] GameManager.Instance为空，使用备用退出方案");
        }
    }

    /// <summary>
    /// 清理事件绑定，防止内存泄漏
    /// </summary>
    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
        }
    }
}

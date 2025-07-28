using BehaviorDesigner.Runtime;
using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Game Manager，我希望它的作用应该是：
/// 1. 为全局脚本提供Player对象的。因为玩家的位置信息、PlayerState信息很重要
/// 2. 为需要实现输入逻辑的其他脚本，提供挂载在Player对象下的PlayerInputSystem。
/// 3. 关于PingManager， 标记地点、标记激活标志、可能后面读取到标记的对象类型。我觉得会经常用到
/// </summary>
public class GameManager : MonoBehaviour
{
    // 游戏状态枚举
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Loading
    }

    public static GameManager Instance { get; private set; }
    [Header("AIAgent BT")]
    public string currentCommand;

    [Header("Game State")]
    public GameState currentGameState = GameState.Playing;

    [Header("Player")]
    public GameObject player;
    public PlayerInputSystem playerInputSystem;
    public WeaponManager playerWeaponManager;

    [Header("Inventory")]
    public InventoryManager inventoryManager;
    public PowerManager powerManager;

    [Header("AI Agent")]
    public GameObject aiTeammate;
    public AIAgentSettings aiAgentSettings;
    public WeaponManager aiPlayerWeaponManager;

    [Header("Manager")]
    public PingMarkerManager pingMarkerManager;

    [Header("Zombie")]
    public ZombieManager zombieManager;

    // 玩家死亡事件声明
    public static event System.Action OnPlayerDeath;
    // ai队友死亡事件声明
    public static event System.Action OnAIPlayerDeath;
    //public static event System.Action OnGameOver;

    private string lastCommand = "";
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 订阅玩家死亡事件
        OnPlayerDeath += HandlePlayerDeath;
        OnAIPlayerDeath += HandleAIPlayerDeath;
    }

    private void OnDestroy()
    {
        // 取消订阅防止内存泄漏
        OnPlayerDeath -= HandlePlayerDeath;
        OnAIPlayerDeath -= HandleAIPlayerDeath;
    }

    #region Getter
    public float GetDistBetweenPlayerAndAIAgent()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 AIAgentPosition = aiTeammate.transform.position;
        float dis = Vector3.Distance(playerPosition, AIAgentPosition);
        return dis;
    }

    public PlayerStats GetPlayerStats()
    {
        return player.GetComponent<PlayerStats>();
    }

    public AITeammateState GetAIAgentStats()
    {
        return aiTeammate.GetComponent<AITeammateState>();
    }

    public Transform GetPlayerTransform()
    {
        return player.transform;
    }

    public PlayerInputSystem GetPlayerInputSystem()
    {
        return player.GetComponent<PlayerInputSystem>();
    }

    public Transform GetAIAgentTransform()
    {
        return aiTeammate.transform;
    }

    public AIAgentSettings GetAIAgentSettings() 
    {
        return aiTeammate.GetComponent<AIAgentSettings>();
    }

    public PingMarkerManager GetPingMarkerManager()
    {
        return pingMarkerManager;
    }

    public InventoryManager GetInventoryManager()
    {
        return inventoryManager;
    }

    public WeaponManager GetPlayerWeaponManager()
    {
        return playerWeaponManager;
    }
    public WeaponManager GetAIPlayerWeaponManager()
    {
        return aiPlayerWeaponManager;
    }

    public bool CheckAIIsAlive()
    {
        return aiTeammate.GetComponent<AITeammateState>().IsAlive();
    }

    public PowerManager GetPowerManager()
    {
        return powerManager;
    }
    #endregion

    // 处理玩家死亡
    private void HandlePlayerDeath()
    {
        Debug.Log("[玩家死亡事件]：GameManager 处理玩家死亡逻辑 & 停止时间 & 更改游戏状态 & 触发GameOver事件");

        // 切换游戏状态
        currentGameState = GameState.GameOver;
        // 停止夜间生成器
        if (zombieManager != null)
        {           
            foreach (var spawner in zombieManager.nightSpawners)
            {
                if (spawner != null)
                {
                    spawner.StopSpawning();
                }
            }
        }

        // 停止时间系统
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.SetTimeSpeed(0f); // 停止时间流逝
        }
        ShowCursor();
    }

    private void HandleAIPlayerDeath()
    {
        Debug.Log("[GameManager] ai队友死亡！等待重新部署！");
    }


    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("游戏结束：显示光标");
    }

    // 重新开始游戏
    public void RestartGame()
    {
        Debug.Log("重新开始游戏...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            UnityEditor.EditorApplication.isPlaying = true;
        };
#else
    Time.timeScale = 1f;
    UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
#endif
    }

    // 退出游戏
    public void QuitGame()
    {
        Debug.Log("退出游戏...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // 静态方法：触发玩家死亡事件
    public static void TriggerPlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }

    // 静态方法：触发AI玩家死亡事件
    public static void TriggerAIPlayerDeath()
    {
        OnAIPlayerDeath?.Invoke();
    }

    // 获取当前游戏状态
    public bool IsGameOver()
    {
        return currentGameState == GameState.GameOver;
    }

    // 暂停/恢复游戏
    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            currentGameState = GameState.Paused;
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
        {
            currentGameState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    public void ReceiveAIBehaviorCommand(string command)
    {
        currentCommand = command;
        // 同步一次行为树变量
        var tree = aiTeammate.GetComponent<BehaviorTree>();
        if (tree != null)
        {
            tree.SetVariableValue("currentCommand", currentCommand);

            if(currentCommand == "unknown")
            {
                RadioPopController.Instance.ShowMessage(MessageKey.AI_parse_fail, RadioPopController.MessageType.Error);
            }

            Debug.Log("GPT指令更新为: " + currentCommand);
        }
    }

    /// <summary>
    /// 清空AI当前指令（用于重生等场景）
    /// </summary>
    public void ClearAIBehaviorCommand()
    {
        currentCommand = "";

        // 同时更新行为树变量
        var tree = aiTeammate.GetComponent<BehaviorTree>();
        if (tree != null)
        {
            tree.SetVariableValue("currentCommand", "");
            Debug.Log("AI指令已清空");
        }
    }
}

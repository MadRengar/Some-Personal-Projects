using BehaviorDesigner.Runtime;
using PlayerControl;
using System.Collections;
using System.Collections.Generic;
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
    public static GameManager Instance { get; private set; }
    public string currentCommand;
    [Header("Player")]
    public GameObject player;
    public PlayerInputSystem playerInputSystem;
    [Header("AI Agent")]
    public GameObject aiTeammate;
    public AIAgentSettings aiAgentSettings;
    [Header("Manager")]
    public PingMarkerManager pingMarkerManager;
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

    public PlayerStats GetPlayerStats()
    {
        return player.GetComponent<PlayerStats>();
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

    public void ReceiveAIBehaviorCommand(string command)
    {
        var tree = aiTeammate.GetComponent<BehaviorTree>();
        if (tree == null)
        {
            return;
        }
        currentCommand = command;
        switch (currentCommand)
        {
            case "move_to_mark":
                Debug.Log("move_to_mark");
                break;
            case "follow_player":
                Debug.Log("follow_player");
                break;
            case "collect_all":
                Debug.Log("collect_all");
                break;
            default:
                Debug.LogWarning("未识别的指令: " + command);
                break;
        }
    }
}

using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
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
}

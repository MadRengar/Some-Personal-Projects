using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject player;
    public PlayerInputSystem playerInputSystem;

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
}

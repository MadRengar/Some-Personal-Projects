using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ZombieAgentPathing[] zombies;

    public void OnPlayerReachedLab()
    {
        Debug.Log("Escape successful!");
        foreach (var z in zombies)
        {
            z.StopChasing();
        }
    }

    public void OnPlayerCaught()
    {
        Debug.Log("Game Over!");
        foreach (var z in zombies)
        {
            z.StopChasing();
        }
    }
}

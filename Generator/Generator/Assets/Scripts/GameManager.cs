using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ZombieAgentPathing[] zombies;

    public void OnPlayerReachedLab()
    {
        foreach (var z in zombies)
        {
            z.StopChasing();
        }
    }

    public void OnPlayerCaught()
    {
        Debug.Log("Injured by zombies£¡");
        //foreach (var z in zombies)
        //{
        //    z.StopChasing();
        //}
    }
}

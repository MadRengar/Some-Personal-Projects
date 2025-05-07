using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimEvent : MonoBehaviour
{
    private ZombieFSM fsm;

    void Start()
    {
        fsm = GetComponentInParent<ZombieFSM>();
    }

    public void OnAttackAnimationEnd()
    {
        fsm.OnAttackAnimationComplete();
    }
}


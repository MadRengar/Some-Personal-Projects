using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.AI;

public class InteractableObject : MonoBehaviour
{
    private NavMeshAgent playerAgent;
    //需要一个标志位来判断是否进行过交互 不然会一直调用Interact()
    private bool haveInteracted = false;
    public void OnClick(NavMeshAgent playerAgent) 
    {
        this.playerAgent = playerAgent;
        playerAgent.stoppingDistance = 0;
        /*分两步 来到附近 + 交互*/
        //S1 移动
        playerAgent.SetDestination(transform.position);
        //S2 交互
        //Interact();

        //每次点击互动 重置标志位
        haveInteracted = false;
    }

    private void Update()
    {
        /*if条件判断*/
        //1 在没有调用OnClick函数时playerAgent是为空的 容易出现空指针异常
        //2 在SetDestination调用后 会计算出一个导航的路径：正在路径计算的时候pathPending为true；计算完毕pathPending改为false
        if (playerAgent != null && haveInteracted == false && playerAgent.pathPending == false)
        {
            //remainingDistance剩余多少距离到目标位置(stoppingDistance)
            if (playerAgent.remainingDistance <= 2) 
            {
                Interact();
                haveInteracted = true;
            }
        }
    }

    /*互动对于子类来说是要可以重写的 1 对于NPC可以对话 2 对于Pickable的物品可以拾起*/
    protected virtual void Interact()
    {
        print("Interacting with InteractableObject.");
    }
}

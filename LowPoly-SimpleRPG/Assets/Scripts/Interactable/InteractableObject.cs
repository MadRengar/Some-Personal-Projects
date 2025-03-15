using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.AI;

public class InteractableObject : MonoBehaviour
{
    private NavMeshAgent playerAgent;
    //��Ҫһ����־λ���ж��Ƿ���й����� ��Ȼ��һֱ����Interact()
    private bool haveInteracted = false;
    public void OnClick(NavMeshAgent playerAgent) 
    {
        this.playerAgent = playerAgent;
        playerAgent.stoppingDistance = 0;
        /*������ �������� + ����*/
        //S1 �ƶ�
        playerAgent.SetDestination(transform.position);
        //S2 ����
        //Interact();

        //ÿ�ε������ ���ñ�־λ
        haveInteracted = false;
    }

    private void Update()
    {
        /*if�����ж�*/
        //1 ��û�е���OnClick����ʱplayerAgent��Ϊ�յ� ���׳��ֿ�ָ���쳣
        //2 ��SetDestination���ú� ������һ��������·��������·�������ʱ��pathPendingΪtrue���������pathPending��Ϊfalse
        if (playerAgent != null && haveInteracted == false && playerAgent.pathPending == false)
        {
            //remainingDistanceʣ����پ��뵽Ŀ��λ��(stoppingDistance)
            if (playerAgent.remainingDistance <= 2) 
            {
                Interact();
                haveInteracted = true;
            }
        }
    }

    /*��������������˵��Ҫ������д�� 1 ����NPC���ԶԻ� 2 ����Pickable����Ʒ����ʰ��*/
    protected virtual void Interact()
    {
        print("Interacting with InteractableObject.");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    //�������������ƶ������NavMeshAgent
    private NavMeshAgent playerAgent;
    // Start is called before the first frame update
    void Start()
    {
        //������GetComponent
        playerAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {   //��갴�� ���� û�е����UI���IsPointerOverGameObject() == false
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            //ScreenPointToRay��ƵĻ��һ��ת��Ϊ���� mousePosition��Ϊ������� ����ֵΪray
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //
            RaycastHit hit;
            bool isCollide = Physics.Raycast(ray, out hit);
            
            if(isCollide) 
            {
                if(hit.collider.tag == "Ground")
                {
                    playerAgent.stoppingDistance = 0;
                    playerAgent.SetDestination(hit.point);
                }else if(hit.collider.tag == "Interactable")
                {
                    hit.collider.GetComponent<InteractableObject>().OnClick(playerAgent);
                }
                
            }

        }
    }
}

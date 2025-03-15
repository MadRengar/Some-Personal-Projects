using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    //申明引用人物移动的组件NavMeshAgent
    private NavMeshAgent playerAgent;
    // Start is called before the first frame update
    void Start()
    {
        //获得组件GetComponent
        playerAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {   //鼠标按下 并且 没有点击到UI组件IsPointerOverGameObject() == false
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            //ScreenPointToRay讲频幕上一点转化为射线 mousePosition即为鼠标坐标 返回值为ray
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

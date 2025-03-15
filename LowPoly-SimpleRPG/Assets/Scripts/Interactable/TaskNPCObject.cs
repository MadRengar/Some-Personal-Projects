using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskNPCObject : InteractableObject
{

    public string npcName;

    public GameTaskSO gameTaskSO;

    public string[] contentInTaskExecuting;//��������еĶԻ�
    public string[] contentInTaskCompleted;//�������ʱ�ĶԻ�
    public string[] contentInTaskEnd;//�������ʱ�ĶԻ�

    private void Start()
    {
        gameTaskSO.state = GameTaskState.Waiting;//��ʼ������״̬
    }

    protected override void Interact()
    {
        switch (gameTaskSO.state)
        {
            case GameTaskState.Waiting:
                DialogueUI.Instance.Show(npcName, gameTaskSO.diague, OnDialogueEnd);//�Ի�������ʱ��ص�OnDialogueEnd����
                break;
            case GameTaskState.Executing:
                DialogueUI.Instance.Show(npcName, contentInTaskExecuting);//������ִ�е��в���Ҫ�ص�
                break;

            case GameTaskState.Completed:
                DialogueUI.Instance.Show(npcName, contentInTaskCompleted, OnDialogueEnd);
                break;
            case GameTaskState.End:
                DialogueUI.Instance.Show(npcName, contentInTaskEnd);//���������ʱ����Ҫ�ص�
                break;
        }
        
    }

    public void OnDialogueEnd()
    {
        switch (gameTaskSO.state)
        {
            case GameTaskState.Waiting:
                gameTaskSO.Start();
                InventoryManager.Instance.AddItem(gameTaskSO.startReward);//������ҳ�ʼ����
                MessageUI.Instance.Show("�������һ���µ�����");
                //print(gameTaskSO.startReward);
                break;
            case GameTaskState.Executing: 
                
                break;

            case GameTaskState.Completed:
                gameTaskSO.End();
                InventoryManager.Instance.AddItem(gameTaskSO.endReward);//�������������ɺ�Ľ���
                MessageUI.Instance.Show("��������ɣ�");
                break;
            case GameTaskState.End:
                break;
        }
    }
}

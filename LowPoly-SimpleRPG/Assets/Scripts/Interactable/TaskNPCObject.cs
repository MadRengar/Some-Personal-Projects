using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskNPCObject : InteractableObject
{

    public string npcName;

    public GameTaskSO gameTaskSO;

    public string[] contentInTaskExecuting;//任务进行中的对话
    public string[] contentInTaskCompleted;//任务完成时的对话
    public string[] contentInTaskEnd;//任务完成时的对话

    private void Start()
    {
        gameTaskSO.state = GameTaskState.Waiting;//初始化任务状态
    }

    protected override void Interact()
    {
        switch (gameTaskSO.state)
        {
            case GameTaskState.Waiting:
                DialogueUI.Instance.Show(npcName, gameTaskSO.diague, OnDialogueEnd);//对话结束的时候回调OnDialogueEnd函数
                break;
            case GameTaskState.Executing:
                DialogueUI.Instance.Show(npcName, contentInTaskExecuting);//任务在执行当中不需要回调
                break;

            case GameTaskState.Completed:
                DialogueUI.Instance.Show(npcName, contentInTaskCompleted, OnDialogueEnd);
                break;
            case GameTaskState.End:
                DialogueUI.Instance.Show(npcName, contentInTaskEnd);//任务结束的时候不需要回调
                break;
        }
        
    }

    public void OnDialogueEnd()
    {
        switch (gameTaskSO.state)
        {
            case GameTaskState.Waiting:
                gameTaskSO.Start();
                InventoryManager.Instance.AddItem(gameTaskSO.startReward);//给予玩家初始奖励
                MessageUI.Instance.Show("你接受了一个新的任务！");
                //print(gameTaskSO.startReward);
                break;
            case GameTaskState.Executing: 
                
                break;

            case GameTaskState.Completed:
                gameTaskSO.End();
                InventoryManager.Instance.AddItem(gameTaskSO.endReward);//给予玩家任务完成后的奖励
                MessageUI.Instance.Show("任务已完成！");
                break;
            case GameTaskState.End:
                break;
        }
    }
}

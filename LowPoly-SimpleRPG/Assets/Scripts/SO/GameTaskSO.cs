using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//任务的状态类型
public enum GameTaskState
{
    Waiting,//等待接受
    Executing,//正在执行
    Completed,//完成
    End,//结束
}

[CreateAssetMenu()]
public class GameTaskSO : ScriptableObject
{
    public GameTaskState state;//任务状态
    public string[] diague;//NPC对话内容

    public ItemSO startReward;
    public ItemSO endReward;

    public int needEnemyCount = 3;
    public int currentEnemyCount = 0;
    public void Start()
    {
        currentEnemyCount = 0;
        state = GameTaskState.Executing;//更改任务状态为“Executing”
        EventCenter.OnEnemyDied += OnEnemyDied;//注册事件中心
    }

    private void OnEnemyDied(Enemy enemy)
    {
        currentEnemyCount++;
        if (state == GameTaskState.Completed) { return; }
        if(currentEnemyCount >= needEnemyCount)
        {
            state = GameTaskState.Completed;
            MessageUI.Instance.Show("任务完成，请前去提交！");
        }
    }

    public void End()
    {
        state = GameTaskState.End;
        EventCenter.OnEnemyDied -= OnEnemyDied;
    }
}

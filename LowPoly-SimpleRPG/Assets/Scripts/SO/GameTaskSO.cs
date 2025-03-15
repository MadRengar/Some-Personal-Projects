using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�����״̬����
public enum GameTaskState
{
    Waiting,//�ȴ�����
    Executing,//����ִ��
    Completed,//���
    End,//����
}

[CreateAssetMenu()]
public class GameTaskSO : ScriptableObject
{
    public GameTaskState state;//����״̬
    public string[] diague;//NPC�Ի�����

    public ItemSO startReward;
    public ItemSO endReward;

    public int needEnemyCount = 3;
    public int currentEnemyCount = 0;
    public void Start()
    {
        currentEnemyCount = 0;
        state = GameTaskState.Executing;//��������״̬Ϊ��Executing��
        EventCenter.OnEnemyDied += OnEnemyDied;//ע���¼�����
    }

    private void OnEnemyDied(Enemy enemy)
    {
        currentEnemyCount++;
        if (state == GameTaskState.Completed) { return; }
        if(currentEnemyCount >= needEnemyCount)
        {
            state = GameTaskState.Completed;
            MessageUI.Instance.Show("������ɣ���ǰȥ�ύ��");
        }
    }

    public void End()
    {
        state = GameTaskState.End;
        EventCenter.OnEnemyDied -= OnEnemyDied;
    }
}

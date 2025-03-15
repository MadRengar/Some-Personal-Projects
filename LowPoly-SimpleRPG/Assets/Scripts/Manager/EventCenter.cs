using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCenter : MonoBehaviour
{
    public static event Action<Enemy> OnEnemyDied;//event�¼��������ί�� �����ٸ���֮��ĵط�ע�����ȡ������

    //���Դ���һ������ ����������ʹ��
    public static void EnemyDied(Enemy enemy)
    {
        OnEnemyDied?.Invoke(enemy);
    }
}

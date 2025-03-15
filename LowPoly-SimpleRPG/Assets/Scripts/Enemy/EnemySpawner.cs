using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*�Զ�����������*/
/*�����߼�
 1 ����һ���յ���Ϸ�����ڳ����У�
 2 �ڸö���λ��ʵ�������˵�prefab��
 3 ���ü�ʱ��
 */
public class EnemySpawner : MonoBehaviour
{
    private float spawnTimer;
    public float spawnTime;//���ɼ��ʱ��
    public GameObject enemyPrefab;//���������õĵ���Ԥ����
    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawnTime) 
        {
            spawnTimer = 0;
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        GameObject.Instantiate(enemyPrefab, transform.position, Quaternion.identity);//ʵ����enemyPrefab
    }
}

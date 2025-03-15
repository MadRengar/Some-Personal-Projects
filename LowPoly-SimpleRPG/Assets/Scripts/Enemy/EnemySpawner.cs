using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*自动敌人生成器*/
/*核心逻辑
 1 创建一个空的游戏对象在场景中；
 2 在该对象位置实例化敌人的prefab；
 3 设置计时器
 */
public class EnemySpawner : MonoBehaviour
{
    private float spawnTimer;
    public float spawnTime;//生成间隔时间
    public GameObject enemyPrefab;//事先制作好的敌人预制体
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
        GameObject.Instantiate(enemyPrefab, transform.position, Quaternion.identity);//实例化enemyPrefab
    }
}

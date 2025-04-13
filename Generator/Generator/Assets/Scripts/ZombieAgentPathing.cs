using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAgentPathing : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public Transform player;
    public MapGenerator mapGen;
    public GameManager gameManager;

    public float pathRefreshRate = 1.5f;

    private List<Vector3> worldPath = new List<Vector3>();
    private int currentIndex = 0;
    private float refreshTimer = 0f;


    void Update()
    {
        refreshTimer += Time.deltaTime;

        if (refreshTimer >= pathRefreshRate)
        {
            RecalculatePath();
            refreshTimer = 0f;
        }

        if (currentIndex < worldPath.Count)
        {
            Vector3 target = new Vector3(worldPath[currentIndex].x, 0.05f, worldPath[currentIndex].z);
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f)
                currentIndex++;
        }

        if (player != null && Vector3.Distance(transform.position, player.position) < 0.6f)
        {
            if (gameManager != null) gameManager.OnPlayerCaught();
        }
    }

    void RecalculatePath()
    {
        if (mapGen == null || player == null) return;

        Vector3 pos = transform.position;
        Vector3 targetPos = player.position;

        // Convert world to tile coords
        int startX = Mathf.RoundToInt(pos.x + mapGen.width / 2);
        int startY = Mathf.RoundToInt(pos.z + mapGen.height / 2);
        int endX = Mathf.RoundToInt(targetPos.x + mapGen.width / 2);
        int endY = Mathf.RoundToInt(targetPos.z + mapGen.height / 2);

        List<MapGenerator.Coord> path = mapGen.FindPathAStar(new MapGenerator.Coord(startX, startY), new MapGenerator.Coord(endX, endY));
        worldPath.Clear();
        currentIndex = 0;

        foreach (var tile in path)
        {
            // from Coord -> Vector3
            Vector3 world = new Vector3(
                tile.tileX - mapGen.width / 2 - 0.5f,
                0.05f,
                tile.tileY - mapGen.height / 2 - 0.5f
            );

            worldPath.Add(world);
        }
    }

    public void StopChasing()
    {
        enabled = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (worldPath != null)
        {
            foreach (var wp in worldPath)
            {
                Gizmos.DrawSphere(wp, 0.1f);
            }
        }

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }

}

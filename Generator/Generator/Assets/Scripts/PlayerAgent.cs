using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgent : MonoBehaviour
{
    public float moveSpeed = 10f;
    public List<Vector3> worldPath = new List<Vector3>();
    private int currentIndex = 0;
    public GameManager gameManager;

    void Update()
    {
        if (worldPath.Count == 0 || currentIndex >= worldPath.Count) return;

        transform.position = Vector3.MoveTowards(transform.position, worldPath[currentIndex], moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, worldPath[currentIndex]) < 0.1f)
        {
            currentIndex++;
            if (currentIndex >= worldPath.Count)
            {
                Debug.Log("Player reached the lab!");
                if (gameManager != null) gameManager.OnPlayerReachedLab();
            }
        }
    }

    public void SetPath(List<Vector3> path)
    {
        worldPath = path;
        currentIndex = 0;
    }
}

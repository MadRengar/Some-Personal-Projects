using UnityEngine;

public class SimpleAutoDestroy : MonoBehaviour
{
    [Header("Ïú»ÙÉèÖÃ")]
    public float destroyAfter = 3f;

    void Start()
    {
        Destroy(gameObject, destroyAfter);
    }
}
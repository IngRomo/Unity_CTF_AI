using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Collider spawnAreaCollider;

    public Vector3 GetRandomPointOnSurface()
    {
        if (spawnAreaCollider == null)
        {
            Debug.LogError("SpawnSurface: spawnAreaCollider not assigned.");
            return transform.position;
        }

        Bounds bounds = spawnAreaCollider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        float y = bounds.max.y; // Top of the platform

        return new Vector3(x, y, z);
    }
}

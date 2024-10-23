using UnityEngine;
using System.Collections.Generic;

public class TreeSpawner : MonoBehaviour
{
    public GameObject treePrefab;    // Tree prefab to instantiate
    public int treeCount = 50;       // Number of trees to spawn
    public float radius = 50f;       // Radius of the circle
    public Vector3 offset;
    public float minDistanceBetweenTrees = 2f;  // Minimum distance between trees to avoid overlap
    public LayerMask terrainLayer;   // The layer of the terrain

    private List<Vector3> treePositions = new List<Vector3>();  // Stores positions of placed trees

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        int treesPlaced = 0;

        while (treesPlaced < treeCount)
        {
            // Generate a random position inside the circle
            Vector2 randomPoint = Random.insideUnitCircle * radius;
            Vector3 treePosition = new Vector3(randomPoint.x, 100f, randomPoint.y); // Start raycast from a high Y position

            // Perform raycast to find the ground on the terrain layer
            if (Physics.Raycast(treePosition, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                // Adjust tree position to be on the terrain surface
                treePosition = hit.point;

                // Check if this position is valid (no overlaps)
                if (IsPositionValid(treePosition))
                {
                    // Spawn the tree at this position
                    GameObject Tree = Instantiate(treePrefab, treePosition + offset, Quaternion.identity);
                    Tree.transform.parent = this.transform;
                    treePositions.Add(treePosition);  // Store the position
                    treesPlaced++;
                }
            }
        }
    }

    bool IsPositionValid(Vector3 position)
    {
        // Check if the new position is far enough from all existing trees
        foreach (Vector3 treePosition in treePositions)
        {
            if (Vector3.Distance(position, treePosition) < minDistanceBetweenTrees)
            {
                return false;  // Too close to another tree, position is invalid
            }
        }
        return true;  // No overlaps, position is valid
    }

    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}